// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Zeitspanne von der die historischen Daten heruntergeladen werden sollen</para>
    ///     Klasse EnumOpensenseDataTimeframe. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumOpensenseDataTimeframe
    {
        /// <summary>
        ///     Ein Tag
        /// </summary>
        Day = 1,

        /// <summary>
        ///     Eine Woche
        /// </summary>
        Week = 7,

        /// <summary>
        ///     Zwei Wochen
        /// </summary>
        TwoWeeks = 14,

        /// <summary>
        ///     Ein Monat
        /// </summary>
        Month = 31,

        /// <summary>
        ///     Zwei Monate
        /// </summary>
        TwoMonths = 62,

        /// <summary>
        ///     Halbes Jahr
        /// </summary>
        HalfYear = 183,

        /// <summary>
        ///     Ein Jahr
        /// </summary>
        Year = 365,

        /// <summary>
        ///     Zwei Jahre
        /// </summary>
        TwoYears = 730
    }
}