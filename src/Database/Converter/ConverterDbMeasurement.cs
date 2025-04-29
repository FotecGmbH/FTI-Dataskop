// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Common;
using Database.Tables;
using NetTopologySuite.Geometries;

namespace Database.Converter;

/// <summary>
///     <para>Messwerte konvertieren</para>
///     Klasse ConverterDbMeasurement. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbMeasurement
{
    /// <summary>
    ///     ExValue nach TableMeasurementResult
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TableMeasurementResult ToTableMeasurementResult(this ExValue v)
    {
        if (v == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbMeasurement)}]({nameof(ToTableMeasurementResult)}): {nameof(v)}");
        }

        return new TableMeasurementResult
        {
            Location = v.Position!.ToDbPosition(),
            TblMeasurementDefinitionId = v.Identifier,
            TimeStamp = v.TimeStamp,
            ValueType = v.ValueType,
            Value = new DbValue
            {
                Number = v.MeasurementNumber,
                Binary = (v.ValueType == EnumValueTypes.Image) ? v.MeasurementImage : v.MeasurementRaw,
                Bit = v.MeasurementBool,
                Text = string.IsNullOrEmpty(v.MeasurementText) ? "" : v.MeasurementText
            },
            SpatialPoint = new Point(v.Position!.Longitude, v.Position!.Latitude) {SRID = 4326}
        };
    }
}