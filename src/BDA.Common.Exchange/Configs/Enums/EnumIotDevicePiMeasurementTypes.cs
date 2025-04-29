// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Mögliche Messwerte für Downstream Type Raspberry Pi</para>
    ///     Klasse EnumIotDeviceArduinoMeasurementTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDevicePiMeasurementTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Gpio Input Output / Default SDA
        /// </summary>
        Gpio2 = 0x02,

        /// <summary>
        ///     Gpio Input Output / Default SCL
        /// </summary>
        Gpio3 = 0x03,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio4 = 0x04,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio5 = 0x05,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio6 = 0x06,

        /// <summary>
        ///     Gpio Input Output / SPI0 CS0
        /// </summary>
        Gpio7 = 0x07,

        /// <summary>
        ///     Gpio Input Output / SPI0 CS1
        /// </summary>
        Gpio8 = 0x08,

        /// <summary>
        ///     Gpio Input Output / SPI0 MISO
        /// </summary>
        Gpio9 = 0x09,

        /// <summary>
        ///     Gpio Input Output / SPI0 MOSI
        /// </summary>
        Gpio10 = 0x0A,

        /// <summary>
        ///     Gpio Input Output / SPI0 SCLK
        /// </summary>
        Gpio11 = 0x0B,

        /// <summary>
        ///     Gpio Input Output / PWM
        /// </summary>
        Gpio12 = 0x0C,

        /// <summary>
        ///     Gpio Input Output / PWM
        /// </summary>
        Gpio13 = 0x0D,

        /// <summary>
        ///     Gpio Input Output / SPI1 CS2
        /// </summary>
        Gpio16 = 0x10,

        /// <summary>
        ///     Gpio Input Output / SPI1 CS1
        /// </summary>
        Gpio17 = 0x11,

        /// <summary>
        ///     Gpio Input Output / PWM / SPI1 CS0
        /// </summary>
        Gpio18 = 0x12,

        /// <summary>
        ///     Gpio Input Output / SPI1 MISO
        /// </summary>
        Gpio19 = 0x13,

        /// <summary>
        ///     Gpio Input Output / SPI1 MOSI
        /// </summary>
        Gpio20 = 0x14,

        /// <summary>
        ///     Gpio Input Output / SPI1 SCLK
        /// </summary>
        Gpio21 = 0x15,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio22 = 0x16,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio23 = 0x17,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio24 = 0x18,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio25 = 0x19,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio26 = 0x1A,

        /// <summary>
        ///     Gpio Input Output
        /// </summary>
        Gpio27 = 0x1B
    }
}