// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Generic;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>IoT Gerät</para>
    ///     Klasse TableIoTDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDataconverter : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Bytearray welches das Assembly beinhaltet.
        /// </summary>
        public string CodeSnippet { get; set; } = "";

        /// <summary>
        ///     Anzeigename des Konverters
        /// </summary>
        public string Displayname { get; set; } = "";

        /// <summary>
        ///     Beschreibung des Konverters
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        ///     Measurementdefinitions
        /// </summary>
        public List<ExMeasurementDefinitionTemplate> Templates { get; set; } = new();

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}