// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BDA.Common.Exchange.GatewayService.Drei
{
    /// <summary>
    ///     <para>Dreu Uplinkdata</para>
    ///     Klasse ExDreiUplinkFrame. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDreiUplinkFrame
    {
        private readonly dynamic _dynamic;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="json"></param>
        public ExDreiUplinkFrame(string json)
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
        public string ReceivedAtString => _dynamic.DevEUI_uplink.Time;

        /// <summary>
        ///     Der Timestamp wann die Nachricht angekommen ist als DateTime.
        /// </summary>
        public DateTime ReceivedAtTimestamp => DateTime.Parse(ReceivedAtString);


        /// <summary>
        ///     Payload der Nachricht als hexstring
        /// </summary>
        public string FramePayloadHexString => _dynamic.DevEUI_uplink.payload_hex;

        /// <summary>
        ///     Die Deviceid des IoTGeräts von dem die Nachricht stammt.
        /// </summary>
        public string DeviceId => _dynamic.DevEUI_uplink.CustomerData.name;

        /// <summary>
        ///     FramePort
        /// </summary>
        public string DevAddr => _dynamic.DevEUI_uplink.DevAddr;

        /// <summary>
        ///     FramePort
        /// </summary>
        public string DevEui => _dynamic.DevEUI_uplink.DevEUI;

        /// <summary>
        ///     FramePort
        /// </summary>
        public int FramePort => _dynamic.DevEUI_uplink.FPort ?? 0;

        #endregion


        /// <summary>
        ///     Gibt die rohen Bytes des Payloads zurück
        /// </summary>
        /// <returns></returns>
        public byte[] GetFramePayloadBytes()
        {
            return HexToBytes(Split(FramePayloadHexString, 2));
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

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        private byte[] HexToBytes(IEnumerable<string> hexValues)
        {
            var bytes = new List<byte>();
            foreach (var hex in hexValues)
            {
                // Convert the number expressed in base-16 to an integer.
                var value = Convert.ToByte(hex, 16);
                bytes.Add(value);
            }

            return bytes.ToArray();
        }
    }
}