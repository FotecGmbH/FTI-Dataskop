// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse WasserstandNoeImporterFromApi. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class WasserstandNoeImporterFromApi
    {
        public static async Task<Dictionary<(string, string), List<WasserstandNoeDataPoint>>> GetDataFromApi(DateTime onlyAfter)
        {
            Dictionary<(string, string), List<WasserstandNoeDataPoint>> dataDictionary =
                new Dictionary<(string, string), List<WasserstandNoeDataPoint>>();

            var ids = new List<int>
            {
                106484, 106823, 107003, 107011, 107029, 107078, 107177, 107235, 107284, 107300, 107334, 107342, 107466, 107490, 107532, 107565, 107573, 107581, 107607, 107623, 107706, 107748, 107789, 107813, 107854, 108183, 108258, 108456, 108464, 108514, 108563, 108712, 108761, 108779, 108803, 108845, 108969, 108985, 108993, 109041, 109108, 109116, 109181, 109215, 109280, 109314, 109330, 109413, 109546, 109645, 109660, 109710, 109785, 109801, 109827, 109868, 109918, 109983, 110254, 111278, 115261, 115410, 115543, 115550, 115568, 115600, 115634, 115642, 115717, 115741, 115808, 115881, 115907, 115923, 115972, 115980, 116004, 116038, 116061, 116087, 116186, 116194, 116269, 116285, 116319, 116350, 116426, 116517, 116533, 116566, 116814, 109728, 116236, 116558, 109074
            };

            foreach (var stationId in ids)
            {
                var type = "Niederschlag";
                var csvUrl = $"https://www.noe.gv.at/wasserstand/kidata/stationdata/{stationId}_{type}_Summen.csv";
                var csvData = await FetchCsvAndExtractData(csvUrl, onlyAfter);

                if (csvData != null)
                {
                    AddToDictionary(dataDictionary, csvData, type);
                }
            }

            foreach (var stationId in ids)
            {
                var type = "Luftfeuchtigkeit";
                var csvUrl = $"https://www.noe.gv.at/wasserstand/kidata/stationdata/{stationId}_{type}_Jahr.csv";
                var csvData = await FetchCsvAndExtractData(csvUrl, onlyAfter);
                if (csvData != null)
                {
                    AddToDictionary(dataDictionary, csvData, type);
                }
            }

            var grundwasserIds = new List<int>
            {
                300111, 300699, 301150, 304410, 304584, 313858, 317255, 317289, 322842, 322891, 326637, 327163, 327171,
                327189, 327312, 327411, 331272, 331447, 331504, 331728, 331669, 331819, 331892, 331918, 331991, 337113,
                337139, 337170, 337329, 337345, 337428, 337436, 337469, 359869
            };

            foreach (var stationId in grundwasserIds)
            {
                var type = "Grundwasserspiegel";
                var csvUrl = $"https://www.noe.gv.at/wasserstand/kidata/stationdata/{stationId}_{type}_Jahr.csv";
                var csvData = await FetchCsvAndExtractData(csvUrl, onlyAfter);
                if (csvData != null)
                {
                    AddToDictionary(dataDictionary, csvData, type);
                }
            }

            foreach (var stationId in ids)
            {
                var type = "Lufttemperatur";
                var csvUrl = $"https://www.noe.gv.at/wasserstand/kidata/stationdata/{stationId}_{type}_Jahr.csv";
                var csvData = await FetchCsvAndExtractData(csvUrl, onlyAfter);
                if (csvData != null)
                {
                    AddToDictionary(dataDictionary, csvData, type);
                }
            }

            return dataDictionary;
        }

        private static async Task<dynamic> FetchLocationData(string stationIdOrName)
        {
            var locationDataUrl = "https://www.noe.gv.at/wasserstand/kidata/maplist/MapList.json";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(locationDataUrl);
                if (response.IsSuccessStatusCode)
                {
                    var stations = JsonConvert.DeserializeObject<List<dynamic>>(await response.Content.ReadAsStringAsync());
                    foreach (var station in stations!)
                    {
                        if (station.Stationname == stationIdOrName || station.Stationnumber == stationIdOrName)
                        {
                            if (!double.TryParse(station.Lat.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                            {
                                continue;
                            }

                            if (!double.TryParse(station.Long.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double longi))
                            {
                                continue;
                            }


                            return new
                            {
                                name = station.Stationname,
                                lat,
                                @long = longi
                            };
                        }
                    }
                }
            }

            return null!;
        }

        private static async Task<List<WasserstandNoeDataPoint>> FetchCsvAndExtractData(string url, DateTime onlyAfter)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Keine Daten gefunden für URL: {url}");
                    return null!;
                }

                var lines = (await response.Content.ReadAsStringAsync()).Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, string> headerInfo = new Dictionary<string, string>();

                foreach (var line in lines.Take(8))
                {
                    if (line.Contains(";"))
                    {
                        var parts = line.Split(';');
                        headerInfo[parts[0].Trim().ToLower().Replace(" ", "_").Replace(".", "")] = parts[1].Trim();
                    }
                }

                List<WasserstandNoeDataPoint> data = new List<WasserstandNoeDataPoint>();
                var readerStarted = false;
                var stationIdOrName = headerInfo.ContainsKey("stationsnummer") ? headerInfo["stationsnummer"] : "Unbekannt";
                var locationInfo = await FetchLocationData(stationIdOrName);

                foreach (var line in lines.Skip(8))
                {
                    if (readerStarted)
                    {
                        var parts = line.Split(';');

                        if (DateTime.TryParse(parts[0].Trim(), out var dat))
                        {
                            if (DateTime.Compare(dat, onlyAfter) < 0)
                            {
                                continue;
                            }
                        }

                        var dataItem = new WasserstandNoeDataPoint
                        {
                            date = parts[0].Trim(),
                            value = parts[1].Trim(),
                            location = new WasserstandNoeLocation
                            {
                                name = headerInfo.ContainsKey("stationsname") ? headerInfo["stationsname"] : "Unbekannt",
                                lat = locationInfo?.lat,
                                @long = locationInfo?.@long
                            },
                            id = stationIdOrName,
                            unit = headerInfo.ContainsKey("einheit") ? headerInfo["einheit"] : "Unbekannt",
                            parameter = headerInfo.ContainsKey("parameter") ? headerInfo["parameter"] : "Unbekannt",
                            info = headerInfo.ContainsKey("zeitreihenname") ? headerInfo["zeitreihenname"] : "Unbekannt"
                        };
                        data.Add(dataItem);
                    }
                    else if (line.Contains("Datum;Wert"))
                    {
                        readerStarted = true;
                    }
                }

                return data;
            }
        }

        private static void AddToDictionary(Dictionary<(string, string), List<WasserstandNoeDataPoint>> dataDictionary, List<WasserstandNoeDataPoint> data, string type)
        {
            foreach (var dataPoint in data)
            {
                var key = (dataPoint.location.name, type);
                if (!dataDictionary.ContainsKey(key))
                {
                    dataDictionary[key] = new List<WasserstandNoeDataPoint>();
                }

                dataDictionary[key].Add(dataPoint);
            }
        }
    }
}