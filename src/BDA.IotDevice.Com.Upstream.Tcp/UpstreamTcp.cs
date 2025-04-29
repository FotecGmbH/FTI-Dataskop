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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Upstream.Base;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Logging;

namespace BDA.IotDevice.Com.Upstream.Tcp
{
    /// <summary>
    ///     <para>Upstream tcp</para>
    ///     Klasse UpstreamTcp. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UpstreamTcp : UpstreamBase
    {
        /// <summary>
        ///     Connection changed action
        /// </summary>
        private readonly Action<EnumIoTDeviceConnectionStates> _connectionChanged;

        /// <summary>
        ///     hostname des gateways (=>_ipAvail = false)
        /// </summary>
        private readonly string _gatewayHostName;

        /// <summary>
        ///     Ipaddresse des gateways (=>ipAvail = true)
        /// </summary>
        private readonly IPAddress _gatewayIpAddress;

        /// <summary>
        ///     Ist die ip addresse verfügbar?
        /// </summary>
        private readonly bool _ipAvail;

        /// <summary>
        ///     Neue configuration erhalten action
        /// </summary>
        private readonly Action<ExGwServiceIotDeviceConfig> _receivedConfig;

        /// <summary>
        ///     Secret des Sensors, als Unique identifier für kommunikation mit gateway
        /// </summary>
        private readonly string _secret;

        /// <summary>
        ///     semaphore
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        ///     ist das gerät initialisiert? (hat configuration?)
        /// </summary>
        private bool _init;

        /// <summary>
        ///     Tcpclient des sensors
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        ///     Sollen die while schleifen/tasks bei der nächsten gelegenheit aufhören?
        /// </summary>
        public bool Exit;

        /// <summary>
        ///     Kommunikation zu gateway über hostname
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="gatewayHostname"></param>
        /// <param name="receivedConfig"></param>
        /// <param name="connectionChanged"></param>
#pragma warning disable CS8618, CS9264
        public UpstreamTcp(string secret, string gatewayHostname, Action<ExGwServiceIotDeviceConfig> receivedConfig, Action<EnumIoTDeviceConnectionStates> connectionChanged) : base(EnumIotDeviceUpstreamTypes.Tcp)
#pragma warning restore CS8618, CS9264
        {
            _receivedConfig = receivedConfig;
            _secret = secret;
            _gatewayHostName = gatewayHostname;
            _ipAvail = false;
            _connectionChanged = connectionChanged;
        }

        /// <summary>
        ///     Kommunikation zu gateway über ipaddressse
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="gatewayIpAddress"></param>
        /// <param name="receivedConfig"></param>
        /// <param name="connectionChanged"></param>
#pragma warning disable CS8618, CS9264
        public UpstreamTcp(string secret, IPAddress gatewayIpAddress, Action<ExGwServiceIotDeviceConfig> receivedConfig, Action<EnumIoTDeviceConnectionStates> connectionChanged) : base(EnumIotDeviceUpstreamTypes.Tcp)
#pragma warning restore CS8618, CS9264
        {
            _receivedConfig = receivedConfig;
            _secret = secret;
            _gatewayIpAddress = gatewayIpAddress;
            _ipAvail = true;
            _connectionChanged = connectionChanged;
        }

        /// <summary>
        ///     Connects and
        ///     starts reading messages from gateway
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task StartAsync(int port)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ConnectAsync(port));
            tasks.Add(StartDownlinkHandlerAsync());
            await Task.WhenAll(tasks).ConfigureAwait(true);
        }

        /// <summary>
        ///     Stops trying to connect and sending messages
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            Exit = true;
            if (_tcpClient.Connected)
            {
                _tcpClient.GetStream().Dispose();
            }

            _tcpClient.Dispose();
        }

        /// <summary>
        ///     Transfers values to gateway
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>if succeded</returns>
        public override async Task<bool> TransferData(List<ExValue> data)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (data == null || data.Count == 0)
            {
                return false;
            }

            if (_tcpClient.Connected)
            {
                await _semaphore.WaitAsync().ConfigureAwait(true);
                try
                {
                    var str = _tcpClient.GetStream();
                    if (str.CanWrite)
                    {
                        // Code for transfer data          
                        var buffer = new byte[] {15}.Concat(Encoding.UTF8.GetBytes(data.ToJson())).ToArray();
                        await str.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(true);
                        Logging.Log.LogInfo($"[{nameof(UpstreamTcp)}]({nameof(TransferData)}): sent: " + data.Count + " values");
                        _connectionChanged(EnumIoTDeviceConnectionStates.Connected);
                        return true;
                    }
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(UpstreamTcp)}]({nameof(TransferData)}): something went wrong on sending data");
                    _connectionChanged(EnumIoTDeviceConnectionStates.Disconnected);
                    return false;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            Logging.Log.LogWarning($"[{nameof(UpstreamTcp)}]({nameof(TransferData)}): not connected to gateway");
            return false;
        }

        /// <summary>
        ///     Connects to gateway
        /// </summary>
        /// <param name="port">port</param>
        /// <returns>task</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task ConnectAsync(int port)
        {
            _tcpClient = new TcpClient();
            _connectionChanged(EnumIoTDeviceConnectionStates.Connecting);

            while (!_init && !Exit) // try to connect till you are initialized
            {
                try
                {
                    if (_ipAvail) // case for ip address
                    {
                        if (!_tcpClient.Connected)
                        {
                            await _tcpClient.ConnectAsync(_gatewayIpAddress, port).ConfigureAwait(true);
                        }
                    }
                    else // case for hostname
                    {
                        if (!_tcpClient.Connected)
                        {
                            await _tcpClient.ConnectAsync(_gatewayHostName, port).ConfigureAwait(true);
                        }
                    }

                    await _semaphore.WaitAsync().ConfigureAwait(true);
                    var strm = _tcpClient.GetStream();
                    var str = Encoding.UTF8.GetBytes(_secret).ToList();
                    str.Insert(0, (byte) str.Count); // how long is the string
                    str.Insert(0, 11); // "need config" code
                    var sendData = str.ToArray();
                    await strm.WriteAsync(sendData, 0, sendData.Length).ConfigureAwait(true);
                    _connectionChanged(EnumIoTDeviceConnectionStates.Connected);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(UpstreamTcp)}]({nameof(ConnectAsync)}): Couldnt connect to gateway");
                    _connectionChanged(EnumIoTDeviceConnectionStates.Disconnected);
                }
                finally
                {
                    _semaphore.Release();
                }

                await Task.Delay(5000).ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Reading messages from gateway
        /// </summary>
        /// <returns></returns>
        private async Task StartDownlinkHandlerAsync()
        {
            while (!Exit)
            {
                await Task.Delay(150).ConfigureAwait(true);

                if (!_tcpClient.Connected)
                {
                    continue;
                }

                await _semaphore.WaitAsync().ConfigureAwait(true);
                try
                {
                    var stream = _tcpClient.GetStream();
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

                        await HandleDownlinkFromGateway(wholeData).ConfigureAwait(true);
                        Logging.Log.LogTrace($"[{nameof(UpstreamTcp)}]({nameof(StartDownlinkHandlerAsync)}): Received Message");
                    }
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(UpstreamTcp)}]({nameof(StartDownlinkHandlerAsync)}): had problem handling downlink/handling stream");
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            if (_tcpClient.Connected)
            {
                _tcpClient.Dispose();
            }
        }

        /// <summary>
        ///     Handles downlink message
        /// </summary>
        /// <param name="buffer">buffer which comes from gateway</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task HandleDownlinkFromGateway(byte[] buffer)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var code = buffer[0];

            switch (code)
            {
                case 11:
                    var jsonStr = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1);
                    jsonStr = SplitJsonString(jsonStr).LastOrDefault();
                    var config = BissDeserialize.FromJson<ExGwServiceIotDeviceConfig>(jsonStr!);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (config != null)
                    {
                        _receivedConfig.Invoke(config);
                    }
                    else
                    {
                        Logging.Log.LogWarning($"[{nameof(UpstreamTcp)}]({nameof(HandleDownlinkFromGateway)}): received not correct formatted config");
                    }

                    _init = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Es kann passieren dass wir mehrere json strings auf einmal bekommen z.B.:
        ///     "{...{....}...} {.....{..{..}..}.}"
        ///     Diese methode soll dies in mehrere json strings aufteilen zu:
        ///     List: "{...{....}...}", "{.....{..{..}..}.}"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<string> SplitJsonString(string input)
        {
            var index = 0;
            var counter = 0;
            List<string> strings = new List<string>();

            while (index < input.Length)
            {
                var tmp = string.Empty;
                do
                {
                    if (input[index] == '{')
                    {
                        counter++;
                    }

                    if (input[index] == '}')
                    {
                        counter--;
                    }

                    tmp = tmp + input[index];
                    index++;
                } while (counter > 0);

                strings.Add(tmp);
            }

            strings.RemoveAll(str => str[0] != '{');

            return strings;
        }
    }
}