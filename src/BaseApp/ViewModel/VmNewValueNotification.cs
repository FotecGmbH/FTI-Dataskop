// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using BDA.Common.Exchange.Enum;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Exchange.Resources;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse VmNewValueNotification. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewNewValueNotification")]
    public class VmNewValueNotification : VmEditDcListPoint<ExNewValueNotification>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmNewValueNotification.DesignInstance}"
        /// </summary>
        public static VmNewValueNotification DesignInstance = new VmNewValueNotification();

        /// <summary>
        ///     VmNewValueNotification
        /// </summary>
        public VmNewValueNotification() : base(ResViewNewValueNotification.Title)
        {
            SetViewProperties(true);
            PickerNewValueNotificationType.AddKey(EnumNewValueNotificationType.Webhook, "Webhook");
            PickerNewValueNotificationType.AddKey(EnumNewValueNotificationType.Mqtt, "MQTT");
            PickerNewValueNotificationType.AddKey(EnumNewValueNotificationType.Email, "Email");
        }

        #region Properties

        /// <summary>
        ///     Benachrichtigung
        /// </summary>
        public INewValueNotification? NewValueNotification { get; set; }

        /// <summary>
        ///     PickerPositionType
        /// </summary>
        public VmPicker<EnumNewValueNotificationType> PickerNewValueNotificationType { get; private set; } = new VmPicker<EnumNewValueNotificationType>(nameof(PickerNewValueNotificationType));

        /// <summary>
        ///     Entry WebHook Url
        /// </summary>
        public VmEntry? EntryWebHookUrl { get; set; }

        /// <summary>
        ///     Entry Email Address
        /// </summary>
        public VmEntry? EntryEmailAddress { get; set; }

        /// <summary>
        ///     Datentyp der Messwertdefinition
        /// </summary>
        public EnumValueTypes ValueTypeOfMeasurementDefinition { get; set; }

        #endregion

        private void Init()
        {
            switch (Data.NewValueNotificationType)
            {
                case EnumNewValueNotificationType.Webhook:
                    InitWebHookEntries();
                    break;
                case EnumNewValueNotificationType.Email:
                    InitEmailEntries();
                    break;
            }
        }

        private void InitWebHookEntries()
        {
            EntryWebHookUrl = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "WebHook-Url",
                "Die Url die bei neuem Wert aufgerufen werden soll mittels Post",
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                (ExNewValueNotificationWebHook) NewValueNotification,
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                nameof(ExNewValueNotificationWebHook.WebHookUrl),
                ValidateFuncWebHookEntry,
                showTitle: true
            );

            EntryWebHookUrl.PropertyChanged += EntryWebHookUrl_PropertyChanged;
        }

        private (string hint, bool valid) ValidateFuncWebHookEntry(string s)
        {
            return NewValueNotification?.NewValueNotificationType != EnumNewValueNotificationType.Webhook ? (string.Empty, true) : VmEntryValidators.ValidateFuncStringEmpty(s);
        }

        private (string hint, bool valid) ValidateFuncEmailEntry(string s)
        {
            return NewValueNotification?.NewValueNotificationType != EnumNewValueNotificationType.Email ? (string.Empty, true) : VmEntryValidators.ValidateFuncStringEmpty(s);
        }

        private void InitEmailEntries()
        {
            EntryEmailAddress = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Email Adresse",
                "Email Adresse",
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                (ExNewValueNotificationEmail) NewValueNotification,
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                nameof(ExNewValueNotificationEmail.EmailAddress),
                ValidateFuncEmailEntry,
                showTitle: true
            );

            EntryEmailAddress.PropertyChanged += EntryEmailAddress_PropertyChanged;
        }

        private bool CanSave()
        {
            return (EntryWebHookUrl?.ValidateData() ?? true) && (EntryEmailAddress?.ValidateData() ?? true);
        }

        #region Overrides

        // ReSharper disable once RedundantOverriddenMember
        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await base.OnActivated(args).ConfigureAwait(true);

            var js = new JsonTypeSafeConverter();
            if (!string.IsNullOrWhiteSpace(Data.AdditionalConfiguration))
            {
                try
                {
                    NewValueNotification = Data.NewValueNotificationType switch
                    {
                        EnumNewValueNotificationType.Webhook => js.Deserialize<ExNewValueNotificationWebHook>(Data.AdditionalConfiguration),
                        EnumNewValueNotificationType.Mqtt => js.Deserialize<ExNewValueNotificationMqtt>(Data.AdditionalConfiguration),
                        EnumNewValueNotificationType.Email => js.Deserialize<ExNewValueNotificationEmail>(Data.AdditionalConfiguration),
                        _ => NewValueNotification
                    };
                }
                catch (Exception)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            NewValueNotification ??= new ExNewValueNotificationWebHook();

            var measurementDefinition = Dc.DcExMeasurementDefinition.FirstOrDefault(x => x.Id == Data.MeasurementDefinitionId);
            ValueTypeOfMeasurementDefinition = measurementDefinition.Data.ValueType;

            Init();
            PickerNewValueNotificationType.SelectKey(Data.NewValueNotificationType);
            PickerNewValueNotificationType.SelectedItemChanged += PickerNewValueNotificationType_SelectedItemChanged;
        }

        // ReSharper disable once RedundantOverriddenMember
        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            return base.OnLoaded();
        }

        // ReSharper disable once RedundantOverriddenMember
        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            return base.OnDisappearing(view);
        }

        #endregion

        #region EventHandler

        private void PickerNewValueNotificationType_SelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumNewValueNotificationType>> e)
        {
            var previousRules = NewValueNotification?.Rules;

            switch (e.CurrentItem.Key)
            {
                case EnumNewValueNotificationType.Webhook:
                {
                    if (NewValueNotification?.NewValueNotificationType != EnumNewValueNotificationType.Webhook)
                    {
                        NewValueNotification = new ExNewValueNotificationWebHook();
                        InitWebHookEntries();
                    }

                    break;
                }
                case EnumNewValueNotificationType.Mqtt:
                {
                    if (NewValueNotification?.NewValueNotificationType != EnumNewValueNotificationType.Mqtt)
                    {
                        NewValueNotification = new ExNewValueNotificationMqtt();
                    }

                    break;
                }
                case EnumNewValueNotificationType.Email:
                {
                    if (NewValueNotification?.NewValueNotificationType != EnumNewValueNotificationType.Email)
                    {
                        NewValueNotification = new ExNewValueNotificationEmail();
                        InitEmailEntries();
                    }

                    break;
                }
            }

            if (previousRules?.Any() == true && NewValueNotification != null)
            {
                NewValueNotification.Rules = previousRules;
            }
        }

        private void EntryWebHookUrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CmdSave.CanExecute();
        }

        private void EntryEmailAddress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CmdSave.CanExecute();
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdAddRule = new VmCommand("Regel hinzufügen", async () =>
            {
                INewValueNotificationRule newRule;
                switch (ValueTypeOfMeasurementDefinition)
                {
                    case EnumValueTypes.Number:
                        newRule = new ExNewNotificationRuleCompareNumbers();
                        break;
                    case EnumValueTypes.Bit:
                        newRule = new ExNewNotificationRuleCompareBool();
                        break;
                    case EnumValueTypes.Text:
                        newRule = new ExNewNotificationRuleCompareString
                        {
                            ComparedValues = new List<ExCompareStringValue>
                            {
                                new ExCompareStringValue()
                            }
                        };
                        break;
                    default:
                        await MsgBox.Show("Momentan nicht möglich für diesen Datentyp der Messwert Definition").ConfigureAwait(true);
                        return;
                }

                NewValueNotification?.Rules.Add(newRule);
                await View.RefreshAsync().ConfigureAwait(true);
            }, glyph: Dc.DcExNewValueNotifications.CmdAddItem.Glyph);

            CmdDeleteRule = new VmCommand("Regel entfernen", async arg =>
            {
                if (arg is INewValueNotificationRule rule)
                {
                    NewValueNotification?.Rules.Remove(rule);
                    await View.RefreshAsync().ConfigureAwait(true);
                }
            }, glyph: Dc.DcExNewValueNotifications.CmdRemoveItem.Glyph);

            CmdSave = new VmCommand(View.CmdSaveHeader?.DisplayName ?? "Speichern", async () =>
            {
                if (NewValueNotification != null)
                {
                    var js = new JsonTypeSafeConverter();
                    Data.NewValueNotificationType = NewValueNotification.NewValueNotificationType;
                    Data.AdditionalConfiguration = js.Serialize(NewValueNotification);
                }

                if (!Dc.DcExNewValueNotifications.Contains(DcListDataPoint))
                {
                    Dc.DcExNewValueNotifications.Add(DcListDataPoint);
                }

                await ViewCmdSaveHeaderAction().ConfigureAwait(true);
            }, glyph: View.CmdSaveHeader?.Glyph ?? Glyphs.Floppy_disk, canExecuteNoParams: CanSave);
        }

        /// <summary>
        ///     Regel hinzufügen
        /// </summary>
        public VmCommand CmdAddRule { get; set; } = null!;

        /// <summary>
        ///     Regel entfernen
        /// </summary>
        public VmCommand CmdDeleteRule { get; set; } = null!;

        /// <summary>
        ///     Speichern
        /// </summary>
        public VmCommand CmdSave { get; set; } = null!;

        #endregion
    }
}