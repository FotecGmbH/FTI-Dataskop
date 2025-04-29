// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse GeosphereDataPoint. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GeosphereDataPoint
    {
        #region Properties

        /// <summary>
        /// Timestamp of the data point
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Name of the data point
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the data point
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Value of the data point
        /// </summary>
        public double? Data { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Coordinates of the data point
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Coordinates of the data point
        /// </summary>
        public double Y { get; set; }

        #endregion
    }
}