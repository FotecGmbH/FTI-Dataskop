// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Infos über ein Gateway welcher via SignalR verbunden ist</para>
    ///     Klasse ExHubGatewayInfos. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExHubGatewayInfos : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Db Id des Gateway (-1 wenn noch nicht registriert)
        /// </summary>
        public long GatewayId { get; set; } = -1;

        /// <summary>
        ///     Gateway Secret - um diesen einer Firma zuweisen zu können
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     Firmware Version des Gateway
        /// </summary>
        public string FirmwareVerion { get; set; } = string.Empty;

        /// <summary>
        ///     Config Version des Gateway
        /// </summary>
        public long ConfigVersion { get; set; } = -1;

        /// <summary>
        ///     Name des Gateway
        /// </summary>
        public string GatewayName { get; set; } = string.Empty;

        /// <summary>
        ///     Beschreibung
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Position des Gateway
        /// </summary>
        public ExPosition? Position { get; set; }

        /// <summary>
        ///     Iot Geräte welcher der Gateway verwaltet
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ExGatewayIotDeviceInfos> IotDevices { get; set; } = new();
#pragma warning restore CA2227 // Collection properties should be read only

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