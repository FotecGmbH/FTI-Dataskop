// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Plattform;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.GatewayService.Interfaces;
using BDA.Service.Com.Base.Helpers;
using Biss.Log.Producer;
using Biss.Serialize;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;
using WebExchange.Interfaces;

namespace BDA.GatewayService;

/// <summary>
///     <para>Anbindung der Gateways an den BDA.Service</para>
///     Klasse GatewayHub. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayHub : Hub
{
    private readonly IGatewayConnectedClientsManager _clientConnection;
    private readonly ITriggerAgent _triggerAgent;

    /// <summary>
    ///     Anbindung der Gateways an den BDA.Service
    /// </summary>
    /// <param name="clientConnection"></param>
    /// <param name="triggerAgent"></param>
    public GatewayHub(IGatewayConnectedClientsManager clientConnection, ITriggerAgent triggerAgent)
    {
        _clientConnection = clientConnection;
        _triggerAgent = triggerAgent;
    }

    /// <summary>
    ///     Called when a new connection is established with the hub.
    /// </summary>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous connect.</returns>
    public override Task OnConnectedAsync()
    {
        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(OnConnectedAsync)}): Connected ConnectionId: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    /// <summary>
    ///     Gateway hat wurde getrennt
    /// </summary>
    /// <param name="exception">Fehler</param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(OnDisconnectedAsync)}): ConnectionId: {Context.ConnectionId}");

        try
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var currentInfos = _clientConnection.GetGatewayInfos(Context.ConnectionId);
            var g = await db.TblGateways.Where(t => t.Id == currentInfos.GatewayId).Include(i => i.TblIotDevices).FirstOrDefaultAsync().ConfigureAwait(false);

            if (g != null)
            {
                g.DeviceCommon.State = EnumDeviceOnlineState.Offline;
                g.DeviceCommon.LastOfflineTime = DateTime.UtcNow;

                var updatedIotDevices = new List<long>();
                foreach (var ioTDevice in g.TblIotDevices)
                {
                    if (ioTDevice.DeviceCommon.State == EnumDeviceOnlineState.Unknown)
                    {
                        continue;
                    }

                    ioTDevice.DeviceCommon.State = EnumDeviceOnlineState.Offline;
                    ioTDevice.DeviceCommon.LastOfflineTime = DateTime.UtcNow;
                    updatedIotDevices.Add(ioTDevice.Id);
                }

                await db.SaveChangesAsync().ConfigureAwait(false);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _triggerAgent.ChangedGateway(EnumTriggerSources.GatewayService, g.Id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                foreach (var ioTDevice in updatedIotDevices)
                {
                    await _triggerAgent.IotDeviceStatusChanged(ioTDevice).ConfigureAwait(true);
                }
            }
            else
            {
                Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(OnDisconnectedAsync)}): gateway {currentInfos.GatewayId} not found in Db!");
            }
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(OnDisconnectedAsync)}): {e}");
        }

        try
        {
            _clientConnection.RemoveGateway(Context.ConnectionId);
        }
        catch
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(OnDisconnectedAsync)}): Disconnected with error: {Context.ConnectionId}");
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    /// <summary>
    ///     Ein Gerät hat sich angemeldet
    /// </summary>
    /// <param name="currentInfos"></param>
    /// <returns></returns>
    [HubMethodName(nameof(GatewayConstants.Reg))]
    public virtual async Task<ExGatwayRegisterResult> RegisterDevice(ExHubGatewayInfos currentInfos)
    {
        if (currentInfos == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(RegisterDevice)}): GatewayHub register gateway currentInfos are null!");
        }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var isNewGateway = false;
        var g = await db.TblGateways.FirstOrDefaultAsync(t => t.Id == currentInfos.GatewayId).ConfigureAwait(false);
        if (g == null && currentInfos.GatewayId > 0)
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(RegisterDevice)}): Gateway with Id {currentInfos.GatewayId} does not exist in database!");
            _clientConnection.AddOrUpdateGateway(Context.ConnectionId, currentInfos);
            return new ExGatwayRegisterResult {DbId = -1, Invalid = true};
        }

        if (g == null)
        {
            g = new TableGateway();
        }

        //Neuer Gateway
        if (currentInfos.GatewayId <= 0)
        {
            g = new TableGateway
            {
                TblCompany = await db.TblCompanies.FirstAsync(f => f.CompanyType == EnumCompanyTypes.NoCompany).ConfigureAwait(false),
                DeviceCommon =
                {
                    Secret = currentInfos.Secret,
                    ConfigversionService = 1
                },
                Position = currentInfos.Position!.ToDbPosition(),
                Information =
                {
                    Name = currentInfos.GatewayName,
                    Description = currentInfos.Description,
                    CreatedDate = DateTime.UtcNow
                }
            };

            isNewGateway = true;
        }

        if (!g.DeviceCommon.Secret.Equals(currentInfos.Secret, StringComparison.Ordinal))
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(RegisterDevice)}): Secret for gatway {currentInfos.GatewayId} is wrong!");
            return new ExGatwayRegisterResult
            {
                DbId = g.Id,
                Invalid = true
            };
        }

        g.DeviceCommon.ConfigversionDevice = currentInfos.ConfigVersion;
        g.DeviceCommon.FirmwareversionDevice = currentInfos.FirmwareVerion;
        g.DeviceCommon.LastOnlineTime = DateTime.UtcNow;
        g.DeviceCommon.State = EnumDeviceOnlineState.Online;

        if (isNewGateway)
        {
            db.TblGateways.Add(g);
        }

        try
        {
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(RegisterDevice)}): {e}");
            return new ExGatwayRegisterResult
            {
                DbId = -1,
                Invalid = true
            };
        }

        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(RegisterDevice)}): register device id {currentInfos.GatewayId}");
        currentInfos.GatewayId = g.Id;
        _clientConnection.AddOrUpdateGateway(Context.ConnectionId, currentInfos);

        if (isNewGateway)
        {
            await AddEmbeddedIotDeviceToGateway(db, g).ConfigureAwait(false);
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(500).ConfigureAwait(true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _triggerAgent.ChangedGateway(EnumTriggerSources.GatewayService, g.Id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        });

        return new ExGatwayRegisterResult
        {
            DbId = g.Id,
            Invalid = false
        };
    }

    /// <summary>
    ///     NewMesurementsFromGateway
    /// </summary>
    /// <param name="data">Neue Messwerte</param>
    /// <returns></returns>
    [HubMethodName(nameof(GatewayConstants.Send))]
    public async Task NewMesurementsFromGateway(ExValuesTransfer data)
    {
        if (data == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): register gateway currentInfos {nameof(ExValuesTransfer)} are null!");
        }

        if (data.Mesurements == null! || data.Mesurements.Count == 0)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): no Data to store from Gateway {data.GatewayId}");
            return;
        }

        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): getting {data.Mesurements.Count} from Gateway {data.GatewayId}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        List<(long, ExMeasurement)> newValuesFromMeasurementDefinitions = new List<(long, ExMeasurement)>();

        foreach (var value in data.Mesurements)
        {
            if (!value.HasValue())
            {
                continue;
            }

            Logging.Log.LogInfo($"[Value]: Gateway {data.GatewayId} value: {value.ToStringWithDetails()}");
            var exist = db.TblMeasurementDefinitions.Any(a => a.Id == value.Identifier);
            if (!exist)
            {
                Logging.Log.LogWarning($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): MeasurementDefinitions {value.Identifier} does not exist in DB.");
                continue;
            }

            var result = value.ToTableMeasurementResult();
            db.TblMeasurementResults.Add(result);
            //await db.SaveChangesAsync().ConfigureAwait(false);

            newValuesFromMeasurementDefinitions.Add((value.Identifier, new ExMeasurement
            {
                Id = result.Id,
                TimeStamp = result.TimeStamp,
                Value = CommonMethodsHelper.GetValueOfMeasurementResult(result),
                Location = new ExPosition
                {
                    Altitude = result.Location.Altitude,
                    Latitude = result.Location.Latitude,
                    Longitude = result.Location.Longitude,
                    Presision = result.Location.Precision,
                    Source = result.Location.Source,
                    TimeStamp = result.Location.TimeStamp
                }
            }));
        }


        await db.SaveChangesAsync().ConfigureAwait(false);

        _ = _triggerAgent.NewMeasurementsFromGateway(data.GatewayId, newValuesFromMeasurementDefinitions);
    }

    //TODO neue measurementdefinition anlegen
    /// <summary>
    ///     NewMesurementsFromGateway
    /// </summary>
    /// <param name="data">Neue Messwerte</param>
    /// <returns></returns>
    [HubMethodName(nameof(GatewayConstants.Create))]
    public async Task CreateMeasurementdefinitionFromGateway(ExValuesTransfer data)
    {
        if (data == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): register gateway currentInfos {nameof(ExValuesTransfer)} are null!");
        }

        if (data.Mesurements == null! || data.Mesurements.Count == 0)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): no Data to store from Gateway {data.GatewayId}");
            return;
        }

        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): getting {data.Mesurements.Count} from Gateway {data.GatewayId}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var newValuesFromMeasurementDefinitions = new List<long>();

        foreach (var value in data.Mesurements)
        {
            Logging.Log.LogInfo($"[Value]: Gateway {data.GatewayId} value: {value.ToStringWithDetails()}");
            var exist = db.TblMeasurementDefinitions.Any(a => a.Id == value.Identifier);
            if (!exist)
            {
                Logging.Log.LogWarning($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGateway)}): MeasurementDefinitions {value.Identifier} does not exist in DB.");
                continue;
            }

            db.TblMeasurementResults.Add(value.ToTableMeasurementResult());
            newValuesFromMeasurementDefinitions.Add(value.Identifier);
        }

        await db.SaveChangesAsync().ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _triggerAgent.NewMeasurementsFromGateway(data.GatewayId, newValuesFromMeasurementDefinitions.Distinct().ToList());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }


    /// <summary>
    ///     Online/Offline Status eines Iot Geräts hat sich geändert
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    [HubMethodName(nameof(GatewayConstants.Du))]
    public virtual async Task UpdateIotDeviceState(ExHubIotDeviceState state)
    {
        if (state == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(UpdateIotDeviceState)}): {nameof(state)} is null!");
        }

        Logging.Log.LogTrace($"[{nameof(GatewayHub)}]({nameof(UpdateIotDeviceState)}): Invoked with {state.State} from device Id {state.IotDeviceId}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        try
        {
            var iot = await db.TblIotDevices.FirstAsync(f => f.Id == state.IotDeviceId).ConfigureAwait(false);
            if (iot.DeviceCommon.State != state.State)
            {
                iot.DeviceCommon.State = state.State;
                switch (state.State)
                {
                    case EnumDeviceOnlineState.Unknown:
                        break;
                    case EnumDeviceOnlineState.Online:
                        iot.DeviceCommon.LastOnlineTime = state.ChangeDateTime;
                        break;
                    case EnumDeviceOnlineState.Offline:
                        iot.DeviceCommon.LastOfflineTime = state.ChangeDateTime;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"[{nameof(GatewayHub)}]({nameof(UpdateIotDeviceState)}): {nameof(state.State)} out of range");
                }
            }

            iot.DeviceCommon.ConfigversionDevice = state.ConfigVersion;
            iot.DeviceCommon.FirmwareversionDevice = state.FirmwareVersion;
            await db.SaveChangesAsync().ConfigureAwait(false);

            _ = Task.Run(async () => { await _triggerAgent.IotDeviceStatusChanged(iot.Id).ConfigureAwait(false); });
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(UpdateIotDeviceState)}): {e}");
        }
    }

    /// <summary>
    ///     Online/Offline Status eines Iot Geräts hat sich geändert
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    [HubMethodName(nameof(GatewayConstants.Downlink))]
    public virtual async Task SendDownlinkMessage(ExDownlinkMessageForDevice message)
    {
        if (message == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(SendDownlinkMessage)}): {nameof(message)} is null!");
        }

        Logging.Log.LogTrace($"[{nameof(GatewayHub)}]({nameof(SendDownlinkMessage)}): Invoked from device Id {message.IotDeviceId}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        try
        {
            var iot = await db.TblIotDevices.FirstAsync(f => f.Id == message.IotDeviceId).ConfigureAwait(false);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (iot == null)
            {
                Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(SendDownlinkMessage)}): Iot device {message.IotDeviceId} not found in Db!");
            }
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayHub)}]({nameof(SendDownlinkMessage)}): {e}");
        }
    }


    #region Eventuell noch finalisieren durch Mko?

    /// <summary>
    ///     NewMesurementsFromGateway
    /// </summary>
    /// <param name="data">Neue Messwerte</param>
    /// <returns></returns>
    public async Task NewMesurementsFromGatewayStream(IAsyncEnumerable<byte[]> data)
    {
        //ToDo Mko ... will nicht warum auch immer!

        var tmp = new List<byte>();
        await foreach (var item in data)
        {
            tmp.AddRange(item);
        }


        var stringData = Encoding.UTF32.GetString(tmp.ToArray());
        //await using var ms = new MemoryStream(Encoding.UTF32.GetBytes(stringData));

        var newData = BissDeserialize.FromJson<ExValuesTransfer>(stringData);

        if (newData == null!)
        {
            throw new NullReferenceException($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGatewayStream)}): {nameof(newData)} is null!");
        }

        if (newData.Mesurements == null! || newData.Mesurements.Count == 0)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGatewayStream)}): no Data to store from Gateway {newData.GatewayId}");
            return;
        }

        Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGatewayStream)}): getting {newData.Mesurements.Count} from Gateway {newData.GatewayId}");

        foreach (var value in newData.Mesurements)
        {
            Logging.Log.LogInfo($"[{nameof(GatewayHub)}]({nameof(NewMesurementsFromGatewayStream)}): value: {value.ToStringWithDetails()}");
        }

        //ToDo Mfa
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
    }

    #endregion


    /// <summary>
    ///     Automatisch ein Iot-Device im Gateway anlegen für Überwachung des Gateway selbst
    /// </summary>
    /// <returns></returns>
    private async Task AddEmbeddedIotDeviceToGateway(Db db, TableGateway gateway)
    {
        var iot = new TableIotDevice
        {
            DeviceCommon =
            {
                ConfigversionDevice = -1,
                ConfigversionService = 1,
                Secret = Guid.NewGuid().ToString(),
                State = EnumDeviceOnlineState.Unknown,
                FirmwareversionDevice = gateway.DeviceCommon.FirmwareversionDevice,
                FirmwareversionService = "1.0.0"
            },
            FallbackPosition = gateway.Position,
            Information =
            {
                Name = "States",
                Description = $"{gateway.Information.Name} State Iot device",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            },
            MeasurementInterval = 50,
            TransmissionInterval = 10,
            TransmissionType = EnumTransmission.Elapsedtime,
            Upstream = EnumIotDeviceUpstreamTypes.InGateway
        };


        gateway.TblIotDevices.Add(iot);
        var t = new GcPlattformDotNet();
        foreach (var m in t.BuildInExMeasurementDefinitions)
        {
            if (m == null!)
            {
                continue;
            }

            var c = new TableMeasurementDefinition();
            m.ToTableMeasurementDefinition(c);
            c.TblIoTDevice = iot;
            db.TblMeasurementDefinitions.Add(c);
        }

        //Auch noch zufällige Werte (nicht aber in Release)
        var float1 = new GcDownstreamVirtualFloat
        {
            Max = 100,
            Min = 0
        };
        // ReSharper disable once UnusedVariable
        var float2 = new GcDownstreamVirtualFloat
        {
            Max = 1000,
            Min = 100,
        };
        var m1 = float1.ToExMeasurementDefinition();
        m1.Information.Name = "Random FLOAT 1";

        var m2 = float1.ToExMeasurementDefinition();
        m2.Information.Name = "Random FLOAT 2";


        var c1 = new TableMeasurementDefinition();
        m1.ToTableMeasurementDefinition(c1);
        c1.TblIoTDevice = iot;
        db.TblMeasurementDefinitions.Add(c1);

        var c2 = new TableMeasurementDefinition();
        m2.ToTableMeasurementDefinition(c2);
        c2.TblIoTDevice = iot;
        db.TblMeasurementDefinitions.Add(c2);

        await db.SaveChangesAsync().ConfigureAwait(false);
        //await Db.CreateOrUpdateMeasurementDefinition(m1, iot, db).ConfigureAwait(false);
        //await Db.CreateOrUpdateMeasurementDefinition(m2, iot, db).ConfigureAwait(false);
    }
}