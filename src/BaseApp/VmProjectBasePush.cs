// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Linq;
using BaseApp.Connectivity;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.Push;
using Biss.Apps.Toast.Options;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     <para>Gemeinsame Methoden für Push</para>
    ///     Klasse VmProjectBaseConnectivity. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        #region Properties

        /// <summary>
        ///     Zugriff auf Push
        /// </summary>
        public static BcPush Push => PushExtension.BcPush();

        #endregion

        /// <summary>
        ///     Pushes abonnieren
        /// </summary>
        public static void SubNotifications()
        {
            PushExtension.BcPush().PushReceived += OnPushReceived;
            PushExtension.BcPush().PushTokenUpdated += OnPushTokenUpdated;
            PushExtension.BcPush().LaunchOptionsChanged += OnLaunchOptionsChanged;
        }

        /// <summary>
        ///     Pushes abmelden.
        /// </summary>
        public static void UnsubNotifications()
        {
            PushExtension.BcPush().PushReceived -= OnPushReceived;
            PushExtension.BcPush().PushTokenUpdated -= OnPushTokenUpdated;
            PushExtension.BcPush().LaunchOptionsChanged -= OnLaunchOptionsChanged;
        }

        /// <summary>
        ///     Gibt es Launchoptions - Start via Notification
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private static bool CheckLaunchOptions()
        {
            try
            {
                var opts = PushExtension.BcPush().LaunchOptions;
                if (opts != null! && opts.Any())
                {
                    Logging.Log.LogInfo($"[{nameof(VmProjectBase)}]({nameof(CheckLaunchOptions)}): LaunchOptions gefunden!");
                    return true;
                }

                PushExtension.BcPush().SetBadgeCount(PushExtension.BcPush().LaunchOptions.Count);

                Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(CheckLaunchOptions)}): Normaler Launch");
                return false;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(CheckLaunchOptions)}): {e}");
                return false;
            }
        }

        /// <summary>
        ///     Push wurde gedrückt - Infos von der gedrückten Notification bekommen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLaunchOptionsChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Add)
            {
                // Push gedrückt - neue LaunchOptions hinzugefügt
                Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(OnLaunchOptionsChanged)}): New: {e.Element}");
            }

            if (e.Action == CollectionChangeAction.Refresh)
            {
                // LaunchOptions geleert
                Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(OnLaunchOptionsChanged)}): Clear");
            }
        }

        /// <summary>
        ///     Push wurde empfangen während die App aktiv ist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPushReceived(object sender, BissNotificationOptions e)
        {
            PushExtension.BcPush().SetBadgeCount(1);
            Logging.Log.LogInfo($"Push erhalten: {e.Title} - {e.Description}");
            Dispatcher!.RunOnDispatcher(async () => { await Toast.ShowAsync(e).ConfigureAwait(true); });
        }

        /// <summary>
        ///     Push Token aktualisieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void OnPushTokenUpdated(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PushExtension.BcPush().Token))
            {
                var dc = BcDataConnectorExtensions.BcDataConnector(null!)?.GetDc<DcProjectBase>();

                if (dc == null!)
                {
                    return;
                }

                dc.DcExDeviceInfo.Data.DeviceToken = PushExtension.BcPush().Token;

                var storeRes = await dc.DcExDeviceInfo.StoreData().ConfigureAwait(false);
                if (!storeRes.DataOk)
                {
                    Logging.Log.LogError($"[DC] DeviceTokenUpdateError Error({storeRes.ErrorType}): {storeRes.ServerExceptionText}");
                }
                else
                {
                    Logging.Log.LogTrace("[DC] DeviceToken Updated");
                }
            }
        }
    }
}