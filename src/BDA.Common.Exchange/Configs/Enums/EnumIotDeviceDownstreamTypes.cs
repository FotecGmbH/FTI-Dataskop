// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Protokolle auf Iot Device Ebene</para>
    ///     Klasse EnumIotDeviceDownstreamTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDeviceDownstreamTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Anbindung eines virtuellen Sensors
        /// </summary>
        Virtual = 0x01,

        /// <summary>
        ///     Anbindung via I2C Bus
        /// </summary>
        I2C = 0x02,

        /// <summary>
        ///     Anbindung via SPI Bus
        /// </summary>
        Spi = 0x03,

        /// <summary>
        ///     Anbindung via Modbus Bus
        /// </summary>
        Modbus = 0x04,

        /// <summary>
        ///     Anbindung der "buildin" des Pi
        /// </summary>
        Pi = 0x05,

        /// <summary>
        ///     Anbindung der "buildin" des Arduino
        /// </summary>
        Arduino = 0x06,

        /// <summary>
        ///     Anbindung der "builtin" des Esp8266
        /// </summary>
        Esp32 = 0x07,

        /// <summary>
        ///     Anbindung der "buildin" eines PC'S (Mac oder Linux)
        /// </summary>
        DotNet = 0x08,

        /// <summary>
        ///     Custom Befehle (max 255) - müssen direkt in Firmware behandelt werden
        ///     Später durch EdgeComputing ersetzt
        /// </summary>
        Custom = 0x09,

        /// <summary>
        ///     Thirdparty sensor
        /// </summary>
        Prebuilt = 0x0A,

        /// <summary>
        ///     Opensense sensor
        /// </summary>
        OpenSense = 0x0B,

        /// <summary>
        ///     Meadow sensor
        /// </summary>
        Meadow = 0x0C
    }
}