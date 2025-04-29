// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace System.Device.Location
{
    public class GeoPosition<T>
    {
        #region Properties

        public T Location { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;

        #endregion

        #region Constructors

        public GeoPosition() :
            this(DateTimeOffset.MinValue, default(T))
        {
        }

        public GeoPosition(DateTimeOffset timestamp, T position)
        {
            Timestamp = timestamp;
            Location = position;
        }

        #endregion
    }
}