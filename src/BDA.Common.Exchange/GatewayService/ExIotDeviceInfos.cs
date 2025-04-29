// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Infos wenn sich ein Iot Device bei "seinem" Gateway meldet</para>
    ///     Klasse ExIotDeviceInfos. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIotDeviceInfos : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id (TableIotDevices) des Geräts -1 bei neuem Gerät
        /// </summary>
        public long DbId { get; set; } = -1;

        /// <summary>
        ///     Doku ToDo MKo
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     Name des Iot Geräts
        /// </summary>
        public string Name { get; set; } = "New Iot Device";

        /// <summary>
        ///     Beschreibung
        /// </summary>
        public string Description { get; set; } = "Description from new Iot Device";

        /// <summary>
        ///     Wie erfolgt die Verbindung mit dem Gateway
        /// </summary>
        public EnumIotDeviceUpstreamTypes UpstreamType { get; set; }

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