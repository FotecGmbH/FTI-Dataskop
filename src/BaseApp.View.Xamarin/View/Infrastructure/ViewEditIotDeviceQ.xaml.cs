// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Xamarin.Forms;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewEditIotDeviceQ
    {
        public ViewEditIotDeviceQ() : this(null)
        {
        }

        public ViewEditIotDeviceQ(object? args = null) : base(args)
        {
            InitializeComponent();
        }

        private void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.UpdateAdditionalConfig();
        }

        private void Editor_OnCompleted(object sender, EventArgs e)
        {
            ViewModel.UpdateAdditionalConfig();
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member