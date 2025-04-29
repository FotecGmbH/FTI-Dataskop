// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using BDA.Service.AppConnectivity.Helper;
using BDA.Service.EMail.Services;
using Biss.Apps.Base;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BDA.Service.AppConnectivity.DataConnector
{
    /// <summary>
    ///     <para>Angemeldeter Benutzer in der App</para>
    ///     Klasse DcExPersons. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

        /// <summary>
        ///     Device fordert Daten für DcExUser
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public async Task<ExUser> GetDcExUser(long deviceId, long userId)
        {
            Logging.Log.LogTrace($"[DcExUser]({nameof(GetDcExUser)}): DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[DcExUser]({nameof(GetDcExUser)}): DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[DcExUser]({nameof(GetDcExUser)}): UserId {userId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var u = db.GetUserWithdependences(userId);

            if (u == null)
            {
                throw new Exception($"[DcExUser]({nameof(GetDcExUser)}): UserId {userId} does not exist in Database!");
            }

            return u.ToExUser();
        }

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcStoreResult> SetDcExUser(long deviceId, long userId, ExUser data)
        {
            Logging.Log.LogTrace($"[DcExUser]({nameof(SetDcExUser)}): DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[DcExUser]({nameof(SetDcExUser)}): DeviceId {deviceId} invalid!");
            }

            if (data == null!)
            {
                throw new ArgumentException($"[DcExUser]({nameof(SetDcExUser)}): {nameof(data)} is null!");
            }


#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var un = data.LoginName.ToUpperInvariant();
            TableUser? dbUser;
            var newUser = false;

            //Neuer User?
            if (userId <= 0)
            {
                var checkUsers = db.TblUsers.Select(s => new {s.LoginName, s.Id}).ToList();
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                checkUsers.ForEach(f => f.LoginName.ToUpperInvariant());
                var check = checkUsers.FirstOrDefault(c => c.LoginName == un);
                if (check != null)
                {
                    dbUser = db.GetUserWithdependences(userId);
                }
                else
                {
                    newUser = true;
                    dbUser = new TableUser
                    {
                        PasswordHash = data.PasswordHash4NewUser,
                        LoginName = data.LoginName,
                        DefaultLanguage = data.DefaultLanguage,
                        LoginConfirmed = false,
                        IsAdmin = false,
                        AgbVersion = "1.0.0",
                        CreatedAtUtc = DateTime.UtcNow,
                        RefreshToken = AppCrypt.GeneratePassword(),
                        JwtToken = AppCrypt.GeneratePassword(),
                        ConfirmationToken = AppCrypt.GeneratePassword(),
                        Locked = false
                    };
                }
            }
            else
            {
                dbUser = db.GetUserWithdependences(userId);
            }

            if (dbUser == null)
            {
                throw new ArgumentException($"[DcExUser]({nameof(SetDcExUser)}): {nameof(dbUser)} is null!");
            }

            dbUser.FirstName = data.FirstName;
            dbUser.LastName = data.LastName;
            dbUser.PushTags = data.PushTags;
            dbUser.Setting10MinPush = data.Setting10MinPush;


            //Gelöschte Token
            foreach (var l in dbUser.TblAccessToken.ToList())
            {
                if (!data.Tokens.Select(s => s.DbId).Contains(l.Id))
                {
                    db.TblAccessToken.Remove(l);
                }
            }

            //Neue Token
            foreach (var d in data.Tokens)
            {
                if (d.DbId <= 0)
                {
                    dbUser.TblAccessToken.Add(new TableAccessToken
                    {
                        GuiltyUntilUtc = d.GuiltyUntilUtc,
                        TblUserId = dbUser.Id,
                        Token = d.Token
                    });
                }
            }

            // Bild löschen
            if (dbUser.TblUserImageId.HasValue && dbUser.TblUserImageId.Value != data.UserImageDbId)
            {
                await FilesDbBlob.DeleteFile(db, dbUser.TblUserImageId.Value).ConfigureAwait(false);
                dbUser.TblUserImageId = null;
            }

            // Bild aktualisieren
            if (data.UserImageDbId > 0 && !dbUser.TblUserImageId.HasValue)
            {
                var image = await db.TblFiles.FirstOrDefaultAsync(i => i.Id == data.UserImageDbId).ConfigureAwait(false);
                if (image == null)
                {
                    throw new ArgumentException($"[DcExUser]({nameof(SetDcExUser)}): Image with Id {data.UserImageDbId} not found in TableFiles!");
                }

                dbUser.TblUserImage = image;
            }

            if (newUser)
            {
                await db.TblUsers.AddAsync(dbUser).ConfigureAwait(false);
            }

            await db.SaveChangesAsync().ConfigureAwait(false);

            if (newUser)
            {
                var email = new UserAccountEmailService(_razorEngine);
                await email.SendValidationMail(dbUser, deviceId, db).ConfigureAwait(false);
            }
            else
            {
                var data2Send = dbUser.ToExUser();
                await SendDcExUser(data2Send, userId: dbUser.Id).ConfigureAwait(false);
            }

            _ = Task.Run(async () => await SendReloadList(EnumReloadDcList.ExCompanyUsers).ConfigureAwait(false));

            return new DcStoreResult();
        }

        #endregion

        #region Passwort

        /// <summary>
        ///     Device fordert Daten für DcExUserPassword
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public Task<ExUserPassword> GetDcExUserPassword(long deviceId, long userId)
        {
            throw new InvalidOperationException($"[DcExUser]({nameof(GetDcExUserPassword)}): Can not read this data!");
        }

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcStoreResult> SetDcExUserPassword(long deviceId, long userId, ExUserPassword data)
        {
            Logging.Log.LogTrace($"[DcExUser]({nameof(SetDcExUserPassword)}): DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[DcExUser]({nameof(SetDcExUserPassword)}): DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[DcExUser]({nameof(SetDcExUserPassword)}): UserId {userId} invalid!");
            }

            if (data == null!)
            {
                throw new ArgumentException($"[DcExUser]({nameof(SetDcExUserPassword)}): {nameof(data)} is null!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
            if (dbUser == null)
            {
                throw new Exception($"[DcExUser]({nameof(SetDcExUserPassword)}): UserId {userId} not found in Database!");
            }

            if (dbUser.PasswordHash != data.CurrentPasswordHash)
            {
                throw new Exception($"[DcExUser]({nameof(SetDcExUserPassword)}): UserId {userId} Password does not match!");
            }

            dbUser.PasswordHash = data.NewPasswordHash;
            await db.SaveChangesAsync().ConfigureAwait(false);

            return new DcStoreResult();
        }

        #endregion
    }
}