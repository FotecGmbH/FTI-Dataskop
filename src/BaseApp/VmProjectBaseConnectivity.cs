// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using Biss.Apps.Connectivity;
using Biss.Apps.Enum;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     <para>Gemeinsame Methoden für Connectivity / DC</para>
    ///     Klasse VmProjectBaseConnectivity. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        /// <summary>
        ///     Für OnSleep
        /// </summary>
        public static bool DcDoNotAutoDisconnect = false;

        /// <summary>
        ///     Wurde (diese) allgemeine Meldung bereits angezeigt?
        /// </summary>
        private static string _lastShownCommonMessageInSession = string.Empty;

        /// <summary>
        ///     Wurde die Meldung das es (diese) Neue Version bereits gibt?
        /// </summary>
        private static Version? _lastVersionChecked;

        private static bool _projectDataLoadedAfterDcConnected;
        private bool _isReconnectedInSession;

        #region Properties

        /// <summary>
        ///     Projektbezogene Daten geladen
        /// </summary>
        public bool ProjectDataLoadedAfterDcConnected
        {
            get => _projectDataLoadedAfterDcConnected;
            set => _projectDataLoadedAfterDcConnected = value;
        }

        #endregion

        /// <summary>
        ///     Ereignis für Projekt Daten geladen
        /// </summary>
        public static event EventHandler? ProjectDataLoaded;

        /// <summary>
        ///     Auto Connect starten - in der richtigen View ausführen
        /// </summary>
        public void DcStartAutoConnect()
        {
            try
            {
                _ = Task.Run(() =>
                {
                    Dispatcher!.RunOnDispatcher(async () =>
                    {
                        Dc.DeviceOnlineConnected += DcOnDeviceOnlineConnectedForUpdateDeviceInfos;
                        Dc.UserAndDeviceOnlineConnected += DcOnUserAndDeviceOnlineConnected;
                        await Dc.OpenConnection(true).ConfigureAwait(true);

                        Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(DcStartAutoConnect)}): Update Menu");
                        CurrentVmMenu?.UpdateMenu();
                    });
                });
                Dc.CommonDataFromServerReceived += DcOnCommonDataFromServerReceived;
                Dc.DcExSettingsInDb.Data.PropertyChanged += DataOnPropertyChanged;
                Dc.ConnectionChanged += DcOnConnectionChanged;
                Dc.LogoutByDc += DcOnLogoutByDc;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(DcStartAutoConnect)}): {e}");
            }
        }

        /// <summary>
        ///     Prüfen ob gerade eine Verbindung mit dem Server besteht inklusive MsgBox Ausgabe.
        /// </summary>
        /// <returns>Verbunden</returns>
        public async Task<bool> CheckConnected()
        {
            if (Dc.ConnectionState != EnumDcConnectionState.Connected)
            {
                await MsgBox.Show(ResCommon.MsgNotConnected, ResCommon.MsgTitleNotConnected).ConfigureAwait(true);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Methode von Ereignis für Projekt Daten geladen
        /// </summary>
        protected virtual void OnProjectDataLoaded()
        {
            var handler = ProjectDataLoaded;
            handler?.Invoke(this, null!);
        }


        /// <summary>
        ///     Sobald das Gerät online verbunden ist die Gerätestammdaten am Server aktualisieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void DcOnDeviceOnlineConnectedForUpdateDeviceInfos(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000).ConfigureAwait(false);
                    await DeviceInfoUpdate().ConfigureAwait(false);
                });


                await Dc.DcExSettingsInDb.WaitDataFromServerAsync().ConfigureAwait(false);
                CheckSettingsInDb();

                //App ohne User bzw. nicht angemeldet
                if (!Dc.CoreConnectionInfos.UserOk)
                {
                    UpdateProjektData();
                }
            }
        }

        /// <summary>
        ///     DC Logt User automatisch ab wenn Account nicht mehr passt (oder gelöscht wurde)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DcOnLogoutByDc(object sender, EventArgs e)
        {
            // User hat falsches PWD eingegeben - Login Seite
            if (Dc.CoreConnectionInfos == null! ||
                Dc.CoreConnectionInfos.UserId <= 0)
            {
                return;
            }

            Dispatcher!.RunOnDispatcher(async () =>
            {
                await MsgBox.Show("Automatischer Login mit dem aktuellen Benutzer ist nicht möglich.", "Login nicht möglich").ConfigureAwait(true);
                CurrentVmMenu?.UpdateMenu();
                GCmdHome.Execute(null!);
            });
        }

        #region Projektabhängig Daten laden

        /// <summary>
        ///     Listen bei eingeloggten User laden wenn dieser (wieder) eingeloggt wird
        /// </summary>
        private void UpdateProjektData()
        {
            Dispatcher!.RunOnDispatcher(async () =>
            {
                //App ohne User bzw. nicht angemeldet
                if (!Dc.CoreConnectionInfos.UserOk)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    await Dc.DcExCompanies.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);

                    await Dc.DcExProjects.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);
                    await Dc.DcExMeasurementDefinition.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);

                    Dc.DcExGateways.ReadListData(reload: true);
                    Dc.DcExIotDevices.ReadListData(reload: true);
                    Dc.DcExGlobalConfig.ReadListData(reload: true);
                    Dc.DcExCompanyUsers.ReadListData(reload: true);
                }
                else
                {
                    await Dc.DcExCompanies.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);
                    await Dc.DcExProjects.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);
                    await Dc.DcExMeasurementDefinition.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);

                    Dc.DcExGateways.ReadListData(reload: true);
                    Dc.DcExIotDevices.ReadListData(reload: true);
                    Dc.DcExGlobalConfig.ReadListData(reload: true);
                    Dc.DcExCompanyUsers.ReadListData(reload: true);
                }
#pragma warning restore CS0618 // Type or member is obsolete
                _ctsLoaded?.Cancel();
                ProjectDataLoadedAfterDcConnected = true;
                OnProjectDataLoaded();
            });
        }

        #endregion

        /// <summary>
        ///     Verbindungs - Header
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void UpdateConnectionState(EnumDcConnectionState e)
        {
            if (Dc.ConnectingCounter <= 1)
            {
                View.EnumHeaderOfflineType = EnumHeaderInfo.None;
                return;
            }

            switch (e)
            {
                case EnumDcConnectionState.Connected:
                    View.EnumHeaderOfflineType = EnumHeaderInfo.None;
                    break;
                case EnumDcConnectionState.Disconnected:
                    View.MessageOffline = ResCommon.EnumConnDisonnected;
                    View.EnumHeaderOfflineType = EnumHeaderInfo.Warning;
                    break;
                case EnumDcConnectionState.Connecting:
                    View.MessageOffline = ResCommon.EnumConnConnecting;
                    View.EnumHeaderOfflineType = EnumHeaderInfo.Warning;
                    break;
                case EnumDcConnectionState.Disconeccting:
                    View.MessageOffline = ResCommon.EnumConnDisconnecting;
                    View.EnumHeaderOfflineType = EnumHeaderInfo.Warning;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        /// <summary>
        ///     Verbindungsstatus hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcOnConnectionChanged(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                if (_isReconnectedInSession)
                {
                    Dc.UserAndDeviceOnlineConnected += DcOnUserAndDeviceOnlineConnected;
                }

                _isReconnectedInSession = true;
            }
            else if (e == EnumDcConnectionState.Disconnected)
            {
                ProjectDataLoadedAfterDcConnected = false;
            }
        }

        /// <summary>
        ///     Neue DbSettings wurden vom Server empfangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckSettingsInDb();
        }

        /// <summary>
        ///     Versionsüberprüfung und allgemeine Server Meldung
        /// </summary>
        private void CheckSettingsInDb()
        {
            if (Dc.DcExSettingsInDb.DataSource != EnumDcDataSource.FromServer)
            {
                return;
            }

            //Allgemeine Meldung als MsgBox und als Info - Projektabhängig finalisieren
            if (!string.IsNullOrEmpty(Dc.DcExSettingsInDb.Data.CommonMessage))
            {
                if (string.IsNullOrEmpty(_lastShownCommonMessageInSession) ||
                    !string.Equals(_lastShownCommonMessageInSession, Dc.DcExSettingsInDb.Data.CommonMessage, StringComparison.CurrentCultureIgnoreCase))
                {
                    _lastShownCommonMessageInSession = Dc.DcExSettingsInDb.Data.CommonMessage;
                    Logging.Log.LogTrace($"[Common Message]: {_lastShownCommonMessageInSession}");
                    Dispatcher!.RunOnDispatcher(async () => { await MsgBox.Show(_lastShownCommonMessageInSession, ResCommon.CmdInfo).ConfigureAwait(true); });
                }

                Dispatcher!.RunOnDispatcher(() => View.SetMessage(Dc.DcExSettingsInDb.Data.CommonMessage));
            }
            else
            {
                Dispatcher!.RunOnDispatcher(() => View.EnumHeaderInfoType = EnumHeaderInfo.None);
            }

            var currentVersion = new Version(AppSettings.Current().AppVersion);
            Logging.Log.LogTrace($"[Version]: App: {currentVersion}, DbCurrent: {Dc.DcExSettingsInDb.Data.CurrentAppVersion}, DbMin: {Dc.DcExSettingsInDb.Data.MinAppVersion}");
            if (currentVersion < Dc.DcExSettingsInDb.Data.MinAppVersion)
            {
                Dispatcher.RunOnDispatcher(async () =>
                {
                    await MsgBox.Show(string.Format(ResCommon.MsgUpdateMandatory, Dc.DcExSettingsInDb.Data.CurrentAppVersion), ResCommon.MsgTitleUpdateMandatory).ConfigureAwait(true);
                    Nav.QuitApp();
                });
            }
            else if (currentVersion < Dc.DcExSettingsInDb.Data.CurrentAppVersion)
            {
                if (_lastVersionChecked == null || _lastVersionChecked != Dc.DcExSettingsInDb.Data.CurrentAppVersion)
                {
                    Dispatcher.RunOnDispatcher(async () => { await MsgBox.Show(string.Format(ResCommon.MsgUpdateAvailable, Dc.DcExSettingsInDb.Data.CurrentAppVersion), ResCommon.MsgTitleUpdateAvailable).ConfigureAwait(true); });
                }

                _lastVersionChecked = Dc.DcExSettingsInDb.Data.CurrentAppVersion;
            }
        }

        /// <summary>
        ///     User Stammdaten haben sich geändert.
        ///     Trigger auf "Locked" mit automatischem ausloggen eines Users wenn das Gerät gerade online ist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExUserOnDataChangedEvent(object sender, DataChangedEventArgs e)
        {
            if (e.Changed)
            {
                if (Dc.DcExUser.Data.Locked)
                {
                    Dispatcher!.RunOnDispatcher(async () =>
                    {
                        await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                        await Dc.Logout().ConfigureAwait(true);
                        CurrentVmMenu?.UpdateMenu();
                        GCmdHome.Execute(null!);
                    });
                }
            }
        }

        /// <summary>
        ///     User (und Device) sind online (registriert)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DcOnUserAndDeviceOnlineConnected(object sender, EnumDcConnectionState e)
        {
            await Dc.DcExUser.WaitDataFromServerAsync(forceUpdate: true).ConfigureAwait(true);
            Dc.UserAndDeviceOnlineConnected -= DcOnUserAndDeviceOnlineConnected;

            if (Dc.DcExUser.Data.Locked)
            {
                Dispatcher!.RunOnDispatcher(async () =>
                {
                    await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                    await Dc.Logout().ConfigureAwait(true);
                    CurrentVmMenu?.UpdateMenu();
                    GCmdHome.Execute(null!);
                });
            }
            else
            {
                Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(DcOnUserAndDeviceOnlineConnected)}): Update Menu");
                InvokeDispatcher(() => CurrentVmMenu?.UpdateMenu());
                Dc.DcExUser.DataChangedEvent += DcExUserOnDataChangedEvent;
            }

            UpdateProjektData();
        }

        /// <summary>
        ///     Daten über das aktuelle Device an die Cloud senden. Wird unter anderem für die Notifizierungen benötigt.
        ///     Sollte aufgerufen werden wenn der User eingeloggt ist. Wenn die App keine User unterstützt:
        ///     Db und Funktionen umbauen (das die Devices ohne User angelegt werden) ODER
        ///     Einen "ALLUSER" anlegen => wenn ersichtlich das eventuell mal ein Login hinzukommt
        /// </summary>
        private async Task DeviceInfoUpdate()
        {
            if (!Dc.DeviceRegisteredOnline)
            {
                return;
            }

            if (DeviceInfo.Plattform == EnumPlattform.XamarinIos || DeviceInfo.Plattform == EnumPlattform.XamarinAndroid)
            {
                var token = string.Empty;

                try
                {
                    token = Push.Token;
                }
                catch (InvalidOperationException e)
                {
                    Logging.Log.LogError($"[Push] Token fetch failed. Error: {e}");
                }

                Dc.DcExDeviceInfo.Data.DeviceToken = string.IsNullOrEmpty(token) ? string.Empty : token;
            }

            Dc.DcExDeviceInfo.Data.DeviceHardwareId = DeviceInfo.DeviceHardwareId;
            Dc.DcExDeviceInfo.Data.Plattform = DeviceInfo.Plattform;
            Dc.DcExDeviceInfo.Data.DeviceIdiom = DeviceInfo.DeviceIdiom;
            Dc.DcExDeviceInfo.Data.OperatingSystemVersion = DeviceInfo.OperatingSystemVersion;
            Dc.DcExDeviceInfo.Data.DeviceType = DeviceInfo.DeviceType;
            Dc.DcExDeviceInfo.Data.Manufacturer = DeviceInfo.Manufacturer;
            Dc.DcExDeviceInfo.Data.Model = DeviceInfo.Model;
            Dc.DcExDeviceInfo.Data.AppVersion = AppSettings.Current().AppVersion;
            Dc.DcExDeviceInfo.Data.CurrentAppType = CurrentAppType;
            Dc.DcExDeviceInfo.Data.DeviceName = DeviceInfo.DeviceName;
            Dc.DcExDeviceInfo.Data.ScreenResolution = DeviceInfo.ScreenResolution;

            if (DeviceInfo.Plattform == EnumPlattform.Wpf || DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
            {
                Dc.DcExDeviceInfo.Data.DeviceType = ResCommon.EnumDeviceTypePc;
                Dc.DcExDeviceInfo.Data.DeviceName = Environment.MachineName;
                Dc.DcExDeviceInfo.Data.OperatingSystemVersion = Environment.OSVersion.Version.Build.ToString();
            }

            if (DeviceInfo.Plattform == EnumPlattform.Web)
            {
                Dc.DcExDeviceInfo.Data.DeviceType = $"{ResCommon.EnumDeviceTypeBrowser} {DeviceInfo.DeviceType}";
            }

            var storeRes = await Dc.DcExDeviceInfo.StoreData().ConfigureAwait(false);
            if (!storeRes.DataOk)
            {
                Logging.Log.LogError($"[DC] DeviceInfoUpdate Error({storeRes.ErrorType}): {storeRes.ServerExceptionText}");
            }
            else
            {
                Logging.Log.LogTrace("[DC] DeviceInfo Updated");
                Dc.DeviceOnlineConnected -= DcOnDeviceOnlineConnectedForUpdateDeviceInfos;
            }
        }

        #region Connectivity

        /// <summary>
        ///     Data Connector
        /// </summary>
        public DcProjectBase Dc => this.GetDc<DcProjectBase>();

        /// <summary>
        ///     Service Access
        /// </summary>
        public SaProjectBase Sa => this.GetSa<SaProjectBase>();

        #endregion

        #region CommonCommands vom Server

        /// <summary>
        ///     Allgemeine Daten wurden vom Server empfangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcOnCommonDataFromServerReceived(object sender, CommonDataEventArgs e)
        {
            if (e == null! || e.Data == null!)
            {
                return;
            }

            Logging.Log.LogTrace($"[CommonDataFromServerReceived] Key {e.Data.Key} Data {e.Data.Value}");

            if (!Enum.TryParse(e.Data.Key, true, out EnumDcCommonCommandsClient command))
            {
                Logging.Log.LogError($"[DC] ReceivedDcCommonData Key {e.Data.Key} is not a valid Member of EnumDcCommonCommandsClient");
                return;
            }

            switch (command)
            {
                case EnumDcCommonCommandsClient.CommonMsg:
                    CallCommonMsg(e.Data.Value);
                    break;
                case EnumDcCommonCommandsClient.ReloadDcList:
                    Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(DcOnCommonDataFromServerReceived)}): Not supported in V8!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Allgemeine Meldung von ConnectivityHost empfangen - diese als MsgBox anzeigen
        /// </summary>
        /// <param name="data"></param>
        private async void CallCommonMsg(string data)
        {
            var msg = BissDeserialize.FromJson<EcDcCommonMessage>(data);
            if (msg == null!)
            {
                Logging.Log.LogError("[DC CallCommonMsg] Can not deserialize data");
                return;
            }

            Logging.Log.LogInfo($"[DC CallCommonMsg] {msg.Title}:{msg.Message}");
            await MsgBox.Show(msg.Message, msg.Title).ConfigureAwait(true);
        }

        #endregion
    }
}