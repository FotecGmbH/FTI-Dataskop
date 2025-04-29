// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.NewValueNotifications
{
    /// <summary>
    ///     <para>Neuer Wert Benachrichtigung über Webhook</para>
    ///     Klasse ExNewValueNotificationWebHook. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewValueNotificationWebHook : ExNewValueNotificationBase
    {
        /// <summary>
        ///     Creates new instance
        /// </summary>
        public ExNewValueNotificationWebHook()
        {
            NewValueNotificationType = EnumNewValueNotificationType.Webhook;
        }

        #region Properties

        /// <summary>
        ///     Url f. Webhook
        /// </summary>
        public string WebHookUrl { get; set; } = string.Empty;

        #endregion
    }
}