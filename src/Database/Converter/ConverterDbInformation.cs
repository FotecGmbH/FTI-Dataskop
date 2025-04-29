// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Common;

namespace Database.Converter;

/// <summary>
///     <para>Db Project konvertieren</para>
///     Klasse ConverterDbProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbInformation
{
    /// <summary>
    ///     DbInfo zu ExInformation
    /// </summary>
    /// <param name="dbInformation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Wenn dbInformation null ist.</exception>
    public static ExInformation ToExInformation(this DbInformation dbInformation)
    {
        if (dbInformation == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(ToExInformation)}): {nameof(dbInformation)} is null");
        }

        return new ExInformation
        {
            CreatedDate = dbInformation.CreatedDate,
            Description = dbInformation.Description,
            Name = dbInformation.Name,
            UpdatedDate = dbInformation.UpdatedDate
        };
    }

    /// <summary>
    ///     ExInformation in ein DbInformation konvertieren
    /// </summary>
    /// <param name="i"></param>
    /// <param name="d"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToDbInformation(this ExInformation i, DbInformation d)
    {
        if (i == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(ToExInformation)}): {nameof(i)} is null");
        }

        if (d == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(ToExInformation)}): {nameof(d)} is null");
        }

        d.CreatedDate = i.CreatedDate;
        d.Description = i.Description;
        d.Name = i.Name;
        d.UpdatedDate = DateTime.UtcNow;
    }
}