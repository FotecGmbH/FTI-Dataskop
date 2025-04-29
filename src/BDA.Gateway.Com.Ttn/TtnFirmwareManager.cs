// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;

namespace BDA.Gateway.Com.Ttn
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse TtnFirmwareManager. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class TtnFirmwareManager
    {
        /// <summary>
        /// </summary>
        /// <param name="deviceId">TtnDeviceid</param>
        /// <param name="appid">TtnAppid</param>
        /// <param name="opcode">Opcode in bytes</param>
        /// <param name="client">MqttClient</param>
        /// <param name="batchsize">Größe der einzelnen Downlinks (Mindestwert 1)</param>
        /// <returns>Die Anzahl der Downlinks die nötig sind um den opcode zu senden. </returns>
        /// <exception cref="ArgumentNullException">Notwendiges Argument war null. Siehe Exception </exception>
        public static async Task<int> TransferOpCode(string deviceId, string appid, byte[] opcode, IManagedMqttClient client, byte batchsize = 40)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            if (appid == null)
            {
                throw new ArgumentNullException(nameof(appid));
            }

            if (opcode == null)
            {
                throw new ArgumentNullException(nameof(opcode));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (batchsize <= 1)
            {
                batchsize = 1;
            }

            var startIndex = 0;

            var first = true;

            Logging.Log.LogInfo($"[{nameof(TtnFirmwareManager)}]({nameof(TransferOpCode)}): Transfering Bytecode: {Convert.ToHexString(opcode)}");

            var batches = 0;

            while (opcode.Any())
            {
                var lastTransmission = opcode.Length < batchsize;
                var batch = opcode.Take(batchsize).Prepend((byte) startIndex);
                if (lastTransmission)
                {
                    batch = batch.Append((byte) 0x00);
                }

                await AppendDownlink(deviceId, appid, batch.ToArray(), (byte) (lastTransmission ? 11 : 10), client, true, first).ConfigureAwait(false);

                batches++;

                first = false;

                opcode = opcode.Skip(batchsize).ToArray();
                startIndex += batchsize;
            }

            return batches;
        }

        /// <summary>
        /// AppendDownlink
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="appid"></param>
        /// <param name="data"></param>
        /// <param name="port"></param>
        /// <param name="mqttClient"></param>
        /// <param name="confirmed"></param>
        /// <param name="deleteExisting"></param>
        /// <returns></returns>
        public static async Task AppendDownlink(string deviceid, string appid, byte[] data, byte port, IManagedMqttClient mqttClient, bool confirmed = false, bool deleteExisting = false)
        {
            var messagePayload = new {f_port = port, frm_payload = Convert.ToBase64String(data), priority = "NORMAL", confirmed};
            var payload = new {downlinks = new[] {messagePayload}};

            var json = JsonConvert.SerializeObject(payload);

            var sendCommand = deleteExisting ? "replace" : "push";

            var mqttMessage = new MqttApplicationMessageBuilder().WithTopic($"v3/{appid}@ttn/devices/{deviceid}/down/{sendCommand}").WithPayload(json).Build();

            try
            {
                await mqttClient.EnqueueAsync(mqttMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Log.LogError(ex, $"[{nameof(TtnFirmwareManager)}]({nameof(AppendDownlink)}): Error while Transfering Opcode");
            }
        }
    }
}