// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Management;
using System.Windows;
using BDA.Common.Exchange.Interfaces;
using BDA.Common.Exchange.Model;
using WpfApp.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(DsDeviceInfos))]

namespace WpfApp.DependencyServices
{
    /// <summary>
    ///     <para>Infos zum PC</para>
    ///     Klasse DsDeviceInfos. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class DsDeviceInfos : IDeviceInfos
    {
        #region Interface Implementations

        /// <summary>
        ///     Plattform Infos auslesen
        /// </summary>
        /// <returns></returns>
        public ExDeviceInfo GetInfosDeviceInfo()
        {
            var r = new ExDeviceInfo();
            using var baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (var queryObj in baseboardSearcher.Get())
            {
                if (queryObj == null!)
                {
                    continue;
                }

                try
                {
                    r.Manufacturer = queryObj["Manufacturer"].ToString()!;
                }
                catch
                {
                    r.Manufacturer = "?";
                }

                try
                {
                    r.Model = queryObj["Product"].ToString()!;
                }
                catch
                {
                    r.Model = "?";
                }

                try
                {
                    r.DeviceHardwareId = queryObj["SerialNumber"].ToString()!;
                }
                catch
                {
                    r.DeviceHardwareId = "?";
                }

                var w = SystemParameters.PrimaryScreenWidth;
                var h = SystemParameters.PrimaryScreenHeight;
                r.ScreenResolution = $"{w} x {h}";
            }


            return r;
        }

        #endregion
    }
}