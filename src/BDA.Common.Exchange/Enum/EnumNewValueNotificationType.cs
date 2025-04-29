// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.ComponentModel.DataAnnotations;

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Enum für Benachrichtigungstyp bei neuem Wert</para>
    ///     Klasse EnumNewValueNotificationType. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumNewValueNotificationType
    {
        /// <summary>
        ///     Benachrichtigung über Webhook
        /// </summary>
        [Display(Name = "Webhook")] Webhook,

        /// <summary>
        ///     Benachrichtigung über Mqtt
        /// </summary>
        [Display(Name = "MQTT")] Mqtt,

        /// <summary>
        ///     Benachrichtigung über Email
        /// </summary>
        [Display(Name = "Email")] Email,

        /// <summary>
        ///     Benachrichtigung über SMS
        /// </summary>
        [Display(Name = "SMS")] Sms,

        /// <summary>
        ///     Benachrichtigung über Push
        /// </summary>
        [Display(Name = "Push")] Push
    }
}