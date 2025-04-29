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
    ///     <para>Neuer Wert Benachrichtigung über MQTT</para>
    ///     Klasse ExNewValueNotificationMqtt. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewValueNotificationMqtt : ExNewValueNotificationBase
    {
        /// <summary>
        ///     Creates new instance
        /// </summary>
        public ExNewValueNotificationMqtt()
        {
            NewValueNotificationType = EnumNewValueNotificationType.Mqtt;
        }
    }
}