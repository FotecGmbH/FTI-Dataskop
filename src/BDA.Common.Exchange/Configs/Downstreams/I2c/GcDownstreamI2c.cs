// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Attributes;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Serialize;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.Downstreams.I2c
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse GlobalConfigI2c. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamI2c : GcDownstreamBase
    {
        #region Properties

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumIotDeviceDownstreamTypes ConfigType => EnumIotDeviceDownstreamTypes.I2C;

        /// <summary>
        ///     Wie ist der Messwert durch den (Gateway) zu interpretieren
        /// </summary>
        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public ConfigRawValueDefinition RawValueDefinition { get; set; } = new ConfigRawValueDefinition();
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        ///     Name des Messwerts
        /// </summary>
        [JsonIgnore]
        public string ValueName { get; set; } = string.Empty;

        /// <summary>
        ///     Chipname
        /// </summary>
        public string ChipName { get; set; } = String.Empty;

        /// <summary>
        ///     ToDo - byte
        /// </summary>
        [ConfigProperty("Chip Adress on I²C")]
        public string ChipAdress { get; set; } = string.Empty;

        #endregion


        /// <summary>
        ///     Byte-Array für die State-Machine
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public byte[] ToStateMachine(EnumIotDeviceDownstreamTypes configType)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Daten für UI und die Datenbank (inkl. CommonConfig)
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS0108, CS0114
        public ExMeasurementDefinition ToExMeasurementDefinition()
#pragma warning restore CS0108, CS0114
        {
            return new ExMeasurementDefinition
            {
                ValueType = EnumValueTypes.Number,
                AdditionalConfiguration = this.ToJson(),
                DownstreamType = EnumIotDeviceDownstreamTypes.I2C,
                Information = new ExInformation
                {
                    Name = ValueName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Description = string.Empty
                }
            };
        }

#pragma warning disable CS0067
#pragma warning disable CS0414
        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
#pragma warning disable CS0108, CS0114
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0108, CS0114
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}