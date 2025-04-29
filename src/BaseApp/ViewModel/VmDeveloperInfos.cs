// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Collections;
using Biss.Apps.Components;
using Biss.Apps.ViewModel;
using Biss.Dc.Core;
using Exchange;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Infos für Entwickler direkt in der laufenden App</para>
    ///     Klasse VmDeveloperInfos. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewDeveloperInfos")]
    public class VmDeveloperInfos : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstance}"
        /// </summary>
        public static VmDeveloperInfos DesignInstance = new VmDeveloperInfos();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceComponentDevInfo}"
        /// </summary>
        public static ComponentDevInfo DesignInstanceComponentDevInfo = new ComponentDevInfo();

        /// <summary>
        ///     VmDeveloperInfos
        /// </summary>
        public VmDeveloperInfos() : base("DEV Infos", subTitle: "Aktuelle Informationen für Entwickler")
        {
            SetViewProperties();
            DbSettingsLoaded = Dc.DcExSettingsInDb.DataSource == EnumDcDataSource.FromServer;
        }

        #region Properties

        /// <summary>
        ///     Letzten (max. 100) Logeinträge
        /// </summary>
        public BxObservableCollection<BissEventsLoggerData> Log => LogEntries;

        /// <summary>
        ///     Dev Infos
        /// </summary>
        public ObservableCollection<ComponentDevInfo> ComponentsDevInfos { get; private set; } = new ObservableCollection<ComponentDevInfo>();

        /// <summary>
        ///     App Settings
        /// </summary>
        public AppSettings CurrentSettings => AppSettings.Current();

        /// <summary>
        ///     Device Info
        /// </summary>
        public BissDeviceInfo Device => DeviceInfo;

        /// <summary>
        ///     Settings vom Server geladen
        /// </summary>
        public bool DbSettingsLoaded { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        ///     View wurde erzeugt und geladen - Aber noch nicht sichtbar (gerendert)
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            ComponentsDevInfos = new ObservableCollection<ComponentDevInfo>(await CManager!.GetDeveloperInfos().ConfigureAwait(true));
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CmdDcDisconnect = new VmCommand("Trennen", () => { Dc.CloseConnection(true); });

            CmdDcConnect = new VmCommand("Verbinden", async () => { await Dc.OpenConnection(true).ConfigureAwait(false); });
        }

        /// <summary>
        ///     Verbindung aufbauen
        /// </summary>
        public VmCommand CmdDcConnect { get; set; } = null!;

        /// <summary>
        ///     DC Verbindung trennen
        /// </summary>
        public VmCommand CmdDcDisconnect { get; set; } = null!;

        #endregion
    }
}