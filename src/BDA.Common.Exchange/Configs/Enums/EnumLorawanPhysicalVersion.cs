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
    ///     <para>LoRaWAN Physical Version</para>
    ///     Klasse EnumLorawanPhysicalVersion. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnumLorawanPhysicalVersion
    {
        PHY_UNKNOWN,
        TS001_V1_0,
        TS001_V1_0_1,
        PHY_V1_0_2,
        PHY_V1_0_2_REV_B,
        PHY_V1_1_REV_A,
        PHY_V1_1_REV_B,
        PHY_V1_0_3_REV_A,
        PHY_V1_0_0,
        PHY_V1_0_1,
        PHY_V1_0_3
    }
}