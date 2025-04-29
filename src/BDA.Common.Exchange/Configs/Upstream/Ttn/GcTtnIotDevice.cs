// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Upstream.Ttn
{
    /// <summary>
    ///     <para>Config für ein Iot Device für Ttn (steht in der Additional Config eines Iot Geräts)</para>
    ///     Klasse GcTtnIotDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Ttn Iot Device")]
    public class GcTtnIotDevice : GcIotDevice
    {
        /// <summary>
        /// </summary>
        public GcTtnIotDevice()
        {
            ConfigType = EnumGlobalConfigTypes.Ttn;
        }

        #region Properties

        /// <summary>
        ///     Globaler (Firmen) Ttn Account
        /// </summary>
        public GcTtn GcTtnCompany { get; set; } = new();


        /// <summary>
        ///     device_id des Geräts. Nur kleinbuchstaben, Zahlen, und Bindestrich erlaubt. (hier einfach dev-eui nutzen?).
        ///     Wenn mittels MQTT Werte abgerufen werden, wird als Referenz dieser Wert angegeben, und sollte einem IoTDevice
        ///     zugeordnet werden können.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        ///     dev_eui des Geräts. Wird vom Device benötigt um zu Joinen. Kann entweder selbst eingegeben werden (aufgedruckt)
        ///     oder von TTN-Generiert werden.
        /// </summary>
        [ConfigProperty("Ttn Dev Eui [Leer lassen zum generieren]")]
        public string DevEui { get; set; } = string.Empty;

        /// <summary>
        ///     app_eui des Geräts. Wird vom Device benötigt um zu Joinen. (aufgedruckt)
        /// </summary>
        [ConfigProperty("Ttn App/Join Eui")]
        public string AppEui { get; set; } = string.Empty;

        /// <summary>
        ///     Appkey des Geräts. Wird vom Device benötigt um zu Joinen. (aufgedruckt)
        /// </summary>
        [ConfigProperty("Ttn AppKey")]
        public string AppKey { get; set; } = string.Empty;

        /// <summary>
        ///     Description des Geräts. Optional
        /// </summary>
        [ConfigProperty("Ttn AppKey [Optional]")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Device wird automatisch auf ttn angelegt. False bei manuell angelegten Devices.
        /// </summary>
        [ConfigProperty("Device automatisch auf TTN anlegen")]
        public bool CreateDeviceOnTtn { get; set; } = true;

        /// <summary>
        ///     Ob Devices per OTAA Joinen dürfen.
        /// </summary>
        public bool SupportsJoin { get; set; } = true;

        /// <summary>
        ///     Lorawanversion des Geräts
        /// </summary>
        public EnumLorawanVersion LorawanVersion { get; set; } = EnumLorawanVersion.MAC_V1_0_3;

        /// <summary>
        ///     Physikalische loraversion des Geräts
        /// </summary>
        public EnumLorawanPhysicalVersion LoraPhysicalVersion { get; set; } = EnumLorawanPhysicalVersion.PHY_V1_0_3_REV_A;

        /// <summary>
        ///     LoraFrequencyplan des Geräts
        /// </summary>
        public EnumLorawanFrequencyPlanId LoraFrequency { get; set; } = EnumLorawanFrequencyPlanId.EU_863_870_TTN;

        #endregion

        /// <summary>
        ///     Erzeugt ein <see cref="GcTtnIotDevice" /> Object aus dem AdditionalConfigstring
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static GcTtnIotDevice? FromAdditionalConfig(string? config)
        {
            if (config is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GcTtnIotDevice>(config);
        }

        /// <summary>
        ///     Converter vom Kompatibilitäts model. Kann später entfernt werden.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static GcTtnIotDevice? FromCompatibilityModel(GcTtnIotDeviceCompat? input)
        {
            if (input is null)
            {
                return null;
            }

            var result = new GcTtnIotDevice
            {
                AppEui = input.AppEui,
                AppKey = input.AppKey,
                ConfigType = input.ConfigType,
                CreateDeviceOnTtn = true,
                Description = input.Description,
                DevEui = input.DevEui,
                DeviceId = input.DeviceId,
                GcTtnCompany = input.GcTtnCompany,
                LoraFrequency = input.LoraFrequency,
                LoraPhysicalVersion = input.LoraPhysicalVersion,
                LorawanVersion = input.LorawanVersion,
                SupportsJoin = input.SupportsJoin,
                UserCode = input.UserCode
            };

            return result;
        }
    }
}