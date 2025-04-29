// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Reflection;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Gateway.Com.Base;
using BDA.IotDevice.Com.Upstream.IotInGateway;
using BDA.IotDevice.Core;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Com.IotInGateway;

/// <summary>
///     <para>Kommunikation wenn das Iot Gerät direkt im Gateway gehostet wird</para>
///     Klasse GatewayComIotInGateway. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayComIotInGateway : GatewayComBase
{
    private readonly string _deviceVersion;
    private IotDeviceCore? _iotDevice;
    private UpstreamIotInGateway? _iotDeviceUpstream;

    /// <summary>
    ///     Kommunikation wenn das Iot Gerät direkt im Gateway gehostet wird
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <param name="iotConfig"></param>
    /// <exception cref="Exception"></exception>
    public GatewayComIotInGateway(long gatewayId, ExGwServiceIotDeviceConfig iotConfig) : base(gatewayId, iotConfig)
    {
        if (IotConfig.UpStreamType != EnumIotDeviceUpstreamTypes.InGateway)
        {
            throw new Exception($"[{nameof(GatewayComIotInGateway)}]({nameof(GatewayComIotInGateway)}): Wrong upload type in Iot Devices Configuration!: {IotConfig.UpStreamType}");
        }

        var v = Assembly.GetExecutingAssembly().GetName().Version;
        _deviceVersion = v == null ? "?" : v.ToString();
        StartNewIotDevice();
    }

    /// <summary>
    ///     Virtuelles Gerät Stoppen
    /// </summary>
    /// <returns></returns>
    public async Task StopVirtualDevice()
    {
        if (IsConnected)
        {
            if (_iotDeviceUpstream != null)
            {
                _iotDeviceUpstream.NewValues += UpstreamOnNewValues;
            }

            if (_iotDevice != null)
            {
                await _iotDevice.StopMainLoop().ConfigureAwait(true);
            }

            await UpdateIotDeviceState(EnumDeviceOnlineState.Offline, _deviceVersion).ConfigureAwait(false);
            IsConnected = false;
        }
    }

    /// <summary>
    ///     Virtuelles Gerät Starten
    /// </summary>
    public void StartVirtualDevice()
    {
        if (!IsConnected)
        {
            StartNewIotDevice();
        }
    }

    /// <summary>
    ///     Neue Konfig an ein bestimmtes Iot - Gerät übertragen
    /// </summary>
    /// <param name="iotDeviceConfig"></param>
    /// <param name="resend"></param>
    /// <returns></returns>
    protected override async Task<bool> TransferConfig(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resend = false)
    {
        IotConfig = iotDeviceConfig;
        if (_iotDevice != null)
        {
            await _iotDevice.StopMainLoop().ConfigureAwait(false);
        }

        StartNewIotDevice();
        return true;
    }

    private async void StartNewIotDevice()
    {
        _iotDevice = new IotDeviceCore(IotConfig);
        _iotDeviceUpstream = (UpstreamIotInGateway) _iotDevice.Upstream;
        _iotDeviceUpstream.NewValues += UpstreamOnNewValues;
        await _iotDevice.StartMainLoop().ConfigureAwait(false);
        await UpdateIotDeviceState(EnumDeviceOnlineState.Online, _deviceVersion).ConfigureAwait(false);
        IsConnected = true;
    }


    private async void UpstreamOnNewValues(object? sender, NewValuesArgs e)
    {
        if (e == null! || e.Values == null!)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayComIotInGateway)}]({nameof(UpstreamOnNewValues)}): e or e.Values are NULL!");
            return;
        }

        await NewValues(e.Values).ConfigureAwait(false);
    }
}