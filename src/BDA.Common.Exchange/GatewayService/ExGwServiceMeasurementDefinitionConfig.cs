// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>"Sensor"</para>
    ///     Klasse ExIotDeviceSensor. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGwServiceMeasurementDefinitionConfig : IBissModel, IAdditionalConfiguration
    {
        /// <summary>
        ///     Konstruktor fuer json deserialise
        /// </summary>
        public ExGwServiceMeasurementDefinitionConfig()
        {
        }

        #region Properties

        /// <summary>
        ///     DB Id (TableMeasurementDefinition)
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Name des Messwerts (für Debug-Ausgaben)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Zusätzliche dynamische Konfiguration (JSON)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Messinterval in Zehntel Sekunden (1=100ms)
        ///     Nur für .NET basierende Iot Devices, -1 wird das Interval vom Iot Device verwendet
        /// </summary>
        public int MeasurementInterval { get; set; }

        /// <summary>
        ///     Art des Werts
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        /// <summary>
        ///     Wie wird der Messwert erfasst? Bussystem (I2C, SPI, ...) oder im Iot (Chip) selbst (ESP32, PI, ...)
        /// </summary>
        public EnumIotDeviceDownstreamTypes DownstreamType { get; set; }

        /// <summary>
        ///     Zeitpunkt der Übertragung
        /// </summary>
        public EnumTransmission TransmissionType { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer uerberschreitung
        /// </summary>
        public bool IfThresholdExceed { get; set; }

        /// <summary>
        ///     Schwellenwert fuer uerberschreitung
        /// </summary>
        public float ThresholdExceedValue { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer unterschreitung
        /// </summary>
        public bool IfThresholdFallBelow { get; set; }

        /// <summary>
        ///     Schwellenwert fuer unterschreitung
        /// </summary>
        public float ThresholdFallBelowValue { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer delta
        /// </summary>
        public bool IfThresholdDelta { get; set; }

        /// <summary>
        ///     Schwellenwert fuer Delta
        /// </summary>
        public float ThresholdDeltaValue { get; set; }

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