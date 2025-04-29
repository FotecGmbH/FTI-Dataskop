// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using Biss.Apps;
using Biss.Apps.Service.Push;
using Biss.Core.Logging.Events;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp.ViewModel;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace ConnectivityHost.BaseApp
{
    /// <summary>
    ///     <para>Basis View Model projektspezifisch</para>
    ///     Klasse ViewModelBase. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase : VmBase
    {
        private static Stream _defaultImage = null!;
        private static VmMenu _menu = null!;

        /// <summary>
        ///     Logeintäge
        /// </summary>
        public static ObservableCollection<BissEventsLoggerEventArgs> LogEntries = new ObservableCollection<BissEventsLoggerEventArgs>();

        /// <summary>
        ///     Aktuelles Menü
        /// </summary>
        public static VmMenu? CurrentVmMenu = null;

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
        }

        #region Properties

        /// <summary>
        ///     Bild
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public Stream Image => _defaultImage;
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        ///     Zugriff auf Push.
        /// </summary>
        public static PushService Push => PushService.Instance;

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
        ///     Im Connectiviy Host IMMER EnumAppType.ConnectivityHost
        /// </summary>
        public EnumAppType CurrentAppType => (EnumAppType) CurrentAppTypeId;

        #endregion

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
            _ = CManager!.InitHigh();
            _gcmdHome.IsSelected = true;
        }

        /// <summary>
        ///     Projekt Initialisieren
        /// </summary>
        /// <returns></returns>
        public static Task InitializeApp()
        {
            CurrentAppTypeId = (int) EnumAppType.ConnectivityHost;
            SetMainViewModel(typeof(VmMain));

            var bisslogConfig = new BissEventsLoggerConfiguration {LogLevel = LogLevel.Trace};

            bisslogConfig.NewLogEntry += BisslogConfigOnNewLogEntry;

            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Trace).AddConsole().SetMinimumLevel(LogLevel.Trace)
                .AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Trace));

#if DEBUG
            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Trace).AddConsole().SetMinimumLevel(LogLevel.Trace)
                .AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Trace));
#else
                        Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Warning).AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Warning));
#endif

            PushService.Initialize(WebSettings.Current());

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

        private static void BisslogConfigOnNewLogEntry(object? sender, BissEventsLoggerEventArgs e)
        {
            try
            {
                LogEntries.Insert(0, e);
                if (LogEntries.Count > 10000)
                {
                    LogEntries.RemoveAt(LogEntries.Count - 1);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}