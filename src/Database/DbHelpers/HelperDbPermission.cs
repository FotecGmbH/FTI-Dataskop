// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Base;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>DESCRIPTION</para>
///     Klasse HelperDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle User und Firmen in welchen der aktuelle Benutzer Firmenadmin (oder globaler Admins) ist
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public IQueryable<TablePermission> GetCompanyUsers(long currentUserId)
    {
        var isAdmin = IsUserAdmin(currentUserId);
        var companiesUserIsAdmin = new List<long>();
        if (!isAdmin)
        {
            companiesUserIsAdmin = TblPermissions.Where(w => w.TblUserId == currentUserId && w.UserRole == EnumUserRole.Admin).Select(s => s.TblCompanyId).ToList();
        }

        var r = TblPermissions.AsNoTracking().Where(w => isAdmin ||
            (companiesUserIsAdmin.Contains(w.TblCompanyId))
        ).Include(i => i.TblUser).ThenInclude(i => i.TblUserImage);
        return r;
    }

    /// <summary>
    ///     Einer Firma wurde ein (neuer) User zugewiesen. Ist dieser noch nicht im System dann Account erstellen (damit
    ///     Registrierung starten).
    ///     Datensatz mit dem richtigen dann sicher vorhandenen User verknüpfen.
    /// </summary>
    /// <param name="user"></param>
    public (bool newUser, TableUser user, string pwd) CheckNewUserForPremission(ExCompanyUser user)
    {
        if (user == null!)
        {
            throw new ArgumentNullException($"[{nameof(Db)}]({nameof(CheckNewUserForPremission)}): {nameof(user)}");
        }


        var tmpList = TblUsers.Select(s => new {s.LoginName, s.Id}).ToList();
        var tmp = tmpList.FirstOrDefault(u => string.Equals(u.LoginName, user.UserLoginEmail, StringComparison.InvariantCultureIgnoreCase));

        //Nuer User
        if (tmp == null)
        {
            var newUserPassword = AppCrypt.GeneratePassword(10);
            var u = new TableUser
            {
                PasswordHash = AppCrypt.CumputeHash(newUserPassword),
                LoginName = user.UserLoginEmail,
                DefaultLanguage = "de",
                LoginConfirmed = false,
                IsAdmin = false,
                AgbVersion = "1.0.0",
                CreatedAtUtc = DateTime.UtcNow,
                RefreshToken = AppCrypt.GeneratePassword(),
                JwtToken = AppCrypt.GeneratePassword(),
                ConfirmationToken = AppCrypt.GeneratePassword(),
                Locked = false
            };
            TblUsers.Add(u);
            SaveChanges();
            user.UserId = u.Id;
            return (true, u, newUserPassword);
        }

        user.UserId = tmp.Id;
        return (false, null!, string.Empty);
    }
}