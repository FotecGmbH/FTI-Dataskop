// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Unterstützte Iot Device Plattformen</para>
    ///     Klasse EnumIotDeviceTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDevicePlattforms
    {
        /// <summary>
        ///     Ein DotNet basieredes IoT Device (läuft auf Windows, Linux und MacOs)
        /// </summary>
        DotNet,

        /// <summary>
        ///     Ein auf DotNet basierender Linux-Pi
        /// </summary>
        RaspberryPi,

        /// <summary>
        ///     Arduino Plattform
        /// </summary>
        Arduino,

        /// <summary>
        ///     Esp32 Plattform inkl. LoPy
        /// </summary>
        Esp32,

        /// <summary>
        ///     3rd-Party Sensor
        /// </summary>
        Prebuilt,

        /// <summary>
        ///     OpenSense Sensor
        /// </summary>
        OpenSense,

        /// <summary>
        ///     Meadow Sensor
        /// </summary>
        Meadow
    }
}