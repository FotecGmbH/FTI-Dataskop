// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using BDA.Common.Exchange.Enum;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Shared
{
    public partial class NewValueNotificationRuleCompareBoolComponent
    {
        public NewValueNotificationRuleCompareBoolComponent()
        {
            var compareOperators = new List<EnumCompareOperator>
            {
                EnumCompareOperator.IsEqual
            };

            VmPickerCompareBoolOperator.AddKeys(compareOperators);

            VmPickerCompareBoolValue.AddKey(true, "true");
            VmPickerCompareBoolValue.AddKey(false, "false");
        }

        #region Properties

        /// <summary>
        ///     Die Regel
        /// </summary>
        [Parameter]
        public ExNewNotificationRuleCompareBool NewValueNotificationRule { get; set; }

        /// <summary>
        ///     Picker für Compare mit Wert 1
        /// </summary>
        public VmPicker<EnumCompareOperator> VmPickerCompareBoolOperator { get; set; } = new VmPicker<EnumCompareOperator>(nameof(VmPickerCompareBoolOperator));

        /// <summary>
        ///     Picker für Compare mit Wert 1
        /// </summary>
        public VmPicker<bool> VmPickerCompareBoolValue { get; set; } = new VmPicker<bool>(nameof(VmPickerCompareBoolValue));

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
            VmPickerCompareBoolOperator.SelectKey(EnumCompareOperator.IsEqual);
            VmPickerCompareBoolValue.SelectKey(NewValueNotificationRule.ComparedValue);

            VmPickerCompareBoolValue.SelectedItemChanged += VmPickerCompareBoolValue_SelectedItemChanged;

            return base.OnInitializedAsync();
        }

        private void VmPickerCompareBoolValue_SelectedItemChanged(object? sender, SelectedItemEventArgs<VmPickerElement<bool>> e)
        {
            NewValueNotificationRule.ComparedValue = e.CurrentItem.Key;
        }
    }
}