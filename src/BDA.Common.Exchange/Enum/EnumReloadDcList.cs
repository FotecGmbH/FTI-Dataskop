// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Client anweisen eine Dc-Liste komplett neu zu landen</para>
    ///     Klasse EnumReloadDcList. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumReloadDcList
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        ExGlobalConfig,
        ExMeasurementDefinition,
        ExIotDevice,
        ExProject,
        ExCompanyUsers,
        ExGateways
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}