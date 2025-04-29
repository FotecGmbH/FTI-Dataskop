// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Enum;
using Biss.Common;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model
{
    /// <summary>
    ///     <para>Infos zum aktuellem Gerät</para>
    ///     Klasse ExDeviceInfo. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDeviceInfo : IBissModel
    {
        #region Properties

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
        public string ScreenResolution { get; set; } = string.Empty;

        /// <summary>
        ///     RefreshToken
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        ///     Letzer Login
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     Aktuelle Type der App falls das Projekt aus mehr als einer App besteht
        /// </summary>
        public EnumAppType CurrentAppType { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}