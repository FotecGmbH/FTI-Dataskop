// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Interfaces;

namespace Database.Tables;

/// <summary>
///     <para>Zuordnung einer Messdefinition zu Projekten</para>
///     Klasse TableMeasurementDefinitionToProjectAssignment. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("MeasurementDefinitionToProjectAssignment")]
public class TableMeasurementDefinitionToProjectAssignment : IBissSerialize
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     Id der Messdefinition
    /// </summary>
    public long? TblMeasurementDefinitionId { get; set; }

    /// <summary>
    ///     Zuordnung zur Messdefinition
    /// </summary>
    [ForeignKey(nameof(TblMeasurementDefinitionId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableMeasurementDefinition? TblMeasurmentDefinitions { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Projekt ID
    /// </summary>
    public long? TblProjctId { get; set; }

    /// <summary>
    ///     Zurodnung zu einem Projekt
    /// </summary>
    [ForeignKey(nameof(TblProjctId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableProject? TblProjcts { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    #endregion
}