// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     <para>This class represents a basic project entity.</para>
    ///     Klasse ExBasicProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExBasicProject
    {
        #region Properties

        /// <summary>
        ///     Basic information of the project enity.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ExBasicInformation Information { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        ///     Measurement definitions of the company.
        /// </summary>
        public List<ExBasicMeasurementDefinition> MeasurementDefinitions { get; set; } = new List<ExBasicMeasurementDefinition>();

        #endregion
    }
}