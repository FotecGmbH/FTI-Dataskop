// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Enums;

namespace BDA.Common.Exchange.Configs.Downstreams.Virtual
{
    /// <summary>
    ///     <para>Messwertgenerator Basis</para>
    ///     Klasse ExIotDeviceCommandVirtualSensor. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamVirtualBase : GcDownstreamBase
    {
        #region Properties

        /// <summary>
        ///     Typ
        /// </summary>
        public EnumIotDeviceVirtualMeasurementTypes VirtualOpcodeType { get; set; }

        /// <summary>
        ///     Der Breitengrad des Zentrums des Kreises in dem zufällig Geokoordinaten generiert werden.
        /// </summary>
        [ConfigProperty("Lat für zufällige Position")]
        public float AreaLatitude { get; set; } = 47.8f;

        /// <summary>
        ///     Der Längengrad des Zentrums des Kreises in dem zufällig Geokoordinaten generiert werden.
        /// </summary>
        [ConfigProperty("Log für zufällige Position")]
        public float AreaLogitute { get; set; } = 16.25f;

        /// <summary>
        ///     Der Radius des Kreises in Metern in dem zufällig Geokoordinaten generiert werden.
        /// </summary>
        [ConfigProperty("Radius für zufällige Position")]
        public int AreaRadius { get; set; } = 10000;

        #endregion
    }
}