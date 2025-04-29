// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Serialize;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Downstreams.Esp32
{
    /// <summary>
    ///     <para>Mögliche Messwerte für Esp32</para>
    ///     Klasse GlobalConfigDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamEsp32 : GcDownstreamBase
    {
        #region Properties

        /// <summary>
        ///     Typ des Messwerts
        /// </summary>
        public EnumIotDeviceEsp32MeasurementTypes Esp32MeasurementType { get; set; }

        /// <summary>
        ///     Name des Messwerts
        /// </summary>
        [JsonIgnore]
        public string ValueName { get; set; } = string.Empty;

        #endregion


        /// <summary>
        ///     Rohdaten für die Statemachine ohne Header
        /// </summary>
        /// <returns></returns>
        public override byte[] GetStateMachinePayload()
        {
            return new[] {(byte) Esp32MeasurementType};
        }

        /// <summary>
        ///     Daten für UI und die Datenbank (inkl. CommonConfig)
        /// </summary>
        /// <returns></returns>
        public override ExMeasurementDefinition ToExMeasurementDefinition()
        {
            return new ExMeasurementDefinition
            {
                ValueType = EnumValueTypes.Number,
                AdditionalConfiguration = this.ToJson(),
                DownstreamType = EnumIotDeviceDownstreamTypes.Esp32,
                Information = new ExInformation
                {
                    Name = ValueName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Description = string.Empty
                }
            };
        }
    }
}