// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.ViewModel.Company;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Infrastructure
{
    /// <summary>
    ///     <para>Konfiguration des gesamten BDA Systems</para>
    ///     Klasse VmInfrastructure. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewInfrastructure", true)]
    public class VmInfrastructure : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmInfrastructure.DesignInstance}"
        /// </summary>
        public static VmInfrastructure DesignInstance = new VmInfrastructure();

        /// <summary>
        ///     VmInfrastructure
        /// </summary>
        public VmInfrastructure() : base("Infrastuktur", subTitle: "Aktuelle Komponenten der Firma")
        {
            SetViewProperties();
        }

        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attach"></param>
        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                Dc.DcExGateways.CollectionEvent += DcExGateways_CollectionEvent;
                Dc.DcExGateways.SelectedItemChanged += DcExGateways_SelectedItemChanged;
                Dc.DcExIotDevices.CollectionEvent += DcExIotDevices_CollectionEvent;
                Dc.DcExIotDevices.SelectedItemChanged += DcExIotDevices_SelectedItemChanged;
                Dc.DcExMeasurementDefinition.CollectionEvent += DcExMeasurementDefinition_CollectionEvent;
                Dc.DcExCompanies.SelectedItemChanged += DcExCompanies_SelectedItemChanged;
            }
            else
            {
                Dc.DcExGateways.CollectionEvent -= DcExGateways_CollectionEvent;
                Dc.DcExGateways.SelectedItemChanged -= DcExGateways_SelectedItemChanged;
                Dc.DcExIotDevices.CollectionEvent -= DcExIotDevices_CollectionEvent;
                Dc.DcExIotDevices.SelectedItemChanged -= DcExIotDevices_SelectedItemChanged;
                Dc.DcExMeasurementDefinition.CollectionEvent -= DcExMeasurementDefinition_CollectionEvent;
                Dc.DcExCompanies.SelectedItemChanged -= DcExCompanies_SelectedItemChanged;
            }
        }

        /// <summary>
        ///     Callback für das CollectionEvent der Dc.DcExIotDevices (Add, Edit, Delete)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task DcExIotDevices_CollectionEvent(object sender, CollectionEventArgs<DcListTypeIotDevice> e)
        {
            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest && CanExecuteAddIotDevice())
            {
                if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                {
                    await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                    return;
                }

                var newIot = new DcListTypeIotDevice(new ExIotDevice
                {
                    Plattform = EnumIotDevicePlattforms.Esp32,
                    TransmissionType = EnumTransmission.Elapsedtime,
                    Upstream = EnumIotDeviceUpstreamTypes.Ttn,
                    GatewayId = Dc.DcExGateways.SelectedItem!.Index,
                    CompanyId = Dc.DcExCompanies.SelectedItem!.Index,
                    Information =
                    {
                        CreatedDate = DateTime.UtcNow,
                    }
                });

                Dc.DcExIotDevices.Add(newIot);

                var r = await Nav.ToViewWithResult(typeof(VmEditIotDevice), newIot).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        // Workaround bis Fix in der Liste 
                        try
                        {
                            Dc.DcExIotDevices.Remove(newIot);
                        }
                        catch (Exception ex)
                        {
                            Logging.Log.LogError($"[{nameof(VmInfrastructure)}]({nameof(DcExIotDevices_CollectionEvent)}): Workaround - {ex}");
                        }
                    }

                    CheckCommandsCanExecute();
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }

            if (e.TypeOfEvent == EnumCollectionEventType.EditRequest)
            {
                if (e.Item != null!)
                {
                    if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                    {
                        await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                        return;
                    }


                    var r = await Nav.ToViewWithResult(typeof(VmEditIotDevice), e.Item).ConfigureAwait(true);
                    await View.RefreshAsync().ConfigureAwait(true);
                    if (r is EnumVmEditResult result)
                    {
                        if (result != EnumVmEditResult.ModifiedAndStored)
                        {
                            if (e.Item.PossibleNewDataOnServer)
                            {
                                e.Item.Update();
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }
                }
            }

            if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                {
                    await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                    return;
                }


                if (e.Item != null!)
                {
                    var msg = await MsgBox.Show("Alle bereits vorhandenen Messwerte werden gelöscht.\r\nFortfahren?", "Achtung", VmMessageBoxButton.YesNo).ConfigureAwait(true);
                    if (msg == VmMessageBoxResult.No)
                    {
                        return;
                    }

                    // Workaround bis Fix in der Liste 
                    try
                    {
                        Dc.DcExIotDevices.Remove(e.Item);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.LogError($"{ex}");
                    }

                    var r = await Dc.DcExIotDevices.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        await MsgBox.Show("Löschen leider nicht möglich!").ConfigureAwait(true);
                        Dc.DcExIotDevices.Add(e.Item);
                    }
                }
            }
        }

        /// <summary>
        ///     Callback für das CollectionEvent der Dc.DcExGateways (Add, Edit, Delete)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task DcExGateways_CollectionEvent(object sender, CollectionEventArgs<DcListTypeGateway> e)
        {
            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
            {
                if (CanExecuteAddGateway())
                {
                    if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                    {
                        await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                        return;
                    }

                    _ = await Nav.ToViewWithResult(typeof(VmAddGatewayToCompany)).ConfigureAwait(true);
                    await View.RefreshAsync().ConfigureAwait(true);
                    CheckCommandsCanExecute();
                }
            }

            if (e.TypeOfEvent == EnumCollectionEventType.EditRequest)
            {
                if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                {
                    await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                    return;
                }

                var r = await Nav.ToViewWithResult(typeof(VmEditGateway), e.Item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        if (e.Item.PossibleNewDataOnServer)
                        {
                            e.Item.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }

            if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                if (e.Item == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmCompany)}]({nameof(InitializeCommands)}): {nameof(e.Item)}");
                }

                if (Dc.DcExCompanies.SelectedItem != null! && !Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
                {
                    await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                    return;
                }


                var msg = await MsgBox.Show("Alle bereits vorhandenen Messwerte werden gelöscht.\r\nFortfahren?", "Achtung", VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (msg == VmMessageBoxResult.No)
                {
                    return;
                }

                // Workaround bis Fix in der Liste 
                try
                {
                    Dc.DcExGateways.Remove(e.Item);
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"{ex}");
                }

                var r = await Dc.DcExGateways.StoreAll().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    await MsgBox.Show("Löschen leider nicht möglich!").ConfigureAwait(true);
                    Dc.DcExGateways.Add(e.Item);
                }
            }
        }

        /// <summary>
        ///     Callback für das CollectionEvent der Dc.DcExMeasurementDefinition (Add, Edit, Delete)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task DcExMeasurementDefinition_CollectionEvent(object sender, CollectionEventArgs<DcListTypeMeasurementDefinition> e)
        {
            if (Dc.DcExCompanies.SelectedItem == null!)
            {
                await MsgBox.Show("Diese Aktion konnte nicht durchgeführt werden").ConfigureAwait(true);
                return;
            }

            if (!Dc.DcExUser.Data.IsAdmInCompany(Dc.DcExCompanies.SelectedItem.Index))
            {
                await MsgBox.Show("Nicht erlaubt für den aktuellen Benutzer.").ConfigureAwait(true);
                return;
            }

            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
            {
                if (!CanExecuteAddMeasurementDefinition())
                {
                    return;
                }

                if (Dc.DcExIotDevices.SelectedItem == null!)
                {
                    await MsgBox.Show("Diese Aktion konnte nicht durchgeführt werden").ConfigureAwait(true);
                    return;
                }

                var newMd = new GcDownstreamVirtualFloat().ToExMeasurementDefinition();
                newMd.IotDeviceId = Dc.DcExIotDevices.SelectedItem.Index;
                newMd.MeasurementInterval = -1;
                var item = new DcListTypeMeasurementDefinition(newMd);

                Dc.DcExMeasurementDefinition.Add(item);

                var r = await Nav.ToViewWithResult(typeof(VmEditMeasurementDefinition), item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        // Workaround bis Fix in der Liste 
                        try
                        {
                            Dc.DcExMeasurementDefinition.Remove(item);
                        }
                        catch (Exception ex)
                        {
                            Logging.Log.LogError($"{ex}");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                CheckCommandsCanExecute();
            }

            if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                var msg = await MsgBox.Show("Alle bereits vorhandenen Messwerte werden gelöscht.\r\nFortfahren?", "Achtung", VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (msg == VmMessageBoxResult.No)
                {
                    return;
                }

                // Workaround bis Fix in der Liste 
                try
                {
                    Dc.DcExMeasurementDefinition.Remove(e.Item);
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"{ex}");
                }

                var r = await Dc.DcExMeasurementDefinition.StoreAll().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    await MsgBox.Show("Löschen leider nicht möglich!").ConfigureAwait(true);
                    Dc.DcExMeasurementDefinition.Add(e.Item);
                }
            }

            if (e.TypeOfEvent == EnumCollectionEventType.EditRequest)
            {
                var r = await Nav.ToViewWithResult(typeof(VmEditMeasurementDefinition), e.Item).ConfigureAwait(true);
                await View.RefreshAsync().ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        if (e.Item.PossibleNewDataOnServer)
                        {
                            e.Item.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }
        }

        /// <summary>
        ///     Ausgewähltes IoT Device geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExIotDevices_SelectedItemChanged(object sender, SelectedItemEventArgs<DcListTypeIotDevice> e)
        {
            UpdateLists(EnumChangedType.IotDevice);
        }

        /// <summary>
        ///     Ausgewähltes Gateway geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExGateways_SelectedItemChanged(object sender, SelectedItemEventArgs<DcListTypeGateway> e)
        {
            Dc.DcExIotDevices.SelectedItem = null!;
            UpdateLists(EnumChangedType.Gateway);
        }

        /// <summary>
        ///     Ausgewählte Firma geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExCompanies_SelectedItemChanged(object sender, SelectedItemEventArgs<DcListTypeCompany> e)
        {
            //Bugfix, wenn auskommentiert, und ein nicht-Admin eingeloggt ist und zeigt das dropdown auch alle NoCompanies an, aber wenn man auswaehlt=>indexoutofrangeexception weil in Dc.DcExCompanies die  NoCompanies rausgefiltert wurden
            //Dc.DcExCompanies.FilterList(f => f.Data.CompanyType != EnumCompanyTypes.NoCompany || Dc.DcExUser.Data.IsAdmin);

            if (e.CurrentItem != null!)
            {
                Dc.DcExGateways.FilterList(f => f.Data.CompanyId == e.CurrentItem.Index);
                Dc.DcExGateways.SelectedItem = null!;
                Dc.DcExLocalAppData.Data.LastSelectedCompanyId = e.CurrentItem.Id;
                Dc.DcExLocalAppData.StoreData();
            }


            UpdateLists(EnumChangedType.Company);
            //CheckCommandsCanExecute();
        }

        /// <summary>
        ///     Listen filtern
        /// </summary>
        private void UpdateLists(EnumChangedType changedType)
        {
            var clearIotDev = Dc.DcExGateways.SelectedItem == null!;
            var clearMdef = Dc.DcExGateways.SelectedItem == null! || Dc.DcExIotDevices.SelectedItem == null!;
            // fuer Performance, wenn iotdevice oder measurementdefinition muss gateway nicht aktualisiert werden
            if (changedType != EnumChangedType.Gateway && changedType != EnumChangedType.IotDevice && changedType != EnumChangedType.MeasurementDefinition)
            {
                if (Dc.DcExCompanies.SelectedItem != null)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    Dc.DcExGateways.FilterList(f => f.Data != null && f.Data.CompanyId == Dc.DcExCompanies.SelectedItem.Index);
                }
                else
                {
                    Dc.DcExGateways.FilterList(f => false);
                }
            }

            if (changedType != EnumChangedType.IotDevice && changedType != EnumChangedType.MeasurementDefinition)
            {
                if (clearIotDev)
                {
                    Dc.DcExIotDevices.FilterList(f => false);

                    if (Dc.DcExIotDevices.SelectedItem != null!)
                    {
                        Dc.DcExIotDevices.SelectedItem = null!;
                    }
                }
                else
                {
                    Dc.DcExIotDevices.FilterList(w => w.Data.GatewayId == Dc.DcExGateways.SelectedItem!.Id);
                }
            }

            if (changedType != EnumChangedType.MeasurementDefinition)
            {
                if (clearMdef)
                {
                    Dc.DcExMeasurementDefinition.FilterList(f => false);

                    if (Dc.DcExMeasurementDefinition.SelectedItem != null!)
                    {
                        Dc.DcExMeasurementDefinition.SelectedItem = null!;
                    }
                }
                else
                {
                    Dc.DcExMeasurementDefinition.FilterList(w => w.Data.IotDeviceId == Dc.DcExIotDevices.SelectedItem!.Index);
                }
            }

            CheckCommandsCanExecute();
        }

        /// <summary>
        ///     Kann Firma Bearbeiten ausgeführt werden
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Kann eine Messwertdefinition hinzugefügt werden?
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAddMeasurementDefinition()
        {
            if (DeviceInfo.Plattform != EnumPlattform.XamarinAndroid && !IsLoaded)
            {
                return false;
            }

            if (Dc.DcExIotDevices.SelectedItem == null!)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Kann ein Iot Gerät hinzugefügt werden?
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAddIotDevice()
        {
            if (DeviceInfo.Plattform != EnumPlattform.XamarinAndroid && !IsLoaded)
            {
                return false;
            }

            if (Dc.DcExGateways.SelectedItem == null!)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Kann ein Gateway hinzugefügt werden?
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAddGateway()
        {
            if (DeviceInfo.Plattform != EnumPlattform.XamarinAndroid && !IsLoaded)
            {
                return false;
            }

            if (Dc.DcExCompanies.SelectedItem == null!)
            {
                return false;
            }

            if (Dc.DcExCompanies.SelectedItem.Data.CompanyType == EnumCompanyTypes.NoCompany)
            {
                return false;
            }

            return true;
        }

        #region Overrides

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            Dc.DcExGateways.LoadingFromHostEvent += (sender, b) => { UpdateLists(EnumChangedType.Other); };
            Dc.DcExIotDevices.LoadingFromHostEvent += (sender, b) => { UpdateLists(EnumChangedType.Other); };
            Dc.DcExMeasurementDefinition.LoadingFromHostEvent += (sender, b) => { UpdateLists(EnumChangedType.Other); };
            //Bugfix, wenn auskommentiert, und ein nicht-Admin eingeloggt ist und zeigt das dropdown auch alle NoCompanies an, aber wenn man auswaehlt=>indexoutofrangeexception weil in Dc.DcExCompanies die  NoCompanies rausgefiltert wurden
            //Dc.DcExCompanies.FilterList(f => f.Data.CompanyType != EnumCompanyTypes.NoCompany || Dc.DcExUser.Data.IsAdmin);
            UpdateLists(EnumChangedType.Other);
            base.OnLoaded();

            AttachDetachVmEvents(true);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachDetachVmEvents(false);
            Dc.DcExGateways.FilterClear();
            Dc.DcExCompanies.FilterClear();
            Dc.DcExIotDevices.FilterClear();
            Dc.DcExMeasurementDefinition.FilterClear();
            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdEditCompanies = new VmCommand("Firma bearbeiten", async () =>
            {
                _ = await Nav.ToViewWithResult(typeof(VmCompany), Dc.DcExCompanies.SelectedItem).ConfigureAwait(true);
                AttachDetachVmEvents(true);
                await View.RefreshAsync().ConfigureAwait(true);
            }, CanExecuteEditCompany, glyph: Glyphs.Notes_edit);
        }

        /// <summary>
        ///     Firma bearbeiten
        /// </summary>
        public VmCommand CmdEditCompanies { get; private set; } = null!;

        #endregion
    }
}