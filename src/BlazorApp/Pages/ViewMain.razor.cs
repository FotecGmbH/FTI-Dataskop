// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using Biss.Collections;
using Radzen.Blazor;

namespace BlazorApp.Pages
{
    public partial class ViewMain
    {
        private RadzenDataGrid<DcListTypeMeasurementDefinition>? _radGridMeasurements;
        private RadzenDataGrid<DcListTypeProject>? _radGridProjects;

        /// <inheritdoc />
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ViewModel.Dc.DcExProjects.SelectedItemChanged += DcExProjectsOnSelectedItemChanged;
                ViewModel.Dc.DcExMeasurementDefinition.SelectedItemChanged += DcExMeasurementDefinitionOnSelectedItemChanged;
                ViewModel.Dc.DcExProjects.CollectionChanged += DcExProjectsOnCollectionChanged;
                ViewModel.Dc.DcExMeasurementDefinition.CollectionChanged += DcExMeasurementDefinitionOnCollectionChanged;
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private void DcExMeasurementDefinitionOnSelectedItemChanged(object? sender, SelectedItemEventArgs<DcListTypeMeasurementDefinition> e)
        {
            if (_radGridMeasurements != null ! && e.CurrentItem == null !)
            {
                _radGridMeasurements.Reset(resetRowState: true);
            }
        }

        private void DcExProjectsOnSelectedItemChanged(object? sender, SelectedItemEventArgs<DcListTypeProject> e)
        {
            if (_radGridProjects != null ! && e.CurrentItem == null !)
            {
                _radGridProjects.Reset(resetRowState: true);
            }
        }

        private void DcExMeasurementDefinitionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_radGridMeasurements != null ! && e.Action != NotifyCollectionChangedAction.Move)
            {
                _radGridMeasurements.Reload();
            }
        }

        private void DcExProjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_radGridProjects != null ! && e.Action != NotifyCollectionChangedAction.Move)
            {
                _radGridProjects.Reload();
            }
        }

        private void ProjectCallback(DcListTypeProject obj)
        {
            ViewModel?.CmdSelectListItem.Execute(obj);
        }

        private void MeasurementCallback(DcListTypeMeasurementDefinition obj)
        {
            ViewModel?.CmdSelectListItem.Execute(obj);
        }
    }
}