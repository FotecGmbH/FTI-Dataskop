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
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.GatewayService.TTN;
using BDA.Common.TtnClient;
using BDA.Gateway.Com.Base;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;

namespace BDA.Gateway.Com.Ttn
{
    /// <inheritdoc />
    public class GatewayComTtn : GatewayComBase
    {
        private static IManagedMqttClient? _commonMqttClientTtn;
        private static MqttTopicFilter[]? _topicFilters;
        private static string _appId = "";
        private static string _mqttUser = "";
        private static bool _initialized;
        private readonly Dictionary<string, string> _deviceAddresses = new();
        private readonly TtnApiClient _ttnApiClient;
        private readonly GcTtnIotDevice _ttnDevice;
        private CancellationTokenSource _cts;
        private int _downlinkDebouce;
        private DateTime? _lastMessage;
        private byte[]? _lastOpCode;
        private Task? _watchdogWorker;

        /// <inheritdoc />
        public GatewayComTtn(ExGwServiceGatewayConfig config, ExGwServiceIotDeviceConfig iotDevice) : base(config.DbId, iotDevice)
        {
            _cts = new CancellationTokenSource();
            _ttnDevice = new GcBaseConverter<GcTtnIotDevice>(iotDevice.AdditionalConfiguration).Base;
            _appId = _ttnDevice.GcTtnCompany.Applicationid;
            _ttnApiClient = new TtnApiClient(_ttnDevice.GcTtnCompany);
            _mqttUser = _appId + "@ttn";
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

                await Init(_ttnDevice.GcTtnCompany).ConfigureAwait(false);
                OnNewMqttMessage += OnNewMqttMessageEventHandler;
            }
            catch (InvalidOperationException e)
            {
                Logging.Log.LogError(e, $"[{nameof(GatewayComTtn)}]({nameof(StartAsync)}): Could not init ttnCom");
            }
        }

        /// <summary>
        ///     Trennt die Verbindung des MqttClients.
        /// </summary>
        public static async Task Reset()
        {
            if (_commonMqttClientTtn is null)
            {
                return;
            }

            _initialized = false;
            await _commonMqttClientTtn.UnsubscribeAsync(_topicFilters?.Select(t => t.Topic.ToString()).ToList()).ConfigureAwait(false);
            await _commonMqttClientTtn.StopAsync().ConfigureAwait(false);
            _commonMqttClientTtn.Dispose();
            _commonMqttClientTtn = null;
        }

        /// <summary>
        ///     Stoppt das Gateway: Disconnected den MqttClient.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            OnNewMqttMessage -= OnNewMqttMessageEventHandler;
        }


        /// <summary>
        ///     Handles Mqttmessages
        /// </summary>
        /// <param name="args"></param>
        public async Task HandleMqttMessageAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            //Join
            if (topic == "v3/" + _mqttUser + "/devices/" + _ttnDevice.DeviceId + "/join")
            {
                Logging.Log.LogInfo($"[{nameof(GatewayComTtn)}]({nameof(HandleMqttMessageAsync)}): Joinrequest received ID: {_ttnDevice.DeviceId}");

#pragma warning disable CS0618 // Type or member is obsolete
                var decoded = new ExTtnJoinFrame(Encoding.UTF8.GetString(args.ApplicationMessage.Payload));
#pragma warning restore CS0618 // Type or member is obsolete
                if (!_deviceAddresses.TryAdd(decoded.DeviceId, decoded.DeviceAddr) && _deviceAddresses.ContainsKey(decoded.DeviceId))
                {
                    _deviceAddresses[decoded.DeviceId] = decoded.DeviceAddr;
                }
            }


            //Uplink
            if (topic == "v3/" + _mqttUser + "/devices/" + _ttnDevice.DeviceId + "/up")
            {
                Logging.Log.LogInfo($"[{nameof(GatewayComTtn)}]({nameof(HandleMqttMessageAsync)}): Uplink received ID: {_ttnDevice.DeviceId}");

                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                var thirdParty = _ttnDevice?.UserCode?.UserCode.Length > 0;

                _lastMessage = DateTime.Now;
#pragma warning disable CS0618 // Type or member is obsolete
                var decoded = new ExTtnUplinkFrame(Encoding.UTF8.GetString(args.ApplicationMessage.Payload));
#pragma warning restore CS0618 // Type or member is obsolete
                // ReSharper disable once RedundantAssignment
                var devaddr = string.Empty;

                if (!_deviceAddresses.ContainsKey(decoded.DeviceId))
                {
                    devaddr = await _ttnApiClient.GetDevaddr(decoded.DeviceId, _appId).ConfigureAwait(false) ?? "";
                    _deviceAddresses.TryAdd(decoded.DeviceId, devaddr);
                }
                else
                {
                    devaddr = _deviceAddresses[decoded.DeviceId];
                }

                if (thirdParty)
                {
                    await UpdateIotDeviceState(EnumDeviceOnlineState.Online, decoded.FirmwareVersion.ToString()).ConfigureAwait(false);
                    await UpdateIotDeviceConfig(IotConfig).ConfigureAwait(false);


                    await NewValues(decoded.GetFramePayloadBytes(), IotConfig.DbId, _ttnDevice!.UserCode, devaddr).ConfigureAwait(false);
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
                        await NewValues(decoded.GetMeasurementData(), IotConfig.DbId, devaddr: devaddr).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogWarning(e, $"[{nameof(GatewayComTtn)}]({nameof(HandleMqttMessageAsync)}): Error while sending new Values");
                    }
                }
                else if (decoded.FramePort == 10)
                {
                    await UpdateIotDeviceState(EnumDeviceOnlineState.Online, "Uninitialized").ConfigureAwait(false);

                    await UpdateIotDeviceConfig(IotConfig, true).ConfigureAwait(false);
                }
                else
                {
                    Logging.Log.LogWarning($"[{nameof(GatewayComTtn)}]({nameof(HandleMqttMessageAsync)}): Device sent {decoded.FramePort} Frameport");
                }
            }
        }

        /// <summary>
        ///     Downlink Nachricht senden
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
#pragma warning disable CS0108, CS0114
        public async Task SendDownlinkMessage(byte[] message)
#pragma warning restore CS0108, CS0114
        {
            await TtnFirmwareManager.AppendDownlink(_ttnDevice.DeviceId, _appId, message, 1, _commonMqttClientTtn!, true, true).ConfigureAwait(false);
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
        protected override async Task<bool> TransferConfig(ExGwServiceIotDeviceConfig? iotDeviceConfig, bool resendConfig = false)
        {
            if (iotDeviceConfig is null)
            {
                return false;
            }

            if (iotDeviceConfig.ConfigVersion == iotDeviceConfig.ConfigVersionService && !resendConfig)
            {
                return true;
            }

            Logging.Log.LogInfo($"[{nameof(GatewayComTtn)}]({nameof(TransferConfig)}): Transfering new Config via MQTT. Device: {iotDeviceConfig.DbId}");
            var opcodes = ToStateMachineEmbedded(iotDeviceConfig);

            if (resendConfig)
            {
                Logging.Log.LogInfo($"[{nameof(GatewayComTtn)}]({nameof(TransferConfig)}): Check if Opcode was already sent was skipped.");
            }

            //Prüfung ob sich OP-Code verandert hat
            if (_lastOpCode != null && _lastOpCode.Length == opcodes.Length && !resendConfig)
            {
                var changed = false;
                for (var i = 0; i < _lastOpCode.Length; i++)
                {
                    if (_lastOpCode[i] != opcodes[i])
                    {
                        changed = true;
                        break;
                    }
                }

                if (!changed)
                {
                    Logging.Log.LogInfo($"[{nameof(GatewayComTtn)}]({nameof(TransferConfig)}): No changes in Op-Code! No Transfer to TTN!");
                    return true;
                }
            }

            var config = new GcBaseConverter<GcTtnIotDevice>(iotDeviceConfig.AdditionalConfiguration).Base;
            if (_commonMqttClientTtn is null)
            {
                return false;
            }

            Logging.Log.LogTrace($"[{nameof(GatewayComTtn)}]({nameof(TransferConfig)}): Send OP-Code for IoT Device {config.DeviceId}: {GcByteHelper.BytesToHexString(opcodes)}");
            var batches = await TtnFirmwareManager.TransferOpCode(config.DeviceId, _appId, opcodes, _commonMqttClientTtn).ConfigureAwait(false);
            _downlinkDebouce = batches + 1;
            _lastOpCode = opcodes;
            return true;
        }

        private static event EventHandler<MqttApplicationMessageReceivedEventArgs>? OnNewMqttMessage;

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
                    Logging.Log.LogTrace($"[{nameof(GatewayComTtn)}]({nameof(WatchdogWorker)}): Watchdogworker Stopped");
                }
            }
        }

        private static async Task Init(GcTtn ttnConfig)
        {
            if (_initialized)
            {
                return;
            }

            await InitCommonMqttClient(ttnConfig).ConfigureAwait(false);

            _initialized = true;
        }

        private async void OnNewMqttMessageEventHandler(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                await HandleMqttMessageAsync(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Log.LogError(ex, $"[{nameof(GatewayComTtn)}]({nameof(OnNewMqttMessageEventHandler)}): Error while handeling MqttMessage: {ex.Message}\n Stacktrace: {ex.StackTrace}");
            }
        }

        private static async Task InitCommonMqttClient(GcTtn config)
        {
            if (_commonMqttClientTtn is null)
            {
                _commonMqttClientTtn = (new MqttFactory()).CreateManagedMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(config.Zone, 1883).WithCredentials(
                        config.Applicationid + "@ttn", config.ApiKey)
                    .Build();

                var managedOptions = new ManagedMqttClientOptionsBuilder().WithClientOptions(options).WithAutoReconnectDelay(TimeSpan.FromSeconds(3)).Build();

                await _commonMqttClientTtn.StartAsync(managedOptions).ConfigureAwait(true);

                _topicFilters = new[] {new MqttTopicFilterBuilder().WithTopic($"v3/{config.Applicationid}@ttn/#").WithExactlyOnceQoS().Build()};
                await _commonMqttClientTtn.SubscribeAsync(_topicFilters).ConfigureAwait(false);


                _commonMqttClientTtn.ApplicationMessageReceivedAsync += CommonMqttClientTtnOnApplicationMessageReceivedAsync;
            }
        }

        private static Task CommonMqttClientTtnOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Logging.Log.LogTrace($"[{nameof(GatewayComTtn)}]({nameof(InitCommonMqttClient)}): MqttMessage received: Payload: {arg.ApplicationMessage.Payload} Topic: {arg.ApplicationMessage.Topic}");
#pragma warning restore CS0618 // Type or member is obsolete
            OnNewMqttMessage?.Invoke(null, arg);
            return Task.CompletedTask;
        }
    }
}