// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Timers;
using BDA.Common.Exchange.Configs.Attributes.Value;
using BDA.Common.Exchange.Configs.Downstreams;
using BDA.Common.Exchange.Configs.Downstreams.Custom;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Downstreams.Esp32;
using BDA.Common.Exchange.Configs.Downstreams.Meadow;
using BDA.Common.Exchange.Configs.Downstreams.OpenSense;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.UserCode;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Common.ParserCompiler;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedParameter.Local

namespace BDA.Gateway.Com.Base;

/// <summary>
///     <para>Basiskommunikation den einzelnen IoT Decives über verschiedene Protokolle</para>
///     Klasse GatewayComBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public abstract partial class GatewayComBase
{
    private readonly Timer _assemblyCleanTimer = new Timer(1000 * 60);
    private readonly TimeSpan _assemblyUnloadTimeout = TimeSpan.FromSeconds(30);
    private readonly long _gatewayId;
    private ConfigHandler<ExGwServiceIotDeviceConfig>? _configHandlerIotDevice;
    private AssemblyLoadContext? _ctx;
    private Assembly? _decodingAssembly;
    private DateTime _lastUsedAssembly;

    /// <summary>
    ///     Neue Config für ein Iot Gerät - nur dann nicht null wenn das Gerät die neue Config noch nicht bestätigt hat!
    /// </summary>
    protected ExGwServiceIotDeviceConfig? NewIotConfig;

    #region Nested Types

    /// <summary>
    ///     LocationData
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#pragma warning disable CA1034 // Nested types should not be visible
    public class LocationData
#pragma warning restore CA1034 // Nested types should not be visible
    {
        #region Properties

        /// <summary>
        ///     Latitude
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        ///     Longitude
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        ///     Altitude
        /// </summary>
        public ushort Altitude { get; set; }

        #endregion
    }

    #endregion


    /// <summary>
    ///     Basiskommunikation den einzelnen IoT Decives über verschiedene Protokolle
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <param name="iotConfig"></param>
    protected GatewayComBase(long gatewayId, ExGwServiceIotDeviceConfig iotConfig)
    {
        NewConfigForGateway += (sender, args) => UnloadAssembly();
        if (iotConfig == null!)
        {
            throw new ArgumentNullException($"[{nameof(GatewayComBase)}]({nameof(GatewayComBase)}): {nameof(iotConfig)} is null");
        }

        _gatewayId = gatewayId;

        if (IotConfig == null!)
        {
            IotConfig = iotConfig;
        }

        //Püfen ob es eine Lokale Config gibt (die eigentlich das IoT Gerät hat) und das Gateway eigentlich eine neuere hätte
        if (CheckLocalConfigHandler(iotConfig))
        {
            if (_configHandlerIotDevice!.Exist)
            {
                IotConfig = _configHandlerIotDevice.ReadConfig();
                NewIotConfig = iotConfig;
            }
        }

        _assemblyCleanTimer.Elapsed += (sender, args) =>
        {
            var timesinceLastUse = DateTime.Now.Subtract(_lastUsedAssembly);
            if (timesinceLastUse < _assemblyUnloadTimeout)
            {
                return;
            }

            UnloadAssembly();
            Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(GatewayComBase)}): Unloading Assembly for device {IotConfig.Name} because i didn't use it for {Math.Round(timesinceLastUse.TotalMinutes, 2)} minutes");
        };

        _assemblyCleanTimer.AutoReset = true;
    }

    #region Properties

    /// <summary>
    ///     Aktuelle Config vom BDA Service - mit dieser wird "gearbeitet"
    /// </summary>
    public ExGwServiceIotDeviceConfig IotConfig { get; protected set; } = null!;

    /// <summary>
    ///     Ist das Gerät gerade aktiv verbunden?
    /// </summary>
    public bool IsConnected { get; set; }

    #endregion

    /// <summary>
    ///     Konvertiert den Gateway Config in eine State-Machine für die C-basierenden IoT Devices (ESP32 und Arduino)
    /// </summary>
    /// <returns></returns>
    public byte[] ToStateMachineEmbedded(ExGwServiceIotDeviceConfig config)
    {
        if (config == null!)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var result = new List<byte>();

        result.Add((byte) config.TransmissionTime);
        result.AddRange(BitConverter.GetBytes(config.TransmissionInterval));
        result.AddRange(BitConverter.GetBytes(config.MeasurementInterval));
        result.AddRange(BitConverter.GetBytes((UInt16) config.ConfigVersionService));

        for (var i = 0; i < config.MeasurementDefinition.Count; i++)
        {
            var dsType = config.MeasurementDefinition[i].DownstreamType;

            // Überspringe MeasurementDefinitions mit downstreamtype None (bsp. Devaddr wird vom Gateway gesetzt)
            if (dsType == EnumIotDeviceDownstreamTypes.None)
            {
                continue;
            }

            var ch = new GcBaseConverter<GcDownstreamBase>(config.MeasurementDefinition[i].AdditionalConfiguration);

            // ReSharper disable once RedundantAssignment
            byte[] opCodes = null!;
            switch (dsType)
            {
                case EnumIotDeviceDownstreamTypes.Virtual:
                    var v = ch.ConvertTo<GcDownstreamVirtualBase>();
                    if (v == null!)
                    {
                        continue;
                    }

                    switch (v.VirtualOpcodeType)
                    {
                        case EnumIotDeviceVirtualMeasurementTypes.Float:
                            opCodes = ch.ConvertTo<GcDownstreamVirtualFloat>().ToStateMachine(dsType);
                            break;
                        case EnumIotDeviceVirtualMeasurementTypes.Image:
                            throw new NotImplementedException();
                        case EnumIotDeviceVirtualMeasurementTypes.Text:
                            throw new NotImplementedException();
                        case EnumIotDeviceVirtualMeasurementTypes.Data:
                            throw new NotImplementedException();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
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
                    opCodes = ch.ConvertTo<GcDownstreamEsp32>().ToStateMachine(dsType);
                    break;
                case EnumIotDeviceDownstreamTypes.DotNet:
                    opCodes = ch.ConvertTo<GcDownstreamDotNet>().ToStateMachine(dsType);
                    break;
                case EnumIotDeviceDownstreamTypes.Custom:
                    var zz = ch.ConvertTo<GcDownstreamCustom>();
                    var ii = zz.ToStateMachine(dsType);
                    opCodes = ii; //ch.ConvertTo<GcDownstreamCustom>().ToStateMachine(dsType);
                    break;
                case EnumIotDeviceDownstreamTypes.OpenSense:
                    opCodes = ch.ConvertTo<GcDownstreamOpenSense>().ToStateMachine(dsType);
                    break;
                case EnumIotDeviceDownstreamTypes.Meadow:
                    opCodes = ch.ConvertTo<GcDownstreamMeadow>().ToStateMachine(dsType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (opCodes != null! && opCodes.Length > 0)
            {
                result.AddRange(opCodes);
            }
        }

        var r = result.ToArray();
        Logging.Log.LogTrace($"[{nameof(GatewayComBase)}]({nameof(ToStateMachineEmbedded)}): OpCodes: {GcByteHelper.BytesToHexString(r)}");
        return r;
    }

    /// <summary>
    ///     Neue Config an Iot Gerät senden
    /// </summary>
    /// <param name="iotDeviceConfig"></param>
    /// <param name="resend">Opcode wird nicht verglichen und einfach nocheinmal geschickt.</param>
    /// <returns></returns>
    public async Task<bool> UpdateIotDeviceConfig(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resend = false)
    {
        if (iotDeviceConfig == null!)
        {
            throw new ArgumentNullException($"[{nameof(GatewayComBase)}]({nameof(UpdateIotDeviceConfig)}): {nameof(iotDeviceConfig)}");
        }


        Logging.Log.LogTrace($"[{nameof(GatewayComBase)}]({nameof(UpdateIotDeviceConfig)}): New config for device name: {iotDeviceConfig.Name} dbId: {iotDeviceConfig.DbId}");
        //Config besitzt keine gultige Db Id
        if (!CheckLocalConfigHandler(iotDeviceConfig))
        {
            return true;
        }


        _configHandlerIotDevice!.StoreConfig(IotConfig);
        NewIotConfig = iotDeviceConfig;
        var transferComplete = false;

        if (iotDeviceConfig.Plattform != EnumIotDevicePlattforms.Prebuilt || iotDeviceConfig.UpStreamType == EnumIotDeviceUpstreamTypes.Tcp)
        {
            transferComplete = await TransferConfig(iotDeviceConfig, resend).ConfigureAwait(false);
        }


        if (transferComplete || iotDeviceConfig.Plattform == EnumIotDevicePlattforms.Prebuilt)
        {
            transferComplete = true;
            IotConfig = iotDeviceConfig;
            NewIotConfig = null;
            _configHandlerIotDevice.Delete();
        }

        return transferComplete;
    }

    /// <summary>
    ///     Neue Rohdaten vom Iot Gerät im Format:
    ///     EINMAL:
    ///     MEASUREMENTS (byte) ein Byte, das angiebt wie oft die Messungen durchgeführt wurden.
    ///     MEHRFACH:
    ///     GPSAVAILABLE (byte) ein Byte, das angibt ob am Ende des DATA arrays noch GPSKoordinaten angefügt wurden.
    ///     DATA (byte[]) Messwertdefinitionen Anzahl und Typ weiß das Gateway durch die ConfigRawValueDefinition in der
    ///     Additional Configuration
    ///     Timstamp (long 4 Byte)
    ///     Optional wenn Gerät Position hat noch:
    ///     Lat (float 4byte), Long (float 4byte), Alt (uint16),
    /// </summary>
    /// <param name="data"></param>
    /// <param name="deviceId">The device ID of the sending Device</param>
    /// <param name="customCode">Usercode der zum parsen genutzt werden soll</param>
    /// <param name="devaddr"></param>
    /// <returns></returns>
    public async Task NewValues(byte[] data, long deviceId, ExUsercode? customCode = null, string? devaddr = null)
    {
        if (data == null!)
        {
            throw new ArgumentNullException($"[{nameof(GatewayComBase)}]({nameof(NewValues)}): {nameof(data)}");
        }

        Logging.Log.LogTrace($"[{nameof(GatewayComBase)}]({nameof(NewValues)}): RawData: {GcByteHelper.BytesToHexString(data)}");


        // Es handelt sich um ein Gerät von uns (FIPY...)
        if (customCode is null)
        {
            // Alle values
            var vals = new List<ExValue>();

            // Die values eines Messungszyklus
            var cycleVals = new List<ExValue>();

            var startIndex = 0;

            // Wieviele Messungszyklen wurden durchgeführt
            var measurementCycles = data[startIndex++];


            for (var j = 0; j < measurementCycles; j++)
            {
                cycleVals.Clear();

                // Gps hängt am ende der Daten.
                var gpsAvailable = Convert.ToBoolean(data[startIndex++]);

                for (var i = 0; i < IotConfig.MeasurementDefinition.Count; i++)
                {
                    var dsType = IotConfig.MeasurementDefinition[i].DownstreamType;

                    if (dsType == EnumIotDeviceDownstreamTypes.None)
                    {
                        continue;
                    }

                    var ch = new GcBaseConverter<GcDownstreamBase>(IotConfig.MeasurementDefinition[i].AdditionalConfiguration);
                    var def = ch.Base.RawValueDefinition;

                    if (def.ByteCount == 0)
                    {
                        continue;
                    }

                    var tmpBytes = new byte[def.ByteCount];
                    Array.Copy(data, startIndex, tmpBytes, 0, def.ByteCount);
                    var v = ParseAndConvertValue(tmpBytes, def);
                    if (v != null)
                    {
                        v.Identifier = IotConfig.MeasurementDefinition[i].DbId;
                        v.ValueType = IotConfig.MeasurementDefinition[i].ValueType;
                        cycleVals.Add(v);
                    }

                    startIndex += def.ByteCount;
                }


                // Extrahieren des Timestamps am Ende der Daten
                var timeBytes = new byte[4];
                Array.Copy(data, startIndex, timeBytes, 0, 4);

                var timestamp = DateTime.UnixEpoch.AddSeconds(BitConverter.ToUInt32(timeBytes));
                startIndex += 4;


                ExPosition? position = null;

                // Wenn Positionsdaten verfügbar sind extrahieren der Positionsdaten.
                if (gpsAvailable)
                {
                    position = ExtractLocation(data, startIndex);
                    startIndex += Marshal.SizeOf(typeof(LocationData));
                }

                var devaddrdefinition = IotConfig.MeasurementDefinition.FirstOrDefault(def => def.Name == "DevAddr");
                if (devaddrdefinition is not null && devaddr is not null)
                {
                    cycleVals.Add(new ExValue
                    {
                        Identifier = devaddrdefinition.DbId,
                        ValueType = devaddrdefinition.ValueType,
                        Position = position,
                        MeasurementText = devaddr
                    });
                }

                foreach (var val in cycleVals)
                {
                    if (timestamp < DateTime.Now - TimeSpan.FromDays(800))
                    {
                        val.TimeStamp = DateTime.Now;
                    }
                    else
                    {
                        val.TimeStamp = timestamp;
                    }

                    val.Position = position;

                    vals.Add(val);
                }
            }

            await NewValues(vals).ConfigureAwait(false);
            return;
        }

        _assemblyCleanTimer.Stop();

        if (_decodingAssembly is null)
        {
            // Es handelt sich um ein third Party gerät. Custom code zum parsen wurde mitgegeben
            using var stream = await Compiler.GetAssembly(customCode.CompleteCode).ConfigureAwait(false);
            _ctx = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
            _decodingAssembly = _ctx.LoadFromStream(stream);
            Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(NewValues)}): Loaded new assembly for device {deviceId}");
        }
        else
        {
            Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(NewValues)}): Reusing assembly for device {deviceId}");
        }

        _lastUsedAssembly = DateTime.Now;

        _assemblyCleanTimer.Start();

        var type = _decodingAssembly.GetTypes().FirstOrDefault(t => t.Name == "DataConverter");
        var methods = type?.GetMethods().FirstOrDefault(m => m.Name == "Convert");
        var obj = methods?.Invoke(null, new object[] {data});

        List<ExValue> resultlist = new List<ExValue>();

        if (obj as ExValue[] != null)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            resultlist = (obj as ExValue[]).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }
        // ReSharper disable once SafeCastIsUsedAsTypeCheck
        else if (obj as List<ExValue> != null)

        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            resultlist = obj as List<ExValue>;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        else if (obj is null)
        {
            return;
        }

        //var resObj = methods?.Invoke(null, new object[] { data }) as ExValue[];

        //if (resObj is null) return;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        foreach (var val in resultlist)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (val.TimeStamp == new DateTime() || val.TimeStamp == null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                val.TimeStamp = DateTime.Now;
            }
        }

        //var resultlist = resObj.ToList();

        var devaddrdefinitionThirdparty = IotConfig.MeasurementDefinition.FirstOrDefault(def => def.Name == "DevAddr");
        if (devaddrdefinitionThirdparty is not null)
        {
            var devaddrRes = resultlist.FirstOrDefault(r => r.Identifier == devaddrdefinitionThirdparty.DbId);

            if (devaddrRes is null)
            {
                resultlist.Add(new ExValue
                {
                    Identifier = devaddrdefinitionThirdparty.DbId,
                    ValueType = devaddrdefinitionThirdparty.ValueType,
                    MeasurementText = devaddr
                });
            }

            if (devaddrRes != null && devaddr is not null)
            {
                devaddrRes.Identifier = devaddrdefinitionThirdparty.DbId;
                devaddrRes.ValueType = devaddrdefinitionThirdparty.ValueType;
                devaddrRes.TimeStamp = DateTime.Now;
                devaddrRes.MeasurementText = devaddr;
            }
        }


        resultlist.RemoveAll(r => !r.HasValue());

        await NewValues(resultlist).ConfigureAwait(false);
    }

    /// <summary>
    ///     Neue Werte vom IoT Gerät - Weiterleitung an BDA Service
    /// </summary>
    /// <param name="data">Aufbereitete Daten</param>
    /// <returns></returns>
    public async Task NewValues(List<ExValue> data)
    {
        if (data == null! || data.Count == 0)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(NewValues)}): No Values to send!");
            return;
        }

        foreach (var value in data)
        {
            if (value.Position == null!)
            {
                value.Position = IotConfig.FallbackPosition;
            }
        }

        await TransferToBdaServiceMesurements(new ExValuesTransfer
        {
            GatewayId = _gatewayId,
            Mesurements = data
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Unloading the current Decodingassembly
    /// </summary>
    protected void UnloadAssembly()
    {
        if (_ctx == null)
        {
            return;
        }

        _ctx.Unload();
        _ctx = null;
        _decodingAssembly = null;
        _assemblyCleanTimer.Stop();
    }

    /// <summary>
    ///     Gerät hat sich mit dem Gateway verbunden
    /// </summary>
    /// <param name="deviceInfos"></param>
    protected void Connected(ExIotDeviceInfos deviceInfos)
    {
        IsConnected = true;
    }

    /// <summary>
    ///     Gerät ist nicht mehr verbunden
    /// </summary>
    protected void Disconnected()
    {
        IsConnected = false;
    }

    /// <summary>
    ///     Aktuellen Status an Gateway
    /// </summary>
    /// <param name="state"></param>
    /// <param name="firmwareVersion"></param>
    /// <returns></returns>
    protected async Task UpdateIotDeviceState(EnumDeviceOnlineState state, string firmwareVersion)
    {
        await UpdateIotDeviceState(new ExHubIotDeviceState
        {
            ChangeDateTime = DateTime.UtcNow,
            ConfigVersion = IotConfig.ConfigVersionService,
            FirmwareVersion = firmwareVersion,
            IotDeviceId = IotConfig.DbId,
            State = state
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Neue Konfig an ein bestimmtes Iot - Gerät übertragen
    /// </summary>
    /// <param name="iotDeviceConfig"></param>
    /// <param name="resend">Opcode wird nicht verglichen und einfach nocheinmal geschickt.</param>
    /// <returns></returns>
    protected abstract Task<bool> TransferConfig(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resend = false);

    /// <summary>
    ///     Gibt es lokal eine aktuelle Konfig des Iot Geräts?
    ///     Diese gibt es nur wenn eine neue Konfig (liegt in der config des Gateway) noch nicht (erfolgreich) an das Iot Gerät
    ///     übermittelt werden konnte.
    /// </summary>
    /// <returns></returns>
    private bool CheckLocalConfigHandler(ExGwServiceIotDeviceConfig iotDeviceConfig)
    {
        Logging.Log.LogTrace($"[{nameof(GatewayComBase)}]({nameof(CheckLocalConfigHandler)}): Check if local config for device name: {iotDeviceConfig.Name} dbId: {iotDeviceConfig.DbId}");

        if (iotDeviceConfig == null!)
        {
            return false;
        }

        if (iotDeviceConfig.DbId <= 0)
        {
            return false;
        }

        if (_configHandlerIotDevice != null)
        {
            return true;
        }

        var f = new FileInfo(Path.Combine(LocalDirectory.FullName, $"configIot{IotConfig.DbId}.json"));
        _configHandlerIotDevice = new ConfigHandler<ExGwServiceIotDeviceConfig>(f);
        return true;
    }

    /// <summary>
    ///     Wert (Rohdaten) konvertieren
    /// </summary>
    /// <param name="data"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private ExValue? ParseAndConvertValue(byte[] data, ConfigRawValueDefinition def)
    {
        if (data == null! || data.Length <= 0)
        {
            throw new ArgumentException(nameof(data));
        }

        var r = new ExValue
        {
            MeasurementRaw = data
        };

        try
        {
            switch (def.RawValueType)
            {
                case EnumRawValueTypes.Custom:
                    return null;
                case EnumRawValueTypes.Bit:
                    r.MeasurementBool = data.Any(a => a != 0);
                    r.MeasurementNumber = r.MeasurementBool.Value ? 1 : 0;
                    r.ValueType = EnumValueTypes.Bit;
                    break;
                case EnumRawValueTypes.Float:
                    r.MeasurementNumber = BitConverter.ToSingle(data);
                    break;
                case EnumRawValueTypes.Double:
                    r.MeasurementNumber = BitConverter.ToDouble(data);
                    break;
                case EnumRawValueTypes.Int16:
                    r.MeasurementNumber = BitConverter.ToInt16(data);
                    break;
                case EnumRawValueTypes.UInt16:
                    r.MeasurementNumber = BitConverter.ToUInt16(data);
                    break;
                case EnumRawValueTypes.Int32:
                    r.MeasurementNumber = BitConverter.ToInt32(data);
                    break;
                case EnumRawValueTypes.UInt32:
                    r.MeasurementNumber = BitConverter.ToUInt32(data);
                    break;
                case EnumRawValueTypes.Int64:
                    r.MeasurementNumber = BitConverter.ToInt64(data);
                    break;
                case EnumRawValueTypes.UInt64:
                    r.MeasurementNumber = BitConverter.ToUInt64(data);
                    break;
                case EnumRawValueTypes.Byte:
                    r.MeasurementNumber = data[0];
                    break;
                case EnumRawValueTypes.ByteArray:
                    //Bereits befüllt
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(ParseAndConvertValue)}): Can not Convert {GcByteHelper.BytesToHexString(data)} to {def.RawValueType}\r\n{e}");
            return null;
        }

        return r;
    }

    private ExPosition? ExtractLocation(byte[] data, int startIndex)
    {
        // ReSharper disable once RedundantAssignment
        var ptr = IntPtr.Zero;
        var size = Marshal.SizeOf(typeof(LocationData));

        ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, startIndex, ptr, size);
        var obj = Marshal.PtrToStructure<LocationData>(ptr);

        return obj is not null ? new ExPosition {Altitude = obj.Altitude, Latitude = obj.Latitude, Longitude = obj.Longitude, Source = EnumPositionSource.Modul} : null;
    }
}