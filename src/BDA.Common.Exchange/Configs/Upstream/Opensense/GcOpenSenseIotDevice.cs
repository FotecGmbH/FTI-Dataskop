// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Enums;

namespace BDA.Common.Exchange.Configs.Upstream.Opensense
{
    /// <summary>
    ///     <para>Config für Opensense Devices</para>
    ///     Klasse GcOpenSenseIotDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Opensense Iot Device")]
    public class GcOpenSenseIotDevice : GcIotDevice
    {
        /// <summary>
        /// </summary>
        public GcOpenSenseIotDevice()
        {
            ConfigType = EnumGlobalConfigTypes.OpenSense;
        }

        #region Properties

        /// <summary>
        ///     Die Id auf Opensense
        /// </summary>
        public string? BoxId { get; set; } = null;

        /// <summary>
        ///     Datum ab dem historische Daten von OpenSense heruntergeladen werden sollen. Null => keine Daten laden.
        /// </summary>
        [ConfigProperty("Historische Daten herunterladen")]
        public DateTime? DownloadDataSince { get; set; } = null;

        #endregion
    }
}