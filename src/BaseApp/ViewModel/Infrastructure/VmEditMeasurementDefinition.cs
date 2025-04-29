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
using BaseApp.Connectivity;
using BaseApp.Helper;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams;
using BDA.Common.Exchange.Configs.Downstreams.Arduino;
using BDA.Common.Exchange.Configs.Downstreams.Custom;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Downstreams.Esp32;
using BDA.Common.Exchange.Configs.Downstreams.I2c;
using BDA.Common.Exchange.Configs.Downstreams.Meadow;
using BDA.Common.Exchange.Configs.Downstreams.OpenSense;
using BDA.Common.Exchange.Configs.Downstreams.Pi;
using BDA.Common.Exchange.Configs.Downstreams.Prebuilt;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Configs.Plattform;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Dc.Client;
using Biss.ObjectEx;
using Biss.Serialize;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace BaseApp.ViewModel.Infrastructure
{
    /// <summary>
    ///     <para>Messwertdefinition bearbeiten</para>
    ///     Klasse VmEditMeasurementDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditMeasurementDefinition", true)]
    public class VmEditMeasurementDefinition : VmEditDcListPoint<ExMeasurementDefinition>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditMeasurementDefinition.DesignInstance}"
        /// </summary>
        public static VmEditMeasurementDefinition DesignInstance = new VmEditMeasurementDefinition();

        private GcBaseConverter<GcDownstreamBase> _configBaseOriginal = null!;

        private GcDownstreamCustom? _customConfigBase;
        private bool _iotMeasurementInterval = true;

        private ConfigPlattformBase _plattform = null!;
        private GcDownstreamPrebuilt? _prebuiltConfigBase;

        private ExMeasurementDefinition? _startSelectedExMeasurementDefinition;

        /// <summary>
        ///     VmEditMeasurementDefinition
        /// </summary>
        public VmEditMeasurementDefinition() : base("Messwertdefinition", subTitle: "Messwertdefinition erstellen oder bearbeiten")
        {
            PickerValueType.AddKey(EnumValueTypes.Number, "Zahl - Double, Int, byte, ...");
            PickerValueType.AddKey(EnumValueTypes.Data, "Datenarray - byte[]");
            PickerValueType.AddKey(EnumValueTypes.Text, "Text - string");
            PickerValueType.AddKey(EnumValueTypes.Image, "Bild als Datenarry - byte[]");
            PickerValueType.AddKey(EnumValueTypes.Bit, "Bit / Boolean");

            foreach (var t in EnumUtil.GetValues<EnumRawValueTypes>())
            {
                PickerRawValueTypes.AddKey(t, $"{t}");
            }
        }

        #region Properties

        /// <summary>
        ///     Messintervall des Iot Geräts verwenden
        /// </summary>
        public bool IotMeasurementInterval
        {
            get => _iotMeasurementInterval;
            set
            {
                _iotMeasurementInterval = value;
                if (_iotMeasurementInterval)
                {
                    if (EntryMeasurmentInterval != null!)
                    {
                        EntryMeasurmentInterval.Value = "-1";
                    }
                }
                else
                {
                    if (EntryMeasurmentInterval != null!)
                    {
                        EntryMeasurmentInterval.Value = "10";
                    }
                }
            }
        }

        /// <summary>
        ///     Iot Gerät des Messwerts
        /// </summary>
        public DcListDataPoint<ExIotDevice> IotDevice { get; set; } = null!;

        /// <summary>
        ///     Dev Infos
        /// </summary>
        public string StateMachineBytes { get; set; } = string.Empty;

        /// <summary>
        ///     EntryAdditionalProperties
        /// </summary>
        public VmEntry EntryAdditionalProperties { get; set; } = null!;

        /// <summary>
        ///     EntryMeasurmentInterval
        /// </summary>
        public VmEntry EntryMeasurmentInterval { get; set; } = null!;

        /// <summary>
        ///     EntryDescription
        /// </summary>
        public VmEntry EntryDescription { get; set; } = null!;

        /// <summary>
        ///     ConfigBase
        /// </summary>
        public GcBaseConverter<GcDownstreamBase> ConfigBase { get; set; } = null!;

        /// <summary>
        ///     EntryName
        /// </summary>
        public VmEntry EntryName { get; set; } = null!;

        /// <summary>
        ///     CanEditPickerRawValueTypes
        /// </summary>
        public bool CanEditPickerRawValueTypes { get; set; }

        /// <summary>
        ///     CanEditRawValueByteCount
        /// </summary>
        public bool CanEditRawValueByteCount { get; set; }

        /// <summary>
        ///     ShowEditPickerRawValueTypes
        /// </summary>
        public bool ShowEditPickerRawValueTypes { get; set; } = true;

        /// <summary>
        ///     ShowCanEditRawValueByteCount
        /// </summary>
        public bool ShowCanEditRawValueByteCount { get; set; } = true;

        /// <summary>
        ///     CanValuePickerChanged
        /// </summary>
        public bool CanValuePickerChanged { get; set; }

        /// <summary>
        ///     OpCode Eingabe
        /// </summary>
        public bool ShowCustomOpCode { get; set; }

        /// <summary>
        ///     Op-Code für "eigene Funktion"
        /// </summary>
        public VmEntry EntryCustomOpCode { get; private set; } = null!;

        /// <summary>
        ///     RawValueByteCount
        /// </summary>
        public VmEntry EntryRawValueByteCount { get; private set; } = null!;

        /// <summary>
        ///     PickerRawValueTypes
        /// </summary>
        public VmPicker<EnumRawValueTypes> PickerRawValueTypes { get; private set; } = new VmPicker<EnumRawValueTypes>(nameof(PickerRawValueTypes));

        /// <summary>
        ///     PickerDownstreamType
        /// </summary>
        public VmPicker<EnumIotDeviceDownstreamTypes> PickerDownstreamType { get; private set; } = new VmPicker<EnumIotDeviceDownstreamTypes>(nameof(PickerDownstreamType));

        /// <summary>
        ///     PickerValueType
        /// </summary>
        public VmPicker<EnumValueTypes> PickerValueType { get; private set; } = new VmPicker<EnumValueTypes>(nameof(PickerValueType));

        /// <summary>
        ///     PickerPredefinedMeasurements
        /// </summary>
        public VmPicker<ExMeasurementDefinition> PickerPredefinedMeasurements { get; private set; } = new VmPicker<ExMeasurementDefinition>(nameof(PickerPredefinedMeasurements));

        /// <summary>
        ///     EntryPosLat
        /// </summary>
        public VmEntry EntryVirtPosLat { get; private set; } = null!;

        /// <summary>
        ///     EntryPosLon
        /// </summary>
        public VmEntry EntryVirtPosLon { get; private set; } = null!;

        /// <summary>
        ///     EntryVirtPosRadius
        /// </summary>
        public VmEntry EntryVirtPosRadius { get; private set; } = null!;

        /// <summary>
        ///     EntryVirtFloatMin
        /// </summary>
        public VmEntry EntryVirtFloatMin { get; private set; } = null!;

        /// <summary>
        ///     EntryVirtFloatMax
        /// </summary>
        public VmEntry EntryVirtFloatMax { get; private set; } = null!;


        /// <summary>
        ///     Einstellungen für virtuellen Float anzeigen
        /// </summary>
        public bool ShowVirtualFloat { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer uerberschreitung
        /// </summary>
        public bool IfThresholdExceed
        {
            get => ThresholdCheck(EnumThresholdType.Exceed);
            set => ThresholdCheckSet(EnumThresholdType.Exceed, value);
        }

        /// <summary>
        ///     Schwellenwert fuer uerberschreitung
        /// </summary>
        public VmEntry EntryThresholdExceedValue { get; private set; } = null!;

        /// <summary>
        ///     Ob Schwellenwert fuer unterschreitung
        /// </summary>
        public bool IfThresholdFallBelow
        {
            get => ThresholdCheck(EnumThresholdType.FallBelow);
            set => ThresholdCheckSet(EnumThresholdType.FallBelow, value);
        }

        /// <summary>
        ///     Schwellenwert fuer unterschreitung
        /// </summary>
        public VmEntry EntryThresholdFallBelowValue { get; private set; } = null!;

        /// <summary>
        ///     Ob Schwellenwert fuer delta
        /// </summary>
        public bool IfThresholdDelta
        {
            get => ThresholdCheck(EnumThresholdType.Delta);
            set => ThresholdCheckSet(EnumThresholdType.Delta, value);
        }

        /// <summary>
        ///     Schwellenwert fuer Delta
        /// </summary>
        public VmEntry EntryThresholdDeltaValue { get; private set; } = null!;

        /// <summary>
        ///     Sollen die schwellenwertdarstellungen dargestellt werden? macht nur sinn wenn die gegenüberliegende software damit
        ///     umgehen kann
        /// </summary>
        public bool ShowThresholds
        {
            get
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (IotDevice == null)
                {
                    return false;
                }

                return IotDevice.Data.IsIotDotnetSensor || IotDevice.Data.Upstream == EnumIotDeviceUpstreamTypes.Tcp;
            }
        }

        #endregion

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            AttachDetachEvents(true);
            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachDetachEvents(false);
            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await base.OnActivated(args).ConfigureAwait(true);

            var iot = Dc.DcExIotDevices.FirstOrDefault(f => f.Index == Data.IotDeviceId);
            if (iot == null || Data.IotDeviceId <= 0)
            {
                await MsgBox.Show("Das Iot Gerät des Messwerts kann zurzeit nicht bearbeitet werden.").ConfigureAwait(true);
                await Nav.Back().ConfigureAwait(true);
            }

            IotDevice = iot!;


            if (!IotDevice.Data.IsIotDotnetSensor && IotDevice.Data.Upstream != EnumIotDeviceUpstreamTypes.Tcp)
            {
                Data.MeasurementInterval = -1;
            }

            if (string.IsNullOrEmpty(Data.AdditionalConfiguration))
            {
                await MsgBox.Show("Messwert kann zurzeit nicht bearbeitet werden.").ConfigureAwait(true);
                await Nav.Back().ConfigureAwait(true);
                return;
            }

            InitConfigBase(Data.AdditionalConfiguration);

            InitPlattform();

            if (Data != null && Data.DownstreamType != 0 && PickerDownstreamType.Any(f => f.Key == Data.DownstreamType))
            {
                PickerDownstreamType.SelectKey(Data.DownstreamType);
            }
            else if (PickerDownstreamType.Any(f => f.Key == EnumIotDeviceDownstreamTypes.Prebuilt))
            {
                PickerDownstreamType.SelectKey(EnumIotDeviceDownstreamTypes.Prebuilt);
            }
            else
            {
                PickerDownstreamType.SelectedItem = PickerDownstreamType.First(f => f.Key == Data!.DownstreamType);
            }

            PickerDownstreamType.SelectedItemChanged += PickerDownstreamTypeOnSelectedItemChanged;

            PickerDownstreamTypeOnSelectedItemChanged(this, null!);
            PickerPredefinedMeasurements.SelectedItemChanged += PickerPredefinedMeasurementsOnSelectedItemChanged;

            PickerRawValueTypes.SelectedItemChanged += PickerRawValueTypesOnSelectedItemChanged;
            PickerValueType.SelectedItemChanged += PickerValueTypeOnSelectedItemChanged;

            InitViewElements();
            Data!.PropertyChanged += DataOnPropertyChanged;
            UpdateStateMachineBytes();

#pragma warning disable CS0618 // Type or member is obsolete
            Dc.DcExNewValueNotifications.ReadListData();
#pragma warning restore CS0618 // Type or member is obsolete

            //Benachrichtigungen filtern
            Dc.DcExNewValueNotifications.FilterList(x => x.Data.MeasurementDefinitionId == Data.Id);
        }

        /// <summary>
        /// ThresholdCheck
        /// </summary>
        /// <param name="thresholdTypeType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool ThresholdCheck(EnumThresholdType thresholdTypeType)
        {
            if (Data != null)
            {
                var conf = GetGcDownstream(Data.DownstreamType);
                if (conf != null)
                {
                    switch (thresholdTypeType)
                    {
                        case EnumThresholdType.Exceed:
                            return conf.IfThresholdExceed;
                        case EnumThresholdType.FallBelow:
                            return conf.IfThresholdFallBelow;
                        case EnumThresholdType.Delta:
                            return conf.IfThresholdDelta;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// ThresholdCheckSet
        /// </summary>
        /// <param name="thresholdTypeType"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ThresholdCheckSet(EnumThresholdType thresholdTypeType, bool value)
        {
            if (Data != null)
            {
                var conf = GetGcDownstream(Data.DownstreamType);

                switch (thresholdTypeType)
                {
                    case EnumThresholdType.Exceed:
                        conf.IfThresholdExceed = value;
                        break;
                    case EnumThresholdType.FallBelow:
                        conf.IfThresholdFallBelow = value;
                        break;
                    case EnumThresholdType.Delta:
                        conf.IfThresholdDelta = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Data.AdditionalConfiguration = conf.ToJson();
            }
        }

        /// <summary>
        /// GetGcDownstream
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GcDownstreamBase GetGcDownstream(EnumIotDeviceDownstreamTypes type)
        {
            switch (type)
            {
                case EnumIotDeviceDownstreamTypes.Pi:
                    return new GcBaseConverter<GcDownstreamPi>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamPi>();
                case EnumIotDeviceDownstreamTypes.Arduino:
                    return new GcBaseConverter<GcDownstreamArduino>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamArduino>();
                case EnumIotDeviceDownstreamTypes.Custom:
                    return new GcBaseConverter<GcDownstreamCustom>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamCustom>();
                case EnumIotDeviceDownstreamTypes.DotNet:
                    return new GcBaseConverter<GcDownstreamDotNet>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamDotNet>();
                case EnumIotDeviceDownstreamTypes.I2C:
                    return new GcBaseConverter<GcDownstreamI2c>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamI2c>();
                case EnumIotDeviceDownstreamTypes.Meadow:
                    return new GcBaseConverter<GcDownstreamMeadow>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamMeadow>();
                case EnumIotDeviceDownstreamTypes.Esp32:
                    return new GcBaseConverter<GcDownstreamEsp32>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamEsp32>();
                case EnumIotDeviceDownstreamTypes.Virtual:
                    return new GcBaseConverter<GcDownstreamVirtualBase>(Data.AdditionalConfiguration).ConvertTo<GcDownstreamVirtualBase>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// ThresholdValueChange
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="thresholdTypeType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ThresholdValueChange(string strValue, EnumThresholdType thresholdTypeType)
        {
            if (!float.TryParse(strValue, out var value))
            {
                return;
            }

            if (Data != null)
            {
                var conf = GetGcDownstream(Data.DownstreamType);

                switch (thresholdTypeType)
                {
                    case EnumThresholdType.Exceed:
                        conf.ThresholdExceedValue = value;
                        break;
                    case EnumThresholdType.FallBelow:
                        conf.ThresholdFallBelowValue = value;
                        break;
                    case EnumThresholdType.Delta:
                        conf.ThresholdDeltaValue = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Data.AdditionalConfiguration = conf.ToJson();
            }
        }

        #region Commands

        // ReSharper disable once RedundantOverriddenMember
        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }

        #endregion

        private void AttachDetachEvents(bool attach)
        {
            if (attach)
            {
                Dc.DcExNewValueNotifications.CollectionEvent -= DcExNewValueNotifications_CollectionEvent;
                Dc.DcExNewValueNotifications.CollectionEvent += DcExNewValueNotifications_CollectionEvent;
            }
            else
            {
                Dc.DcExNewValueNotifications.CollectionEvent -= DcExNewValueNotifications_CollectionEvent;
            }
        }

        private void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExMeasurementDefinition.AdditionalConfiguration))
            {
                UpdateStateMachineBytes();
            }
        }

        private void UpdateStateMachineBytes()
        {
            var baseTmp = new GcBaseConverter<GcDownstreamBase>(Data.AdditionalConfiguration);
            // ReSharper disable once RedundantAssignment
            GcDownstreamBase? tmp = null;

            switch (PickerDownstreamType.SelectedItem!.Key)
            {
                case EnumIotDeviceDownstreamTypes.Virtual:
                    tmp = baseTmp.ConvertTo<GcDownstreamVirtualFloat>();
                    break;
                case EnumIotDeviceDownstreamTypes.I2C:
                    tmp = baseTmp.ConvertTo<GcDownstreamI2c>();
                    break;
                case EnumIotDeviceDownstreamTypes.Spi:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Modbus:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Pi:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Arduino:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Esp32:
                    tmp = baseTmp.ConvertTo<GcDownstreamEsp32>();
                    break;
                case EnumIotDeviceDownstreamTypes.DotNet:
                    tmp = baseTmp.ConvertTo<GcDownstreamDotNet>();
                    break;
                case EnumIotDeviceDownstreamTypes.Custom:
                    tmp = baseTmp.ConvertTo<GcDownstreamCustom>();
                    break;
                case EnumIotDeviceDownstreamTypes.Prebuilt:
                    tmp = baseTmp.ConvertTo<GcDownstreamPrebuilt>();
                    break;
                case EnumIotDeviceDownstreamTypes.OpenSense:
                    tmp = baseTmp.ConvertTo<GcDownstreamOpenSense>();
                    break;
                case EnumIotDeviceDownstreamTypes.Meadow:
                    tmp = baseTmp.ConvertTo<GcDownstreamMeadow>();
                    break;
                case EnumIotDeviceDownstreamTypes.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tmp != null!)
            {
                StateMachineBytes = GcByteHelper.BytesToHexString(tmp.ToStateMachine(PickerDownstreamType.SelectedItem.Key));
            }
        }

        private void PickerValueTypeOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumValueTypes>> e)
        {
            PickerRawValueTypesOnSelectedItemChanged(sender, null!);
            Data.ValueType = PickerValueType.SelectedItem!.Key;
        }

        private void InitConfigBase(string data)
        {
            if (_configBaseOriginal == null!)
            {
                _configBaseOriginal = new GcBaseConverter<GcDownstreamBase>(data);
                ConfigBase = _configBaseOriginal;
            }
            else
            {
                ConfigBase = new GcBaseConverter<GcDownstreamBase>(data);
            }

            ConfigBase.Base.RawValueDefinition.PropertyChanged += RawValueDefinitionOnPropertyChanged;
            PickerRawValueTypes.SelectKey(ConfigBase.Base.RawValueDefinition.RawValueType);
            PickerRawValueTypesOnSelectedItemChanged(this, null!);

            PickerValueType.SelectKey(Data.ValueType);
        }

        private void PickerRawValueTypesOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumRawValueTypes>> e)
        {
            if (PickerDownstreamType.SelectedItem == null!)
            {
                return;
            }

            if (PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.Custom || PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.Prebuilt)
            {
                CanValuePickerChanged = true;

                if (_customConfigBase != null)
                {
                    _customConfigBase.ValueType = PickerValueType.SelectedItem!.Key;
                }

                if (_prebuiltConfigBase != null)
                {
                    _prebuiltConfigBase.ValueType = PickerValueType.SelectedItem!.Key;
                }

                switch (PickerValueType.SelectedItem!.Key)
                {
                    case EnumValueTypes.Number:
                        CanEditPickerRawValueTypes = true;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        break;
                    case EnumValueTypes.Data:
                    case EnumValueTypes.Image:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = false;
                        ConfigBase.Base.RawValueDefinition.ByteCount = -1;
                        if (e == null! || e.CurrentItem == null! || e.CurrentItem.Key != EnumRawValueTypes.ByteArray)
                        {
                            PickerRawValueTypes.SelectedItemChanged -= PickerRawValueTypesOnSelectedItemChanged;
                            PickerRawValueTypes.SelectKey(EnumRawValueTypes.ByteArray);
                            PickerRawValueTypes.SelectedItemChanged += PickerRawValueTypesOnSelectedItemChanged;
                        }

                        break;
                    case EnumValueTypes.Text:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = false;
                        ShowEditPickerRawValueTypes = false;
                        ConfigBase.Base.RawValueDefinition.ByteCount = -1;
                        if (e == null! || e.CurrentItem == null! || e.CurrentItem.Key != EnumRawValueTypes.Custom)
                        {
                            PickerRawValueTypes.SelectedItemChanged -= PickerRawValueTypesOnSelectedItemChanged;
                            PickerRawValueTypes.SelectKey(EnumRawValueTypes.Custom);
                            PickerRawValueTypes.SelectedItemChanged += PickerRawValueTypesOnSelectedItemChanged;
                        }

                        break;
                    case EnumValueTypes.Bit:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;

                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        if (e == null! || e.CurrentItem == null! || e.CurrentItem.Key != EnumRawValueTypes.Bit)
                        {
                            PickerRawValueTypes.SelectedItemChanged -= PickerRawValueTypesOnSelectedItemChanged;
                            PickerRawValueTypes.SelectKey(EnumRawValueTypes.Bit);
                            PickerRawValueTypes.SelectedItemChanged += PickerRawValueTypesOnSelectedItemChanged;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ConfigBase.Base.RawValueDefinition.RawValueType = PickerRawValueTypes.SelectedItem!.Key;

                switch (PickerRawValueTypes.SelectedItem.Key)
                {
                    case EnumRawValueTypes.Custom:
                    case EnumRawValueTypes.ByteArray:
                        CanEditRawValueByteCount = true;
                        break;
                    case EnumRawValueTypes.Bit:
                    case EnumRawValueTypes.Float:
                    case EnumRawValueTypes.Double:
                    case EnumRawValueTypes.Int16:
                    case EnumRawValueTypes.UInt16:
                    case EnumRawValueTypes.Int32:
                    case EnumRawValueTypes.UInt32:
                    case EnumRawValueTypes.Int64:
                    case EnumRawValueTypes.UInt64:
                    case EnumRawValueTypes.Byte:
                        CanEditRawValueByteCount = false;
                        ConfigBase.Base.RawValueDefinition.ByteCount = ConfigRawValueDefinition.GetByteCount(PickerRawValueTypes.SelectedItem.Key);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PickerRawValueTypes.SelectedItem.Key));
                }
            }
            else
            {
                CanEditRawValueByteCount = false;
                CanValuePickerChanged = false;
                CanEditPickerRawValueTypes = false;
                ShowCanEditRawValueByteCount = true;
                ShowEditPickerRawValueTypes = true;
            }
        }

        private void RawValueDefinitionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ConfigRawValueDefinition d)
            {
                PickerRawValueTypes.SelectKey(d.RawValueType);
            }
        }

        private void PickerPredefinedMeasurementsOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<ExMeasurementDefinition>> e)
        {
            if (e == null || e.CurrentItem == null)
            {
                return;
            }

            Data.DownstreamType = e.CurrentItem.Key.DownstreamType;
            Data.Information = e.CurrentItem.Key.Information;
            Data.ValueType = e.CurrentItem.Key.ValueType;
            Data.AdditionalConfiguration = e.CurrentItem.Key.AdditionalConfiguration;

            InitConfigBase(Data.AdditionalConfiguration);
            InitViewElements();
        }

        private void ConfigBaseOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GcDownstreamBase b)
            {
                Data.AdditionalConfiguration = b.ToExMeasurementDefinition().AdditionalConfiguration;
            }
        }

        /// <summary>
        ///     Downstream Typ geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PickerDownstreamTypeOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumIotDeviceDownstreamTypes>> e)
        {
            if (e != null! && !_plattform.SupportedDownstreamTypes.Contains(e.CurrentItem.Key))
            {
                await MsgBox.Show($"{e.CurrentItem.Description} wird von der Plattform nicht unterstützt.").ConfigureAwait(true);
#pragma warning disable CS0618 // Type or member is obsolete
                PickerDownstreamType.SelectedItemChanged -= PickerDownstreamTypeOnSelectedItemChanged;
                PickerDownstreamType.SelectedItem = e.OldItem;
#pragma warning restore CS0618 // Type or member is obsolete
                PickerDownstreamType.SelectedItemChanged += PickerDownstreamTypeOnSelectedItemChanged;
            }
            else
            {
                InitMeasurements();
                if (_startSelectedExMeasurementDefinition != null)
                {
                    if (PickerPredefinedMeasurements.Count == 0 && e != null)
                    {
                        await MsgBox.Show("Keine freie Hardware bei der Plattform.").ConfigureAwait(true);
                        PickerDownstreamType.SelectedItemChanged -= PickerDownstreamTypeOnSelectedItemChanged;
#pragma warning disable CS0618 // Type or member is obsolete
                        PickerDownstreamType.SelectedItem = e.OldItem;
#pragma warning restore CS0618 // Type or member is obsolete
                        PickerDownstreamType.SelectedItemChanged += PickerDownstreamTypeOnSelectedItemChanged;
                        InitMeasurements();
                    }

                    if (PickerPredefinedMeasurements.Any(a => a.Key == _startSelectedExMeasurementDefinition))
                    {
                        PickerPredefinedMeasurements.SelectKey(_startSelectedExMeasurementDefinition);
                    }
                    else
                    {
                        PickerPredefinedMeasurements.SelectKey(PickerPredefinedMeasurements.FirstOrDefault().Key);
                    }
                }
            }

            Data.DownstreamType = PickerDownstreamType.SelectedItem!.Key;
        }

        private async Task DcExNewValueNotifications_CollectionEvent(object sender, CollectionEventArgs<DcListDataPoint<ExNewValueNotification>> e)
        {
            switch (e.TypeOfEvent)
            {
                case EnumCollectionEventType.AddRequest:
                    await AddNewValueNotification().ConfigureAwait(true);
                    break;
                case EnumCollectionEventType.EditRequest:
                    await EditNewValueNotification(e.Item).ConfigureAwait(true);
                    break;
                case EnumCollectionEventType.DeleteRequest:
                    await DeleteNewValueNotification(e.Item).ConfigureAwait(true);
                    break;
            }
        }

        private async Task AddNewValueNotification()
        {
            var n = new ExNewValueNotification
            {
                MeasurementDefinitionId = Data.Id,
                UserId = Dc.DcExUser.Data.Id
            };
            var dcNewValueNotification = new DcListTypeNewValueNotification(n);

            await EditNewValueNotification(dcNewValueNotification).ConfigureAwait(false);
        }

        private async Task EditNewValueNotification(DcListDataPoint<ExNewValueNotification> dcListDataPoint)
        {
            IsNavigatedToNavToViewWithResult = true;
            // ReSharper disable once UnusedVariable
            var r = await Nav.ToViewWithResult(typeof(VmNewValueNotification), dcListDataPoint).ConfigureAwait(true);
            IsNavigatedToNavToViewWithResult = false;
            AttachDetachEvents(true);
            await View.RefreshAsync().ConfigureAwait(false);
        }

        private async Task DeleteNewValueNotification(DcListDataPoint<ExNewValueNotification> dcListDataPoint)
        {
            var answer = await MsgBox.Show("Wollen Sie diese Benachrichtigung wirklich löschen?", "Löschen", VmMessageBoxButton.YesNo, VmMessageBoxImage.Warning).ConfigureAwait(true);

            if (answer != VmMessageBoxResult.Yes)
            {
                return;
            }

            Dc.DcExNewValueNotifications.Remove(dcListDataPoint);
            var storeResult = await Dc.DcExNewValueNotifications.StoreAll().ConfigureAwait(true);

            if (storeResult.DataOk)
            {
                await MsgBox.Show("Benachrichtigung gelöscht.").ConfigureAwait(true);
            }
            else
            {
                await MsgBox.Show("Benachrichtigung konnte nicht gelöscht werden.").ConfigureAwait(true);
            }
        }

        private void InitMeasurements()
        {
            PickerPredefinedMeasurements.Clear();

            foreach (var d in _plattform.BuildInExMeasurementDefinitions)
            {
                var add = false;
                if (PickerDownstreamType.SelectedItem!.Key == EnumIotDeviceDownstreamTypes.Esp32)
                {
                    var configBase = new GcBaseConverter<GcDownstreamBase>(d.AdditionalConfiguration);
                    var config = configBase.ConvertTo<GcDownstreamEsp32>();
                    var original = _configBaseOriginal.ConvertTo<GcDownstreamEsp32>();
                    if (original.Esp32MeasurementType == config.Esp32MeasurementType)
                    {
                        if (_startSelectedExMeasurementDefinition == null!)
                        {
                            _startSelectedExMeasurementDefinition = d;
                        }

                        add = true;
                    }
                    else
                    {
                        var foundInOther = false;
                        foreach (var md in Dc.DcExMeasurementDefinition.Where(w => w.Data.IotDeviceId == IotDevice.Index && w.Data.DownstreamType == EnumIotDeviceDownstreamTypes.Esp32))
                        {
                            var configBaseOther = new GcBaseConverter<GcDownstreamBase>(md.Data.AdditionalConfiguration);
                            var configOther = configBaseOther.ConvertTo<GcDownstreamEsp32>();

                            //Hardware bereits in anderer Messwertdefinition in Verwendung
                            if (configOther.Esp32MeasurementType == config.Esp32MeasurementType)
                            {
                                foundInOther = true;
                                add = false;
                                break;
                            }
                        }

                        if (!foundInOther)
                        {
                            add = true;
                        }
                    }
                }
                else if (PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.DotNet)
                {
                    var configBase = new GcBaseConverter<GcDownstreamBase>(d.AdditionalConfiguration);
                    var config = configBase.ConvertTo<GcDownstreamDotNet>();

                    var original = _configBaseOriginal.ConvertTo<GcDownstreamDotNet>();
                    if (original.DotNetMeasurementType == config.DotNetMeasurementType)
                    {
                        if (_startSelectedExMeasurementDefinition == null!)
                        {
                            _startSelectedExMeasurementDefinition = d;
                        }

                        add = true;
                    }
                    else
                    {
                        var foundInOther = false;
                        foreach (var md in Dc.DcExMeasurementDefinition.Where(w => w.Data.IotDeviceId == IotDevice.Index && w.Data.DownstreamType == EnumIotDeviceDownstreamTypes.DotNet))
                        {
                            var configBaseOther = new GcBaseConverter<GcDownstreamBase>(md.Data.AdditionalConfiguration);
                            var configOther = configBaseOther.ConvertTo<GcDownstreamDotNet>();

                            //Hardware bereits in anderer Messwertdefinition in Verwendung
                            if (configOther.DotNetMeasurementType == config.DotNetMeasurementType)
                            {
                                foundInOther = true;
                                add = false;
                                break;
                            }
                        }

                        if (!foundInOther)
                        {
                            add = true;
                        }
                    }
                }

                if (add)
                {
                    PickerPredefinedMeasurements.AddKey(d, $"{d.Information.Name}");
                }
            }


            if (PickerDownstreamType.SelectedItem!.Key == EnumIotDeviceDownstreamTypes.Virtual)
            {
                var virtualFloat = new GcDownstreamVirtualFloat().ToExMeasurementDefinition();
                PickerPredefinedMeasurements.AddKey(virtualFloat, $"{virtualFloat.Information.Name}");
                if (_startSelectedExMeasurementDefinition == null!)
                {
                    _startSelectedExMeasurementDefinition = virtualFloat;
                }
            }
            else if (PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.Custom)
            {
                var custom = new GcDownstreamCustom().ToExMeasurementDefinition();
                PickerPredefinedMeasurements.AddKey(custom, $"{custom.Information.Name}");
                if (_startSelectedExMeasurementDefinition == null!)
                {
                    _startSelectedExMeasurementDefinition = custom;
                }
            }
            else if (PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.I2C)
            {
                var i2c = new GcDownstreamI2c().ToExMeasurementDefinition();
                PickerPredefinedMeasurements.AddKey(i2c, $"{i2c.Information.Name}");
                if (_startSelectedExMeasurementDefinition == null!)
                {
                    _startSelectedExMeasurementDefinition = i2c;
                }
            }
            else if (PickerDownstreamType.SelectedItem.Key == EnumIotDeviceDownstreamTypes.Meadow)
            {
                var meadow = new GcDownstreamMeadow().ToExMeasurementDefinition();
                PickerPredefinedMeasurements.AddKey(meadow, $"{meadow.Information.Name}");
                if (_startSelectedExMeasurementDefinition == null!)
                {
                    _startSelectedExMeasurementDefinition = meadow;
                }
            }
        }

        private void InitPlattform()
        {
            switch (IotDevice.Data.Plattform)
            {
                case EnumIotDevicePlattforms.DotNet:
                    _plattform = new GcPlattformDotNet();
                    break;
                case EnumIotDevicePlattforms.RaspberryPi:
                    _plattform = new GcPlatformPi();
                    break;
                case EnumIotDevicePlattforms.Arduino:
                    _plattform = new GcPlatformArduino();
                    break;
                case EnumIotDevicePlattforms.Esp32:
                    _plattform = new GcPlattformEsp32();
                    break;
                case EnumIotDevicePlattforms.Prebuilt:
                    _plattform = new GcPlatformPrebuilt();
                    break;
                case EnumIotDevicePlattforms.OpenSense:
                    _plattform = new GcPlatformOpenSense();
                    break;
                case EnumIotDevicePlattforms.Meadow:
                    _plattform = new GcPlatformMeadow();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var type in _plattform.SupportedDownstreamTypes)
            {
                switch (type)
                {
                    case EnumIotDeviceDownstreamTypes.Virtual:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Virtual, "Anbindung eines virtuellen Sensors");
                        break;
                    case EnumIotDeviceDownstreamTypes.I2C:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.I2C, "Anbindung via I2C Bus");
                        break;
                    case EnumIotDeviceDownstreamTypes.Spi:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Spi, "Anbindung via SPI Bus");
                        break;
                    case EnumIotDeviceDownstreamTypes.Modbus:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Modbus, "Anbindung via Modbus Bus");
                        break;
                    case EnumIotDeviceDownstreamTypes.Pi:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Pi, "Anbindung der \"builtin\" des Pi");
                        break;
                    case EnumIotDeviceDownstreamTypes.Arduino:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Arduino, "Anbindung der \"builtin\" des Arduino");
                        break;
                    case EnumIotDeviceDownstreamTypes.Esp32:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Esp32, "Anbindung der \"builtin\" des Esp32");
                        break;
                    case EnumIotDeviceDownstreamTypes.DotNet:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.DotNet, "Anbindung der \"builtin\" eines PC'S (Mac oder Linux)");
                        break;
                    case EnumIotDeviceDownstreamTypes.Custom:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Custom, "Custom Befehle (max 255) - müssen direkt in Firmware behandelt werden");
                        break;
                    case EnumIotDeviceDownstreamTypes.Prebuilt:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Prebuilt, "Vorgefertigter Sensor. Keine Einstellung möglich.");
                        break;
                    case EnumIotDeviceDownstreamTypes.OpenSense:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.OpenSense, "OpenSense Sensor. Keine Einstellung möglich.");
                        break;
                    case EnumIotDeviceDownstreamTypes.Meadow:
                        PickerDownstreamType.AddKey(EnumIotDeviceDownstreamTypes.Meadow, "Anbindung der \"builtin\" des Meadow");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void InitViewElements()
        {
            var entries = this.GetAllInstancesWithType<VmEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i] = null!;
            }

            EntryName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Name:",
                "Name des Messwerts",
                Data.Information,
                nameof(ExMeasurementDefinition.Information.Name),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDescription = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Beschreibung:",
                "Beschreibung",
                Data.Information,
                nameof(ExMeasurementDefinition.Information.Description),
                showTitle: false
            );

            EntryMeasurmentInterval = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Messinterval (1=100ms):",
                "Messinterval in Zehntel Sekunden (1=100ms)",
                Data,
                nameof(ExMeasurementDefinition.MeasurementInterval),
                VmEntryValidators.ValidateFuncInt,
                showTitle: false
            );

            EntryAdditionalProperties = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Zusätzliche Einstellungen:",
                "Zusätzliche Einstellungen",
                Data,
                nameof(ExMeasurementDefinition.AdditionalProperties),
                showTitle: false
            );

            ShowVirtualFloat = false;
            CanValuePickerChanged = false;
            ShowCustomOpCode = false;
            _customConfigBase = null!;

            if (Data.DownstreamType == EnumIotDeviceDownstreamTypes.Virtual)
            {
                var virt = ConfigBase.ConvertTo<GcDownstreamVirtualFloat>();
                virt.PropertyChanged += ConfigBaseOnPropertyChanged;

                EntryVirtPosLat = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Zufällige Position Latitude:",
                    "GPS Latitude",
                    virt,
                    nameof(GcDownstreamVirtualFloat.AreaLatitude),
                    VmEntryValidators.ValidateFuncDouble,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryVirtPosLon = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Zufällige Position :",
                    "GPS Longitude",
                    virt,
                    nameof(GcDownstreamVirtualFloat.AreaLogitute),
                    VmEntryValidators.ValidateFuncDouble,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryVirtPosRadius = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Zufällige Position Radius (m):",
                    "Zufällige Position Radius (m)",
                    virt,
                    nameof(GcDownstreamVirtualFloat.AreaRadius),
                    VmEntryValidators.ValidateFuncInt,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryVirtFloatMin = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Minimum zufälliger Wert:",
                    "Minimum zufälliger Wert",
                    virt,
                    nameof(GcDownstreamVirtualFloat.Min),
                    VmEntryValidators.ValidateFuncDouble,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryVirtFloatMax = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Maximum zufälliger Wert:",
                    "Maximum zufälliger Wert",
                    virt,
                    nameof(GcDownstreamVirtualFloat.Max),
                    VmEntryValidators.ValidateFuncDouble,
                    showTitle: false,
                    showMaxChar: false
                );

                ShowVirtualFloat = true;
            }
            else if (Data.DownstreamType == EnumIotDeviceDownstreamTypes.Custom)
            {
                CanValuePickerChanged = true;
                ShowCustomOpCode = true;

                _customConfigBase = ConfigBase.ConvertTo<GcDownstreamCustom>();
                _customConfigBase.PropertyChanged += ConfigBaseOnPropertyChanged;
                _customConfigBase.RawValueDefinition.PropertyChanged += (sender, args) =>
                    Data.AdditionalConfiguration = _customConfigBase.ToExMeasurementDefinition().AdditionalConfiguration;

                ConfigBase.Base.RawValueDefinition.PropertyChanged += (sender, args) =>
                {
                    if (_customConfigBase != null)
                    {
                        _customConfigBase.RawValueDefinition.ByteCount = ConfigBase.Base.RawValueDefinition.ByteCount;
                        _customConfigBase.RawValueDefinition.RawValueType = ConfigBase.Base.RawValueDefinition.RawValueType;
                        if (!EntryRawValueByteCount.Value.Equals(_customConfigBase.RawValueDefinition.ByteCount.ToString(), StringComparison.InvariantCulture))
                        {
                            //Debugger.Break();
                            EntryRawValueByteCount = new VmEntry(
                                EnumVmEntryBehavior.StopTyping,
                                "Byte Anzahl:",
                                "Anzahl der Bytes (-1 = unbestimmt)",
                                ConfigBase.Base.RawValueDefinition,
                                nameof(GcDownstreamCustom.RawValueDefinition.ByteCount),
                                VmEntryValidators.ValidateFuncInt,
                                showTitle: false,
                                showMaxChar: false
                            );
                        }
                    }
                };


                EntryRawValueByteCount = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Byte Anzahl:",
                    "Anzahl der Bytes (-1 = unbestimmt)",
                    ConfigBase.Base.RawValueDefinition,
                    nameof(GcDownstreamCustom.RawValueDefinition.ByteCount),
                    VmEntryValidators.ValidateFuncInt,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryCustomOpCode = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Eigene OP-Code Id:",
                    "Byte: 0-255",
                    _customConfigBase,
                    nameof(GcDownstreamCustom.StateMachineId),
                    VmEntryValidators.ValidateFuncByte,
                    showTitle: false,
                    showMaxChar: false
                );

                switch (PickerValueType.SelectedItem!.Key)
                {
                    case EnumValueTypes.Number:
                        CanEditPickerRawValueTypes = true;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        break;
                    case EnumValueTypes.Data:
                    case EnumValueTypes.Image:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = false;
                        break;
                    case EnumValueTypes.Text:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = false;
                        ShowEditPickerRawValueTypes = false;
                        break;
                    case EnumValueTypes.Bit:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (PickerRawValueTypes.SelectedItem!.Key)
                {
                    case EnumRawValueTypes.Custom:
                    case EnumRawValueTypes.ByteArray:
                        CanEditRawValueByteCount = true;
                        break;
                    case EnumRawValueTypes.Bit:
                    case EnumRawValueTypes.Float:
                    case EnumRawValueTypes.Double:
                    case EnumRawValueTypes.Int16:
                    case EnumRawValueTypes.UInt16:
                    case EnumRawValueTypes.Int32:
                    case EnumRawValueTypes.UInt32:
                    case EnumRawValueTypes.Int64:
                    case EnumRawValueTypes.UInt64:
                    case EnumRawValueTypes.Byte:
                        CanEditRawValueByteCount = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PickerRawValueTypes.SelectedItem.Key));
                }
            }
            else if (Data.DownstreamType == EnumIotDeviceDownstreamTypes.Prebuilt)
            {
                CanValuePickerChanged = true;
                _prebuiltConfigBase = ConfigBase.ConvertTo<GcDownstreamPrebuilt>();
                _prebuiltConfigBase.PropertyChanged += ConfigBaseOnPropertyChanged;
                _prebuiltConfigBase.RawValueDefinition.PropertyChanged += (sender, args) =>
                    Data.AdditionalConfiguration = _prebuiltConfigBase.ToExMeasurementDefinition().AdditionalConfiguration;

                ConfigBase.Base.RawValueDefinition.PropertyChanged += (sender, args) =>
                {
                    if (_prebuiltConfigBase != null)
                    {
                        _prebuiltConfigBase.RawValueDefinition.ByteCount = ConfigBase.Base.RawValueDefinition.ByteCount;
                        _prebuiltConfigBase.RawValueDefinition.RawValueType = ConfigBase.Base.RawValueDefinition.RawValueType;
                        if (!EntryRawValueByteCount.Value.Equals(_prebuiltConfigBase.RawValueDefinition.ByteCount.ToString(), StringComparison.InvariantCulture))
                        {
                            //Debugger.Break();
                        }
                    }
                };


                EntryRawValueByteCount = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Byte Anzahl:",
                    "Anzahl der Bytes (-1 = unbestimmt)",
                    ConfigBase.Base.RawValueDefinition,
                    nameof(GcDownstreamCustom.RawValueDefinition.ByteCount),
                    VmEntryValidators.ValidateFuncInt,
                    showTitle: false,
                    showMaxChar: false
                );

                switch (PickerValueType.SelectedItem!.Key)
                {
                    case EnumValueTypes.Number:
                        CanEditPickerRawValueTypes = true;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        break;
                    case EnumValueTypes.Data:
                    case EnumValueTypes.Image:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = false;
                        break;
                    case EnumValueTypes.Text:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = false;
                        ShowEditPickerRawValueTypes = false;
                        break;
                    case EnumValueTypes.Bit:
                        CanEditPickerRawValueTypes = false;
                        CanEditRawValueByteCount = false;
                        ShowCanEditRawValueByteCount = true;
                        ShowEditPickerRawValueTypes = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (PickerRawValueTypes.SelectedItem!.Key)
                {
                    case EnumRawValueTypes.Custom:
                    case EnumRawValueTypes.ByteArray:
                        CanEditRawValueByteCount = true;
                        break;
                    case EnumRawValueTypes.Bit:
                    case EnumRawValueTypes.Float:
                    case EnumRawValueTypes.Double:
                    case EnumRawValueTypes.Int16:
                    case EnumRawValueTypes.UInt16:
                    case EnumRawValueTypes.Int32:
                    case EnumRawValueTypes.UInt32:
                    case EnumRawValueTypes.Int64:
                    case EnumRawValueTypes.UInt64:
                    case EnumRawValueTypes.Byte:
                        CanEditRawValueByteCount = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PickerRawValueTypes.SelectedItem.Key));
                }
            }

            if (ShowThresholds)
            {
                EntryThresholdExceedValue = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Schwellenwert Überschreitung:",
                    "-",
                    validateFunc: VmEntryValidators.ValidateFuncFloatButAlwaysTrue,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryThresholdFallBelowValue = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Schwellenwert Unterschreitung:",
                    "-",
                    validateFunc: VmEntryValidators.ValidateFuncFloatButAlwaysTrue,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryThresholdDeltaValue = new VmEntry(
                    EnumVmEntryBehavior.StopTyping,
                    "Schwellenwert Delta:",
                    "-",
                    validateFunc: VmEntryValidators.ValidateFuncFloatButAlwaysTrue,
                    showTitle: false,
                    showMaxChar: false
                );

                EntryThresholdExceedValue.PropertyChanged += (sender, args) => ThresholdValueChange(((VmEntry) sender).BindingData, EnumThresholdType.Exceed);
                EntryThresholdFallBelowValue.PropertyChanged += (sender, args) => ThresholdValueChange(((VmEntry) sender).BindingData, EnumThresholdType.FallBelow);
                EntryThresholdDeltaValue.PropertyChanged += (sender, args) => ThresholdValueChange(((VmEntry) sender).BindingData, EnumThresholdType.Delta);
            }
        }
    }
}