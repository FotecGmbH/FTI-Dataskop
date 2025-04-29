// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using BaseApp.View.Xamarin;
using Biss.Apps.iOs;
using Biss.Apps.Push.iOS;
using Foundation;
using UIKit;

namespace IOsApp
{
    /// <summary>
    ///     <para>
    ///         The UIApplicationDelegate for the application. This class is responsible for launching the User Interface of
    ///         the application, as well as listening (and optionally responding) to application events from iOS.
    ///     </para>
    ///     Klasse AppDelegate. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : BissAppsFormsApplicationDelegate
    {
        /// <summary>
        ///     The UIApplicationDelegate for the application.
        /// </summary>
        public AppDelegate()
        {
            Param = new object[]
            {
                AppSettings.Current().ProjectWorkUserFolder,
                AppSettings.Current(),
            };
        }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            BissAppsFinishedLaunching(uiApplication, launchOptions, 0);
            this.BissUsePush(AppSettings.Current(), launchOptions);
            LoadApplication(new BissApp(Initializer));
            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}