// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using Microsoft.EntityFrameworkCore;

namespace Database.Common;

/// <summary>
///     <para>Parameter welche Gateways und IoT Geräte beinhalten</para>
///     Klasse Version. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Owned]
public class DbDeviceCommon
{
    #region Properties

    /// <summary>
    ///     Version im Gerät
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string FirmwareversionDevice { get; set; } = string.Empty;

    /// <summary>
    ///     Version am Backend-Service
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string FirmwareversionService { get; set; } = string.Empty;

    /// <summary>
    ///     Version der Configuration im Gerät
    /// </summary>
    public long ConfigversionDevice { get; set; }

    /// <summary>
    ///     Version der Configuration am Backend-Service
    /// </summary>
    public long ConfigversionService { get; set; }

    /// <summary>
    ///     Aktueller Status des Geräts
    /// </summary>
    public EnumDeviceOnlineState State { get; set; }

    /// <summary>
    ///     Wann ist das Gerät das letzte Mal online gegangen
    /// </summary>
    public DateTime LastOnlineTime { get; set; }

    /// <summary>
    ///     Wann ist das Gerät das letzte Mal offline gegangen
    /// </summary>
    public DateTime LastOfflineTime { get; set; }

    /// <summary>
    ///     "Secret" des Geräts. Wird von Gerät selbst erzeugt.
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Secret { get; set; } = string.Empty;

    #endregion
}