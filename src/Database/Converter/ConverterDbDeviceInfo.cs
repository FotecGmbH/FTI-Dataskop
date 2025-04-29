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
public static class ConverterDbDeviceInfo
{
    /// <summary>
    ///     Konvertieren in TableDevice
    /// </summary>
    /// <param name="d">Daten</param>
    /// <param name="t">DB Element</param>
    /// <returns></returns>
    public static void ToTableDevice(this ExDeviceInfo d, TableDevice t)
    {
        if (d == null! || t == null)
        {
            throw new NullReferenceException($"[{nameof(ConverterDbDeviceInfo)}]({nameof(ToTableDevice)}): {nameof(ExDeviceInfo)} or {nameof(TableDevice)} is null!");
        }

        t.ScreenResolution = d.ScreenResolution;
        t.RefreshToken = d.RefreshToken;
        t.LastLogin = d.LastLogin;
        t.AppVersion = d.AppVersion;
        t.DeviceHardwareId = d.DeviceHardwareId;
        t.Plattform = d.Plattform;
        t.DeviceIdiom = d.DeviceIdiom;
        t.OperatingSystemVersion = d.OperatingSystemVersion;
        t.DeviceType = d.DeviceType;
        t.DeviceName = d.DeviceName;
        t.Model = d.Model;
        t.Manufacturer = d.Manufacturer;
        t.DeviceToken = d.DeviceToken;
    }
}