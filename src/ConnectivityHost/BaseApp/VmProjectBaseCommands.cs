// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
using Biss.Apps.ViewModel;
using ConnectivityHost.BaseApp.ViewModel;
using Exchange.Resources;

namespace ConnectivityHost.BaseApp
{
    /// <summary>
    ///     <para>Commands für alle Views und das Menü</para>
    ///     Klasse VmProjectBaseCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        private static VmCommandSelectable _gcmdMore = null!;
        private static VmCommandSelectable _gcmdHome = null!;
        private static VmCommandSelectable _gcmdDevices = null!;
        private static VmCommandSelectable _gcmdUsers = null!;
        private static VmCommandSelectable _gcmdDcOverview = null!;
        private static VmCommandSelectable _gcmdSettings = null!;

        /// <summary>
        ///     Projektbeogene, globale VmCommands(Selectable) initialisieren
        /// </summary>
        protected override bool InitializeProjectBaseCommands()
        {
            _gcmdMore = new VmCommandSelectable("mehr", () =>
            {
                GCmdShowMenu.Execute(null!);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(250).ConfigureAwait(false);
                    _gcmdMore.IsSelected = false;
                }).ConfigureAwait(false);
            }, "Mehr Infos", Glyphs.Navigation_menu_horizontal);

            _gcmdHome = new VmCommandSelectable("Home", () => { Nav.ToView(typeof(VmMain), showMenu: true, cachePage: false); }, "ToolTip", Glyphs.House_1);

            _gcmdDevices = new VmCommandSelectable("Geräte", () => { Nav.ToView(typeof(VmDevices), showMenu: true, cachePage: false); }, "Geräte", Glyphs.Mobile_phone);
            _gcmdUsers = new VmCommandSelectable("Benutzer", () => { Nav.ToView(typeof(VmUsers), showMenu: true, cachePage: false); }, "Benutzer", Glyphs.Messaging_msn_messenger);

            _gcmdDcOverview = new VmCommandSelectable("Statistik", () => { Nav.ToView(typeof(VmStatistics), showMenu: true, cachePage: false); }, "Statistik", Glyphs.Statistics_daytum);
            _gcmdSettings = new VmCommandSelectable("Einstellungen", () => { Nav.ToView(typeof(VmSettings), showMenu: true, cachePage: false); }, "Einstellungen", Glyphs.Cog_1);
            return true;
        }

#pragma warning disable 1591
        public VmCommandSelectable GCmdMore => _gcmdMore;
        public VmCommandSelectable GCmdHome => _gcmdHome;
        public VmCommandSelectable GCmdDevices => _gcmdDevices;
        public VmCommandSelectable GCmdUsers => _gcmdUsers;
        public VmCommandSelectable GCmdDcOverview => _gcmdDcOverview;
        public VmCommandSelectable GCmdSettings => _gcmdSettings;
#pragma warning restore 1591
    }
}