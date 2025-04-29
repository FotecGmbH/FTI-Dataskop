// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Microsoft.Extensions.DependencyInjection;

namespace BDA.Service.Com.MQTT
{
    /// <summary>
    ///     <para>MQTTService-Extensions</para>
    ///     Klasse Extensions. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class MqttServiceExtensions
    {
        /// <summary>
        ///     Adds the MWTTService with the given options
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        public static void AddMqttService(this IServiceCollection services, Action<MqttServiceOptions> optionsAction)
        {
            services.Configure(optionsAction);
            services.AddSingleton<IMqttService, MqttService>();
        }

        /// <summary>
        ///     Adds the MQTTService with default options
        /// </summary>
        /// <param name="services"></param>
        public static void AddMqttService(this IServiceCollection services)
        {
            services.Configure<MqttServiceOptions>(_ => { });
            services.AddSingleton<IMqttService, MqttService>();
        }
    }
}