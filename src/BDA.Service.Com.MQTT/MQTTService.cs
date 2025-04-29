// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace BDA.Service.Com.MQTT
{
    /// <summary>
    ///     MQTT Service
    /// </summary>
    public class MqttService : IMqttService, IDisposable
    {
        private readonly MqttServiceOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private MqttServer? _mqttServer;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MqttService(IOptions<MqttServiceOptions> options, IServiceProvider serviceProvider)
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        #region Properties

        /// <summary>
        ///     Gibt an ob der Server läuft
        /// </summary>
        public bool IsRunning => _mqttServer is {IsStarted: true};

        #endregion

        #region Interface Implementations

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _mqttServer?.Dispose();
        }

        /// <summary>
        ///     Startet den MQTT Server
        /// </summary>
        public async Task StartAsync()
        {
            if (_mqttServer != null)
            {
                return;
            }

            var serverOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(_options.Port)
                .Build();

            _mqttServer = new MqttFactory().CreateMqttServer(serverOptions);

            // Prüfung ob der User sich verbinden darf
            _mqttServer.ValidatingConnectionAsync += async e => { e.ReasonCode = await _options.UserValidationFunction(e.UserName, e.Password, _serviceProvider).ConfigureAwait(false) ? MqttConnectReasonCode.Success : MqttConnectReasonCode.BadUserNameOrPassword; };

            // Prüfen ob der User das Topic abonnieren darf
            _mqttServer.InterceptingSubscriptionAsync += (async e => { e.Response.ReasonCode = await _options.TopicValidationFunction(e.ClientId, e.TopicFilter.Topic, _serviceProvider).ConfigureAwait(false) ? MqttSubscribeReasonCode.GrantedQoS0 : MqttSubscribeReasonCode.NotAuthorized; });

            await _mqttServer.StartAsync().ConfigureAwait(false);

            Logging.Log.LogInfo($"[{nameof(MqttService)}]({nameof(StartAsync)}): Mqttservices started");
        }

        /// <summary>
        ///     Sending the value per MQTT. Topic is the measurementDefinitionId
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="userToken"></param>
        /// <param name="measurementDefinitionId"></param>
        public async Task PublishMeasurementResultAsync(ExMeasurement value, string userToken, long measurementDefinitionId)
        {
            if (_mqttServer == null)
            {
                return;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_options.TopicPrefix + $"/newValue/{userToken}/{measurementDefinitionId}")
                .WithPayload(value.ToJson())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) {SenderClientId = "Server"}).ConfigureAwait(false);
        }

        /// <summary>
        ///     Stoppt den MQTT Server
        /// </summary>
        public async Task StopAsync()
        {
            if (_mqttServer == null)
            {
                return;
            }

            await _mqttServer.StopAsync().ConfigureAwait(false);
            _mqttServer.Dispose();
            _mqttServer = null;

            Logging.Log.LogInfo($"[{nameof(MqttService)}]({nameof(StartAsync)}): Mqttservices Stopped");
        }

        #endregion
    }
}