// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Ex Information</para>
    ///     Klasse ExInformation. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExInformation : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Name (autm. kürzen) für UI Listen
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Name))]
        public string UiNameShort
        {
            get
            {
                if (Name.Length > 20)
                {
                    return $"{Name.Substring(0, 13)}...";
                }

                return Name;
            }
        }

        /// <summary>
        ///     Beschreibung
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Infos (Name + Bescheibung)
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Name), nameof(Description))]
        public string Info => string.IsNullOrEmpty(Description) ? $"{Name}" : $"{Name} - {Description}";

        /// <summary>
        ///     Erzeugt am
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        ///     Zuletzt geändert am
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        #endregion

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