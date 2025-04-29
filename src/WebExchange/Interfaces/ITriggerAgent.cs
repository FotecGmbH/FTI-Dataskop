// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Model.ConfigApp;

namespace WebExchange.Interfaces
{
    /// <summary>
    ///     <para>Inteface für den TriggerAgent</para>
    ///     Interface ITriggerAgent. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface ITriggerAgent
    {
        /// <summary>
        ///     Gatewaydaten wurden verändert
        /// </summary>
        /// <param name="source">Wer hat die Änderung gemacht?</param>
        /// <param name="gatewayId">Welches Gateway</param>
        Task ChangedGateway(EnumTriggerSources source, long gatewayId);

        /// <summary>
        ///     Neue Daten vom Gateway wurden in DB gesichert
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="measurementDefinitionIds"></param>
        Task NewMeasurementsFromGateway(long gatewayId, List<long> measurementDefinitionIds);

        /// <summary>
        ///     Neue Daten vom Gateway wurden in DB gesichert
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="values"></param>
        Task NewMeasurementsFromGateway(long gatewayId, List<(long, ExMeasurement)> values);

        /// <summary>
        ///     Status eines Iot Gerätes (Online/Offline/Configversion/Firmwareverseion ...) haben sich geändert
        /// </summary>
        /// <param name="iotDeviceId"></param>
        Task IotDeviceStatusChanged(long iotDeviceId);

        /// <summary>
        ///     Downlink Message an Iot device ueber Gateway senden
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="iotDeviceId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendDownlinkMessage(long gatewayId, long iotDeviceId, byte[] message);
    }
}