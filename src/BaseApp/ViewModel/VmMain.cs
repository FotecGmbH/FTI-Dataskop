// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.ViewModel.Company;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Dc.Client;
using Exchange.Resources;
using PropertyChanged;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Hauptansicht - Projekte und Messwerte</para>
    ///     Klasse ViewModelUserAccount. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewMain", true)]
    public class VmMain : VmProjectBase
    {
        private bool _firstTimeViewLoaded = true;
        private DcListDataPoint<ExMeasurementDefinition> _selectedMeasurementDefinition = null!;
        private DcListDataPoint<ExProject> _selectedProject = null!;

        /// <summary>
        ///     ViewModel Template
        /// </summary>
        public VmMain() : base(ResViewMain.LblTitle, subTitle: ResViewMain.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEntry.DesignInstance}"
        /// </summary>
        public static VmMain DesignInstance => new VmMain();

        /// <summary>
        ///     Firma bearbeiten
        /// </summary>
        public VmCommand CmdEditCompanies { get; private set; } = null!;

        /// <summary>
        ///     Workaround weil Selected Item für diesen Zweck nicht richtig funkioniert
        /// </summary>
        public VmCommand CmdSelectListItem { get; private set; } = null!;

        /// <summary>
        ///     UI Liste für Projekte einer Firma
        /// </summary>
        public BxObservableCollection<DcListTypeProject> UiProjects { get; private set; } = null!;

        /// <summary>
        ///     UI Liste für Projekte einer Firma
        /// </summary>
        public BxObservableCollection<DcListTypeMeasurementDefinition> UiMeasurement { get; private set; } = null!;


        /// <summary>
        ///     MeasurementDefinition im Ui ausgewählt
        /// </summary>
        [DoNotCheckEquality]
        public DcListDataPoint<ExMeasurementDefinition> SelectedMeasurementDefinition
        {
            get => _selectedMeasurementDefinition;
            set
            {
                if (_selectedMeasurementDefinition != null! && _selectedMeasurementDefinition == value)
                {
                    _selectedMeasurementDefinition.Data.IsSelected = !_selectedMeasurementDefinition.Data.IsSelected;
                }
                else
                {
                    _selectedMeasurementDefinition = value;
                    if (_selectedMeasurementDefinition != null!)
                    {
                        _selectedMeasurementDefinition.Data.IsSelected = true;
                    }
                }

                if (UiMeasurement != null!)
                {
                    foreach (var pro in UiMeasurement)
                    {
                        if (pro == _selectedMeasurementDefinition)
                        {
                            continue;
                        }

                        pro.Data.IsSelected = false;
                    }
                }

                View.Refresh();
            }
        }

        /// <summary>
        ///     Details zum Messwert sichtbar
        /// </summary>
        public bool ShowDetails => SelectedMeasurementDefinition != null!;

        /// <summary>
        ///     Aktuell ausgewähltes (oder nicht) Projekt
        /// </summary>
        [DoNotCheckEquality]
        public DcListDataPoint<ExProject> SelectedProject
        {
            get => _selectedProject;
            private set
            {
                if (_selectedProject != null! && _selectedProject == value)
                {
                    _selectedProject.Data.IsSelected = !_selectedProject.Data.IsSelected;
                }
                else
                {
                    _selectedProject = value;
                    if (_selectedProject != null!)
                    {
                        _selectedProject.Data.IsSelected = true;
                    }
                }

                if (UiProjects != null!)
                {
                    foreach (var pro in UiProjects)
                    {
                        if (pro == _selectedProject)
                        {
                            continue;
                        }

                        pro.Data.IsSelected = false;
                    }
                }

                // ReSharper disable once RedundantAssignment
                List<long> tmpList = null!;
                if (_selectedProject == null! || !_selectedProject.Data.IsSelected)
                {
                    tmpList = Dc.DcExMeasurementDefinition.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem?.Index).Select(s => s.Index).ToList();

                    if (Dc.DcExCompanies.SelectedItem != null!)
                    {
                        InfoSelectedMeasurements = $"Aus Firma {Dc.DcExCompanies.SelectedItem.Data.Information.Name}";
                    }
                }
                else
                {
                    tmpList = _selectedProject.Data.MeasurementDefinitions;
                    InfoSelectedMeasurements = $"Aus Projekt {_selectedProject.Data.Information.Name}";
                }

                SelectedMeasurementDefinition = null!;
                UiMeasurement = new BxObservableCollection<DcListTypeMeasurementDefinition>(Dc.DcExMeasurementDefinition.Where(w => tmpList.Contains(w.Index)));
            }
        }


        /// <summary>
        ///     Info Text für Ui
        /// </summary>
        public string InfoSelectedMeasurements { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachDetachVmEvents(false);
            View.BusyClear();
            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            if (!ProjectDataLoadedAfterDcConnected)
            {
                View.BusySet("Lade Daten ...", 200);
            }

            //Workaround Busy
            Dispatcher!.RunOnDispatcher(async () =>
            {
                await Task.Delay(10000).ConfigureAwait(false);
                if (View.IsBusy)
                {
                    PickerCompaniesUpdate();
                }

                View.BusyClear();
            });

            return base.OnAppearing(view);
        }

        /// <summary>View ist komplett geladen und sichtbar</summary>
        /// <returns></returns>
        public override Task OnLoaded()
        {
            if (!Dc.AutoConnect)
            {
                DcStartAutoConnect();
            }

            if (!ProjectDataLoadedAfterDcConnected)
            {
                ProjectDataLoaded += (sender, args) =>
                {
                    InitAfterLoadedData();
                    View.BusyClear();
                };
            }
            else
            {
                InitAfterLoadedData();
                View.BusyClear();
            }

            AttachDetachVmEvents(true);

            return base.OnLoaded();
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdEditCompanies = new VmCommand("Firma bearbeiten", async () =>
            {
                _ = await Nav.ToViewWithResult(typeof(VmCompany), Dc.DcExCompanies.SelectedItem).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
            }, CanExecuteEditCompany, glyph: Glyphs.Notes_edit);


            CmdSelectListItem = new VmCommand("", i =>
            {
                if (i is DcListDataPoint<ExProject> p)
                {
                    SelectedProject = p;
                }
                else if (i is DcListDataPoint<ExMeasurementDefinition> m)
                {
                    SelectedMeasurementDefinition = m;
                }
            }, glyph: Glyphs.Arrow_right);

            base.InitializeCommands();
        }

        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                Dc.DcExCompanies.SelectedItemChanged += DcExCompanies_SelectedItemChanged;
            }
            else
            {
                Dc.DcExCompanies.SelectedItemChanged -= DcExCompanies_SelectedItemChanged;
            }
        }

        private void DcExCompanies_SelectedItemChanged(object sender, SelectedItemEventArgs<DcListTypeCompany> e)
        {
            //Bugfix, wenn auskommentiert, und ein nicht-Admin eingeloggt ist und zeigt das dropdown auch alle NoCompanies an, aber wenn man auswaehlt=>indexoutofrangeexception weil in Dc.DcExCompanies die  NoCompanies rausgefiltert wurden
            //Dc.DcExCompanies.FilterList(f => f.Data.CompanyType != EnumCompanyTypes.NoCompany || Dc.DcExUser.Data.IsAdmin);

            if (e.CurrentItem != null!)
            {
                Dc.DcExLocalAppData.Data.LastSelectedCompanyId = e.CurrentItem.Id;
                Dc.DcExLocalAppData.StoreData();

                UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem?.Index));
                SelectedProject = null!;
            }

            CheckCommandsCanExecute();
        }

        private async void InitAfterLoadedData()
        {
            if (_firstTimeViewLoaded)
            {
                _firstTimeViewLoaded = false;
                if (Dc.DcExLocalAppData.Data.LastSelectedCompanyId != null &&
                    Dc.DcExCompanies.Any(x => x.Id == Dc.DcExLocalAppData.Data.LastSelectedCompanyId))
                {
                    Dc.DcExCompanies.SelectedItem = Dc.DcExCompanies.First(a => a.Index == Dc.DcExLocalAppData.Data.LastSelectedCompanyId);
                }

                Dc.DcExCompanies.CollectionChanged += (sender, args) =>
                {
                    PickerCompaniesUpdate();
                    View.BusyClear();
                };


                Dc.DcExProjects.LoadingFromHostEvent += DcExProjectsOnLoadingFromHostEvent;
            }
            else
            {
                UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem?.Index));
                SelectedProject = null!;
            }

            if (Dc.CoreConnectionInfos.UserOk)
            {
                if (string.IsNullOrEmpty(Dc.DcExUser.Data.FirstName) || string.IsNullOrEmpty(Dc.DcExUser.Data.LastName))
                {
                    await MsgBox.Show("Bitte vervollständigen Sie Ihren Account.", "Benutzeraccount").ConfigureAwait(true);
                    _ = await Nav.ToViewWithResult(typeof(VmEditUser)).ConfigureAwait(false);
                    await View.RefreshAsync().ConfigureAwait(true);
                }
            }
        }

        private bool CanExecuteEditCompany()
        {
            var r = Dc.DcExCompanies.SelectedItem != null! && Dc.DcExCompanies.SelectedItem.Data.CompanyType != EnumCompanyTypes.NoCompany;

            if (r)
            {
                if (!Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem!.Index))
                {
                    return false;
                }
            }

            return r;
        }

        private void DcExProjectsOnLoadingFromHostEvent(object sender, bool e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                if (Dc.DcExCompanies.SelectedItem == null!)
                {
                    return;
                }

                UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem.Index));
            });
        }

        private void PickerCompaniesUpdate()
        {
            if (DeviceInfo.Plattform != EnumPlattform.Web)
            {
                Dispatcher!.RunOnDispatcher(() =>
                {
                    UiProjects = new BxObservableCollection<DcListTypeProject>(Dc.DcExProjects.Where(w => w.Data.CompanyId == Dc.DcExCompanies.SelectedItem?.Index));
                    SelectedProject = null!;
                });
            }
        }
    }
}