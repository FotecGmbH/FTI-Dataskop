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
using WebExchange;

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExCompanies</para>
///     Klasse DcExCompanies. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcMeasurementDefinition
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
    public async Task<List<DcServerListItem<ExMeasurementDefinition>>> GetDcExMeasurementDefinition(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExMeasurementDefinition>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        foreach (var md in db.GetMeasurementDefinitionForUser(userId))
        {
            var d = new DcServerListItem<ExMeasurementDefinition>
            {
                Data = md.ToExMeasurementDefinition(),
                SortIndex = md.Id,
                Index = md.Id,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            var measurementCount = await db.TblMeasurementResults.CountAsync(c => c.TblMeasurementDefinitionId == md.Id).ConfigureAwait(false);
            d.Data.CurrentValue.ValueCounter = measurementCount;
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcMeasurementDefinition sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExMeasurementDefinition(long deviceId, long userId, List<DcStoreListItem<ExMeasurementDefinition>> data, long secondId)
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
        TableMeasurementDefinition c = null!;
        var anyDelete = false;
        var modifiedIotDevices = new List<long>();
        var modifiedMeasurementDefinitions = new List<long>();

        foreach (var d in data)
        {
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    c = new TableMeasurementDefinition();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Modified:
                    c = db.TblMeasurementDefinitions.First(f => f.Id == d.Index);
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Deleted:
                    c = db.TblMeasurementDefinitions.Where(f => f.Id == d.Index).Include(i => i.TblMeasurements).Include(i => i.TblMeasurementDefinitionToProjectAssignments).First();
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                db.TblMeasurementResults.RemoveRange(c.TblMeasurements);
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                db.TblMeasurementDefinitionToProjectAssignments.RemoveRange(c.TblMeasurementDefinitionToProjectAssignments);
                db.TblMeasurementDefinitions.Remove(c);
                anyDelete = true;
            }
            else
            {
                d.Data.ToTableMeasurementDefinition(c);
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblMeasurementDefinitions.Add(c);
            }

            modifiedIotDevices.Add(c.TblIotDeviceId);

            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = c.Id;
                tmp.NewSortIndex = c.Id;
                r.NewIndex.Add(tmp);
            }

            modifiedMeasurementDefinitions.Add(c.Id);
        }

        modifiedIotDevices = modifiedIotDevices.Distinct().ToList();
        long gatewayId = -1;
        foreach (var iotDevice in modifiedIotDevices)
        {
            var iot = db.TblIotDevices.First(f => f.Id == iotDevice);
            iot.DeviceCommon.ConfigversionService++;
            gatewayId = iot.TblGatewayId!.Value;
        }

        await db.SaveChangesAsync().ConfigureAwait(true);

        _ = Task.Run(async () =>
        {
            await Task.Delay(300).ConfigureAwait(true);

            if (anyDelete)
            {
                await SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(false);
            }
            else
            {
                await MeasurementDefinitionDataChanged(modifiedMeasurementDefinitions, gatewayId).ConfigureAwait(false);
            }

            foreach (var iotDevice in modifiedIotDevices)
            {
                await IotDeviceDataChanged(iotDevice).ConfigureAwait(false);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TriggerAgent.ChangedGateway(EnumTriggerSources.ServiceAppConnectivity, gatewayId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        });

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExMeasurementDefinition
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExMeasurementDefinition>> SyncDcExMeasurementDefinition(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion
}