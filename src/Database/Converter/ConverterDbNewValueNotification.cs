// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using Database.Tables;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Benachrichtigung f. neuen Wert: Ex -> Table -> Ex</para>
    ///     Klasse ConverterDbNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbNewValueNotification
    {
        /// <summary>
        /// ToExNewValueNotification
        /// </summary>
        /// <param name="tableNewValueNotification"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExNewValueNotification ToExNewValueNotification(this TableNewValueNotification tableNewValueNotification)
        {
            if (tableNewValueNotification == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbNewValueNotification)}]({nameof(ToExNewValueNotification)}): {nameof(tableNewValueNotification)} is null");
            }

            return new ExNewValueNotification
            {
                Id = tableNewValueNotification.Id,
                NewValueNotificationType = tableNewValueNotification.NewValueNotificationType,
                AdditionalConfiguration = tableNewValueNotification.AdditionalConfiguration,
                MeasurementDefinitionId = tableNewValueNotification.TblMeasurementDefinitionId,
                UserId = tableNewValueNotification.TblUserId,
            };
        }

        /// <summary>
        /// ToTableNewValueNotification
        /// </summary>
        /// <param name="exNewValueNotificationBaseBase"></param>
        /// <param name="tableNewValueNotification"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableNewValueNotification(this ExNewValueNotification exNewValueNotificationBaseBase, TableNewValueNotification tableNewValueNotification)
        {
            if (exNewValueNotificationBaseBase == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbNewValueNotification)}]({nameof(ToTableNewValueNotification)}): {nameof(exNewValueNotificationBaseBase)} is null");
            }

            if (tableNewValueNotification == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbNewValueNotification)}]({nameof(ToTableNewValueNotification)}): {nameof(tableNewValueNotification)} is null");
            }

            tableNewValueNotification.Id = exNewValueNotificationBaseBase.Id;
            tableNewValueNotification.NewValueNotificationType = exNewValueNotificationBaseBase.NewValueNotificationType;
            tableNewValueNotification.TblMeasurementDefinitionId = exNewValueNotificationBaseBase.MeasurementDefinitionId;
            tableNewValueNotification.TblUserId = exNewValueNotificationBaseBase.UserId;
            tableNewValueNotification.AdditionalConfiguration = exNewValueNotificationBaseBase.AdditionalConfiguration;
        }
    }
}