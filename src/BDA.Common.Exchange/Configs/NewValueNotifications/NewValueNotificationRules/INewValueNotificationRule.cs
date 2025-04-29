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
    ///     <para>Interface für Neuer-Wert-Benachrichtigungsregel</para>
    ///     Interface INewValueNotificationRule. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface INewValueNotificationRule
    {
        #region Properties

        /// <summary>
        ///     Typ der Regel
        /// </summary>
        EnumNewValueNotificationRuleType NewValueNotificationRuleType { get; set; }

        #endregion

        /// <summary>
        ///     Ob Benachrichtigung erwünscht
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public bool IsNotificationDesired(ExMeasurement measurement);
    }
}