// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Datentransport</para>
    ///     Klasse ExValuesTransfer. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExValuesTransfer : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Von welchem Gateway kommen die Daten
        /// </summary>
        public long GatewayId { get; set; }

        /// <summary>
        ///     Messwerte
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ExValue> Mesurements { get; set; } = null!;
#pragma warning restore CA2227 // Collection properties should be read only

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