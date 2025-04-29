// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using Microsoft.EntityFrameworkCore;

namespace Database.Common;

/// <summary>
///     <para>Position</para>
///     Klasse Position. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Owned]
public class DbPosition
{
    #region Properties

    /// <summary>
    ///     Longitude
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     Latitude
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     Altitude
    /// </summary>
    public double Altitude { get; set; }

    /// <summary>
    ///     Genauigkeit in Meter
    /// </summary>
    public double Precision { get; set; }

    /// <summary>
    ///     Zeitpunkt der GPS Position
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    ///     Quelle - Von wem stammen die Daten
    /// </summary>
    public EnumPositionSource Source { get; set; }

    #endregion
}