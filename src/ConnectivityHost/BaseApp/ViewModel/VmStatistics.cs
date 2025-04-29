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
using Biss.Apps.Attributes;
using Biss.Common;
using Biss.Dc.Server;
using Database;
using Database.Tables;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     Class for chart
    /// </summary>
    public class DataItem
    {
        #region Properties

        /// <summary>
        ///     string for Cart Label
        /// </summary>
        public string Labels { get; set; } = String.Empty;

        /// <summary>
        ///     double for Values
        /// </summary>
        public double Value { get; set; }

        #endregion
    }

    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse VmStatistics. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewStatistics")]
    public class VmStatistics : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmStatistics.DesignInstance}"
        /// </summary>
        public static VmStatistics DesignInstance = new VmStatistics();

        /// <summary>
        ///     VmStatistics
        /// </summary>
        public VmStatistics() : base("Statistiken", "Statistiken über die verbundenen Geräte")
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = false;
            View.ShowMenu = true;
            View.ShowSubTitle = true;
            RefreshData();
        }

        #region Properties

        /// <summary>
        ///     Geräte Liste
        /// </summary>
        public List<TableDevice> DevicesDb { get; private set; } = null!;

        /// <summary>
        ///     List for Device Chart
        /// </summary>
        public List<DataItem> Devices { get; set; } = new();

        /// <summary>
        ///     List for App Version Chart
        /// </summary>
        public List<DataItem> AppVersions { get; set; } = new();

        /// <summary>
        ///     List for Ios Version Chart
        /// </summary>
        public List<DataItem> IosChart { get; set; } = new();

        /// <summary>
        ///     List for Android Version Chart
        /// </summary>
        public List<DataItem> AndroidChart { get; set; } = new();

        /// <summary>
        ///     List for WPF Version Chart
        /// </summary>
        public List<DataItem> WpfChart { get; set; } = new();

        /// <summary>
        ///     List for Web Chart
        /// </summary>
        public List<DataItem> WebChart { get; set; } = new();

        /// <summary>
        ///     Dc Connection
        /// </summary>
        public IDcConnections? DcConnection { get; set; } = null!;

        #endregion

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            Devices = new()
            {
                new DataItem {Labels = "Android", Value = DevicesDb.Count(d => d.Plattform == EnumPlattform.XamarinAndroid)},
                new DataItem {Labels = "IOS", Value = DevicesDb.Count(d => d.Plattform == EnumPlattform.XamarinIos)},
                new DataItem {Labels = "WPF", Value = DevicesDb.Count(d => d.Plattform == EnumPlattform.XamarinWpf)},
                new DataItem {Labels = "Web", Value = DevicesDb.Count(d => d.Plattform == EnumPlattform.Web)}
            };

            var appversions = DevicesDb.GroupBy(t => t.AppVersion).ToList();
            foreach (var version in appversions)
            {
                AppVersions.Add(new DataItem
                {
                    Labels = $"{version.Key}",
                    Value = version.Count()
                });
            }

            var androidDevices = DevicesDb.Where(t => t.Plattform == EnumPlattform.XamarinAndroid).ToList();
            var anOsversions = androidDevices.GroupBy(t => t.OperatingSystemVersion).ToList();
            foreach (var version in anOsversions)
            {
                AndroidChart.Add(new DataItem
                {
                    Labels = $"{version.Key}",
                    Value = version.Count()
                });
            }

            var iosDevices = DevicesDb.Where(t => t.Plattform == EnumPlattform.XamarinIos).ToList();
            var iosOsversions = iosDevices.GroupBy(t => t.OperatingSystemVersion).ToList();
            foreach (var version in iosOsversions)
            {
                IosChart.Add(new DataItem
                {
                    Labels = $"{version.Key}",
                    Value = version.Count()
                });
            }

            var webDevices = DevicesDb.Where(t => t.Plattform == EnumPlattform.Web).ToList();
            var webOsversions = webDevices.GroupBy(t => t.OperatingSystemVersion).ToList();
            foreach (var version in webOsversions)
            {
                WebChart.Add(new DataItem
                {
                    Labels = $"{version.Key}",
                    Value = version.Count()
                });
            }

            var wpfDevices = DevicesDb.Where(t => t.Plattform == EnumPlattform.XamarinWpf).ToList();
            var wpfOsversions = wpfDevices.GroupBy(t => t.OperatingSystemVersion).ToList();
            foreach (var version in wpfOsversions)
            {
                WpfChart.Add(new DataItem
                {
                    Labels = $"{version.Key}",
                    Value = version.Count()
                });
            }

            return base.OnLoaded();
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
        }

        private void RefreshData()
        {
            View.BusySet();
            using (var db = new Db())
            {
                DevicesDb = db.TblDevices.ToList();
            }

            View.BusyClear();
        }
    }
}