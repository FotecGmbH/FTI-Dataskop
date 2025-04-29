// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.Interfaces
{
    /// <summary>
    ///     <para>Konfig Basis für Gateway und Iot Device</para>
    ///     Interface IConfigBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IConfigBase : IBissSerialize
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id des Gateway oder IoT Device
        /// </summary>
        long DbId { get; set; }

        /// <summary>
        ///     Konfigurierter Name des Gateway oder IotDevice
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Secret - um dieses Iot-Geräts einem Gateway oder Firma zuweisen zu können
        /// </summary>
        string Secret { get; set; }

        /// <summary>
        ///     Firmware Version des Gateway oder IoT Device
        /// </summary>
        string FirmwareVersion { get; set; }

        /// <summary>
        ///     Config Version des Gateway oder IoT Device
        /// </summary>
        long ConfigVersion { get; set; }

        #endregion
    }
}