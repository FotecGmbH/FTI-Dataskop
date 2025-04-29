// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.Downstreams.OpenSense
{
    /// <summary>
    ///     <para>Mögliche Messwerte für OpenSense</para>
    ///     Klasse GcDownstreamOpenSense. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcOpensenseMeasurementDefinition : IBissModel
    {
        /// <summary>
        ///     Zusätzliche Konfiguration für die Measurementdefinitions die für ein Opensense device angelegt werden.
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public GcOpensenseMeasurementDefinition(string sensorId)
        {
        }

        /// <summary>
        /// </summary>
        public GcOpensenseMeasurementDefinition()
        {
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