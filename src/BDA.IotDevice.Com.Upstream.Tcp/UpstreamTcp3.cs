// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Upstream.Base;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Logging;

namespace BDA.IotDevice.Com.Upstream.Tcp
{
    /// <summary>
    ///  For sensor which every time sending something, starts a new connection
    /// </summary>
    public class UpstreamTcp3 : UpstreamBase
    {
        /// <summary>
        ///     hostname des gateways (=>_ipAvail = false)
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private readonly string _gatewayHostName;

        /// <summary>
        ///     Ipaddresse des gateways (=>ipAvail = true)
        /// </summary>
        private readonly IPAddress _gatewayIpAddress;

        private readonly byte[] _secretBytes = new byte[36];

        private readonly List<byte> _sendMeasurementResults = new List<byte>();

        private ExGwServiceIotDeviceConfig _deviceConfig;

        private bool _initialized;

        private byte _measurementCycles;

        private OpcodeHandler _opcodeHandler;

        private int _port;

        private TcpWorker _tcpWorker;

        private bool _thresholdCheck;

        private long _timeSinceLastTransmit;
        /// <summary>
        /// Exit
        /// </summary>
        public bool Exit = false;

        /// <summary>
        /// UpstreamTcp3
        /// </summary>
        /// <param name="deviceConfig"></param>
        /// <param name="gatewayIpAddress"></param>
        /// <param name="port"></param>
#pragma warning disable CS8618, CS9264
        public UpstreamTcp3(ExGwServiceIotDeviceConfig deviceConfig, IPAddress gatewayIpAddress, int port) : base(EnumIotDeviceUpstreamTypes.Tcp)
#pragma warning restore CS8618, CS9264
        {
            _gatewayIpAddress = gatewayIpAddress;
            Init(deviceConfig, port);
        }

        /// <summary>
        /// UpstreamTcp3
        /// </summary>
        /// <param name="deviceConfig"></param>
        /// <param name="gatewayHostname"></param>
        /// <param name="port"></param>
#pragma warning disable CS8618, CS9264
        public UpstreamTcp3(ExGwServiceIotDeviceConfig deviceConfig, string gatewayHostname, int port) : base(EnumIotDeviceUpstreamTypes.Tcp)
#pragma warning restore CS8618, CS9264
        {
            _gatewayHostName = gatewayHostname;
            Init(deviceConfig, port);
        }

        #region Properties

        /// <summary>
        ///     For scenario that more than 1 iotdevice gets started "configaddress" shouldnt be the same
        /// </summary>
        private string ConfigurationPathForThisIoTDevice
        {
            get => _deviceConfig.Name + "Configuration.txt";
        }

        #endregion

        /// <summary>
        /// TransferData
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<bool> TransferData(List<ExValue> data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Main loop for processing the iot device tasks
        /// </summary>
        /// <returns></returns>
        public async Task MainLoop()
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            if (_gatewayIpAddress == null)
            {
                throw new ArgumentException(nameof(_gatewayIpAddress));
            }

            await SetDeviceConfigurationFromFileOrTcp(_gatewayIpAddress, _port).ConfigureAwait(true);

            while (!Exit)
            {
                stopWatch.Restart();

                _initialized = false;
                // ReSharper disable once RedundantAssignment
                var readBuffer = new byte[200];

                InvokeMeasureProcess();

                // for seconds
                _timeSinceLastTransmit += _deviceConfig.MeasurementInterval / 10;

                readBuffer = await InvokeSendMeasureResultsThenGetProcess().ConfigureAwait(true);

                if (readBuffer.Length > 0)
                {
                    await HandleDownlink(readBuffer);
                }

                stopWatch.Stop();

                // sleepInterval
                await Task.Delay(Math.Max(_deviceConfig.MeasurementInterval * 100 - (int) stopWatch.ElapsedMilliseconds, 1)).ConfigureAwait(true);
            }
        }

        private void Init(ExGwServiceIotDeviceConfig deviceConfig, int port)
        {
            _deviceConfig = deviceConfig;
            Encoding.UTF8.GetBytes(deviceConfig.Secret.ToCharArray(), 0, deviceConfig.Secret.Length, _secretBytes, 0);
            _tcpWorker = new TcpWorker();
            _opcodeHandler = new OpcodeHandler();
            _tcpWorker.Init(_gatewayIpAddress, 802);
            _port = port;
        }

        /// <summary>
        ///     Handles downlink message
        /// </summary>
        /// <param name="buffer">buffer which comes from gateway</param>
        private async Task HandleDownlink(byte[] buffer)
        {
            await SetDeviceConfiguration(buffer).ConfigureAwait(true);
        }

        /// <summary>
        ///     Invoking the send process with first checking if it needs to send now
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<byte[]> InvokeSendMeasureResultsThenGetProcess( /*IPAddress gatewayIpAddress, int tcpPort*/)
        {
            var readBuffer = new byte[200];

            if (_thresholdCheck || _deviceConfig.TransmissionTime == EnumTransmission.Instantly || (_deviceConfig.TransmissionTime == EnumTransmission.Elapsedtime && _deviceConfig.TransmissionInterval <= _timeSinceLastTransmit) || (_deviceConfig.TransmissionTime == EnumTransmission.NumberOfMeasurements && _measurementCycles >= _deviceConfig.TransmissionInterval)) // TODO not t
            {
                var sendData = new List<byte>();
                sendData.Add(15); //.Concat(_secretBytes));
                sendData.AddRange(GetMetadata());

                sendData.AddRange(_sendMeasurementResults);

                try
                {
                    readBuffer = await _tcpWorker.TcpWorkerSendGet(_gatewayIpAddress, _port, sendData.ToArray()).ConfigureAwait(true);
                    Logging.Log.LogTrace($"[{nameof(UpstreamTcp)}]({nameof(MainLoop)}): Sent: " + _sendMeasurementResults.Count + " Bytes of measure-results to gateway");
                }
                catch (Exception)
                {
                    Logging.Log.LogError($"[{nameof(UpstreamTcp)}]({nameof(MainLoop)}): couldn't send TCP message");
                }

                _timeSinceLastTransmit = 0; // reset time
                _sendMeasurementResults.Clear();
                _measurementCycles = 0;

                if (readBuffer.Length != 0)
                {
                    // Only for representation / Logging, only display till all other elements are zereos
                    Logging.Log.LogTrace($"[{nameof(UpstreamTcp)}]({nameof(MainLoop)}): Received message bytes: " + String.Join("-", readBuffer.Take(Array.FindLastIndex(readBuffer, b => b != 0))));
                    return readBuffer;
                }

                _thresholdCheck = false;
            }

            return Array.Empty<byte>();
        }

        private void InvokeMeasureProcess()
        {
            byte gpsAvailable = 0;

            // Measure process
            _measurementCycles++;
            _sendMeasurementResults.Add(gpsAvailable);
            (List<byte>, bool) res = new ValueTuple<List<byte>, bool>(new List<byte>(), false);
            try
            {
                res = _opcodeHandler.WorkOpcodes(_deviceConfig);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
            }

            _sendMeasurementResults.AddRange(res.Item1);
            _thresholdCheck = res.Item2;
            if (_thresholdCheck)
            {
                Logging.Log.LogTrace($"[{nameof(UpstreamTcp)}]({nameof(InvokeMeasureProcess)}): triggered value");
            }
        }


        /// <summary>
        ///     If the config file exists and i can read the config from it
        ///     i will set the device configuration, otherwise i will try to
        ///     get it through tcp from the gateway and write it to the config file
        /// </summary>
        /// <returns>task</returns>
        private async Task SetDeviceConfigurationFromFileOrTcp(IPAddress gatewayIpAddress, int tcpPort)
        {
            // try getting configuration from file 
            if (File.Exists(ConfigurationPathForThisIoTDevice))
            {
                try
                {
                    var configText = await File.ReadAllTextAsync(ConfigurationPathForThisIoTDevice).ConfigureAwait(true);
                    _opcodeHandler.SetConfig(_deviceConfig, BissDeserialize.FromJson<byte[]>(configText));
                }
                catch (Exception)
                {
                    Logging.Log.LogError($"[{nameof(UpstreamTcp)}]({nameof(MainLoop)}):Couldn't read config from existing config file");
                }

                _ = await _tcpWorker.TcpWorkerSendGet(gatewayIpAddress, tcpPort, (new[] {(byte) 9, (byte) _secretBytes.Length}).Concat(_secretBytes).ToArray()).ConfigureAwait(true);

                _initialized = true;
            }

            while (!_initialized) // send message to get config
            {
                // ReSharper disable once RedundantAssignment
                var readBuffer = new byte[200];

                // get configuration     // not initializedPort
                readBuffer = await _tcpWorker.TcpWorkerSendGet(gatewayIpAddress, tcpPort, (new[] {(byte) 11, (byte) _secretBytes.Length}).Concat(_secretBytes).ToArray()).ConfigureAwait(true);
                // Only for representation / Logging, only display till all other elements are zereos
                Logging.Log.LogTrace($"[{nameof(UpstreamTcp)}]({nameof(MainLoop)}): Received message bytes: " + String.Join("-", readBuffer.Take(Array.FindLastIndex(readBuffer, b => b != 0))));

                if (readBuffer.Length > 0)
                {
                    await SetDeviceConfiguration(readBuffer).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        ///     set config and write it to the config file
        /// </summary>
        /// <returns>task</returns>
        private async Task SetDeviceConfiguration(byte[] opCodes)
        {
            _opcodeHandler.SetConfig(_deviceConfig, opCodes);
            _initialized = true;
            await File.WriteAllTextAsync(ConfigurationPathForThisIoTDevice, opCodes.ToJson()).ConfigureAwait(true);
        }

        private List<byte> GetMetadata()
        {
            var result = new List<byte>();

            result.Add(_measurementCycles);

            return result;
        }
    }
}