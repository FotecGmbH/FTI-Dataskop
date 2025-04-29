// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>
    ///         Wenn die Plattform eines Iot Geräts "DotNet" ist - Welche Downstream Typen werden unterstützt und welche
    ///         "buildid" Messwert definitionen besitzt dieser?
    ///     </para>
    ///     Klasse ConfigPlattformDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcPlattformDotNet : ConfigPlattformBase
    {
        /// <summary>
        ///     Wenn die Plattform eines Iot Geräts "DotNet" ist?
        ///     Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        /// </summary>
        public GcPlattformDotNet() : base(EnumIotDevicePlattforms.DotNet)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Virtual);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.DotNet);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Custom);

            var m1 = new GcDownstreamDotNet
            {
                ValueName = "CPU Auslastung (%)",
                DotNetMeasurementType = EnumIotDeviceDotNetMeasurementTypes.CpuUsagePercent,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 4,
                    RawValueType = EnumRawValueTypes.Float
                }
            };
            var m2 = new GcDownstreamDotNet
            {
                ValueName = "Ram belegt (%)",
                DotNetMeasurementType = EnumIotDeviceDotNetMeasurementTypes.Memory,
                RawValueDefinition = new ConfigRawValueDefinition
                {
                    ByteCount = 4,
                    RawValueType = EnumRawValueTypes.Float
                }
            };

            BuildInExMeasurementDefinitions.Add(m1.ToExMeasurementDefinition());
            BuildInExMeasurementDefinitions.Add(m2.ToExMeasurementDefinition());
        }
    }
}