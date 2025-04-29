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
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Core;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Exchange.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>BISS Template Start View(Model)</para>
    ///     Klasse ViewModelUserAccount. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewMain", true)]
    public class VmMain : VmProjectBase
    {
        /// <summary>
        ///     ViewModel Template
        /// </summary>
        public VmMain() : base("Main", subTitle: "Main View")
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = false;
            View.ShowMenu = true;
        }

        #region Properties

        /// <summary>
        ///     Alle Geräte.
        /// </summary>
        public int AllDevices => DcConnection!.GetClients().Count;

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEntry.DesignInstance}"
        /// </summary>
        public static VmMain DesignInstance => new VmMain();

        /// <summary>
        ///     Dc Connection
        /// </summary>
        public IDcConnections? DcConnection { get; set; } = null!;

        /// <summary>
        ///     Bearbeiten Command
        /// </summary>
        public VmCommand CmdClearLog { get; set; } = null!;

        /// <summary>
        ///     Link zu Playstore
        /// </summary>
        public string PlaystoreLink => "TOENTER";

        /// <summary>
        ///     Link zu Appcenter
        /// </summary>
        public string AppcenterLink => "TOENTER";

        /// <summary>
        ///     Link zu Devops
        /// </summary>
        public string DevopsLink => "TOENTER";

        /// <summary>
        ///     Link zu Firebase
        /// </summary>
        public string FirebaseLink => "TOENTER";

        /// <summary>
        ///     Link zum Appstore
        /// </summary>
        public string AppstoreLink => "TOENTER";

        /// <summary>
        ///     Android Geräte
        /// </summary>
        public int AndroidDevices { get; set; }

        /// <summary>
        ///     Ios Geräte
        /// </summary>
        public int IosDevices { get; set; }

        /// <summary>
        ///     WPF Geräte
        /// </summary>
        public int WpfDevices { get; set; }

        /// <summary>
        ///     Web Geräte
        /// </summary>
        public int WebDevices { get; set; }

        /// <summary>
        ///     Migration Information
        /// </summary>
        public List<string> MigrationData { get; set; } = new List<string>();

        #endregion

        /// <summary>
        ///     Berechne Daten
        /// </summary>
        /// <returns></returns>
        public async Task CalculateData()
        {
            var connectedDevices = new List<TableDevice>();
            try
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                if (DcConnection == null)
                {
                    return;
                }

                if (DcConnection.GetClients().Count != 0)
                {
                    foreach (var device in DcConnection.GetClients())
                    {
                        var foundDevice = db.TblDevices.FirstOrDefault(d => d.Id == device.DeviceId);
                        if (foundDevice != null)
                        {
                            connectedDevices.Add(foundDevice);
                        }
                    }

                    AndroidDevices = connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinAndroid);
                    IosDevices = connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinIos);
                    WpfDevices = connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinWpf);
                    WebDevices = connectedDevices.Count(d => d.Plattform == EnumPlattform.Web);
                }

                var d = await db.Database.GetAppliedMigrationsAsync();

                MigrationData = d.ToList();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdClearLog = new VmCommand("Log löschen", _ => { LogEntries.Clear(); }, glyph: Glyphs.Bin_2);
        }
    }

    /// <summary>
    ///     Extension Methods.
    /// </summary>
    public static class DeviceExtensionMethods
    {
        /// <summary>
        ///     Get Android Devices Count.
        /// </summary>
        /// <param name="devices">Geräte</param>
        /// <returns>Anzahl.</returns>
        public static int GetAndroidDevices(this List<ExDcCoreInfos> devices)
        {
            var connectedDevices = new List<TableDevice>();
            try
            {
                using var db = new Db();

                if (devices == null!)
                {
                    return 0;
                }

                foreach (var device in devices)
                {
                    var foundDevice = db.TblDevices.FirstOrDefault(d => d.Id == device.DeviceId);
                    if (foundDevice != null)
                    {
                        connectedDevices.Add(foundDevice);
                    }
                }

                return connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinAndroid);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }
        }

        /// <summary>
        ///     Get Ios Devices Count.
        /// </summary>
        /// <param name="devices">Geräte</param>
        /// <returns>Anzahl.</returns>
        public static int GetIosDevices(this List<ExDcCoreInfos> devices)
        {
            var connectedDevices = new List<TableDevice>();
            try
            {
                using var db = new Db();

                if (devices == null!)
                {
                    return 0;
                }

                foreach (var device in devices)
                {
                    var foundDevice = db.TblDevices.FirstOrDefault(d => d.Id == device.DeviceId);
                    if (foundDevice != null)
                    {
                        connectedDevices.Add(foundDevice);
                    }
                }

                return connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinIos);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }
        }

        /// <summary>
        ///     Get Wpf Devices Count.
        /// </summary>
        /// <param name="devices">Geräte</param>
        /// <returns>Anzahl.</returns>
        public static int GetWpfDevices(this List<ExDcCoreInfos> devices)
        {
            try
            {
                var connectedDevices = new List<TableDevice>();
                using var db = new Db();

                if (devices == null!)
                {
                    return 0;
                }

                foreach (var device in devices)
                {
                    var foundDevice = db.TblDevices.FirstOrDefault(d => d.Id == device.DeviceId);
                    if (foundDevice != null)
                    {
                        connectedDevices.Add(foundDevice);
                    }
                }

                return connectedDevices.Count(d => d.Plattform == EnumPlattform.XamarinWpf);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }
        }

        /// <summary>
        ///     Get Web Devices Count.
        /// </summary>
        /// <param name="devices">Geräte</param>
        /// <returns>Anzahl.</returns>
        public static int GetWebDevices(this List<ExDcCoreInfos> devices)
        {
            try
            {
                var connectedDevices = new List<TableDevice>();
                using var db = new Db();

                if (devices == null!)
                {
                    return 0;
                }

                foreach (var device in devices)
                {
                    var foundDevice = db.TblDevices.FirstOrDefault(d => d.Id == device.DeviceId);
                    if (foundDevice != null)
                    {
                        connectedDevices.Add(foundDevice);
                    }
                }

                return connectedDevices.Count(d => d.Plattform == EnumPlattform.Web);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                throw;
            }
        }
    }
}