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
    public IQueryable<TableCompany> GetTableCompanyForUser(long userId)
    {
        var isAdmin = IsUserAdmin(userId);
        var r = TblCompanies.AsNoTracking().Where(c =>
            isAdmin ||
            c.CompanyType == EnumCompanyTypes.PublicCompany || c.CompanyType == EnumCompanyTypes.NoCompany ||
            (c.TblPermissions.Any(a => a.TblUserId == userId))
        );
        return r;
    }

    /// <summary>
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="noTracking"></param>
    /// <returns></returns>
    public TableCompany? GetCompanyWithDependencies(long companyId, bool noTracking = false)
    {
        if (noTracking)
        {
            return TblCompanies.AsNoTracking()
                .Include(c => c.TblProjects)
                .Include(c => c.TblGateways)
                .ThenInclude(g => g.TblIotDevices)
                .ThenInclude(i => i.TblMeasurementDefinitions).FirstOrDefault(c => c.Id == companyId);
        }

        return TblCompanies
            .Include(c => c.TblProjects)
            .Include(c => c.TblGateways)
            .ThenInclude(g => g.TblIotDevices)
            .ThenInclude(i => i.TblMeasurementDefinitions).FirstOrDefault(c => c.Id == companyId);
    }
}