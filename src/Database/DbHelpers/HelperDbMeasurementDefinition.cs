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
///     <para>Helper für <see cref="TblMeasurementDefinitions" /> </para>
///     Klasse HelperDbMeasurementDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle Iot Geräte für einen User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableMeasurementDefinition> GetMeasurementDefinitionForUser(long userId)
    {
        var isAdmin = IsUserAdmin(userId);

        //var r = TblMeasurementDefinitions.AsNoTracking().Include(i => i.TblIoTDevice);

        var r = TblMeasurementDefinitions.AsNoTracking().Where(c =>
            isAdmin ||
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            c.TblIoTDevice!.TblGateway!.TblCompany.CompanyType == EnumCompanyTypes.PublicCompany ||
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            (c.TblIoTDevice!.TblGateway!.TblCompany.TblPermissions.Any(a => a.TblUserId == userId)
            )).Include(i => i.TblIoTDevice).ThenInclude(t => t.TblGateway).ThenInclude(t => t!.TblCompany).Include(i => i.TblMeasurements).Select(s => new TableMeasurementDefinition
        {
            TblMeasurements = s.TblMeasurements.Where(w => w.Id == s.TblMeasurements.Max(m => m.Id)).ToList(),
            AdditionalConfiguration = s.AdditionalConfiguration,
            AdditionalProperties = s.AdditionalProperties,
            DownstreamType = s.DownstreamType,
            Id = s.Id,
            Information = s.Information,
            MeasurementInterval = s.MeasurementInterval,
            TblIoTDevice = s.TblIoTDevice,
            TblIotDeviceId = s.TblIotDeviceId,
            ValueType = s.ValueType
        });

        return r;
    }
}