// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.EMail.Services;
using Biss.Dc.Core;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.AppConnectivity.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExCompanyUsers</para>
///     Klasse DcExCompanyUsers. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcCompanyUsers
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
    /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <param name="filter">Optionaler Filter für die Daten</param>
    /// <returns>Daten oder eine Exception auslösen</returns>
    public async Task<List<DcServerListItem<ExCompanyUser>>> GetDcExCompanyUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExCompanyUser>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        //Gastbenutzer
        if (userId <= 0)
        {
            return result;
        }

        //Superadmins
        var admins = db.TblUsers.Where(u => u.IsAdmin).Select(s => new
        {
            s.Id, s.FirstName, s.LastName, s.LoginName
        }).ToList();
        var noCompanyId = db.TblCompanies.Where(w => w.CompanyType == EnumCompanyTypes.NoCompany).Select(s => s.Id).First();

        var startId = long.MaxValue - 1 - admins.Count;
        foreach (var admin in admins)
        {
            var img = db.TblUsers.Where(f => f.Id == admin.Id).Include(i => i.TblUserImage).FirstOrDefault()?.TblUserImage?.PublicLink;
            var image = img == null! ? string.Empty : img;

            var d = new DcServerListItem<ExCompanyUser>
            {
                Data = new()
                {
                    IsSuperadmin = true,
                    UserRole = EnumUserRole.Admin,
                    CompanyId = noCompanyId,
                    Fullname = $"{admin.FirstName} {admin.LastName}",
                    LoginDoneByUser = true,
                    UserId = admin.Id,
                    UserLoginEmail = admin.LoginName,
                    UserRight = EnumUserRight.ReadWrite,
                    UserImageLink = image
                },
                SortIndex = startId,
                Index = startId,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            startId++;
            result.Add(d);
        }

        foreach (var p in db.GetCompanyUsers(userId))
        {
            var d = new DcServerListItem<ExCompanyUser>
            {
                Data = p.ToExCompanyUser(),
                SortIndex = p.Id,
                Index = p.Id,
#pragma warning disable CS0618 // Type or member is obsolete
                SecondId = secondId
#pragma warning restore CS0618 // Type or member is obsolete
            };
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcCompanyUsers sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExCompanyUsers(long deviceId, long userId, List<DcStoreListItem<ExCompanyUser>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var r = new DcListStoreResult
        {
            SecondId = secondId,
            StoreResult = new(),
            ElementsStored = new()
        };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        // ReSharper disable once RedundantAssignment
        TablePermission p = null!;
#pragma warning disable CS0219
        var anyDelete = false;
#pragma warning restore CS0219
        var modifiedUsers = new List<long>();

        foreach (var d in data)
        {
            (bool newUser, TableUser user, string pwd) informNewUserForNewAccount = (false, null!, string.Empty);
            var tmp = new DcListStoreResultIndexAndData();

            switch (d.State)
            {
                case EnumDcListElementState.New:
                    p = new TablePermission();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    informNewUserForNewAccount = db.CheckNewUserForPremission(d.Data);
                    break;
                case EnumDcListElementState.Modified:
                    p = db.TblPermissions.First(f => f.Id == d.Index);
                    r.ElementsStored++;
                    modifiedUsers.Add(p.TblUserId);
                    break;
                case EnumDcListElementState.Deleted:
                    p = db.TblPermissions.First(f => f.Id == d.Index);
                    modifiedUsers.Add(p.TblUserId);
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                db.TblPermissions.Remove(p);
                // ReSharper disable once RedundantAssignment
                anyDelete = true;
            }
            else
            {
                d.Data.ToTablePermission(p);
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblPermissions.Add(p);
            }

            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = p.Id;
                tmp.NewSortIndex = p.Id;
                r.NewIndex.Add(tmp);
            }

            if (informNewUserForNewAccount.newUser)
            {
                modifiedUsers.Add(informNewUserForNewAccount.user.Id);
                var email = new UserAccountEmailService(_razorEngine);
                await email.SendValidationMailWithoutDevice(informNewUserForNewAccount.user, db, informNewUserForNewAccount.pwd).ConfigureAwait(false);
            }
        }

        _ = Task.Run(async () =>
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            foreach (var id in modifiedUsers)
            {
                var dbUser = db2.GetUserWithdependences(id);
                if (dbUser == null)
                {
                    continue;
                }

                var data2Send = dbUser.ToExUser();
                await SendDcExUser(data2Send, userId: id).ConfigureAwait(false);
            }


            await Task.Delay(300).ConfigureAwait(true);
            //if (anyDelete)
            //{
            //    await SendReloadList(EnumReloadDcList.ExProject).ConfigureAwait(false);
            //}
            //ToDo: Mko Geht besser!
            await SendReloadList(EnumReloadDcList.ExCompanyUsers).ConfigureAwait(false);

            //foreach (var i in mod)
            //{
            //    TriggerAgent.ChangedGateway(EnumTriggerSources.ServiceAppConnectivity, i);
            //}
        });

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExCompanyUsers
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public Task<DcListSyncResultData<ExCompanyUser>> SyncDcExCompanyUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        throw new NotImplementedException();
    }

    #endregion
}