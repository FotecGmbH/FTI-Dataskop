// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Upstream.Base;
using BDA.IotDevice.Com.Upstream.IotInGateway;
using Biss.Log.Producer;

namespace BDA.IotDevice.Core;

/// <summary>
///     <para>Die Statemachine die den Mainloop beinhaltet.</para>
///     Klasse StateMachine. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Obsolete]
public class IotDeviceStateMachineOld
{
    private readonly List<ExValue> _buffer = new();
    private readonly ExGwServiceIotDeviceConfig _deviceConfig;
    private List<Task> _commandTasks = new();
    private CancellationTokenSource? _cts;
    private Task? _sendTask;

    /// <summary>
    ///     Erzeugt eine neue Instanz
    /// </summary>
    /// <param name="deviceConfig"></param>
    public IotDeviceStateMachineOld(ExGwServiceIotDeviceConfig deviceConfig)
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

    #region Properties

    /// <summary>
    /// Upstream
    /// </summary>
    public UpstreamBase Upstream { get; }


    /// <summary>
    ///     Gibt an ob die Statemachine derzeit läuft.
    /// </summary>
    public bool IsRunning { get; private set; }

    #endregion


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

        IsRunning = true;

        SetupTasks(_cts.Token);

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
        _cts?.Dispose();
        await Task.WhenAll(_commandTasks).ConfigureAwait(false);
        if (_sendTask is not null)
        {
            await _sendTask.ConfigureAwait(false);
        }

        Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(StopMainLoop)}): Statemachine stopped");
    }

    /// <summary>
    ///     Started für jede MeasurementDefinition einen separaten Task der den Befehl im angegebenen Interval abarbeitet.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <exception cref="ArgumentNullException">Config ist null</exception>
    public void SetupTasks(CancellationToken stoppingToken)
    {
        _commandTasks = new List<Task>();
        foreach (var measurementDefinition in _deviceConfig.MeasurementDefinition)
        {
            var task = GenerateCommandAction(measurementDefinition, stoppingToken);
            if (task == null)
            {
                continue;
            }

            task.Start();
            _commandTasks.Add(task);
        }


        // Erzeuge einen Task für die Übertragung der Daten
        _sendTask = new Task(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_deviceConfig.TransmissionInterval * 1000, stoppingToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    //ignored
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                if (_buffer.Count > 0)
                {
                    //var temp = _buffer.ToList();
                    //_buffer.Clear();

                    //if (await Upstream.TransferData(temp).ConfigureAwait(false))
                    //{
                    //    foreach (var exValue in temp)
                    //    {
                    //        var data = exValue.ValueType == EnumValueTypes.Number ? exValue.MeasurementNumber.ToString() : exValue.ValueType == EnumValueTypes.Text ? exValue.MeasurementText : "Cannot represent Data as string";
                    //        Logging.Log.LogInfo($"[{nameof(IotDeviceStateMachine)}]({nameof(SetupTasks)}): Sentdata: {data}, ID: {exValue.Identifier}");
                    //    }

                    //    Logging.Log.LogInfo($"[{nameof(IotDeviceStateMachine)}]({nameof(SetupTasks)}): Clearing sendbuffer");
                    //}
                    //else
                    //{
                    //    Logging.Log.LogWarning($"[{nameof(IotDeviceStateMachine)}]({nameof(SetupTasks)}): Error sending data");
                    //    Logging.Log.LogInfo($"[{nameof(IotDeviceStateMachine)}]({nameof(SetupTasks)}): Clearing sendbuffer");
                    //}
                }

                Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendbuffer empty. Skipping sendroutine");
            }

            Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendingtask ran to completion");
        });

        _sendTask.Start();

        Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(SetupTasks)}): Sendingstart started");
    }

    /// <summary>
    ///     Erzeugt einen Task der das Command im passenden Interval abarbeitet
    /// </summary>
    /// <param name="def">Messwert Konfiguration und State-Machine</param>
    /// <param name="stoppingToken">Token um den Task abzubrechen</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Task? GenerateCommandAction(ExGwServiceMeasurementDefinitionConfig def, CancellationToken stoppingToken)
    {
        if (def == null!)
        {
            throw new ArgumentNullException(nameof(def));
        }

        switch (def.DownstreamType)
        {
            case EnumIotDeviceDownstreamTypes.Virtual:
                return ProcessDownstreamVirtual(def, stoppingToken);
            case EnumIotDeviceDownstreamTypes.I2C:
                break;
            case EnumIotDeviceDownstreamTypes.Spi:
                break;
            case EnumIotDeviceDownstreamTypes.Modbus:
                break;
            case EnumIotDeviceDownstreamTypes.Pi:
                break;
            case EnumIotDeviceDownstreamTypes.Arduino:
                break;
            case EnumIotDeviceDownstreamTypes.Esp32:
                break;
            case EnumIotDeviceDownstreamTypes.DotNet:
                return ProcessDownstreamDotNet(def, stoppingToken);
            default:
                throw new ArgumentOutOfRangeException($"[{nameof(IotDeviceCore)}]({nameof(GenerateCommandAction)}): {def.DownstreamType} out of range");
        }


        return null;
    }

    /// <summary>
    /// ProcessDownstreamDotNet
    /// </summary>
    /// <param name="def"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public Task? ProcessDownstreamDotNet(ExGwServiceMeasurementDefinitionConfig def, CancellationToken stoppingToken)
    {
        if (def == null!)
        {
            throw new ArgumentNullException($"[{nameof(IotDeviceCore)}]({nameof(ProcessDownstreamDotNet)}): {nameof(def)} is null");
        }

        var configBase = new GcBaseConverter<GcDownstreamDotNet>(def.AdditionalConfiguration);
        var delayMs = def.MeasurementInterval <= 0 ? _deviceConfig.MeasurementInterval : def.MeasurementInterval;
        delayMs *= 100;

        if (configBase.Base.DotNetMeasurementType == EnumIotDeviceDotNetMeasurementTypes.CpuUsagePercent ||
            configBase.Base.DotNetMeasurementType == EnumIotDeviceDotNetMeasurementTypes.Memory)
        {
            return new Task(async () =>
            {
                object? osInfo = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (configBase.Base.DotNetMeasurementType == EnumIotDeviceDotNetMeasurementTypes.CpuUsagePercent)
                    {
                        osInfo = new PerformanceCounter
                        {
                            CategoryName = "Processor",
                            CounterName = "% Processor Time",
                            InstanceName = "_Total"
                        };
                    }
                    else
                    {
                        osInfo = new PerformanceCounter
                        {
                            CategoryName = "Memory",
                            CounterName = "% Committed Bytes In Use",
                        };
                    }
                }


                while (!stoppingToken.IsCancellationRequested)
                {
                    double val = -1f;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && osInfo != null)
                    {
                        val = ((PerformanceCounter) osInfo).NextValue();
                        val = Math.Round(val, 2);
                    }

                    _buffer.Add(new ExValue
                    {
                        Identifier = def.DbId,
                        MeasurementNumber = val,
                        TimeStamp = DateTime.Now,
                        ValueType = EnumValueTypes.Number,
                        Position = null
                    });

                    try
                    {
                        await Task.Delay(delayMs, stoppingToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignored
                    }
                }

                Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(ProcessDownstreamDotNet)}): Task executing command with ID {def.DbId} ran to completion");
            });
        }

        throw new NotImplementedException($"[{nameof(IotDeviceCore)}]({nameof(ProcessDownstreamDotNet)}): not implemented");
    }


    /// <summary>
    ///     Behandlung von virtuellen Messwerten
    /// </summary>
    /// <param name="def"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public Task? ProcessDownstreamVirtual(ExGwServiceMeasurementDefinitionConfig def, CancellationToken stoppingToken)
    {
        if (def == null!)
        {
            throw new ArgumentNullException(nameof(def));
        }

        var configBase = new GcBaseConverter<GcDownstreamVirtualBase>(def.AdditionalConfiguration);
        var delayMs = def.MeasurementInterval <= 0 ? _deviceConfig.MeasurementInterval : def.MeasurementInterval;
        delayMs *= 100;

        if (configBase.Base.VirtualOpcodeType == EnumIotDeviceVirtualMeasurementTypes.Float)
        {
            var config = configBase.ConvertTo<GcDownstreamVirtualFloat>();
            return new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var (latitude, longitude) = GetLatLong(configBase.Base);
                    var rng = new Random((int) DateTime.Now.Ticks);
#pragma warning disable CA5394 // Do not use insecure randomness
                    var val = (double) (rng.NextSingle() * (config.Max - config.Min) + config.Min);
#pragma warning restore CA5394 // Do not use insecure randomness

                    val = Math.Round(val, 2);

                    _buffer.Add(new ExValue
                    {
                        Identifier = def.DbId,
                        MeasurementNumber = val,
                        TimeStamp = DateTime.Now,
                        ValueType = EnumValueTypes.Number,
                        Position = new()
                        {
                            Latitude = latitude,
                            Longitude = longitude
                        }
                    });

                    try
                    {
                        await Task.Delay(delayMs, stoppingToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignored
                    }
                }

                Logging.Log.LogInfo($"[{nameof(IotDeviceCore)}]({nameof(ProcessDownstreamVirtual)}): Task executing command with ID {def.DbId} ran to completion");
            });
        }

        throw new NotImplementedException($"[{nameof(IotDeviceCore)}]({nameof(ProcessDownstreamVirtual)}): not implemented");
    }

    /// <summary>
    ///     Methode wird der Downstream Base übergeben damit erfasste Messwerte entweder direkt übertragen oder gepuffert
    ///     übertragen werden
    /// </summary>
    /// <param name="values">Erfasste Messwerte der Downstream Type</param>
    /// <param name="transferInstantly">Sofort an das Gateway übertragen</param>
    /// <returns>Transfer erfolgreich?</returns>
    /// <exception cref="NotImplementedException"></exception>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // ReSharper disable once UnusedMember.Local
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    // ReSharper disable once UnusedMember.Local
    private async Task TransferValues(List<ExValue> values, bool transferInstantly)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        throw new NotImplementedException();
    }


    /// <summary>
    ///     Erzeuge zufällige Koordinaten in einem gewissen Umkreis.
    /// </summary>
    /// <param name="baseInfos"></param>
    /// <returns></returns>
    private Tuple<float, float> GetLatLong(GcDownstreamVirtualBase baseInfos)
    {
        var random = new Random();

        // Convert radius from meters to degrees
        var radiusInDegrees = baseInfos.AreaRadius / 111000f;

#pragma warning disable CA5394 // Do not use insecure randomness
        var u = random.NextSingle();
        var v = random.NextSingle();
#pragma warning restore CA5394 // Do not use insecure randomness
        var w = radiusInDegrees * MathF.Sqrt(u);
        var t = 2 * MathF.PI * v;
        var x = w * MathF.Cos(t);
        var y = w * MathF.Sin(t);

        // Adjust the x-coordinate for the shrinking of the east-west distances
        var newX = x / MathF.Cos((MathF.PI / 180) * baseInfos.AreaLatitude);

        var foundLongitude = newX + baseInfos.AreaLogitute;
        var foundLatitude = y + baseInfos.AreaLatitude;

        return new Tuple<float, float>(foundLatitude, foundLongitude);
    }
}