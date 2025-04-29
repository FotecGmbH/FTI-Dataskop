// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules
{
    /// <summary>
    ///     <para>Regel für Benachrichtigung</para>
    ///     Klasse ExNewValueNotificationRuleBase. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract class ExNewValueNotificationRuleBase : IBissModel, INewValueNotificationRule
    {
        #region Properties

        /// <summary>
        ///     Typ der Regel
        /// </summary>
        public EnumNewValueNotificationRuleType NewValueNotificationRuleType { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS1591
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS1591

        /// <summary>
        ///     Ob Benachrichtigung erwünscht
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public abstract bool IsNotificationDesired(ExMeasurement measurement);

        #endregion
    }
}