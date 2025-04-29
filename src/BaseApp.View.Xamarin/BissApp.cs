// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.Interfaces;
using Biss.Log.Producer;

namespace BaseApp.View.Xamarin
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse BissApp. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissApp : App
    {
        private CancellationTokenSource _sleepToken = new CancellationTokenSource();


        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="platform"></param>
        /// <exception cref="Exception"></exception>
        public BissApp(IBissAppPlattform platform) : base(platform)
        {
        }

        #region OnStart/Sleep/Resume

        /// <inheritdoc />
        protected override void OnStart()
        {
            // Handle when your app starts
            base.OnStart();
            //VmProjectBase.SubNotifications();
            Logging.Log.LogInfo("XamarinForms: OnStart");
        }

        /// <inheritdoc />
        protected override void OnSleep()
        {
            // Handle when your app sleeps
            base.OnSleep();
            Logging.Log.LogInfo("XamarinForms: OnSleep");

            //DC nach 30s trennen
            _sleepToken = new CancellationTokenSource();
            if (VmProjectBase.DcDoNotAutoDisconnect == false)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(30000, _sleepToken.Token).ConfigureAwait(true);
                    if (!_sleepToken.IsCancellationRequested)
                    {
                        Logging.Log.LogInfo("APP.xaml.cs: Auto Close DC Conntection");
                        var dc = BcDataConnectorExtensions.BcDataConnector(null!)?.GetDc<DcProjectBase>();
                        dc?.CloseConnection(true);
                    }
                });
            }

            //VmProjectBase.UnsubNotifications();
        }

        /// <inheritdoc />
        protected override async void OnResume()
        {
            // Handle when your app resumes
            base.OnResume();
            Logging.Log.LogInfo("XamarinForms: OnResume");
            _sleepToken.Cancel();

            // DC bei Bedarf wieder starten
            var dc = BcDataConnectorExtensions.BcDataConnector(null!)?.GetDc<DcProjectBase>();
            if (dc != null)
            {
                if (dc.AutoConnect == false)
                {
                    await dc.OpenConnection(true).ConfigureAwait(true);
                }
            }

            //VmProjectBase.SubNotifications();
        }

        #endregion
    }
}