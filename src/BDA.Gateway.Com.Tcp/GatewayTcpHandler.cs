// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Com.Tcp
{
    /// <summary>
    ///     <para>Handles the tcp communication</para>
    ///     Klasse GatewayTcpHandler. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GatewayTcpHandler
    {
        /// <summary>
        ///     list of the clients inclusive names
        /// </summary>
        private readonly List<NamedTcpClient> _clients = new();

        /// <summary>
        ///     semaphore
        /// </summary>
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        ///     worker for listening for new tcp connections
        /// </summary>
        private readonly GatewayTcpListenerServer _tcpListenerServer;

        /// <summary>
        ///     For [GatewayComTcp] so it knows to where to send data
        /// </summary>
        public readonly Action<string, byte[]> SendFunction;

        /// <summary>
        ///     Counter for logging, that the logging window is not overfilled with "Cannot find needed device" messages
        /// </summary>
        private int _logCounter;

        /// <summary>
        ///     when to exit
        /// </summary>
        public bool Exit;

        /// <summary>
        ///     If already started
        /// </summary>
        public bool Started;

        /// <summary>
        ///     Constructor
        /// </summary>
        public GatewayTcpHandler()
        {
            _tcpListenerServer = new GatewayTcpListenerServer(802, async cl =>
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(true);
                _clients.Add(new NamedTcpClient(string.Empty, cl));
                _semaphoreSlim.Release();
            });

            SendFunction = async (secret, buffer) =>
            {
                var cl = _clients.LastOrDefault(cl => cl.Secret == secret);
                if (cl == null)
                {
                    return;
                }

                await WriteAsync(cl.Client, buffer).ConfigureAwait(true);
            };
        }

        #region Properties

        /// <summary>
        ///     Tcp Communications inclusive configuration of them
        /// </summary>
        private List<GatewayComTcp> TcpIotDevices { get; } = new();

        #endregion

        /// <summary>
        ///     writes to tcp client
        /// </summary>
        /// <param name="client"> tcp client</param>
        /// <param name="buffer">buffer to send</param>
        /// <returns>task</returns>
        public async Task WriteAsync(TcpClient client, byte[] buffer)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (client == null || !client.Connected)
            {
                return;
            }

            var stream = client.GetStream();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (stream != null && stream.CanWrite)
            {
                try
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(true);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(GatewayTcpHandler)}]({nameof(WriteAsync)}): couldnt write to client");
                }
            }
        }

        /// <summary>
        /// TcpIotDevicesClear
        /// </summary>
        /// <returns></returns>
        public async Task TcpIotDevicesClear()
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(true);
            TcpIotDevices.Clear();
            _clients.ForEach(c => c.Initialized = false); // normaly clear is caused by change in config, so everyone should be "uninitialized" so they receive new config
            _semaphoreSlim.Release();
        }

        /// <summary>
        /// TcpIotDevicesAdd
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task TcpIotDevicesAdd(GatewayComTcp device)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(true);
            TcpIotDevices.Add(device);
            _semaphoreSlim.Release();
        }

        /// <summary>
        ///     Starts udp broadcasting, tcp listening and tcp reading workers
        /// </summary>
        /// <returns>task</returns>
        public async Task StartAsync()
        {
            Started = true;

            List<Task> tasks = new List<Task>();
            tasks.Add(_tcpListenerServer.ListenerWorker());
            tasks.Add(ReadWorker());
            await Task.WhenAll(tasks).ConfigureAwait(true);
        }

        /// <summary>
        ///     Stops all workers
        /// </summary>
        /// <returns>task</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _tcpListenerServer.Exit = true;
            Exit = true;
            _clients.ForEach(cl => cl.Client.Dispose());
        }

        /// <summary>
        ///     Reads on all open tcp connections, if someone wants to send something
        /// </summary>
        /// <returns>task</returns>
        public async Task ReadWorker()
        {
            Logging.Log.LogTrace($"[{nameof(GatewayTcpHandler)}]({nameof(ReadWorker)}): Started Reading from Tcp Connections");

            while (!Exit)
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(true);

                _clients.ForEach(async client =>
                {
                    if (client.Client.Connected)
                    {
                        var stream = client.Client.GetStream();
                        byte[] wholeData;
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        if (stream != null && stream.CanRead && stream.DataAvailable)
                        {
                            var buffer = new byte[200];
                            using (var ms = new MemoryStream())
                            {
                                int len;
                                while (stream.CanRead && stream.DataAvailable && (len = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, Math.Min(buffer.Length, len));
                                }


                                wholeData = ms.ToArray();
                            }

                            Logging.Log.LogTrace($"[{nameof(GatewayTcpHandler)}]({nameof(ReadWorker)}): Received Message from: " + client.Secret);

                            await ProcessMessageAsync(wholeData, client).ConfigureAwait(true);
                        }
                    }
                });

                //_clients.RemoveAll(c => !c.Client.Connected);

                _semaphoreSlim.Release();

                await Task.Delay(200).ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Sends current config to the iotdevice with the given secret
        /// </summary>
        /// <param name="secret">secret of the device</param>
        /// <param name="resend"></param>
        /// <returns></returns>
        public async Task<bool> SendConfigAsync(string secret, bool resend)
        {
            var curr = TcpIotDevices.LastOrDefault(dev => dev.IotConfig.Secret == secret);

            if (curr == null)
            {
                if (_logCounter++ % 10 == 0) // so logging window don't get overfilled
                {
                    Logging.Log.LogWarning($"[{nameof(GatewayTcpHandler)}]({nameof(ProcessMessageAsync)}): Cannot find needed device config");
                }

                return false;
                // device is not in this gateway till now
                // TODO 
                //throw new NotImplementedException("user created iotdevice with cli app, but there is yet no config for this device on the server");
            }

            await curr.UpdateIotDeviceConfig(curr.IotConfig, resend).ConfigureAwait(true);
            return true;
        }


        /// <summary>
        ///     Processes messages coming from the device
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="client">client</param>
        /// <returns>task</returns>
        public async Task ProcessMessageAsync(byte[] buffer, NamedTcpClient client)
        {
            if (client == null)
            {
                throw new ArgumentException(nameof(client));
            }

            // ReSharper disable once RedundantAssignment
            GatewayComTcp curr = null!;

            var code = buffer[0];


            switch (code)
            {
                case 9:
                    var lengthh = buffer[1];
                    var secrett = Encoding.UTF8.GetString(buffer.Skip(2).Take(lengthh).ToArray());
                    client.Secret = secrett;
                    break;
                case 10:
                case 11:
                    // needs config
                    var length = buffer[1];
                    var secret = Encoding.UTF8.GetString(buffer.Skip(2).Take(length).ToArray());
                    client.Secret = secret;

                    client.Initialized = await SendConfigAsync(secret, true).ConfigureAwait(true);
                    break;
                case 15:
                    curr = TcpIotDevices.LastOrDefault(dev => dev.IotConfig.Secret == client.Secret)!;

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (curr == null)
                    {
                        if (_logCounter++ % 10 == 0) // so logging window don't get overfilled
                        {
                            Logging.Log.LogWarning($"[{nameof(GatewayTcpHandler)}]({nameof(ProcessMessageAsync)}): Cannot find needed device config");
                        }
                    }
                    else
                    {
                        var bytes = buffer.Skip(1).ToArray();
                        try
                        {
                            await curr.NewValues(bytes, curr.IotConfig.DbId).ConfigureAwait(false);
                            client.Initialized = await SendConfigAsync(client.Secret, false).ConfigureAwait(true);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogError("Problems on new Values");
                        }

                        await curr.UpdateIotDeviceState(EnumDeviceOnlineState.Online, string.Empty).ConfigureAwait(true);
                    }

                    break;
                default:
                    Logging.Log.LogWarning($"[{nameof(GatewayTcpHandler)}]({nameof(ProcessMessageAsync)}) Not known Message code: {code}");
                    break;
            }
        }
    }
}