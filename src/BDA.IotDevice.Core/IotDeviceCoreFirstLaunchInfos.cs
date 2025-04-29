// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.IotDevice.Core
{
    /// <summary>
    ///     <para>First Launch infos for iotdevice</para>
    ///     Klasse IoTDeviceCoreFirstLaunchInfos. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class IoTDeviceCoreFirstLaunchInfos
    {
        #region Properties

        /// <summary>
        ///     Name des iot devices
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Upstreamtype of the iot device
        /// </summary>
        public EnumIotDeviceUpstreamTypes UpstreamType { get; set; }

        /// <summary>
        ///     Position des Gerätes
        /// </summary>
        public ExPosition? Position { get; set; }

        /// <summary>
        ///     Hostname or ipaddress => was entered by user
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string HostNOrIPAddess { get; set; } = string.Empty;

        /// <summary>
        ///     Secret des Gerätes
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        #endregion

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}