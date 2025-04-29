// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
// ReSharper disable UnusedParameter.Local

namespace BDA.Service.Com.MQTT
{
    /// <summary>
    ///     <para>Options für den Mqttservice</para>
    ///     Klasse MQTTServiceOptions. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class MqttServiceOptions
    {
        #region Properties

        /// <summary>
        ///     Der Port des MQTT Servers
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        ///     Prefix für die Topics
        /// </summary>
        public string TopicPrefix { get; set; } = "/BDA";

        /// <summary>
        ///     Funktion die Aufgerufen wird um zu prüfen ob ein User berechtigt ist sich zu verbinden
        /// </summary>
        public Func<string, string, IServiceProvider, Task<bool>> UserValidationFunction { get; set; } = (s, s1, serviceProvider) => Task.FromResult(true);

        /// <summary>
        ///     Funktion die aufgerufen wird um zu prüfen ob ein User berechtigt ist ein Topic zu abonnieren
        /// </summary>
        public Func<string, string, IServiceProvider, Task<bool>> TopicValidationFunction { get; set; } = (clientId, topic, serviceProvider) => Task.FromResult(true);

        #endregion
    }
}