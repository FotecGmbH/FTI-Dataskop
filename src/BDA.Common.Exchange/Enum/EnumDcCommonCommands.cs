// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Dc Commands zum Server</para>
    ///     Klasse EnumDcCommonCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDcCommonCommands
    {
        /// <summary>
        ///     Datei löschen
        /// </summary>
        FileDelete,

        /// <summary>
        ///     Bestätigng E-Mail erneut senden
        /// </summary>
        ResendAccessEMail,

        /// <summary>
        ///     Passwort zurücksetzen starten
        /// </summary>
        ResetPassword,

        /// <summary>
        ///     Testpush senden
        /// </summary>
        SendTestPush,
    }

    /// <summary>
    ///     <para>Dc Commands zum Client</para>
    ///     Klasse EnumDcCommonCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDcCommonCommandsClient
    {
        /// <summary>
        ///     Allgemeine Meldung
        /// </summary>
        CommonMsg,

        /// <summary>
        ///     Dc Liste neu laden
        /// </summary>
        ReloadDcList
    }
}