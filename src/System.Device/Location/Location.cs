﻿// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace System.Device.Location
{
    public class GeoLocation
    {
        public static readonly GeoLocation Unknown = new GeoLocation(GeoCoordinate.Unknown);

        #region Properties

        public GeoCoordinate Coordinate { get; private set; }
        public Double Heading { get; private set; }
        public Double Speed { get; private set; }
        public CivicAddress Address { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }

        #endregion

        #region Constructors

        public GeoLocation(GeoCoordinate coordinate)
            : this(coordinate, Double.NaN, Double.NaN)
        {
        }

        public GeoLocation(GeoCoordinate coordinate, Double heading, Double speed)
            : this(coordinate, heading, speed, CivicAddress.Unknown, DateTimeOffset.Now)
        {
        }

        public GeoLocation(CivicAddress address)
            : this(GeoCoordinate.Unknown, Double.NaN, Double.NaN, address, DateTimeOffset.Now)
        {
        }

        public GeoLocation(GeoCoordinate coordinate, Double heading, Double speed, CivicAddress address, DateTimeOffset timestamp)
        {
            if (coordinate == null)
            {
                throw new ArgumentNullException("coordinate");
            }

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (heading < 0.0 || heading > 360.0)
            {
                throw new ArgumentOutOfRangeException("heading", SR.Argument_MustBeInRangeZeroTo360);
            }

            if (speed < 0.0)
            {
                throw new ArgumentOutOfRangeException("speed", SR.Argument_MustBeNonNegative);
            }

            Coordinate = coordinate;
            Address = address;
            Heading = heading;
            Speed = speed;
            Timestamp = timestamp;
        }

        #endregion
    }
}