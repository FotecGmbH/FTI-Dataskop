// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Möchliche Firmentypen</para>
    ///     Klasse EnumCompanyTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumCompanyTypes
    {
        /// <summary>
        ///     Spezielle Firma (nur 1x in Db) in welcher Gateways "geparkt" werden
        /// </summary>
        NoCompany,

        /// <summary>
        ///     Diese Firma kann jeder User (auch User der App die keinen Account besitzen "Gäste") sehen
        /// </summary>
        PublicCompany,

        /// <summary>
        ///     "Normale" Firma
        /// </summary>
        Company
    }
}