// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Generic;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     ExRestCompany.cs
    /// </summary>
    public class ExRestCompany
    {
        #region Properties

        /// <summary>
        ///     Device ID für DB
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new ExInformation();

        /// <summary>
        ///     Typ der Firma
        /// </summary>
        public EnumCompanyTypes CompanyType { get; set; } = EnumCompanyTypes.Company;

        /// <summary>
        ///     Liste von Projekten für die TreeView.
        /// </summary>
        public List<ExRestProject> Projects { get; set; } = new List<ExRestProject>();

        #endregion
    }

    /// <summary>
    ///     ExRestCompanyWithUserRights
    /// </summary>
    public class ExRestCompanyWithUserRights : ExRestCompany
    {
        #region Properties

        /// <summary>
        ///     Recht
        /// </summary>
        public EnumUserRight UserRight { get; set; }

        #endregion
    }
}