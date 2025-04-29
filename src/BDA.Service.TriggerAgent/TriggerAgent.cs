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
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.GatewayService.Interfaces;
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.Com.NewValueNotification;
using BDA.Service.Encryption;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebExchange;
using WebExchange.Interfaces;

namespace BDA.Service.TriggerAgent;

/// <summary>
///     <para>Service für Nachrichtenaustausch</para>
///     Klasse TriggerAgent. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class TriggerAgent : ITriggerAgent
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDcConnections _app;
    private readonly ServerRemoteCalls _dc;
    private readonly IGatewayConnectedClientsManager _gateway;
    private readonly INewValueNotificationService _newValueNotificationService;
    private readonly ISymmetricEncryption _symmetricEncryption;

    /// <summary>
    ///     Service für Nachrichtenaustausch
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="appConnectivity"></param>
    /// <param name="gateway"></param>
    /// <param name="newValueNotificationService"></param>
    /// <param name="symmetricEncryption"></param>
    /// <exception cref="ArgumentException"></exception>
    public TriggerAgent(IServiceScopeFactory serviceScopeFactory, IDcConnections appConnectivity, IGatewayConnectedClientsManager gateway, INewValueNotificationService newValueNotificationService, ISymmetricEncryption symmetricEncryption)
    {
        _app = appConnectivity;
        _gateway = gateway;
        _newValueNotificationService = newValueNotificationService ?? throw new ArgumentNullException(nameof(newValueNotificationService));
        _symmetricEncryption = symmetricEncryption;

        if (serviceScopeFactory == null!)
        {
            throw new ArgumentException($"[{nameof(TriggerAgent)}]({nameof(TriggerAgent)}): {nameof(serviceScopeFactory)} is NULL!");
        }

        var scope = serviceScopeFactory.CreateScope();
        var s = scope.ServiceProvider.GetService<IServerRemoteCalls>();
        if (scope == null! || s == null!)
        {
            throw new ArgumentException($"[{nameof(TriggerAgent)}]({nameof(TriggerAgent)}): {nameof(scope)} is NULL!");
        }

        _dc = (ServerRemoteCalls) s;
        _dc.SetClientConnection(_app);
    }

    /// <summary>
    ///     Gatewaydaten (inkl. Iot-Devices) haben sich geändert. Das Gateway hat aktualisierte Daten gesendet
    ///     Prüfen ob Daten (abhängig von der Configversion des Gateways und der Iot-Devices) an das Gateway gesendet werden
    ///     müssen
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <returns></returns>
    private async Task ChangedGatewayFromSaConnectivity(long gatewayId)
    {
        // ReSharper disable once UnusedVariable
        var dc = _dc;
        // ReSharper disable once UnusedVariable
        var gw = _gateway;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var g = await db.TblGateways.Where(t => t.Id == gatewayId).Include(i => i.TblIotDevices).ThenInclude(t => t.TblMeasurementDefinitions).FirstAsync().ConfigureAwait(false);

        if (g == null!)
        {
            Logging.Log.LogError($"[{nameof(TriggerAgent)}]({nameof(ChangeTrackerExtensions)}):  Gateway {gatewayId} not found.");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var update = g.DeviceCommon.ConfigversionDevice != g.DeviceCommon.ConfigversionService;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (!update)
        {
            foreach (var iot in g.TblIotDevices)
            {
                if (iot.DeviceCommon.ConfigversionService != g.DeviceCommon.ConfigversionDevice)
                {
                    update = true;
                    break;
                }
            }
        }

        //Gateway informieren
        if (update)
        {
            var data = g.ToExGwServiceGatewayConfig();

            foreach (var iotDevice in data.IotDevices)
            {
                _symmetricEncryption.DecryptAdditionalConfiguration(iotDevice);
            }

            var r = await _gateway.SendConfig(gatewayId, data).ConfigureAwait(false);
            if (r)
            {
                g.DeviceCommon.ConfigversionDevice = g.DeviceCommon.ConfigversionService;
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task ChangedGatewayFromGatewayService(long gatewayId)
    {
        await _dc.SendGatewayUpdate(gatewayId).ConfigureAwait(false);
    }

    #region Interface Implementations

    /// <summary>
    ///     Gatewaydaten wurden verändert
    /// </summary>
    /// <param name="source">Wer hat die Änderung gemacht?</param>
    /// <param name="gatewayId">Welches Gateway</param>
    public async Task ChangedGateway(EnumTriggerSources source, long gatewayId)
    {
        Logging.Log.LogInfo($"[{nameof(TriggerAgent)}]({nameof(ChangedGateway)}): ChangedGateway from {source} for gateway {gatewayId}");
        //ToDo Gwe: Mit Mko reden
        switch (source)
        {
            case EnumTriggerSources.ServiceAppConnectivity:
                await ChangedGatewayFromSaConnectivity(gatewayId).ConfigureAwait(false);
                break;
            case EnumTriggerSources.GatewayService:
                await ChangedGatewayFromGatewayService(gatewayId).ConfigureAwait(false);
                break;
        }

        await _dc.GatewayDataChanged(gatewayId).ConfigureAwait(false);
        await ChangedGatewayFromSaConnectivity(gatewayId).ConfigureAwait(false);
    }

    /// <summary>
    ///     Neue Daten vom Gateway wurden in DB gesichert
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <param name="measurementDefinitionIds"></param>
    public async Task NewMeasurementsFromGateway(long gatewayId, List<long> measurementDefinitionIds)
    {
        await _dc.MeasurementDefinitionDataChanged(measurementDefinitionIds, gatewayId).ConfigureAwait(false);
        //await _dc.SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(false);
    }

    /// <summary>
    ///     Neue Daten vom Gateway wurden in DB gesichert
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <param name="values"></param>
    public async Task NewMeasurementsFromGateway(long gatewayId, List<(long, ExMeasurement)> values)
    {
        await _dc.MeasurementDefinitionDataChanged(values.Select(i => i.Item1).Distinct().ToList(), gatewayId).ConfigureAwait(false);

        foreach (var (id, value) in values)
        {
            await _newValueNotificationService.SendNewValueNotificationAsync(id, value).ConfigureAwait(false);
        }

        //await _dc.SendReloadList(EnumReloadDcList.ExMeasurementDefinition).ConfigureAwait(false);
    }

    /// <summary>
    ///     Status eines Iot Gerätes (Online/Offline/Configversion/Firmwareverseion ...) haben sich geändert
    /// </summary>
    /// <param name="iotDeviceId"></param>
    public async Task IotDeviceStatusChanged(long iotDeviceId)
    {
        await _dc.IotDeviceDataChanged(iotDeviceId).ConfigureAwait(false);
    }

    /// <summary>
    ///     Downlink nachricht an ein iot device ueber das gateway senden
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <param name="iotDeviceId"></param>
    /// <param name="message"></param>
    public async Task SendDownlinkMessage(long gatewayId, long iotDeviceId, byte[] message)
    {
        var downlinkMessage = new ExDownlinkMessageForDevice(iotDeviceId, message);
        await _gateway.SendDownlinkMessage(gatewayId, downlinkMessage).ConfigureAwait(false);
    }

    #endregion
}