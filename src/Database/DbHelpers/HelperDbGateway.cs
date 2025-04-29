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
///     <para>Hilfsfunktionen für TableGateway</para>
///     Klasse HelperDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle Gateways für einen User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableGateway> GetTableGatewayForUser(long userId)
    {
        var r = TblGateways.AsNoTracking().Where(c =>
            c.TblCompany.CompanyType == EnumCompanyTypes.PublicCompany || c.TblCompany.CompanyType == EnumCompanyTypes.NoCompany ||
            (c.TblCompany.TblPermissions.Any(a => a.TblUserId == userId) ||
                TblUsers.Any(a => a.Id == userId && a.IsAdmin)
            ));
        return r;
    }
}