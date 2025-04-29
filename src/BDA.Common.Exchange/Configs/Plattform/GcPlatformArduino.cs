// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams.Arduino;
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
    public class GcPlatformArduino : ConfigPlattformBase
    {
        /// <summary>
        ///     Wenn die Plattform eines Iot Geräts "Arduino" ist?
        ///     Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        /// </summary>
        public GcPlatformArduino() : base(EnumIotDevicePlattforms.Arduino)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Arduino);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Virtual);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.I2C);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Modbus);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Spi);
            var m1 = new GcDownstreamArduino
            {
                ValueName = "Analog 0 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A0,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m2 = new GcDownstreamArduino
            {
                ValueName = "Analog 1 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A1,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };
            var m3 = new GcDownstreamArduino
            {
                ValueName = "Analog 2 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A2,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };
            var m4 = new GcDownstreamArduino
            {
                ValueName = "Analog 3 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A3,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };
            var m5 = new GcDownstreamArduino
            {
                ValueName = "Analog 4 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };
            var m6 = new GcDownstreamArduino
            {
                ValueName = "Analog 5 (uint 10bit)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.A5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 2,
                    RawValueType = EnumRawValueTypes.UInt16
                }
            };

            var m9 = new GcDownstreamArduino
            {
                ValueName = "Gpio 2 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio2,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m10 = new GcDownstreamArduino
            {
                ValueName = "Gpio 3 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio3,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m11 = new GcDownstreamArduino
            {
                ValueName = "Gpio 4 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m12 = new GcDownstreamArduino
            {
                ValueName = "Gpio 5 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m13 = new GcDownstreamArduino
            {
                ValueName = "Gpio 6 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio6,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m14 = new GcDownstreamArduino
            {
                ValueName = "Gpio 7 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio7,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m15 = new GcDownstreamArduino
            {
                ValueName = "Gpio 8 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio8,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m16 = new GcDownstreamArduino
            {
                ValueName = "Gpio 9 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio9,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m17 = new GcDownstreamArduino
            {
                ValueName = "Gpio 10 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio10,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m18 = new GcDownstreamArduino
            {
                ValueName = "Gpio 11 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio11,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m19 = new GcDownstreamArduino
            {
                ValueName = "Gpio 12 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio12,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };

            var m20 = new GcDownstreamArduino
            {
                ValueName = "Gpio 13 (bool)",
                ArduinoMeasurementType = EnumIotDeviceArduinoMeasurementTypes.Gpio13,
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
        }
    }
}