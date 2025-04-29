// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Newtonsoft.Json.Linq;

namespace BDA.Common.Exchange.GatewayService.TTN
{
    /// <summary>
    ///     <para>Abstraktion für einen TtnFrame (Join Nachricht)</para>
    ///     Klasse ExTtnJoinFrame. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExTtnJoinFrame
    {
        private readonly dynamic _dynamic;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="json"></param>
        public ExTtnJoinFrame(string json)
        {
            _dynamic = JObject.Parse(json);
        }

        #region Properties

        /// <summary>
        ///     Die Deviceid des IoTGeräts von dem die Nachricht stammt.
        /// </summary>
        public string DeviceId => _dynamic.end_device_ids.device_id;

        /// <summary>
        ///     Die ApplikationID der Nachricht.
        /// </summary>
        public string ApplicationId => _dynamic.end_device_ids.application_ids.application_id;

        /// <summary>
        ///     Die DevAddr des IoTGeräts von dem die Nachricht stammt.
        /// </summary>
        public string DeviceAddr => _dynamic.end_device_ids.dev_addr;

        #endregion
    }
}