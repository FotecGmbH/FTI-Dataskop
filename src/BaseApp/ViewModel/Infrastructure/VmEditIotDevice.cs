// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Configs.Downstreams;
using BDA.Common.Exchange.Configs.Downstreams.OpenSense;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream;
using BDA.Common.Exchange.Configs.Upstream.Drei;
using BDA.Common.Exchange.Configs.Upstream.Microtronics;
using BDA.Common.Exchange.Configs.Upstream.Opensense;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Configs.UserCode;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.MicrotronicsClient;
using BDA.Common.OpensenseClient;
using BDA.Common.ParserCompiler;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Interfaces;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BaseApp.ViewModel.Infrastructure
{
    /// <summary>
    ///     <para>Iot Device anlegen oder bearbeiten</para>
    ///     Klasse VmAddIotDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditIotDevice", true)]
    public class VmEditIotDevice : VmEditDcListPoint<ExIotDevice>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddIotDevice.DesignInstance}"
        /// </summary>
        public static VmEditIotDevice DesignInstance = new VmEditIotDevice();

        /// <summary>
        ///     Falls Tcp Upstream, dann ob secret eingegeben wird(device wurde bereits mit cli app erstellt)
        ///     oder ob device neu erstellt wird
        /// </summary>
        private bool _enterSecret = true;

        private long _gwIdOriginal;

        /// <summary>
        ///     VmAddIotDevice - Iot Device bearbeiten oder TTN Device anlegen
        /// </summary>
        public VmEditIotDevice() : base("IoT-Gerät", subTitle: "IoT-Gerät erstellen oder bearbeiten")
        {
            PickerPositionType.AddKey(EnumPositionSource.Pc, "Betriebssystem/Manuelle Eingabe");
            PickerPositionType.AddKey(EnumPositionSource.Internet, "Über Internet");
            PickerPositionType.AddKey(EnumPositionSource.Modul, "Von GPS Modul");
            PickerPositionType.AddKey(EnumPositionSource.Lbs, "Über Mobilfunk");

            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Ble, "Über bluetooth low energy");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.InGateway, "Iot Gerät läuft im Gateway");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Serial, "Über serielle Schnittstelle");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Tcp, "Über Ethernet / TCP");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Ttn, "Über TTN - TheThingsNetwork");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.OpenSense, "Über OpenSense");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Drei, "Über Drei");
            PickerUpstreamType.AddKey(EnumIotDeviceUpstreamTypes.Microtronics, "Über Microtronics");


            PickerPlattformType.AddKey(EnumIotDevicePlattforms.Arduino, "Arduino");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.DotNet, "Pc / Linux / Mac");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.Esp32, "ESP32");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.RaspberryPi, "Raspberry Pi");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.Prebuilt, "Fertiger Sensor");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.OpenSense, "Opensense Sensor");
            PickerPlattformType.AddKey(EnumIotDevicePlattforms.Meadow, "Meadow Sensor");


            PickerTransmissionType.AddKey(EnumTransmission.Elapsedtime, "Ablauf nach n Sekunden (lt. Übertragungsinterval)");
            PickerTransmissionType.AddKey(EnumTransmission.Instantly, "Sofort nach Messung");
            PickerTransmissionType.AddKey(EnumTransmission.NumberOfMeasurements, "Nach n Messwerten (lt. Übertragungsinterval)");

            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.Day, "Letzter Tag");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.Week, "Letzte Woche");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.TwoWeeks, "Letzte 2 Wochen");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.Month, "Letztes Monat");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.TwoMonths, "Letzte 2 Monate");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.HalfYear, "Letztes halbe Jahr");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.Year, "Letztes Jahr");
            PickerDataTimeframe.AddKey(EnumOpensenseDataTimeframe.TwoYears, "Letzte 2 Jahre");


            CheckBeforeSaving = BeforeSaving;
        }

        #region Properties

        /// <summary>
        ///     Derzeitiger Zustand der View
        /// </summary>
        public ViewState CurrentViewState { get; set; } = ViewState.Default;

        /// <summary>
        ///     EntryAdditionalProperties
        /// </summary>
        public VmEntry EntryAdditionalProperties { get; set; } = null!;

        /// <summary>
        ///     Vorname
        /// </summary>
        public VmEntry EntryName { get; set; } = null!;

        /// <summary>
        ///     Nachname
        /// </summary>
        public VmEntry EntryDescription { get; set; } = null!;

        /// <summary>
        ///     EntryPosLat
        /// </summary>
        public VmEntry EntryPosLat { get; private set; } = null!;

        /// <summary>
        ///     EntryPosLon
        /// </summary>
        public VmEntry EntryPosLon { get; private set; } = null!;

        /// <summary>
        ///     EntryPosAlt
        /// </summary>
        public VmEntry EntryPosAlt { get; private set; } = null!;

        /// <summary>
        ///     EntryAdditionalConfiguration
        /// </summary>
        public VmEntry EntryAdditionalConfiguration { get; private set; } = null!;

        /// <summary>
        ///     PickerPositionType
        /// </summary>
        public VmPicker<EnumPositionSource> PickerPositionType { get; private set; } = new VmPicker<EnumPositionSource>(nameof(PickerPositionType));

        /// <summary>
        ///     PickerPositionType
        /// </summary>
        public VmPicker<long> PickerGateways { get; private set; } = new VmPicker<long>(nameof(PickerGateways));

        /// <summary>
        ///     PickerUpstreamType
        /// </summary>
        public VmPicker<EnumIotDeviceUpstreamTypes> PickerUpstreamType { get; private set; } = new VmPicker<EnumIotDeviceUpstreamTypes>(nameof(PickerUpstreamType));

        /// <summary>
        ///     PickerPlattformType
        /// </summary>
        public VmPicker<EnumIotDevicePlattforms> PickerPlattformType { get; private set; } = new VmPicker<EnumIotDevicePlattforms>(nameof(PickerPlattformType));

        /// <summary>
        ///     PickerTransmissionType
        /// </summary>
        public VmPicker<EnumTransmission> PickerTransmissionType { get; private set; } = new VmPicker<EnumTransmission>(nameof(PickerTransmissionType));

        /// <summary>
        ///     PickerOpensenseTimeframe
        /// </summary>
        public VmPicker<EnumOpensenseDataTimeframe> PickerDataTimeframe { get; private set; } = new VmPicker<EnumOpensenseDataTimeframe>(nameof(PickerDataTimeframe));

        /// <summary>
        ///     PickerConverter
        /// </summary>
        public VmPicker<long> PickerConverter { get; private set; } = new VmPicker<long>(nameof(PickerConverter));


        /// <summary>
        ///     EntryTransmissionInterval
        /// </summary>
        public VmEntry EntryTransmissionInterval { get; set; } = null!;

        /// <summary>
        ///     EntryUserCode
        /// </summary>
        public VmEntry EntryUserCode { get; set; } = null!;

        /// <summary>
        ///     EntryOpensenseBoxId
        /// </summary>
        public VmEntry EntryOpensenseBoxId { get; set; } = null!;

        /// <summary>
        ///     EntryMeasurmentInterval
        /// </summary>
        public VmEntry EntryMeasurmentInterval { get; set; } = null!;

        /// <summary>
        ///     EntrySecret
        /// </summary>
        public VmEntry EntrySecret { get; set; } = null!;

        /// <summary>
        ///     Bearbeiten eine dynamischen Konfig
        /// </summary>
        public VmCommand CmdEditDynConfig { get; private set; } = null!;

        /// <summary>
        ///     Dynamischen Konfig Command anzeigen
        /// </summary>
        public bool ShowCmdEditDynConfig { get; set; }

        /// <summary>
        ///     Kann die AdditionalConfig manuell bearbeitet werden?
        /// </summary>
        public bool ShowAdditionalConfigEntry { get; set; } = true;


        /// <summary>
        ///     Codesnipped des Users
        /// </summary>
        public string CodeSnippet { get; set; } = "// Code zum Parsen der Daten";

        /// <summary>
        ///     Teil des Codesnippets der die Instanzierung der Result Values beinhaltet.
        /// </summary>
        public string CodeHeader { get; set; } = "";

        /// <summary>
        ///     Teil des Codesnippets der das Return-Statement beinhaltet
        /// </summary>
        public string CodeFooter { get; set; } = "";

        /// <summary>
        ///     Boxid für Opensense
        /// </summary>
        public string BoxId { get; set; } = "";

        /// <summary>
        ///     Datum ab wann historische Daten heruntergeladen werden sollen.
        /// </summary>
        public DateTime? OpensenseDownloadDate { get; set; } = null;

        /// <summary>
        ///     Ob Historische Daten heruntergeladen werden sollen.
        /// </summary>
        public bool DownloadData { get; set; }

        /// <summary>
        ///     Falls Tcp als Upstream ausgewählt wurde hat user die möglichkeit secret zu generieren oder einzugeben
        /// </summary>
        public bool ShowSecretEntry { get; set; }

        /// <summary>
        ///     Falls Tcp Upstream, dann ob secret eingegeben wird(device wurde bereits mit cli app erstellt)
        ///     oder ob device neu erstellt wird
        /// </summary>
        public bool EnterSecret
        {
            get { return _enterSecret; }
            set
            {
                if (value == false)
                {
                    EntrySecret.Value = Guid.NewGuid().ToString();
                }

                _enterSecret = value;
            }
        }

        /// <summary>
        ///     Multiple Results per measurement defintion
        /// </summary>
        public bool MultipleResultsPerDef { get; set; }

        #endregion

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            var t = base.OnActivated(args);

            if (!Dc.DcExDataconverters.Loading)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                await Dc.DcExDataconverters.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (Data.Upstream == EnumIotDeviceUpstreamTypes.Ttn || Data.Plattform == EnumIotDevicePlattforms.OpenSense)
            {
                ShowAdditionalConfigEntry = false;
            }

            var companyId = Dc.DcExGateways.First(f => f.Index == Data.GatewayId).Data.CompanyId;
            foreach (var gw in Dc.DcExGateways.Where(g => g.Data.CompanyId == companyId))
            {
                PickerGateways.AddKey(gw.Index, $"{gw.Data.Information.Name} ({gw.Index})");
            }

            _gwIdOriginal = Data.GatewayId!.Value;

            EntryName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Name:",
                "Name des Iot-Geräts",
                Data.Information,
                nameof(ExIotDevice.Information.Name),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDescription = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Beschreibung:",
                "Beschreibung",
                Data.Information,
                nameof(ExIotDevice.Information.Description),
                showTitle: false
            );

            EntryPosLat = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Latitude:",
                "GPS Latitude",
                Data.Location,
                nameof(ExIotDevice.Location.Latitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );

            EntryPosLon = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Longitude:",
                "GPS Longitude",
                Data.Location,
                nameof(ExIotDevice.Location.Longitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );

            EntryPosAlt = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Altitude:",
                "GPS Altitude",
                Data.Location,
                nameof(ExIotDevice.Location.Altitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );
            EntrySecret = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Secret:",
                "Secret",
                Data.DeviceCommon,
                nameof(ExIotDevice.DeviceCommon.Secret),
                str =>
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (PickerUpstreamType != null && PickerUpstreamType.SelectedItem != null && PickerUpstreamType.SelectedItem.Key == EnumIotDeviceUpstreamTypes.Tcp)
                    {
                        return VmEntryValidators.ValidateFuncStringEmpty(str);
                    }

                    return (string.Empty, true);
                }
                ,
                showTitle: false,
                showMaxChar: false
            );

            //EntryAdditionalConfiguration = new VmEntry(
            //    EnumVmEntryBehavior.StopTyping,
            //    "Dynamische Konfiguration:",
            //    "Dynamische Konfiguration",
            //    Data,
            //    nameof(ExIotDevice.AdditionalConfiguration),
            //    showTitle: false
            //);

            EntryAdditionalProperties = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Zusätzliche Einstellungen:",
                "Zusätzliche Einstellungen",
                Data,
                nameof(ExIotDevice.AdditionalProperties),
                showTitle: false
            );

            EntryMeasurmentInterval = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Messinterval (1=100ms):",
                "Messinterval in Zehntel Sekunden (1=100ms)",
                Data,
                nameof(ExIotDevice.MeasurmentInterval),
                VmEntryValidators.ValidateFuncInt,
                showTitle: false
            );

            EntryTransmissionInterval = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Übertragungsinterval (s/n):",
                "Übertragungsinterval in Sekunden oder nach n Messungen",
                Data,
                nameof(ExIotDevice.TransmissionInterval),
                VmEntryValidators.ValidateFuncInt,
                showTitle: false
            );

            EntryUserCode = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Usercode",
                "",
                this,
                nameof(CodeSnippet),
                showTitle: false
            );

            EntryOpensenseBoxId = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "BoxID",
                "Opensense Box ID",
                this,
                nameof(BoxId),
                showTitle: false
            );

            var currentPropsObject = BissDeserialize.FromJson<JObject>(Data.AdditionalConfiguration);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var currentUsercode = currentPropsObject?.GetValue("UserCode", StringComparison.InvariantCulture);
            var code = currentUsercode?.ToObject<ExUsercode>();
            if (code != null && (code.Header.Contains("multiple", StringComparison.InvariantCulture) || code.Header.Contains("List<ExValue>", StringComparison.InvariantCulture)))
            {
                MultipleResultsPerDef = true;
            }

            PropertyChanged += (sender, eventArgs) =>
            {
                if ((eventArgs.PropertyName == nameof(CurrentViewState) || eventArgs.PropertyName == nameof(MultipleResultsPerDef)) && CurrentViewState == ViewState.PrebuiltCustomcode)
                {
                    InitCodeSnipped();
                }

                if (eventArgs.PropertyName == nameof(CodeSnippet) || eventArgs.PropertyName == nameof(BoxId) || eventArgs.PropertyName == nameof(DownloadData))
                {
                    UpdateAdditionalConfig();
                }
            };


            PickerPositionType.SelectedItem = PickerPositionType.First(f => f.Key == Data.Location.Source);
            PickerPositionType.SelectedItemChanged += (sender, eventArgs) => Data.Location.Source = eventArgs.CurrentItem.Key;

            PickerDataTimeframe.SelectedItem = PickerDataTimeframe.First();
            PickerDataTimeframe.SelectedItemChanged += (sender, eventArgs) => UpdateAdditionalConfig();

            PickerGateways.SelectedItem = PickerGateways.First(f => f.Key == Data.GatewayId);
            PickerGateways.SelectedItemChanged += PickerGatewaysOnSelectedItemChanged;

            PickerUpstreamType.SelectedItem = PickerUpstreamType.First(f => f.Key == Data.Upstream);
            PickerUpstreamType.SelectedItemChanged += PickerUpstreamTypeOnSelectedItemChanged;

            PickerPlattformType.SelectedItem = PickerPlattformType.First(f => f.Key == Data.Plattform);
            PickerPlattformType.SelectedItemChanged += PickerPlattformType_SelectedItemChanged;

            PickerTransmissionType.SelectedItem = PickerTransmissionType.First(f => f.Key == Data.TransmissionType);
            PickerTransmissionType.SelectedItemChanged += PickerTransmissionTypeOnSelectedItemChanged;

            PickerConverter.AddKey(-1, DcListDataPoint.Index == -1 ? "Eigener Code" : "Nur bei neu angelegten Devices", "Eigenen Code schreiben.");
            // ReSharper disable once UnusedVariable
            var conv = Dc.DcExDataconverters.FirstOrDefault(c => c.Index == Data.DataConverterId);
            PickerConverter.SelectedItemChanged += PickerConverterOnSelectedItemChanged;
            PickerConverter.SelectKey(-1);

            if (DcListDataPoint.Index == -1)
            {
                foreach (var converter in Dc.DcExDataconverters)
                {
                    PickerConverter.AddKey(converter.Index, converter.Data.Displayname, converter.Data.Description);
                }
            }

            if (Data.Upstream == EnumIotDeviceUpstreamTypes.Ttn || Data.Upstream == EnumIotDeviceUpstreamTypes.Drei || Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics)
            {
                ShowCmdEditDynConfig = true;
            }

            UpdateState();
            ReadConfig();

            await t;
        }

        /// <summary>
        ///     Usercode in den AdditionalConfiguration updaten
        /// </summary>
        public void UpdateAdditionalConfig()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Data == null)
            {
                Logging.Log.LogWarning($"[{nameof(VmEditIotDevice)}]({nameof(UpdateAdditionalConfig)}): Data is null");
                return;
            }


            if (Data.Plattform == EnumIotDevicePlattforms.Prebuilt && Data.Upstream != EnumIotDeviceUpstreamTypes.Tcp)
            {
                var usercode = new ExUsercode {Header = CodeHeader, UserCode = CodeSnippet, Footer = CodeFooter};
                try
                {
                    var config = BissDeserialize.FromJson<GcIotDevice>(Data.AdditionalConfiguration);
                    if (config.ConfigType == EnumGlobalConfigTypes.Ttn)
                    {
                        config = BissDeserialize.FromJson<GcTtnIotDevice>(Data.AdditionalConfiguration);
                        config.ConfigType = EnumGlobalConfigTypes.Ttn;
                        config.UserCode = usercode;
                        Data.AdditionalConfiguration = config.ToJson();
                    }
                    else if (config.ConfigType == EnumGlobalConfigTypes.Drei)
                    {
                        config = BissDeserialize.FromJson<GcDreiIotDevice>(Data.AdditionalConfiguration);
                        config.ConfigType = EnumGlobalConfigTypes.Drei;
                        config.UserCode = usercode;
                        Data.AdditionalConfiguration = config.ToJson();
                    }
                }
                catch (InvalidOperationException e)
                {
                    Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show(e.Message).ConfigureAwait(false));
                }
            }

            if (Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics)
            {
                var mConfig = BissDeserialize.FromJson<GcMicrotronicsIotDevice>(Data.AdditionalConfiguration);
                mConfig.ConfigType = EnumGlobalConfigTypes.Microtronics;

                if (DownloadData)
                {
                    mConfig.DownloadDataSince = DateTime.Now.ToUniversalTime().AddDays(-1 * (int) PickerDataTimeframe.SelectedItem!.Key);
                }
                else
                {
                    mConfig.DownloadDataSince = null;
                }

                Data.Upstream = EnumIotDeviceUpstreamTypes.Microtronics;
                //config.UserCode = usercode;
                Data.AdditionalConfiguration = mConfig.ToJson();
            }

            if (Data.Plattform == EnumIotDevicePlattforms.OpenSense)
            {
                try
                {
                    var config = new GcBaseConverter<GcOpenSenseIotDevice>(Data.AdditionalConfiguration).Base;
                    config.UserCode = null;
                    config.ConfigType = EnumGlobalConfigTypes.OpenSense;
                    config.BoxId = BoxId;

                    if (DownloadData)
                    {
                        config.DownloadDataSince = DateTime.Now.ToUniversalTime().AddDays(-1 * (int) PickerDataTimeframe.SelectedItem!.Key);
                    }
                    else
                    {
                        config.DownloadDataSince = null;
                    }

                    Data.AdditionalConfiguration = config.ToJson();
                    Data.Upstream = EnumIotDeviceUpstreamTypes.OpenSense;
                }
                catch (InvalidOperationException e)
                {
                    Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show(e.Message).ConfigureAwait(false));
                }
            }
        }


        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdEditDynConfig = new VmCommand("", async () =>
            {
                IsNavigatedToNavToViewWithResult = true;
                _ = await Nav.ToViewWithResult(typeof(VmConfigs), Data).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                IsNavigatedToNavToViewWithResult = false;

                if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
                {
                    GCmdShowMenu.Execute(null!);
                }
            }, glyph: Glyphs.Pencil);

            base.InitializeCommands();
        }

        private void PickerConverterOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<long>> e)
        {
            if (e.CurrentItem.Key == -1)
            {
                Data.DataConverterId = null;
            }
            else
            {
                Data.DataConverterId = e.CurrentItem.Key;
            }

            if (Data.Plattform != EnumIotDevicePlattforms.Prebuilt)
            {
                Data.DataConverterId = null;
                return;
            }

            UpdateState();
            Logging.Log.LogInfo($"[{nameof(VmEditIotDevice)}]({nameof(PickerConverterOnSelectedItemChanged)}): Set Converter to {e.CurrentItem.Description}");
        }

        private void InitCodeSnipped()
        {
            CodeFooter = $"\treturn results; {Environment.NewLine}}}";

            var definitions = Dc.DcExMeasurementDefinition.Where(l => l.Data.IotDeviceId == DcListDataPoint.Index).ToArray();
            var headerBuilder = new StringBuilder("");
            if (MultipleResultsPerDef)
            {
                headerBuilder.AppendLine("// Parsermethod for multiple measurements.");
                headerBuilder.AppendLine("public static List<ExValue> Convert(byte[] input)");
                headerBuilder.AppendLine("{");
                headerBuilder.AppendLine("\tvar results = new List<ExValue>();");
                headerBuilder.AppendLine("");
            }
            else
            {
                headerBuilder.AppendLine("// Parsermethod for a single measurement.");
                headerBuilder.AppendLine("public static ExValue[] Convert(byte[] input)");
                headerBuilder.AppendLine("{");
                headerBuilder.AppendLine($"\tvar results = new ExValue[{definitions.Length}];");
                headerBuilder.AppendLine("");
            }


            for (var i = 0; i < definitions.Length; i++)
            {
                if (MultipleResultsPerDef)
                {
                    headerBuilder.AppendLine($"\t// {definitions[i].Data.Information.Name}");
                    headerBuilder.AppendLine($"\t// Identifier = {definitions[i].Index}, ValueType = {definitions[i].Data.ValueType.GetType().Name}.{definitions[i].Data.ValueType}");
                    headerBuilder.AppendLine("");
                }
                else
                {
                    headerBuilder.AppendLine($"\t// {definitions[i].Data.Information.Name}");
                    headerBuilder.AppendLine($"\tresults[{i}] = new ExValue() {{ Identifier = {definitions[i].Index}, ValueType = {definitions[i].Data.ValueType.GetType().Name}.{definitions[i].Data.ValueType}}};");
                    headerBuilder.AppendLine("");
                }
            }

            //EnsureCorrectConfiguration();
            CodeHeader = headerBuilder.ToString();
            var currentPropsObject = BissDeserialize.FromJson<JObject>(Data.AdditionalConfiguration);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var currentUsercode = currentPropsObject?.GetValue("UserCode", StringComparison.InvariantCulture);
            CodeSnippet = currentUsercode?.ToObject<ExUsercode>()?.UserCode ?? "";
            EntryUserCode.Value = CodeSnippet;
        }

        private void ReadConfig()
        {
            var currentConfigObj = BissDeserialize.FromJson<JObject?>(Data.AdditionalConfiguration);
            if (currentConfigObj is null)
            {
                Data.AdditionalConfiguration = new GcIotDevice().ToJson();
                currentConfigObj = BissDeserialize.FromJson<JObject>(Data.AdditionalConfiguration);
            }

            if (currentConfigObj.TryGetValue("BoxId", StringComparison.InvariantCultureIgnoreCase, out var boxId))
            {
                BoxId = boxId.Value<string>() ?? "";
                EntryOpensenseBoxId.Value = BoxId;
                this.InvokeOnPropertyChanged(nameof(BoxId));
            }
            //if (currentConfigObj.TryGetValue("UserCode", StringComparison.InvariantCultureIgnoreCase, out JToken? usercode))
            //{
            //    CodeSnippet = usercode.ToObject<ExUsercode>()?.UserCode ?? "";
            //}
        }

        private async Task<bool> BeforeSaving()
        {
            if (Data.Upstream == EnumIotDeviceUpstreamTypes.Ttn)
            {
                var additionalConfigOk = false;
                if (!string.IsNullOrEmpty(Data.AdditionalConfiguration))
                {
                    try
                    {
                        _ = BissDeserialize.FromJson<GcTtnIotDevice>(Data.AdditionalConfiguration);
                        additionalConfigOk = true;
                    }
                    catch (Exception exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmEditIotDevice)}]({nameof(BeforeSaving)}): {exception}");
                        throw;
                    }
                }

                if (!additionalConfigOk)
                {
                    await MsgBox.Show("Bitte prüfen Sie Ihre TTN Konfiguration.\r\n Speichern nicht möglich").ConfigureAwait(true);
                    return false;
                }
            }

            if (CurrentViewState == ViewState.PrebuiltCustomcode)
            {
                try
                {
                    var usercodeToken = BissDeserialize.FromJson<JObject>(Data.AdditionalConfiguration)["UserCode"];
                    if (usercodeToken is null)
                    {
                        Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show("Kein Usercode gefunden", "Error").ConfigureAwait(false));
                        return false;
                    }

                    var codeJson = usercodeToken.ToString();
                    var code = BissDeserialize.FromJson<ExUsercode>(codeJson);

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (code is null)
                    {
                        Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show("Kein Usercode gefunden", "Error").ConfigureAwait(false));
                        return false;
                    }


                    // Testen ob der Usercode valide ist.
                    var assemblyData = await Compiler.GetAssembly(code.CompleteCode).ConfigureAwait(false);
                    _ = assemblyData.DisposeAsync().ConfigureAwait(false);
                }
                catch (InvalidOperationException e)
                {
                    Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show(e.Message, "Error").ConfigureAwait(false));
                    return false;
                }
            }

            if (Data.Upstream == EnumIotDeviceUpstreamTypes.OpenSense)
            {
                var client = new OpensenseClient();
                var config = new GcBaseConverter<GcIotDevice>(Data.AdditionalConfiguration).ConvertTo<GcOpenSenseIotDevice>();
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (config != null && config.BoxId != null)
                {
                    var sensors = await client.GetCurrentValuesAsync(config.BoxId).ConfigureAwait(false);
                    var existingIds = Dc.DcExMeasurementDefinition.Where(def => def.Data.IotDeviceId == DcListDataPoint.Index).Select(d =>
                    {
                        var result = "";
                        try
                        {
                            result = new GcBaseConverter<GcDownstreamBase>(d.Data.AdditionalConfiguration).ConvertTo<GcDownstreamOpenSense>().SensorID;
                        }
                        catch
                        {
                            //Ignored
                        }

                        return result;
                    }).Where(s => !string.IsNullOrEmpty(s));
                    foreach (var sensor in sensors)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        if (existingIds!.Contains(sensor.OpensenseId))
                        {
                            continue;
                        }

                        var definition = new ExMeasurementDefinition
                        {
                            AdditionalConfiguration = (new GcDownstreamOpenSense {SensorID = sensor.OpensenseId}).ToJson(),
                            CompanyId = Data.CompanyId ?? -1,
                            DownstreamType = EnumIotDeviceDownstreamTypes.OpenSense,
                            ValueType = EnumValueTypes.Text,
                            Information = new ExInformation
                            {
                                CreatedDate = DateTime.UtcNow,
                                Description = $"Sensorid: {sensor.OpensenseId}",
                                Name = sensor.Title
                            },
                        };
                        Data.MeasurementDefinitions.Add(definition);
                    }
                }
                else
                {
                    Dispatcher?.RunOnDispatcher(async () => await MsgBox.Show("Konnte Box auf Opensense nicht finden.", "Error").ConfigureAwait(false));
                    return false;
                }
            }

            if (Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics)
            {
                var config = new GcBaseConverter<GcIotDevice>(Data.AdditionalConfiguration).ConvertTo<GcMicrotronicsIotDevice>();
                var client = new MicrotronicsApiClient(config.GcMicrotronicsCompany.BackendDomain, config.GcMicrotronicsCompany.UserName, config.GcMicrotronicsCompany.Password);
                var channels = await client.GetValidChannels(config.CustomerId, config.SiteId, config.Configuration);

                var existingIds = Dc.DcExMeasurementDefinition.Where(def => def.Data.IotDeviceId == DcListDataPoint.Index).Select(x => x.Data.Id);

                if (!existingIds.Any())
                {
                    foreach (var channel in channels)
                    {
                        var definition = new ExMeasurementDefinition
                        {
                            //AdditionalConfiguration = ().ToJson(),
                            CompanyId = Data.CompanyId ?? -1,
                            //DownstreamType = EnumIotDeviceDownstreamTypes.OpenSense,
                            ValueType = EnumValueTypes.Text,
                            Information = new ExInformation
                            {
                                CreatedDate = DateTime.UtcNow,
                                Description = channel,
                                Name = channel //sensor.Title
                            },
                        };
                        Data.MeasurementDefinitions.Add(definition);
                    }
#pragma warning disable CS0618 // Type or member is obsolete
                }
            }

            return true;
        }

        private async void PickerTransmissionTypeOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumTransmission>> e)
        {
            await MsgBox.Show("Diese Funktion is in einer späteren Version möglich!").ConfigureAwait(true);
            PickerTransmissionType.SelectedItemChanged -= PickerTransmissionTypeOnSelectedItemChanged;
            PickerTransmissionType.SelectedItem = e.OldItem;
#pragma warning restore CS0618 // Type or member is obsolete
            PickerTransmissionType.SelectedItemChanged += PickerTransmissionTypeOnSelectedItemChanged;
        }

        private void UpdateState()
        {
            if (Data.Upstream == EnumIotDeviceUpstreamTypes.OpenSense)
            {
                CurrentViewState = ViewState.OpenSense;
            }
            else if (Data.Plattform == EnumIotDevicePlattforms.Prebuilt && PickerConverter.SelectedItem!.Key == -1 && Data.Upstream != EnumIotDeviceUpstreamTypes.Tcp)
            {
                CurrentViewState = ViewState.PrebuiltCustomcode;
            }
            else if (Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics)
            {
                CurrentViewState = ViewState.Microtronics;
            }
            else if (Data.Plattform == EnumIotDevicePlattforms.Prebuilt && Data.Upstream != EnumIotDeviceUpstreamTypes.Tcp)
            {
                CurrentViewState = ViewState.Prebuilt;
            }
            else
            {
                CurrentViewState = ViewState.Default;
            }
        }

        private void PickerPlattformType_SelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumIotDevicePlattforms>> e)
        {
            //await MsgBox.Show("Das ist leider nicht möglich!").ConfigureAwait(true);
            //PickerPlattformType.SelectedItemChanged -= PickerPlattformType_SelectedItemChanged;
            //PickerPlattformType.SelectedItem = e.OldItem;
            //PickerPlattformType.SelectedItemChanged += PickerPlattformType_SelectedItemChanged;
            Data.Plattform = e.CurrentItem.Key;
            if (Data.Plattform != EnumIotDevicePlattforms.Prebuilt)
            {
                Data.DataConverterId = null;
            }

            UpdateState();
            UpdateAdditionalConfig();
        }


        private async void PickerUpstreamTypeOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumIotDeviceUpstreamTypes>> e)
        {
            if (e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.OpenSense || e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.Ttn || e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.Tcp || e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.Drei || e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.Microtronics)
            {
                Data.Upstream = e.CurrentItem.Key;
                ShowCmdEditDynConfig = Data.Upstream == EnumIotDeviceUpstreamTypes.Ttn || Data.Upstream == EnumIotDeviceUpstreamTypes.Drei || Data.Upstream == EnumIotDeviceUpstreamTypes.Microtronics;
                ShowSecretEntry = (e.CurrentItem.Key == EnumIotDeviceUpstreamTypes.Tcp);
                UpdateState();
                return;
            }

            await MsgBox.Show("Das ist leider nicht möglich!").ConfigureAwait(true);
            PickerUpstreamType.SelectedItemChanged -= PickerUpstreamTypeOnSelectedItemChanged;
#pragma warning disable CS0618 // Type or member is obsolete
            PickerUpstreamType.SelectedItem = e.OldItem;
#pragma warning restore CS0618 // Type or member is obsolete
            View.Refresh();
            PickerUpstreamType.SelectedItemChanged += PickerUpstreamTypeOnSelectedItemChanged;
        }


        /// <summary>
        ///     Gateway Änderung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PickerGatewaysOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<long>> e)
        {
            if (e.CurrentItem.Key != _gwIdOriginal)
            {
                var msg = await MsgBox.Show("Iot Gerät wirklich einem anderen Gateway zuweisen?",
                    "Achtung",
                    VmMessageBoxButton.YesNo).ConfigureAwait(true);

                if (msg == VmMessageBoxResult.No)
                {
                    PickerGateways.SelectedItemChanged -= PickerGatewaysOnSelectedItemChanged;
#pragma warning disable CS0618 // Type or member is obsolete
                    PickerGateways.SelectedItem = e.OldItem;
#pragma warning restore CS0618 // Type or member is obsolete
                    PickerGateways.SelectedItemChanged += PickerGatewaysOnSelectedItemChanged;

                    return;
                }
            }

            Data.GatewayId = e.CurrentItem.Key;
        }
    }

    /// <summary>
    /// ViewState
    /// </summary>
    public enum ViewState
    {
        /// <summary>
        /// PrebuiltCustomcode
        /// </summary>
        PrebuiltCustomcode,
        /// <summary>
        /// Prebuilt
        /// </summary>
        Prebuilt,
        /// <summary>
        /// OpenSense
        /// </summary>
        OpenSense,
        /// <summary>
        /// Microtronics
        /// </summary>
        Microtronics,
        /// <summary>
        /// Default
        /// </summary>
        Default
    }

    /// <summary>
    /// ViewElement
    /// </summary>
    public enum ViewElement
    {
        /// <summary>
        /// Upstream
        /// </summary>
        Upstream,
        /// <summary>
        /// ConverterType
        /// </summary>
        ConverterType,
        /// <summary>
        /// TransmissionType
        /// </summary>
        TransmissionType,
        /// <summary>
        /// TransmissionInterval
        /// </summary>
        TransmissionInterval,
        /// <summary>
        /// MeasurementInterval
        /// </summary>
        MeasurementInterval,
        /// <summary>
        /// CodeArea
        /// </summary>
        CodeArea,
        /// <summary>
        /// OpensenseBoxId
        /// </summary>
        OpensenseBoxId,
        /// <summary>
        /// HistoricalData
        /// </summary>
        HistoricalData,
        /// <summary>
        /// Platform
        /// </summary>
        Platform
    }
}