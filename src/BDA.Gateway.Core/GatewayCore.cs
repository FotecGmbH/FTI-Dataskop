// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Gateway.Com.Base;
using BDA.Gateway.Com.Base.Model;
using BDA.Gateway.Com.Drei;
using BDA.Gateway.Com.IotInGateway;
using BDA.Gateway.Com.Microtronics;
using BDA.Gateway.Com.Opensense;
using BDA.Gateway.Com.Tcp;
using BDA.Gateway.Com.Ttn;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Core;

/// <summary>
///     <para>BDA Gateway Core</para>
///     Klasse GatewayCore. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayCore
{
    private readonly ConfigHandler<ExGwServiceGatewayConfig> _configHandler;

    /// <summary>
    ///     Falls User ein dc signal host angibt nimm diesen, ansonsten Appsettings.Current
    /// </summary>
    private readonly string _dcSignalHost;


    private readonly DirectoryInfo _localDataDirectory;
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary>
    /// BDA Gateway Core
    /// </summary>
    /// <param name="dcSignalHost"></param>
    /// <param name="gatewayFirstLaunch"></param>
    /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CS8618, CS9264
    public GatewayCore(string dcSignalHost, Func<string, GatewayCoreFirstLaunchInfos>? gatewayFirstLaunch = null)
#pragma warning restore CS8618, CS9264
    {
        if (dcSignalHost == null)
        {
            throw new ArgumentNullException(nameof(dcSignalHost));
        }

        _localDataDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "_Data"));
        if (!_localDataDirectory.Exists)
        {
            _localDataDirectory.Create();
        }

        var configFile = Path.Combine(_localDataDirectory.FullName, "gwconfig.json");
        _configHandler = new ConfigHandler<ExGwServiceGatewayConfig>(new FileInfo(configFile));

        Config = _configHandler.ReadConfig();
        if (_configHandler.IsNewConfig && gatewayFirstLaunch != null)
        {
            var tmp = gatewayFirstLaunch.Invoke(Config.Name);
            Config.Position = tmp.Position;
            Config.Name = tmp.Name;
            Config.Description = tmp.Description;
            _configHandler.StoreConfig(Config);
        }

        _dcSignalHost = dcSignalHost;
    }

    /// <summary>
    ///     BDA Gateway Core
    ///     Zweite Option Core aufzurufen.
    ///     Hierbei wird wenn noch nicht initialisiert nach einem pfad gefragt wo die gwconfig.json liegt oder
    ///     ansonsten mit Eingabe "advanced" wird wie beim oberen hantiert
    /// </summary>
#pragma warning disable CS8618, CS9264
    public GatewayCore(string dcSignalHost, Func<string>? firstLaunchConfiguration = null, Func<string, GatewayCoreFirstLaunchInfos>? gatewayFirstLaunch = null)
#pragma warning restore CS8618, CS9264
    {
        if (dcSignalHost == null)
        {
            throw new ArgumentNullException(nameof(dcSignalHost));
        }

        _localDataDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "_Data"));
        if (!_localDataDirectory.Exists)
        {
            _localDataDirectory.Create();
        }

        var configFile = Path.Combine(_localDataDirectory.FullName, "gwconfig.json");
        _configHandler = new ConfigHandler<ExGwServiceGatewayConfig>(new FileInfo(configFile));

        Config = _configHandler.ReadConfig();
        if (_configHandler.IsNewConfig && gatewayFirstLaunch != null)
        {
            var path = firstLaunchConfiguration!.Invoke();
            if (path == "advanced")
            {
                var tmp = gatewayFirstLaunch.Invoke(Config.Name);
                Config.Position = tmp.Position;
                Config.Name = tmp.Name;
                Config.Description = tmp.Description;
                _configHandler.StoreConfig(Config);
            }
            else
            {
                var config = File.ReadAllText(path);
                File.WriteAllText(_localDataDirectory.FullName + "/gwconfig.json", config);
                Config = BissDeserialize.FromJson<ExGwServiceGatewayConfig>(config);
            }
        }

        _dcSignalHost = dcSignalHost;
    }

    #region Properties

    /// <summary>
    /// EmbeddedIotDevices
    /// </summary>
    public List<GatewayComIotInGateway> EmbeddedIotDevices { get; } = new();
    /// <summary>
    /// TtnIotDevices
    /// </summary>
    public List<GatewayComTtn> TtnIotDevices { get; } = new();
    /// <summary>
    /// OpensenseIotDevices
    /// </summary>
    public List<GatewayComOpensense> OpensenseIotDevices { get; } = new();
    /// <summary>
    /// DreiDevices
    /// </summary>
    public List<GatewayComDrei> DreiDevices { get; } = new();
    /// <summary>
    /// MicrotronicsDevices
    /// </summary>
    public List<GatewayComMicrotronics> MicrotronicsDevices { get; } = new();
    /// <summary>
    /// TcpHandler
    /// </summary>
    public GatewayTcpHandler TcpHandler { get; } = new();

    /// <summary>
    ///     Aktuelle Konfiguration des Gateways
    /// </summary>
    public ExGwServiceGatewayConfig Config { get; set; }

    /// <summary>
    ///     Gatway ist am Server mit seiner Id und Secret angemeldet
    /// </summary>
    public bool GatwayRegisteredOnline { get; set; }

    #endregion

    /// <summary>
    ///     Gateway starten
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        GatewayComBase.GatewayConnectionChanged += GatewayComBaseOnConnectionChanged;
        GatewayComBase.NewConfigForGateway += GatewayComBaseOnNewConfig;
        GatewayComBase.SendDownlinkMessage += SendDownlinkMessage;

        try
        {
            GatewayComBase.InitializeHubConnection(_dcSignalHost, _localDataDirectory);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"{e}");
        }

        await ApplyConfig().ConfigureAwait(false);
        OnRefreshCliUi();
    }

    /// <summary>
    ///     Gateway beenden
    /// </summary>
    /// <returns></returns>
    public async Task Shutdown()
    {
        await GatewayComDrei.StopCommonHttpServer().ConfigureAwait(false);
        await GatewayComBase.CloseGatewayConnection().ConfigureAwait(true);
    }

    /// <summary>
    ///     Ereignis für CLI soll sich aktualisieren wenn möglich
    /// </summary>
    public event EventHandler RefreshCliUi;

    /// <summary>
    ///     Ereignis für neu starten
    /// </summary>
    public event EventHandler? Restart;

    /// <summary>
    ///     Alle virtuellen Iot Devices des Gateway stoppen
    /// </summary>
    public async void StopAllVirtualIotDevices()
    {
        foreach (var device in EmbeddedIotDevices)
        {
            await device.StopVirtualDevice().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Alle virtuellen Iot Devices des Gateway starten
    /// </summary>
    public void StartAllVirtualIotDevices()
    {
        foreach (var device in EmbeddedIotDevices)
        {
            device.StartVirtualDevice();
        }
    }

    /// <summary>
    ///     Methode von Ereignis für CLI soll sich aktualisieren wenn möglich
    /// </summary>
    public virtual void OnRefreshCliUi()
    {
        var handler = RefreshCliUi;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        handler?.Invoke(this, null!);
    }

    /// <summary>
    ///     Methode von Ereignis für neu starten
    /// </summary>
    protected virtual void OnRestart()
    {
        var handler = Restart;
        handler?.Invoke(this, null!);
    }

    /// <summary>
    ///     Send Downlink Nachricht
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="downlinkMessage"></param>
    private void SendDownlinkMessage(object? sender, ExDownlinkMessageForDevice downlinkMessage)
    {
        var curr = TtnIotDevices.FirstOrDefault(ttniot => ttniot.IotConfig.DbId == downlinkMessage.IotDeviceId);

        if (curr != null)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            curr.SendDownlinkMessage(downlinkMessage.Message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    /// <summary>
    ///     Neue Konfig vom Server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GatewayComBaseOnNewConfig(object? sender, NewConfigEventArgs e)
    {
        _configHandler.StoreConfig(e.GatewayConfig);
        Config = e.GatewayConfig;
        await ApplyConfig().ConfigureAwait(false);
        OnRefreshCliUi();
    }

    /// <summary>
    ///     Konfig anwenden
    /// </summary>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private async Task ApplyConfig()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        if (Config.IotDevices == null!)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayCore)}]({nameof(ApplyConfig)}): Gateway {Config.DbId} has no configured Iot Devices");
            _semaphore.Release();
            return;
        }

        foreach (var ttnDevice in TtnIotDevices)
        {
            ttnDevice.Stop();
        }

        foreach (var opensenseDevice in OpensenseIotDevices)
        {
            opensenseDevice.Stop();
        }

        foreach (var dreiDevice in DreiDevices)
        {
            dreiDevice.Stop();
        }

        //await GatewayComTtn.Reset().ConfigureAwait(false);

        foreach (var device in EmbeddedIotDevices)
        {
            await device.StopVirtualDevice().ConfigureAwait(false);
        }

        EmbeddedIotDevices.Clear();
        TtnIotDevices.Clear();
        OpensenseIotDevices.Clear();
        DreiDevices.Clear();
        MicrotronicsDevices.ForEach(md => md.Stop());
        MicrotronicsDevices.Clear();
        await TcpHandler.TcpIotDevicesClear().ConfigureAwait(true);


        try
        {
            foreach (var iotDevice in Config.IotDevices)
            {
                Logging.Log.LogInfo($"[{nameof(GatewayCore)}]({nameof(ApplyConfig)}): Device  Upstream:{iotDevice.UpStreamType}/ ID:{iotDevice.DbId}");
                switch (iotDevice.UpStreamType)
                {
                    case EnumIotDeviceUpstreamTypes.InGateway:
                        var iot = new GatewayComIotInGateway(Config.DbId, iotDevice);
                        EmbeddedIotDevices.Add(iot);
                        break;
                    case EnumIotDeviceUpstreamTypes.Serial:
                        throw new NotImplementedException();
                    case EnumIotDeviceUpstreamTypes.Tcp:
                        await ApplyTcpConfig(iotDevice).ConfigureAwait(true);
                        break;
                    case EnumIotDeviceUpstreamTypes.Ttn:
                        await ApplyTtnConfig(iotDevice).ConfigureAwait(true);
                        break;
                    case EnumIotDeviceUpstreamTypes.Drei:
                        await ApplyDreiConfig(iotDevice).ConfigureAwait(true);
                        break;
                    case EnumIotDeviceUpstreamTypes.OpenSense:
                        await ApplyOpensenseConfig(iotDevice).ConfigureAwait(true);
                        break;
                    case EnumIotDeviceUpstreamTypes.Microtronics:
                        await ApplyMicrotronicsConfig(iotDevice).ConfigureAwait(true);
                        break;
                    case EnumIotDeviceUpstreamTypes.Ble:
                        throw new NotImplementedException();
                    default:
                        Logging.Log.LogError($"[{nameof(GatewayCore)}]({nameof(ApplyConfig)}): Upstreamtype = {iotDevice.UpStreamType}");
                        throw new ArgumentOutOfRangeException();
                }
            }

            var devEuis = Config.IotDevices
                .Where(device => device.UpStreamType == EnumIotDeviceUpstreamTypes.Drei)
                .Select(device => new GcBaseConverter<GcTtnIotDevice>(device.AdditionalConfiguration).Base.DevEui).ToList();

            if (devEuis.Any())
            {
                await GatewayComDrei.UpdateDreiFlowConfiguration(devEuis).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logging.Log.LogError(e, $"[{nameof(GatewayCore)}]({nameof(ApplyConfig)}): Error while applying config.");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private async Task ApplyTcpConfig(ExGwServiceIotDeviceConfig iotDevice)
    {
        var newTcpCom = new GatewayComTcp(Config, iotDevice, TcpHandler.SendFunction);
        await TcpHandler.TcpIotDevicesAdd(newTcpCom).ConfigureAwait(true);

        if (!TcpHandler.Started)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => TcpHandler.StartAsync().ConfigureAwait(true));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    private async Task ApplyOpensenseConfig(ExGwServiceIotDeviceConfig iotDevice)
    {
        var newOpensenseCom = new GatewayComOpensense(Config, iotDevice);

        Logging.Log.LogInfo($"[{nameof(GatewayCore)}]({nameof(ApplyOpensenseConfig)}): Applying Config for Opensensedevice: {iotDevice.DbId}");
        OpensenseIotDevices.Add(newOpensenseCom);

        await newOpensenseCom.StartAsync().ConfigureAwait(true);
    }

    private async Task ApplyTtnConfig(ExGwServiceIotDeviceConfig iotDevice)
    {
        var newTtnCom = new GatewayComTtn(Config, iotDevice);

        TtnIotDevices.Add(newTtnCom);

        await newTtnCom.StartAsync().ConfigureAwait(true);
    }

    private async Task ApplyDreiConfig(ExGwServiceIotDeviceConfig iotDevice)
    {
        var newDreiCom = new GatewayComDrei(Config, iotDevice);

        DreiDevices.Add(newDreiCom);

        await newDreiCom.StartAsync().ConfigureAwait(true);
    }

    private async Task ApplyMicrotronicsConfig(ExGwServiceIotDeviceConfig iotDevice)
    {
        var newMicrotronicsCom = new GatewayComMicrotronics(Config, iotDevice);

        MicrotronicsDevices.Add(newMicrotronicsCom);

        await newMicrotronicsCom.StartAsync().ConfigureAwait(true);
    }

    /// <summary>
    ///     Verbindungsstatus zum Server hat sich geändert
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GatewayComBaseOnConnectionChanged(object? sender, EnumGatewayConnectionStates e)
    {
        Logging.Log.LogTrace($"[{nameof(GatewayCore)}]({nameof(GatewayComBaseOnConnectionChanged)}): connection changed to: {e}");

        if (GatwayRegisteredOnline && e != EnumGatewayConnectionStates.Connected)
        {
            GatwayRegisteredOnline = false;
        }

        if (!GatwayRegisteredOnline)
        {
            if (e == EnumGatewayConnectionStates.Connected)
            {
                var r = await GatewayComBase.RegisterGateway(Config.ToExHubGatewayInfos()).ConfigureAwait(false);
                if (r != null!)
                {
                    if (r.Invalid)
                    {
                        Logging.Log.LogWarning($"[{nameof(GatewayCore)}]({nameof(GatewayComBaseOnNewConfig)}): Gateway gelöscht. Starte neu ...");
                        _configHandler.Delete();
                        OnRestart();
                        return;
                    }

                    if (Config.DbId != r.DbId)
                    {
                        Config.UpdateFromExHubGatewayInfos(r);
                        _configHandler.StoreConfig(Config);
                    }

                    GatwayRegisteredOnline = true;
                }
            }
        }

        OnRefreshCliUi();
    }
}