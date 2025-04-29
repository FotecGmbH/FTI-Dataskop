// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Übertragung</para>
    ///     Klasse EnumTransmission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumTransmission
    {
        /// <summary>
        ///     Sofort
        /// </summary>
        Instantly = 0,

        /// <summary>
        ///     Nach Ablauf der Zeit
        /// </summary>
        Elapsedtime = 1,

        /// <summary>
        ///     Nach Anzahl der Messungen
        /// </summary>
        NumberOfMeasurements = 2
    }
}