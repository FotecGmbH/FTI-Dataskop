// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Apps.Components;
using Biss.Apps.Connectivity.Interfaces;
using Biss.Apps.Connectivity.Sa;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;
using Biss.Apps.Push.Interfaces;
using Biss.Dc.Client;
using Exchange.Interfaces;

namespace Exchange
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class AppSettings :
        IAppSettings,
        IAppSettingsNavigation,
        IAppSettingsFiles,
        IAppSettingConnectivity,
        IAppSettingsPush,
        IAppSettingsLinks
    {
        private static AppSettings _current = null!;

        #region Properties

        #region IAppSettingsFiles

        public VmFiles BaseFiles { get; set; }

        #endregion IAppSettingsFiles

        #endregion

        /// <summary>
        ///     Get default Settings for AppSettings
        /// </summary>
        /// <returns></returns>
        public static AppSettings Current()
        {
            if (_current == null!)
            {
                _current = new AppSettings();
            }

            return _current;
        }

        #region IAppSettings

        /// <summary>
        ///     BISS SDK Lizenz
        /// </summary>
        public string License => "TOENTER";

        /// <summary>
        ///     Branch für diese Konfiguration
        /// </summary>
        public string BranchName => "TOENTER";

        /// <summary>
        ///     Mode für Constants 0 - DEFAULT RELEASE 1 - DEFAULT CUSTOMER BETA >1 - DEVELOPER
        /// </summary>
        public int AppConfigurationConstants => 2;

        /// <summary>
        ///     Produktversion
        /// </summary>
        public string AppVersion => "TOENTER";

        /// <summary>
        ///     App Name
        /// </summary>
        public string AppName => "BDA Dev";

        /// <summary>
        ///     App Ordner auf Plattform
        /// </summary>
        public string ProjectWorkUserFolder => "TOENTER";

        /// <summary>
        ///     App Identifier
        /// </summary>
        public string PackageName => "TOENTER";

        public ExLanguageContent LanguageContent { get; set; }

        #endregion IAppSettings

        #region IAppSettingsNavigation

        /// <summary>
        ///     App Orientation
        /// </summary>
        public EnumAppOrientation AppOrientationOverride => EnumAppOrientation.Auto;

        /// <summary>
        ///     In welchen Namespace befinden sich die Xamarin.Forms Views
        /// </summary>
        public string DefaultViewNamespace { get; set; } = "BaseApp.View.Xamarin.View.";

        /// <summary>
        ///     In welchen Assembly befinden sich die Xamarin.Forms Views
        /// </summary>
        public string DefaultViewAssembly { get; set; } = "BaseApp.View.Xamarin";

        public IQuitApplication QuitApplication { get; set; }
        public object Master { get; set; }
        public object MasterDetail { get; set; }
        public object Navigation { get; set; }
        public object Shell { get; set; }
        public object NavigationManager { get; set; }
        public INavArgsHelper NavArgsHelper { get; set; }
        public VmNavigator BaseNavigator { get; set; }

        #endregion IAppSettingsNavigation

        #region IAppSettingConnectivity

        /// <summary>
        ///     SignalR für DC und Gateways
        /// </summary>
        public string DcSignalHost => "https://localhost:5002/";

        /// <summary>
        ///     SA Host - REST
        /// </summary>
        public string SaApiHost => "https://localhost:5002/api/";

        public DcDataRoot DcClient { get; set; }
        public IDcClientInfoStorage DcAppStorage { get; set; }
        public IDcClientCacheStorage DcAppCache { get; set; }
        public bool DcEnabled { get; set; }

        /// <summary>
        ///     App mit User
        /// </summary>
        public bool DcUseUser { get; set; } = true;

        public bool SaEnabled { get; set; }
        public RestAccessBase SaClient { get; set; }

        #endregion IAppSettingConnectivity

        #region IAppSettingsPush

        /// <summary>
        ///     Id des Notification-Channels
        /// </summary>
        public string NotificationChannelId => "DefaultId";

        /// <summary>
        ///     Name des Notification-Channels
        /// </summary>
        public string NotificationChannelName => "PushBenachrichtigungen";

        /// <summary>
        ///     Standard Topic
        /// </summary>
        public string DefaultTopic => "DEFAULT";

        public IPlatformPush Platform { get; set; }

        #endregion IAppSettingsPush

        #region IAppSettingsLinks

        /// <summary>
        ///     App im Playstore
        /// </summary>
        public string DroidLink => "TOENTER";

        /// <summary>
        ///     App im Appstore
        /// </summary>
        public string IosLink => "TOENTER";

        /// <summary>
        ///     App im Windows Store
        /// </summary>
        public string WindowsLink => "TOENTER";

        /// <summary>
        ///     Deployte BlazorApp
        /// </summary>
        public string BlazorLink => "TOENTER";

        /// <summary>
        ///     Link zum Appcenter iOS
        /// </summary>
        public string IosTelemetryLink => "TOENTER";

        /// <summary>
        ///     Link zum Appcenter Android
        /// </summary>
        public string DroidTelemetryLink => "TOENTER";

        /// <summary>
        ///     Link zu Application Insights
        /// </summary>
        public string BlazorTelemetryLink => "TOENTER";

        /// <summary>
        ///     Link zu Portal.azure
        /// </summary>
        public string AzureResourceLink => "TOENTER";

        /// <summary>
        ///     Link zu Fotec DevOps
        /// </summary>
        public string DevOpsLink => "TOENTER";

        #endregion IAppSettingsLinks
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
}