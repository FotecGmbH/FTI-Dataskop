// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Tables
{
    /// <summary>
    ///     <para>Access Token der Benutzer</para>
    ///     Klasse TableAccessToken. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("AccessToken")]
    public class TableAccessToken
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Token
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Token { get; set; } = string.Empty;

        /// <summary>
        ///     Gültig bis
        /// </summary>
        public DateTime GuiltyUntilUtc { get; set; }

        /// <summary>
        ///     User welchem der Token zugewiesen ist
        /// </summary>
        public long TblUserId { get; set; }

        /// <summary>
        ///     User welchem der Token zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual TableUser TblUser { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion
    }
}