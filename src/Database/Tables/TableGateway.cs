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
using BDA.Common.Exchange.Interfaces;
using Database.Common;

namespace Database.Tables;

/// <summary>
///     <para>Gateway</para>
///     Klasse TableGateway. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Gateway")]
public class TableGateway : IAdditionalConfiguration
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
    ///     Attribute des Gateways (gleiche besitz ein IoT Device - MFa eventuell eine eigene Tabelle?)
    /// </summary>
    public DbDeviceCommon DeviceCommon { get; set; } = new();

    /// <summary>
    ///     Standort an dem sich das Gatway befindet
    /// </summary>
    public DbPosition Position { get; set; } = new();

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
    ///     Firma die der Berechtigung zugewiesen ist
    /// </summary>
    public long TblCompanyId { get; set; }

    /// <summary>
    ///     Firma die der Berechtigung zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblCompanyId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableCompany TblCompany { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Liste der IoT-Geräte, die mit dem Gateway verbunden sind
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<TableIotDevice> TblIotDevices { get; set; } = new List<TableIotDevice>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion
}