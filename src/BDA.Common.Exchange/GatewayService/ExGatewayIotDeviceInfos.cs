// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Enum;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Infos vom Gateway über "seine" Iot-Devices</para>
    ///     Klasse ExGatewayIotDeviceInfos. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGatewayIotDeviceInfos : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Iot Device Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Aktueller Status des IotDevice
        /// </summary>
        public EnumDeviceOnlineState State { get; set; }

        /// <summary>
        ///     Name des IoT Geräts
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Version im Gerät
        /// </summary>
        public string Firmwareversion { get; set; } = string.Empty;

        /// <summary>
        ///     Version der Configuration im Gerät
        /// </summary>
        public long Configversion { get; set; }

        /// <summary>
        ///     Wann ist das Gerät das letzte Mal online gegangen
        /// </summary>
        public DateTime LastOnlineTime { get; set; }

        /// <summary>
        ///     Wann ist das Gerät das letzte Mal offline gegangen
        /// </summary>
        public DateTime LastOfflineTime { get; set; }

        /// <summary>
        ///     "Secret" des Geräts. Wird von Gerät selbst erzeugt.
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}