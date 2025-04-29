// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.NewValueNotifications
{
    /// <summary>
    ///     <para>Neuer Wert Benachrichtigung</para>
    ///     Klasse ExNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewValueNotificationBase : IBissModel, INewValueNotification
    {
        #region Properties

        /// <summary>
        ///     Typ der Benachrichtigung
        /// </summary>
        public EnumNewValueNotificationType NewValueNotificationType { get; set; }

        /// <summary>
        ///     Regeln für Benachrichtigung
        /// </summary>
        public List<INewValueNotificationRule> Rules { get; set; } = new();

        #endregion

        /// <summary>
        /// IsNotificationDesired
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public bool IsNotificationDesired(ExMeasurement measurement)
        {
            return Rules == null! || Rules.Count == 0 || Rules.Any(rule => rule.IsNotificationDesired(measurement));
        }

        #region Interface Implementations

#pragma warning disable CS1591
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS1591

        #endregion
    }
}