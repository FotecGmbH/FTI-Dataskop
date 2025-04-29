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
    ///     <para>Attribut für ein Property einer Konfiguration</para>
    ///     Klasse ConfigNameAttribute. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
#pragma warning disable CA1813 // Avoid unsealed attributes
    public class ConfigPropertyAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
    {
        /// <summary>
        ///     Attribut für ein Property einer Konfiguration
        /// </summary>
        /// <param name="label"></param>
        /// <param name="sortIndex"></param>
        public ConfigPropertyAttribute(string label, int sortIndex = 0)
        {
            Label = label;
            SortIndex = sortIndex;
        }

        #region Properties

        /// <summary>
        ///     Info
        /// </summary>
        public virtual string Label { get; }

        /// <summary>
        ///     Sort Index für Ui
        /// </summary>
        public virtual int SortIndex { get; }

        #endregion
    }
}