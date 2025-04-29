// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using System.Linq;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>ConverterDbMeasurementDefinition</para>
///     Klasse ConverterDbMeasurementDefinition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbMeasurementDefinition
{
    /// <summary>
    ///     TableMeasurementDefinition nach ExGwServiceMeasurementDefinitionConfig
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExGwServiceMeasurementDefinitionConfig ToExGwServiceMeasurementDefinitionConfig(this TableMeasurementDefinition m)
    {
        if (m == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinition)}]({nameof(ToExGwServiceMeasurementDefinitionConfig)}): {nameof(m)} is null");
        }

        var r = new ExGwServiceMeasurementDefinitionConfig
        {
            Name = m.Information.Name,
            AdditionalConfiguration = m.AdditionalConfiguration,
            DbId = m.Id,
            ValueType = m.ValueType,
            MeasurementInterval = m.MeasurementInterval,
            DownstreamType = m.DownstreamType
        };
        return r;
    }

    /// <summary>
    ///     ExMeasurementDefinition nach TableMeasurementDefinition
    /// </summary>
    /// <param name="m"></param>
    /// <param name="t"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTableMeasurementDefinition(this ExMeasurementDefinition m, TableMeasurementDefinition t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinition)}]({nameof(ToTableMeasurementDefinition)}): {nameof(t)}");
        }

        if (m == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinition)}]({nameof(ToTableMeasurementDefinition)}): {nameof(m)}");
        }

        m.Information.ToDbInformation(t.Information);
        t.TblIotDeviceId = m.IotDeviceId;
        t.AdditionalConfiguration = m.AdditionalConfiguration;
        t.AdditionalProperties = m.AdditionalProperties;
        t.MeasurementInterval = m.MeasurementInterval;
        t.ValueType = m.ValueType;
        t.DownstreamType = m.DownstreamType;
    }


    /// <summary>
    ///     TableMeasurementDefinition nach ExMeasurementDefinition.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExMeasurementDefinition ToExMeasurementDefinition(this TableMeasurementDefinition t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinition)}]({nameof(ToExMeasurementDefinition)}): {nameof(t)} is null");
        }

        var r = new ExMeasurementDefinition
        {
            Id = t.Id,
            AdditionalConfiguration = t.AdditionalConfiguration,
            Information = t.Information.ToExInformation(),
            ValueType = t.ValueType,
            DownstreamType = t.DownstreamType,
            MeasurementInterval = t.MeasurementInterval,
            AdditionalProperties = t.AdditionalProperties,
            IotDeviceId = t.TblIotDeviceId,
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            CompanyId = t.TblIoTDevice!.TblGateway!.TblCompany.Id
        };

        var val = t.TblMeasurements.LastOrDefault();
        r.CurrentValue.SourceInfo = $"{t.Information.Name}[{t.Id}] - {t.TblIoTDevice.Information.Name}[{t.TblIotDeviceId}]";
        r.CurrentValue.ValueCounter = t.TblMeasurements == null! ? 0 : t.TblMeasurements.Count;

        if (val == null)
        {
            r.CurrentValue.Value = "Keine Daten";
        }
        else
        {
            r.CurrentValue.Id = val.Id;
            r.CurrentValue.Location = val.Location.ToExPosition();
            r.CurrentValue.TimeStamp = val.TimeStamp;

            if (val.Value == null!)
            {
                r.CurrentValue.Value = "Fehler in Daten!";
            }
            else
            {
                switch (t.ValueType)
                {
                    case EnumValueTypes.Number:
                        r.CurrentValue.Value = val.Value.Number.HasValue ? NumberFormathelper(val.Value.Number.Value) : "Fehler in Zahl";
                        break;
                    case EnumValueTypes.Data:
                        r.CurrentValue.Value = val.Value.Binary == null! ? "Fehler in Bytes" : GcByteHelper.BytesToHexString(val.Value.Binary);
                        break;
                    case EnumValueTypes.Text:
                        r.CurrentValue.Value = string.IsNullOrEmpty(val.Value.Text) ? "Fehler in Text" : val.Value.Text;
                        break;
                    case EnumValueTypes.Image:
                        r.CurrentValue.Value = val.Value.Binary == null! ? "Fehler in Bild" : $"Bild {val.Value.Binary.Length / 1024} kB";
                        break;
                    case EnumValueTypes.Bit:
                        r.CurrentValue.Value = val.Value.Bit == null ? "Fehler in Bit" : val.Value.Bit.Value.ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return r;
    }

    private static string NumberFormathelper(double number)
    {
        if ((number % 1) != 0)
        {
            return Math.Round(number, 2).ToString(new CultureInfo("de-at"));
        }

        return number.ToString(new CultureInfo("de-at"));
    }
}