// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules
{
    /// <summary>
    ///     <para>Mit Wert vergleichen</para>
    ///     Klasse ExCompareDoubleValue. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExCompareStringValue
    {
        #region Properties

        /// <summary>
        ///     Vergleichswert
        /// </summary>
        public string CompareValue { get; set; } = string.Empty;

        /// <summary>
        ///     Vergleichsoperator
        /// </summary>
        public EnumCompareOperator CompareOperator { get; set; }

        #endregion

        /// <summary>
        ///     Ob Vergleich korrekt ist
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool IsCorrect(string value)
        {
            switch (CompareOperator)
            {
                case EnumCompareOperator.IsEqual:
                    return value == CompareValue;
                case EnumCompareOperator.IsNotEqual:
                    return value != CompareValue;
                case EnumCompareOperator.IsGreater:
                    return value.Length > CompareValue.Length;
                case EnumCompareOperator.IsGreaterOrEqual:
                    return value.Length >= CompareValue.Length;
                case EnumCompareOperator.IsLess:
                    return value.Length < CompareValue.Length;
                case EnumCompareOperator.IsLessOrEqual:
                    return value.Length <= CompareValue.Length;
                case EnumCompareOperator.Contains:
                    return value.Contains(CompareValue, StringComparison.InvariantCulture);
                case EnumCompareOperator.NotContains:
                    return !value.Contains(CompareValue, StringComparison.InvariantCulture);

                default:
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("Cannot use this operator");
            }
        }
    }
}