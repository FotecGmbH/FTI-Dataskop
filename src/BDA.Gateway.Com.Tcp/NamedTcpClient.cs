// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Net.Sockets;

namespace BDA.Gateway.Com.Tcp
{
    /// <summary>
    ///     class which holds a string secret and a tcp client
    /// </summary>
    public class NamedTcpClient
    {
        /// <summary>
        ///     tcp client
        /// </summary>
        private TcpClient _tcpClient = null!;

        /// <summary>
        ///     named tcp client
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="tcpClient"></param>
        public NamedTcpClient(string secret, TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            Secret = secret;
        }

        #region Properties

        /// <summary>
        ///     Secret
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     If already got config
        /// </summary>
        public bool Initialized { get; set; } = false;

        /// <summary>
        ///     Tcp client
        /// </summary>
        public TcpClient Client
        {
            get { return _tcpClient; }
            set
            {
                if (_tcpClient == null)
                {
                    throw new ArgumentNullException(nameof(_tcpClient));
                }

                _tcpClient = value;
            }
        }

        #endregion
    }
}