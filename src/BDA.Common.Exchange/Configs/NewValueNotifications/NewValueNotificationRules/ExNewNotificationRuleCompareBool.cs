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
    ///     <para>Regel für Benachrichtigung mit Boolean</para>
    ///     Klasse ExNewNotificationRuleCompareBool. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewNotificationRuleCompareBool : ExNewValueNotificationRuleBase
    {
        /// <summary>
        ///     new instance
        /// </summary>
        public ExNewNotificationRuleCompareBool()
        {
            NewValueNotificationRuleType = EnumNewValueNotificationRuleType.Boolean;
        }

        #region Properties

        /// <summary>
        ///     Vergleichswert
        /// </summary>
        public bool ComparedValue { get; set; }

        /// <summary>
        ///     Typ der Regel
        /// </summary>
#pragma warning disable CS0108, CS0114
        public EnumNewValueNotificationRuleType NewValueNotificationRuleType { get; set; }
#pragma warning restore CS0108, CS0114

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

            var success = bool.TryParse(measurement.Value, out var value);

            if (!success)
            {
                return false;
            }

            return value == ComparedValue;
        }
    }
}