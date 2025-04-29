// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Service.EMail.Services
{
    /// <summary>
    ///     Model für EMails, wird in den Views verwendet
    /// </summary>
    public class EMailModelUser
    {
        #region Properties

        /// <summary>
        ///     Vorname
        /// </summary>
        public string Firstname { get; set; } = string.Empty;

        /// <summary>
        ///     Nachname
        /// </summary>
        public string Lastname { get; set; } = string.Empty;

        /// <summary>
        ///     Benutzername
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        ///     Passwort
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        ///     Link
        /// </summary>
        public string Link { get; set; } = string.Empty;

        #endregion
    }
}