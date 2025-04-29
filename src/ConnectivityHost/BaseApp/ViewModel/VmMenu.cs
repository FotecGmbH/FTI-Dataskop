// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using Biss.Apps.Collections;
using Biss.Apps.ViewModel;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>VmMenu</para>
    ///     Klasse VmMenu. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class VmMenu : VmProjectBase
    {
        /// <summary>
        ///     ViewModel Template
        /// </summary>
        public VmMenu() : base(string.Empty)
        {
            CurrentVmMenu = this;
        }

        #region Properties

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMenu.DesignInstance}"
        /// </summary>
        public static VmMenu DesignInstance => new VmMenu();

        /// <summary>
        ///     Alle Menüeinträge für seitliches Menü
        /// </summary>
        public BxObservableCollection<VmCommandSelectable> CmdAllMenuCommands { get; } = new BxObservableCollection<VmCommandSelectable>();

        #endregion

        /// <summary>
        ///     Menüeinträge aktualisieren
        /// </summary>
        public void UpdateMenu()
        {
            CmdAllMenuCommands.Clear();
            CmdAllMenuCommands.Add(GCmdHome);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdAllMenuCommands.Add(GCmdHome);
            CmdAllMenuCommands.Add(GCmdUsers);
            CmdAllMenuCommands.Add(GCmdDevices);
            CmdAllMenuCommands.Add(GCmdDcOverview);
            CmdAllMenuCommands.Add(GCmdSettings);
        }
    }
}