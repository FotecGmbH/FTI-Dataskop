// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
// ReSharper disable InconsistentNaming

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse WasserstandNoeLocation. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class WasserstandNoeLocation
    {
        #region Properties

        /// <summary>
        /// Name of the location
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Coordinates of the location
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// Coordinates of the location
        /// </summary>
        public double @long { get; set; }

        #endregion
    }
}