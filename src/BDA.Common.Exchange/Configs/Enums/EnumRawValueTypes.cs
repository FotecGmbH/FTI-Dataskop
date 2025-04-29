// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Unterstütze Typen</para>
    ///     Klasse EnumSuportedRawValues. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumRawValueTypes
    {
        /// <summary>
        ///     Für Edge-Computing
        /// </summary>
        Custom,

        /// <summary>
        ///     Boolean - byte[1]
        /// </summary>
        Bit,

        /// <summary>
        ///     Float - byte[4]
        /// </summary>
        Float,

        /// <summary>
        ///     Double - byte[8]
        /// </summary>
        Double,

        /// <summary>
        ///     Int16 - byte[2]
        /// </summary>
        Int16,

        /// <summary>
        ///     UInt16 - byte[2]
        /// </summary>
        UInt16,

        /// <summary>
        ///     Int64 - byte[4]
        /// </summary>
        Int32,

        /// <summary>
        ///     UInt64 - byte[4]
        /// </summary>
        UInt32,

        /// <summary>
        ///     Int64 - byte[8]
        /// </summary>
        Int64,

        /// <summary>
        ///     UInt64 - byte[8]
        /// </summary>
        UInt64,

        /// <summary>
        ///     Byte - byte[1]
        /// </summary>
        Byte,

        /// <summary>
        ///     ByteArray - byte[n]
        /// </summary>
        ByteArray
    }
}