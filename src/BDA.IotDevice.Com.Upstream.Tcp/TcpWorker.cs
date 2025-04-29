// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Biss.Log.Producer;

namespace BDA.IotDevice.Com.Upstream.Tcp
{
    /// <summary>
    ///     <para>Worker for tcp handling</para>
    ///     Klasse TcpWorker. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class TcpWorker
    {
        private TcpClient _client = null!;

        /// <summary>
        ///     Connects to an adress, sends data to it, and tries to receive data
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="sendBuffer"></param>
        /// <returns></returns>
        public async Task<byte[]> TcpWorkerSendGet(IPAddress ipAddress, int port, byte[] sendBuffer)
        {
            // ReSharper disable once RedundantAssignment
            NetworkStream stream = null!;

            stream = _client.GetStream();

            await stream.WriteAsync(sendBuffer).ConfigureAwait(true);

            await Task.Delay(5000).ConfigureAwait(true); // Waiting window, in which the gateway could send a message

            var readBuffer = new byte[200];

            var readed = false;

            if (stream.CanRead && stream.DataAvailable)
            {
                readed = true;
                // ReSharper disable once UnusedVariable
                var res = await stream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(true);
                Logging.Log.LogInfo($"[{nameof(TcpWorker)}]({nameof(TcpWorkerSendGet)}): received data from server");
            }

            if (readed)
            {
                return readBuffer;
            }

            return Array.Empty<byte>();
        }

        /// <summary>
        /// init
        /// </summary>
        /// <param name="gatewayIpAddress"></param>
        /// <param name="tcpPort"></param>
        public void Init(IPAddress gatewayIpAddress, int tcpPort)
        {
            _client = new TcpClient();
            while (!_client.Connected)
            {
                try
                {
                    _client.Connect(gatewayIpAddress, tcpPort);
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}