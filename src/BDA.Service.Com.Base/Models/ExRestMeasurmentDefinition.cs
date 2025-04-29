// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     <para>Exchange Rest Messwertdefinition</para>
    ///     Klasse ExRestMeasurmentDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExRestMeasurmentDefinition
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>

        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

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
        ///     Wie wird der Messwert erfasst? Bussystem (I2C, SPI, ...), Drittsystem (Ttn, ...) oder im Iot (Chip) selbst (ESP32,
        ///     PI, ...)
        /// </summary>
        public EnumIotDeviceDownstreamTypes DownstreamType { get; set; }

        #endregion
    }
}