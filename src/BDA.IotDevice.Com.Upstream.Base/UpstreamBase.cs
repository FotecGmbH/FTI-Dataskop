// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.IotDevice.Com.Upstream.Base
{
    /// <summary>
    ///     <para>Basis für Datentransfer vom Gateway zum Server</para>
    ///     Klasse UpstreamBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract class UpstreamBase
    {
        /// <summary>
        ///     Basis für Datentransfer vom Gateway zum Server
        /// </summary>
        /// <param name="upstreamType"></param>
        protected UpstreamBase(EnumIotDeviceUpstreamTypes upstreamType)
        {
            UpstreamType = upstreamType;
        }

        #region Properties

        /// <summary>
        ///     Wie werden die Daten transveriert zum Gateway
        /// </summary>
        public EnumIotDeviceUpstreamTypes UpstreamType { get; }

        /// <summary>
        ///     Aktuell mit Gateway verbunden
        /// </summary>
        public bool IsConnectedToGateway { get; set; }

        #endregion

        /// <summary>
        ///     Daten an den Gateway übertragen
        /// </summary>
        /// <param name="data">Daten</param>
        /// <returns>true - übertragen, false - nicht möglich</returns>
        public abstract Task<bool> TransferData(List<ExValue> data);
    }
}