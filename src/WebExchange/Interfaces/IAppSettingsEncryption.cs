// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace WebExchange.Interfaces
{
    /// <summary>
    ///     <para>Einstellungen für Verschlüsselung</para>
    ///     Interface IAppSettingsEncryption. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAppSettingsEncryption
    {
        #region Properties

        /// <summary>
        ///     Privater Schlüssel f. Verschlüsselung
        /// </summary>
        string SymmetricEncryptionPrivateKey { get; set; }

        #endregion
    }
}