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
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;
using Database.Common;

namespace Database.Tables;

/// <summary>
///     <para>Globale Einstellungen für eine Firma</para>
///     Klasse TableCompanyGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("CompanyGlobalConfig")]
public class TableCompanyGlobalConfig : IBissSerialize, IAdditionalConfiguration
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Typ der Konfiguration
    /// </summary>
    public EnumGlobalConfigTypes GlobalConfigType { get; set; }

    /// <summary>
    ///     Informationen (Name, Beschreibung, ...)
    /// </summary>
    public DbInformation Information { get; set; } = new();

    /// <summary>
    ///     Inhalt der Konfiguration (Zugangsdaten, JSON, Connection-String, ...)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalConfiguration { get; set; } = string.Empty;

    /// <summary>
    ///     Version der Config
    /// </summary>
    public long ConfigVersion { get; set; }

    /// <summary>
    ///     Firma der die Konfiguration zugewiesen ist
    /// </summary>
    public long TblCompanyId { get; set; }

    /// <summary>
    ///     Firma der die Konfiguration zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblCompanyId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableCompany TblCompany { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Zuordnung zu Iot Device
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableIotDevice>? TblIotDevice { get; set; } = new List<TableIotDevice>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion
}