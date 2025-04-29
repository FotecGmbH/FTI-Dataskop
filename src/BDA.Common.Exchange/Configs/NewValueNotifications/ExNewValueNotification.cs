// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.NewValueNotifications
{
    /// <summary>
    ///     <para>Neuer Wert Benachrichtigung Basisklasse</para>
    ///     Klasse ExNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExNewValueNotification : IBissModel, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     Db-Id der Benachrichtigung
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Typ der Benachrichtigung
        /// </summary>
        public EnumNewValueNotificationType NewValueNotificationType { get; set; }


        /// <summary>
        ///     Zusätzliche Daten für die Benachrichtigung (komplettes eigentliches Objekt als Json)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Messdefinition Id
        /// </summary>
        public long MeasurementDefinitionId { get; set; }

        /// <summary>
        ///     User Id
        /// </summary>
        public long UserId { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}