// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Globalization;
using BDA.Common.Exchange.Enum;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Parameter welche Gateways und IoT Geräte beinhalten </para>
    ///     Klasse ExDeviceInformation. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExCommonInfo : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Version im Gerät
        /// </summary>
        public string FirmwareversionDevice { get; set; } = string.Empty;

        /// <summary>
        ///     Version am Server
        /// </summary>
        public string FirmwareversionService { get; set; } = string.Empty;

        /// <summary>
        ///     Version der Configuration im Gerät
        /// </summary>
        public long ConfigversionDevice { get; set; }

        /// <summary>
        ///     Version der Configuration im Gerät
        /// </summary>
        public long ConfigversionServer { get; set; }

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
        ///     Lokale Zeit
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(LastOfflineTime))]
        public DateTime LastOfflineTimeLocal => LastOfflineTime.ToLocalTime();


        /// <summary>
        ///     Lokale Zeit
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(LastOnlineTime))]
        public DateTime LastOnlineTimeLocal => LastOnlineTime.ToLocalTime();

        /// <summary>
        ///     "Secret" des Geräts. Wird von Gerät selbst erzeugt.
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     Aktuelle Info für UI
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(State), nameof(LastOfflineTime), nameof(LastOfflineTime))]
        public string Info => GetInfo();

        #endregion

        private string GetInfo()
        {
            switch (State)
            {
                case EnumDeviceOnlineState.Unknown:
                    return "Status unbekannt";
                case EnumDeviceOnlineState.Online:
                    return $"Online seit {LastOnlineTimeLocal.ToString("g", CultureInfo.CurrentUICulture)}";
                case EnumDeviceOnlineState.Offline:
                    return $"Offline seit {LastOfflineTimeLocal.ToString("g", CultureInfo.CurrentUICulture)}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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