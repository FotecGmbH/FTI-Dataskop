// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Microsoft.EntityFrameworkCore;

namespace Database.Common;

/// <summary>
///     <para>Wert</para>
///     Klasse Value. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Owned]
public class DbValue
{
    #region Properties

    /// <summary>
    ///     Text
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Text { get; set; } = String.Empty;

    /// <summary>
    ///     Nummer
    /// </summary>
    public double? Number { get; set; }

    /// <summary>
    ///     Binärdaten
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
    public byte[]? Binary { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

    /// <summary>
    ///     0 oder 1
    /// </summary>
    public bool? Bit { get; set; }

    #endregion
}