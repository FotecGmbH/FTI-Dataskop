// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Extensions.</para>
    ///     Klasse Extensions (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class BissExtensions
    {
        /// <summary>
        ///     Splittet in eine liste von listen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="size"></param>
        /// <returns></returns>
#pragma warning disable CA1002 // Do not expose generic lists -> wird genau so gebraucht.
        [Obsolete("Kommt in der nächsten Version ins Biss.Core")]
        public static List<List<T>> Split<T>(this List<T> collection, int size)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            var chunks = new List<List<T>>();

            var originalCount = collection.Count();
            var chunkCount = originalCount / size;

            if (originalCount % size > 0)
            {
                chunkCount++;
            }

            for (var i = 0; i < chunkCount; i++)
            {
                chunks.Add(collection.Skip(i * size).Take(size).ToList());
            }

            return chunks;
        }
    }
}