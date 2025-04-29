// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Threading.Tasks;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Service.Com.MQTT
{
    /// <summary>
    /// IMqttService
    /// </summary>
    public interface IMqttService
    {
        #region Properties

        /// <summary>
        /// IsRunning
        /// </summary>
        bool IsRunning { get; }

        #endregion

        /// <summary>
        /// PublishMeasurementResultAsync
        /// </summary>
        /// <param name="value"></param>
        /// <param name="userToken"></param>
        /// <param name="measurementDefinitionId"></param>
        /// <returns></returns>
        Task PublishMeasurementResultAsync(ExMeasurement value, string userToken, long measurementDefinitionId);
        /// <summary>
        /// StartAsync
        /// </summary>
        /// <returns></returns>
        Task StartAsync();
        /// <summary>
        /// StopAsync
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}