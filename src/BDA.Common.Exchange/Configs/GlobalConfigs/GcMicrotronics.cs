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
using BDA.Common.Exchange.Configs.Interfaces;
using Biss.Interfaces;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.GlobalConfigs
{
    /// <summary>
    ///     <para>Microtronics Configuration</para>
    ///     Klasse GcMicrotronics. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Config for Microtronics")]
    public class GcMicrotronics : IBissModel, IGlobalConfig
    {
        #region Properties

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; } = EnumGlobalConfigTypes.Microtronics;

        /// <summary>
        ///     User name for Microtronics
        /// </summary>
        [ConfigProperty("Microtronics Username")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Password for Microtronics
        /// </summary>
        [ConfigProperty("Microtronics Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        ///     Backend domaene for Microtronics
        /// </summary>
        [ConfigProperty("Microtronics Backend Domain")]
        public string BackendDomain { get; set; } = "https://austria.microtronics.com";

        #endregion

        /// <summary>
        ///     Creates object from jsonstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcMicrotronics? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcMicrotronics>(config);
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}