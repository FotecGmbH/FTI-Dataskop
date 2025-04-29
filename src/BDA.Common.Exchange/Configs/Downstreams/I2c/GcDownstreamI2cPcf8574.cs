// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes;

namespace BDA.Common.Exchange.Configs.Downstreams.I2c
{
    /// <summary>
    ///     <para>Config für Pcf8574</para>
    ///     Klasse ClobalConfigI2cPcf8574. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("I²C PCF8574")]
    public class GcDownstreamI2CPcf8574 : GcDownstreamI2c
    {
        /// <summary>
        ///     Config für Pcf8574
        /// </summary>
        public GcDownstreamI2CPcf8574()
        {
            ChipName = "PCF8574";
        }

        #region Properties

        /// <summary>
        ///     Pin
        /// </summary>
        [ConfigProperty("Pin (0-7)")]
        public string PinNumber { get; set; } = string.Empty;

        #endregion
    }
}