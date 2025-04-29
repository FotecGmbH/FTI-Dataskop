// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.DreiClient;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Drei;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.GatewayService.Drei;
using BDA.Gateway.Com.Base;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace BDA.Gateway.Com.Drei
{
    /// <summary>
    /// GatewayComDrei
    /// </summary>
    public class GatewayComDrei : GatewayComBase
    {
        private static bool _initialized;
        private static NgrokHandler? _ngrokHandler;
        private static DreiClient? _dreiClient;
        private static string? _gatewayId;
        private static Timer? _ngrokRestartTimer;
        private readonly GcDreiIotDevice? _dreiConfig;
        private CancellationTokenSource _cts;
        // ReSharper disable once UnusedMember.Local
        private Dictionary<string, string> _deviceAddresses = new();
        private int _downlinkDebouce;
        private DateTime? _lastMessage;
        private Task? _watchdogWorker;

        /// <inheritdoc />
        public GatewayComDrei(ExGwServiceGatewayConfig config, ExGwServiceIotDeviceConfig iotDevice) : base(config.DbId, iotDevice)
        {
            _gatewayId ??= $"{config.Name}({config.DbId})";
            _cts = new CancellationTokenSource();
            _dreiConfig ??= new GcBaseConverter<GcDreiIotDevice>(iotDevice.AdditionalConfiguration).Base;
        }

        /// <summary>
        ///     Startet den Mqtt-Listener
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _cts.Cancel();

                if (_watchdogWorker != null)
                {
                    await _watchdogWorker.ConfigureAwait(false);
                }

                _cts.Dispose();

                _cts = new CancellationTokenSource();

                _watchdogWorker = WatchdogWorker(_cts.Token);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                await Init(_dreiConfig.GcDreiCompany).ConfigureAwait(false);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (_ngrokHandler is not null)
                {
                    _ngrokHandler.NewPostDataReceived += NgrokHandlerOnNewPostDataReceived;
                }
            }

            catch (InvalidOperationException e)
            {
                Logging.Log.LogError(e, $"[{nameof(GatewayComDrei)}]({nameof(StartAsync)}): Could not init DreiCom");
            }
        }

        /// <summary>
        ///     Stoppt das Gateway: Disconnected den MqttClient.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            if (_ngrokHandler is not null)
            {
                _ngrokHandler.NewPostDataReceived -= NgrokHandlerOnNewPostDataReceived;
            }
        }

        /// <summary>
        ///     Handles Mqttmessages
        /// </summary>
        /// <param name="frame"></param>
        public async Task HandlePostData(ExDreiUplinkFrame? frame)
        {
            if (frame is null || _dreiConfig!.DevEui != frame.DevEui)
            {
                return;
            }

            Logging.Log.LogInfo($"[{nameof(GatewayComDrei)}]({nameof(HandlePostData)}): Uplink received from Drei: DeviceID: {frame.DeviceId}");

            var thirdParty = _dreiConfig?.UserCode?.UserCode.Length > 0;

            _lastMessage = DateTime.Now;
            var decoded = frame;

            if (thirdParty)
            {
                await UpdateIotDeviceState(EnumDeviceOnlineState.Online, decoded.FirmwareVersion.ToString()).ConfigureAwait(false);
                await UpdateIotDeviceConfig(IotConfig).ConfigureAwait(false);


                await NewValues(decoded.GetFramePayloadBytes(), IotConfig.DbId, _dreiConfig!.UserCode, frame.DevAddr).ConfigureAwait(false);
                return;
            }

            // FPort 15 => Normale Messungsdaten
            if (decoded.FramePort == 15)
            {
                await UpdateIotDeviceState(EnumDeviceOnlineState.Online, decoded.FirmwareVersion.ToString()).ConfigureAwait(false);

                if (_downlinkDebouce-- <= 0)
                {
                    await UpdateIotDeviceConfig(IotConfig).ConfigureAwait(false);
                }

                try
                {
                    await NewValues(decoded.GetMeasurementData(), IotConfig.DbId, devaddr: frame.DevAddr).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logging.Log.LogWarning(e, $"[{nameof(GatewayComDrei)}]({nameof(HandlePostData)}): Error while sending new Values");
                }
            }
            else if (decoded.FramePort == 10)
            {
                await UpdateIotDeviceState(EnumDeviceOnlineState.Online, "Uninitialized").ConfigureAwait(false);

                await UpdateIotDeviceConfig(IotConfig, true).ConfigureAwait(false);
            }
            else
            {
                Logging.Log.LogWarning($"[{nameof(GatewayComDrei)}]({nameof(HandlePostData)}): Device sent {decoded.FramePort} Frameport");
            }
        }

        /// <summary>
        ///     Stoppt den Ngrok tunnel und den http server
        /// </summary>
        public static async Task StopCommonHttpServer()
        {
            if (_ngrokHandler is not null)
            {
                await _ngrokHandler.StopTunnelAsync().ConfigureAwait(false);
            }

            _dreiClient?.Dispose();
        }

        /// <summary>
        ///     Erstellt die den passenden drei Flow für alle deveuis
        /// </summary>
        /// <param name="deveuis">Die deveuis die auf diesen Flow geleitet werden sollen.</param>
        public static async Task UpdateDreiFlowConfiguration(List<string> deveuis)
        {
            if (_gatewayId is not null && _dreiClient is not null)
            {
                await _dreiClient.SetupGatewayFlow(_gatewayId, deveuis).ConfigureAwait(false);
            }
        }


        /// <summary>
        ///     Neue Konfig an ein bestimmtes Iot - Gerät übertragen
        /// </summary>
        /// <param name="iotDeviceConfig"></param>
        /// <param name="resendConfig">
        ///     Wenn dieses Argument true ist, wird der Opcode geschickt auch wenn er gleich ist wie der
        ///     zuletzt übertragenen Opcode.
        /// </param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<bool> TransferConfig(ExGwServiceIotDeviceConfig? iotDeviceConfig, bool resendConfig = false)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        private async Task WatchdogWorker(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_lastMessage != null)
                {
                    if (DateTime.Now - _lastMessage > TimeSpan.FromSeconds(IotConfig.Plattform == EnumIotDevicePlattforms.Prebuilt ? 60 * 10 : IotConfig.TransmissionInterval * 1.5f))
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
                    Logging.Log.LogTrace($"[{nameof(GatewayComDrei)}]({nameof(WatchdogWorker)}): Watchdogworker Stopped");
                }
            }
        }

        private static async Task Init(GcDrei ttnConfig)
        {
            if (_initialized)
            {
                return;
            }

            await InitCommonHttpServer(ttnConfig).ConfigureAwait(false);
            _initialized = true;
        }

        private async void NgrokHandlerOnNewPostDataReceived(object? sender, EventArgsNewPostDataReceived e)
        {
            try
            {
                await HandlePostData(e.UplinkFrame).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Log.LogError(ex, $"[{nameof(GatewayComDrei)}]({nameof(NgrokHandlerOnNewPostDataReceived)}): Error while handeling Postdata: {ex.Message}\n Stacktrace: {ex.StackTrace}");
            }
        }

        private static async Task InitCommonHttpServer(GcDrei config)
        {
            _dreiClient ??= new DreiClient(config.LoginName, config.Password);

            _ngrokHandler = new NgrokHandler();

            _ngrokRestartTimer = new Timer(TimeSpan.FromHours(1));
            _ngrokRestartTimer.AutoReset = true;
            _ngrokRestartTimer.Elapsed += async (_, _) => await CreateTunnelAndSetupDreiConnection().ConfigureAwait(false);
            _ngrokRestartTimer.Start();
            await CreateTunnelAndSetupDreiConnection().ConfigureAwait(false);
        }

        private static async Task CreateTunnelAndSetupDreiConnection()
        {
            if (_ngrokHandler is null)
            {
                throw new InvalidOperationException("Ngrokhandler was null. Cannot operate correctly");
            }

            Logging.Log.LogInfo($"[{nameof(GatewayComDrei)}]({nameof(CreateTunnelAndSetupDreiConnection)}): Restarting ngrok and server");

            var puburl = await _ngrokHandler.StartTunnelAsync(restartIfRunning: true).ConfigureAwait(false);

            if (puburl.Length > 0 && _dreiClient is not null)
            {
                await _dreiClient.SetupGatewayConnection(puburl, _gatewayId ?? "Error").ConfigureAwait(false);
            }
        }
    }
}