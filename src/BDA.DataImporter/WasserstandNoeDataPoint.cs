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
    ///     Klasse WasserstandNoeDataPoint. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class WasserstandNoeDataPoint
    {
        #region Properties

        public string date { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public WasserstandNoeLocation location { get; set; } = null!;
        public string id { get; set; } = string.Empty;
        public string unit { get; set; } = string.Empty;
        public string parameter { get; set; } = string.Empty;
        public string info { get; set; } = string.Empty;

        #endregion
    }
}