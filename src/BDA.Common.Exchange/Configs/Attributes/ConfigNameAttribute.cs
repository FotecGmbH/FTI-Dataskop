// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Configs.Attributes
{
    /// <summary>
    ///     <para>Name einer Konfiguration Attribute</para>
    ///     Klasse ConfigNameAttribute. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CA1813 // Avoid unsealed attributes
    public class ConfigNameAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
    {
        /// <summary>
        ///     Name einer Konfiguration Attribute
        /// </summary>
        /// <param name="label"></param>
        public ConfigNameAttribute(string label)
        {
            Label = label;
        }

        #region Properties

        /// <summary>
        ///     Name
        /// </summary>
        public virtual string Label { get; }

        #endregion
    }
}