// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Globalization;
using BDA.Common.Exchange.Enum;
using Biss.Interfaces;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Positions Model</para>
    ///     Klasse ExPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExPosition : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Longitde
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        ///     Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        ///     Altitude
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        ///     Genauigkeit
        /// </summary>
        public double Presision { get; set; }

        /// <summary>
        ///     Zeitpunkt der GPS Position
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Quelle - Von wem stammen die Daten
        /// </summary>
        public EnumPositionSource Source { get; set; }

        /// <summary>
        ///     Link der Position
        /// </summary>
        [JsonIgnore]
        public string GoogleString => ToGoogleString();

        #endregion


        /// <summary>
        ///     Link der Position
        /// </summary>
        /// <returns></returns>
        public string ToGoogleString()
        {
            return "https://www.google.at/maps/@" + $"{Latitude.ToString(new CultureInfo("en"))},{Longitude.ToString(new CultureInfo("en"))}z";
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }

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