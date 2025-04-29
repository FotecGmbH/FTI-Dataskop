// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Common.Exchange.Configs.Interfaces
{
    /// <summary>
    ///     <para>Globale Config Downstream</para>
    ///     Interface IGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IGcDownstream
    {
        #region Properties

        /// <summary>
        ///     Wie ist der Messwert durch den (Gateway) zu interpretieren
        /// </summary>
        ConfigRawValueDefinition RawValueDefinition { get; set; }

        #endregion

        /// <summary>
        ///     Byte-Array für die State-Machine
        /// </summary>
        /// <returns></returns>
        byte[] ToStateMachine(EnumIotDeviceDownstreamTypes configType);

        /// <summary>
        ///     Daten für UI und die Datenbank (inkl. CommonConfig)
        /// </summary>
        /// <returns></returns>
        ExMeasurementDefinition ToExMeasurementDefinition();
    }
}