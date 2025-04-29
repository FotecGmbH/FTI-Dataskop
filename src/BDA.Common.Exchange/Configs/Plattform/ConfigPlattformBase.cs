// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>Basis für eine Plattform (ESP32, Pi, ..)</para>
    ///     Klasse ConfigPlattformBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract class ConfigPlattformBase
    {
        /// <summary>
        ///     Basis für eine Plattform (ESP32, Pi, ..)
        /// </summary>
        /// <param name="plattform"></param>
        protected ConfigPlattformBase(EnumIotDevicePlattforms plattform)
        {
            Plattform = plattform;
        }

        #region Properties

        /// <summary>
        ///     Plattform selbst
        /// </summary>
        public EnumIotDevicePlattforms Plattform { get; internal set; }

        /// <summary>
        ///     Unterstütze Protokolle
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<EnumIotDeviceDownstreamTypes> SupportedDownstreamTypes { get; set; } = new List<EnumIotDeviceDownstreamTypes>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Mögliche durch die Plattform gegebene Messwerte
        /// </summary>
        public List<ExMeasurementDefinition> BuildInExMeasurementDefinitions { get; set; } = new List<ExMeasurementDefinition>();

        #endregion
    }
}