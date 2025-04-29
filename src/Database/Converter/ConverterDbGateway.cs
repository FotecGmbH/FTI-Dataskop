// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>DbPosition konvertieren</para>
///     Klasse ConverterDbPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbGateway
{
    /// <summary>
    ///     Datenbank Gateway in Gateway Config konvertieren
    /// </summary>
    /// <param name="gw"></param>
    /// <returns></returns>
    public static ExGwServiceGatewayConfig ToExGwServiceGatewayConfig(this TableGateway gw)
    {
        if (gw == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbGateway)}]({nameof(ToExGwServiceGatewayConfig)}): {nameof(gw)} is null");
        }

        var r = new ExGwServiceGatewayConfig
        {
            DbId = gw.Id,
            Description = gw.Information.Description,
            ConfigVersion = gw.DeviceCommon.ConfigversionService,
            FirmwareVersion = gw.DeviceCommon.FirmwareversionDevice,
            Name = gw.Information.Name,
            Position = gw.Position.ToExPosition(),
            Secret = gw.DeviceCommon.Secret,
            IotDevices = new()
        };

        foreach (var iotDevice in gw.TblIotDevices)
        {
            r.IotDevices.Add(iotDevice.ToExGwServiceIotDeviceConfig());
        }

        return r;
    }

    /// <summary>
    ///     TblGateway zu ExGateway
    /// </summary>
    /// <param name="tblGateway"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExGateway ToExGateway(this TableGateway tblGateway)
    {
        if (tblGateway == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbGateway)}]({nameof(ToExGateway)}): {nameof(tblGateway)} is null");
        }

        var exGateWay = new ExGateway
        {
            Id = tblGateway.Id,
            AdditionalProperties = tblGateway.AdditionalProperties,
            AdditionalConfiguration = tblGateway.AdditionalConfiguration,
            CompanyId = tblGateway.TblCompanyId,
            DeviceCommon = tblGateway.DeviceCommon.ToExCommonInfo(),
            Information = tblGateway.Information.ToExInformation(),
            Location = tblGateway.Position.ToExPosition(),
            TblCompanyId = tblGateway.TblCompanyId
        };

        //if (tblGateway.TblIotDevices != null!)
        //{
        //    foreach (var tblIotTevice in tblGateway.TblIotDevices)
        //    {
        //        exGateWay.IoTDevices.Add(tblIotTevice.ToExIoTDevice());
        //    }
        //}

        return exGateWay;
    }

    /// <summary>
    ///     Proxy Klasse in Db Tabelle
    /// </summary>
    /// <param name="gw"></param>
    /// <param name="t"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTableGateway(this ExGateway gw, TableGateway t)
    {
        if (gw == null!)
        {
            throw new ArgumentNullException(nameof(gw));
        }

        if (t == null!)
        {
            throw new ArgumentNullException(nameof(t));
        }

        t.AdditionalConfiguration = gw.AdditionalConfiguration;
        t.AdditionalProperties = gw.AdditionalProperties;
        gw.DeviceCommon.ToDbDeviceCommon(t.DeviceCommon);
        gw.Information.ToDbInformation(t.Information);
        t.Position = gw.Location.ToDbPosition();
        t.TblCompanyId = gw.CompanyId;
    }
}