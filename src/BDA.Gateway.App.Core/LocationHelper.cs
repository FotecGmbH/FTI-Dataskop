// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Device.Location;
using System.Threading;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Log.Producer;

namespace BDA.Gateway.App.Core;

/// <summary>
///     <para>Wenn möglich Gps-Position lesen</para>
///     Klasse LocationHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class LocationHelper
{
    private static AutoResetEvent _wait = null!;

    /// <summary>
    ///     Wenn möglich Posiotion aus OS lesen
    /// </summary>
    /// <returns></returns>
    public static ExPosition? GetPosition()
    {
        ExPosition? result = null;
        _wait = new AutoResetEvent(false);
        using var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
        watcher.StatusChanged += (_, e) =>
        {
            Logging.Log.LogInfo($"[{nameof(LocationHelper)}]({nameof(GetPosition)}): GetPosition state: {e.Status}");
            if (e.Status == GeoPositionStatus.Disabled)
            {
                _wait.Set();
            }
        };
        watcher.PositionChanged += (_, e) =>
        {
            Logging.Log.LogInfo($"[{nameof(LocationHelper)}]({nameof(GetPosition)}): Position: {e.Position.Location.Latitude} , {e.Position.Location.Longitude} , {e.Position.Location.Altitude}");
            result = new ExPosition
            {
                TimeStamp = DateTime.UtcNow,
                Altitude = e.Position.Location.Altitude,
                Latitude = e.Position.Location.Latitude,
                Longitude = e.Position.Location.Longitude,
                Presision = (float) e.Position.Location.HorizontalAccuracy,
                // Source = "System.Device.Location.GeoCoordinateWatcher" -> Habs erstmal auf PC gestellt (GWe) ToDo: MKo
                Source = EnumPositionSource.Pc
            };

            _wait.Set();
        };

        watcher.Start();
        _wait.WaitOne(new TimeSpan(0, 0, 3));
        watcher.Stop();
        return result;
    }
}