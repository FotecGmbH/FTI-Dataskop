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
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbDeviceCommon
{
    /// <summary>
    ///     DbDeviceCommon zu ExCommonInfo
    /// </summary>
    /// <param name="dbDeviceCommon"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExCommonInfo ToExCommonInfo(this DbDeviceCommon dbDeviceCommon)
    {
        if (dbDeviceCommon == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbDeviceCommon)}]({nameof(ToExCommonInfo)}): {nameof(dbDeviceCommon)}");
        }

        var exCommonInfo = new ExCommonInfo
        {
            ConfigversionDevice = dbDeviceCommon.ConfigversionDevice,
            ConfigversionServer = dbDeviceCommon.ConfigversionService,
            FirmwareversionDevice = dbDeviceCommon.FirmwareversionDevice,
            FirmwareversionService = dbDeviceCommon.FirmwareversionService,
            LastOfflineTime = dbDeviceCommon.LastOfflineTime,
            LastOnlineTime = dbDeviceCommon.LastOnlineTime,
            Secret = dbDeviceCommon.Secret,
            State = dbDeviceCommon.State
        };

        return exCommonInfo;
    }

    /// <summary>
    ///     ExCommonInfo in ein DbDeviceCommon kopieren
    /// </summary>
    /// <param name="i"></param>
    /// <param name="d"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToDbDeviceCommon(this ExCommonInfo i, DbDeviceCommon d)
    {
        if (i == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbDeviceCommon)}]({nameof(ToExCommonInfo)}): {nameof(i)}");
        }

        if (d == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbDeviceCommon)}]({nameof(ToExCommonInfo)}): {nameof(d)}");
        }

        d.FirmwareversionService = i.FirmwareversionService;
        d.ConfigversionService = i.ConfigversionServer;
        if (i.ConfigversionServer < i.ConfigversionDevice)
        {
            i.ConfigversionServer = i.ConfigversionDevice;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (i.Secret != null)
        {
            d.Secret = i.Secret;
        }

        d.ConfigversionService++;
    }
}