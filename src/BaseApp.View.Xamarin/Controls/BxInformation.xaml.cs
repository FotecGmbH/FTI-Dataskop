// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     Control für allgemeine Infos eines Iot Device und Gateways
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BxInformation : ContentView
    {
        /// <summary>
        ///     ExInformationProperty Bindable Property
        /// </summary>
        public static BindableProperty ExInformationProperty = BindableProperty.Create(
            nameof(ExInformation),
            typeof(ExInformation),
            typeof(BxInformation),
            new ExInformation()
        );

        /// <summary>
        ///     ExInformationProperty Bindable Property
        /// </summary>
        public static BindableProperty DbIdProperty = BindableProperty.Create(
            nameof(DbId),
            typeof(long),
            typeof(BxInformation),
            new long()
        );


        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public BxInformation()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        ///     ExInformation
        /// </summary>
        public ExInformation ExInformation
        {
            get => (ExInformation) GetValue(ExInformationProperty);
            set => SetValue(ExInformationProperty, value);
        }

        /// <summary>
        ///     Db Id formatiert
        /// </summary>
        public long DbId
        {
            get => (long) GetValue(DbIdProperty);
            set => SetValue(DbIdProperty, value);
        }

        #endregion
    }
}