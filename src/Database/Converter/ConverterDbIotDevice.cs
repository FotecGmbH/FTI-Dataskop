// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>DbPosition konvertieren</para>
///     Klasse ConverterDbPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbIotDevice
{
    /// <summary>
    ///     Datenbank Gateway in Gateway Config konvertieren
    /// </summary>
    /// <param name="iot"></param>
    /// <returns></returns>
    public static ExGwServiceIotDeviceConfig ToExGwServiceIotDeviceConfig(this TableIotDevice iot)
    {
        if (iot == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbIotDevice)}]({nameof(ToExGwServiceIotDeviceConfig)}): {nameof(iot)} is null");
        }

        var r = new ExGwServiceIotDeviceConfig
        {
            Name = iot.Information.Name,
            Secret = iot.DeviceCommon.Secret,
            AdditionalConfiguration = iot.AdditionalConfiguration,
            ConfigVersion = iot.DeviceCommon.ConfigversionDevice,
            ConfigVersionService = iot.DeviceCommon.ConfigversionService,
            DbId = iot.Id,
            FirmwareVersion = iot.DeviceCommon.FirmwareversionDevice,
            MeasurementInterval = iot.MeasurementInterval,
            TransmissionInterval = iot.TransmissionInterval,
            TransmissionTime = iot.TransmissionType,
            Plattform = iot.Plattform,
            UpStreamType = iot.Upstream,
            FallbackPosition = iot.FallbackPosition.ToExPosition(),
            MeasurementDefinition = new()
        };

        foreach (var measurementDefinition in iot.TblMeasurementDefinitions)
        {
            r.MeasurementDefinition.Add(measurementDefinition.ToExGwServiceMeasurementDefinitionConfig());
        }

        return r;
    }

    /// <summary>
    ///     TableIotDevice zu ExIoTDevice.
    /// </summary>
    /// <param name="tblIotDevice"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExIotDevice ToExIoTDevice(this TableIotDevice tblIotDevice)
    {
        if (tblIotDevice == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbIotDevice)}]({nameof(ToExIoTDevice)}): {nameof(tblIotDevice)} is null");
        }

        var exIoTDevice = new ExIotDevice
        {
            AdditionalConfiguration = tblIotDevice.AdditionalConfiguration,
            AdditionalProperties = tblIotDevice.AdditionalProperties,
            GatewayId = tblIotDevice.TblGatewayId,
            Information = tblIotDevice.Information.ToExInformation(),
            Upstream = tblIotDevice.Upstream,
            Plattform = tblIotDevice.Plattform,
            TransmissionType = tblIotDevice.TransmissionType,
            TransmissionInterval = tblIotDevice.TransmissionInterval,
            MeasurmentInterval = tblIotDevice.MeasurementInterval,
            GlobalConfigId = tblIotDevice.TblCompanyGlobalConfigId,
            CompanyId = tblIotDevice.TblGateway!.TblCompanyId,
            //DataConverterId = tblIotDevice.TblDataconverterId
        };

        if (tblIotDevice.DeviceCommon != null!)
        {
            exIoTDevice.DeviceCommon = tblIotDevice.DeviceCommon.ToExCommonInfo();
        }

        if (tblIotDevice.FallbackPosition != null!)
        {
            exIoTDevice.Location = tblIotDevice.FallbackPosition.ToExPosition();
        }

        if (tblIotDevice.Information != null!)
        {
            exIoTDevice.Information = tblIotDevice.Information.ToExInformation();
        }

        if (tblIotDevice.TblMeasurementDefinitions != null!)
        {
            foreach (var tblMd in tblIotDevice.TblMeasurementDefinitions)
            {
                exIoTDevice.MeasurementDefinitions.Add(tblMd.ToExMeasurementDefinition());
            }
        }

        return exIoTDevice;
    }


    /// <summary>
    ///     ExGlobalConfig in eine TableCompanyGlobalConfig übernehmen
    /// </summary>
    /// <param name="i">ExGlobalConfig</param>
    /// <param name="t">TableCompanyGlobalConfig</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTableIotDevice(this ExIotDevice i, TableIotDevice t)
    {
        if (i == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(TableIotDevice)}): {nameof(i)} is null");
        }

        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbInformation)}]({nameof(TableIotDevice)}): {nameof(t)} is null");
        }

        i.Information.ToDbInformation(t.Information);
        t.TblCompanyGlobalConfigId = (i.GlobalConfigId == null!) ? null! : i.GlobalConfigId;
        t.AdditionalProperties = i.AdditionalProperties;
        t.AdditionalConfiguration = i.AdditionalConfiguration;
        i.DeviceCommon.ToDbDeviceCommon(t.DeviceCommon);
        t.FallbackPosition = i.Location.ToDbPosition();
        t.MeasurementInterval = i.MeasurmentInterval;
        t.TransmissionInterval = i.TransmissionInterval;
        t.TransmissionType = i.TransmissionType;
        t.Upstream = i.Upstream;
        t.TblGatewayId = i.GatewayId;
        t.Plattform = i.Plattform;
        t.TblDataconverterId = i.DataConverterId;

        t.TblMeasurementDefinitions = new List<TableMeasurementDefinition>();

        foreach (var def in i.MeasurementDefinitions)
        {
            var tblDefinition = new TableMeasurementDefinition();
            def.ToTableMeasurementDefinition(tblDefinition);
            t.TblMeasurementDefinitions.Add(tblDefinition);
        }
    }
}