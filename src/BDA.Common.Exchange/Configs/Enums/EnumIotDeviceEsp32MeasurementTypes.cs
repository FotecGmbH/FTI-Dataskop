// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Mögliche Messwerte für Downstream Type Esp32</para>
    ///     Klasse EnumIotDeviceDownstreamTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDeviceEsp32MeasurementTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Gpio Input Output / IO0 / WROOM 0
        /// </summary>
        Gpio0 = 0x01,

        /// <summary>
        ///     Gpio Input Output / IO1 / WROOM 1
        /// </summary>
        Gpio1 = 0x02,

        /// <summary>
        ///     Gpio Input Output / IO2 / WROOM 2
        /// </summary>
        Gpio2 = 0x03,

        /// <summary>
        ///     Gpio Input Output / IO3 / WROOM 3
        /// </summary>
        Gpio3 = 0x04,

        /// <summary>
        ///     Gpio Input Output / IO4 / WROOM 4
        /// </summary>
        Gpio4 = 0x05,

        /// <summary>
        ///     Gpio Input Output / IO5 / WROOM 5
        /// </summary>
        Gpio5 = 0x06,

        /// <summary>
        ///     Gpio Input Output / IO12 / WROOM 12
        /// </summary>
        Gpio12 = 0x07,

        /// <summary>
        ///     Gpio Input Output / IO13 / WROOM 13
        /// </summary>
        Gpio13 = 0x08,

        /// <summary>
        ///     Gpio Input Output / IO14 / WROOM 14
        /// </summary>
        Gpio14 = 0x09,

        /// <summary>
        ///     Gpio Input Output / IO15 / WROOM 15
        /// </summary>
        Gpio15 = 0x0A,

        /// <summary>
        ///     Gpio Input Output / IO18 / WROOM 18
        /// </summary>
        Gpio18 = 0x0B,

        /// <summary>
        ///     Gpio Input Output / IO19 / WROOM 19
        /// </summary>
        Gpio19 = 0x0C,

        /// <summary>
        ///     Gpio Input Output / IO21 / WROOM 21
        /// </summary>
        Gpio21 = 0x0D,

        /// <summary>
        ///     Gpio Input Output / IO22 / WROOM 22
        /// </summary>
        Gpio22 = 0x0E,

        /// <summary>
        ///     Gpio Input Output / IO23 / WROOM 23
        /// </summary>
        Gpio23 = 0x0F,

        /// <summary>
        ///     Gpio Input Output / IO25 / WROOM 25
        /// </summary>
        Gpio25 = 0x10,

        /// <summary>
        ///     Gpio Input Output / IO26 / WROOM 26
        /// </summary>
        Gpio26 = 0x11,

        /// <summary>
        ///     Gpio Input Output / IO27 / WROOM 27
        /// </summary>
        Gpio27 = 0x12,

        /// <summary>
        ///     Input Output / IO32 / WROOM 32
        /// </summary>
        Gpio32 = 0x13,

        /// <summary>
        ///     Gpio Input Output / IO33 / WROOM 33
        /// </summary>
        Gpio33 = 0x14,


        /// <summary>
        ///     ADC1_CH4 / IO32 / WROOM 32
        /// </summary>
        Adc1Ch4 = 0x81,

        /// <summary>
        ///     ADC1_CH4 / IO32 / WROOM 32
        /// </summary>
        Adc1Ch5 = 0x82,

        /// <summary>
        ///     ADC1_CH4 / IO32 / WROOM 32
        /// </summary>
        Adc1Ch6 = 0x83,

        /// <summary>
        ///     ADC1_CH4 / IO32 / WROOM 32
        /// </summary>
        Adc1Ch7 = 0x84,

        /// <summary>
        ///     Interner Halleffectsensor.
        /// </summary>
        InternalHalleffect = 0x91,
    }
}