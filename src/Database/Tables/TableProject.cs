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
using Database.Common;

namespace Database.Tables;

/// <summary>
///     <para>Projekt</para>
///     Klasse TableProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Project")]
public class TableProject
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Informationen (Name, Beschreibung, ...)
    /// </summary>
    public DbInformation Information { get; set; } = new();

    /// <summary>
    ///     Veröffentlicht/Aktiv
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    ///     Veröffentlicht am
    /// </summary>
    public DateTime PublishedDate { get; set; }

    /// <summary>
    ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalProperties { get; set; } = string.Empty;

    /// <summary>
    ///     Firma der das Projekt zugewiesen ist
    /// </summary>
    public long TblCompanyId { get; set; }

    /// <summary>
    ///     Öffentlich, kann von jedem gesehen werden
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    ///     Firma der das Projekt zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblCompanyId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableCompany TblCompany { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Zurordnung der Messdefinitionenen
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<TableMeasurementDefinitionToProjectAssignment> TblMeasurementDefinitionToProjectAssignments { get; set; } = new List<TableMeasurementDefinitionToProjectAssignment>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion
}