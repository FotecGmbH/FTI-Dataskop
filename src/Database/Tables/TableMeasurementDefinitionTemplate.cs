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
using Database.Common;

namespace Database.Tables
{
    /// <summary>
    ///     Information für Firmen
    /// </summary>
    [Table("MeasurementDefinitionTemplate")]
    public class TableMeasurementDefinitionTemplate
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Name
        /// </summary>
        public DbInformation Information { get; set; } = new();

        /// <summary>
        ///     Art des Werts
        /// </summary>
        public EnumValueTypes ValueType { get; set; }

        /// <summary>
        ///     Zugehöriges Iot-Gerät
        /// </summary>
        public long TblDataconverterId { get; set; }

        /// <summary>
        ///     Zugehöriges IoT Device
        /// </summary>
        [ForeignKey(nameof(TblDataconverterId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual TableDataconverter TblDataconverter { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion
    }
}