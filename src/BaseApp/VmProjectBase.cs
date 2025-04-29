// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaseApp.ViewModel;
using BDA.Common.Exchange.Enum;
using Biss.Apps;
using Biss.Apps.Collections;
using Biss.Apps.Interfaces;
using Biss.Common;
using Biss.Core.Logging.Events;
using Biss.Dc.Core;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     Logeinträge für UI
    /// </summary>
    public class BissEventsLoggerData : BissEventsLoggerEventArgs, IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>Ist das aktuelle Element selektiert</summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

#pragma warning disable CS0067
        /// <summary>
        ///     Ereignis wenn das Element selektiert wurde im Element
        /// </summary>
        public event EventHandler<BissSelectableEventArgs>? Selected;
#pragma warning restore CS0067

        #endregion
    }


    /// <summary>
    ///     <para>Basis View Model projektspezifisch</para>
    ///     Klasse ViewModelBase. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase : VmBase
    {
        private static Stream _defaultImage = null!;
        private static VmMenu _menu = null!;

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:DesignInstanceBissEventsLoggerEventArgs}"
        /// </summary>
        public static BissEventsLoggerData DesignInstanceBissEventsLoggerEventArgs = new BissEventsLoggerData();

        /// <summary>
        ///     Logeintäge
        /// </summary>
        public static BxObservableCollection<BissEventsLoggerData> LogEntries = new BxObservableCollection<BissEventsLoggerData>();

        internal static bool _showDarkTheme;

        private CancellationTokenSource? _ctsLoaded;

        /// <summary>
        ///     Basis View Model für alle ViewModel
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="args"></param>
        /// <param name="subTitle"></param>
        protected VmProjectBase(string pageTitle, object? args = null, string subTitle = "") : base(pageTitle, args, subTitle)
        {
            if (_defaultImage == null!)
            {
                _defaultImage = Images.ReadImageAsStream(EnumEmbeddedImage.Logo_png);
            }

            Appeared += VmProjectBase_Appeared;
            Loaded += VmProjectBase_Loaded;
            Disappeared += VmProjectBase_Disappeared;
        }

        #region Properties

        /// <summary>
        ///     Aktuelles Menü
        /// </summary>
        public static VmMenu? CurrentVmMenu { get; protected set; } = null;

        /// <summary>
        ///     Wpf
        /// </summary>
        public bool IsWpf => DeviceInfo.Plattform == EnumPlattform.XamarinWpf;

        /// <summary>
        ///     Warten auf DC-Daten welche beim "späteren" Start asynchron geladen werden
        /// </summary>
        public bool WaitForProjectDataLoadedAfterDcConnected { get; set; }

        /// <summary>
        ///     Bild
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public Stream Image => _defaultImage;
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        ///     Zugriff auf das Hauptmenü
        /// </summary>
        public static VmMenu GetVmBaseStatic
        {
            get
            {
                if (_menu == null!)
                {
                    _menu = new VmMenu();
                }

                return _menu;
            }
        }

        /// <summary>
        ///     Aktuelle Type der App falls das Projekt aus mehr als einer App besteht
        /// </summary>
        public EnumAppType CurrentAppType => (EnumAppType) CurrentAppTypeId;

        /// <summary>
        ///     Developer Infos in den Views anzeigen (bei Release nicht)
        /// </summary>
        public bool ShowDeveloperInfos => AppSettings.Current().AppConfigurationConstants != 0;

        #endregion

        /// <summary>
        ///     View für das Projekt parametrieren
        /// </summary>
        /// <param name="modalPage"></param>
        public void SetViewProperties(bool modalPage = false)
        {
            if (TabletMode)
            {
                if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
                {
                    View.ShowMenu = false;
                }
                else if (modalPage == false)
                {
                    View.ShowMenu = true;
                }

                View.ShowFooter = false;
                View.ShowHeader = true;
                View.ShowSubTitle = true;

                if (Dc.CoreConnectionInfos != null!)
                {
                    View.ShowUser = Dc.CoreConnectionInfos.UserOk;
                }

                if (modalPage)
                {
                    View.ShowBack = true;
                    View.ShowUser = false;
                }
                else
                {
                    if (Dc.CoreConnectionInfos != null!)
                    {
                        View.ShowUser = Dc.CoreConnectionInfos.UserOk;
                    }
                }
            }
            else
            {
                View.ShowFooter = !modalPage;
                View.ShowHeader = true;
                View.ShowBack = modalPage;
                View.ShowMenu = !modalPage;
                View.ShowUser = true;
            }

            CurrentVmMenu?.UpdateFooterButton();

            UpdateConnectionState(Dc.ConnectionState);
        }

        /// <summary>
        ///     Falls der Hardware Back Button gedrückt wurde sollte bevor die App geschlossen wird noch die ViewMain aufgemacht
        ///     werden
        /// </summary>
        public override void OpenMainPage()
        {
            _gcmdHome.IsSelected = true;
        }

        /// <summary>
        ///     Welche View soll initial getartet weren
        /// </summary>
        public static void LaunchFirstView()
        {
            Logging.Log.LogInfo($"[{nameof(VmProjectBase)}]({nameof(LaunchFirstView)})");

            _ = CManager!.InitHigh();

            if (CurrentVmMenu != null!)
            {
                CurrentVmMenu.UpdateMenu();
                CurrentVmMenu.UpdateFooterButton();

                _gcmdHome.IsSelected = true;

                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                GetCurrentView?.GetViewModel()?.View?.Refresh(true);
            }
            else
            {
                Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(LaunchFirstView)}): Menu still null");
            }
        }

        /// <summary>
        ///     Projekt Initialisieren
        /// </summary>
        /// <param name="currentAppType">User oder Admin App</param>
        /// <param name="showDarkTheme">Dunkles Theme aktiv.</param>
        /// <returns></returns>
        public static Task InitializeApp(EnumAppType currentAppType = EnumAppType.User, bool showDarkTheme = true)
        {
            CurrentAppTypeId = (int) currentAppType;
            var bisslogConfig = new BissEventsLoggerConfiguration {LogLevel = LogLevel.Trace};
            bisslogConfig.NewLogEntry += BisslogConfigOnNewLogEntry;
#if DEBUG
            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Trace)
                .AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Trace));
#else
            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Warning).AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Warning));
#endif
            SetMainViewModel(typeof(VmMain));

            try
            {
                Logging.Log.LogTrace("Init App");
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"Init App Error: {e}");
                throw;
            }

            return Task.CompletedTask;
        }

        private void VmProjectBase_Appeared(object sender, EventArgs e)
        {
            Dc.ConnectionChanged += VmProjectBase_DcOnConnectionChanged;
        }

        private void VmProjectBase_Loaded(object sender, EventArgs e)
        {
            CurrentVmMenu?.UpdateFooterButton();
        }

        private void VmProjectBase_Disappeared(object sender, EventArgs e)
        {
            Dc.ConnectionChanged -= VmProjectBase_DcOnConnectionChanged;
        }

        private void VmProjectBase_DcOnConnectionChanged(object sender, EnumDcConnectionState e)
        {
            UpdateConnectionState(e);
        }

        /// <summary>
        ///     Logeintrag intern merken für die Developer Infos View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void BisslogConfigOnNewLogEntry(object sender, BissEventsLoggerEventArgs e)
        {
            if (Dispatcher == null!)
            {
                return;
            }

            Dispatcher.RunOnDispatcher(() =>
            {
                var data = new BissEventsLoggerData
                {
                    LogLevel = e.LogLevel,
                    Message = e.Message,
                    TimeStamp = e.TimeStamp
                };

                LogEntries.Insert(0, data);
                if (LogEntries.Count > 100)
                {
                    LogEntries.RemoveAt(LogEntries.Count - 1);
                }
            });
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            await base.OnAppearing(view).ConfigureAwait(true);
            if (WaitForProjectDataLoadedAfterDcConnected)
            {
                if (Dc.ConnectionState == EnumDcConnectionState.Connected && !ProjectDataLoadedAfterDcConnected)
                {
                    _ctsLoaded = new CancellationTokenSource();
                    View.BusySet("Lade Daten ...");
                    await Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(200, _ctsLoaded.Token).ConfigureAwait(false);
                            if (ProjectDataLoadedAfterDcConnected)
                            {
                                break;
                            }
                        } while (!_ctsLoaded.IsCancellationRequested);
                    }).ConfigureAwait(true);
                    View.BusyClear();
                }
            }
        }

        // ReSharper disable once RedundantOverriddenMember
        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            return base.OnActivated(args);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            if (!Dc.AutoConnect)
            {
                DcStartAutoConnect();
            }

            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.DcExUser.DataChangedEvent -= DcExUserOnDataChangedEvent;
            return base.OnDisappearing(view);
        }

        #endregion

        ///// <summary>
        /////     Crash ins Appcenter schicken
        ///// </summary>
        ///// <param name="exception"></param>
        ///// <param name="additionalInfos"></param>
        //public static void LogCrash4Appcenter(Exception exception, string additionalInfos = "")
        //{
        //    if (string.IsNullOrWhiteSpace(additionalInfos))
        //    {
        //        Crashes.TrackError(exception);
        //    }
        //    else
        //    {
        //        var er = ErrorAttachmentLog.AttachmentWithText(additionalInfos, "AdditionalInfo");

        //        Crashes.TrackError(exception, attachments: er);
        //    }
        //}
    }
}