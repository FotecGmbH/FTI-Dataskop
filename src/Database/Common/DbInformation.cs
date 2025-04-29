// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Microsoft.EntityFrameworkCore;

namespace Database.Common
{
    /// <summary>
    ///     Information für Firmen
    /// </summary>
    [Owned]
    public class DbInformation
    {
        #region Properties

        /// <summary>
        ///     Name
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Beschreibung
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Erzeugt am
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        ///     Zuletzt geändert am
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        #endregion
    }
}