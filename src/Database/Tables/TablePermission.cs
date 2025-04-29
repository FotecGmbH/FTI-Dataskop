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

namespace Database.Tables;

/// <summary>
///     <para>Berechtigung eines Benutzers</para>
///     Klasse TableUserPermission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Permission")]
public class TablePermission : IAdditionalConfiguration
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Recht
    /// </summary>
    public EnumUserRight UserRight { get; set; }

    /// <summary>
    ///     Die Rolle des Benutzers.
    /// </summary>
    public EnumUserRole UserRole { get; set; }

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

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     User dem die Berechtigung zugewiesen ist
    /// </summary>
    public long TblUserId { get; set; }

    /// <summary>
    ///     User dem die Berechtigung zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblUserId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableUser TblUser { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

    #endregion
}