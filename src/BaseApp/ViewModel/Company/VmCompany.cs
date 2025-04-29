// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Company
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse VmCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewCompany", true)]
    public class VmCompany : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmCompany.DesignInstance}"
        /// </summary>
        public static VmCompany DesignInstance = new VmCompany();

        private bool _noUiListUpdate;

        /// <summary>
        ///     VmCompany
        /// </summary>
        public VmCompany() : base("Firmendaten", subTitle: "Projekte, globale Firmeneinstellungen und Firmenbenutzer")
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     UI Liste für Configs einer Firma
        /// </summary>
        public BxObservableCollection<DcListDataPoint<ExGlobalConfig>> UiGlobalConfig { get; private set; } = new BxObservableCollection<DcListDataPoint<ExGlobalConfig>>();

        /// <summary>
        ///     UI Liste für Projekte einer Firma
        /// </summary>
        public BxObservableCollection<DcListTypeProject> UiProjects { get; private set; } = new BxObservableCollection<DcListTypeProject>();

        /// <summary>
        ///     UI Liste für Projekte einer Firma
        /// </summary>
        public BxObservableCollection<DcListDataPoint<ExCompanyUser>> UiUsers { get; private set; } = new BxObservableCollection<DcListDataPoint<ExCompanyUser>>();


        /// <summary>
        ///     Globale Config anlegen oder bearbeiten
        /// </summary>
        public VmCommand CmdEditGlobalConfig { get; private set; } = null!;

        /// <summary>
        ///     Global Config löschen
        /// </summary>
        public VmCommand CmdDeleteGlobalConfig { get; private set; } = null!;

        /// <summary>
        ///     Neue Globale Config
        /// </summary>
        public VmCommand CmdAddGlobalConfig { get; private set; } = null!;

        /// <summary>
        ///     Projekt hinzufügen
        /// </summary>
        public VmCommand CmdAddProject { get; private set; } = null!;

        /// <summary>
        ///     Projekt löschen
        /// </summary>
        public VmCommand CmdDeleteProject { get; private set; } = null!;

        /// <summary>
        ///     Bestehenden User bearbeiten
        /// </summary>
        public VmCommand CmdEditUser { get; private set; } = null!;

        /// <summary>
        ///     User hinzufügen
        /// </summary>
        public VmCommand CmdAddUser { get; private set; } = null!;

        /// <summary>
        ///     User löschen
        /// </summary>
        public VmCommand CmdDeleteUser { get; private set; } = null!;

        /// <summary>
        ///     Bestehendes Projekt bearbeiten
        /// </summary>
        public VmCommand CmdEditProject { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            if (!_noUiListUpdate && Dc.DcExCompanies.SelectedItem != null!)
            {
                UiGlobalConfig = new BxObservableCollection<DcListDataPoint<ExGlobalConfig>>(Dc.DcExGlobalConfig.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem.Index));
                UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem.Index));
                UiUsers = new BxObservableCollection<DcListDataPoint<ExCompanyUser>>(Dc.DcExCompanyUsers.Where(w => w.Data != null! && (w.Data.CompanyId == Dc.DcExCompanies.SelectedItem.Index || w.Data.IsSuperadmin)));
            }

            if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
            {
                if (ShowMenuPressed != null!)
                {
                    _ = ShowMenuPressed(null!);
                }
            }

            AttachVmEvents();

            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachVmEvents(true);
            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdAddProject = new VmCommand("", async () =>
            {
                // ReSharper disable once VariableCanBeNotNullable
                // ReSharper disable once RedundantAssignment
                DcListTypeProject? item = null!;
                _noUiListUpdate = true;

                var newProject = new ExProject
                {
                    CompanyId = Dc.DcExCompanies.SelectedItem!.Index,
                    IsPublic = Dc.DcExCompanies.SelectedItem.Data.CompanyType == EnumCompanyTypes.PublicCompany,
                    Published = true,
                    PublishedDate = DateTime.UtcNow,
                    Information =
                    {
                        CreatedDate = DateTime.UtcNow,
                        Name = ""
                    }
                };
                item = new DcListTypeProject(newProject);
                Dc.DcExProjects.Add(item);
                var r = await Nav.ToViewWithResult(typeof(VmAddOrEditProject), item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            Dc.DcExProjects.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                    else
                    {
                        UiProjects.Add(item);
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Add_circle_bold);

            CmdEditProject = new VmCommand("", async project =>
            {
                if (project == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(project)}");
                }

                _noUiListUpdate = true;
                var r = await Nav.ToViewWithResult(typeof(VmAddOrEditProject), project).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        var p = (DcListDataPoint<ExProject>) project;
                        if (p.PossibleNewDataOnServer)
                        {
                            p.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Notes_edit);

            CmdDeleteProject = new VmCommand("", async project =>
            {
                if (project == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(project)}");
                }

                if (project is DcListTypeProject item)
                {
                    // Workaround bis fix in Biss.Collections
                    try
                    {
                        Dc.DcExProjects.Remove(item);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }

                    var r = await Dc.DcExGlobalConfig.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        await MsgBox.Show("Löchen leider nicht möglich!").ConfigureAwait(true);
                        Dc.DcExProjects.Add(item);
                    }
                    else
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            UiProjects.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                }
            }, glyph: Glyphs.Bin_2);


            CmdAddGlobalConfig = new VmCommand("", async () =>
            {
                // ReSharper disable once RedundantAssignment
                DcListDataPoint<ExGlobalConfig>? item = null!;
                _noUiListUpdate = true;

                var newGc = new ExGlobalConfig
                {
                    CompanyId = Dc.DcExCompanies.SelectedItem!.Index,
                    ConfigType = EnumGlobalConfigTypes.Ttn,
                    ConfigVersion = 1,
                    Information =
                    {
                        CreatedDate = DateTime.UtcNow,
                        Name = ""
                    }
                };
                item = new DcListDataPoint<ExGlobalConfig>(newGc);
                Dc.DcExGlobalConfig.Add(item);
                newGc.Id = item.Index;

                var r = await Nav.ToViewWithResult(typeof(VmEditGlobalConfig), item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            Dc.DcExGlobalConfig.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                    else
                    {
                        UiGlobalConfig.Add(item);
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Add_circle_bold);

            CmdEditGlobalConfig = new VmCommand("", async gc =>
            {
                if (gc == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(gc)}");
                }

                _noUiListUpdate = true;
                var r = await Nav.ToViewWithResult(typeof(VmEditGlobalConfig), gc).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        var p = (DcListDataPoint<ExGlobalConfig>) gc;
                        if (p.PossibleNewDataOnServer)
                        {
                            p.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Notes_edit);

            CmdDeleteGlobalConfig = new VmCommand("", async gc =>
            {
                if (gc == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(gc)}");
                }

                if (gc is DcListDataPoint<ExGlobalConfig> item)
                {
                    if (item.Data.IsUsedInIotDevice)
                    {
                        await MsgBox.Show("Diese Konfiguration kann nicht gelöscht werden da diese in Verwendung ist.").ConfigureAwait(true);
                        return;
                    }

                    // Workaround bis fix in Biss.Collections
                    try
                    {
                        Dc.DcExGlobalConfig.Remove(item);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }

                    var r = await Dc.DcExGlobalConfig.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        await MsgBox.Show("Löchen leider nicht möglich!").ConfigureAwait(true);
                        Dc.DcExGlobalConfig.Add(item);
                    }
                    else
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            UiGlobalConfig.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                }
            }, glyph: Glyphs.Bin_2);


            CmdAddUser = new VmCommand("", async () =>
            {
                // ReSharper disable once RedundantAssignment
                DcListDataPoint<ExCompanyUser>? item = null!;
                _noUiListUpdate = true;

                var newUser = new ExCompanyUser
                {
                    CompanyId = Dc.DcExCompanies.SelectedItem!.Index,
                    UserRight = EnumUserRight.Read,
                    UserRole = EnumUserRole.User
                };
                item = new DcListDataPoint<ExCompanyUser>(newUser);
                Dc.DcExCompanyUsers.Add(item);
                var r = await Nav.ToViewWithResult(typeof(VmAddUser), item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            Dc.DcExCompanyUsers.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                    else
                    {
                        UiUsers.Add(item);
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Add_circle_bold);

            CmdEditUser = new VmCommand("", async user =>
            {
                if (user == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(user)}");
                }

                _noUiListUpdate = true;
                var r = await Nav.ToViewWithResult(typeof(VmAddUser), user).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        var p = (DcListDataPoint<ExCompanyUser>) user;
                        if (p.PossibleNewDataOnServer)
                        {
                            p.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                _noUiListUpdate = false;
            }, glyph: Glyphs.Notes_edit);

            CmdDeleteUser = new VmCommand("", async user =>
            {
                if (user == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(user)}");
                }

                if (user is DcListDataPoint<ExCompanyUser> item)
                {
                    // Workaround bis fix in Biss.Collections
                    try
                    {
                        Dc.DcExCompanyUsers.Remove(item);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }

                    var r = await Dc.DcExCompanyUsers.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        await MsgBox.Show("Löchen leider nicht möglich!").ConfigureAwait(true);
                        Dc.DcExCompanyUsers.Add(item);
                    }
                    else
                    {
                        // Workaround bis fix in Biss.Collections
                        try
                        {
                            UiUsers.Remove(item);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"{e}");
                        }
                    }
                }
            }, glyph: Glyphs.Bin_2);
        }

        private void AttachVmEvents(bool detach = false)
        {
            if (detach)
            {
                Dc.DcExGlobalConfig.LoadingFromHostEvent -= DcExGlobalConfigOnLoadingFromHostEvent;
                Dc.DcExProjects.LoadingFromHostEvent -= DcExProjectsOnLoadingFromHostEvent;
                Dc.DcExCompanyUsers.LoadingFromHostEvent -= DcCompanyUsersOnLoadingFromHostEvent;
            }
            else
            {
                Dc.DcExGlobalConfig.LoadingFromHostEvent += DcExGlobalConfigOnLoadingFromHostEvent;
                Dc.DcExProjects.LoadingFromHostEvent += DcExProjectsOnLoadingFromHostEvent;
                Dc.DcExCompanyUsers.LoadingFromHostEvent += DcCompanyUsersOnLoadingFromHostEvent;
            }
        }

        private void DcExGlobalConfigOnLoadingFromHostEvent(object sender, bool e)
        {
            Dispatcher!.RunOnDispatcher(() => { UiGlobalConfig = new BxObservableCollection<DcListDataPoint<ExGlobalConfig>>(Dc.DcExGlobalConfig.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem!.Index)); });
        }

        private void DcExProjectsOnLoadingFromHostEvent(object sender, bool e)
        {
            Dispatcher!.RunOnDispatcher(() => { UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem!.Index)); });
        }

        private void DcCompanyUsersOnLoadingFromHostEvent(object sender, bool e)
        {
            Dispatcher!.RunOnDispatcher(() => { UiUsers = new BxObservableCollection<DcListDataPoint<ExCompanyUser>>(Dc.DcExCompanyUsers.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem!.Index || w.Data.IsSuperadmin)); });
        }
    }
}