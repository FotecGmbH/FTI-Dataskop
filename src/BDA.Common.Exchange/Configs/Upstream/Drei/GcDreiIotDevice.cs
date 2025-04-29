// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Upstream.Drei
{
    /// <summary>
    ///     <para>Config für ein Iot Device für Drei (steht in der Additional Config eines Iot Geräts)</para>
    ///     Klasse GcTtnIotDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Drei Iot Device")]
    public class GcDreiIotDevice : GcIotDevice
    {
        /// <summary>
        /// </summary>
        public GcDreiIotDevice()
        {
            ConfigType = EnumGlobalConfigTypes.Drei;
        }

        #region Properties

        /// <summary>
        ///     Globaler (Firmen) Drei Account
        /// </summary>
        public GcDrei GcDreiCompany { get; set; } = new();

        /// <summary>
        ///     device_id des Geräts. Nur kleinbuchstaben, Zahlen, und Bindestrich erlaubt. (hier einfach dev-eui nutzen?).
        ///     Wenn mittels MQTT Werte abgerufen werden, wird als Referenz dieser Wert angegeben, und sollte einem IoTDevice
        ///     zugeordnet werden können.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        ///     deveui des geraetes
        /// </summary>
        public string DevEui { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///     Erzeugt ein <see cref="GcTtnIotDevice" /> Object aus dem AdditionalConfigstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcDreiIotDevice? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcDreiIotDevice>(config);
        }

        /// <summary>
        ///     Converter vom Kompatibilitäts model. Kann später entfernt werden.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static GcDreiIotDevice? FromCompatibilityModel(GcDreiIotDeviceCompat? input)
        {
            if (input is null)
            {
                return null;
            }

            var result = new GcDreiIotDevice
            {
                ConfigType = input.ConfigType,
                DeviceId = input.DeviceId,
                UserCode = input.UserCode,
                DevEui = input.DevEui,
                GcDreiCompany = input.GcDreiCompany
            };

            return result;
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