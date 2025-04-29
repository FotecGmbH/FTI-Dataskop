// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using Biss.Apps.ViewModel;
using Exchange.Resources;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Shared
{
    public partial class NewValueNotificationRuleCompareStringComponent
    {
        #region Properties

        /// <summary>
        ///     Die Regel
        /// </summary>
        [Parameter]
        public ExNewNotificationRuleCompareString NewValueNotificationRule { get; set; }

        /// <summary>
        ///     Adden
        /// </summary>
        public VmCommand CmdAddCompareValue { get; set; } = null!;

        #endregion

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            CmdAddCompareValue = new VmCommand("Hinzufügen", () =>
            {
                NewValueNotificationRule.ComparedValues.Add(new ExCompareStringValue());
                StateHasChanged();
            }, glyph: Glyphs.Add);

            return base.OnInitializedAsync();
        }
    }
}