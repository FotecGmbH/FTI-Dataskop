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
using BDA.Service.AppConnectivity.Helper;
using BDA.Service.EMail.Services;
using BDA.Service.Encryption;
using Biss.Dc.Core;
using Biss.Dc.Server.DcChat;
using Biss.Log.Producer;
using ConnectivityHost.DataConnector.Chat;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebExchange.Interfaces;

namespace BDA.Service.AppConnectivity.DataConnector
{
    /// <summary>
    ///     <para>Dc Implementierung der Funktionen am Server</para>
    ///     Klasse ServerRemoteCalls. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls : ServerRemoteCallBase, IServerRemoteCalls
    {
        private readonly ICustomRazorEngine _razorEngine;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISymmetricEncryption _symmetricEncryption;
        private ITriggerAgent? _tmpTriggerAgent;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="razorEngine">DI Razor Engine</param>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="symmetricEncryption"></param>
        public ServerRemoteCalls(ICustomRazorEngine razorEngine, IServiceScopeFactory serviceScopeFactory, ISymmetricEncryption symmetricEncryption)
        {
            _razorEngine = razorEngine;
            _serviceScopeFactory = serviceScopeFactory;
            ChatBase = new DcChat(this);
            _symmetricEncryption = symmetricEncryption;
        }

        #region Properties

        /// <summary>
        ///     Trigger Agent des Connectivity Host
        /// </summary>
        public ITriggerAgent TriggerAgent
        {
            get
            {
                if (_tmpTriggerAgent == null)
                {
                    var scope = _serviceScopeFactory.CreateScope();
                    var s = scope.ServiceProvider.GetService<ITriggerAgent>();
                    if (scope == null! || s == null!)
                    {
                        return null!;
                    }

                    _tmpTriggerAgent = s;
                }

                return _tmpTriggerAgent!;
            }
        }

        /// <summary>
        ///     Chat
        /// </summary>
        public DcChatServerBase ChatBase { get; }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Migration "alter V5" Apps
        /// </summary>
        /// <param name="oldInfos">Aktuelle Infos aus "ExUserAccountData"</param>
        /// <returns></returns>
        public Task<ExDcCoreInfos> MigrateDevice(DcMigrationInfos oldInfos)
        {
            throw new NotImplementedException();
        }

        /// <summary>Neues File wurde von einem Client empfangen</summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="userId">User Id - kann auch -1 sein - also kein User</param>
        /// <param name="fileName">Originaldateiname</param>
        /// <param name="file">Datei</param>
        /// <param name="commonData">Allgemeine zusätzliche Infos</param>
        /// <returns></returns>
        public async Task<DcTransferFileResult> TransferFile(long deviceId, long userId, string fileName, List<byte> file, string commonData)
        {
            if (file == null! || file.Count == 0)
            {
                throw new ArgumentException($"[DC]({nameof(TransferFile)}): File \"{fileName}\" is NULL or has no bytes!");
            }

            Logging.Log.LogTrace($"[DC]({nameof(TransferFile)}): DeviceId: {deviceId}, UserId: {userId}, FileName: {fileName} FileSize: {file.Count} bytes");
            return await FilesDbBlob.StoreFile(fileName, file).ConfigureAwait(false);
        }

        #endregion

        #region Device und User

        /// <summary>
        ///     Neues Gerät in der Datenbank anlegen.
        /// </summary>
        /// <returns>Db Id des neuen Gerätes</returns>
        public long RegisterNewDevice()
        {
            Logging.Log.LogTrace($"[DC]({nameof(RegisterNewDevice)}): RegisterNewDevice");
            using var db = new Db();
            var data = new TableDevice {LastDateTimeUtcOnline = DateTime.UtcNow};
            db.TblDevices.Add(data);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logging.Log.LogWarning($"[DC]({nameof(RegisterNewDevice)}): Error adding new Device: {e}");
                return -1;
            }

            Logging.Log.LogTrace($"[DC]({nameof(RegisterNewDevice)}) - generated Id: {data.Id}");
            return data.Id;
        }

        /// <summary>
        ///     Status (verbunden / nicht verbunden) eines Gerätes hat sich geändert.
        ///     Status in der DB ablegen bei Bedarf
        /// </summary>
        /// <param name="deviceId">id des Gerätes</param>
        /// <param name="state">Status</param>
        /// <param name="exception">Gegebenfalls Fehler der zum disconnect geführt hat</param>
        public async void DeviceConnectionChanged(long deviceId, EnumDcConnectionState state, Exception? exception = null)
        {
            if (exception != null)
            {
                Logging.Log.LogError($"[DC]({nameof(DeviceConnectionChanged)}): Device ({deviceId}) Disconnect with error: {exception}");
            }

            if (deviceId < 0)
            {
                Logging.Log.LogError($"[DC]({nameof(DeviceConnectionChanged)}): invalid device id {deviceId}, State {state}");
                return;
            }

            Logging.Log.LogTrace($"[DC]({nameof(DeviceConnectionChanged)}): deviceId {deviceId} state {state}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var device = db.TblDevices.FirstOrDefault(d => d.Id == deviceId);

            if (device == null)
            {
                Logging.Log.LogError($"[DC]({nameof(DeviceConnectionChanged)}): Device {deviceId} not found in DB, State {state}");
                return;
            }

            switch (state)
            {
                case EnumDcConnectionState.Connected:
                    device.IsAppRunning = true;
                    break;
                case EnumDcConnectionState.Disconnected:
                    device.IsAppRunning = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            device.LastDateTimeUtcOnline = DateTime.UtcNow;
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Benutzer Id anhand des Login (EMail) aus der DB laden. Sollte kein Entrag existieren dann -1 als Rückgabe
        /// </summary>
        /// <param name="loginName">E-Mail, Telefonnummer, was auch immer ...</param>
        /// <param name="deviceId">
        ///     Device Id aus Datenbank - eventuell für Traces falls jemand versucht gültige Loginnamen zu
        ///     finden
        /// </param>
        /// <returns>UserId</returns>
        public DcCheckUserLoginResult GetUserIdByLoginName(string loginName, long deviceId)
        {
            Logging.Log.LogTrace($"[DC]({nameof(GetUserIdByLoginName)}): {loginName} from device {deviceId}");
            if (string.IsNullOrEmpty(loginName))
            {
                Logging.Log.LogWarning($"[DC]({nameof(GetUserIdByLoginName)}): {loginName} is empty!");
                return new DcCheckUserLoginResult();
            }

            using var db = new Db();
            var un = loginName.ToUpperInvariant();
            var tmp = db.TblUsers.Select(s => new {s.LoginName, s.Id, s.LoginConfirmed, s.Locked}).ToList();
            if (tmp == null! || tmp.Count == 0)
            {
                return new DcCheckUserLoginResult();
            }

            if (tmp.All(u => u.LoginName.ToUpperInvariant() != un))
            {
                return new DcCheckUserLoginResult();
            }

            var user = tmp.First(u => u.LoginName.ToUpperInvariant() == un);
            return new DcCheckUserLoginResult
            {
                UserId = user.Id,
                LoginConfirmed = user.LoginConfirmed,
                Locked = user.Locked
            };
        }

        /// <summary>
        ///     Einen Benutzer anmelden. Bei Bedarf auch das Device dem Benutzer zuordnen.
        /// </summary>
        /// <param name="infos">Aktuelle Daten</param>
        /// <param name="userPasswortHash">Wenn sich der User einloggt - Passwort (Hash)</param>
        /// <returns>Aktualisierte Daten (bei Bedarf). null bei Fehler!</returns>
        public ExDcCoreInfos LoginUser(ExDcCoreInfos infos, string userPasswortHash)
        {
            using var db = new Db();
            var user = db.TblUsers.FirstOrDefault(u => u.Id == infos.UserId);
            var device = db.TblDevices.FirstOrDefault(d => d.Id == infos.DeviceId);

            if (user == null || device == null || infos == null!)
            {
                Logging.Log.LogError($"[DC]({nameof(LoginUser)}): Wrong Configuration!");
                return null!;
            }

            Logging.Log.LogTrace($"[DC]({nameof(LoginUser)}): Id {infos.UserId} from device {infos.DeviceId}");


            //Benutzer meldet sich neu an
            if (!string.IsNullOrEmpty(userPasswortHash))
            {
                if (user.PasswordHash != userPasswortHash)
                {
                    Logging.Log.LogWarning($"[DC]({nameof(LoginUser)}):: Wrong Passwort!");
                    return null!;
                }

                device.LastLogin = DateTime.UtcNow;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[DC]({nameof(LoginUser)}): Db SaveChanges Error: {e}");
                }
            }
            //Bereits angemeldeter Benutzer
            else
            {
                if (infos.RefreshToken != user.RefreshToken)
                {
                    Logging.Log.LogWarning($"[DC]({nameof(LoginUser)}): Wrong RefreshToken!");
                    return null!;
                }
            }

            //Gerät mit Benutzer "verehelichen"
            var informOtherDevices = false;
            if (device.TblUserId == null || device.TblUserId != infos.UserId)
            {
                device.TblUserId = user.Id;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[DC]({nameof(LoginUser)}): Db SaveChanges Error: {e}");
                }

                informOtherDevices = true;
            }

            device.LastLogin = DateTime.UtcNow;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[DC]({nameof(LoginUser)}): Db SaveChanges Error: {e}");
            }

            if (informOtherDevices)
            {
                _ = Task.Run(async () =>
                {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                    await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                    var dbUser = await db2.TblUsers.Where(i => i.Id == infos.UserId).Include(x => x.TblUserImage).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (dbUser != null)
                    {
                        var data = dbUser.ToExUser();
                        foreach (var client in ClientConnection.GetClients())
                        {
                            if (client.DeviceId == infos.DeviceId || client.UserId != infos.UserId)
                            {
                                continue;
                            }

                            await SendDcExUser(data, client.DeviceId).ConfigureAwait(false);
                        }
                    }
                });
            }

            infos.RefreshToken = user.RefreshToken;
            infos.JwtToken = user.JwtToken;
            return infos;
        }

        /// <summary>
        ///     Aktuellen Benutzer abmelden
        ///     Benuter Id aus der Device Tabelle entfernen
        /// </summary>
        /// <param name="deviceDeviceId">Device Id</param>
        /// <param name="deviceUserId">User Id</param>
        /// <returns></returns>
        public async Task<bool> LogoutUser(long deviceDeviceId, long deviceUserId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var user = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == deviceUserId).ConfigureAwait(false);
            var device = await db.TblDevices.FirstOrDefaultAsync(d => d.Id == deviceDeviceId).ConfigureAwait(false);

            if (user == null || device == null)
            {
                Logging.Log.LogError($"[DC]({nameof(LogoutUser)}): Wrong Configuration!");
                return false;
            }

            Logging.Log.LogTrace($"[DC]({nameof(LogoutUser)}): Id {deviceUserId} from device {deviceDeviceId}");

            if (device.TblUserId != null && device.TblUserId == deviceUserId)
            {
                device.TblUserId = null;
                try
                {
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[DC]({nameof(LogoutUser)}): Db SaveChanges Error: {e}");
                }
            }

            var dbUser = await db.TblUsers.Where(i => i.Id == deviceUserId).Include(x => x.TblUserImage).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);
            if (dbUser != null)
            {
                var data = dbUser.ToExUser();
                foreach (var client in ClientConnection.GetClients())
                {
                    if (client.DeviceId == deviceDeviceId || client.UserId != deviceUserId)
                    {
                        continue;
                    }

                    await SendDcExUser(data, client.DeviceId).ConfigureAwait(false);
                }
            }


            return true;
        }

        /// <summary>
        ///     Einen Benutzer aus dem System löschen bzw. seine Daten anonymisieren - laut Kundenwunsch
        /// </summary>
        /// <param name="userId">Benutzer Id</param>
        /// <returns></returns>
        public async Task<DcCommonCommandResult> DeleteUser(long userId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbUser = db2.GetUserWithdependences(userId);
            if (dbUser == null!)
            {
                throw new ArgumentException("User not found in DB");
            }

            var data2Send = dbUser.ToExUser();
            data2Send.Locked = true;
            //Bewirkt das der User auf allen aktiven Geräten ausgelogt wird
            await SendDcExUser(data2Send, userId: userId).ConfigureAwait(false);


            throw new NotImplementedException("Und jetzt? dbUser anonymisieren oder Löschen?");
        }

        #endregion
    }
}