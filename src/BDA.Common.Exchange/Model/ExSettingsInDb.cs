// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace BDA.Common.Exchange.Model
{
    /// <summary>
    ///     <para>Globale App Einstellungen (Versionen usw.) aus der Datenbank</para>
    ///     Klasse ExSettingsInDb. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExSettingsInDb : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Details zu den AGB's
        /// </summary>
        public string AgbString { get; set; } = "1.0.0";

        /// <summary>
        ///     Details zu den AGB's
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(AgbString))]
        public Version Agb => string.IsNullOrEmpty(AgbString) ? new Version("1.0.0") : new Version(AgbString);

        /// <summary>
        ///     Aktuelle Version in den Stores
        /// </summary>
        public string CurrentAppVersionString { get; set; } = "1.0.0";

        /// <summary>
        ///     Aktuelle Version in den Stores
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(CurrentAppVersionString))]
        public Version CurrentAppVersion => string.IsNullOrEmpty(CurrentAppVersionString) ? new Version("1.0.0") : new Version(CurrentAppVersionString);

        /// <summary>
        ///     Minimale App die auf den Clients sein muss
        /// </summary>
        public string MinAppVersionString { get; set; } = string.Empty;

        /// <summary>
        ///     Minimale App die auf den Clients sein muss
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(MinAppVersionString))]
        public Version MinAppVersion => string.IsNullOrEmpty(MinAppVersionString) ? new Version("1.0.0") : new Version(MinAppVersionString);

        /// <summary>
        ///     Allgemeine Meldung für die Clients (wird einmal beim App-Start angezeigt)
        /// </summary>
        public string CommonMessage { get; set; } = string.Empty;

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