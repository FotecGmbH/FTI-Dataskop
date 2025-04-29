// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Tables;

/// <summary>
///     <para>IoT Gerät</para>
///     Klasse TableIoTDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Dataconverter")]
public class TableDataconverter
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Bytearray welches das Assembly beinhaltet.
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string CodeSnippet { get; set; } = "";

    /// <summary>
    ///     Anzeigename des Konverters
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Displayname { get; set; } = "";

    /// <summary>
    ///     Beschreibung des Konverters
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Description { get; set; } = "";

    /// <summary>
    ///     Measurementdefinitions
    /// </summary>
    public ICollection<TableMeasurementDefinitionTemplate> Templates { get; set; } = new List<TableMeasurementDefinitionTemplate>();

    #endregion
}