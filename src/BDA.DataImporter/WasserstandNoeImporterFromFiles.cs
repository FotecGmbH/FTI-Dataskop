// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse WasserstandNoeImporterFromFiles. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class WasserstandNoeImporterFromFiles
    {
        public static Dictionary<(string, string), List<WasserstandNoeDataPoint>> GetDataFromDirectory(string directoryPath)
        {
            var dataDict = new Dictionary<(string, string), List<WasserstandNoeDataPoint>>();

            foreach (var filePath in Directory.GetFiles(directoryPath, "*.json"))
            {
                var fileName = Path.GetFileName(filePath);
                (string, string) locationName = ExtractLocationName(fileName);

                var jsonData = File.ReadAllText(filePath);
                var dataPoints = JsonConvert.DeserializeObject<List<WasserstandNoeDataPoint>>(jsonData);

                if (!dataDict.ContainsKey(locationName))
                {
                    dataDict[locationName] = new List<WasserstandNoeDataPoint>();
                }

                dataDict[locationName].AddRange(dataPoints!);
            }

            return dataDict;
        }

        public static (string, string) ExtractLocationName(string fileName)
        {
            // Extrahiert den Ortsnamen aus dem Dateinamen
            var parts = fileName.Split('_');

            if (parts[^1].Contains(','))
            {
                var location = parts[^1].Split(',')[0];
                return (location.Trim(), parts[0]);
            }

            if (parts[^1].Contains('.'))
            {
                var location = parts[^1].Split('.')[0];
                return (location.Trim(), parts[0]);
            }

            return (parts[^1], parts[0]);
        }
    }
}