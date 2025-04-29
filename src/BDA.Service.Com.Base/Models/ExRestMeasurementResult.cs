// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     <para>Ergebniss einer Messung</para>
    ///     Klasse TableMeasurementResult.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExRestMeasurementResult
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Zeitstempel, bei dem die Daten gesammelt wurden
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Wurde an folgendem Ort aufgenommen
        /// </summary>
        public ExPosition Location { get; set; } = new ExPosition();

        /// <summary>
        ///     Typ des Wertes
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        /// <summary>
        ///     Messwert
        /// </summary>
        public string Value { get; set; } = string.Empty;


        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

        #endregion
    }
}