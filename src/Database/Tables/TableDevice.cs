// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Common;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace Database.Tables
{
    /// <summary>
    ///     <para>Device (Handys) Tabelle für die BDA.Service Apps (verbundene Geräte mit dem System)</para>
    ///     Klasse TableDevice. (C) 2020 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("Device")]
    public class TableDevice
    {
        #region Properties

        /// <summary>
        ///     Device ID für DB
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     HardwareId
        /// </summary>
        public string DeviceHardwareId { get; set; } = string.Empty;

        /// <summary>
        ///     Plattform des Gerätes
        /// </summary>
        public EnumPlattform Plattform { get; set; }

        /// <summary>
        ///     Geräteart
        /// </summary>
        public EnumDeviceIdiom DeviceIdiom { get; set; }

        /// <summary>
        ///     Version des Os
        /// </summary>
        public string OperatingSystemVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Info zum Device/Geräteart
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        ///     Name des Gerätes - vom User gesetzt
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the model of the device.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the manufacturer of the device.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        ///     Eindeutiger Token des Gerätes AKA PushId APPS: AzurePush WPF: To be implemented
        /// </summary>
        public string DeviceToken { get; set; } = string.Empty;

        /// <summary>
        ///     Version der App
        /// </summary>
        public string AppVersion { get; set; } = string.Empty;

        /// <summary>
        ///     LastDateTimeUtcOnline
        /// </summary>
        public DateTime LastDateTimeUtcOnline { get; set; }

        /// <summary>
        ///     IsAppRunning
        /// </summary>
        public bool IsAppRunning { get; set; }

        /// <summary>
        ///     Bildschirmauflösung
        /// </summary>
        public string? ScreenResolution { get; set; }

        /// <summary>
        ///     RefreshToken
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        ///     Letzer Login
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        ///     User der gerade oder als Letztes angemeldet war
        /// </summary>
        public long? TblUserId { get; set; }

        /// <summary>
        ///     User der gerade oder als Letztes angemeldet war
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
        public virtual TableUser? TblUser { get; set; }

        #endregion
    }
}