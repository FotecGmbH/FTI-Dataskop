// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Dc.Core;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>Datenaustausch für Projekte einer Firma</para>
///     Klasse DcExCompanies. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcExProjects
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
    public async Task<List<DcServerListItem<ExProject>>> GetDcExProjects(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExProject>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        foreach (var p in db.GetTableProjectForUser(userId))
        {
            var d = new DcServerListItem<ExProject>
            {
                Data = p.ToExProject(),
                SortIndex = p.Id,
                Index = p.Id,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcExProjects sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExProjects(long deviceId, long userId, List<DcStoreListItem<ExProject>> data, long secondId)
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

        // ReSharper disable once RedundantAssignment
        TableProject p = null!;
#pragma warning disable CS0219
        var anyDelete = false;
#pragma warning restore CS0219

        foreach (var d in data)
        {
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    p = new TableProject();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Modified:
                    p = db.TblProjects.Where(f => f.Id == d.Index).Include(i => i.TblMeasurementDefinitionToProjectAssignments).First();
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Deleted:
                    p = db.TblProjects.Where(f => f.Id == d.Index).Include(i => i.TblMeasurementDefinitionToProjectAssignments).First();
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                db.TblMeasurementDefinitionToProjectAssignments.RemoveRange(p.TblMeasurementDefinitionToProjectAssignments);
                db.TblProjects.Remove(p);
                // ReSharper disable once RedundantAssignment
                anyDelete = true;
            }
            else
            {
                var itemsToRemove = d.Data.ToTableProject(p);
                if (itemsToRemove != null! && itemsToRemove.Count > 0)
                {
                    db.TblMeasurementDefinitionToProjectAssignments.RemoveRange(itemsToRemove);
                }
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblProjects.Add(p);
            }

            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = p.Id;
                tmp.NewSortIndex = p.Id;
                r.NewIndex.Add(tmp);
            }
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(300).ConfigureAwait(true);
            //if (anyDelete)
            //{
            //    await SendReloadList(EnumReloadDcList.ExProject).ConfigureAwait(false);
            //}
            //ToDo: Mko Geht besser!
            await SendReloadList(EnumReloadDcList.ExProject).ConfigureAwait(false);

            //foreach (var i in mod)
            //{
            //    TriggerAgent.ChangedGateway(EnumTriggerSources.ServiceAppConnectivity, i);
            //}
        });

        return r;
    }

    #endregion
}