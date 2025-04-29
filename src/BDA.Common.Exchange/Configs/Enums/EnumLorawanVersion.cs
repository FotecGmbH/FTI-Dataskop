// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

// ReSharper disable InconsistentNaming

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>LoRaWAN Macversion</para>
    ///     Klasse EnumLorawanVersion. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnumLorawanVersion
    {
        MAC_UNKNOWN,
        MAC_V1_0,
        MAC_V1_0_1,
        MAC_V1_0_2,
        MAC_V1_1,
        MAC_V1_0_3,
        MAC_V1_0_4
    }
}