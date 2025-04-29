// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BDA.Common.Exchange.GatewayService.TTN
{
    /// <summary>
    ///     <para>Abstraktion für einen TtnFrame (Uplink Nachricht)</para>
    ///     Klasse ExTtnFrame. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExTtnUplinkFrame
    {
        private readonly dynamic _dynamic;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="json"></param>
        public ExTtnUplinkFrame(string json)
        {
            _dynamic = JObject.Parse(json);
        }

        #region Properties

        /// <summary>
        ///     Anzahl der Messungszyklen. (Byte [0])
        /// </summary>
        public ushort FirmwareVersion => GetFramePayloadBytes().Take(2).Count() == 2 ? BitConverter.ToUInt16(GetFramePayloadBytes().Take(2).ToArray()) : (ushort) 0;

        /// <summary>
        ///     Der Timestamp wann die Nachricht angekommen ist als String (UTC). Format:yyyy-MM-ddTHH:mm:ss.fffffffffZ
        /// </summary>
        public string ReceivedAtString => _dynamic.received_at;

        /// <summary>
        ///     Der Timestamp wann die Nachricht angekommen ist als DateTime.
        /// </summary>
        public DateTime ReceivedAtTimestamp => DateTime.ParseExact(ReceivedAtString.Substring(0, ReceivedAtString.Length - 3), "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);


        /// <summary>
        ///     Payload der Nachricht Base64 kodiert.
        /// </summary>
        public string FramePayloadBase64 => _dynamic.uplink_message.frm_payload;

        /// <summary>
        ///     Payload der Nachricht als hexstring
        /// </summary>
        public string FramePayloadHexString => string.Join(' ', GetFramePayloadBytes().Select(b => b.ToString("X2")));

        /// <summary>
        ///     Die Deviceid des IoTGeräts von dem die Nachricht stammt.
        /// </summary>
        public string DeviceId => _dynamic.end_device_ids.device_id;

        /// <summary>
        ///     Die ApplikationID der Nachricht.
        /// </summary>
        public string ApplicationId => _dynamic.end_device_ids.application_ids.application_id;

        /// <summary>
        ///     FramePort
        /// </summary>
        public int FramePort => _dynamic.uplink_message.f_port ?? 0;

        #endregion


        /// <summary>
        ///     Gibt die rohen Bytes des Payloads zurück
        /// </summary>
        /// <returns></returns>
        public byte[] GetFramePayloadBytes()
        {
            return Convert.FromBase64String(FramePayloadBase64);
        }


        /// <summary>
        ///     Gibt die rohen Messungsbytes zurück. Hierbei wird das erste Byte entfernt und als Anzahl der Messungszyklen
        ///     interpretiert.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMeasurementData()
        {
            return GetFramePayloadBytes().Skip(2).ToArray();
        }
    }
}