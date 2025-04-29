// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Threading.Tasks;
using BaseApp.ViewModel;
using BaseApp.ViewModel.Infrastructure;
using Biss.Apps.ViewModel;
using Exchange.Resources;

namespace BaseApp
{
    /// <summary>
    ///     <para>Commands für alle Views und das Menü</para>
    ///     Klasse VmProjectBaseCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        private static VmCommandSelectable _gcmdMore = null!;
        private static VmCommandSelectable _gcmdHeader = null!;
        private static VmCommandSelectable _gcmdHome = null!;
        private static VmCommandSelectable _gcmdLogin = null!;
        private static VmCommandSelectable _gcmdSettings = null!;
        private static VmCommandSelectable _gcmdInfrastructure = null!;
        private static VmCommandSelectable _gcmdDeveloperInfos = null!;
        private static VmCommandSelectable _gcmdInfos = null!;

        /// <summary>
        ///     Projektbeogene, globale VmCommands(Selectable) initialisieren
        /// </summary>
        protected override bool InitializeProjectBaseCommands()
        {
            _gcmdMore = new VmCommandSelectable(ResCommon.CmdMore, () =>
            {
                GCmdShowMenu.Execute(null!);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(250).ConfigureAwait(false);
                    _gcmdMore.IsSelected = false;
                }).ConfigureAwait(false);
            }, ResCommon.CmdMoreToolTip, Glyphs.Navigation_menu_horizontal);

            _gcmdHeader = new VmCommandSelectable(string.Empty, async () => { await MsgBox.Show(ResCommon.MsgHeaderInfos, ResCommon.MsgTitleHeaderInfos).ConfigureAwait(true); });

            _gcmdHome = new VmCommandSelectable(ResCommon.CmdHome, () => { Nav.ToView(typeof(VmMain), showMenu: true, cachePage: true); }, glyph: Glyphs.House_chimney_2);

            _gcmdLogin = new VmCommandSelectable(ResCommon.CmdLogin, () => { Nav.ToView(typeof(VmLogin), showMenu: true, cachePage: false); }, glyph: Glyphs.Monitor_upload);

            VmViewProperties.SetGcmdUserCommand(new VmCommandSelectable(ResCommon.CmdUser, () =>
            {
                if (!Dc.DeviceAndUserRegisteredLocal)
                {
                    _gcmdLogin.Execute(null!);
                }
                else
                {
                    Nav.ToView(typeof(VmUser), showMenu: true, cachePage: false);
                }
            }, glyph: Glyphs.Single_man));

            _gcmdSettings = new VmCommandSelectable(ResCommon.CmdSettings, () => { Nav.ToView(typeof(VmSettings), showMenu: true, cachePage: true); }, glyph: Glyphs.Cog);

            _gcmdInfrastructure = new VmCommandSelectable("Infrastruktur",
                () => { Nav.ToView(typeof(VmInfrastructure), showMenu: true, cachePage: true); },
                glyph: Glyphs.Hierarchy_9);

            _gcmdDeveloperInfos = new VmCommandSelectable("DEV Infos", () => { Nav.ToView(typeof(VmDeveloperInfos), showMenu: true, cachePage: true); }, glyph: Glyphs.Computer_bug);

            _gcmdInfos = new VmCommandSelectable(ResCommon.CmdInfo, () => { Nav.ToView(typeof(VmInfo), showMenu: true, cachePage: true); }, glyph: Glyphs.Information_circle);

            return true;
        }

#pragma warning disable 1591
        public VmCommandSelectable GCmdHeader => _gcmdHeader;
        public VmCommandSelectable GCmdMore => _gcmdMore;
        public VmCommandSelectable GCmdHome => _gcmdHome;
        public VmCommandSelectable GCmdLogin => _gcmdLogin;
        public VmCommandSelectable GCmdSettings => _gcmdSettings;
        public VmCommandSelectable GCmdInfrastructure => _gcmdInfrastructure;
        public VmCommandSelectable GCmdDeveloperInfos => _gcmdDeveloperInfos;
        public VmCommandSelectable GCmdInfos => _gcmdInfos;
#pragma warning restore 1591
    }
}