// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.ComponentModel;
using System.Threading.Tasks;
using BaseApp;
using BDA.Common.Exchange.Model;
using Biss.Apps.Blazor.Pages;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Shared
{
    /// <summary>
    ///     Main Layout Komponente.
    /// </summary>
    public partial class MainLayout : BissMainLayout
    {
        #region Properties

        /// <summary>
        ///     Nav Menu für View Binding
        /// </summary>
        private NavMenu? MenuProject
        {
            get => NavMenu as NavMenu;
            set => NavMenu = value;
        }

        /// <summary>
        ///     ViewModel für View Binding
        /// </summary>
        private VmProjectBase? ViewModelProject
        {
            get => ViewModel as VmProjectBase;
            set => ViewModel = value;
        }

        #endregion

        /// <inheritdoc />
        public override Task AttachDetachEvents(bool attach)
        {
            if (ViewModel != null)
            {
                if (attach)
                {
                    ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
                    ViewModel.View.PropertyChanged += ViewOnPropertyChanged;

                    if (ViewModelProject != null)
                    {
                        ViewModelProject.Dc.DcExUser.Data.PropertyChanged += UserDataOnPropertyChanged;
                    }

                    if (ViewModel.View.CmdSaveHeader != null)
                    {
                        ViewModel.View.CmdSaveHeader.PropertyChanged += CmdSaveHeaderOnPropertyChanged;
                    }
                }
                else
                {
                    ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                    ViewModel.View.PropertyChanged -= ViewOnPropertyChanged;

                    if (ViewModelProject != null!)
                    {
                        ViewModelProject.Dc.DcExUser.Data.PropertyChanged -= UserDataOnPropertyChanged;
                    }

                    if (ViewModel.View.CmdSaveHeader != null)
                    {
                        ViewModel.View.CmdSaveHeader.PropertyChanged -= CmdSaveHeaderOnPropertyChanged;
                    }
                }
            }

            return base.AttachDetachEvents(attach);
        }

        private async void UserDataOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExUser.UserImageLink))
            {
                Logging.Log.LogTrace($"[{nameof(MainLayout)}]({nameof(UserDataOnPropertyChanged)}): Dc.User.Image");
                await InvokeAsync(StateHasChanged).ConfigureAwait(true);
            }
        }

        private async void ViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VmViewProperties.CmdSaveHeader))
            {
                if (ViewModel?.View?.CmdSaveHeader != null)
                {
                    ViewModel.View.CmdSaveHeader.PropertyChanged += CmdSaveHeaderOnPropertyChanged;
                }
            }

            Logging.Log.LogTrace($"[{nameof(MainLayout)}]({nameof(ViewOnPropertyChanged)}): VM.View - {e.PropertyName}");
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }

        private async void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsLoaded))
            {
                return;
            }

            Logging.Log.LogTrace($"[{nameof(MainLayout)}]({nameof(ViewModelOnPropertyChanged)}): VM.PC - {e.PropertyName}");
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }

        /// <summary>
        ///     Property Changed des Header Commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CmdSaveHeaderOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Logging.Log.LogTrace($"[{nameof(MainLayout)}]({nameof(CmdSaveHeaderOnPropertyChanged)}): SaveHeader - {e.PropertyName}");
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }
    }
}