// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;

namespace BaseApp.ViewModel
{
    #region Hilfsklassen

    /// <summary>
    ///     <para>Zugriffsrechte auf Firmen des User</para>
    ///     Klasse UiPermission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UiPermission : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     Rolle des Users in der Firma
        /// </summary>
        public EnumUserRole UserRole { get; set; }

        /// <summary>
        ///     Rechte des Users bei der Firma
        /// </summary>
        public EnumUserRight UserRight { get; set; }

        /// <summary>
        ///     Firma
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        ///     Ui Rolle
        /// </summary>
        public string UserRoleUi
        {
            get
            {
                switch (UserRole)
                {
                    case EnumUserRole.User:
                        return "Firmenbenutzer";
                    case EnumUserRole.Admin:
                        return "Firmenadministrator";
                    default:
                        return "?";
                }
            }
        }

        /// <summary>
        ///     Ui Rechte
        /// </summary>
        public string UserRightUi
        {
            get
            {
                switch (UserRight)
                {
                    case EnumUserRight.Read:
                        return "Daten lesen";
                    case EnumUserRight.ReadWrite:
                        return "Daten bearbeiten";
                    default:
                        return "?";
                }
            }
        }
        /// <summary>
        /// IsSelected
        /// </summary>
        public bool IsSelected { get; set; }
        /// <summary>
        /// CanEnableIsSelect
        /// </summary>
        public bool CanEnableIsSelect { get; set; }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Selected
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event EventHandler<BissSelectableEventArgs> Selected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        #endregion
    }

    #endregion

    /// <summary>
    ///     <para>View Model für Benutzer</para>
    ///     Klasse VmUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewUser", true)]
    public class VmUser : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUser.DesignInstance}"
        /// </summary>
        public static VmUser DesignInstance = new VmUser();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static ExUserDevice DesignInstanceExUserDevice = new ExUserDevice();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static ExAccessToken DesignInstanceExAccessToken = new ExAccessToken();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static UiPermission DesignInstanceExUserPremission = new UiPermission();

        /// <summary>
        ///     VmUser
        /// </summary>
        public VmUser() : base(ResViewUser.LblTitle, subTitle: ResViewUser.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Zugriffsrechte
        /// </summary>
        public ObservableCollection<UiPermission> UiPermissions { get; set; } = new ObservableCollection<UiPermission>();

        /// <summary>
        ///     Test Command
        /// </summary>
        public VmCommand CmdLogout { get; private set; } = null!;

        /// <summary>
        ///     Benutzer bearbeiten
        /// </summary>
        public VmCommand CmdEdit { get; private set; } = null!;

        /// <summary>
        ///     Passwort ändern
        /// </summary>
        public VmCommand CmdChangePassword { get; private set; } = null!;

        /// <summary>
        ///     Token hinzufügen
        /// </summary>
        public VmCommand CmdAddToken { get; private set; } = null!;

        /// <summary>
        ///     Token löschen
        /// </summary>
        public VmCommand CmdDeleteToken { get; private set; } = null!;

        /// <summary>
        ///     Token in die Zwischenablage kopieren
        /// </summary>
        public VmCommand CmdCopyToken { get; private set; } = null!;

        /// <summary>
        ///     Benutzer Id des User
        /// </summary>
        public string UserId => $"User Id: {Dc.CoreConnectionInfos.UserId}";

        /// <summary>
        ///     Geräte Id des User
        /// </summary>
        public string DeviceId => $"Device Id: {Dc.CoreConnectionInfos.DeviceId}";

        #endregion

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            LoadUiPermissions();
            Dc.DcExUser.DataChangedEvent += DcExUserOnDataChangedEvent;

            return base.OnLoaded();
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdLogout = new VmCommand(ResViewUser.CmdLogout, async () =>
            {
                View.BusySet("Melde ab...", 100);
                CurrentVmMenu?.SetMenuBusy(true);

                ProjectDataLoadedAfterDcConnected = false;
                Dc.ConnectionChanged += OnLogoutClientDcConnected;
                await Dc.Logout().ConfigureAwait(true);
                Dispatcher!.RunOnDispatcher(() =>
                {
                    CurrentVmMenu?.UpdateMenu();
                    CurrentVmMenu?.SetMenuBusy(false);
                    GCmdHome.Execute(null!);
                    View.BusyClear();
                });
            }, glyph: Glyphs.User_logout);

            CmdEdit = new VmCommand(ResViewUser.CmdEdit, async () => { await Nav.ToViewWithResult(typeof(VmEditUser)).ConfigureAwait(false); }, glyph: Glyphs.Pencil);

            CmdChangePassword = new VmCommand(ResViewUser.CmdChangePassword, async () => { await Nav.ToViewWithResult(typeof(VmEditUserPassword)).ConfigureAwait(false); }, glyph: Glyphs.Password_lock_1);

            CmdAddToken = new VmCommand("Token erstellen", async () =>
            {
                var g = Guid.NewGuid().ToString();
                g += Guid.NewGuid().ToString();
                g = g.Replace("-", "", StringComparison.InvariantCultureIgnoreCase).Trim();

                var t = new ExAccessToken
                {
                    DbId = 0,
                    GuiltyUntilUtc = DateTime.UtcNow.AddYears(1),
                    Token = g
                };
                Dc.DcExUser.Data.Tokens.Add(t);

                View.BusySet("Sichere Token ...");
                var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
                View.BusyClear();
                if (!(r is {DataOk: true}))
                {
                    await MsgBox.Show($"Fehler beim Sichern des Token: {r.ServerExceptionText}").ConfigureAwait(true);
                    // Workaround bis fix in Biss.Collections
                    try
                    {
                        Dc.DcExUser.Data.Tokens.Remove(t);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }
                }
            }, glyph: Glyphs.Add);

            CmdDeleteToken = new VmCommand("", async t =>
            {
                if (t is ExAccessToken token)
                {
                    // Workaround bis fix in Biss.Collections
                    try
                    {
                        Dc.DcExUser.Data.Tokens.Remove(token);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }


                    View.BusySet("Lösche Token ...");
                    var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
                    View.BusyClear();
                    if (!(r is {DataOk: true}))
                    {
                        await MsgBox.Show($"Fehler beim Löschen des Token: {r.ServerExceptionText}").ConfigureAwait(true);
                        Dc.DcExUser.Data.Tokens.Add(token);
                    }
                }
            }, glyph: Glyphs.Bin_2);

            CmdCopyToken = new VmCommand("", async t =>
            {
                if (t is ExAccessToken token)
                {
                    if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
                    {
                        var powershell = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "powershell",
                                Arguments = $"-command \"Set-Clipboard -Value \\\"{token.Token}\\\"\"",
                                CreateNoWindow = true
                            }
                        };
                        powershell.Start();
                        powershell.WaitForExit();
                    }
                    else if (DeviceInfo.Plattform == EnumPlattform.Web)
                    {
                        await Open.ClipboardSetText(token.Token).ConfigureAwait(true);
                    }
                    else
                    {
                        await Clipboard.SetTextAsync($"{token.Token}").ConfigureAwait(true);
                    }

                    await MsgBox.Show("Token wurde in die Zwischenablage kopiert").ConfigureAwait(true);
                }
            }, glyph: Glyphs.Copy_paste);
        }

        private void OnLogoutClientDcConnected(object sender, EnumDcConnectionState e)
        {
            {
                if (e == EnumDcConnectionState.Connected)
                {
                    DcOnDeviceOnlineConnectedForUpdateDeviceInfos(null!, e);
                    Dc.ConnectionChanged -= OnLogoutClientDcConnected;
                }
            }
        }

        private void DcExUserOnDataChangedEvent(object sender, DataChangedEventArgs e)
        {
            LoadUiPermissions();
        }

        private void LoadUiPermissions()
        {
            UiPermissions.Clear();
            if (Dc.DcExUser.Data.IsAdmin)
            {
                foreach (var company in Dc.DcExCompanies)
                {
                    UiPermissions.Add(new UiPermission
                    {
                        Company = company.Data.Information.Name,
                        UserRight = EnumUserRight.ReadWrite,
                        UserRole = EnumUserRole.Admin
                    });
                }
            }
            else
            {
                foreach (var premission in Dc.DcExUser.Data.Premissions)
                {
                    UiPermissions.Add(new UiPermission
                    {
                        Company = Dc.DcExCompanies.First(f => f.Index == premission.CompanyId).Data.Information.Name,
                        UserRight = premission.UserRight,
                        UserRole = premission.UserRole
                    });
                }
            }
        }
    }
}