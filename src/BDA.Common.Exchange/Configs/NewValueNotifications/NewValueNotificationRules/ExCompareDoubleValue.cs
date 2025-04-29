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
    public class ExCompareDoubleValue
    {
        #region Properties

        /// <summary>
        ///     Vergleichswert
        /// </summary>
        public double CompareValue { get; set; }

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
        public bool IsCorrect(double value)
        {
            switch (CompareOperator)
            {
                case EnumCompareOperator.IsEqual:
                    return Math.Abs(value - CompareValue) < 0.0001;
                case EnumCompareOperator.IsNotEqual:
                    return Math.Abs(value - CompareValue) > 0.0001;
                case EnumCompareOperator.IsGreater:
                    return value > CompareValue;
                case EnumCompareOperator.IsGreaterOrEqual:
                    return value >= CompareValue;
                case EnumCompareOperator.IsLess:
                    return value < CompareValue;
                case EnumCompareOperator.IsLessOrEqual:
                    return value <= CompareValue;
                default:
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("Cannot use this operator");
            }
        }
    }
}