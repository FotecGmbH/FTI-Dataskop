// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;

namespace BaseApp.ViewModel.Infrastructure
{
    /// <summary>
    ///     <para>Gateway Stammdaten bearbeiten</para>
    ///     Klasse VmEditGateway. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditGateway", true)]
    public class VmEditGateway : VmEditDcListPoint<ExGateway>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditGateway.DesignInstance}"
        /// </summary>
        public static VmEditGateway DesignInstance = new VmEditGateway();

        /// <summary>
        ///     VmEditGateway
        /// </summary>
        public VmEditGateway() : base("Gateway", subTitle: "Gateway bearbeiten")
        {
            PickerPositionType.AddKey(EnumPositionSource.Pc, "Betriebssystem/Manuelle Eingabe");
            PickerPositionType.AddKey(EnumPositionSource.Internet, "Über Internet");
            PickerPositionType.AddKey(EnumPositionSource.Modul, "Von GPS Modul");
            PickerPositionType.AddKey(EnumPositionSource.Lbs, "Über Mobilfunk");
        }

        #region Properties

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

        #endregion

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var t = base.OnActivated(args);

            EntryName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Name:",
                "Name des Gateway",
                Data.Information,
                nameof(ExGateway.Information.Name),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );

            EntryDescription = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Beschreibung:",
                "Beschreibung",
                Data.Information,
                nameof(ExGateway.Information.Description),
                showTitle: false
            );

            EntryPosLat = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Latitude:",
                "GPS Latitude",
                Data.Location,
                nameof(ExGateway.Location.Latitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );

            EntryPosLon = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Longitude:",
                "GPS Longitude",
                Data.Location,
                nameof(ExGateway.Location.Longitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );

            EntryPosAlt = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Altitude:",
                "GPS Altitude",
                Data.Location,
                nameof(ExGateway.Location.Altitude),
                VmEntryValidators.ValidateFuncDouble,
                showTitle: false,
                showMaxChar: false
            );

            EntryAdditionalConfiguration = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Dynamische Konfiguration:",
                "Dynamische Konfiguration",
                Data,
                nameof(ExGateway.AdditionalConfiguration),
                showTitle: false
            );

            EntryAdditionalProperties = new VmEntry(
                EnumVmEntryBehavior.StopTyping,
                "Zusätzliche Einstellungen:",
                "Zusätzliche Einstellungen",
                Data,
                nameof(ExGateway.AdditionalProperties),
                showTitle: false
            );

            PickerPositionType.SelectedItem = PickerPositionType.First(f => f.Key == Data.Location.Source);
            PickerPositionType.SelectedItemChanged += (sender, eventArgs) => Data.Location.Source = eventArgs.CurrentItem.Key;

            return t;
        }
    }
}