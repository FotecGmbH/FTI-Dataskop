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
    ///     <para>Ttn Configuration</para>
    ///     Klasse GlobalConfigTtn. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Config for Ttn")]
    public class GcTtn : IBissModel, IGlobalConfig
    {
        #region Properties

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; } = EnumGlobalConfigTypes.Ttn;

        /// <summary>
        ///     Name der Konfiguration
        /// </summary>
        [ConfigProperty("Ttn user name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Der Api-Key für die ttn-applikation. Beispiel:
        ///     "00000.000000000000................"
        /// </summary>
        [ConfigProperty("Ttn API-Key")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        ///     Der TTN-Server. Beispiel: "eu1.cloud.thethings.network"
        /// </summary>
        [ConfigProperty("Ttn zone")]
        public string Zone { get; set; } = string.Empty;

        /// <summary>
        ///     Der TTN-Server. Beispiel: "eu1.cloud.thethings.network"
        /// </summary>
        [ConfigProperty("Ttn userid")]
        public string Userid { get; set; } = string.Empty;

        /// <summary>
        ///     Der TTN-Server. Beispiel: "eu1.cloud.thethings.network"
        /// </summary>
        [ConfigProperty("Ttn ApplicationID")]
        public string Applicationid { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///     Creates object from jsonstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcTtn? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcTtn>(config);
        }

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