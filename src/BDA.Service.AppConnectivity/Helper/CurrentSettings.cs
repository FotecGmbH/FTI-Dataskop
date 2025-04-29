// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Service.AppConnectivity.Helper
{
    /// <summary>
    ///     <para>Aktuelle Einstellungen</para>
    ///     Klasse CurrentSettings. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class CurrentSettings
    {
        #region Properties

        /// <summary>
        ///     AGB
        /// </summary>
        public string Agb { get; set; } = null!;

        /// <summary>
        ///     Aktuelle App Version
        /// </summary>
        public string CurrentAppVersion { get; set; } = null!;

        /// <summary>
        ///     Minimale App Version
        /// </summary>
        public string MinAppVersion { get; set; } = null!;

        /// <summary>
        ///     Allgemeine Nachricht
        /// </summary>
        public string CommonMessage { get; set; } = null!;

        #endregion
    }
}