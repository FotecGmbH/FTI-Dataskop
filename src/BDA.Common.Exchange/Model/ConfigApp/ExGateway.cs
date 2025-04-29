// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Daten eines Gateway</para>
    ///     Klasse ExGateway. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGateway : IBissModel, IBissSelectable, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     IoT Device ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new();

        /// <summary>
        ///     Attribute des Gateways (gleiche besitz ein IoT Device - MFa eventuell eine eigene Tabelle?)
        /// </summary>
        public ExCommonInfo DeviceCommon { get; set; } = new();

        /// <summary>
        ///     Standort an dem sich das Gatway befindet
        /// </summary>
        public ExPosition Location { get; set; } = new();

        /// <summary>
        ///     Zusätzliche dynamische Konfiguration (JSON)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
        /// </summary>
        public string AdditionalProperties { get; set; } = string.Empty;

        /// <summary>
        ///     Firma die der Berechtigung zugewiesen ist
        /// </summary>
        public long TblCompanyId { get; set; }

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Id der Firma die der Berechtigung zugewiesen ist
        /// </summary>
        public long CompanyId { get; set; }

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