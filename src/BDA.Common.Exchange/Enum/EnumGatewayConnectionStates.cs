// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Verbindungsstatus eines Gateways</para>
    ///     Klasse EnumGatewayConnectionStates. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumGatewayConnectionStates
    {
        /// <summary>
        ///     Mit Server verbunden
        /// </summary>
        Connected,

        /// <summary>
        ///     Nicht verbunden mit Server
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Verbindung mit Server wird aufgebaut
        /// </summary>
        Connecting,

        /// <summary>
        ///     Verbindung mit Server wird beendet durch Client
        /// </summary>
        Disconecting
    }
}