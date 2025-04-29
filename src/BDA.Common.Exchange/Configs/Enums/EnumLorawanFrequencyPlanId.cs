// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

// ReSharper disable InconsistentNaming

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Frequencyplan of a LoRaWAN Device (Only European Frequencies at this Point)</para>
    ///     Klasse EnumLorawanFrequencyPlanId. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnumLorawanFrequencyPlanId
    {
        /// <summary>
        ///     Europe 863-870 MHz (SF12 for RX2)
        /// </summary>
        EU_863_870,

        /// <summary>
        ///     Europe 863-870 MHz (SF9 for RX2 - recommended)
        /// </summary>
        EU_863_870_TTN,

        /// <summary>
        ///     Europe 863-870 MHz, 6 channels for roaming (Draft)
        /// </summary>
        EU_863_870_ROAMING_DRAFT,

        /// <summary>
        ///     Europe 433 MHz (ITU region 1)
        /// </summary>
        EU_433
    }
}