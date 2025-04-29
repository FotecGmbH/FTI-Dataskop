// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Common;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace BDA.Common.Exchange.Model
{
    /// <summary>
    ///     <para>Rechte in einer Firma falls der User nicht Superadmin (in TableUser = Admin = True) ist</para>
    ///     Klasse ExUserPremission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUserPremission
    {
        #region Properties

        /// <summary>
        ///     Recht für die Firma mit der Id
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Rolle des Users in der Firma
        /// </summary>
        public EnumUserRole UserRole { get; set; }

        /// <summary>
        ///     Rechte des Users bei der Firma
        /// </summary>
        public EnumUserRight UserRight { get; set; }

        #endregion
    }


    /// <summary>
    ///     <para>Benutzerstammdaten</para>
    ///     Klasse ExUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUser : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Ist der User Super-Admin
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        ///     Vorname
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        ///     Nachname
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        ///     Name
        /// </summary>
        [DependsOn(nameof(FirstName), nameof(LastName))]
        [JsonIgnore]
        public string Fullname => $"{FirstName} {LastName}";

        /// <summary>
        ///     Name für Login (Email, Telefonnummer, etc)
        /// </summary>
        public string LoginName { get; set; } = string.Empty;

        /// <summary>
        ///     Bild des User
        /// </summary>
        public string UserImageLink { get; set; } = string.Empty;

        /// <summary>
        ///     Bild id in Files Tabelle
        /// </summary>
        public long UserImageDbId { get; set; }

        /// <summary>
        ///     Bild OK?
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(UserImageLink))]
        public bool HasImage => !string.IsNullOrEmpty(UserImageLink);

        /// <summary>
        ///     Bei einem neuem Benutzer das Passwort als Hash
        /// </summary>
        public string PasswordHash4NewUser { get; set; } = string.Empty;

        /// <summary>
        ///     Standardsprache des Benutzers
        /// </summary>
        public string DefaultLanguage { get; set; } = string.Empty;

        /// <summary>
        ///     Demo Einstellung für User (alle 10 min eine Push Nachricht senden)
        /// </summary>
        public bool Setting10MinPush { get; set; }

        /// <summary>
        ///     Berechtigungen auf Firmenebene - nur relevat für Admin == False
        /// </summary>
        public List<ExUserPremission> Premissions { get; set; } = new List<ExUserPremission>();

        /// <summary>
        ///     Geräte eines User
        /// </summary>
        public ObservableCollection<ExUserDevice> UserDevices { get; set; } = new ObservableCollection<ExUserDevice>();

        /// <summary>
        ///     User Token
        /// </summary>
        public ObservableCollection<ExAccessToken> Tokens { get; set; } = new ObservableCollection<ExAccessToken>();


        /// <summary>
        ///     Erstmaliges Login erfolgreich (User werden ja durch (Firmen)Admins angelegt)
        /// </summary>
        public bool LoginConfirmed { get; set; }

        /// <summary>
        ///     Account ist gesperrt
        /// </summary>
        public bool Locked { get; set; }


        /// <summary>
        ///     Registrierte Tags für Push - Json Objekt der Liste
        /// </summary>
        public string? PushTags { get; set; }

        #endregion

        /// <summary>
        ///     Ist der User in einer bestimmten Firma Admin?
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool IsAdmInCompany(long companyId)
        {
            if (IsAdmin)
            {
                return true;
            }

            if (Premissions.Any(a => a.CompanyId == companyId && a.UserRole == EnumUserRole.Admin))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Hat der User Schreibrechte in einer bestimmten Firma
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool CanUserWriteInCompany(long companyId)
        {
            if (IsAdmin)
            {
                return true;
            }

            if (Premissions.Any(a => a.CompanyId == companyId && a.UserRight == EnumUserRight.ReadWrite))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Hat der User Leserechte in einer bestimmten Firma
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool CanUserReadInCompany(long companyId)
        {
            if (IsAdmin)
            {
                return true;
            }

            //Hat der User Leseberechtigung oder wird auf die öffentliche Firma zugegriffen
            if (Premissions.Any(a => a.CompanyId == companyId && a.UserRight == EnumUserRight.Read) || companyId == 2)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Indicates whether a user has no rights at all in the company.
        /// </summary>
        /// <returns>True if the user doesnt have any rights in the company, otherwise false.</returns>
        public bool HaveNoRightsInCompany(long id) => !IsAdmInCompany(id) && !CanUserReadInCompany(id) && CanUserWriteInCompany(id);

        /// <summary>
        ///     Indicates whether user has permission to device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool HasPermissionTo(ExIotDevice? device)
        {
            if (device is null)
            {
                return false;
            }

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            return Premissions?.Any(x => x.CompanyId == device.CompanyId) ?? false;
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }

    /// <summary>
    ///     <para>Gerät eines User</para>
    ///     Klasse ExUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUserDevice : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Name des Gerätes - vom User gesetzt
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the model of the device.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the manufacturer of the device.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        ///     Plattform des Gerätes
        /// </summary>
        public EnumIotDevicePlattforms Plattform { get; set; }

        /// <summary>
        ///     Geräteart
        /// </summary>
        public EnumDeviceIdiom DeviceIdiom { get; set; }

        /// <summary>
        /// IsSelected
        /// </summary>
        public bool IsSelected { get; set; }
        /// <summary>
        /// CanEnableIsSelect
        /// </summary>
        public bool CanEnableIsSelect { get; set; }

        #endregion

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;

        /// <summary>
        /// Selected
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event EventHandler<BissSelectableEventArgs> Selected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}