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
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.AppConnectivity.Helper;
using BDA.Service.EMail.Services;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Server;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp.Model;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Resources;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Benutzer View</para>
    ///     Klasse VmUsers. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewUsers")]
    public class VmUsers : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUsers.DesignInstance}"
        /// </summary>
        public static VmUsers DesignInstance = new VmUsers();

        /// <summary>
        ///     VmUsers
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VmUsers() : base("Benutzerdaten", subTitle: "Siehe die Benutzerdaten ein!")
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = false;
            View.ShowMenu = true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshData();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        #region Properties

        /// <summary>
        ///     Server Remote Calls
        /// </summary>
        public IServerRemoteCalls ServerRemoteCalls { get; set; } = null!;

        /// <summary>
        ///     Razor Engine
        /// </summary>
        public ICustomRazorEngine? RazorEngine { get; set; }

        /// <summary>
        ///     Liste von Benutzern
        /// </summary>
        public List<ExUserModel> Users { get; private set; } = new();

        /// <summary>
        ///     Benutzer Geräte
        /// </summary>
        public List<TableDevice> UserDevices { get; private set; } = new();

        /// <summary>
        ///     Bearbeiten Command
        /// </summary>
        public VmCommand CmdDevices { get; set; }

        /// <summary>
        ///     Sortiert Command
        /// </summary>
        public VmCommand CmdSort { get; set; }

        /// <summary>
        ///     Command für Dc Notify (alle Geräte des Benutzers)
        /// </summary>
        public VmCommand CmdDcNotifyUser { get; set; }

        /// <summary>
        ///     Command für Dc Notify (nur ein Gerät)
        /// </summary>
        public VmCommand CmdDcNotifyDevice { get; set; }

        /// <summary>
        ///     Send Email Command
        /// </summary>
        public VmCommand CmdSendMail { get; set; }

        /// <summary>
        ///     Command für Push Notify (alle Geräte des Benutzers)
        /// </summary>
        public VmCommand CmdPushNotifyUser { get; set; }

        /// <summary>
        ///     Command für Push Notify (nur ein Gerät)
        /// </summary>
        public VmCommand CmdPushNotifyDevice { get; set; }

        /// <summary>
        ///     Command für Admin Änderung
        /// </summary>
        public VmCommand CmdChangeAdmin { get; set; }

        /// <summary>
        ///     Command für Locking Änderung
        /// </summary>
        public VmCommand CmdChangeLocking { get; set; }


        /// <summary>
        ///     Command für Login Bestätigung Änderung
        /// </summary>
        public VmCommand CmdChangeLoginConfirmed { get; set; }

        /// <summary>
        ///     Nachrichten Entry
        /// </summary>
        public VmEntry MessageEntry { get; set; }

        /// <summary>
        ///     Id des ausgewählten Benutzers
        /// </summary>
        public long? SelectedUserId { get; set; }

        /// <summary>
        ///     Dc Connection
        /// </summary>
        public IDcConnections DcConnection { get; set; }

        /// <summary>
        ///     Command zum Löschen des Benutzers
        /// </summary>
        public VmCommand CmdDeleteUser { get; set; }

        /// <summary>
        ///     Command zum Löschen des Gerätes
        /// </summary>
        public VmCommand CmdDeleteDevice { get; set; }

        #endregion

        /// <summary>
        ///     Ob App auf Gerät gerade läuft.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool IsRunning(TableDevice device)
        {
            return device != null! && device.IsAppRunning;
        }

        /// <summary>
        ///     Ob Push möglich auf dieses Gerät.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool IsPushAvailable(TableDevice device)
        {
            if (device == null!)
            {
                return false;
            }

            return (device.Plattform == EnumPlattform.XamarinAndroid || device.Plattform == EnumPlattform.XamarinIos) && !string.IsNullOrWhiteSpace(device.DeviceToken);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
#pragma warning disable CA1506
        protected override void InitializeCommands()
#pragma warning restore CA1506
        {
            CmdDeleteDevice = new VmCommand("", async obj =>
            {
                if (obj is TableDevice device)
                {
                    var r = await MsgBox.Show($"Gerät {device.DeviceName} wirklich löschen?", "Gerät löschen", VmMessageBoxButton.YesNo, VmMessageBoxImage.Hand).ConfigureAwait(true);
                    if (r == VmMessageBoxResult.Ok || r == VmMessageBoxResult.Yes)
                    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        var tmpUserId = (await db.TblDevices.FirstOrDefaultAsync(u => u.Id == device.Id).ConfigureAwait(false)).TblUserId;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                        var deleteDeviceRes = await DbHelper.DeleteDevice(db, device.Id).ConfigureAwait(true);
                        if (!deleteDeviceRes)
                        {
                            _ = await MsgBox.Show("Gerät konnte NICHT gelöscht werden!").ConfigureAwait(true);
                        }
                        else
                        {
                            var dbUser = await db.TblUsers.Where(i => i.Id == tmpUserId).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);

                            if (dbUser != null!)
                            {
                                var data = dbUser.ToExUser();
                                await ((ServerRemoteCalls) ServerRemoteCalls).SendDcExUser(data, null, dbUser.Id).ConfigureAwait(true);
                            }

                            await db.SaveChangesAsync().ConfigureAwait(true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            RefreshData();

                            _ = await MsgBox.Show("Gerät wurde erfolgreich gelöscht!").ConfigureAwait(true);
                        }
                    }
                }
            }, "Benutzer löschen", Glyphs.Bin_2);

            CmdDevices = new VmCommand("", obj =>
            {
                var user = (TableUser) obj;
                if (SelectedUserId == user.Id)
                {
                    SelectedUserId = null;

                    RefreshData();
                    return;
                }

                SelectedUserId = user.Id;
                RefreshData(SelectedUserId);
            }, "Geräte des Benutzers", Glyphs.Phone_action_information_1);

            CmdSort = new VmCommand("", obj => { Users = Users.OrderBy(x => x.GetType().GetProperty((string) obj)!.GetValue(x, null)).ToList(); }, glyph: Glyphs.Keyboard_arrow_up);

            CmdSendMail = new VmCommand("", async obj =>
            {
                if (obj is TableUser user)
                {
                    var data = new SendMessageData {SendVia = SendViaEnum.Email, Users = new List<TableUser> {user}};

                    _ = await Nav.ToViewWithResult(typeof(VmMessage), data).ConfigureAwait(true);
                }
            }, "Email an den Benutzer senden", Glyphs.Email_action_send_1);

            CmdPushNotifyUser = new VmCommand("", async obj =>
            {
                var data = new SendMessageData {SendVia = SendViaEnum.Push};

                if (obj is TableUser user)
                {
                    data.Devices = (await GetActiveDevicesOfUser(user).ConfigureAwait(true)).Where(IsPushAvailable).ToList();

                    if (data.Devices.Any())
                    {
                        _ = await Nav.ToViewWithResult(typeof(VmMessage), data).ConfigureAwait(true);
                    }
                    else
                    {
                        _ = await MsgBox.Show("Keine aktiven push-fähigen Geräte beim Benutzer! Senden einer Nachricht über Push nicht möglich!", "Keine aktiven Geräte").ConfigureAwait(true);
                    }
                }
            }, "Alle Geräte des Benutzers per Push benachrichtigen", Glyphs.Alarm_bell);

            CmdPushNotifyDevice = new VmCommand("", async obj =>
            {
                var data = new SendMessageData {SendVia = SendViaEnum.Push};

                if (obj is TableDevice device)
                {
                    data.Devices = new List<TableDevice> {device};
                }

                if (!data.Devices.Any())
                {
                    return;
                }

                _ = await Nav.ToViewWithResult(typeof(VmMessage), data).ConfigureAwait(true);
            }, "Dieses Gerät per Push benachrichtigen", Glyphs.Alarm_bell);

            CmdDcNotifyUser = new VmCommand("", async obj =>
            {
                var data = new SendMessageData {SendVia = SendViaEnum.Dc};


                if (obj is TableUser user)
                {
                    data.Devices = user.TblDevices.Where(IsRunning).ToList();
                    Logging.Log.LogInfo($"Für User Id {user.Id} wurden {data.Devices.Count()} Geräte gefunden bei denen push enabled ist. ");
                }

                if (data.Devices.Any())
                {
                    _ = await Nav.ToViewWithResult(typeof(VmMessage), data).ConfigureAwait(true);
                }
                else
                {
                    _ = await MsgBox.Show("Keine aktiven Geräte beim Benutzer! Senden einer Nachricht über DC nicht möglich!", "Keine aktiven Geräte").ConfigureAwait(true);
                }

                RefreshData();
            }, "Alle Geräte des Benutzers per DC benachrichtigen", Glyphs.Send_email);

            CmdDcNotifyDevice = new VmCommand("", async obj =>
            {
                var data = new SendMessageData {SendVia = SendViaEnum.Dc};

                if (obj is TableDevice device)
                {
                    data.Devices = new List<TableDevice> {device};
                }

                if (!data.Devices.Any())
                {
                    return;
                }

                _ = await Nav.ToViewWithResult(typeof(VmMessage), data).ConfigureAwait(true);
            }, "Dieses Gerät per DC benachrichtigen", Glyphs.Send_email);

            CmdChangeAdmin = new VmCommand("", async obj =>
            {
                if (obj is not TableUser user)
                {
                    return;
                }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                db.TblUsers.FirstOrDefault(u => u.Id == user.Id)!.IsAdmin = !user.IsAdmin;
                await db.SaveChangesAsync().ConfigureAwait(true);
                RefreshData();
            }, "Admin Status ändern");

            CmdChangeLocking = new VmCommand("", async obj =>
            {
                if (obj is not TableUser user)
                {
                    return;
                }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                db.TblUsers.FirstOrDefault(u => u.Id == user.Id)!.Locked = !user.Locked;
                await db.SaveChangesAsync().ConfigureAwait(true);
                RefreshData();


                // Info an die aktiven Clients sich abzumelden
                var dbUser = await db.TblUsers.Where(i => i.Id == user.Id).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);
                if (dbUser != null! && dbUser.Locked)
                {
                    var data = dbUser.ToExUser();
                    await ((ServerRemoteCalls) ServerRemoteCalls).SendDcExUser(data, null, dbUser.Id).ConfigureAwait(true);
                }
            }, "Gesperrt Status ändern");

            CmdChangeLoginConfirmed = new VmCommand("", async obj =>
            {
                if (obj is not TableUser user)
                {
                    return;
                }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                db.TblUsers.FirstOrDefault(u => u.Id == user.Id)!.LoginConfirmed = !user.LoginConfirmed;
                await db.SaveChangesAsync().ConfigureAwait(true);
                RefreshData();
            }, "Login Bestätigungsstatus ändern");

            CmdDeleteUser = new VmCommand("", async obj =>
            {
                if (obj is TableUser user)
                {
                    var r = await MsgBox.Show($"Benutzer {user.LoginName} wirklich löschen?", "Benutzer löschen", VmMessageBoxButton.YesNo, VmMessageBoxImage.Hand).ConfigureAwait(true);
                    if (r == VmMessageBoxResult.Ok || r == VmMessageBoxResult.Yes)
                    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                        db.TblUsers.FirstOrDefault(u => u.Id == user.Id)!.Locked = true;

                        var dbUser = await db.TblUsers.Where(i => i.Id == user.Id).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);
                        if (dbUser != null! && dbUser.Locked)
                        {
                            var data = dbUser.ToExUser();
                            await ((ServerRemoteCalls) ServerRemoteCalls).SendDcExUser(data, null, dbUser.Id).ConfigureAwait(true);
                        }

                        await db.SaveChangesAsync().ConfigureAwait(true);

                        var res = await DbHelper.DeleteUserAccount(db, user.Id).ConfigureAwait(true);
                        if (res)
                        {
                            _ = await MsgBox.Show("Benutzer wurde erfolgreich gelöscht!").ConfigureAwait(true);
                        }
                        else
                        {
                            _ = await MsgBox.Show("Benutzer konnte NIHCT gelöscht werden!").ConfigureAwait(true);
                        }

                        RefreshData();
                    }
                }
            }, "Benutzer löschen", Glyphs.Bin_2);

            MessageEntry = new VmEntry(EnumVmEntryBehavior.Instantly, "Nachricht", "Nachricht");
        }

        private async Task RefreshData(long? userId = null)
        {
            View.BusySet("Lade Daten...");
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using (var db = new Db())
            {
                var users = db.TblUsers.Include(t => t.TblDevices).ToList();
                Users = users.Select(GetUserModel).ToList();
                UserDevices = userId == null ? new List<TableDevice>() : db.TblDevices.Where(t => t.TblUserId == userId).ToList();
            }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            View.BusyClear();
        }

        private ExUserModel GetUserModel(TableUser user)
        {
            var exUser = new ExUserModel
            {
                Id = user.Id,
                AgbVersion = user.AgbVersion,
                LastLogin = GetMaxDateTime(user),
                //DataVersion = user.DataVersion,
                //IsArchived = user.IsArchived,
                LoginName = user.LoginName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsAdmin = user.IsAdmin,
                PasswordHash = user.PasswordHash,
                RefreshToken = user.RefreshToken,
                JwtToken = user.JwtToken,
                Locked = user.Locked,
                LoginConfirmed = user.LoginConfirmed,
                CreatedAtUtc = user.CreatedAtUtc,
                DefaultLanguage = user.DefaultLanguage,
                ConfirmationToken = user.ConfirmationToken,
                PushTags = user.PushTags,
                TblDevices = user.TblDevices,
                TblAccessToken = user.TblAccessToken,
                //TblChats = user.TblChats,
                Setting10MinPush = user.Setting10MinPush,
                TblPermissions = user.TblPermissions,
                TblUserImage = user.TblUserImage,
                TblUserImageId = user.TblUserImageId,
            };

            return exUser;
        }

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        /// <summary>
        ///     Letztes Login Datum herausfinden.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private DateTime GetMaxDateTime(TableUser user)
        {
            var datetimes = user.TblDevices.Select(t => t.LastLogin).ToList();
            datetimes.Add(DateTime.MinValue);
            return datetimes.Max() ?? DateTime.MinValue;
        }

        private async Task<IEnumerable<TableDevice>> GetActiveDevicesOfUser(TableUser user)
        {
            if (user == null!)
            {
                throw new NullReferenceException("[VmUser:GetActiveDevicesOfUser] user is Null!");
            }

            // ReSharper disable once AccessToModifiedClosure
            var activeDevicesOfUser = DcConnection.GetClients().Where(x => x.UserId == user.Id);
            var activeDeviceIds = activeDevicesOfUser.Select(x => x.DeviceId);

            if (!user.TblDevices.Any())
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                // ReSharper disable once AccessToModifiedClosure
                user = await db.TblUsers.Include(x => x.TblDevices).FirstOrDefaultAsync(x => x.Id == user.Id).ConfigureAwait(true);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            return user!.TblDevices.Where(x => activeDeviceIds.Contains(x.Id));
        }
    }
}