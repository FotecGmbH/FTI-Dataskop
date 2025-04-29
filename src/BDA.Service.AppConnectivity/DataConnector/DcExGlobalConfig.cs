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
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.TtnClient;
using BDA.Service.Encryption;
using Biss.Dc.Core;
using Biss.Serialize;
using Database;
using Database.Converter;
using Database.Tables;

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExCompanies</para>
///     Klasse DcExCompanies. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcExGlobalConfig
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
    public async Task<List<DcServerListItem<ExGlobalConfig>>> GetDcExGlobalConfig(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExGlobalConfig>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        //Gastbenutzer
        if (userId <= 0)
        {
            return result;
        }

        var config = db.GetTableCompanyGlobalConfigForUser(userId);
        foreach (var c in config)
        {
            var data = c.ToExGlobalConfig();

            _symmetricEncryption.DecryptAdditionalConfiguration(data);
            result.Add(new DcServerListItem<ExGlobalConfig>
                {
                    Data = data,
                    Index = c.Id,
                    SortIndex = c.Id
                }
            );
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcExGlobalConfig sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExGlobalConfig(long deviceId, long userId, List<DcStoreListItem<ExGlobalConfig>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var iotDevicesChanged = false;
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
        TableCompanyGlobalConfig c = null!;


        foreach (var d in data)
        {
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    c = new TableCompanyGlobalConfig();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Modified:
                    c = db.TblCompanyGlobalConfigs.First(f => f.Id == d.Index);
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Deleted:
                    c = db.TblCompanyGlobalConfigs.First(f => f.Id == d.Index);
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                db.TblCompanyGlobalConfigs.Remove(c);
            }
            else
            {
                d.Data.ToTableCompanyGlobalConfig(c);
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblCompanyGlobalConfigs.Add(c);
                if (d.Data.ConfigType == EnumGlobalConfigTypes.Ttn)
                {
                    var config = GcTtn.FromAdditionalConfig(d.Data.AdditionalConfiguration);
                    if (config is not null)
                    {
                        var ttnclient = new TtnApiClient(config);
                        if (!(await ttnclient.ApplicationExists(config.Applicationid).ConfigureAwait(false)))
                        {
                            await ttnclient.CreateApplication(config.Applicationid, config.Userid).ConfigureAwait(false);
                        }
                    }
                }

                if (d.Data.ConfigType == EnumGlobalConfigTypes.Drei)
                {
                    // ReSharper disable once UnusedVariable
                    var config = GcDrei.FromAdditionalConfig(d.Data.AdditionalConfiguration);
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

            //Aktulisieren der verknüpften Einstellungen
            if (d.State == EnumDcListElementState.Modified)
            {
                var iotDevices = db.TblIotDevices.Where(w => w.TblCompanyGlobalConfigId == d.Index);
                foreach (var iot in iotDevices)
                {
                    _symmetricEncryption.DecryptAdditionalConfiguration(iot);
                    if (iot.Upstream == EnumIotDeviceUpstreamTypes.Ttn && d.Data.ConfigType == EnumGlobalConfigTypes.Ttn)
                    {
                        var ttnDevice = BissDeserialize.FromJson<GcTtnIotDevice>(iot.AdditionalConfiguration);
                        var ttnGc = BissDeserialize.FromJson<GcTtn>(d.Data.AdditionalConfiguration);
                        ttnDevice.GcTtnCompany = ttnGc;
                        iot.AdditionalConfiguration = ttnDevice.ToJson();
                        iotDevicesChanged = true;
                    }
                    else if (iot.Upstream == EnumIotDeviceUpstreamTypes.Drei && d.Data.ConfigType == EnumGlobalConfigTypes.Drei)
                    {
                        iot.AdditionalConfiguration = d.Data.AdditionalConfiguration;
                        iotDevicesChanged = true;
                    }

                    _symmetricEncryption.EncryptAdditionalConfiguration(iot);
                }

                if (d.Data.ConfigType == EnumGlobalConfigTypes.Ttn)
                {
                    var config = GcTtn.FromAdditionalConfig(d.Data.AdditionalConfiguration);
                    if (config is not null)
                    {
                        var ttnclient = new TtnApiClient(config);
                        if (!(await ttnclient.ApplicationExists(config.Applicationid).ConfigureAwait(false)))
                        {
                            await ttnclient.CreateApplication(config.Applicationid, config.Userid).ConfigureAwait(false);
                        }
                    }
                }

                if (d.Data.ConfigType == EnumGlobalConfigTypes.Drei)
                {
                    // ReSharper disable once UnusedVariable
                    var config = GcDrei.FromAdditionalConfig(d.Data.AdditionalConfiguration);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);
            }
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000).ConfigureAwait(true);
            await SendReloadList(EnumReloadDcList.ExGlobalConfig).ConfigureAwait(false);
            if (iotDevicesChanged)
            {
                await SendReloadList(EnumReloadDcList.ExIotDevice).ConfigureAwait(false);
            }
        });

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExGlobalConfig
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExGlobalConfig>> SyncDcExGlobalConfig(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion
}