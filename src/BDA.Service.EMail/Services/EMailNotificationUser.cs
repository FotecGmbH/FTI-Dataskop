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
    ///     Email Notification User
    /// </summary>
    public class EMailNotificationUser
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
        ///     Beispieltext
        /// </summary>
        public string Text { get; set; } = string.Empty;

        #endregion
    }
}