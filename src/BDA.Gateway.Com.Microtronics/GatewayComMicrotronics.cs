// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Microtronics;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.MicrotronicsClient;
using BDA.Gateway.Com.Base;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace BDA.Gateway.Com.Microtronics
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse GatewayComMicrotronics. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GatewayComMicrotronics : GatewayComBase
    {
        private readonly GcMicrotronicsIotDevice _config;
        private readonly Timer _requestTimer = new(60 * 1000);
        private readonly SemaphoreSlim _startingSemaphore = new SemaphoreSlim(1);
        private CancellationTokenSource? _cts;
        private DateTime? _lastTimeStamp;
        private MicrotronicsApiClient _microtronicsClient;
        private int _updating;

        /// <inheritdoc />
        public GatewayComMicrotronics(ExGwServiceGatewayConfig config, ExGwServiceIotDeviceConfig iotDevice) : base(config.DbId, iotDevice)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (iotDevice is null)
            {
                throw new ArgumentNullException(nameof(iotDevice));
            }

            _config = new GcBaseConverter<GcMicrotronicsIotDevice>(iotDevice.AdditionalConfiguration).Base;

            if (iotDevice.TransmissionInterval > 0)
            {
                _requestTimer = new Timer(iotDevice.TransmissionInterval * 1000);
            }

            _microtronicsClient = new MicrotronicsApiClient(_config.GcMicrotronicsCompany.BackendDomain, _config.GcMicrotronicsCompany.UserName, _config.GcMicrotronicsCompany.Password);
        }

        /// <summary>
        /// start
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            try
            {
                await _startingSemaphore.WaitAsync();
                Stop();

                _cts = new CancellationTokenSource();
                _microtronicsClient = new MicrotronicsApiClient(_config.GcMicrotronicsCompany.BackendDomain, _config.GcMicrotronicsCompany.UserName, _config.GcMicrotronicsCompany.Password);

                await Update().ConfigureAwait(false);
                _requestTimer.Elapsed += RequestTimerOnElapsed;
                _requestTimer.Start();
            }
            catch (InvalidOperationException e)
            {
                Logging.Log.LogError(e, $"[{nameof(GatewayComMicrotronics)}]({nameof(StartAsync)}): Could not init Com");
            }
            finally
            {
                _startingSemaphore.Release();
            }
        }

        /// <summary>
        ///     Holt die neueseten Werte von Microtronics
        /// </summary>
        public async Task Update()
        {
            var channels = IotConfig.MeasurementDefinition;
            var sensordata = await _microtronicsClient.GetYoungestValues(_config.CustomerId, _config.SiteId, _config.HistDataConfiguration, channels.Select(m => m.Name).ToList()).ConfigureAwait(false);
            var values = new List<ExValue>();

            foreach (var sensorValue in sensordata)
            {
                var curr = channels.FirstOrDefault(c => c.Name == sensorValue.Item1);

                if (curr == null)
                {
                    continue;
                }

                var value = new ExValue();
                value.ValueType = EnumValueTypes.Text;
                value.MeasurementText = sensorValue.Item3;
                value.Identifier = curr.DbId;
                value.TimeStamp = sensorValue.Item2;
                values.Add(value);

                Logging.Log.LogInfo($"[{nameof(GatewayComMicrotronics)}]({nameof(Update)}): New MicrotronicsValue: Value: {value.MeasurementText}");
            }

            if (values.All(v => v.TimeStamp == _lastTimeStamp)) // im Normalfall (senden alle auf einmal) haben alle den gleichen Timestamp
            {
                values.Clear();
            }

            if (values.Any())
            {
                _lastTimeStamp = values.First().TimeStamp;
                await UpdateIotDeviceState(EnumDeviceOnlineState.Online, "Microtronics").ConfigureAwait(false);
                await UpdateIotDeviceConfig(IotConfig).ConfigureAwait(false);
                await NewValues(values).ConfigureAwait(false);
            }
        }


        /// <summary>
        ///     Stoppt den Microtronicsworker
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _microtronicsClient.Dispose();
            _requestTimer.Elapsed -= RequestTimerOnElapsed;
            _requestTimer.Stop();
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
                await _startingSemaphore.WaitAsync();
                await Update().ConfigureAwait(false);
                _startingSemaphore.Release();
                _updating = 0;
            }
        }
    }
}