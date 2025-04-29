// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Database.Common;
using NetTopologySuite.Geometries;

namespace Database.Tables;

/// <summary>
///     <para>Ergebniss einer Messung</para>
///     Klasse TableMeasurementResult.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("MeasurementResult")]
public class TableMeasurementResult : IAdditionalConfiguration
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Zeitstempel, bei dem die Daten gesammelt wurden
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    ///     Wurde an folgendem Ort aufgenommen
    /// </summary>
    public DbPosition Location { get; set; } = new();

    /// <summary>
    ///     Punkt für die Durchführung der Ortsbezogenen Daten
    ///     TODO: Automatische Berechnung anstatt getter setter
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Point SpatialPoint { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Typ des Wertes
    /// </summary>
    public EnumValueTypes ValueType { get; set; }

    /// <summary>
    ///     Messwert
    /// </summary>
    public DbValue Value { get; set; } = new();

    /// <summary>
    ///     Zusätzliche dynamische Konfiguration (JSON)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalConfiguration { get; set; } = string.Empty;

    /// <summary>
    ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalProperties { get; set; } = string.Empty;

    /// <summary>
    ///     Zugehöriger Definition des Messwerts
    /// </summary>
    public long TblMeasurementDefinitionId { get; set; }

    /// <summary>
    ///     Zugehörige Messdefinitionen
    /// </summary>
    [ForeignKey(nameof(TblMeasurementDefinitionId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableMeasurementDefinition TblMeasurementDefinition { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    #endregion
}