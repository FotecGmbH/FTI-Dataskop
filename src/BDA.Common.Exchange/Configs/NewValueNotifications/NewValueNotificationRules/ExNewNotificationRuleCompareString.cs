// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules
{
    /// <summary>
    ///     <para>Regel für Benachrichtigung mit String</para>
    ///     Klasse ExNewNotificationRuleCompareString. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewNotificationRuleCompareString : ExNewValueNotificationRuleBase
    {
        /// <summary>
        /// ExNewNotificationRuleCompareString
        /// </summary>
        public ExNewNotificationRuleCompareString()
        {
            NewValueNotificationRuleType = EnumNewValueNotificationRuleType.String;
        }

        #region Properties

        /// <summary>
        ///     Vergleichswert
        /// </summary>
        public List<ExCompareStringValue> ComparedValues { get; set; } = new();

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

            return ComparedValues.All(x => x.IsCorrect(measurement.Value));
        }
    }
}