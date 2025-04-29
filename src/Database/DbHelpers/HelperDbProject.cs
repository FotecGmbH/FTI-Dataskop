// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using BDA.Common.Exchange.Enum;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>DESCRIPTION</para>
///     Klasse HelperDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle Firmen für einen User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableProject> GetTableProjectForUser(long userId)
    {
        var isAdmin = IsUserAdmin(userId);
        var r = TblProjects.AsNoTracking().Where(c =>
            isAdmin ||
            c.TblCompany.CompanyType == EnumCompanyTypes.PublicCompany ||
            (c.TblCompany.TblPermissions.Any(a => a.TblUserId == userId))
        ).Include(i => i.TblMeasurementDefinitionToProjectAssignments);
        return r;
    }
}