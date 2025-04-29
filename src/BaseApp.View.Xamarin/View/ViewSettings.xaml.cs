// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using BaseApp.ViewModel;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewSettings
    {
        public ViewSettings() : this(null)
        {
            ViewModel.SwitchTheme += ViewModelOnSwitchTheme;
        }

        public ViewSettings(object? args = null) : base(args)
        {
            InitializeComponent();
            ViewModel.SwitchTheme += ViewModelOnSwitchTheme;
        }

        private void ViewModelOnSwitchTheme(object sender, EventArg<bool> e)
        {
            App.SwitchTheme(e.Data);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member