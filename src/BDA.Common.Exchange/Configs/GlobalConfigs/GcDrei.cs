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
    ///     <para>Drei Configuration</para>
    ///     Klasse GcDrei. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Config for Drei")]
    public class GcDrei : IBissModel, IGlobalConfig
    {
        #region Properties

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; } = EnumGlobalConfigTypes.Drei;

        /// <summary>
        ///     Loginname for Drei
        /// </summary>
        [ConfigProperty("Drei Loginname")]
        public string LoginName { get; set; } = string.Empty;

        /// <summary>
        ///     Password for Drei
        /// </summary>
        [ConfigProperty("Drei Password")]
        public string Password { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///     Creates object from jsonstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcDrei? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcDrei>(config);
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