// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Umstream Möglichkeiten</para>
    ///     Klasse EnumIotDeviceUpstreamTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIotDeviceUpstreamTypes
    {
        /// <summary>
        ///     Keine Operation
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Über serielle Schnittstelle
        /// </summary>
        Serial = 0x01,

        /// <summary>
        ///     Über TCP
        /// </summary>
        Tcp = 0x02,

        /// <summary>
        ///     Iot Device im Gateway integriert
        /// </summary>
        InGateway = 0x03,

        /// <summary>
        ///     Via TTN
        /// </summary>
        Ttn = 0x04,

        /// <summary>
        ///     Über Blauzahn
        /// </summary>
        Ble = 0x05,

        /// <summary>
        ///     Opensense
        /// </summary>
        OpenSense = 0x06,

        /// <summary>
        ///     Drei
        /// </summary>
        Drei = 0x07,

        /// <summary>
        ///     Microtronics
        /// </summary>
        Microtronics = 0x08,
    }
}