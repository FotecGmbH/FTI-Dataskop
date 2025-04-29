// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>Helper für DB-NewValueNotification</para>
///     Klasse HelperDbNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IQueryable<TableNewValueNotification> GetNewValueNotificationsForUserId(long userId, bool includeMeasurementDefinitions = false)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var result = TblNewValueNotifications
            .Where(x => x.TblUserId == userId);

        if (includeMeasurementDefinitions)
        {
            result = result.Include(x => x.TblMeasurementDefinition);
        }

        return result;
    }
}