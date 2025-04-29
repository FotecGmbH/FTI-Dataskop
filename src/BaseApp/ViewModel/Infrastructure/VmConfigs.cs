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
using BDA.Common.Exchange.Configs.Upstream;
using BDA.Common.Exchange.Configs.Upstream.Drei;
using BDA.Common.Exchange.Configs.Upstream.Microtronics;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.MicrotronicsClient;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.ObjectEx;
using Biss.Serialize;
using Exchange.Resources;

namespace BaseApp.ViewModel.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         View für dynamische BDA Konfigurationen
    ///         ToDo Mko - dynamisch machen
    ///         Status für erste Release - TTN Konfiguration
    ///     </para>
    ///     Klasse VmConfigs. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewConfigs", true)]
    public class VmConfigs : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmConfigs.DesignInstance}"
        /// </summary>
        public static VmConfigs DesignInstance = new VmConfigs();

        private ExIotDevice _iotDevice = null!;

        /// <summary>
        ///     VmConfigs
        /// </summary>
        public VmConfigs() : base("Dynamische Konfiguration", subTitle: "Einstellungen für einen Sensor")
        {
            SetViewProperties(true);

            Loaded += (sender, args) => GcIotDeviceOnPropertyChanged(this, null!);
        }

        #region Properties

        /// <summary>
        ///     Global config - iot device
        /// </summary>
        public GcIotDevice GcIoTDevice { get; private set; } = new GcTtnIotDevice();

        /// <summary>
        ///     Ttn
        /// </summary>
        public GcTtnIotDevice Ttn
        {
            get => GcIoTDevice as GcTtnIotDevice ?? new GcTtnIotDevice(); // in wpf wird auf properties hiervon gebunden. in zukunft anders ueberlegen
        }

        /// <summary>
        ///     EntryTtnAppKey
        /// </summary>
        public VmEntry EntryTtnAppKey { get; set; } = null!;

        /// <summary>
        ///     EntryTtnDescription
        /// </summary>
        public VmEntry EntryTtnDescription { get; private set; } = null!;

        /// <summary>
        ///     EntryTtnAppEui
        /// </summary>
        public VmEntry EntryTtnAppEui { get; set; } = null!;

        /// <summary>
        ///     EntryTtnDeviceId
        /// </summary>
        public VmEntry EntryTtnDeviceId { get; set; } = null!;

        /// <summary>
        ///     EntryTtnDevEui
        /// </summary>
        public VmEntry EntryTtnDevEui { get; private set; } = null!;


        /// <summary>
        ///     PickerCopanyConfigs
        /// </summary>
        public VmPicker<ExGlobalConfig> PickerCopanyConfigs { get; private set; } = new VmPicker<ExGlobalConfig>(nameof(PickerCopanyConfigs));


        /// <summary>
        ///     PickerCopanyEnumLorawanVersion
        /// </summary>
        public VmPicker<EnumLorawanVersion> PickerEnumLorawanVersion { get; private set; } = new VmPicker<EnumLorawanVersion>(nameof(PickerEnumLorawanVersion));

        /// <summary>
        ///     PickerEnumLorawanPhysicalVersion
        /// </summary>
        public VmPicker<EnumLorawanPhysicalVersion> PickerEnumLorawanPhysicalVersion { get; private set; } = new VmPicker<EnumLorawanPhysicalVersion>(nameof(PickerEnumLorawanPhysicalVersion));

        /// <summary>
        ///     PickerEnumLorawanFrequencyPlanId
        /// </summary>
        public VmPicker<EnumLorawanFrequencyPlanId> PickerEnumLorawanFrequencyPlanId { get; private set; } = new VmPicker<EnumLorawanFrequencyPlanId>(nameof(PickerEnumLorawanFrequencyPlanId));

        /// <summary>
        ///     EntryDreiDeviceId
        /// </summary>
        public VmEntry EntryDreiDeviceId { get; set; } = null!;

        /// <summary>
        ///     EntryDreiDeviceId
        /// </summary>
        public VmEntry EntryDreiDevEui { get; set; } = null!;

        /// <summary>
        ///     PickerMicrotronicsCustomers (auf welche customer hat der benutzer zugriff?)
        /// </summary>
        public VmPicker<string> PickerMicrotronicsCustomers { get; private set; } = new VmPicker<string>(nameof(PickerMicrotronicsCustomers));

        /// <summary>
        ///     PickerMicrotronicsSites (auf welche sites hat der benutzer zugriff?)
        /// </summary>
        public VmPicker<string> PickerMicrotronicsSites { get; private set; } = new VmPicker<string>(nameof(PickerMicrotronicsSites));

        /// <summary>
        ///     PickerMicrotronicsConfigs (Welche Configuration soll gewählt werden?)
        /// </summary>
        public VmPicker<string> PickerMicrotronicsConfigs { get; private set; } = new VmPicker<string>(nameof(PickerMicrotronicsConfigs));

        /// <summary>
        ///     PickerMicrotronicsHistDataConfigs (Welche Configuration fuer historische daten soll gewählt werden?)
        /// </summary>
        public VmPicker<string> PickerMicrotronicsHistDataConfigs { get; private set; } = new VmPicker<string>(nameof(PickerMicrotronicsHistDataConfigs));


        /// <summary>
        ///     Serialisierte Daten
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;


        /// <summary>
        ///     If TTN-DEVEUI should be generated or not
        /// </summary>
        public bool GenerateDevEui { get; set; }

        /// <summary>
        ///     If TTN-APPEUI should be generated or not
        /// </summary>
        public bool GenerateAppEui { get; set; }

        /// <summary>
        ///     If TTN-APPKEY should be generated or not
        /// </summary>
        public bool GenerateAppKey { get; set; }

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
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            GcIoTDevice.PropertyChanged -= GcIotDeviceOnPropertyChanged;
            CheckSaveBehavior = null!;
            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await base.OnActivated(args).ConfigureAwait(true);
            if (args is ExIotDevice a)
            {
                _iotDevice = a;
            }
            else
            {
                throw new ArgumentNullException($"[{nameof(VmConfigs)}]({nameof(OnActivated)}): {nameof(args)}");
            }


            switch (_iotDevice.Upstream)
            {
                case EnumIotDeviceUpstreamTypes.Ttn:
                    TtnVisible = true;
                    DreiVisible = false;
                    MicrotronicsVisible = false;

                    await InitPickerPickerCopanyConfigs(EnumGlobalConfigTypes.Ttn).ConfigureAwait(true);

                    InitTtnEntriesAndEvents();
                    break;
                case EnumIotDeviceUpstreamTypes.Drei:
                    TtnVisible = false;
                    DreiVisible = true;
                    MicrotronicsVisible = false;
                    await InitPickerPickerCopanyConfigs(EnumGlobalConfigTypes.Drei).ConfigureAwait(true);

                    InitDreiEntriesAndEvents();
                    break;
                case EnumIotDeviceUpstreamTypes.Microtronics:
                    TtnVisible = false;
                    DreiVisible = false;
                    MicrotronicsVisible = true;

                    await InitPickerPickerCopanyConfigs(EnumGlobalConfigTypes.Microtronics).ConfigureAwait(true);

                    await InitMicrotronicsEntriesAndEvents().ConfigureAwait(true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            /*Ttn*/
            GcIoTDevice.PropertyChanged += GcIotDeviceOnPropertyChanged;
            if (GcIoTDevice.ConfigType == EnumGlobalConfigTypes.Ttn)
            {
                CheckSaveBehavior = new CheckSaveBissSerializeBehavior<GcTtnIotDevice>();
            }

            if (GcIoTDevice.ConfigType == EnumGlobalConfigTypes.Drei)
            {
                CheckSaveBehavior = new CheckSaveBissSerializeBehavior<GcDreiIotDevice>();
            }

            if (GcIoTDevice.ConfigType == EnumGlobalConfigTypes.Microtronics)
            {
                CheckSaveBehavior = new CheckSaveBissSerializeBehavior<GcMicrotronicsIotDevice>();
            }

            if (CheckSaveBehavior != null)
            {
                CheckSaveBehavior.SetCompareData(GcIoTDevice);
            }

            return base.OnLoaded();
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand("Ok", async () =>
            {
                if (this.GetAllInstancesWithType<VmEntry>().Any(p => !p.DataValid))
                {
                    await MsgBox.Show("Speichern leider nicht möglich!", "Eingabefehler").ConfigureAwait(false);
                    return;
                }

                _iotDevice.AdditionalConfiguration = AdditionalConfiguration;
                _iotDevice.GlobalConfigId = PickerCopanyConfigs.SelectedItem!.Key.Id;
                CheckSaveBehavior = null!;
                ViewResult = true;

                await Nav.Back().ConfigureAwait(true);
            }, CanExecuteSave, glyph: Glyphs.Check);
        }

        private async Task InitPickerPickerCopanyConfigs(EnumGlobalConfigTypes configType)
        {
            foreach (var gc in Dc.DcExGlobalConfig.Where(g => g.Data.CompanyId == _iotDevice.CompanyId && g.Data.ConfigType == configType).Select(s => s.Data))
            {
                PickerCopanyConfigs.AddKey(gc, gc.Information.Name);
            }

            if (PickerCopanyConfigs.Count == 0)
            {
                ViewResult = null;
                await MsgBox.Show("Es exisitiert noch keine Konfiguration für diese Konfigurationsart bei dieser Firma.").ConfigureAwait(true);
                await Nav.Back().ConfigureAwait(true);
            }
        }

        private async Task InitMicrotronicsEntriesAndEvents()
        {
            InitPickerCopanyConfigs(EnumGlobalConfigTypes.Microtronics);

            SetPickerCompanyConfigsAndGcIoTDevice<GcMicrotronicsIotDevice>(EnumGlobalConfigTypes.Microtronics);

            var microtronicsConfig = BissDeserialize.FromJson<GcMicrotronicsIotDevice?>(_iotDevice.AdditionalConfiguration);

            if (microtronicsConfig != null)
            {
                var config = Dc.DcExGlobalConfig.FirstOrDefault(g => g.Data.Information.Name == PickerCopanyConfigs.SelectedItem!.Key.Information.Name);

                if (config != null)
                {
                    microtronicsConfig.GcMicrotronicsCompany = BissDeserialize.FromJson<GcMicrotronics>(config.Data.AdditionalConfiguration);
                }

                _iotDevice.AdditionalConfiguration = microtronicsConfig.ToJson();

                var apiClient = new MicrotronicsApiClient(microtronicsConfig.GcMicrotronicsCompany.BackendDomain, microtronicsConfig.GcMicrotronicsCompany.UserName, microtronicsConfig.GcMicrotronicsCompany.Password);
                var customers = await apiClient.GetCustomerNames().ConfigureAwait(true);
                foreach (var customer in customers)
                {
                    PickerMicrotronicsCustomers.AddKey(customer, customer);
                }

                for (var i = 0; i < 10; i++)
                {
                    PickerMicrotronicsConfigs.AddKey("config" + i, "config" + i);
                    PickerMicrotronicsHistDataConfigs.AddKey("histdata" + i, "histdata" + i);
                }

                PickerMicrotronicsConfigs.SelectKey("config0");
                PickerMicrotronicsHistDataConfigs.SelectKey("histdata0");

                PickerMicrotronicsCustomers.SelectedItemChanged += PickerMicrotronicsCustomers_SelectedItemChanged;
                PickerMicrotronicsSites.SelectedItemChanged += PickerMicrotronicsSites_SelectedItemChanged;
                PickerMicrotronicsConfigs.SelectedItemChanged += PickerMicrotronicsConfigsOnSelectedItemChanged;
                PickerMicrotronicsHistDataConfigs.SelectedItemChanged += PickerMicrotronicsHistDataConfigsOnSelectedItemChanged;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerMicrotronicsConfigsOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<string>> e)
        {
            var curr = (GcMicrotronicsIotDevice) GcIoTDevice;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (curr != null)
            {
                curr.Configuration = e.CurrentItem.Key;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerMicrotronicsHistDataConfigsOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<string>> e)
        {
            var curr = (GcMicrotronicsIotDevice) GcIoTDevice;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (curr != null)
            {
                curr.HistDataConfiguration = e.CurrentItem.Key;
            }
        }

        /// <summary>
        ///     select sites for the selected customer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PickerMicrotronicsCustomers_SelectedItemChanged(object sender, EventArgs e)
        {
            PickerMicrotronicsSites.Clear();
            var microtronicsConfig = BissDeserialize.FromJson<GcMicrotronicsIotDevice?>(_iotDevice.AdditionalConfiguration);

            if (microtronicsConfig != null)
            {
                microtronicsConfig.CustomerId = PickerMicrotronicsCustomers.SelectedItem!.Key;
                _iotDevice.AdditionalConfiguration = microtronicsConfig.ToJson();

                var apiClient = new MicrotronicsApiClient(microtronicsConfig.GcMicrotronicsCompany.BackendDomain, microtronicsConfig.GcMicrotronicsCompany.UserName, microtronicsConfig.GcMicrotronicsCompany.Password);
                var sites = await apiClient.GetSites(PickerMicrotronicsCustomers.SelectedItem.Key).ConfigureAwait(true);

                PickerMicrotronicsSites.Clear();

                foreach (var site in sites)
                {
                    PickerMicrotronicsSites.AddKey(site, site);
                }
            }
        }

        //from the selected site, get the site id and set it and the customer id in the iot device
        private void PickerMicrotronicsSites_SelectedItemChanged(object sender, EventArgs e)
        {
            var microtronicsConfig = BissDeserialize.FromJson<GcMicrotronicsIotDevice?>(_iotDevice.AdditionalConfiguration);
            if (microtronicsConfig != null)
            {
                microtronicsConfig.SiteId = PickerMicrotronicsSites.SelectedItem!.Key;
                microtronicsConfig.CustomerId = PickerMicrotronicsCustomers.SelectedItem!.Key;
                _iotDevice.AdditionalConfiguration = microtronicsConfig.ToJson();
                var curr = (GcMicrotronicsIotDevice) GcIoTDevice;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (curr != null)
                {
                    curr.GcMicrotronicsCompany = microtronicsConfig.GcMicrotronicsCompany;
                    curr.CustomerId = microtronicsConfig.CustomerId;
                    curr.SiteId = microtronicsConfig.SiteId;
                }
                //GcIoTDevice = BissDeserialize.FromJson<GcMicrotronicsIotDevice>(_iotDevice.AdditionalConfiguration);

                //_ = CanExecuteSave();
            }
        }

        /// <summary>
        ///     Initializes entries and events for drei
        /// </summary>
        private void InitDreiEntriesAndEvents()
        {
            InitPickerCopanyConfigs(EnumGlobalConfigTypes.Drei);

            SetPickerCompanyConfigsAndGcIoTDevice<GcDreiIotDevice>(EnumGlobalConfigTypes.Drei);

            EntryDreiDeviceId = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Id des Iot Geräts:",
                "von Drei Name unique",
                ((GcDreiIotDevice) GcIoTDevice),
                nameof(GcDreiIotDevice.DeviceId),
                ValidateFuncTtnDeviceId,
                showTitle: false);
            EntryDreiDevEui = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "DevEui des Iot Geräts:",
                "DevEui des IoT Geräts",
                ((GcDreiIotDevice) GcIoTDevice),
                nameof(GcDreiIotDevice.DevEui),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );
        }

        /// <summary>
        ///     Initializes entries and events for ttn
        /// </summary>
        private void InitTtnEntriesAndEvents()
        {
            InitPickerCopanyConfigs(EnumGlobalConfigTypes.Ttn);

            foreach (var value in EnumUtil.GetValues<EnumLorawanVersion>())
            {
                PickerEnumLorawanVersion.AddKey(value, value.ToString());
            }

            foreach (var value in EnumUtil.GetValues<EnumLorawanPhysicalVersion>())
            {
                PickerEnumLorawanPhysicalVersion.AddKey(value, value.ToString());
            }

            foreach (var value in EnumUtil.GetValues<EnumLorawanFrequencyPlanId>())
            {
                PickerEnumLorawanFrequencyPlanId.AddKey(value, value.ToString());
            }

            SetPickerCompanyConfigsAndGcIoTDevice<GcTtnIotDevice>(EnumGlobalConfigTypes.Ttn);

            PickerCopanyConfigs.SelectedItemChanged += (sender, args) =>
            {
                var gc = args.CurrentItem.Key;
                ((GcTtnIotDevice) GcIoTDevice).GcTtnCompany = BissDeserialize.FromJson<GcTtn>(gc.AdditionalConfiguration);
            };
            PickerEnumLorawanVersion.SelectedItem = PickerEnumLorawanVersion.First(f => f.Key == ((GcTtnIotDevice) GcIoTDevice).LorawanVersion);
            PickerEnumLorawanPhysicalVersion.SelectedItem = PickerEnumLorawanPhysicalVersion.First(f => f.Key == ((GcTtnIotDevice) GcIoTDevice).LoraPhysicalVersion);
            PickerEnumLorawanFrequencyPlanId.SelectedItem = PickerEnumLorawanFrequencyPlanId.First(f => f.Key == ((GcTtnIotDevice) GcIoTDevice).LoraFrequency);


            EntryTtnDeviceId = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Id des Iot Geräts:",
                "Nur Kleinbuchstaben, Zahlen, und Bindestrich erlaubt",
                ((GcTtnIotDevice) GcIoTDevice),
                nameof(GcTtnIotDevice.DeviceId),
                ValidateFuncTtnDeviceId,
                showTitle: false
            );

            EntryTtnDevEui = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn Dev Eui:",
                "Wird vom Device benötigt um zu verbinden",
                ((GcTtnIotDevice) GcIoTDevice),
                nameof(GcTtnIotDevice.DevEui),
                showTitle: false
            );

            EntryTtnAppEui = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn App/Join Eui:",
                "app_eui des Geräts",
                ((GcTtnIotDevice) GcIoTDevice),
                nameof(GcTtnIotDevice.AppEui),
                showTitle: false
            );

            EntryTtnAppKey = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn AppKey:",
                "Appkey des Geräts",
                ((GcTtnIotDevice) GcIoTDevice),
                nameof(GcTtnIotDevice.AppKey),
                showTitle: false
            );

            EntryTtnDescription = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Ttn Beschreibung:",
                "Optional",
                ((GcTtnIotDevice) GcIoTDevice),
                nameof(GcTtnIotDevice.Description),
                showTitle: false
            );

            if (string.IsNullOrEmpty(((GcTtnIotDevice) GcIoTDevice).DevEui))
            {
                GenerateDevEui = true;
            }

            if (string.IsNullOrEmpty(((GcTtnIotDevice) GcIoTDevice).AppEui))
            {
                GenerateAppEui = true;
            }

            if (string.IsNullOrEmpty(((GcTtnIotDevice) GcIoTDevice).AppKey))
            {
                GenerateAppKey = true;
            }
        }

        private void InitPickerCopanyConfigs(EnumGlobalConfigTypes configTypes)
        {
            foreach (var gc in Dc.DcExGlobalConfig.Where(g => g.Data.CompanyId == _iotDevice.CompanyId && g.Data.ConfigType == configTypes).Select(s => s.Data))
            {
                PickerCopanyConfigs.AddKey(gc, gc.Information.Name);
            }
        }

        private bool CanExecuteSave()
        {
            var r = false;
            if (!IsLoaded)
            {
                if (View.CmdSaveHeader != null)
                {
                    View.CmdSaveHeader.IsVisible = r;
                }

                return r;
            }

            if (string.IsNullOrEmpty(_iotDevice.AdditionalConfiguration))
            {
                r = true;
            }
            else
            {
                if (CheckSaveBehavior != null)
                {
                    r = CheckSaveBehavior.Check();
                }
            }

            if (View.CmdSaveHeader != null)
            {
                View.CmdSaveHeader.IsVisible = r;
            }

            return r;
        }

        private void GcIotDeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AdditionalConfiguration = GcIoTDevice.ToJson();
            View.CmdSaveHeader?.CanExecute();
        }

        private (string hint, bool valid) ValidateFuncTtnDeviceId(string arg)
        {
            var r = VmEntryValidators.ValidateFuncStringEmpty(arg);
            if (!r.valid)
            {
                return r;
            }

            if (arg.Any(char.IsUpper))
            {
                return ("Großbuchstaben nicht erlaubt.", false);
            }

            if (arg.Any(char.IsWhiteSpace))
            {
                return ("Leerzeichen nicht erlaubt.", false);
            }

            return ("", true);
        }

        /// <summary>
        ///     Setzt den company picker und das GcIotDevice
        /// </summary>
        /// <typeparam name="T1">Typparameter der configuration</typeparam>
        /// <param name="upstreamType"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SetPickerCompanyConfigsAndGcIoTDevice<T1>(EnumGlobalConfigTypes upstreamType) where T1 : GcIotDevice
        {
            var config = BissDeserialize.FromJson<T1>(_iotDevice.AdditionalConfiguration);
            // ReSharper disable once RedundantAssignment
            var isNotValidConfig = false;

            switch (upstreamType)
            {
                case EnumGlobalConfigTypes.Ttn:
                    var configttn = config as GcTtnIotDevice;
                    configttn ??= GcTtnIotDevice.FromCompatibilityModel(BissDeserialize.FromJson<GcTtnIotDeviceCompat?>(_iotDevice.AdditionalConfiguration));
                    isNotValidConfig = configttn is null || string.IsNullOrEmpty(_iotDevice.AdditionalConfiguration) || string.IsNullOrEmpty(configttn.DeviceId);
                    break;
                case EnumGlobalConfigTypes.Drei:
                    var configdrei = config as GcDreiIotDevice;
                    configdrei ??= GcDreiIotDevice.FromCompatibilityModel(BissDeserialize.FromJson<GcDreiIotDeviceCompat?>(_iotDevice.AdditionalConfiguration));
                    isNotValidConfig = configdrei is null || string.IsNullOrEmpty(_iotDevice.AdditionalConfiguration) || string.IsNullOrEmpty(configdrei.DeviceId);
                    break;
                case EnumGlobalConfigTypes.Microtronics:
                    var configmic = config as GcMicrotronicsIotDevice;
                    isNotValidConfig = configmic is null || string.IsNullOrEmpty(_iotDevice.AdditionalConfiguration) || string.IsNullOrEmpty(configmic.CustomerId) || string.IsNullOrEmpty(configmic.SiteId) || _iotDevice.GlobalConfigId == null;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (isNotValidConfig)
            {
                var gc = PickerCopanyConfigs.First().Key;
                PickerCopanyConfigs.SelectKey(gc);

                GcIoTDevice = upstreamType switch
                {
                    EnumGlobalConfigTypes.Ttn => new GcTtnIotDevice {GcTtnCompany = BissDeserialize.FromJson<GcTtn>(gc.AdditionalConfiguration)},
                    EnumGlobalConfigTypes.Drei => new GcDreiIotDevice {GcDreiCompany = BissDeserialize.FromJson<GcDrei>(gc.AdditionalConfiguration)},
                    EnumGlobalConfigTypes.Microtronics => new GcMicrotronicsIotDevice {GcMicrotronicsCompany = BissDeserialize.FromJson<GcMicrotronics>(gc.AdditionalConfiguration)},
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                GcIoTDevice = BissDeserialize.FromJson<T1>(_iotDevice.AdditionalConfiguration);
                PickerCopanyConfigs.SelectKey(PickerCopanyConfigs.FirstOrDefault().Key);
                var gc = PickerCopanyConfigs.FirstOrDefault(w => w.Key.Id == _iotDevice.GlobalConfigId).Key;
                PickerCopanyConfigs.SelectKey(gc);
            }
        }
    }
}