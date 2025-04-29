// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace Database.Tables
{
    /// <summary>
    ///     <para>TableUser</para>
    ///     Klasse TableUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("User")]
    public class TableUser
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Vorname
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        ///     Nachname
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        ///     Benutzer ist Globaler - Administrator
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        ///     akzeptierte agb version
        /// </summary>
        public string AgbVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Name für Login (Email, Telefonnummer, etc)
        /// </summary>
        public string LoginName { get; set; } = string.Empty;

        /// <summary>
        ///     Passworthash für Login
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        ///     Refreshtoken für Serverzugriff
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        ///     JwtToken für Serverzugriff
        /// </summary>
        public string JwtToken { get; set; } = string.Empty;

        /// <summary>
        ///     Account gesperrt
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        ///     Login bestätigt (Email, Telefonnummer, etc)
        /// </summary>
        public bool LoginConfirmed { get; set; }

        /// <summary>
        ///     Erstellt am
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        ///     Standardsprache
        /// </summary>
        public string DefaultLanguage { get; set; } = string.Empty;

        /// <summary>
        ///     GUID für Account-Bestätigung und Passwort-Reset (im Link enthalten)
        /// </summary>
        public string ConfirmationToken { get; set; } = string.Empty;

        /// <summary>
        ///     Registrierte Tags für Push - Json Objekt der Liste
        /// </summary>
        public string? PushTags { get; set; }

        /// <summary>
        ///     Demo Einstellung für User (alle 10 min eine Push Nachricht senden)
        /// </summary>
        public bool Setting10MinPush { get; set; }

        /// <summary>
        ///     UserProfilbild
        /// </summary>
        [ForeignKey(nameof(TblUserImage))]
        public long? TblUserImageId { get; set; }

        /// <summary>
        ///     UserProfilbild
        /// </summary>
        public virtual TableFile? TblUserImage { get; set; }

        /// <summary>
        ///     Geräte des Benutzers
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TableDevice> TblDevices { get; set; } = new List<TableDevice>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Permissions des Benutzers
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TablePermission> TblPermissions { get; set; } = new List<TablePermission>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     AccessToken des Benutzers
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TableAccessToken> TblAccessToken { get; set; } = new List<TableAccessToken>();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion
    }
}