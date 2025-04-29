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
    ///     <para>Einstellungen für einen "Datenspeicher"</para>
    ///     Interface IAppSettingsDataBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAppSettingsDataBase
    {
        #region Properties

        /// <summary>
        ///     Gesamter Connection-String (ist dieser leer wird er automatisch aus den anderen Properties generiert)
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        ///     Datenbank
        /// </summary>
        string ConnectionStringDb { get; }

        /// <summary>
        ///     Datenbank-Server
        /// </summary>
        string ConnectionStringDbServer { get; }

        /// <summary>
        ///     Db User
        /// </summary>
        string ConnectionStringUser { get; }

        /// <summary>
        ///     Db User Passwort
        /// </summary>
        string ConnectionStringUserPwd { get; }

        #endregion
    }
}