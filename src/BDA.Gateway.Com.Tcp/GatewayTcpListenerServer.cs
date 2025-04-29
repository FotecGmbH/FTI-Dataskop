// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Com.Tcp
{
    /// <summary>
    ///     <para>Tcp server listening for new clients which want to connect</para>
    ///     Klasse GatewayTcpListenerServer. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GatewayTcpListenerServer
    {
        /// <summary>
        ///     action to invoke when new connection is asteblished
        /// </summary>
        private readonly Action<TcpClient> _newConnection;

        /// <summary>
        ///     port
        /// </summary>
        private readonly int _port;

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="port">port</param>
        /// <param name="newConnection">action</param>
        public GatewayTcpListenerServer(int port, Action<TcpClient> newConnection)
        {
            _port = port;
            _newConnection = newConnection;
        }

        #region Properties

        /// <summary>
        ///     when to stop the worker
        /// </summary>
        public bool Exit { get; set; } = false;

        #endregion


        /// <summary>
        ///     starts listener worker
        /// </summary>
        /// <returns></returns>
        public async Task ListenerWorker()
        {
            Logging.Log.LogTrace($"[{nameof(GatewayTcpListenerServer)}]({nameof(ListenerWorker)}): Started Listening for TCP Clients");
            var server = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
            server.Start();

            while (!Exit)
            {
                var newClient = await server.AcceptTcpClientAsync();

                _newConnection.Invoke(newClient);

                Logging.Log.LogTrace($"[{nameof(GatewayTcpListenerServer)}]({nameof(ListenerWorker)}):Accepted client");
                await Task.Delay(100).ConfigureAwait(true);
            }

            server.Stop();
            server.Server.Dispose();
        }
    }
}