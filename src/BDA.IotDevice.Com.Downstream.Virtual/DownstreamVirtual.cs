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
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Downstream.Base;

namespace BDA.IotDevice.Com.Downstream.Virtual
{
    /// <summary>
    ///     <para>Messwertanbindung für "Virtuelle - zufällige" Messwerte</para>
    ///     Klasse DownstreamVirtual. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class DownstreamVirtual : DownstreamBase
    {
        private readonly Random _random;
        private readonly GcBaseConverter<GcDownstreamVirtualBase> _virtualConfig;

        /// <summary>
        ///     Messwertanbindung für "DotNet" Messwerte
        /// </summary>
        /// <param name="transferMethod">Erfasste Daten weiter geben</param>
        /// <param name="config">Konfiguration für diesen Messwert</param>
        /// <param name="stopToken">Globaler Stop Token</param>
        public DownstreamVirtual(Func<List<ExValue>, bool, Task> transferMethod, ExGwServiceMeasurementDefinitionConfig config, CancellationTokenSource stopToken) : base(transferMethod, config, stopToken)
        {
            if (Config.DownstreamType != EnumIotDeviceDownstreamTypes.Virtual)
            {
                throw new InvalidDataException($"Wrong Downstream Type {Config.DownstreamType} for {nameof(DownstreamVirtual)}");
            }

            _virtualConfig = new GcBaseConverter<GcDownstreamVirtualBase>(Config.AdditionalConfiguration);
            _random = new Random((int) DateTime.Now.Ticks);
        }

        /// <summary>
        ///     Messwert im System abfragen
        /// </summary>
        /// <returns></returns>
        protected override Task<ExValue> GetValue()
        {
            // ReSharper disable once RedundantAssignment
            ExValue r = null!;

            switch (_virtualConfig.Base.VirtualOpcodeType)
            {
                case EnumIotDeviceVirtualMeasurementTypes.Float:
                    r = ReadFloat();
                    break;
                case EnumIotDeviceVirtualMeasurementTypes.Image:
                    throw new NotImplementedException();
                case EnumIotDeviceVirtualMeasurementTypes.Text:
                    throw new NotImplementedException();
                case EnumIotDeviceVirtualMeasurementTypes.Data:
                    throw new NotImplementedException();
                case EnumIotDeviceVirtualMeasurementTypes.None:
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
        ///     Zufälliger Float in gewissen Grenzen
        /// </summary>
        /// <returns></returns>
        private ExValue ReadFloat()
        {
            var config = _virtualConfig.ConvertTo<GcDownstreamVirtualFloat>();
#pragma warning disable CA5394 // Do not use insecure randomness
            var val = (double) (_random.NextSingle() * (config.Max - config.Min) + config.Min);
#pragma warning restore CA5394 // Do not use insecure randomness
            var pos = GetLatLongAlt(_virtualConfig.Base);
            val = Math.Round(val, 2);
            return (new ExValue
            {
                Identifier = Config.DbId,
                MeasurementNumber = val,
                TimeStamp = DateTime.Now,
                ValueType = EnumValueTypes.Number,
                MeasurementRaw = BitConverter.GetBytes(val),
                Position = new()
                {
                    Latitude = pos.Lat,
                    Longitude = pos.Lon,
                    Altitude = pos.Alt
                }
            });
        }

        /// <summary>
        ///     Erzeuge zufällige Koordinaten in einem gewissen Umkreis und zufälliger Höhe
        /// </summary>
        /// <param name="baseInfos"></param>
        /// <returns></returns>
        private (float Lat, float Lon, float Alt) GetLatLongAlt(GcDownstreamVirtualBase baseInfos)
        {
            // Convert radius from meters to degrees
            var radiusInDegrees = baseInfos.AreaRadius / 111000f;

#pragma warning disable CA5394 // Do not use insecure randomness
            var u = _random.NextSingle();
            var v = _random.NextSingle();
#pragma warning restore CA5394 // Do not use insecure randomness
            var w = radiusInDegrees * MathF.Sqrt(u);
            var t = 2 * MathF.PI * v;
            var x = w * MathF.Cos(t);
            var y = w * MathF.Sin(t);

            // Adjust the x-coordinate for the shrinking of the east-west distances
            var newX = x / MathF.Cos((MathF.PI / 180) * baseInfos.AreaLatitude);

            var foundLongitude = newX + baseInfos.AreaLogitute;
            var foundLatitude = y + baseInfos.AreaLatitude;

#pragma warning disable CA5394 // Do not use insecure randomness
            var alt = (_random.NextSingle() * 5000);
#pragma warning restore CA5394 // Do not use insecure randomness
            alt = (float) Math.Round(alt, 2);

            return (foundLatitude, foundLongitude, alt);
        }
    }
}