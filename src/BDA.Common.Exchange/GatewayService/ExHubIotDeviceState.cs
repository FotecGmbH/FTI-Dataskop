// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Status eines Iot Geräts hat sich geändert. Diese Info leitet der GW weiter an das BDA Service</para>
    ///     Klasse ExHubIotDeviceState. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExHubIotDeviceState
    {
        #region Properties

        /// <summary>
        ///     Id des Iot Geräts
        /// </summary>
        public long IotDeviceId { get; set; }

        /// <summary>
        ///     Status
        /// </summary>
        public EnumDeviceOnlineState State { get; set; }

        /// <summary>
        ///     Zeitpunkt der Änderung
        /// </summary>
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        ///     Version der Konfiguration
        /// </summary>
        public long ConfigVersion { get; set; }

        /// <summary>
        ///     Firmware Version am Iot Gerät
        /// </summary>
        public string FirmwareVersion { get; set; } = string.Empty;

        #endregion
    }
}