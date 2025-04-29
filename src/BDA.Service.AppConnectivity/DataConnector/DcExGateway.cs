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
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.TtnClient;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Errors.Model;
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
    ///     Device fordert Listen Daten für DcGateways
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
    public async Task<List<DcServerListItem<ExGateway>>> GetDcExGateways(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExGateway>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        //if (!db.TblMeasurementDefinitionTemplates.Any())
        //{

        //    var tempTemplate = new TableMeasurementDefinitionTemplate() {Information = new DbInformation() {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temperature", Description = ""}, ValueType = EnumValueTypes.Number, };
        //    var humidityTemplate = new TableMeasurementDefinitionTemplate() {Information = new DbInformation() {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temperature", Description = ""}, ValueType = EnumValueTypes.Number, };
        //    var parser = new TableDataconverter() {Displayname = "LHT65", Description = "Temperaturesensor", Templates = new List<TableMeasurementDefinitionTemplate>() {tempTemplate, humidityTemplate}, CodeSnippet = "public static ExValue[] Convert(byte[] input)\r\n{\r\n\tvar results = new ExValue[2];\r\n\r\n\t// Temperatur\r\n\tresults[0] = new ExValue() { Identifier = $0, ValueType = EnumValueTypes.Number};\r\n\r\n\t// Humidity\r\n\tresults[1] = new ExValue() { Identifier = $1, ValueType = EnumValueTypes.Number};\r\n\r\n \tresults[0].MeasurementNumber = 150;\tresults[1].MeasurementNumber = 10;\treturn results; \r\n"};
        //    db.TblDataconverters.Add(parser);
        //    await db.SaveChangesAsync();
        //}

        foreach (var gw in db.GetTableGatewayForUser(userId))
        {
            var d = new DcServerListItem<ExGateway>
            {
                Data = gw.ToExGateway(),
                SortIndex = gw.Id,
                Index = gw.Id,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcGateways sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExGateways(long deviceId, long userId, List<DcStoreListItem<ExGateway>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPosition)}]({nameof(data)}): {nameof(data)} is null");
        }

        var r = new DcListStoreResult
        {
            SecondId = secondId,
            StoreResult = new(),
            ElementsStored = new()
        };

        List<long> mod = new();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var anyDelete = false;

        foreach (var gw in data)
        {
            switch (gw.State)
            {
                case EnumDcListElementState.New:
                    throw new MethodNotAllowedException();
                case EnumDcListElementState.Modified:
                    var dbGateway = await db.TblGateways.FirstOrDefaultAsync(g => g.Id == gw.Index).ConfigureAwait(false);
                    gw.Data.ToTableGateway(dbGateway!);
                    r.ElementsStored++;
                    mod.Add(dbGateway!.Id);
                    break;
                case EnumDcListElementState.Deleted:
                    anyDelete = true;
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (gw.State == EnumDcListElementState.Deleted)
            {
                var g = await db.TblGateways.Where(g => g.Id == gw.Index).Include(i => i.TblIotDevices).ThenInclude(i => i.TblMeasurementDefinitions).ThenInclude(i => i.TblMeasurements).Include(i => i.TblIotDevices).ThenInclude(i => i.TblMeasurementDefinitions).ThenInclude(i => i.TblMeasurementDefinitionToProjectAssignments).FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (g == null)
                {
                    continue;
                }

                db.TblMeasurementResults.RemoveRange(g.TblIotDevices.SelectMany(s => s.TblMeasurementDefinitions).SelectMany(s => s.TblMeasurements));
                db.TblMeasurementDefinitionToProjectAssignments.RemoveRange(g.TblIotDevices.SelectMany(s => s.TblMeasurementDefinitions).SelectMany(s => s.TblMeasurementDefinitionToProjectAssignments));
                db.TblMeasurementDefinitions.RemoveRange(g.TblIotDevices.SelectMany(s => s.TblMeasurementDefinitions));
                foreach (var iotDevice in g.TblIotDevices)
                {
                    try
                    {
                        var config = GcTtnIotDevice.FromAdditionalConfig(iotDevice.AdditionalConfiguration);
                        if (config is not null)
                        {
                            var ttnclient = new TtnApiClient(config.GcTtnCompany);
                            _ = HandleDeletedTtnDevice(iotDevice, ttnclient);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExGateways)}): {e}");
                    }
                }

                db.TblIotDevices.RemoveRange(g.TblIotDevices);
                db.TblGateways.Remove(g);
            }
        }


        await db.SaveChangesAsync().ConfigureAwait(false);

        _ = Task.Run(async () =>
        {
            //Der App etwas Zeit geben
            await Task.Delay(300).ConfigureAwait(false);

            if (anyDelete)
            {
                await SendReloadList(EnumReloadDcList.ExProject).ConfigureAwait(true);
                await SendReloadList(EnumReloadDcList.ExGateways).ConfigureAwait(true);
                await SendReloadList(EnumReloadDcList.ExIotDevice).ConfigureAwait(true);
                await SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(true);
            }

            foreach (var i in mod)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                TriggerAgent.ChangedGateway(EnumTriggerSources.ServiceAppConnectivity, i);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        });
        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExGateways
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExGateway>> SyncDcExGateways(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion
}