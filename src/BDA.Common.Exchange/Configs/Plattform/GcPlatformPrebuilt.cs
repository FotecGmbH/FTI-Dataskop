// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;

namespace BDA.Common.Exchange.Configs.Plattform
{
    /// <summary>
    ///     <para>
    ///         Wenn die Plattform eines Iot Geräts "Arduino" ist - Welche Downstream Typen werden unterstützt und welche
    ///         "buildid" Messwert definitionen besitzt dieser?
    ///     </para>
    ///     Klasse ConfigPlattformDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcPlatformPrebuilt : ConfigPlattformBase
    {
        /// <summary>
        ///     Wenn die Plattform eines Iot Geräts "Arduino" ist?
        ///     Welche Downstream Typen werden unterstützt und welche "buildin" Messwert definitionen besitzt dieser?
        /// </summary>
        public GcPlatformPrebuilt() : base(EnumIotDevicePlattforms.Prebuilt)
        {
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Prebuilt);
            SupportedDownstreamTypes.Add(EnumIotDeviceDownstreamTypes.Custom);
        }
    }
}