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
using BDA.Common.Exchange.Configs.Downstreams.Custom;
using BDA.Common.Exchange.Configs.Downstreams.OpenSense;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Drei;
using BDA.Common.Exchange.Configs.Upstream.Microtronics;
using BDA.Common.Exchange.Configs.Upstream.Opensense;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.MicrotronicsClient;
using BDA.Common.OpensenseClient;
using BDA.Common.TtnClient;
using BDA.Service.Encryption;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using ConnectivityHost.Helper;
using Database;
using Database.Common;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebExchange;
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExCompanies</para>
///     Klasse DcExCompanies. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region LoRa

    private async Task HandleNewLoRaDevice(TableIotDevice device, Db? context = null)
    {
        var dispose = context is null;
        context ??= new Db();

        var devAddrDefinition = device.TblMeasurementDefinitions.FirstOrDefault(def => def.Information.Name == "DevAddr");
        if (devAddrDefinition is null)
        {
            var definition = new TableMeasurementDefinition
            {
                DownstreamType = EnumIotDeviceDownstreamTypes.None,
                ValueType = EnumValueTypes.Text,

                Information = new DbInformation
                {
                    CreatedDate = DateTime.UtcNow,
                    Name = "DevAddr",
                    Description = "TTN-DeviceAddress",
                    UpdatedDate = DateTime.UtcNow
                }
            };

            device.TblMeasurementDefinitions.Add(definition);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        if (dispose)
        {
            await context.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Thirdparty

    private async Task HandleNewThirdpartyDevice(TableIotDevice device, Db? context = null)
    {
        var owncontext = context is null;
        if (owncontext)
        {
            context = new Db();
        }

        var converter = device.TblDataconverter;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var templates = device.TblDataconverter.Templates.ToArray();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        List<GcDownstreamCustom> additionalCustomConfigs = null!; // Option 1: Fertiger Sensor mit vordefiniertem Customcode, Option 2: Sensor mit eigenen CustomOpcodes Downstreams

        try
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            additionalCustomConfigs = BissDeserialize.FromJson<List<GcDownstreamCustom>>(converter.CodeSnippet);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        catch (Exception)
        {
            // ignored
        }

        if (additionalCustomConfigs is null)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            for (var i = 0; i < converter.Templates.Count; i++)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            {
                var definition = new TableMeasurementDefinition
                {
                    DownstreamType = EnumIotDeviceDownstreamTypes.Custom,
                    ValueType = templates[i].ValueType,
                    TblIotDeviceId = device.Id,
                    Information = new DbInformation
                    {
                        CreatedDate = DateTime.UtcNow,
                        Name = templates[i].Information.Name,
                        Description = templates[i].Information.Description,
                        UpdatedDate = DateTime.UtcNow
                    }
                };
                device.TblMeasurementDefinitions.Add(definition);
            }

            await context!.SaveChangesAsync().ConfigureAwait(false);

            var definitions = device.TblMeasurementDefinitions.Where(def => def.DownstreamType != EnumIotDeviceDownstreamTypes.None).ToArray();

            var usercode = UserCodeHelper.GetUsercode(definitions, device.TblDataconverter.CodeSnippet);

            if (device.Upstream == EnumIotDeviceUpstreamTypes.Ttn)
            {
                var config = new GcBaseConverter<GcTtnIotDevice>(device.AdditionalConfiguration).Base;
                config.UserCode = usercode;
                device.AdditionalConfiguration = JsonConvert.SerializeObject(config);
            }
        }
        else
        {
            for (var i = 0; i < converter!.Templates.Count; i++)
            {
                var definition = new TableMeasurementDefinition
                {
                    DownstreamType = EnumIotDeviceDownstreamTypes.Custom,
                    ValueType = templates[i].ValueType,
                    TblIotDeviceId = device.Id,
                    Information = new DbInformation
                    {
                        CreatedDate = DateTime.UtcNow,
                        Name = templates[i].Information.Name,
                        Description = templates[i].Information.Description,
                        UpdatedDate = DateTime.UtcNow
                    },
                    AdditionalConfiguration = additionalCustomConfigs.ElementAt(i).ToJson()
                };
                device.TblMeasurementDefinitions.Add(definition);
            }

            await context!.SaveChangesAsync().ConfigureAwait(false);

            // ReSharper disable once UnusedVariable
            var definitions = device.TblMeasurementDefinitions.Where(def => def.DownstreamType != EnumIotDeviceDownstreamTypes.None).ToArray();
        }


        await context.SaveChangesAsync().ConfigureAwait(false);

        if (owncontext)
        {
            await context.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcIotDevices
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
    public async Task<List<DcServerListItem<ExIotDevice>>> GetDcExIotDevices(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExIotDevice>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        foreach (var iot in db.GetTableIotDeviceForUser(userId))
        {
            var data = iot.ToExIoTDevice();

            _symmetricEncryption.DecryptAdditionalConfiguration(data);

            var d = new DcServerListItem<ExIotDevice>
            {
                Data = data,
                SortIndex = iot.Id,
                Index = iot.Id,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcIotDevices sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExIotDevices(long deviceId, long userId, List<DcStoreListItem<ExIotDevice>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException(nameof(data));
        }

        long gateway = -1;
        var updateMeasurementDefinitionsOpensense = false;
        // ReSharper disable once RedundantAssignment
        var updateMeasurementDefinitionsMicrotronics = false;
        TtnApiClient? ttnclient = null;

        var r = new DcListStoreResult
        {
            SecondId = secondId,
            StoreResult = new(),
            ElementsStored = new()
        };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        TableIotDevice c = null!;
        var anyDelete = false;
        var downloadOpensense = false;
        var downloadMicrotronics = false;


        foreach (var d in data)
        {
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    c = new TableIotDevice();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Modified:
                    c = db.TblIotDevices.First(f => f.Id == d.Index);
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Deleted:
                    c = db.TblIotDevices.Where(f => f.Id == d.Index).Include(i => i.TblMeasurementDefinitions).ThenInclude(i => i.TblMeasurements).Include(i => i.TblMeasurementDefinitions).ThenInclude(i => i.TblMeasurementDefinitionToProjectAssignments).First();
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                _symmetricEncryption.DecryptAdditionalConfiguration(c);
                gateway = d.Data.GatewayId ?? -1;
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                db.TblMeasurementResults.RemoveRange(c.TblMeasurementDefinitions!.SelectMany(s => s.TblMeasurements));
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                db.TblMeasurementDefinitionToProjectAssignments.RemoveRange(c.TblMeasurementDefinitions!.SelectMany(s => s.TblMeasurementDefinitionToProjectAssignments));
                db.TblMeasurementDefinitions.RemoveRange(c.TblMeasurementDefinitions);
                db.TblIotDevices.Remove(c);
                anyDelete = true;
                if (c.Upstream == EnumIotDeviceUpstreamTypes.Ttn)
                {
                    var config = GcTtnIotDevice.FromAdditionalConfig(c.AdditionalConfiguration);
                    if (config is not null)
                    {
                        ttnclient ??= new TtnApiClient(config.GcTtnCompany);
                    }

                    if (ttnclient is not null)
                    {
                        _ = HandleDeletedTtnDevice(c, ttnclient);
                    }
                }
            }
            else
            {
                d.Data.ToTableIotDevice(c);
            }

            if (d.State == EnumDcListElementState.New)
            {
                updateMeasurementDefinitionsOpensense = d.Data.Plattform == EnumIotDevicePlattforms.OpenSense;
                downloadOpensense = updateMeasurementDefinitionsOpensense && new GcBaseConverter<GcOpenSenseIotDevice>(c.AdditionalConfiguration).Base.DownloadDataSince != null;

                updateMeasurementDefinitionsMicrotronics = d.Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics;
                downloadMicrotronics = updateMeasurementDefinitionsMicrotronics && new GcBaseConverter<GcMicrotronicsIotDevice>(c.AdditionalConfiguration).Base.DownloadDataSince != null;

                gateway = c.TblGatewayId ?? -1;
                db.TblIotDevices.Add(c);

                if (c.Upstream is EnumIotDeviceUpstreamTypes.Drei or EnumIotDeviceUpstreamTypes.Ttn)
                {
                    await HandleNewLoRaDevice(c, db).ConfigureAwait(false);
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Ttn)
                {
                    var config = GcTtnIotDevice.FromAdditionalConfig(c.AdditionalConfiguration);
                    if (config is not null)
                    {
                        ttnclient ??= new TtnApiClient(config.GcTtnCompany);
                    }

                    if (ttnclient is not null)
                    {
                        await HandleNewTtnDevice(c, ttnclient).ConfigureAwait(false);
                    }
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Drei)
                {
                    // ReSharper disable once UnusedVariable
                    var config = GcDreiIotDevice.FromAdditionalConfig(c.AdditionalConfiguration);
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Tcp) // because for tcp i need to set secret for myself
                {
                    c.DeviceCommon.Secret = d.Data.DeviceCommon.Secret;
                }
            }

            if (d.State == EnumDcListElementState.Modified)
            {
                updateMeasurementDefinitionsOpensense = d.Data.Plattform == EnumIotDevicePlattforms.OpenSense;
                gateway = c.TblGatewayId ?? -1;

                if (c.Upstream is EnumIotDeviceUpstreamTypes.Drei or EnumIotDeviceUpstreamTypes.Ttn)
                {
                    c.TblMeasurementDefinitions = db.TblMeasurementDefinitions.Where(md => md.TblIotDeviceId == c.Id).ToList();
                    await HandleNewLoRaDevice(c, db).ConfigureAwait(false);
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Ttn)
                {
                    var config = GcTtnIotDevice.FromAdditionalConfig(c.AdditionalConfiguration);
                    if (config is not null)
                    {
                        ttnclient ??= new TtnApiClient(config.GcTtnCompany);
                    }

                    if (ttnclient is not null)
                    {
                        await HandleModifiedTtnDevice(c, ttnclient).ConfigureAwait(false);
                    }
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Drei)
                {
                    // ReSharper disable once UnusedVariable
                    var config = GcDreiIotDevice.FromAdditionalConfig(c.AdditionalConfiguration);
                }

                if (c.Upstream == EnumIotDeviceUpstreamTypes.Tcp) // because for tcp i need to set secret for myself
                {
                    c.DeviceCommon.Secret = d.Data.DeviceCommon.Secret;
                }
            }

            //vor dem speichern AdditionalConiguration verschlüsseln
            _symmetricEncryption.EncryptAdditionalConfiguration(c);

            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = c.Id;
                tmp.NewSortIndex = c.Id;
                r.NewIndex.Add(tmp);
            }


            // Handling Dataconverter
            if (d.State == EnumDcListElementState.New)
            {
                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                var device = await db.TblIotDevices.Where(dev => dev.TblDataconverter != null).Include(dev => dev.TblDataconverter!).ThenInclude(conv => conv.Templates).FirstOrDefaultAsync(dev => dev.Id == c!.Id).ConfigureAwait(false);
                if (device is {Plattform: EnumIotDevicePlattforms.Prebuilt, TblDataconverter: { }})
                {
                    updateMeasurementDefinitionsOpensense = true;
                    _symmetricEncryption.DecryptAdditionalConfiguration(device);
                    await HandleNewThirdpartyDevice(device, db).ConfigureAwait(false);
                    _symmetricEncryption.EncryptAdditionalConfiguration(device);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }


        _ = Task.Run(async () =>
        {
            await using var context = new Db();
            await Task.Delay(300).ConfigureAwait(true);

            if (downloadOpensense)
            {
                var oclient = new OpensenseClient();
                _symmetricEncryption.DecryptAdditionalConfiguration(c);
                var config = new GcBaseConverter<GcOpenSenseIotDevice>(c.AdditionalConfiguration).Base;
                if (config.DownloadDataSince != null)
                {
                    var counter = 0;

                    var sensorIdCache = new Dictionary<string, TableMeasurementDefinition>();


#pragma warning disable CS8604 // Possible null reference argument.
                    // ReSharper disable once RedundantSuppressNullableWarningExpression
                    await foreach (var measurement in oclient.GetAllValuesInTimeframe(config!.BoxId, config.DownloadDataSince.Value))
#pragma warning restore CS8604 // Possible null reference argument.
                    {
                        // ReSharper disable once RedundantAssignment
                        TableMeasurementDefinition? measurementDefinition = null;
                        if (!sensorIdCache.TryGetValue(measurement.OpensenseId, out measurementDefinition))
                        {
                            var def = c.TblMeasurementDefinitions.FirstOrDefault(definition => new GcBaseConverter<GcDownstreamOpenSense>(definition.AdditionalConfiguration).Base.SensorID == measurement.OpensenseId);
                            if (def != null)
                            {
                                // ReSharper disable once ConstantConditionalAccessQualifier
                                measurementDefinition = await context.TblMeasurementDefinitions.FindAsync(def?.Id);
                                if (measurementDefinition is not null)
                                {
                                    sensorIdCache.TryAdd(measurement.OpensenseId, measurementDefinition);
                                }
                            }
                        }

                        if (measurementDefinition is null)
                        {
                            continue;
                        }

                        var value = new ExValue
                        {
                            Identifier = measurementDefinition.Id,
                            TimeStamp = measurement.CreatedAt,
                            Position = new ExPosition
                            {
                                Altitude = measurement.Altitude,
                                Longitude = measurement.Longitude,
                                Latitude = measurement.Latitude,
                                Source = EnumPositionSource.Modul,
                                TimeStamp = measurement.CreatedAt
                            },
                            MeasurementText = measurement.Value,
                            ValueType = EnumValueTypes.Text
                        };


                        // ReSharper disable once ConstantConditionalAccessQualifier
                        measurementDefinition?.TblMeasurements.Add(value.ToTableMeasurementResult());

                        counter++;
                        if (counter > 1000)
                        {
                            //vor dem speichern AdditionalConiguration verschlüsseln
                            _symmetricEncryption.EncryptAdditionalConfiguration(c);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                            counter = 0;
                        }
                    }

                    config.DownloadDataSince = null;
                    c.AdditionalConfiguration = config.ToJson();

                    //vor dem speichern AdditionalConiguration verschlüsseln
                    _symmetricEncryption.EncryptAdditionalConfiguration(c);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            if (downloadMicrotronics)
            {
                var counter = 0;
                _symmetricEncryption.DecryptAdditionalConfiguration(c);
                var config = new GcBaseConverter<GcMicrotronicsIotDevice>(c.AdditionalConfiguration).Base;
                var mclient = new MicrotronicsApiClient(config.GcMicrotronicsCompany.BackendDomain, config.GcMicrotronicsCompany.UserName, config.GcMicrotronicsCompany.Password);

                if (config.DownloadDataSince != null)
                {
                    var channels = c.TblMeasurementDefinitions;

                    var vals = await mclient.GetValuesTillNow(config.CustomerId, config.SiteId, config.HistDataConfiguration, channels.Select(m => m.Information.Name).ToList(), (DateTime) config.DownloadDataSince).ConfigureAwait(true);

                    foreach (var channel in channels)
                    {
                        // ReSharper disable once RedundantAssignment
                        TableMeasurementDefinition? measurementDefinition = null;
                        measurementDefinition = await context.TblMeasurementDefinitions.FindAsync(channel.Id);
                        if (measurementDefinition is null)
                        {
                            continue;
                        }

                        foreach (var v in vals[channel.Information.Name])
                        {
                            try
                            {
                                var value = new ExValue
                                {
                                    Position = new ExPosition(),
                                    Identifier = measurementDefinition.Id,
                                    TimeStamp = v.Item1,
                                    MeasurementText = v.Item2,
                                    ValueType = EnumValueTypes.Text
                                };

                                measurementDefinition.TblMeasurements.Add(value.ToTableMeasurementResult());
                            }
                            catch (Exception e)
                            {
                                Logging.Log.LogError($"[{nameof(ServerRemoteCalls)}][{nameof(StoreDcExIotDevices)}] Problems saving the values for one channel from a microtronics iot device: {e}");
                            }

                            counter++;
                            if (counter > 100)
                            {
                                //vor dem speichern AdditionalConiguration verschlüsseln
                                _symmetricEncryption.EncryptAdditionalConfiguration(c);
                                await context.SaveChangesAsync().ConfigureAwait(false);
                                counter = 0;
                            }
                        }
                    }

                    config.DownloadDataSince = null;
                    c.AdditionalConfiguration = config.ToJson();

                    //vor dem speichern AdditionalConiguration verschlüsseln
                    _symmetricEncryption.EncryptAdditionalConfiguration(c);
                    await context.SaveChangesAsync().ConfigureAwait(false);

                    //var uu = context.TblIotDevices.Include(i => i.TblMeasurementDefinitions).ThenInclude(m => m.TblMeasurements).Where(u => u.Id == c.Id).ToList();
                }
            }

            if (anyDelete)
            {
                await SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(false);
            }

            await SendReloadList(EnumReloadDcList.ExIotDevice).ConfigureAwait(false);

            if (updateMeasurementDefinitionsOpensense)
            {
                await SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(false);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (gateway != -1 && TriggerAgent != null)
            {
                await TriggerAgent.ChangedGateway(EnumTriggerSources.ServiceAppConnectivity, gateway).ConfigureAwait(false);
            }
        });

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExIotDevices
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExIotDevice>> SyncDcExIotDevices(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region TTN

    private async Task HandleNewTtnDevice(TableIotDevice device, TtnApiClient client)
    {
        var config = GcTtnIotDevice.FromAdditionalConfig(device.AdditionalConfiguration);
        if (config is null || !config.CreateDeviceOnTtn)
        {
            return;
        }

        await client.ChangeAccount(config.GcTtnCompany).ConfigureAwait(false);

        var generatedConfig = await client.AddDevice(config).ConfigureAwait(false);
        var newConfig = client.CopyGeneratedProperties(config, generatedConfig);
        device.AdditionalConfiguration = newConfig.ToJson();
    }

    private async Task HandleDeletedTtnDevice(TableIotDevice device, TtnApiClient client)
    {
        var config = GcTtnIotDevice.FromAdditionalConfig(device.AdditionalConfiguration);
        if (config is null || !config.CreateDeviceOnTtn)
        {
            return;
        }

        await client.ChangeAccount(config.GcTtnCompany).ConfigureAwait(false);

        await client.DeleteDevice(config.GcTtnCompany.Applicationid, config.DeviceId).ConfigureAwait(false);
    }

    private async Task HandleModifiedTtnDevice(TableIotDevice device, TtnApiClient client)
    {
        await HandleDeletedTtnDevice(device, client).ConfigureAwait(false);
        await HandleNewTtnDevice(device, client).ConfigureAwait(false);
    }

    #endregion
}