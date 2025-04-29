// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using BDA.Common.Exchange.Enum;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Shared
{
    public partial class CompareStringComponent
    {
        public CompareStringComponent()
        {
            var compareOperators = new List<EnumCompareOperator>
            {
                EnumCompareOperator.IsEqual,
                EnumCompareOperator.IsNotEqual,
                EnumCompareOperator.Contains,
                EnumCompareOperator.NotContains
            };

            VmPickerCompareStringOperator.AddKeys(compareOperators);
        }

        #region Properties

        /// <summary>
        ///     Vergleichswert
        /// </summary>
        [Parameter]
        [EditorRequired]
        public ExCompareStringValue CompareStringValue { get; set; }

        /// <summary>
        ///     Picker für Compare
        /// </summary>
        public VmPicker<EnumCompareOperator> VmPickerCompareStringOperator { get; set; } = new(nameof(VmPickerCompareStringOperator));

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
            VmPickerCompareStringOperator.SelectKey(CompareStringValue.CompareOperator);
            VmPickerCompareStringOperator.SelectedItemChanged += VmPickerCompareStringOperator_SelectedItemChanged;

            return base.OnInitializedAsync();
        }

        private void VmPickerCompareStringOperator_SelectedItemChanged(object? sender, SelectedItemEventArgs<VmPickerElement<EnumCompareOperator>> e)
        {
            CompareStringValue.CompareOperator = e.CurrentItem.Key;
        }
    }
}