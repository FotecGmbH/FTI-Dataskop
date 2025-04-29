// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Methoden "strings" für die SignalR-Hub Methoden</para>
    ///     Klasse ExGatewayMethods. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class GatewayConstants
    {
        /// <summary>
        ///     Name des Hub
        /// </summary>
        public static string HubName = "gwhub";

        #region Properties

        /// <summary>
        ///     Gateway sendet eine Liste von ExValue
        /// </summary>
        public static string Send
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Send)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        /// <summary>
        ///     Gateway sendet eine Liste von ExValue
        /// </summary>
        public static string Create
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Create)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        /// <summary>
        ///     Hub sendet eine Config an einen Gateway
        /// </summary>
        public static string Conf
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Conf)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        /// <summary>
        ///     Gateway senden Infos an den Hub
        /// </summary>
        public static string Reg
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Reg)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        /// <summary>
        ///     IotGerät - Verbindungsupdate
        /// </summary>
        public static string Du
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Du)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        /// <summary>
        ///     Downlink an ein iot gerät Sended
        /// </summary>
        public static string Downlink
        {
            get
            {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Exception($"[{nameof(GatewayConstants)}]({nameof(Downlink)}): Not for access!");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
        }

        #endregion
    }
}