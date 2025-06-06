﻿// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace WebExchange
{
    /// <summary>
    ///     <para>Wer hat den TiggerAgent "getriggert"?</para>
    ///     Klasse EnumTriggerSources. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumTriggerSources
    {
        /// <summary>
        ///     Von einem "Drittsystem"
        /// </summary>
        ServiceComBase,

        /// <summary>
        ///     Von der BDA Backend App
        /// </summary>
        ServiceAppConnectivity,

        /// <summary>
        ///     Von einem Gateway
        /// </summary>
        GatewayService
    }
}