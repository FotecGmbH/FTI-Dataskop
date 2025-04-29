// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Serialize;

namespace BDA.Common.Exchange.Configs.Downstreams.Virtual
{
    /// <summary>
    ///     <para>Virtueller Messwertgenerator für Float</para>
    ///     Klasse ExIotDeviceCommandVirtualSensor. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ConfigName("Virtueller FLOAT")]
    public class GcDownstreamVirtualFloat : GcDownstreamVirtualBase
    {
        /// <summary>
        ///     Virtueller Messwertgenerator für Float
        /// </summary>
        public GcDownstreamVirtualFloat()
        {
            RawValueDefinition = new ConfigRawValueDefinition
            {
                RawValueType = EnumRawValueTypes.Float,
                ByteCount = 4
            };
            VirtualOpcodeType = EnumIotDeviceVirtualMeasurementTypes.Float;
        }

        #region Properties

        /// <summary>
        ///     Minimum des Bereichs in dem zufällige Werte erzeugt werden.
        /// </summary>
        [ConfigProperty("Minimum zufälliger Wert")]
        public float Min { get; set; } = 0;

        /// <summary>
        ///     Maximum des Bereichs in dem zufällige Werte erzeugt werden.
        /// </summary>
        [ConfigProperty("Maximum zufälliger Wert")]
        public float Max { get; set; } = 100;

        #endregion

        /// <summary>
        ///     Rohdaten für die Statemachine ohne Header
        /// </summary>
        /// <returns></returns>
        public override byte[] GetStateMachinePayload()
        {
            var result = new byte[9];
            // ReSharper disable once UnusedVariable
            var x = BitConverter.GetBytes(Min);

            result[0] = (byte) VirtualOpcodeType;

            Array.Copy(BitConverter.GetBytes(Min), 0, result, 1, 4);
            Array.Copy(BitConverter.GetBytes(Max), 0, result, 5, 4);

            return result;
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
                DownstreamType = EnumIotDeviceDownstreamTypes.Virtual,
                Information = new ExInformation
                {
                    Name = "Zufällige Zahl",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Description = string.Empty
                }
            };
        }
    }
}