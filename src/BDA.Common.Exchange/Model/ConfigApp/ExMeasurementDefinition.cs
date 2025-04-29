// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Messwertdefinition eines Iot Geräts</para>
    ///     Klasse ExMeasurementDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExMeasurementDefinition : IBissModel, IBissSelectable, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     Id aus DB
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     Zugehöriges Iot-Gerät
        /// </summary>
        public long IotDeviceId { get; set; }

        /// <summary>
        ///     Zu welcher Firma gehört die Definition
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Art des Werts
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        /// <summary>
        ///     Wie wird der Messwert erfasst? Bussystem (I2C, SPI, ...) oder im Iot (Chip) selbst (ESP32, PI, ...)
        /// </summary>
        public EnumIotDeviceDownstreamTypes DownstreamType { get; set; }

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Messinterval in Zehntel Sekunden (1=100ms)
        ///     Nur für .NET basierende Iot Devices, -1 wird das Interval vom Iot Device verwendet
        /// </summary>
        public int MeasurementInterval { get; set; }

        /// <summary>
        ///     Zusätzliche dynamische Konfiguration (JSON) - die eigentliche Konfiguration des Messwerts
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

        /// <summary>
        ///     Aktueller Wert aus Db
        /// </summary>
        public ExMeasurement CurrentValue { get; set; } = new();

        /// <summary>
        ///     Messwerte
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        [Obsolete]
        public ObservableCollection<ExMeasurement> Measurements { get; set; } = new();
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