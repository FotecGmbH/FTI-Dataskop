﻿// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
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
    ///     <para>ExCompany</para>
    ///     Klasse ExCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExCompany : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new ExInformation();

        /// <summary>
        ///     Typ der Firma
        /// </summary>
        public EnumCompanyTypes CompanyType { get; set; } = EnumCompanyTypes.Company;

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

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