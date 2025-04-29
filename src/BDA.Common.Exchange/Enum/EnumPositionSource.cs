// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Quelle der ermittelten Position</para>
    ///     Klasse EnumPositionSource. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumPositionSource
    {
        /// <summary>
        ///     Standort PC (sollte die Position von einem Program ermittlet werden)
        /// </summary>
        Pc,

        /// <summary>
        ///     Internet
        /// </summary>
        Internet,

        /// <summary>
        ///     Wurde von einem Modul ermittelt
        /// </summary>
        Modul,

        /// <summary>
        ///     Stammt von Mobilfunknetz (Triangulierung Masten)
        /// </summary>
        Lbs
    }
}