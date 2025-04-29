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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Downstream.Base;
using BDA.IotDevice.Com.Upstream.Base;
using BDA.IotDevice.Com.Upstream.IotInGateway;
using BDA.IotDevice.Com.Upstream.Tcp;
using Biss.Log.Producer;
using Biss.Serialize;

namespace BDA.IotDevice.Core;

/// <summary>
///     <para>Die Statemachine die den Mainloop beinhaltet.</para>
///     Klasse StateMachine. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class IotDeviceCore
{
    /// <summary>
    ///     buffer für zwischenspeichern der daten die nach sendintervall gesesndet werden
    /// </summary>
    private readonly List<ExValue> _buffer = new();

    /// <summary>
    ///     confighandler for cli app for saving and getting configuration
    /// </summary>
    private readonly ConfigHandler<ExGwServiceIotDeviceConfig> _configHandler;

    /// <summary>
    ///     lock object
    /// </summary>
    private readonly object _listLock = new();

    /// <summary>
    ///     measurementworker
    /// </summary>
    private readonly List<DownstreamBase> _measurementWorker = new();

    /// <summary>
    ///     cancelation token für die measurementworker
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    ///     config des gerätes
    /// </summary>
    private ExGwServiceIotDeviceConfig _deviceConfig;

    /// <summary>
    ///     task für senden von von daten
    /// </summary>
    private Task? _sendTask;

    /// <summary>
    ///     Erzeugt eine neue Instanz
    /// </summary>
    /// <param name="deviceConfig"></param>
#pragma warning disable CS8618, CS9264
    public IotDeviceCore(ExGwServiceIotDeviceConfig deviceConfig)
#pragma warning restore CS8618, CS9264
    {
        if (deviceConfig == null!)
        {
            throw new ArgumentNullException(nameof(deviceConfig));
        }

        _deviceConfig = deviceConfig;
        switch (_deviceConfig.UpStreamType)
        {
            case EnumIotDeviceUpstreamTypes.Serial:
                throw new NotImplementedException();
            case EnumIotDeviceUpstreamTypes.Tcp:
                throw new NotImplementedException();
            case EnumIotDeviceUpstreamTypes.InGateway:
                Upstream = new UpstreamIotInGateway();
                break;
            case EnumIotDeviceUpstreamTypes.Ttn:
                throw new NotImplementedException();
            case EnumIotDeviceUpstreamTypes.Ble:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(_deviceConfig), $"[{nameof(IotDeviceCore)}]({nameof(IotDeviceCore)}): Could not determin corresponding upstream instance Config: {_deviceConfig.UpStreamType}");
        }
    }

    /// <summary>
    ///     Initialize für cli app
    /// </summary>
    /// <param name="iotDeviceFirstLaunch"></param>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
#pragma warning disable CS8618, CS9264
    public IotDeviceCore(Func<string, IoTDeviceCoreFirstLaunchInfos>? iotDeviceFirstLaunch = null)
#pragma warning restore CS8618, CS9264
    {
        var localDataDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "_Data"));
        if (!localDataDirectory.Exists)
        {
            localDataDirectory.Create();
        }

        var configFile = Path.Combine(localDataDirectory.FullName, "iotdevconfig.json");
        _configHandler = new ConfigHandler<ExGwServiceIotDeviceConfig>(new FileInfo(configFile));

        _deviceConfig = _configHandler.ReadConfig();
        _deviceConfig.Name = _deviceConfig.Name.Replace("GwService", string.Empty);

        if (_configHandler.IsNewConfig && iotDeviceFirstLaunch != null)
        {
            var tmp = iotDeviceFirstLaunch.Invoke(_deviceConfig.Name);
            //config.Position = tmp.Position; TODO
            if (!string.IsNullOrEmpty(tmp.Name))
            {
                _deviceConfig.Name = tmp.Name;
            }

            if (!string.IsNullOrEmpty(tmp.Secret))
            {
                _deviceConfig.Secret = tmp.Secret;
            }

            _deviceConfig.UpStreamType = tmp.UpstreamType;
            _deviceConfig.AdditionalConfiguration = tmp.HostNOrIPAddess.ToJson();

            if (_deviceConfig.UpStreamType != EnumIotDeviceUpstreamTypes.None)
            {
                _configHandler.StoreConfig(_deviceConfig);
            }
        }

        switch (_deviceConfig.UpStreamType)
        {
            case EnumIotDeviceUpstreamTypes.Serial:
                throw new NotImplementedException();
            case EnumIotDeviceUpstreamTypes.Tcp:
                ReceivedConfig(_deviceConfig);
                InitUpstream();
                break;
            case EnumIotDeviceUpstreamTypes.InGateway:
                Upstream = new UpstreamIotInGateway();
                break;
            case EnumIotDeviceUpstreamTypes.Ttn:
                throw new NotImplementedException();
            case EnumIotDeviceUpstreamTypes.Ble:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(_deviceConfig), $"[{nameof(IotDeviceCore)}]({nameof(IotDeviceCore)}): Could not determin corresponding upstream instance Config: {_deviceConfig.UpStreamType}");
        }
    }

    #region Properties

    /// <summary>
    ///     Verbindung zum Gateway
    /// </summary>
    public UpstreamBase Upstream { get; private set; }

    /// <summary>
    ///     ConnectionState des Gerätes
    /// </summary>
    public EnumIoTDeviceConnectionStates ConnectionState { get; private set; }


    /// <summary>
    ///     Gibt an ob die Statemachine derzeit läuft.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///     public Property um von außerhalb die derzeitige config lesen zu können
    /// </summary>
    public ExGwServiceIotDeviceConfig Config
    {
        get { return _deviceConfig; }
    }

    #endregion


    /// <summary>
    ///     Config wurde erhalten / geändert
    /// </summary>
    /// <param name="conf"></param>
    public void ReceivedConfig(ExGwServiceIotDeviceConfig conf)
    {
        var ipAddress = _deviceConfig.AdditionalConfiguration;
        _deviceConfig = conf;
        _deviceConfig.AdditionalConfiguration = ipAddress;
        if (_cts != null)
        {
            _cts.Cancel(); // cancel the current measurement worker tasks
        }

        _cts = new CancellationTokenSource();
        OnRefreshCliUi();
        _configHandler.StoreConfig(_deviceConfig);
        SetupTasks(_cts); // the config changed, so i need to start the tasks new
    }

    /// <summary>
    ///     Initialisieren des Upstreams
    /// </summary>
    /// <returns></returns>
    public void InitUpstream()
    {
        var hostIpAddress = BissDeserialize.FromJson<string>(_deviceConfig.AdditionalConfiguration);

        // ReSharper disable once UnusedVariable
        var connectionChanged = new Action<EnumIoTDeviceConnectionStates>(state =>
        {
            if (ConnectionState != state) // has changed
            {
                ConnectionState = state;
                OnRefreshCliUi();
            }
        });

        IPAddress ipAddr;
        UpstreamTcp3 upStrTcp;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (IPAddress.TryParse(hostIpAddress, out ipAddr))
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        {
            upStrTcp = new UpstreamTcp3(_deviceConfig, ipAddr, 802);
        }
        else
        {
            upStrTcp = new UpstreamTcp3(_deviceConfig, hostIpAddress, 802);
        }

        //if(Upstream != null && (UpstreamTcp3)Upstream != null)
        //    ((UpstreamTcp3)Upstream).Stop();
        Upstream = upStrTcp;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        upStrTcp.MainLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        //upStrTcp.StartAsync(802);
    }

    /// <summary>
    ///     Ereignis für CLI soll sich aktualisieren wenn möglich
    /// </summary>
    public event EventHandler RefreshCliUi;

    /// <summary>
    ///     Startet die Statemachine. Tut nichts wenn der loop bereits läuft.
    /// </summary>
    public Task StartMainLoop()
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        SetupTasks(_cts);
        IsRunning = true;
        Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(StartMainLoop)}): Statemachine started");
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Stoppt die Statemachine
    /// </summary>
    public async Task StopMainLoop()
    {
        if (!IsRunning)
        {
            return;
        }

        _cts?.Cancel();
        await Task.WhenAll(_measurementWorker.Select(s => s.WorkerTask)).ConfigureAwait(false);
        if (_sendTask is not null)
        {
            await _sendTask.ConfigureAwait(false);
        }

        _cts?.Dispose();

        //if(_configHandler != null) // only relevant for cli app, because otherwise we dont need confighandler and he will be null
        //    _configHandler.StoreConfig(_deviceConfig);
        Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(StopMainLoop)}): Statemachine stopped");
    }

    /// <summary>
    ///     Started für jede MeasurementDefinition einen separaten Task der den Befehl im angegebenen Interval abarbeitet.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <exception cref="ArgumentNullException">Config ist null</exception>
    public void SetupTasks(CancellationTokenSource stoppingToken)
    {
        foreach (var measurementDefinition in _deviceConfig.MeasurementDefinition)
        {
            //ToDo. MKo im Konfigtool (auch) berücksitigen
            measurementDefinition.MeasurementInterval = measurementDefinition.MeasurementInterval > 0 ? measurementDefinition.MeasurementInterval : _deviceConfig.MeasurementInterval;
            //ToDo. MKo fertig implementieren (Db, Konfig Tool ...)
            measurementDefinition.TransmissionType = _deviceConfig.TransmissionTime;

            _measurementWorker.Add(DownstreamHelper.GetDownstreamInstance(TransferValues, measurementDefinition, stoppingToken));
        }

        // Erzeuge einen Task für die Übertragung der Daten
        _sendTask = new Task(async () =>
        {
            var transmissionInterval = _deviceConfig.TransmissionInterval <= 0 ? 10 : _deviceConfig.TransmissionInterval;
            transmissionInterval *= 1000;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(transmissionInterval, stoppingToken.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    //ignored
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                List<ExValue> temp = new();
                lock (_listLock)
                {
                    if (_buffer.Count > 0)
                    {
                        temp = _buffer.ToList();
                        _buffer.Clear();
                    }
                }

                if (temp.Count > 0)
                {
                    var r = await Upstream.TransferData(temp).ConfigureAwait(true);
                    if (!r)
                    {
                        lock (_listLock)
                        {
                            _buffer.AddRange(temp);
                        }
                    }
                }
                else
                {
                    Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendbuffer empty. Skipping sendroutine");
                }
            }

            Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendingtask ran to completion");
        });

        _sendTask.Start();
        Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendingstart started");
    }

    /// <summary>
    ///     Methode von Ereignis für CLI soll sich aktualisieren wenn möglich
    /// </summary>
    protected virtual void OnRefreshCliUi()
    {
        var handler = RefreshCliUi;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        handler?.Invoke(this, null!);
    }

    /// <summary>
    ///     Methode wird der Downstream Base übergeben damit erfasste Messwerte entweder direkt übertragen oder gepuffert
    ///     übertragen werden
    /// </summary>
    /// <param name="values">Erfasste Messwerte der Downstream Type</param>
    /// <param name="transferInstantly">Sofort an das Gateway übertragen</param>
    /// <returns>Transfer erfolgreich?</returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task TransferValues(List<ExValue> values, bool transferInstantly)
    {
        if (transferInstantly)
        {
            var r = await Upstream.TransferData(values).ConfigureAwait(false);
            if (!r)
            {
                lock (_listLock)
                {
                    _buffer.AddRange(values);
                }
            }
        }
        else
        {
            lock (_listLock)
            {
                _buffer.AddRange(values);
            }
        }
    }
}