// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Unterstütze Datentypen eines Messwerts</para>
    ///     Klasse EnumValueType. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumValueTypes
    {
        /// <summary>
        ///     Double, Int, byte, ...
        /// </summary>
        Number,

        /// <summary>
        ///     byte []
        /// </summary>
        Data,

        /// <summary>
        ///     string
        /// </summary>
        Text,

        /// <summary>
        ///     byte[]
        /// </summary>
        Image,

        /// <summary>
        ///     Bit
        /// </summary>
        Bit
    }
}