// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Threading.Tasks;
using BaseApp;
using Biss.Apps.Blazor.Pages;
using Biss.Apps.ViewModel;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorApp.Shared
{
    /// <summary>
    ///     Nav Menu.
    /// </summary>
    public partial class NavMenu : BissNavMenu
    {
        /// <summary>
        ///     Method invoked after each time the component has been rendered. Note that the component does
        ///     not automatically re-render after the completion of any returned <see cref="T:System.Threading.Tasks.Task" />,
        ///     because
        ///     that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        ///     Set to <c>true</c> if this is the first time
        ///     <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        ///     on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        ///     The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and
        ///     <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        ///     are useful for performing interop, or interacting with values received from <c>@ref</c>.
        ///     Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        ///     once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.CollectionChanged += async (_, _) => { await InvokeAsync(StateHasChanged).ConfigureAwait(true); };
                VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItemChanged += async (_, _) => { await InvokeAsync(StateHasChanged).ConfigureAwait(true); };
            }

            await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
        }

        /// <summary>
        ///     Navigation
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="args">args</param>
        private async Task Navigate(VmCommandSelectable cmd, MouseEventArgs args)
        {
            VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem = cmd;
            await JsRuntime.InvokeVoidAsync("collapseNavbar").ConfigureAwait(true);
        }

        /// <summary>
        ///     Menüeintrag ist aktiv oder inaktiv
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private string IsActive(VmCommandSelectable cmd)
        {
            if (VmProjectBase.GetVmBaseStatic == null ! || VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem == null)
            {
                return "";
            }

            if (cmd == VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem)
            {
                return "active";
            }

            return "";
        }
    }
}