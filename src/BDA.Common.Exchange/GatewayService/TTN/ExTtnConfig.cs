// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.GatewayService.TTN
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ExTtnConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Obsolete]
    public class ExTtnConfig
    {
        #region Properties

        /// <summary>
        ///     Der Api-Key für die ttn-applikation. Beispiel:
        ///     "NNSXS.AAWCUGYJONIFBMIJADON6GLOB65G7AFUYXZZYYI.SDOX3JFS67XFBHLZOLYWND6MI4RDWRQJXVL6KB73ACSBBA4AWFZA"
        /// </summary>
        public string ApiKey { get; set; } = "";


        /// <summary>
        ///     Die Applikationid für die ttn-applikation. Beispiel: "test-application-bda"
        /// </summary>
        public string ApplicationId { get; set; } = "";

        /// <summary>
        ///     Der TTN-Server. Beispiel: "eu1.cloud.thethings.network"
        /// </summary>
        public string Zone { get; set; } = "";

        #endregion
    }
}