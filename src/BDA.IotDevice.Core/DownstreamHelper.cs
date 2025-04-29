// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Downstream.Base;
using BDA.IotDevice.Com.Downstream.DotNet;
using BDA.IotDevice.Com.Downstream.Virtual;


namespace BDA.IotDevice.Core
{
    /// <summary>
    ///     <para>Downstream Type via laden (eventuell später via Reflection)</para>
    ///     Klasse DownstreamHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class DownstreamHelper
    {
        /// <summary>
        ///     Downstream Type via laden (eventuell später via Reflection)
        /// </summary>
        /// <param name="transferMethod"></param>
        /// <param name="config"></param>
        /// <param name="stopToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static DownstreamBase GetDownstreamInstance(Func<List<ExValue>, bool, Task> transferMethod, ExGwServiceMeasurementDefinitionConfig config, CancellationTokenSource stopToken)
        {
            if (config == null!)
            {
                throw new ArgumentNullException($"[{nameof(DownstreamHelper)}]({nameof(GetDownstreamInstance)}): {nameof(config)}");
            }

            switch (config.DownstreamType)
            {
                case EnumIotDeviceDownstreamTypes.Virtual:
                    return new DownstreamVirtual(transferMethod, config, stopToken);
                case EnumIotDeviceDownstreamTypes.I2C:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Spi:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Modbus:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Pi:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Arduino:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.Esp32:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.DotNet:
                    return new DownstreamDotNet(transferMethod, config, stopToken);
                case EnumIotDeviceDownstreamTypes.Custom:
                    throw new NotImplementedException();
                case EnumIotDeviceDownstreamTypes.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}