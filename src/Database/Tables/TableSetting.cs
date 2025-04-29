// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BDA.Common.Exchange.Enum;

namespace Database.Tables
{
    /// <summary>
    ///     <para>TableSetting</para>
    ///     Klasse TableSetting. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("Setting")]
    public class TableSetting
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Settings - Key
        /// </summary>
        public EnumDbSettings Key { get; set; }

        /// <summary>
        ///     Wert des Key
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Value { get; set; } = string.Empty;

        #endregion
    }
}