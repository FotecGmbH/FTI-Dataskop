// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.ComponentModel.DataAnnotations;

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Enum für Vergleichsoperatoren</para>
    ///     Klasse EnumCompareOperator. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumCompareOperator
    {
        /// <summary>
        ///     Ist gleich
        /// </summary>
        [Display(Name = "ist gleich", ShortName = "=")]
        IsEqual,

        /// <summary>
        ///     Ist nicht gleich
        /// </summary>
        [Display(Name = "ist ungleich", ShortName = "!=")]
        IsNotEqual,

        /// <summary>
        ///     Ist größer
        /// </summary>
        [Display(Name = "ist größer", ShortName = ">")]
        IsGreater,

        /// <summary>
        ///     Ist größer oder gleich
        /// </summary>
        [Display(Name = "ist größer gleich", ShortName = ">=")]
        IsGreaterOrEqual,

        /// <summary>
        ///     Ist kleiner
        /// </summary>
        [Display(Name = "ist kleiner", ShortName = "<")]
        IsLess,

        /// <summary>
        ///     Ist kleiner oder gleich
        /// </summary>
        [Display(Name = "ist kleiner gleich", ShortName = "<=")]
        IsLessOrEqual,

        /// <summary>
        ///     Enthält
        /// </summary>
        [Display(Name = "enthält")] Contains,

        /// <summary>
        ///     enthält nicht
        /// </summary>
        [Display(Name = "enthält nicht")] NotContains
    }
}