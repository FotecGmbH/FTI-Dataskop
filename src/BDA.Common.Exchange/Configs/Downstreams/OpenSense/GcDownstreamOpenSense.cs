// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Serialize;

namespace BDA.Common.Exchange.Configs.Downstreams.OpenSense
{
    /// <summary>
    ///     <para>Mögliche Messwerte für OpenSense</para>
    ///     Klasse GcDownstreamOpenSense. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamOpenSense : GcDownstreamBase
    {
        /// <summary>
        ///     Möglicher Messwerte für eine eigene Funktion am Iot Gerät
        /// </summary>
        public GcDownstreamOpenSense()
        {
            RawValueDefinition = new ConfigRawValueDefinition
            {
                RawValueType = EnumRawValueTypes.ByteArray,
                ByteCount = ConfigRawValueDefinition.GetByteCount(EnumRawValueTypes.Float)
            };
        }

        #region Properties

        /// <summary>
        ///     Die Opensense-Sensorid des Messwerts
        /// </summary>
        public string SensorID { get; set; } = "";

        /// <summary>
        ///     ValueType
        /// </summary>
        public EnumValueTypes ValueType { get; set; } = EnumValueTypes.Text;

        #endregion

        /// <summary>
        ///     Daten für UI und die Datenbank (inkl. CommonConfig)
        /// </summary>
        /// <returns></returns>
        public override ExMeasurementDefinition ToExMeasurementDefinition()
        {
            return new ExMeasurementDefinition
            {
                ValueType = ValueType,
                AdditionalConfiguration = this.ToJson(),
                DownstreamType = EnumIotDeviceDownstreamTypes.Custom,
                Information = new ExInformation
                {
                    Name = "Opensense",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Description = string.Empty
                }
            };
        }
    }
}