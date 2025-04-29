// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Downstream.Base;

namespace BDA.IotDevice.Com.Downstream.DotNet
{
    /// <summary>
    ///     <para>Messwertanbindung für "DotNet" Messwerte</para>
    ///     Klasse DownstreamDotNet. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class DownstreamDotNet : DownstreamBase
    {
        private readonly GcBaseConverter<GcDownstreamDotNet> _dotnetConfig = null!;
        private readonly PerformanceCounter? _pcCpuWindows;
        private readonly PerformanceCounter? _pcMemoryWindows;

        /// <summary>
        ///     Messwertanbindung für "DotNet" Messwerte
        /// </summary>
        /// <param name="transferMethod">Erfasste Daten weiter geben</param>
        /// <param name="config">Konfiguration für diesen Messwert</param>
        /// <param name="stopToken">Globaler Stop Token</param>
        public DownstreamDotNet(Func<List<ExValue>, bool, Task> transferMethod, ExGwServiceMeasurementDefinitionConfig config, CancellationTokenSource stopToken) : base(transferMethod, config, stopToken)
        {
            if (Config.DownstreamType != EnumIotDeviceDownstreamTypes.DotNet)
            {
                throw new InvalidDataException($"Wrong Downstream Type {Config.DownstreamType} for {nameof(DownstreamDotNet)}");
            }

            _dotnetConfig = new GcBaseConverter<GcDownstreamDotNet>(Config.AdditionalConfiguration);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _pcCpuWindows = new PerformanceCounter
                {
                    CategoryName = "Processor",
                    CounterName = "% Processor Time",
                    InstanceName = "_Total"
                };
                _pcMemoryWindows = new PerformanceCounter
                {
                    CategoryName = "Memory",
                    CounterName = "% Committed Bytes In Use",
                };
            }
        }

        /// <summary>
        ///     Messwert im System abfragen
        /// </summary>
        /// <returns></returns>
        protected override Task<ExValue> GetValue()
        {
            // ReSharper disable once RedundantAssignment
            ExValue r = null!;

            switch (_dotnetConfig.Base.DotNetMeasurementType)
            {
                case EnumIotDeviceDotNetMeasurementTypes.Memory:
                    r = ReadMemory();
                    break;
                case EnumIotDeviceDotNetMeasurementTypes.CpuUsagePercent:
                    r = ReadCpu();
                    break;
                case EnumIotDeviceDotNetMeasurementTypes.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (r == null!)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw new NullReferenceException();
            }

            return Task.FromResult(r);
        }

        /// <summary>
        ///     Speicher Auslastung lesen
        ///     ToDo: Implementierung für Linux und MacOs
        /// </summary>
        /// <returns></returns>
        private ExValue ReadMemory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ReadPerformanceCounterWindows(_pcMemoryWindows!);
            }

            return new ExValue
            {
                Identifier = Config.DbId,
                MeasurementNumber = -1,
                TimeStamp = DateTime.Now,
                ValueType = EnumValueTypes.Number,
                Position = null
            };
        }

        /// <summary>
        ///     CPU Auslastung lesen
        ///     ToDo: Implementierung für Linux und MacOs
        /// </summary>
        /// <returns></returns>
        private ExValue ReadCpu()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ReadPerformanceCounterWindows(_pcCpuWindows!);
            }

            return new ExValue
            {
                Identifier = Config.DbId,
                MeasurementNumber = -1,
                TimeStamp = DateTime.Now,
                ValueType = EnumValueTypes.Number,
                Position = null
            };
        }

        /// <summary>
        ///     In Windows lesen von Daten über den Perfomance Counter
        /// </summary>
        /// <returns></returns>
        private ExValue ReadPerformanceCounterWindows(PerformanceCounter pc)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var val = (double) pc.NextValue();
            if (val == 0)
            {
                val = pc.NextValue();
            }

#pragma warning restore CA1416 // Validate platform compatibility
            val = Math.Round(val, 2);

            return new ExValue
            {
                Identifier = Config.DbId,
                MeasurementNumber = val,
                TimeStamp = DateTime.Now,
                ValueType = EnumValueTypes.Number,
                MeasurementRaw = BitConverter.GetBytes(val),
                Position = null
            };
        }
    }
}