// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Projekt einer Firma</para>
    ///     Klasse ExProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExProject : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     Veröffentlicht/Aktiv
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        ///     Veröffentlicht am
        /// </summary>
        public DateTime PublishedDate { get; set; }

        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

        /// <summary>
        ///     Öffentlich, kann von jedem gesehen werden
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        ///     Id der Firma
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Messwerte für dieses Projekt
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<long> MeasurementDefinitions { get; set; } = new();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;

#pragma warning restore CS0067
#pragma warning restore CS0414

#pragma warning disable CS0414
        /// <inheritdoc />
        public event EventHandler<BissSelectableEventArgs> Selected = null!;
#pragma warning restore CS0414

        #endregion
    }
}