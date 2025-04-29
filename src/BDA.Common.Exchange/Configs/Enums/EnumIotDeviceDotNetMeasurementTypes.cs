// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Mögliche Messwerte für Donstream Type DotNet und PI</para>
    ///     Klasse EnumIotDeviceDownstreamTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDeviceDotNetMeasurementTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Benötigter RAM
        /// </summary>
        Memory = 0x01,

        /// <summary>
        ///     Auslastung der CPU
        /// </summary>
        CpuUsagePercent = 0x02
    }
}