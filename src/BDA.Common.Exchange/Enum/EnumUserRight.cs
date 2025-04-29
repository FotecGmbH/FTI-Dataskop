// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Benutzerrechte in einer einzelnen Firma</para>
    ///     Klasse EnumUserRight. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumUserRight
    {
        /// <summary>
        ///     Leserechte
        /// </summary>
        Read = 0,

        /// <summary>
        ///     Lese und Schreibrechte (bei Firmen-Admins immer)
        /// </summary>
        ReadWrite = 1
    }
}