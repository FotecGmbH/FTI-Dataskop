// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>Converter für die Tabelle TableCompanyGlobalConfig</para>
///     Klasse ConverterDbCompanyGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbCompanyGlobalConfig
{
    /// <summary>
    ///     Datenbankeintrag nach ExGlobalConfig
    /// </summary>
    /// <param name="t">TableCompanyGlobalConfig</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExGlobalConfig ToExGlobalConfig(this TableCompanyGlobalConfig t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException(nameof(t));
        }

        return new ExGlobalConfig
        {
            Id = t.Id,
            Information = t.Information.ToExInformation(),
            CompanyId = t.TblCompanyId,
            AdditionalConfiguration = t.AdditionalConfiguration,
            ConfigType = t.GlobalConfigType,
            ConfigVersion = t.ConfigVersion,
            IsUsedInIotDevice = t.TblIotDevice is {Count: > 0}
        };
    }

    /// <summary>
    ///     ExGlobalConfig in eine TableCompanyGlobalConfig übernehmen
    /// </summary>
    /// <param name="c">ExGlobalConfig</param>
    /// <param name="t">TableCompanyGlobalConfig</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTableCompanyGlobalConfig(this ExGlobalConfig c, TableCompanyGlobalConfig t)
    {
        if (c == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(ToTableCompanyGlobalConfig)}): {nameof(c)} is null");
        }

        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(ToTableCompanyGlobalConfig)}): {nameof(t)} is null");
        }

        c.Information.ToDbInformation(t.Information);
        t.AdditionalConfiguration = c.AdditionalConfiguration;
        t.TblCompanyId = c.CompanyId;
        t.ConfigVersion = c.ConfigVersion + 1;
        t.GlobalConfigType = c.ConfigType;
    }
}