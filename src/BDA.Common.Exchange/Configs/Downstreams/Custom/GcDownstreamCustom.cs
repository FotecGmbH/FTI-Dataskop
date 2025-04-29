// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Serialize;

namespace BDA.Common.Exchange.Configs.Downstreams.Custom
{
    /// <summary>
    ///     <para>Möglicher Messwerte für eine eigene Funktion am Iot Gerät</para>
    ///     Klasse GlobalConfigDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamCustom : GcDownstreamBase
    {
        /// <summary>
        ///     Möglicher Messwerte für eine eigene Funktion am Iot Gerät
        /// </summary>
        public GcDownstreamCustom()
        {
            RawValueDefinition = new ConfigRawValueDefinition
            {
                RawValueType = EnumRawValueTypes.Float,
                ByteCount = ConfigRawValueDefinition.GetByteCount(EnumRawValueTypes.Float)
            };
        }

        #region Properties

        /// <summary>
        ///     StateMachineId
        /// </summary>
        public byte StateMachineId { get; set; } = 1;

        /// <summary>
        /// DataArgs
        /// </summary>
        public byte[] DataArgs { get; set; } = Array.Empty<byte>();

        /// <summary>
        ///     ValueType
        /// </summary>
        public EnumValueTypes ValueType { get; set; } = EnumValueTypes.Number;

        #endregion

        /// <summary>
        ///     Rohdaten für die Statemachine ohne Header
        /// </summary>
        /// <returns></returns>
        public override byte[] GetStateMachinePayload()
        {
            return (new[] {StateMachineId}).Concat(DataArgs).ToArray();
        }

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
                    Name = "Eigene Funktion",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Description = string.Empty
                }
            };
        }
    }
}