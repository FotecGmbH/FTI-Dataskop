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
using BDA.Common.Exchange.OpenSense;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BDA.Common.OpensenseClient
{
    /// <summary>
    ///     Client für die Abrfrage von Opensense daten
    /// </summary>
    public class OpensenseClient : IDisposable
    {
        /// <summary>
        ///     Baseaddress to Opensense api
        /// </summary>
        public const string BaseAddr = "https://api.opensensemap.org/";

        private readonly HttpClient _httpClient;

        /// <summary>
        ///     Konstructor
        /// </summary>
        public OpensenseClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseAddr)
            };
        }


        /// <summary>
        ///     Ruft alle Sensoren einer Opensensebox ab.
        /// </summary>
        /// <param name="boxId">Die Id der box</param>
        /// <returns>Eine Liste von Sensoren</returns>
        public async Task<List<ExOpenSenseSensor>> GetCurrentValuesAsync(string boxId)
        {
            var result = new List<ExOpenSenseSensor>();

            if (string.IsNullOrEmpty(boxId))
            {
                return result;
            }

            var response = await _httpClient.GetAsync($"boxes/{boxId}/sensors").ConfigureAwait(false);
            if (response is {IsSuccessStatusCode: true})
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false) ?? "";
                var lat = -1.0;
                var lon = -1.0;
                var alt = -1;

                var jObject = JsonConvert.DeserializeObject<JObject>(responseString);
                if (jObject?["sensors"] is JArray sensors)
                {
                    foreach (var jSensor in sensors)
                    {
                        try
                        {
                            var sensor = new ExOpenSenseSensor
                            {
                                OpensenseId = jSensor["_id"]?.Value<string>() ?? throw new ArgumentNullException(nameof(ExOpenSenseSensor.OpensenseId), "ID needs a value"),
                                Title = jSensor["title"]?.Value<string>() ?? "",
                                Unit = jSensor["unit"]?.Value<string>() ?? "",
                                SensorType = jSensor["sensorType"]?.Value<string>() ?? "",
                                Value = jSensor["lastMeasurement"]?["value"]?.Value<string>() ?? "",
                                CreatedAt = jSensor["lastMeasurement"]?["createdAt"]?.Value<DateTime>() ?? DateTime.MinValue,
                                Longitude = lon,
                                Latitude = lat,
                                Altitude = alt
                            };

                            if (lat < 0)
                            {
                                (lat, lon, alt) = await GetValueLocation(boxId, sensor).ConfigureAwait(false);
                                sensor.Longitude = lon;
                                sensor.Latitude = lat;
                                sensor.Altitude = alt;
                            }

                            result.Add(sensor);
                        }
                        catch (Exception )
                        {
                            // Ignored
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Fügt die Coordinaten zu einem Sensor hinzu
        /// </summary>
        /// <param name="boxId"></param>
        /// <param name="sensor"></param>
        public async Task<(double, double, int)> GetValueLocation(string boxId, ExOpenSenseSensor sensor)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (sensor is null)
            {
                return (-1, -1, -1);
            }

            var values = await GetAllSensorValuesInTimeframe(boxId, sensor, sensor.CreatedAt.ToUniversalTime(), DateTime.Now.ToUniversalTime()).ToListAsync().ConfigureAwait(false);
            var latest = values.OrderByDescending(v => v.CreatedAt).FirstOrDefault(v => v.CreatedAt == sensor.CreatedAt);

            return (latest?.Latitude ?? -1, latest?.Longitude ?? -1, latest?.Altitude ?? -1);
        }

        /// <summary>
        /// </summary>
        /// <param name="boxId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ExOpenSenseSensor> GetAllValuesInTimeframe(string boxId, DateTime startdate, DateTime? enddate = null)
        {
            enddate ??= DateTime.Now.ToUniversalTime();
            var currentValues = await GetCurrentValuesAsync(boxId).ConfigureAwait(false);

            foreach (var sensor in currentValues)
            {
                await foreach (var measurement in GetAllSensorValuesInTimeframe(boxId, sensor, startdate, enddate.Value))
                {
                    yield return measurement;
                }
            }
        }

        private async IAsyncEnumerable<ExOpenSenseSensor> GetAllSensorValuesInTimeframe(string boxId, ExOpenSenseSensor sensor, DateTime startdate, DateTime enddate)
        {
            var datechunks = Helper.SplitDateRange(startdate, enddate, TimeSpan.FromDays(30));
            foreach (var (start, end) in datechunks)
            {
                var startstring = start.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
                var endstring = end.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

                var response = await _httpClient.GetAsync($"boxes/{boxId}/data/{sensor.OpensenseId}?from-date={startstring}&to-date={endstring}").ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false) ?? "";

                var jObject = JsonConvert.DeserializeObject<JArray>(responseString);
                if (jObject != null)
                {
                    foreach (var jvalue in jObject)
                    {
                        var entry = new ExOpenSenseSensor();
                        try
                        {
                            entry.OpensenseId = sensor.OpensenseId;
                            entry.Title = sensor.Title;
                            entry.Unit = sensor.Unit;
                            entry.SensorType = sensor.SensorType;
                            entry.Value = jvalue["value"]?.Value<string>() ?? "";
                            entry.CreatedAt = jvalue["createdAt"]?.Value<DateTime>() ?? DateTime.MinValue;
                            try
                            {
                                entry.Latitude = jvalue["location"]?[0]?.Value<double>() ?? 0;
                                entry.Longitude = jvalue["location"]?[1]?.Value<double>() ?? 0;
                                if (((JArray?) jvalue["location"])?.Count == 3)
                                {
                                    entry.Altitude = jvalue["location"]?[2]?.Value<int>() ?? 0;
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Logging.Log.LogDebug($"[{nameof(OpensenseClient)}]({nameof(GetAllSensorValuesInTimeframe)}): Location incomplete.");
                            }
                        }
                        catch (Exception)
                        {
                            // Ignored
                        }

                        yield return entry;
                    }
                }
            }
        }

        #region Interface Implementations

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion
    }
}