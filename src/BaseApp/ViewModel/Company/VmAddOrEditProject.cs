// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Dc.Client;
using Biss.Interfaces;
using Exchange.Resources;

namespace BaseApp.ViewModel.Company
{
    #region Hilfsklassen

    /// <summary>
    ///     <para>UI Hilfsklasse</para>
    ///     Klasse UiMeasurements. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UiMeasurements : IBissModel, IBissSelectable
    {
        private bool _isSelected;

        /// <summary>
        ///     UI Hilfsklasse
        /// </summary>
        /// <param name="md"></param>
        /// <param name="gwId"></param>
        /// <param name="isUsedInProject"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UiMeasurements(DcListDataPoint<ExMeasurementDefinition> md, long gwId, bool isUsedInProject)
        {
            if (md == null!)
            {
                throw new ArgumentNullException($"[{nameof(UiMeasurements)}]({nameof(UiMeasurements)}): {nameof(md)}");
            }

            Id = md.Index;
            IotId = md.Data.IotDeviceId;
            GwId = gwId;
            Name = md.Data.Information.Info;
            Details = md.Data.CurrentValue.SourceInfo;
            IsUsedInProject = isUsedInProject;
        }

        #region Properties

        /// <summary>
        ///     Db Id
        /// </summary>
        public long Id { get; } = -1;

        /// <summary>
        ///     Iot Gerät Id für Filter
        /// </summary>
        public long IotId { get; set; }

        /// <summary>
        ///     Gw Gerät Id für Filter
        /// </summary>
        public long GwId { get; set; }

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        ///     Details
        /// </summary>
        public string Details { get; } = string.Empty;

        /// <summary>
        ///     Ausgewählt im Projekt
        /// </summary>
        public bool IsUsedInProject { get; set; }

        /// <summary>Ist das aktuelle Element selektiert</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!CanEnableIsSelect)
                {
                    _isSelected = false;
                }
                else
                {
                    _isSelected = value;

                    Selected?.Invoke(this, new BissSelectableEventArgs(_isSelected));
                }
            }
        }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        #endregion

        #region Interface Implementations

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Ereignis wenn das Element selektiert wurde im Element
        /// </summary>
        public event EventHandler<BissSelectableEventArgs>? Selected;

        #endregion
    }

    #endregion

    /// <summary>
    ///     <para>Neues Projekt anlegen oder bearbeiten</para>
    ///     Klasse VmAddProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddProject", true)]
    public class VmAddOrEditProject : VmEditDcListPoint<ExProject>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddProject.DesignInstance}"
        /// </summary>
        public static VmAddOrEditProject DesignInstance = new VmAddOrEditProject();

        private readonly List<UiMeasurements> _allItems = new List<UiMeasurements>();

        /// <summary>
        ///     VmAddProject
        /// </summary>
        public VmAddOrEditProject() : base("Projekt", subTitle: "Projekt bearbeiten oder erstellen")
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Gateways der Firma
        /// </summary>
        public VmPicker<long> PickerGateways { get; private set; } = new VmPicker<long>(nameof(PickerGateways));

        /// <summary>
        ///     Iot Devices der Gateways der Firma
        /// </summary>
        public VmPicker<long> PickerIotDevices { get; private set; } = new VmPicker<long>(nameof(PickerIotDevices));

        /// <summary>
        ///     Mögliche Messwerte zum Hinzufügen
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public BxObservableCollection<UiMeasurements> ItemsForAdd { get; set; } = new BxObservableCollection<UiMeasurements>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Aktuelle Messwerte des Projekts
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public BxObservableCollection<UiMeasurements> ItemsCurrent { get; set; } = new BxObservableCollection<UiMeasurements>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     Messwert zum Projekt
        /// </summary>
        public VmCommand CmdAddItem { get; private set; } = null!;

        /// <summary>
        ///     Messwert vom Projekt löschen
        /// </summary>
        public VmCommand CmdDeleteItem { get; private set; } = null!;


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
                "Projektname",
                Data.Information,
                nameof(ExProject.Information.Name),
                VmEntryValidators.ValidateFuncStringEmpty
            );

            EntryDescription = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Beschreibung:",
                "Beschreibung",
                Data.Information,
                nameof(ExProject.Information.Description)
            );

            EntryAdditionalProperties = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Zusätzliche Einstellungen:",
                "Zusätzliche Einstellungen",
                Data,
                nameof(ExProject.AdditionalProperties)
            );

            PickerGateways.AddKey(-1, "Alle Gateways");

            foreach (var gw in Dc.DcExGateways.Where(g => g.Data.CompanyId == Data.CompanyId))
            {
                PickerGateways.AddKey(gw.Index, $"{gw.Data.Information.Name} (Id: {gw.Index})");
            }

            PickerIotDevices.AddKey(-1, "Alle Iot Geräte");
            foreach (var iot in Dc.DcExIotDevices.Where(g => g.Data.CompanyId == Data.CompanyId))
            {
                PickerIotDevices.AddKey(iot.Index, $"{iot.Data.Information.Name} (Id: {iot.Index})");
                foreach (var md in Dc.DcExMeasurementDefinition.Where(w => w.Data.IotDeviceId == iot.Index))
                {
                    _allItems.Add(new UiMeasurements(md, iot.Data.GatewayId!.Value, Data.MeasurementDefinitions.Contains(md.Index)));
                }
            }

            ItemsForAdd = new BxObservableCollection<UiMeasurements>(_allItems);
            ItemsCurrent = new BxObservableCollection<UiMeasurements>(_allItems.Where(i => i.IsUsedInProject));


            PickerGateways.SelectedItem = PickerGateways.First();
            PickerIotDevices.SelectedItem = PickerIotDevices.First();

            UpdateFilter();

            PickerGateways.SelectedItemChanged += (sender, eventArgs) => UpdateFilter();
            PickerIotDevices.SelectedItemChanged += (sender, eventArgs) => UpdateFilter();

            return r;
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CmdAddItem = new VmCommand("", item =>
            {
                if (item is UiMeasurements m)
                {
                    m.IsUsedInProject = true;
                    ItemsCurrent.Add(m);
                    Data.MeasurementDefinitions.Add(m.Id);
                    View.CmdSaveHeader!.CanExecute();

                    UpdateFilter();
                }
                else
                {
                    throw new ArgumentException();
                }
            }, glyph: Glyphs.Add_circle_1);
            CmdDeleteItem = new VmCommand("", item =>
            {
                if (item is UiMeasurements m)
                {
                    m.IsUsedInProject = false;
                    ItemsCurrent.Remove(m);
                    Data.MeasurementDefinitions.Remove(m.Id);
                    View.CmdSaveHeader!.CanExecute();

                    UpdateFilter();
                }
                else
                {
                    throw new ArgumentException();
                }
            }, glyph: Glyphs.Bin_2);
        }

        /// <summary>
        ///     Filter aktualisieren
        /// </summary>
        private void UpdateFilter()
        {
            if (PickerIotDevices.SelectedItem!.Key > 0)
            {
                ItemsForAdd.FilterList(f => f.IotId == PickerIotDevices.SelectedItem.Key && !f.IsUsedInProject);
            }
            else if (PickerGateways.SelectedItem!.Key > 0)
            {
                ItemsForAdd.FilterList(f => f.GwId == PickerGateways.SelectedItem.Key && !f.IsUsedInProject);
            }
            else
            {
                ItemsForAdd.FilterList(f => !f.IsUsedInProject);
            }

            ItemsForAdd.Sort((measurements, uiMeasurements) => measurements.Id.CompareTo(uiMeasurements.Id));
        }
    }
}