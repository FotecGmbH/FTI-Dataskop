// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Threading.Tasks;
using BaseApp;
using Biss.Apps;
using Biss.Apps.Blazor.Pages;
using Biss.Apps.Connectivity.Blazor;
using Biss.Log.Producer;
using Exchange;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TG.Blazor.IndexedDB;

namespace BlazorApp.BissApps
{
    /// <summary>
    ///     Basis für RazorPages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProjectRazorPage<T> : BissRazorPage<T> where T : VmProjectBase
    {
        #region Properties

        /// <summary>
        ///     Detail-View View Model.
        /// </summary>
        public VmBase? DetailViewViewModel { get; set; } = null!;

        #endregion

        #region Injects

        /// <summary>
        ///     DC Cache - wenn ohne Indexed DB entfern!
        /// </summary>
        [Inject]
        protected IndexedDBManager? IndexedDb { get; set; }

        /// <summary>
        ///     JSInProcessRuntime für Blazor
        /// </summary>
        [Inject]
        protected IJSInProcessRuntime? JsInProcessRuntime { get; set; }

        #endregion

        #region BissLifecycle

        /// <inheritdoc />
        protected override async Task OnAppStart()
        {
            Logging.Log.LogInfo($"[ProjectRazorPage-({GetType()}]({nameof(OnAppStart)}): Start");

            await this.BissUseConnectivity(AppSettings.Current(), JsRuntime, JsInProcessRuntime, IndexedDb).ConfigureAwait(true);

            await base.OnAppStart().ConfigureAwait(true);

            Logging.Log.LogInfo($"[ProjectRazorPage-({GetType()}]({nameof(OnAppStart)}): Finish");
        }

        /// <inheritdoc />
        protected override Task OnViewCreate()
        {
            Logging.Log.LogTrace($"[ProjectRazorPage-({GetType()}]({nameof(OnViewCreate)}): ");
            return base.OnViewCreate();
        }

        /// <inheritdoc />
        protected override Task OnViewAppearing()
        {
            Logging.Log.LogTrace($"[ProjectRazorPage-({GetType()}]({nameof(OnViewAppearing)}): ");
            return base.OnViewAppearing();
        }

        /// <inheritdoc />
        protected override Task OnViewActivated()
        {
            Logging.Log.LogTrace($"[ProjectRazorPage-({GetType()}]({nameof(OnViewActivated)}): ");
            return base.OnViewActivated();
        }

        /// <inheritdoc />
        protected override Task OnViewLoaded()
        {
            Logging.Log.LogTrace($"[ProjectRazorPage-({GetType()}]({nameof(OnViewLoaded)}): ");
            return base.OnViewLoaded();
        }

        #endregion
    }
}