// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbDevice
{
    /// <summary>
    ///     Konvertieren in ExDeviceInfo
    /// </summary>
    /// <param name="t">Daten</param>
    /// <returns></returns>
    public static ExDeviceInfo ToExDeviceInfo(this TableDevice t)
    {
        if (t == null!)
        {
            throw new NullReferenceException($"[{nameof(ConverterDbCompany)}]({nameof(ToExDeviceInfo)}): {nameof(TableDevice)} is null!");
        }

        return new ExDeviceInfo
        {
            LastDateTimeUtcOnline = t.LastDateTimeUtcOnline,
            IsAppRunning = t.IsAppRunning,
            ScreenResolution = string.IsNullOrEmpty(t.ScreenResolution) ? string.Empty : t.ScreenResolution,
            RefreshToken = string.IsNullOrEmpty(t.RefreshToken) ? string.Empty : t.RefreshToken,
            LastLogin = t.LastLogin ?? DateTime.MinValue,
            AppVersion = t.AppVersion,
            DeviceHardwareId = t.DeviceHardwareId,
            Plattform = t.Plattform,
            DeviceIdiom = t.DeviceIdiom,
            OperatingSystemVersion = t.OperatingSystemVersion,
            DeviceType = t.DeviceType,
            DeviceName = t.DeviceName,
            Model = t.Model,
            Manufacturer = t.Manufacturer,
            DeviceToken = t.DeviceToken
        };
    }
}