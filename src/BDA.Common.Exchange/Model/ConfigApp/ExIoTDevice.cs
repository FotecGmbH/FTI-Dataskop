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
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>ExIoTDevice</para>
    ///     Klasse ExIoTDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIotDevice : IBissModel, IBissSelectable, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     IoT Device ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     Der Datakonverter der zum parsen der Daten genutzt wird
        /// </summary>
        public long? DataConverterId { get; set; }

        /// <summary>
        ///     Standort
        /// </summary>
        public ExPosition Location { get; set; } = new();

        /// <summary>
        ///     Attribute des IotDevice (gleiche besitz ein Gateway - MFa eventuell eine eigene Tabelle?)
        /// </summary>
        public ExCommonInfo DeviceCommon { get; set; } = new();

        /// <summary>
        ///     Wie leitet das Iot Gerät Daten an ein Gateway weiter
        /// </summary>
        public EnumIotDeviceUpstreamTypes Upstream { get; set; }

        /// <summary>
        ///     Plattform des Iot Geräts
        /// </summary>
        public EnumIotDevicePlattforms Plattform { get; set; }

        /// <summary>
        ///     MeasurmentInterval anzeigen
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Plattform))]
        public bool IsIotDotnetSensor => Plattform == EnumIotDevicePlattforms.DotNet || Plattform == EnumIotDevicePlattforms.RaspberryPi || Plattform == EnumIotDevicePlattforms.Meadow;

        /// <summary>
        ///     Zeitpunkt der Übertragung
        /// </summary>
        public EnumTransmission TransmissionType { get; set; }

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
        public int MeasurmentInterval { get; set; }

        /// <summary>
        ///     Zusätzliche dynamische Konfiguration (JSON)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

        /// <summary>
        ///     Id des Gateways
        /// </summary>
        public long? GatewayId { get; set; }

        /// <summary>
        ///     Zugehörige Firma
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        ///     Zugehörige globale Config
        /// </summary>
        public long? GlobalConfigId { get; set; }

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Liste der Messdefinitionen
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<ExMeasurementDefinition> MeasurementDefinitions { get; set; } = new List<ExMeasurementDefinition>();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;

#pragma warning restore CS0067
#pragma warning restore CS0414

#pragma warning disable CS0414
        /// <inheritdoc />
        public event EventHandler<BissSelectableEventArgs> Selected = null!;
#pragma warning restore CS0414

        #endregion
    }
}