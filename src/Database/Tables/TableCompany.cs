// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BDA.Common.Exchange.Enum;
using Database.Common;

namespace Database.Tables
{
    /// <summary>
    ///     <para>Firmen Tabelle für DB</para>
    ///     Klasse TableCompany. (C) 2020 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("Company")]
    public class TableCompany
    {
        #region Properties

        /// <summary>
        ///     Device ID für DB
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public DbInformation Information { get; set; } = new DbInformation();

        /// <summary>
        ///     Typ der Firma
        /// </summary>
        public EnumCompanyTypes CompanyType { get; set; } = EnumCompanyTypes.Company;


        /// <summary>
        ///     Permissions der Firma
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TablePermission> TblPermissions { get; set; } = new List<TablePermission>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Gateways der Firma
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TableGateway> TblGateways { get; set; } = new List<TableGateway>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Projekte der Firma
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TableProject> TblProjects { get; set; } = new List<TableProject>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Konfigurationen der Firma
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public virtual ICollection<TableCompanyGlobalConfig>? TblCompanyGlobalConfigurations { get; set; } = new List<TableCompanyGlobalConfig>();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion
    }
}