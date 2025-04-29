// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Configs.Enums
{
    /// <summary>
    ///     <para>Schwellenwerte, z.b. Ueberschreitun, Unterschreitung, Delta</para>
    ///     Klasse EnumThresholdType. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumThresholdType
    {
        /// <summary>
        ///     Ueberschreitung
        /// </summary>
        Exceed,

        /// <summary>
        ///     Unterschreitung
        /// </summary>
        FallBelow,

        /// <summary>
        ///     Delta
        /// </summary>
        Delta,

        /// <summary>
        ///     None
        /// </summary>
        None
    }
}