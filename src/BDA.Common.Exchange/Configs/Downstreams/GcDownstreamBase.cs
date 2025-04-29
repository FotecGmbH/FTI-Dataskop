// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Interfaces;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.Downstreams
{
    /// <summary>
    ///     <para>Global Config - Downstream Basis</para>
    ///     Klasse GcDownstreamBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcDownstreamBase : IBissModel, IGcDownstream
    {
        #region Properties

        /// <summary>
        ///     Wie ist der Messwert durch den (Gateway) zu interpretieren
        /// </summary>
        public ConfigRawValueDefinition RawValueDefinition { get; set; } = null!;

        /// <summary>
        ///     Ob Schwellenwert fuer uerberschreitung
        /// </summary>
        public bool IfThresholdExceed { get; set; }

        /// <summary>
        ///     Schwellenwert fuer uerberschreitung
        /// </summary>
        public float ThresholdExceedValue { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer unterschreitung
        /// </summary>
        public bool IfThresholdFallBelow { get; set; }

        /// <summary>
        ///     Schwellenwert fuer unterschreitung
        /// </summary>
        public float ThresholdFallBelowValue { get; set; }

        /// <summary>
        ///     Ob Schwellenwert fuer delta
        /// </summary>
        public bool IfThresholdDelta { get; set; }

        /// <summary>
        ///     Schwellenwert fuer Delta
        /// </summary>
        public float ThresholdDeltaValue { get; set; }

        #endregion

        /// <summary>
        ///     Rohdaten für die Statemachine ohne Header
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetStateMachinePayload()
        {
            return Array.Empty<byte>();
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414


        /// <summary>
        ///     Byte-Array für die State-Machine
        ///     Doko ToDo!
        /// </summary>
        /// <returns></returns>
        public byte[] ToStateMachine(EnumIotDeviceDownstreamTypes configType)
        {
            var payLoad = GetStateMachinePayload();


            var thresholds = new List<(EnumThresholdType, double)>();
            if (IfThresholdExceed)
            {
                thresholds.Add((EnumThresholdType.Exceed, ThresholdExceedValue));
            }

            if (IfThresholdFallBelow)
            {
                thresholds.Add((EnumThresholdType.FallBelow, ThresholdFallBelowValue));
            }

            if (IfThresholdDelta)
            {
                thresholds.Add((EnumThresholdType.Delta, ThresholdDeltaValue));
            }

            var threshholdBytes = new byte[thresholds.Count * 5];
            for (var i = 0; i < thresholds.Count; i++)
            {
                threshholdBytes[i * 5] = (byte) thresholds.ElementAt(i).Item1; //type

                Array.Copy(BitConverter.GetBytes((float) thresholds.ElementAt(i).Item2), 0, threshholdBytes, (i * 5) + 1, 4); //vlaue
            }

            var result = new byte[payLoad.Length + threshholdBytes.Length + 3];

            var indexCounter = 0;

            //DSC	CT	OCT	TCT	TRT	TRV	D
            result[indexCounter++] = (byte) configType;
            result[indexCounter++] = (byte) (payLoad.Length + threshholdBytes.Length + 1);
            if (payLoad.Length > 0)
            {
                Array.Copy(payLoad, 0, result, 2, 1);
                indexCounter++;
                result[indexCounter++] = (byte) thresholds.Count;
            }
            else
            {
                result[indexCounter++] = (byte) thresholds.Count;
            }

            Array.Copy(threshholdBytes, 0, result, indexCounter, threshholdBytes.Length);
            if (payLoad.Length > 1)
            {
                Array.Copy(payLoad, 1, result, indexCounter + threshholdBytes.Length, payLoad.Length - 1);
            }

            return result;
        }

        /// <summary>
        ///     Daten für UI und die Datenbank (inkl. CommonConfig)
        /// </summary>
        /// <returns></returns>
        public virtual ExMeasurementDefinition ToExMeasurementDefinition()
        {
            throw new NotSupportedException("Must be implemented in Child-Class");
        }

        #endregion
    }
}