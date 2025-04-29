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
    ///     Control für ExInformation
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InformationView : ContentView
    {
        /// <summary>
        ///     Information Bindable Property
        /// </summary>
        public static BindableProperty ExInformationProperty = BindableProperty.Create(nameof(ExInformation),
            typeof(ExInformation),
            typeof(InformationView),
            new ExInformation());

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public static BindableProperty SelectedProperty = BindableProperty
            .Create(
                nameof(Selected),
                typeof(bool),
                typeof(InformationView),
                false,
                propertyChanged: SelectedPropertyChanged);


        /// <summary>
        ///     Ausgewählte farbe
        /// </summary>
        public static BindableProperty SelectedColorProperty = BindableProperty
            .Create(
                nameof(SelectedColor),
                typeof(Color),
                typeof(InformationView),
                Color.Red);

        /// <summary>
        ///     Ausgewählte farbe
        /// </summary>
        public static BindableProperty NotSelectedColorProperty = BindableProperty
            .Create(
                nameof(NotSelectedColor),
                typeof(Color),
                typeof(InformationView),
                Color.Green);

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public InformationView()
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
        ///     Farbe des Rahmens.
        /// </summary>
        public bool Selected
        {
            get => (bool) GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }

        /// <summary>
        ///     Farbe wenn ausgewählt.
        /// </summary>
        public Color SelectedColor
        {
            get => (Color) GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        /// <summary>
        ///     Farbe wenn ausgewählt.
        /// </summary>
        public Color NotSelectedColor
        {
            get => (Color) GetValue(NotSelectedColorProperty);
            set => SetValue(NotSelectedColorProperty, value);
        }

        #endregion

        private static void SelectedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is InformationView infoView && newvalue is bool selected)
            {
                if (selected)
                {
                    infoView.ContentFrame.BackgroundColor = infoView.SelectedColor;
                }
                else
                {
                    infoView.ContentFrame.BackgroundColor = infoView.NotSelectedColor;
                }
            }
        }
    }
}