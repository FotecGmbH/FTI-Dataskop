// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;

namespace BDA.Common.OpensenseClient
{
    /// <summary>
    ///     <para>Helpermethods für den OpensenseClient</para>
    ///     Klasse Helper. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class Helper
    {
        /// <summary>
        ///     Teilt ein Zeitfenster in komponenten mit der angegebenen länge
        /// </summary>
        /// <param name="start">Startdatum</param>
        /// <param name="end">Enddatum</param>
        /// <param name="chunksize">Länge der Komponenten</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, TimeSpan chunksize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start + chunksize) < end)
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }

            yield return Tuple.Create(start, end);
        }
    }
}