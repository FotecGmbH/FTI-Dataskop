// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Mögliche Messwertdefinitionen für Virtuelle Messwerte</para>
    ///     Klasse EnumIotDeviceDownstreamTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDeviceVirtualMeasurementTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Zufällige Zahl
        /// </summary>
        Float = 0x01,

        /// <summary>
        ///     Zufälliges Bild
        /// </summary>
        Image = 0x02,

        /// <summary>
        ///     Zufälliger Text
        /// </summary>
        Text = 0x03,

        /// <summary>
        ///     Zufällige Daten in einem byte[5]
        /// </summary>
        Data = 0x04
    }
}