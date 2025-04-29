// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Apps.Interfaces;

namespace WebExchange.Interfaces
{
    /// <summary>
    ///     <para>E-Mail Settings Interface</para>
    ///     Interface IAppSettingsEMail. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAppSettingsEMail : IAppSettingsBase
    {
        #region Properties

        /// <summary>
        ///     Als "wer" (E-Mail Adresse) wird gesendet (für Antworten)
        /// </summary>
        string SendEMailAs { get; }

        /// <summary>
        ///     Welcher Name des Senders wird angezeigt
        /// </summary>
        string SendEMailAsDisplayName { get; }

        /// <summary>
        ///     Sendgrid Key (falls Sendgrid verwendet wird)
        /// </summary>
        string SendGridApiKey { get; }

        #endregion
    }
}