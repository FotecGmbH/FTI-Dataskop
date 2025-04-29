// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
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
        public BxObservableCollection<VmCommandSelectable> CmdAllMenuCommands { get; } = new BxObservableCollection<VmCommandSelectable> {SelectionMode = EnumSelectionMode.Single};

        /// <summary>
        ///     Busy
        /// </summary>
        public bool MenuBusy { get; private set; }

        #endregion

        /// <summary>
        ///     Menü beschäftigt
        /// </summary>
        /// <param name="busy"></param>
        public void SetMenuBusy(bool busy)
        {
            MenuBusy = busy;
        }

        /// <summary>
        ///     Menüeinträge aktualisieren
        /// </summary>
        public void UpdateMenu()
        {
            VmCommandSelectable selected = null!;

            if (CmdAllMenuCommands.Any() && CmdAllMenuCommands.SelectedItem != null!)
            {
                selected = CmdAllMenuCommands.SelectedItem;
            }

            CmdAllMenuCommands.Clear();
            //CmdAllMenuCommands.SelectedItem = null!;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            if (Dc?.CoreConnectionInfos == null! ||
                !Dc.DeviceAndUserRegisteredLocal)
            {
                AddDefault();
            }
            else
            {
                CmdAllMenuCommands.Add(GCmdHome);

                CmdAllMenuCommands.Add(GCmdInfrastructure);
                CmdAllMenuCommands.Add(View.GCmdUser);

                if (DeviceInfo.Plattform != EnumPlattform.Web)
                {
                    CmdAllMenuCommands.Add(GCmdSettings);
                }

                CmdAllMenuCommands.Add(GCmdInfos);

                if (ShowDeveloperInfos)
                {
                    CmdAllMenuCommands.Add(GCmdDeveloperInfos);
                }
            }

            if (selected != null!)
            {
                if (CmdAllMenuCommands.Contains(selected))
                {
                    CmdAllMenuCommands.SelectedItem = selected;
                }
                else
                {
                    selected.IsSelected = false;
                    GCmdHome.IsSelected = true;
                }
            }
        }

        /// <summary>
        ///     "Default" Button im Footer im Phone Mode
        /// </summary>
        public void UpdateFooterButton()
        {
            View.GCmdFooter1 = GCmdHome;
            View.GCmdFooter2 = null!;
            View.GCmdFooter3 = GCmdInfos;
            View.GCmdFooter4 = null!;
            View.GCmdFooter5 = GCmdMore;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            try
            {
                InvokeDispatcher(() => CurrentVmMenu?.UpdateMenu());
                UpdateFooterButton();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{GetType().Name}]({nameof(InitializeCommands)}): {e}");
            }
        }

        /// <summary>
        ///     "Default" Einträge (wenn User noch nicht eingeloggt ist) laden
        /// </summary>
        private void AddDefault()
        {
            CmdAllMenuCommands.Add(GCmdHome);

            CmdAllMenuCommands.Add(GCmdLogin);

            if (DeviceInfo.Plattform != EnumPlattform.Web)
            {
                CmdAllMenuCommands.Add(GCmdSettings);
            }

            CmdAllMenuCommands.Add(GCmdInfos);

            if (ShowDeveloperInfos)
            {
                CmdAllMenuCommands.Add(GCmdDeveloperInfos);
            }
        }
    }
}