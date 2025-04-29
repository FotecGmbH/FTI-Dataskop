// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     <para>This class represents a basic model of a measurement definition containing only basic information</para>
    ///     Klasse ExBasicMeasurementDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExBasicMeasurementDefinition
    {
        #region Properties

        /// <summary>
        ///     Basic information of the measurement definition enity.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ExBasicInformation Information { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        #endregion
    }
}