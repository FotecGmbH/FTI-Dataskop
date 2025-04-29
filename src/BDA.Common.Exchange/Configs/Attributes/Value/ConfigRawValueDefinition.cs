// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.Attributes.Value
{
    /// <summary>
    ///     <para>Was kommt vom IoT Gerät für einen einzelnen Messwert</para>
    ///     Klasse ConfigRawValueDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConfigRawValueDefinition : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Wie viele Bytes hat der Messwert
        /// </summary>
        public int ByteCount { get; set; } = 4;

        /// <summary>
        ///     Als was soll er interpretiert werden
        /// </summary>
        public EnumRawValueTypes RawValueType { get; set; } = EnumRawValueTypes.Float;

        #endregion

        /// <summary>
        ///     Anzahl der Bytes je unterstützen Type
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetByteCount(EnumRawValueTypes rawValueType)
        {
            switch (rawValueType)
            {
                case EnumRawValueTypes.Bit:
                    return 1;
                case EnumRawValueTypes.Float:
                    return 4;
                case EnumRawValueTypes.Double:
                    return 8;
                case EnumRawValueTypes.Int16:
                    return 2;
                case EnumRawValueTypes.UInt16:
                    return 2;
                case EnumRawValueTypes.Int32:
                    return 4;
                case EnumRawValueTypes.UInt32:
                    return 4;
                case EnumRawValueTypes.Int64:
                    return 8;
                case EnumRawValueTypes.UInt64:
                    return 8;
                case EnumRawValueTypes.Byte:
                    return 1;
                case EnumRawValueTypes.Custom:
                case EnumRawValueTypes.ByteArray:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}