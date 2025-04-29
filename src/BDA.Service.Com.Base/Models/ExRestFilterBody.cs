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
    ///     <para>Filter</para>
    ///     Klasse ExFilterBody. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExRestFilterBody
    {
        #region Properties

        /// <summary>
        ///     Filter für zusätzliche Attribute
        /// </summary>
        public string AdditionalProperties { get; set; } = null!;

        #endregion
    }
}