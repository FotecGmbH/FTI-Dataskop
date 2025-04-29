// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Interfaces
{
    /// <summary>
    ///     <para>Interface für Objekte mit AdditionalConfiguration</para>
    ///     Interface IAdditionalConfiguration. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     Additional Configuration
        /// </summary>
        string AdditionalConfiguration { get; set; }

        #endregion
    }
}