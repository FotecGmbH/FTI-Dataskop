// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Status eines Gateways oder Iot Geräts</para>
    ///     Klasse EnumDeviceOnlineState. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDeviceOnlineState
    {
        /// <summary>
        ///     Gerät wurde konfiguriert aber die Konfiguration (noch nie) einem physischen Gerät zugewiesen
        /// </summary>
        Unknown,

        /// <summary>
        ///     Gerade online
        /// </summary>
        Online,

        /// <summary>
        ///     Gerade offline
        /// </summary>
        Offline
    }
}