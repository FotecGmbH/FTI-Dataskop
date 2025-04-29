// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Google.Protobuf.WellKnownTypes;

namespace BDA.Service.Com.GRPC.Extensions
{
    /// <summary>
    ///     <para>This class contains extension methods for the DateTime</para>
    ///     Klasse DateTimeExtensions. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class DateTimeExtensions
    {
        public static Timestamp ConvertToUtcGoogleTimestamp(this DateTime date)
        {
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utcDate);
        }
    }
}