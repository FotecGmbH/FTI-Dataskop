// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Biss.Dc.Core;

namespace Database.Tables
{
    /// <summary>
    ///     <para>Benachrichtigungen bei neuem Messwert</para>
    ///     Klasse TableNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("NewValueNotification")]
    public class TableNewValueNotification : IDcChangeTracking, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     ID für DB
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Typ der Benachrichtigung
        /// </summary>
        public EnumNewValueNotificationType NewValueNotificationType { get; set; }

        /// <summary>
        ///     Zusätzliche Daten für die Benachrichtigung
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Zugehörige Messdefinition Id
        /// </summary>
        public long TblMeasurementDefinitionId { get; set; }

        /// <summary>
        ///     Zugehörige Messdefinition
        /// </summary>
        [ForeignKey(nameof(TblMeasurementDefinitionId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual TableMeasurementDefinition TblMeasurementDefinition { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        ///     Zugehörige User Id
        /// </summary>
        public long TblUserId { get; set; }

        /// <summary>
        ///     Zugehöriger User
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual TableUser TblUser { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion

        /// <summary>
        ///     Version der Zeile. Wird automatisch durch den SQL Server aktualisiert
        ///     https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application#add-an-optimistic-concurrency-property-to-the-department-entity
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        [Timestamp]
        public byte[] DataVersion { get; set; } = null!;

        /// <summary>Archiviert</summary>
        public bool IsArchived { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}