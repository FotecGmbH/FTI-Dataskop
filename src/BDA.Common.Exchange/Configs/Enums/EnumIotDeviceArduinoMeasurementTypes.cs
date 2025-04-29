// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Mögliche Messwerte für Downstream Type Arduino</para>
    ///     Klasse EnumIotDeviceArduinoMeasurementTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Flags]
    public enum EnumIotDeviceArduinoMeasurementTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Gpio Input Output / D2 / Interrupt Pin
        /// </summary>
        Gpio2 = 0x01,

        /// <summary>
        ///     Gpio Input Output / D3 / PWM / Interrupt Pin
        /// </summary>
        Gpio3 = 0x02,

        /// <summary>
        ///     Gpio Input Output / D4
        /// </summary>
        Gpio4 = 0x03,

        /// <summary>
        ///     Gpio Input Output / D5 / PWM
        /// </summary>
        Gpio5 = 0x04,

        /// <summary>
        ///     Gpio Input Output / D6 / PWM
        /// </summary>
        Gpio6 = 0x05,

        /// <summary>
        ///     Gpio Input Output / D7
        /// </summary>
        Gpio7 = 0x06,

        /// <summary>
        ///     Gpio Input Output / D8
        /// </summary>
        Gpio8 = 0x07,

        /// <summary>
        ///     Gpio Input Output / D9 / PWM
        /// </summary>
        Gpio9 = 0x08,

        /// <summary>
        ///     Gpio Input Output / D10 / PWM / Default CS
        /// </summary>
        Gpio10 = 0x09,

        /// <summary>
        ///     Gpio Input Output / D11 / PWM / Default MOSI
        /// </summary>
        Gpio11 = 0x0A,

        /// <summary>
        ///     Gpio Input Output / D12 / Default MISO
        /// </summary>
        Gpio12 = 0x0B,

        /// <summary>
        ///     Gpio Input Output / D13 / Default CLK
        /// </summary>
        Gpio13 = 0x0C,

        /// <summary>
        ///     Analog Input Output / A0
        /// </summary>
        A0 = 0x80,

        /// <summary>
        ///     Analog Input Output / A1
        /// </summary>
        A1 = 0x81,

        /// <summary>
        ///     Analog Input Output / A2
        /// </summary>
        A2 = 0x82,

        /// <summary>
        ///     Analog Input Output / A3
        /// </summary>
        A3 = 0x83,

        /// <summary>
        ///     Analog Input Output / A4 / Default SDA
        /// </summary>
        A4 = 0x84,

        /// <summary>
        ///     Analog Input Output / A5 / Default SCL
        /// </summary>
        A5 = 0x85
    }
}