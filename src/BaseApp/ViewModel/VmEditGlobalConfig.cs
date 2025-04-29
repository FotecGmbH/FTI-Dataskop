// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Interfaces;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Serialize;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Globale Konfiguration bearbeiten</para>
    ///     Klasse VmEditGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditGlobalConfig", true)]
    public class VmEditGlobalConfig : VmEditDcListPoint<ExGlobalConfig>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditGlobalConfig.DesignInstance}"
        /// </summary>
        public static VmEditGlobalConfig DesignInstance = new VmEditGlobalConfig();

        /// <summary>
        ///     VmEditGlobalConfig
        /// </summary>
        public VmEditGlobalConfig() : base("Globale Konfiguration", subTitle: "Globale Einstellungen für die Firma")
        {
            SetViewProperties(true);

            PickerConfigType.AddKey(EnumGlobalConfigTypes.Ttn, "TTN");
            PickerConfigType.AddKey(EnumGlobalConfigTypes.Drei, "Drei");
            PickerConfigType.AddKey(EnumGlobalConfigTypes.Microtronics, "Microtronics");
        }

        #region Properties

        /// <summary>
        ///     EntryName
        /// </summary>
        public VmEntry? EntryName { get; private set; }

        /// <summary>
        ///     EntryTtnZone
        /// </summary>
        public VmEntry? EntryTtnZone { get; set; }

        /// <summary>
        ///     EntryTtnAppId
        /// </summary>
        public VmEntry? EntryTtnAppId { get; set; }

        /// <summary>
        ///     EntryTtnUserid
        /// </summary>
        public VmEntry? EntryTtnUserId { get; set; }

        /// <summary>
        ///     EntryTtnApiKey
        /// </summary>
        public VmEntry? EntryTtnApiKey { get; set; }

        /// <summary>
        ///     EntryDreiLoginName
        /// </summary>
        public VmEntry? EntryDreiLoginName { get; set; }

        /// <summary>
        ///     EntryDreiPassword
        /// </summary>
        public VmEntry? EntryDreiPassword { get; set; }

        /// <summary>
        ///     EntryMicrotronicsUserName
        /// </summary>
        public VmEntry? EntryMicrotronicsUserName { get; set; }

        /// <summary>
        ///     EntryMicrotronicsPassword
        /// </summary>
        public VmEntry? EntryMicrotronicsPassword { get; set; }

        /// <summary>
        ///     EntryMicrotronicsBackendDomain
        /// </summary>
        public VmEntry? EntryMicrotronicsBackendDomain { get; set; }

        /// <summary>
        ///     PickerPositionType
        /// </summary>
        public VmPicker<EnumGlobalConfigTypes> PickerConfigType { get; private set; } = new VmPicker<EnumGlobalConfigTypes>(nameof(PickerConfigType));

        /// <summary>
        ///     GlobalConfig
        /// </summary>
        public IGlobalConfig GlobalConfig { get; set; } = null!;

        /// <summary>
        ///     EntryDescription
        /// </summary>
        public VmEntry EntryDescription { get; set; } = null!;

        /// <summary>
        ///     If ttn config should be visible
        /// </summary>
        public bool TtnVisible { get; set; } = true;

        /// <summary>
        ///     if drei config should be visible
        /// </summary>
        public bool DreiVisible { get; set; }

        /// <summary>
        ///     if microtronics config should be visible
        /// </summary>
        public bool MicrotronicsVisible { get; set; }

        #endregion


        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var r = base.OnActivated(args);
            EntryName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Name:",
                "Name der Konfiguration",
                Data.Information,
                nameof(ExGlobalConfig.Information.Name),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDescription = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Beschreibung:",
                "Beschreibung der Konfiguration",
                Data.Information,
                nameof(ExGlobalConfig.Information.Description),
                showTitle: false
            );

            PickerConfigType.SelectedItem = PickerConfigType.First(f => f.Key == Data.ConfigType);

            InitEntriesAndEvents();

            return r;
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            AttachDetachVmEvents(true);
            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachDetachVmEvents(false);
            return base.OnDisappearing(view);
        }

        private void PickerConfigType_SelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumGlobalConfigTypes>> e)
        {
            Data.ConfigType = e.CurrentItem.Key;
            if (EntryName != null)
            {
                EntryName.PropertyChanged -= EntryNameOnPropertyChanged;
            }

            if (EntryTtnZone != null)
            {
                EntryTtnZone.PropertyChanged -= EntryTtnOnPropertyChanged;
            }

            if (EntryTtnApiKey != null)
            {
                EntryTtnApiKey.PropertyChanged -= EntryTtnOnPropertyChanged;
            }

            if (EntryTtnAppId != null)
            {
                EntryTtnAppId.PropertyChanged -= EntryTtnOnPropertyChanged;
            }

            if (EntryTtnUserId != null)
            {
                EntryTtnUserId.PropertyChanged -= EntryTtnOnPropertyChanged;
            }

            if (EntryDreiLoginName != null)
            {
                EntryDreiLoginName.PropertyChanged -= EntryDreiOnPropertyChanged;
            }

            if (EntryDreiPassword != null)
            {
                EntryDreiPassword.PropertyChanged -= EntryDreiOnPropertyChanged;
            }

            if (EntryMicrotronicsUserName != null)
            {
                EntryMicrotronicsUserName.PropertyChanged -= EntryMicrotronicsOnPropertyChanged;
            }

            if (EntryMicrotronicsPassword != null)
            {
                EntryMicrotronicsPassword.PropertyChanged -= EntryMicrotronicsOnPropertyChanged;
            }

            if (EntryMicrotronicsBackendDomain != null)
            {
                EntryMicrotronicsBackendDomain.PropertyChanged -= EntryMicrotronicsOnPropertyChanged;
            }

            InitEntriesAndEvents();
        }

        /// <summary>
        ///     Init entries and events
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void InitEntriesAndEvents()
        {
            TtnVisible = false;
            DreiVisible = false;
            MicrotronicsVisible = false;

            switch (Data.ConfigType)
            {
                case EnumGlobalConfigTypes.Ttn:
                    TtnVisible = true;
                    InitTtnEntriesAndEvents();
                    break;
                case EnumGlobalConfigTypes.Drei:
                    DreiVisible = true;
                    InitDreiEntriesAndEvents();
                    break;
                case EnumGlobalConfigTypes.Microtronics:
                    MicrotronicsVisible = true;
                    InitMicrotronicsEntriesAndEvents();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attach"></param>
        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                PickerConfigType.SelectedItemChanged += PickerConfigType_SelectedItemChanged;
            }
            else
            {
                PickerConfigType.SelectedItemChanged -= PickerConfigType_SelectedItemChanged;
            }
        }

        /// <summary>
        ///     Initializes entries and events for drei
        /// </summary>
        private void InitDreiEntriesAndEvents()
        {
            if (string.IsNullOrEmpty(Data.AdditionalConfiguration))
            {
                GlobalConfig = new GcDrei();
            }
            else
            {
                GlobalConfig = BissDeserialize.FromJson<GcDrei>(Data.AdditionalConfiguration);
            }

            EntryDreiLoginName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Drei Loginname:",
                "Der Login Name",
                (GcDrei) GlobalConfig,
                nameof(GcDrei.LoginName),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDreiPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Drei Passwort:",
                "Das Passwort des Users",
                (GcDrei) GlobalConfig,
                nameof(GcDrei.Password),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDreiLoginName.PropertyChanged += EntryDreiOnPropertyChanged;
            EntryDreiPassword.PropertyChanged += EntryDreiOnPropertyChanged;

            SetValidateFunctionsOfNotUsedEntries(EnumGlobalConfigTypes.Drei);
        }


        /// <summary>
        ///     Initializes entries and events for ttn
        /// </summary>
        private void InitTtnEntriesAndEvents()
        {
            if (string.IsNullOrEmpty(Data.AdditionalConfiguration))
            {
                GlobalConfig = new GcTtn();
            }
            else
            {
                GlobalConfig = BissDeserialize.FromJson<GcTtn>(Data.AdditionalConfiguration);
            }

            EntryTtnZone = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn Zone:",
                "Der TTN-Server. Zb. eu1.cloud.thethings.network",
                (GcTtn) GlobalConfig, //_ttnConfig,
                nameof(GcTtn.Zone),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryTtnApiKey = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn ApiKey:",
                "Der globale Api-Key des Users",
                (GcTtn) GlobalConfig, //_ttnConfig,
                nameof(GcTtn.ApiKey),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryTtnAppId = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn ApplicationID:",
                "Die ApplikationID der erzeugten Applikation",
                (GcTtn) GlobalConfig, //_ttnConfig,
                nameof(GcTtn.Applicationid),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryTtnUserId = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn Userid:",
                "Die Userid die bei der Registrierung angegeben wurde",
                (GcTtn) GlobalConfig, //_ttnConfig,
                nameof(GcTtn.Userid),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );


            EntryName!.PropertyChanged += EntryNameOnPropertyChanged;
            EntryTtnZone.PropertyChanged += EntryTtnOnPropertyChanged;
            EntryTtnApiKey.PropertyChanged += EntryTtnOnPropertyChanged;
            EntryTtnAppId.PropertyChanged += EntryTtnOnPropertyChanged;
            EntryTtnUserId.PropertyChanged += EntryTtnOnPropertyChanged;

            SetValidateFunctionsOfNotUsedEntries(EnumGlobalConfigTypes.Ttn);
        }

        private void InitMicrotronicsEntriesAndEvents()
        {
            if (string.IsNullOrEmpty(Data.AdditionalConfiguration))
            {
                GlobalConfig = new GcMicrotronics();
            }
            else
            {
                GlobalConfig = BissDeserialize.FromJson<GcMicrotronics>(Data.AdditionalConfiguration);
            }

            EntryMicrotronicsUserName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Microtronics User name:",
                "Der User Name",
                (GcMicrotronics) GlobalConfig,
                nameof(GcMicrotronics.UserName),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryMicrotronicsPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Microtronics Passwort:",
                "Das Passwort des Users",
                (GcMicrotronics) GlobalConfig,
                nameof(GcMicrotronics.Password),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryMicrotronicsBackendDomain = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Microtronics Backend Domain:",
                "https://austria.microtronics.com",
                (GcMicrotronics) GlobalConfig,
                nameof(GcMicrotronics.BackendDomain),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryMicrotronicsUserName.PropertyChanged += EntryMicrotronicsOnPropertyChanged;
            EntryMicrotronicsPassword.PropertyChanged += EntryMicrotronicsOnPropertyChanged;
            EntryMicrotronicsBackendDomain.PropertyChanged += EntryMicrotronicsOnPropertyChanged;

            SetValidateFunctionsOfNotUsedEntries(EnumGlobalConfigTypes.Microtronics);
        }


        private void SetValidateFunctionsOfNotUsedEntries(EnumGlobalConfigTypes type)
        {
            if (type != EnumGlobalConfigTypes.Ttn)
            {
                if (EntryTtnApiKey != null)
                {
                    EntryTtnApiKey.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryTtnAppId != null)
                {
                    EntryTtnAppId.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryTtnUserId != null)
                {
                    EntryTtnUserId.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryTtnZone != null)
                {
                    EntryTtnZone.ValidateFunc = str => (string.Empty, true);
                }
            }

            if (type != EnumGlobalConfigTypes.Drei)
            {
                if (EntryDreiLoginName != null!)
                {
                    EntryDreiLoginName.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryDreiPassword != null!)
                {
                    EntryDreiPassword.ValidateFunc = str => (string.Empty, true);
                }
            }

            if (type != EnumGlobalConfigTypes.Microtronics)
            {
                if (EntryMicrotronicsUserName != null!)
                {
                    EntryMicrotronicsUserName.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryMicrotronicsPassword != null!)
                {
                    EntryMicrotronicsPassword.ValidateFunc = str => (string.Empty, true);
                }

                if (EntryMicrotronicsBackendDomain != null!)
                {
                    EntryMicrotronicsBackendDomain.ValidateFunc = str => (string.Empty, true);
                }
            }
        }

        private void EntryTtnOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VmEntry.Value))
            {
                Data.AdditionalConfiguration = ((GcTtn) GlobalConfig).ToJson();
            }
        }

        private void EntryNameOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Data.ConfigType == EnumGlobalConfigTypes.Ttn)
            {
                if (e.PropertyName == nameof(VmEntry.Value))
                {
                    ((GcTtn) GlobalConfig).Name = Data.Information.Name;
                    Data.AdditionalConfiguration = ((GcTtn) GlobalConfig).ToJson();
                }
            }
        }

        private void EntryDreiOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VmEntry.Value))
            {
                Data.AdditionalConfiguration = ((GcDrei) GlobalConfig).ToJson();
            }
        }

        private void EntryMicrotronicsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VmEntry.Value))
            {
                Data.AdditionalConfiguration = ((GcMicrotronics) GlobalConfig).ToJson();
            }
        }
    }
}