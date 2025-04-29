// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     View für Global Config TTN
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GcTtnView : ContentView
    {
        /// <summary>
        /// </summary>
        public static BindableProperty GlobalConfigProperty = BindableProperty
            .Create(nameof(GlobalConfig),
                typeof(GcTtn),
                typeof(GcTtnView));


        /// <summary>
        ///     Konstruktor
        /// </summary>
        public GcTtnView()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        ///     Globale Konfig fürs Binding
        /// </summary>
        public GcTtn GlobalConfig
        {
            get => (GcTtn) GetValue(GlobalConfigProperty);
            set => SetValue(GlobalConfigProperty, value);
        }

        #endregion
    }
}