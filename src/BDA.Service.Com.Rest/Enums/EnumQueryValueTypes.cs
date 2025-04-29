// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Text.Json.Serialization;
using BDA.Common.Exchange.Enum;

namespace BDA.Service.Com.Rest.Enums;

/// <summary>
///     <para>DESCRIPTION</para>
///     Klasse EnumQueryValueTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnumQueryValueTypes
{
    /// <summary>
    ///     Double
    /// </summary>
    Number = EnumValueTypes.Number,

    /// <summary>
    ///     byte []
    /// </summary>
    Data = EnumValueTypes.Data,

    /// <summary>
    ///     string
    /// </summary>
    Text = EnumValueTypes.Text,

    /// <summary>
    ///     byte[]
    /// </summary>
    Image = EnumValueTypes.Image,

    /// <summary>
    ///     Alle
    /// </summary>
    All = -1
}