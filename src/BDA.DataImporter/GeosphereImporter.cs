// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using Database;
using Database.Common;
using Database.Tables;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para></para>
    ///     Klasse GeosphereImporter. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GeosphereImporter
    {
        public static async Task Import(string dataset, string parametersInput, string stationIds, string startInput, string endInput, long iotDeviceId, bool withCreatingMeasurementDefinition)
        {
            List<GeosphereDataPoint> dataPoints1 = await GetDataFromGeosphere(dataset, parametersInput, stationIds, startInput, endInput);
            var tableData1 = ConvertToTableMeasurementResult(dataPoints1);
            ImportDataToDatabase(tableData1, iotDeviceId, withCreatingMeasurementDefinition);
        }


        // Konvertiert die Daten von GeosphereDataPoints zu Database.TableMeasurementResult
        public static List<(string, string, TableMeasurementResult)> ConvertToTableMeasurementResult(List<GeosphereDataPoint> dataPoints)
        {
            List<(string, string, TableMeasurementResult)> tableMeasurementResults = new List<(string, string, TableMeasurementResult)>();

            foreach (var dataPoint in dataPoints)
            {
                var tableMeasurementResult = new TableMeasurementResult
                {
                    TimeStamp = DateTime.Parse(dataPoint.Timestamp),
                    Location = new DbPosition
                    {
                        Longitude = dataPoint.X,
                        Latitude = dataPoint.Y
                    },
                    SpatialPoint = new Point(dataPoint.X, dataPoint.Y) {SRID = 4326},
                    ValueType = EnumValueTypes.Number,
                    Value = new DbValue {Number = dataPoint.Data}
                };
                //dataPoint.Name;
                tableMeasurementResults.Add((dataPoint.Name, dataPoint.Id, tableMeasurementResult));
            }

            return tableMeasurementResults;
        }

        // Konvertiert Dictionary<string,TableMeasurementResult> zu den dazugehörigen List<TableMeasurementDefinition> und fügt diese TableMeasurementDefinitions der Datenbank hinzu
        public static void ImportDataToDatabase(List<(string, string, TableMeasurementResult)> tableMeasurementResults, long ioTDeviceId, bool withCreatingMeasurementDefinitions)
        {
            var db = new Db();

            var counter = 0;

            var devices = db.TblIotDevices.Include(idev => idev.TblMeasurementDefinitions).ThenInclude(md => md.Information);

            var device = devices.FirstOrDefault(dev => dev.Id == ioTDeviceId);

            if (device == null)
            {
                Console.WriteLine("Device not found");
                return;
            }

            foreach (var tableMeasurementResult in tableMeasurementResults)
            {
                if (withCreatingMeasurementDefinitions)
                {
                    if (!device.TblMeasurementDefinitions.Any(md => md.Information.Name == tableMeasurementResult.Item1))
                    {
                        db.TblMeasurementDefinitions.Add(new TableMeasurementDefinition
                        {
                            Information = new DbInformation
                            {
                                CreatedDate = DateTime.Now,
                                Name = tableMeasurementResult.Item1,
                                Description = tableMeasurementResult.Item2
                            },
                            ValueType = tableMeasurementResult.Item3.ValueType,
                            TblIotDeviceId = device.Id
                        });
                        db.SaveChanges();
                    }
                }

                var md = db.TblMeasurementDefinitions.Include(md => md.Information).FirstOrDefault(md => md.Information.Name == tableMeasurementResult.Item1 && md.TblIotDeviceId == device.Id);

                if (md == null)
                {
                    Console.WriteLine("MeasurementDefinition not found");
                    continue;
                }

                tableMeasurementResult.Item3.TblMeasurementDefinitionId = md.Id;

                var mr = db.TblMeasurementResults.AsNoTracking().Any(mr => mr.TblMeasurementDefinitionId == tableMeasurementResult.Item3.TblMeasurementDefinitionId && mr.TimeStamp.Equals(tableMeasurementResult.Item3.TimeStamp));

                if (!mr) // wenn es noch keinen Wert für diese Definition und Zeit gibt, dann erstelle das MeasurementResult
                {
                    counter++;
                    db.TblMeasurementResults.Add(tableMeasurementResult.Item3);

                    if (counter > 100)
                    {
                        db.SaveChanges();
                        counter = 0;
                    }
                }
            }

            db.SaveChanges();
        }


        public static async Task<List<GeosphereDataPoint>> GetDataFromGeosphere(string dataset, string parametersInput, string stationIds, string startInput, string endInput)
        {
            List<GeosphereDataPoint> dataPoints = new List<GeosphereDataPoint>();
            var url = $"https://dataset.api.hub.geosphere.at/v1/station/historical/{dataset}?parameters={parametersInput}&station_ids={stationIds}&start={startInput}&end={endInput}";

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<GeosphereApiResponse>(responseBody);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                var timestamps = apiResponse.timestamps;

                foreach (var feature in apiResponse.features)
                {
                    // ReSharper disable once UnusedVariable
                    var stationId = (string) feature["properties"]["station"];

                    var parameters = (JObject) feature["properties"]["parameters"];

                    foreach (var parameter in parameters.Properties())
                    {
                        var paramName = parameter.Name;
                        var paramValues = (JObject) parameter.Value;
                        var name = (string) paramValues["name"];
                        var unit = (string) paramValues["unit"];
                        var data = (JArray) paramValues["data"];
#pragma warning disable CS8604 // Possible null reference argument.
                        var x = (double) feature["geometry"]["coordinates"][0];

                        var y = (double) feature["geometry"]["coordinates"][1];
#pragma warning restore CS8604 // Possible null reference argument.

                        for (var i = 0; i < timestamps.Count; i++)
                        {
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                            if (data[i] == null || data[i].Type == JTokenType.Null)
                            {
                                continue;
                            }

                            dataPoints.Add(new GeosphereDataPoint
                            {
                                // ReSharper disable once RedundantCast
                                Timestamp = (string) timestamps[i],
                                Name = name!,
                                Unit = unit!,
                                Data = (double) data[i],
                                Id = paramName /*stationId*/,
                                X = x,
                                Y = y
                            });
                        }
                    }

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return dataPoints;
        }
    }
}