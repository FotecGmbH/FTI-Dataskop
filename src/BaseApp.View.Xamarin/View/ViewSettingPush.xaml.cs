// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.View
{
    /// <summary>
    ///     ViewPush
    /// </summary>
    public partial class ViewSettingPush
    {
        /// <summary>
        ///     ViewPush
        /// </summary>
        public ViewSettingPush()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     ViewPush
        /// </summary>
        /// <param name="args"></param>
        public ViewSettingPush(object args = null!) : base(args)
        {
            InitializeComponent();
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await SwitchPushEnabled.TranslateTo(-5, 0, 100).ConfigureAwait(true);
            await SwitchPushEnabled.TranslateTo(15, 0, 100).ConfigureAwait(true);
            await SwitchPushEnabled.TranslateTo(0, 0, 100).ConfigureAwait(true);
        }
    }
}