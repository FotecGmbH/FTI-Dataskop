// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Common;

namespace Database.Converter;

/// <summary>
///     <para>DbPosition konvertieren</para>
///     Klasse ConverterDbPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbPosition
{
    /// <summary>
    ///     ExPosition nach DbPosition
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static DbPosition ToDbPosition(this ExPosition p)
    {
        if (p == null!)
        {
            return new DbPosition();
        }

        return new DbPosition
        {
            TimeStamp = p.TimeStamp,
            Altitude = p.Altitude,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            Precision = p.Presision,
            Source = p.Source
        };
    }

    /// <summary>
    ///     DbPosition zu ExPosition
    /// </summary>
    /// <param name="dbPosition"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExPosition ToExPosition(this DbPosition dbPosition)
    {
        if (dbPosition == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPosition)}]({nameof(ToExPosition)}): {nameof(dbPosition)} is null");
        }

        return new ExPosition
        {
            Altitude = dbPosition.Altitude,
            Latitude = dbPosition.Latitude,
            Longitude = dbPosition.Longitude,
            Presision = dbPosition.Precision,
            Source = dbPosition.Source,
            TimeStamp = dbPosition.TimeStamp
        };
    }
}