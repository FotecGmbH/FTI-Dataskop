// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Logging;

namespace BDA.IotDevice.Com.Upstream.Tcp
{
    // For sensor which every time sending something, starts a new connection
    /// <summary>
    ///     <para>Handling opcodes and their parsing</para>
    ///     Klasse OpcodeHandler. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class OpcodeHandler
    {
        private readonly ThresholdHandler _thresholdHandler = new ThresholdHandler();

        /// <summary>
        ///     Sets the given config with the given buffer
        /// </summary>
        /// <param name="deviceConfig">config to set</param>
        /// <param name="buffer">buffer</param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetConfig(ExGwServiceIotDeviceConfig deviceConfig, byte[] buffer)
        {
            if (deviceConfig == null)
            {
                throw new ArgumentNullException(nameof(deviceConfig) + "was null");
            }

            deviceConfig.TransmissionTime = (EnumTransmission) buffer.First();
            deviceConfig.TransmissionInterval = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray());
            deviceConfig.MeasurementInterval = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray());
            deviceConfig.ConfigVersionService = BitConverter.ToUInt16(buffer.Skip(9).Take(2).ToArray());

            buffer = buffer.Skip(11).ToArray(); // Skip assigned configuration

            deviceConfig.MeasurementDefinition.Clear();

            while (buffer.Any(b => b != 0))
            {
                var dsType = (EnumIotDeviceDownstreamTypes) buffer[0];
                var payLoadLength = buffer[1];


                var newM = new ExGwServiceMeasurementDefinitionConfig();
                newM.DownstreamType = dsType;

                var oct = buffer[2];

                var threshholdCount = buffer[3];
                buffer = buffer.Skip(4).ToArray();

                for (var i = 0; i < threshholdCount; i++)
                {
                    var typ = (EnumThresholdType) buffer[i * 5];
                    var val = BitConverter.ToSingle(buffer, 1 + (i * 5));

                    switch (typ)
                    {
                        case EnumThresholdType.Exceed:
                            newM.IfThresholdExceed = true;
                            newM.ThresholdExceedValue = val;
                            break;
                        case EnumThresholdType.FallBelow:
                            newM.IfThresholdFallBelow = true;
                            newM.ThresholdFallBelowValue = val;
                            break;
                        case EnumThresholdType.Delta:
                            newM.IfThresholdDelta = true;
                            newM.ThresholdDeltaValue = val;
                            break;
                    }
                }

                buffer = buffer.Skip(threshholdCount * 5).ToArray();

                newM.AdditionalConfiguration = (new[] {oct}.Concat(buffer.Take(payLoadLength - 2 - threshholdCount * 5))).ToArray().ToJson(); //.ToJson();
                //Console.WriteLine(newM.AdditionalConfiguration);
                deviceConfig.MeasurementDefinition.Add(newM);
                buffer = buffer.Skip(payLoadLength - 2 - threshholdCount * 5).ToArray(); // + 2 = payloadlength byte and dsType Byte
            }
        }

        /// <summary>
        ///     Handles the opcodes from the config
        /// </summary>
        /// <param name="deviceConfig">config</param>
        /// <exception cref="NotImplementedException"></exception>
        public (List<byte>, bool) WorkOpcodes(ExGwServiceIotDeviceConfig deviceConfig)
        {
            if (deviceConfig == null)
            {
                throw new ArgumentException("no config");
            }

            var result = new List<byte>();

            (bool, EnumThresholdType) check = (false, EnumThresholdType.None);

            var counter = 0;

            foreach (var dc in deviceConfig.MeasurementDefinition)
            {
                var opCode = BissDeserialize.FromJson<byte[]>(dc.AdditionalConfiguration);

                switch (dc.DownstreamType)
                {
                    case EnumIotDeviceDownstreamTypes.Virtual:
                        var v = (EnumIotDeviceVirtualMeasurementTypes) opCode[0];

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                        if (v == null!)
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                        {
                            break;
                        }

                        switch (v)
                        {
                            case EnumIotDeviceVirtualMeasurementTypes.Float:
                                var min = BitConverter.ToSingle(opCode.Skip(1).Take(4).ToArray());
                                var max = BitConverter.ToSingle(opCode.Skip(5).Take(4).ToArray());
                                var randFloat = (float) (new Random((int) DateTime.UtcNow.Ticks % int.MaxValue)).NextDouble() * (max - min) + min;
                                Logging.Log.LogTrace($"[{nameof(OpcodeHandler)}]({nameof(WorkOpcodes)}): Created with min : " + min + " max: " + max + " number: " + randFloat);
                                result.AddRange(BitConverter.GetBytes(randFloat));
                                dc.DbId = counter++;
                                var checkVir = _thresholdHandler.CheckThreshold(dc, randFloat);
                                if (checkVir.Item1)
                                {
                                    check = checkVir;
                                }

                                break;
                            case EnumIotDeviceVirtualMeasurementTypes.Image:
                                throw new NotImplementedException();
                            case EnumIotDeviceVirtualMeasurementTypes.Text:
                                throw new NotImplementedException();
                            case EnumIotDeviceVirtualMeasurementTypes.Data:
                                throw new NotImplementedException();
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case EnumIotDeviceDownstreamTypes.I2C:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Spi:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Modbus:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Pi:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Arduino:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Esp32:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.DotNet:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Custom:
                        var oct = opCode[0];

                        switch (oct)
                        {
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                            case 5:
                                var randFloat = (float) (new Random((int) DateTime.UtcNow.Ticks % int.MaxValue)).NextDouble();
                                Logging.Log.LogTrace($"[{nameof(OpcodeHandler)}]({nameof(WorkOpcodes)}): Created custom code 5");
                                result.AddRange(BitConverter.GetBytes(randFloat));
                                dc.DbId = counter++;
                                var checkCus = _thresholdHandler.CheckThreshold(dc, randFloat);
                                if (checkCus.Item1)
                                {
                                    check = checkCus;
                                }

                                break;
                        }

                        break;
                    case EnumIotDeviceDownstreamTypes.OpenSense:
                        throw new NotImplementedException();
                    case EnumIotDeviceDownstreamTypes.Meadow:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (result.Count != 0)
            {
                var timeDiff = DateTime.UtcNow - DateTime.UnixEpoch;
                var timeDiffInt = (UInt32) timeDiff.TotalSeconds;
                var timeDiffBytes = BitConverter.GetBytes(timeDiffInt);
                result.AddRange(timeDiffBytes);
            }

            return (result, check.Item1);
        }
    }
}