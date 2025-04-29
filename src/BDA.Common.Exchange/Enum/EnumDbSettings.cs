// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Allgemeine Datenbank Settings für das Projekt</para>
    ///     Klasse EnumDbSettings. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDbSettings
    {
        /// <summary>
        ///     Details zu den AGB's
        /// </summary>
        Agb = 0,

        /// <summary>
        ///     Aktuelle Version in den Stores
        /// </summary>
        CurrentAppVersion = 1,

        /// <summary>
        ///     Minimale App die auf den Clients sein muss
        /// </summary>
        MinAppVersion = 2,

        /// <summary>
        ///     Allgemeine Meldung für die Clients (wird einmal beim App-Start angezeigt)
        /// </summary>
        CommonMessage = 3,

        /// <summary>
        ///     Konfig App Windows Download Link
        /// </summary>
        ConfigAppWindows = 4,

        /// <summary>
        ///     Gateway App Windows Download Link
        /// </summary>
        GatewayAppWindows = 5,

        /// <summary>
        ///     Gateway App Linux Download Link
        /// </summary>
        GatewayAppLinux = 6,

        /// <summary>
        ///     Sensor Quellcode Donwload Link für FipiTtn
        /// </summary>
        SensorTemplateFipiTtn = 7,

        /// <summary>
        ///     GRPC-Protofiles Download Link
        /// </summary>
        InterfaceGrpc = 8,

        /// <summary>
        ///     Konfig App Web Link
        /// </summary>
        ConfigAppWeb = 9
    }
}