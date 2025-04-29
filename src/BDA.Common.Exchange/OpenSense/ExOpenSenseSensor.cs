// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.OpenSense
{
    /// <summary>
    ///     <para>OpensenseSensor</para>
    ///     Klasse ExOpenSenseSensor. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExOpenSenseSensor : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Die ID des Sensors auf Opensense
        /// </summary>
        public string OpensenseId { get; set; } = "";

        /// <summary>
        ///     Title
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        ///     Unit
        /// </summary>
        public string Unit { get; set; } = "";

        /// <summary>
        ///     SensorType
        /// </summary>
        public string SensorType { get; set; } = "";

        /// <summary>
        ///     Value
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        ///     Date the entry was created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Altitude
        /// </summary>
        public int Altitude { get; set; }

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}