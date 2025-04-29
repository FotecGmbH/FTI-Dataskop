// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Verbindungsstatus eines IoTDevices</para>
    ///     Klasse EnumIoTDeviceConnectionStates. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumIoTDeviceConnectionStates
    {
        /// <summary>
        ///     Mit Gateway verbunden
        /// </summary>
        Connected,

        /// <summary>
        ///     Nicht verbunden mit Gateway
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Verbindung mit Gateway wird aufgebaut
        /// </summary>
        Connecting,

        /// <summary>
        ///     Verbindung mit Gateway wird beendet durch Client
        /// </summary>
        Disconecting
    }
}