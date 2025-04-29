// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Upstream.Microtronics
{
    /// <summary>
    ///     <para>Config für Microtronics Devices</para>
    ///     Klasse GcMicrotronicsIotDevice. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Microtronics Iot Device")]
    public class GcMicrotronicsIotDevice : GcIotDevice
    {
        /// <summary>
        /// </summary>
        public GcMicrotronicsIotDevice()
        {
            ConfigType = EnumGlobalConfigTypes.Microtronics;
        }

        #region Properties

        /// <summary>
        ///     Globaler (Firmen) Microtronics Account
        /// </summary>
        public GcMicrotronics GcMicrotronicsCompany { get; set; } = new();

        /// <summary>
        ///     Site id des Geraets
        /// </summary>
        public string SiteId { get; set; } = string.Empty;


        /// <summary>
        ///     Customer id des Geraets
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        ///     Datum ab dem historische Daten heruntergeladen werden sollen. Null => keine Daten laden.
        /// </summary>
        [ConfigProperty("Historische Daten herunterladen")]
        public DateTime? DownloadDataSince { get; set; } = null;


        /// <summary>
        ///     Configuration 0 - 9 der Site
        /// </summary>
        [ConfigProperty("Microtronics Configuration")]
        public string Configuration { get; set; } = "config0";

        /// <summary>
        ///     HistData Configuration 0 - 9 der Site
        /// </summary>
        [ConfigProperty("Microtronics HistData Configuration")]
        public string HistDataConfiguration { get; set; } = "histdata0";

        #endregion

        /// <summary>
        ///     Erzeugt ein <see cref="GcMicrotronicsIotDevice" /> Object aus dem AdditionalConfigstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcMicrotronicsIotDevice? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcMicrotronicsIotDevice>(config);
        }

#pragma warning disable CS0067
#pragma warning disable CS0414
       
#pragma warning disable CS0108, CS0114
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS0108, CS0114
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}