// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.NewValueNotifications
{
    /// <summary>
    ///     <para>Interface für Neuer-Wert-Benachrichtigung</para>
    ///     Interface INewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface INewValueNotification
    {
        #region Properties

        /// <summary>
        ///     Typ der Benachrichtigung
        /// </summary>
        EnumNewValueNotificationType NewValueNotificationType { get; set; }

        /// <summary>
        ///     Reglen für Benachrichtigung
        /// </summary>
        List<INewValueNotificationRule> Rules { get; set; }

        #endregion
    }
}