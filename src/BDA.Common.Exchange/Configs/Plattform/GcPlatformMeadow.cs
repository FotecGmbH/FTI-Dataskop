// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams.Meadow;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>
    ///         Wenn die Plattform eines Iot Geräts "Meadow" ist - Welche Downstream Typen werden unterstützt und welche
    ///         "buildid" Messwert definitionen besitzt dieser?
    ///     </para>
    ///     Klasse GcPlatformMeadow. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcPlatformMeadow : ConfigPlattformBase
    {
        // <summary>
        /// Wenn die Plattform eines Iot Geräts "Meadow" ist?
        /// Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        // </summary>
        public GcPlatformMeadow() : base(EnumIotDevicePlattforms.Meadow)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Meadow);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Virtual);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Custom);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.I2C);


            var m1 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 0 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio0,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m2 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 1 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio1,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m3 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 2 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio2,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m4 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 3 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio3,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            // ReSharper disable once UnusedVariable
            var m5 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 4 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio4,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m6 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 5 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio5,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m7 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 6 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio6,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m8 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 7 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio7,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m9 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 8 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio8,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m10 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 9 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio9,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m11 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 10 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio10,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m12 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 12 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio12,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m13 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 13 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio13,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m14 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 14 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio14,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 1,
                    RawValueType = EnumRawValueTypes.Bit
                }
            };
            var m15 = new GcDownstreamMeadow
            {
                ValueName = "Gpio 15 (bool)",
                MeadowMeasurementType = EnumIotDeviceMeadowMeasurementType.Gpio15,
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
        }
    }
}