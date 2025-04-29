// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams.Pi;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>
    ///         Wenn die Plattform eines Iot Geräts "Arduino" ist - Welche Downstream Typen werden unterstützt und welche
    ///         "buildid" Messwert definitionen besitzt dieser?
    ///     </para>
    ///     Klasse ConfigPlattformDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcPlatformPi : ConfigPlattformBase
    {
        /// <summary>
        ///     Wenn die Plattform eines Iot Geräts "Arduino" ist?
        ///     Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        /// </summary>
        public GcPlatformPi() : base(EnumIotDevicePlattforms.RaspberryPi)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Pi);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Virtual);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.I2C);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Modbus);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Spi);

            var m1 = new GcDownstreamPi
            {
                ValueName = "Gpio 2 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio2,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m2 = new GcDownstreamPi
            {
                ValueName = "Gpio 3 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio3,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m3 = new GcDownstreamPi
            {
                ValueName = "Gpio 4 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m4 = new GcDownstreamPi
            {
                ValueName = "Gpio 5 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m5 = new GcDownstreamPi
            {
                ValueName = "Gpio 6 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio6,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m6 = new GcDownstreamPi
            {
                ValueName = "Gpio 7 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio7,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m7 = new GcDownstreamPi
            {
                ValueName = "Gpio 8 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio8,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m8 = new GcDownstreamPi
            {
                ValueName = "Gpio 9 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio9,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m9 = new GcDownstreamPi
            {
                ValueName = "Gpio 10 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio10,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m10 = new GcDownstreamPi
            {
                ValueName = "Gpio 11 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio11,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m11 = new GcDownstreamPi
            {
                ValueName = "Gpio 12 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio12,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m12 = new GcDownstreamPi
            {
                ValueName = "Gpio 13 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio13,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m15 = new GcDownstreamPi
            {
                ValueName = "Gpio 16 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio16,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m16 = new GcDownstreamPi
            {
                ValueName = "Gpio 17 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio17,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m17 = new GcDownstreamPi
            {
                ValueName = "Gpio 18 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio18,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m18 = new GcDownstreamPi
            {
                ValueName = "Gpio 19 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio19,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m19 = new GcDownstreamPi
            {
                ValueName = "Gpio 20 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio20,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m20 = new GcDownstreamPi
            {
                ValueName = "Gpio 21 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio21,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m21 = new GcDownstreamPi
            {
                ValueName = "Gpio 22 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio22,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m22 = new GcDownstreamPi
            {
                ValueName = "Gpio 23 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio23,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m23 = new GcDownstreamPi
            {
                ValueName = "Gpio 24 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio24,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m24 = new GcDownstreamPi
            {
                ValueName = "Gpio 25 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio25,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m25 = new GcDownstreamPi
            {
                ValueName = "Gpio 26 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio26,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m26 = new GcDownstreamPi
            {
                ValueName = "Gpio 27 (bool)",
                PiMeasurementType = EnumIotDevicePiMeasurementTypes.Gpio27,
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
            BuildInExMeasurementDefinitions.Add(m5.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m6.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m7.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m8.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m9.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m10.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m11.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m12.ToExMeasurementDefinition());
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
            BuildInExMeasurementDefinitions.Add(m26.ToExMeasurementDefinition());
        }
    }
}