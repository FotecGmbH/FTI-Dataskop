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
using Exchange.Resources;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Shared
{
    public partial class NewValueNotificationRuleCompareNumberComponent
    {
        public NewValueNotificationRuleCompareNumberComponent()
        {
            var compareOperators = new List<EnumCompareOperator>
            {
                EnumCompareOperator.IsEqual,
                EnumCompareOperator.IsNotEqual,
                EnumCompareOperator.IsGreater,
                EnumCompareOperator.IsGreaterOrEqual,
                EnumCompareOperator.IsLess,
                EnumCompareOperator.IsLessOrEqual
            };

            VmPickerCompareNumberOperatorValue1.AddKeys(compareOperators);
            VmPickerCompareNumberOperatorValue2.AddKeys(compareOperators);
        }

        #region Properties

        /// <summary>
        ///     Die Regel
        /// </summary>
        [Parameter]
        [EditorRequired]
        public ExNewNotificationRuleCompareNumbers NewValueNotificationRule { get; set; }

        /// <summary>
        ///     Picker für Compare mit Wert 1
        /// </summary>
        public VmPicker<EnumCompareOperator> VmPickerCompareNumberOperatorValue1 { get; set; } = new VmPicker<EnumCompareOperator>(nameof(VmPickerCompareNumberOperatorValue1));

        /// <summary>
        ///     Picker für Compare mit Wert 2
        /// </summary>
        public VmPicker<EnumCompareOperator> VmPickerCompareNumberOperatorValue2 { get; set; } = new VmPicker<EnumCompareOperator>(nameof(VmPickerCompareNumberOperatorValue2));

        /// <summary>
        ///     Zweiten Vergleichswert hinzufügen
        /// </summary>
        public VmCommand CmdAddComparedValue { get; set; } = null!;

        /// <summary>
        ///     Zweiten Vergleichswert entfernen
        /// </summary>
        public VmCommand CmdDeleteComparedValue { get; set; } = null!;

        #endregion

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);

            CmdAddComparedValue = new VmCommand("Vergleichswert hinzufügen", () =>
            {
                NewValueNotificationRule.ComparedValue2 ??= new ExCompareDoubleValue();
                StateHasChanged();
            }, glyph: Glyphs.Add);

            CmdDeleteComparedValue = new VmCommand("Vergleichswert entfernen", () =>
            {
                NewValueNotificationRule.ComparedValue2 = null;
                StateHasChanged();
            }, glyph: Glyphs.Bin);

            VmPickerCompareNumberOperatorValue1.SelectKey(NewValueNotificationRule.ComparedValue1.CompareOperator);

            if (NewValueNotificationRule.ComparedValue2 != null)
            {
                VmPickerCompareNumberOperatorValue2.SelectKey(NewValueNotificationRule.ComparedValue2.CompareOperator);
            }

            VmPickerCompareNumberOperatorValue1.SelectedItemChanged += VmPickerCompareNumberOperatorValue1_SelectedItemChanged;
            VmPickerCompareNumberOperatorValue2.SelectedItemChanged += VmPickerCompareNumberOperatorValue2_SelectedItemChanged;
        }

        private void VmPickerCompareNumberOperatorValue2_SelectedItemChanged(object? sender, SelectedItemEventArgs<VmPickerElement<EnumCompareOperator>> e)
        {
            if (NewValueNotificationRule.ComparedValue2 != null)
            {
                NewValueNotificationRule.ComparedValue2.CompareOperator = e.CurrentItem.Key;
            }
        }

        private void VmPickerCompareNumberOperatorValue1_SelectedItemChanged(object? sender, SelectedItemEventArgs<VmPickerElement<EnumCompareOperator>> e)
        {
            NewValueNotificationRule.ComparedValue1.CompareOperator = e.CurrentItem.Key;
        }
    }
}