// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Upstream.Drei
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse GcDreiIotDeviceCompat. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDreiIotDeviceCompat : GcDreiIotDevice
    {
        /// <summary>
        /// GcDreiIotDeviceCompat
        /// </summary>
        public GcDreiIotDeviceCompat()
        {
            ConfigType = EnumGlobalConfigTypes.Drei;
        }

        #region Properties

        /// <summary>
        ///     Globaler (Firmen) Drei Account
        /// </summary>
#pragma warning disable CS0108, CS0114
        public GcDrei GcDreiCompany { get; set; } = new();
#pragma warning restore CS0108, CS0114

        /// <summary>
        ///     device_id des Geräts. Nur kleinbuchstaben, Zahlen, und Bindestrich erlaubt. (hier einfach dev-eui nutzen?).
        ///     Wenn mittels MQTT Werte abgerufen werden, wird als Referenz dieser Wert angegeben, und sollte einem IoTDevice
        ///     zugeordnet werden können.
        /// </summary>
#pragma warning disable CS0108, CS0114
        public string DeviceId { get; set; } = string.Empty;
#pragma warning restore CS0108, CS0114

        /// <summary>
        ///     deveui des geraetes
        /// </summary>
#pragma warning disable CS0108, CS0114
        public string DevEui { get; set; } = string.Empty;
#pragma warning restore CS0108, CS0114

        #endregion

        /// <summary>
        ///     Erzeugt ein  Object aus dem AdditionalConfigstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS0108, CS0114
        public static GcDreiIotDevice? FromAdditionalConfig(string? config)
#pragma warning restore CS0108, CS0114
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcDreiIotDevice>(config);
        }

#pragma warning disable CS0067
#pragma warning disable CS0414
      
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0108, CS0114
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108, CS0114
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}