// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Globalization;
using BDA.Common.Exchange.Enum;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Ein einzelner erfasster Messwert</para>
    ///     Klasse ExValue. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExValue : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Die Art des Messwerts
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        /// <summary>
        ///     Messwert als Text.
        /// </summary>
        public string? MeasurementText { get; set; }

        /// <summary>
        ///     Messwert als Zahl.
        /// </summary>
        public double? MeasurementNumber { get; set; }

        /// <summary>
        ///     Messwert als Boolean.
        /// </summary>
        public bool? MeasurementBool { get; set; }

        /// <summary>
        ///     Messwert als Bytes
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? MeasurementRaw { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Messwert als Bild
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? MeasurementImage { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Zeitstempel an dem die Messung durchgeführt wurde
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Position des Messwerts
        /// </summary>
        public ExPosition? Position { get; set; } = null;

        /// <summary>
        ///     Eindeutige ID für die Zuweisung in der Datenbank
        /// </summary>
        public long Identifier { get; set; }

        #endregion

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var v = GetMeasurement();
            var r = string.Empty;
            if (v is byte[] b)
            {
                for (var i = 0; i < b.LongLength; i++)
                {
                    if (i > 20)
                    {
                        r += $" + {b.LongLength - i} more bytes";
                    }

                    r += b[i].ToString("X2 ");
                }
            }
            else
            {
                r = v.ToString();
            }

            return r;
        }

        /// <summary>
        ///     String mit Details über das aktuelle Objekt
        /// </summary>
        /// <returns></returns>
        public string ToStringWithDetails()
        {
            return $"{TimeStamp.ToString(new CultureInfo("de"))} - {Identifier} - {ValueType}: {ToString()}";
        }

        /// <summary>
        ///     Gibt den gesetzten Messwert zurück. Abhänging vom gesetzten ValueType
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws if corresponding property is null (e.g. ValueType=Text and
        ///     MeasurementText=NULL)
        /// </exception>
#pragma warning disable CA1024 // Use properties where appropriate
        public object GetMeasurement()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return ValueType switch
            {
                EnumValueTypes.Data when MeasurementRaw is not null => MeasurementRaw,
                EnumValueTypes.Text when MeasurementText is not null => MeasurementText,
                EnumValueTypes.Number when MeasurementNumber is not null => MeasurementNumber,
                EnumValueTypes.Image when MeasurementImage is not null => MeasurementImage,
                EnumValueTypes.Bit when MeasurementBool is not null => MeasurementBool,
                _ => throw new InvalidOperationException($"[{nameof(ExValue)}]({nameof(GetMeasurement)}): Type is {ValueType} but corresponding property is null"),
            };
        }

        /// <summary>
        ///     Gibt an ob der ExValue einen Wert hat
        /// </summary>
        /// <returns>Ob der ExValue einen Wert hat</returns>
        public bool HasValue()
        {
            return (ValueType == EnumValueTypes.Data && MeasurementRaw is not null ||
                ValueType == EnumValueTypes.Text && MeasurementText is not null ||
                ValueType == EnumValueTypes.Number && MeasurementNumber is not null ||
                ValueType == EnumValueTypes.Image && MeasurementImage is not null ||
                ValueType == EnumValueTypes.Bit && MeasurementBool is not null);
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