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
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Geräte Ansicht</para>
    ///     Klasse VmDevices. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewDevices")]
    public class VmDevices : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDevices.DesignInstance}"
        /// </summary>
        public static VmDevices DesignInstance = new VmDevices();

        /// <summary>
        ///     VmDevices
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VmDevices() : base("Geräte", subTitle: "Siehe die Gerätedaten ein!")
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = false;
            View.ShowMenu = true;
            RefreshData().ConfigureAwait(true);
        }

        #region Properties

        /// <summary>
        ///     Zeige Message Entry
        /// </summary>
        public bool ShowEntry { get; set; }

        /// <summary>
        ///     Modus: Dc an alle
        /// </summary>
        public bool SendDcAll { get; set; }

        /// <summary>
        ///     Modus: Push an alle
        /// </summary>
        public bool SendPushAll { get; set; }

        /// <summary>
        ///     Modus: Dc
        /// </summary>
        public bool SendDc { get; set; }

        /// <summary>
        ///     Show Details Id
        /// </summary>
        public long? ShowDetailsId { get; set; }

        /// <summary>
        ///     Modus: Push
        /// </summary>
        public bool SendPush { get; set; }

        /// <summary>
        ///     Geräte
        /// </summary>
        public List<TableDevice> Devices { get; private set; }

        /// <summary>
        ///     Dc Connections
        /// </summary>
        public IDcConnections DcConnections { get; set; }

        /// <summary>
        ///     ServerRemoteCalls
        /// </summary>
        public IServerRemoteCalls ServerRemoteCalls { get; set; }

        /// <summary>
        ///     Sort Command
        /// </summary>
        public VmCommand CmdSort { get; set; }

        /// <summary>
        ///     Einzelnen Dc Benachrichtigen
        /// </summary>
        public VmCommand CmdDcNotify { get; set; }

        /// <summary>
        ///     Einzelnen Push Benachrichtigen
        /// </summary>
        public VmCommand CmdPushNotify { get; set; }

        /// <summary>
        ///     Command zum Löschen eines Gerätes
        /// </summary>
        public VmCommand CmdDeleteDevice { get; set; }

        /// <summary>
        ///     Alle Dc Benachrichtigen
        /// </summary>
        public VmCommand CmdDcNotifyAll { get; set; }

        /// <summary>
        ///     Alle Push Benachrichtigen
        /// </summary>
        public VmCommand CmdPushNotifyAll { get; set; }

        /// <summary>
        ///     Zeige Geräte Details
        /// </summary>
        public VmCommand CmdShowDetails { get; set; }

        /// <summary>
        ///     Nachrichten Entry
        /// </summary>
        public VmEntry MessageEntry { get; set; }

        #endregion

        /// <summary>
        ///     Daten erneuern
        /// </summary>
        /// <returns></returns>
        public async Task RefreshData()
        {
            View.BusySet(delay: 0);
            try
            {
                await using var db = new Db();
                Devices = db.TblDevices.ToList();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }

            View.BusyClear();
        }

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
            CmdSort = new VmCommand(string.Empty, obj => { Devices = Devices.OrderBy(x => x.GetType().GetProperty((string) obj)!.GetValue(x, null)).ToList(); }, glyph: Glyphs.Keyboard_arrow_up);

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
                        var tmpUserId = (await db.TblDevices.FirstOrDefaultAsync(u => u.Id == device.Id).ConfigureAwait(true)).TblUserId;
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

                            await RefreshData().ConfigureAwait(true);

                            _ = await MsgBox.Show("Gerät wurde erfolgreich gelöscht!").ConfigureAwait(true);
                        }
                    }
                }
            }, "Benutzer löschen", Glyphs.Bin_2);

            CmdDcNotifyAll = new VmCommand("Alle Benachrichtigen (via Dc)", async () =>
            {
                var devices = GetActiveDevices();

                if (!devices.Any())
                {
                    _ = await MsgBox.Show("Keine aktiven Geräte mehr gefunden.", "Keine Geräte mehr aktiv", icon: VmMessageBoxImage.Error).ConfigureAwait(true);
                    await RefreshData().ConfigureAwait(true);
                    return;
                }

                _ = await Nav.ToViewWithResult(typeof(VmMessage), new SendMessageData {Devices = devices.ToList(), SendVia = SendViaEnum.Dc}, cachePage: false).ConfigureAwait(true);
            }, /*canExecuteNoParams: () => Devices.Any(x => IsRunning(x)),*/ glyph: Glyphs.Send_email);

            CmdDcNotify = new VmCommand("", async obj =>
            {
                if (obj is TableDevice device)
                {
                    var currentConnectedDeviceIds = DcConnections.GetClients().Select(x => x.DeviceId);
                    if (!currentConnectedDeviceIds.Contains(device.Id))
                    {
                        _ = await MsgBox.Show("Gerät wurde womöglich in der Zwischenzeit abgemeldet.", "Gerät nicht mehr aktiv", icon: VmMessageBoxImage.Error).ConfigureAwait(true);
                        await RefreshData().ConfigureAwait(true);
                        return;
                    }

                    _ = await Nav.ToViewWithResult(typeof(VmMessage), new SendMessageData {Devices = new List<TableDevice> {device}, SendVia = SendViaEnum.Dc}, cachePage: false).ConfigureAwait(true);
                }
            }, "Benachrichtigen (via Dc)", Glyphs.Send_email);

            CmdPushNotifyAll = new VmCommand("Alle Benachrichtigen (via Push)", async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                var devices = db.TblDevices.Where(d => !string.IsNullOrWhiteSpace(d.DeviceToken));
                if (devices.Any())
                {
                    _ = await Nav.ToViewWithResult(typeof(VmMessage), new SendMessageData {Devices = devices.ToList(), SendVia = SendViaEnum.Push}, cachePage: false).ConfigureAwait(true);
                }
            }, /*canExecuteNoParams: () => Devices.Any(x => IsPushAvailable(x)),*/ glyph: Glyphs.Alarm_bell);

            CmdPushNotify = new VmCommand("", async selectedDevice =>
            {
                if (selectedDevice is TableDevice device)
                {
                    _ = await Nav.ToViewWithResult(typeof(VmMessage), new SendMessageData {Devices = new List<TableDevice> {device}, SendVia = SendViaEnum.Push}, cachePage: false).ConfigureAwait(true);
                }
            }, "Benachrichtigen (via Push)", Glyphs.Alarm_bell);
            MessageEntry = new VmEntry(EnumVmEntryBehavior.Instantly, "Nachricht", "Nachricht");

            CmdShowDetails = new VmCommand("", async obj =>
            {
                if (!(obj is TableDevice device))
                {
                    throw new InvalidOperationException();
                }

                if (ShowDetailsId == device.Id)
                {
                    ShowDetailsId = null;
                    await RefreshData().ConfigureAwait(true);
                    return;
                }

                ShowDetailsId = device.Id;
                await RefreshData().ConfigureAwait(true);
            }, "Zeige Details", Glyphs.Phone_action_information_1);
        }

        private List<TableDevice> GetActiveDevices()
        {
            var activeDevices = DcConnections.GetClients();

            var activeDeviceIds = activeDevices.Select(x => x.DeviceId);

            return Devices.Where(x => activeDeviceIds.Contains(x.Id)).ToList();
        }
    }
}