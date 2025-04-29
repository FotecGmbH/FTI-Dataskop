// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams.Esp32;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>
    ///         Wenn die Plattform eines Iot Geräts "Esp32" ist - Welche Downstream Typen werden unterstützt und welche
    ///         "buildid" Messwert definitionen besitzt dieser?
    ///     </para>
    ///     Klasse ConfigPlattformDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcPlattformEsp32 : ConfigPlattformBase
    {
        /// <summary>
        ///     Wenn die Plattform eines Iot Geräts "Esp32" ist?
        ///     Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        /// </summary>
        public GcPlattformEsp32() : base(EnumIotDevicePlattforms.Esp32)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Esp32);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Virtual);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.I2C);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Modbus);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Spi);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Custom);


            var m1 = new GcDownstreamEsp32
            {
                ValueName = "Analog Digital Converter 1 Channel 4 (uint 12bit)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Adc1Ch4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m2 = new GcDownstreamEsp32
            {
                ValueName = "Analog Digital Converter 1 Channel 5 (uint 12bit)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Adc1Ch5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m3 = new GcDownstreamEsp32
            {
                ValueName = "Analog Digital Converter 1 Channel 6 (uint 12bit)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Adc1Ch6,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m4 = new GcDownstreamEsp32
            {
                ValueName = "Analog Digital Converter 1 Channel 7 (uint 12bit)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Adc1Ch7,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m6 = new GcDownstreamEsp32
            {
                ValueName = "Internal Halleffect sensor",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.InternalHalleffect,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m7 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 0 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio0,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m8 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 1 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio1,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m9 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 2 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio2,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m10 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 3 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio3,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m11 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 4 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m12 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 5 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m13 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 12 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio12,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m14 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 13 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio13,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m15 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 14 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio14,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m16 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 15 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio15,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m17 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 18 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio18,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m18 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 19 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio19,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m19 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 21 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio21,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m20 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 22 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio22,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m21 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 23 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio23,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m22 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 25 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio25,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m23 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 26 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio26,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m24 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 27 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio27,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m25 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 32 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio32,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m26 = new GcDownstreamEsp32
            {
                ValueName = "Gpio 33 (bool)",
                Esp32MeasurementType = EnumIotDeviceEsp32MeasurementTypes.Gpio33,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };


            BuildInExMeasurementDefinitions.Add(m1.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m2.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m3.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m4.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m6.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m7.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m8.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m9.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m10.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m11.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m12.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m13.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m14.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m15.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m16.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m17.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m18.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m19.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m20.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m21.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m22.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m23.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m24.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m25.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m25.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m26.ToExMeasurementDefinition());
        }
    }
}