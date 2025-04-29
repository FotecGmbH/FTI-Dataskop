// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using Biss.Dc.Core;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>DESCRIPTION</para>
///     Klasse DcExNewValueNotifications. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcExNewValueNotifications
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
    /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <param name="filter">Optionaler Filter für die Daten</param>
    /// <returns>Daten oder eine Exception auslösen</returns>
    public async Task<List<DcServerListItem<ExNewValueNotification>>> GetDcExNewValueNotifications(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var returnList = new List<DcServerListItem<ExNewValueNotification>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var newValueNotifications = db.GetNewValueNotificationsForUserId(userId);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (newValueNotifications is null)
        {
            return returnList;
        }

        foreach (var newValueNotification in newValueNotifications)
        {
            returnList.Add(new DcServerListItem<ExNewValueNotification>
            {
                Index = newValueNotification.Id,
                SortIndex = newValueNotification.Id,
                Data = newValueNotification.ToExNewValueNotification()
            });
        }

        return returnList;
    }

    /// <summary>
    ///     Device will Listen Daten für DcExNewValueNotifications sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExNewValueNotifications(long deviceId, long userId, List<DcStoreListItem<ExNewValueNotification>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var r = new DcListStoreResult
        {
            SecondId = secondId,
            StoreResult = new(),
            ElementsStored = new()
        };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

#pragma warning disable CS0219 // Variable is assigned but its value is never used
        var anyDelete = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

        foreach (var d in data)
        {
            // ReSharper disable once RedundantAssignment
            TableNewValueNotification? c = null!;
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    c = new TableNewValueNotification();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    d.Data.ToTableNewValueNotification(c);
                    db.TblNewValueNotifications.Add(c);
                    break;
                case EnumDcListElementState.Modified:
                    c = await db.TblNewValueNotifications
                        .FirstOrDefaultAsync(f => f.Id == d.Index)
                        .ConfigureAwait(false);

                    if (c is null)
                    {
                        continue;
                    }

                    r.ElementsStored++;
                    d.Data.ToTableNewValueNotification(c);
                    break;
                case EnumDcListElementState.Deleted:
                    c = await db.TblNewValueNotifications
                        .FirstOrDefaultAsync(f => f.Id == d.Index)
                        .ConfigureAwait(false);

                    if (c is null)
                    {
                        continue;
                    }

                    db.TblNewValueNotifications.Remove(c);
                    // ReSharper disable once RedundantAssignment
                    anyDelete = true;
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await db.SaveChangesAsync().ConfigureAwait(true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (d.State == EnumDcListElementState.New && c is not null)
            {
                tmp.NewIndex = c.Id;
                tmp.NewSortIndex = c.Id;
                r.NewIndex.Add(tmp);
            }
        }

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExNewValueNotifications
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExNewValueNotification>> SyncDcExNewValueNotifications(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion
}