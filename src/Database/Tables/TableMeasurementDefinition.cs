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
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Database.Common;

namespace Database.Tables;

/// <summary>
///     <para>Definiton einer Messung</para>
///     Klasse TableMeasurementDefinition.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("MeasurementDefinition")]
public class TableMeasurementDefinition : IAdditionalConfiguration
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
    ///     Messinterval in Zehntel Sekunden (1=100ms)
    ///     Nur für .NET basierende Iot Devices, -1 wird das Interval vom Iot Device verwendet
    /// </summary>
    public int MeasurementInterval { get; set; }

    /// <summary>
    ///     Art des Werts
    /// </summary>
    public EnumValueTypes ValueType { get; set; }

    /// <summary>
    ///     Wie wird der Messwert erfasst? Bussystem (I2C, SPI, ...), Drittsystem (Ttn, ...) oder im Iot (Chip) selbst (ESP32,
    ///     PI, ...)
    /// </summary>
    public EnumIotDeviceDownstreamTypes DownstreamType { get; set; }

    /// <summary>
    ///     Zugehöriges Iot-Gerät
    /// </summary>
    public long TblIotDeviceId { get; set; }

    /// <summary>
    ///     Zugehöriges IoT Device
    /// </summary>
    [ForeignKey(nameof(TblIotDeviceId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableIotDevice TblIoTDevice { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Messwerte
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableMeasurementResult> TblMeasurements { get; set; } = new List<TableMeasurementResult>();
#pragma warning restore CA2227 // Collection properties should be read only

    /// <summary>
    ///     Zurordnung der Messdefinitionenen
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableMeasurementDefinitionToProjectAssignment> TblMeasurementDefinitionToProjectAssignments { get; set; } = new List<TableMeasurementDefinitionToProjectAssignment>();
#pragma warning restore CA2227 // Collection properties should be read only

    /// <summary>
    ///     Neuester Messwert. Wird per trigger von der Datenbank gesetzt
    /// </summary>
    public long? TblLatestMeasurementResultId { get; set; }

    #endregion
}