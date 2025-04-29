// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>DbPosition konvertieren</para>
///     Klasse ConverterDbPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbPermission
{
    /// <summary>
    ///     TablePermission nach ExCompanyUser
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExCompanyUser ToExCompanyUser(this TablePermission t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToExCompanyUser)}): {nameof(t)}");
        }

        return new ExCompanyUser
        {
            CompanyId = t.TblCompanyId,
            LoginDoneByUser = t.TblUser.LoginConfirmed,
            UserId = t.TblUserId,
            UserLoginEmail = t.TblUser.LoginName,
            UserRight = t.UserRight,
            UserRole = t.UserRole,
            Fullname = $"{t.TblUser.FirstName} {t.TblUser.LastName}",
            UserImageLink = t.TblUser.TblUserImage == null! ? string.Empty : t.TblUser.TblUserImage.PublicLink
        };
    }

    /// <summary>
    ///     ExCompanyUser nach TablePermission
    /// </summary>
    /// <param name="u"></param>
    /// <param name="p"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTablePermission(this ExCompanyUser u, TablePermission p)
    {
        if (p == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToTablePermission)}): {nameof(p)}");
        }

        if (u == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToTablePermission)}): {nameof(u)}");
        }

        p.TblCompanyId = u.CompanyId;
        p.TblUserId = u.UserId;
        p.UserRight = u.UserRight;
        p.UserRole = u.UserRole;
    }
}