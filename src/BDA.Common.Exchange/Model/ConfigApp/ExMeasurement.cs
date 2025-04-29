// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>ExMeasurement - aktueller Messwert aus DB für Apps</para>
    ///     Klasse ExMeasurement. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExMeasurement : IBissModel
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Anzahl der gesamt erfassten Messwerte in der DB
        /// </summary>
        public long ValueCounter { get; set; }

        /// <summary>
        ///     Zeitstempel, bei dem die Daten gesammelt wurden
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Anzeigbarer Wert
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        ///     Wurde an folgendem Ort aufgenommen
        /// </summary>
        public ExPosition Location { get; set; } = new();

        /// <summary>
        ///     Infos zur Quelle für UI
        /// </summary>
        public string SourceInfo { get; set; } = string.Empty;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}