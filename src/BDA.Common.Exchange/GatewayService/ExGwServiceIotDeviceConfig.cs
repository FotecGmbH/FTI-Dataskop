// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Interfaces;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Die Konfiguration für einen Sensor</para>
    ///     Klasse ExIotDeviceConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGwServiceIotDeviceConfig : IBissModel, IConfigBase, IAdditionalConfiguration
    {
        /// <summary>
        ///     Initialisiere ExGwServiceIotDeviceConfig
        /// </summary>
        public ExGwServiceIotDeviceConfig()
        {
        }

        #region Properties

        /// <summary>
        ///     Datenbank Id des Iot Geräts (Tabelle TableIotDevice)
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Konfigurierter  Name des Gateway oder IotDevice
        /// </summary>
        public string Name { get; set; } = "NoIotDeviceName";

        /// <summary>
        ///     Secret - eventuell nicht benötigt beim Iot Device
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     Firmware Version des IoT Device
        /// </summary>
        public string FirmwareVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Config Version des IoT Device
        /// </summary>
        public long ConfigVersion { get; set; } = -1;

        /// <summary>
        ///     Config Version des IoT Device am BDA Backend Service
        /// </summary>
        public long ConfigVersionService { get; set; } = -1;

        /// <summary>
        ///     Wie werden die Daten an das Gateway übertragen
        /// </summary>
        public EnumIotDeviceUpstreamTypes UpStreamType { get; set; }

        /// <summary>
        ///     Plattform des Iot Geräts
        /// </summary>
        public EnumIotDevicePlattforms Plattform { get; set; }

        /// <summary>
        ///     Zeitpunkt der Übertragung
        /// </summary>
        public EnumTransmission TransmissionTime { get; set; }

        /// <summary>
        ///     Übertragungsinterval in Sekunden (Entweder alle X Mal oder X Sekunden)
        ///     Kleiner, gleich 0 => Sofort
        /// </summary>
        public int TransmissionInterval { get; set; }

        /// <summary>
        ///     Messinterval in Zehntel Sekunden (1=100ms)
        ///     Für Iot-Embedded "C" Geräte wird nur diese Zeit verwendet
        ///     Das Messintervall bei TableMeasurementDefinition spielt dann keine Rolle
        /// </summary>
        public int MeasurementInterval { get; set; }

        /// <summary>
        ///     Zusätzliche dynamische Konfiguration (JSON)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Fallback Standort wenn Iot Device kein GPS besitzt
        /// </summary>
        public ExPosition FallbackPosition { get; set; } = new ExPosition();

        /// <summary>
        ///     Sensoren des Iot Geräts
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ExGwServiceMeasurementDefinitionConfig> MeasurementDefinition { get; set; } = new List<ExGwServiceMeasurementDefinitionConfig>();
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