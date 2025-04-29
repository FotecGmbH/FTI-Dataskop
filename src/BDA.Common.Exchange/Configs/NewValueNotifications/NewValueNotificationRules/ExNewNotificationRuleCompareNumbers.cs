// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules
{
    /// <summary>
    ///     <para>Regel für Benachrichtigung mit Zahlen</para>
    ///     Klasse ExNewNotificationRuleCompareTwoNumbers. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewNotificationRuleCompareNumbers : ExNewValueNotificationRuleBase
    {
        /// <summary>
        ///     new instance
        /// </summary>
        public ExNewNotificationRuleCompareNumbers()
        {
            NewValueNotificationRuleType = EnumNewValueNotificationRuleType.Number;
        }

        #region Properties

        /// <summary>
        ///     Vergleichswert 1
        /// </summary>
        public ExCompareDoubleValue ComparedValue1 { get; set; } = new();

        /// <summary>
        ///     Vergleichswert 2
        /// </summary>
        public ExCompareDoubleValue? ComparedValue2 { get; set; }

        #endregion

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public override bool IsNotificationDesired(ExMeasurement measurement)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            var success = double.TryParse(measurement.Value, out var value);

            if (!success)
            {
                return false;
            }

            //alle Regeln müssen passen (UND-Verknüpft)
            return ComparedValue1.IsCorrect(value) && (ComparedValue2?.IsCorrect(value) ?? true);
        }
    }
}