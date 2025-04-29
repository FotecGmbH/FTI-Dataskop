// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BDA.Common.Exchange.Configs.Downstreams;
using BDA.Common.Exchange.Configs.Downstreams.OpenSense;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Opensense;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.OpensenseClient;
using BDA.Gateway.Com.Base;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace BDA.Gateway.Com.Opensense
{
    /// <inheritdoc />
    public class GatewayComOpensense : GatewayComBase
    {
        private readonly GcOpenSenseIotDevice _config;
        private readonly Timer _requestTimer = new(60 * 1000);
        private readonly SemaphoreSlim _startingSemaphore = new SemaphoreSlim(1);
        private readonly Dictionary<long, DateTime> latestSensorUpdate = new();
        private readonly Dictionary<string, long> sensorIds = new();
        private CancellationTokenSource? _cts;
        private DateTime? _lastMessage;
        private OpensenseClient _opensenseClient = new();
        private int _updating;
        private Task? _watchdogWorker;


        /// <inheritdoc />
        public GatewayComOpensense(ExGwServiceGatewayConfig config, ExGwServiceIotDeviceConfig iotDevice) : base(config.DbId, iotDevice)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (iotDevice is null)
            {
                throw new ArgumentNullException(nameof(iotDevice));
            }

            _config = new GcBaseConverter<GcOpenSenseIotDevice>(iotDevice.AdditionalConfiguration).Base;
        }

        /// <summary>
        ///     Basiskommunikation um mit dem BDA Service zu kommunizieren (via Signal R)
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
#pragma warning disable CS8618, CS9264
        public GatewayComOpensense(AssemblyLoadContext? ctx) : base(ctx)
#pragma warning restore CS8618, CS9264
        {
        }

        /// <summary>
        ///     Startet den Opensenseworker
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                await _startingSemaphore.WaitAsync();
                Stop();

                _cts = new CancellationTokenSource();
                _opensenseClient = new();

                if (_watchdogWorker != null)
                {
                    await _watchdogWorker.ConfigureAwait(false);
                }

                _watchdogWorker = WatchdogWorker(_cts.Token);


                sensorIds.Clear();
                latestSensorUpdate.Clear();
                foreach (var definition in IotConfig.MeasurementDefinition)
                {
                    var config = new GcBaseConverter<GcDownstreamBase>(definition.AdditionalConfiguration).ConvertTo<GcDownstreamOpenSense>();
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (config is not null)
                    {
                        sensorIds.TryAdd(config.SensorID, definition.DbId);
                    }
                }


                await Update().ConfigureAwait(false);
                _requestTimer.Elapsed += RequestTimerOnElapsed;
                _requestTimer.Start();
            }
            catch (InvalidOperationException e)
            {
                Logging.Log.LogError(e, $"[{nameof(GatewayComOpensense)}]({nameof(StartAsync)}): Could not init ttnCom");
            }
            finally
            {
                _startingSemaphore.Release();
            }
        }


        /// <summary>
        ///     Stoppt den Opensenseworker
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _opensenseClient.Dispose();
            _requestTimer.Elapsed -= RequestTimerOnElapsed;
            _requestTimer.Stop();
        }


        /// <summary>
        ///     Holt die neueseten Werte von Opensense
        /// </summary>
        public async Task Update()
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var sensordata = await _opensenseClient.GetCurrentValuesAsync(_config?.BoxId ?? "").ConfigureAwait(false);
            var values = new List<ExValue>();

            foreach (var sensorValue in sensordata)
            {
                if (sensorIds.TryGetValue(sensorValue.OpensenseId, out var id))
                {
                    if (latestSensorUpdate.TryGetValue(id, out var latestUpdate) && latestUpdate == sensorValue.CreatedAt)
                    {
                        continue;
                    }

                    if (!latestSensorUpdate.TryAdd(id, sensorValue.CreatedAt))
                    {
                        latestSensorUpdate[id] = sensorValue.CreatedAt;
                    }

                    var value = new ExValue();
                    value.ValueType = EnumValueTypes.Text;
                    value.MeasurementText = sensorValue.Value;
                    value.Identifier = id;
                    value.TimeStamp = sensorValue.CreatedAt;
                    values.Add(value);

                    Logging.Log.LogInfo($"[{nameof(GatewayComOpensense)}]({nameof(Update)}): New OpensenseValue: Value: {value.MeasurementText} Sensor: {sensorValue.Title}");
                }
            }

            if (values.Any())
            {
                await UpdateIotDeviceState(EnumDeviceOnlineState.Online, "Opensense").ConfigureAwait(false);
                await UpdateIotDeviceConfig(IotConfig).ConfigureAwait(false);
                await NewValues(values).ConfigureAwait(false);
                _lastMessage = DateTime.Now;
            }
        }

        /// <summary>
        ///     Neue Konfig an ein bestimmtes Iot - Gerät übertragen
        /// </summary>
        /// <param name="iotDeviceConfig"></param>
        /// <param name="resend">Opcode wird nicht verglichen und einfach nocheinmal geschickt.</param>
        /// <returns></returns>
        protected override Task<bool> TransferConfig(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resend = false)
        {
            return Task.FromResult(true);
        }

        private async void RequestTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _updating, 1, 0) == 0)
            {
                await Update().ConfigureAwait(false);
                _updating = 0;
            }
        }

        private async Task WatchdogWorker(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_lastMessage != null)
                {
                    if (DateTime.Now - _lastMessage > TimeSpan.FromSeconds(60 * 10))
                    {
                        await UpdateIotDeviceState(EnumDeviceOnlineState.Offline, IotConfig.FirmwareVersion).ConfigureAwait(false);
                    }
                }

                try
                {
                    await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    Logging.Log.LogTrace($"[{nameof(GatewayComOpensense)}]({nameof(WatchdogWorker)}): Watchdogworker Stopped");
                }
            }
        }
    }
}