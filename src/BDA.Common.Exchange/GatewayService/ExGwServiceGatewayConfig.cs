// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Interfaces;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Konfiguration für ein Gateway</para>
    ///     Klasse ExGatewayConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGwServiceGatewayConfig : IBissModel, IConfigBase
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id des Gateway
        /// </summary>
        public long DbId { get; set; } = -1;

        /// <summary>
        ///     Konfigurierter Name des Gateway
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Beschreibung
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Gateway Secret - um diesen einer Firma zuweisen zu können
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        ///     Firmware Version des Gateway
        /// </summary>
        public string FirmwareVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Config Version des Gateway
        /// </summary>
        public long ConfigVersion { get; set; } = -1;

        /// <summary>
        ///     Position des Gateway
        /// </summary>
        public ExPosition? Position { get; set; }

        /// <summary>
        ///     Iot Geräte
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ExGwServiceIotDeviceConfig> IotDevices { get; set; } = null!;
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion

        /// <summary>
        ///     Konvertieren
        /// </summary>
        /// <returns></returns>
        public ExHubGatewayInfos ToExHubGatewayInfos()
        {
            return new ExHubGatewayInfos
            {
                GatewayId = DbId,
                ConfigVersion = ConfigVersion,
                FirmwareVerion = FirmwareVersion,
                Secret = Secret,
                GatewayName = Name,
                Position = Position,
                Description = Description
            };
        }

        /// <summary>
        ///     Config aktualisieren
        /// </summary>
        /// <param name="update"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void UpdateFromExHubGatewayInfos(ExGatwayRegisterResult update)
        {
            if (update == null!)
            {
                throw new ArgumentNullException($"[{nameof(ExGwServiceGatewayConfig)}]({nameof(UpdateFromExHubGatewayInfos)}): {nameof(update)} is null");
            }

            if (update.Invalid)
            {
                throw new Exception($"[{nameof(ExGwServiceGatewayConfig)}]({nameof(UpdateFromExHubGatewayInfos)}): Can not update Config");
            }

            DbId = update.DbId;
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}