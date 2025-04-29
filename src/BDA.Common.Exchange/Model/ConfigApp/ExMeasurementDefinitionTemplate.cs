// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Enum;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ExMeasurementDefinitionTemplate. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExMeasurementDefinitionTemplate : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Information
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     ID des dazugehörigen dataconverters
        /// </summary>
        public long DataconverterId { get; set; }

        /// <summary>
        ///     Art des Werts
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}