// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace BDA.Common.MicrotronicsClient
{
    /// <summary>
    ///     <para>API Client fuer Microtronics</para>
    ///     Klasse MicrotronicsApiClient. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class MicrotronicsApiClient
    {
        private readonly string _apiString = "/api/1/";
        private readonly string _backendDomain = "https://austria.microtronics.com";

        private readonly HttpClient _client;

        private readonly string _password;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly string _userName;

        /// <summary>
        /// MicrotronicsApiClient
        /// </summary>
        /// <param name="backendDomain"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public MicrotronicsApiClient(string backendDomain, string userName, string password)
        {
            _backendDomain = backendDomain;
            _userName = userName;
            _password = password;
            _client = new HttpClient {BaseAddress = new Uri(_backendDomain)};
            var authenticationString = $"{_userName}:{_password}";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString)));
        }

        /// <summary>
        /// GetCustomerNames
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetCustomerNames()
        {
            List<string> customerNames = new List<string>();

            var response = await GetRequest(_apiString + "customers").ConfigureAwait(true);

            if (response.Item1 == HttpStatusCode.OK)
            {
                try
                {
                    var responseJson = JArray.Parse(response.Item2);
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    customerNames = responseJson.Select(x => x["customer_id"].Value<string>()).ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8604 // Possible null reference argument.
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"{e}");
                }
            }

            return customerNames;
        }


        /// <summary>
        /// Get sites for customer id
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetSites(string customerId)
        {
            List<string> sites = new List<string>();

            var response = await GetRequest(_apiString + "customers/" + customerId + "/sites").ConfigureAwait(true);

            if (response.Item1 == HttpStatusCode.OK)
            {
                try
                {
                    var responseJson = JArray.Parse(response.Item2);
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    sites = responseJson.Select(x => x["site_id"].Value<string>()).ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8604 // Possible null reference argument.
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"{e}");
                }
            }

            return sites;
        }


        /// <summary>
        /// Get sites for customer id
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<List<string>> GetValidChannels(string customerId, string siteId, string config)
        {
            List<string> keys = new List<string>();

            var response = await GetRequest(_apiString + "customers/" + customerId + "/sites/" + siteId + "/" + config).ConfigureAwait(true);

            if (response.Item1 == HttpStatusCode.OK)
            {
                try
                {
                    var responseJson = JObject.Parse(response.Item2);
                    keys = responseJson.Properties().Select(p => p.Name).ToList();
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"{e}");
                }
            }

            return keys;
        }

        /// <summary>
        /// Get values from site, selecting all channels of it and get the values from a given timestamp till now
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteId"></param>
        /// <param name="histData"></param>
        /// <param name="channels"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<(DateTime, string)>>> GetValuesTillNow(string customerId, string siteId, string histData, List<string> channels, DateTime from)
        {
            var str = ", \"from\": \"" + from.ToString("yyyyMMdd") + "\", \"until\": \"*\"";

            return await GetData(customerId, siteId, histData, channels, str, "");
        }

        /// <summary>
        /// Get youngest value from site, selecting all channels of it
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteId"></param>
        /// <param name="histData"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public async Task<List<(string, DateTime, string)>> GetYoungestValues(string customerId, string siteId, string histData, List<string> channels)
        {
            var data = await GetData(customerId, siteId, histData, channels, "", "/youngest");
            var result = new List<(string, DateTime, string)>();

            try
            {
                result = data.Select(d => (d.Key, d.Value.First().Item1, d.Value.First().Item2)).ToList();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(MicrotronicsApiClient)}][{nameof(GetYoungestValues)}]{e}");
            }

            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        ///     Deckt verschiedene Anfragen an die Microtronics API ab die Daten abfragen (z.b. juengstes oder Zeitspanne)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteId"></param>
        /// <param name="histData">histData configuration</param>
        /// <param name="channels">channels</param>
        /// <param name="stringAfterChannels">
        ///     Alle channels werden selected, falls zusaetzliche informationen in json format
        ///     eingegeben werden muessen, hier (z.b."fromTime")
        /// </param>
        /// <param name="urlAddition">z.b. "youngest"</param>
        /// <returns></returns>
        private async Task<Dictionary<string, List<(DateTime, string)>>> GetData(string customerId, string siteId, string histData, List<string> channels, string stringAfterChannels, string urlAddition)
        {
            channels = channels.Distinct().ToList();
            Dictionary<string, List<(DateTime, string)>> values = new Dictionary<string, List<(DateTime, string)>>();

            if (channels.Contains("stamp"))
            {
                channels.Remove("stamp");
                channels.Insert(0, "stamp"); // sicherstellen, dass stamp immer an erster Stelle steht, WEIL, stamp wird immer mitgeschickt, egal ob es in channels ist oder nicht und so können wir die zuordnung einfacher machen
            }

            channels.ForEach(c => values.Add(c, new List<(DateTime, string)>()));

            var channelString = string.Join(",", channels.Select(c => "\"" + c + "\""));
            var jsonPayload = "{\"select\": [" + channelString + "]" + stringAfterChannels + "}";
            var response = await GetRequest(_apiString + $"customers/{customerId}/sites/{siteId}/{histData}{urlAddition}?json={Uri.EscapeDataString(jsonPayload)}").ConfigureAwait(true);

            if (response.Item1 == HttpStatusCode.OK)
            {
                try
                {
                    var responseJson = JArray.Parse(response.Item2);

                    foreach (var dataPackage in responseJson)
                    {
                        var data = JArray.Parse(dataPackage.ToString());
                        var timestamp = DateTime.ParseExact(data[0].Value<string>(), "yyyyMMddHHmmssfff", null);

                        if (channels.Contains("stamp"))
                        {
                            for (var i = 0; i < data.Count; i++)
                            {
                                if (i == 0) //
                                {
                                    values[channels[i]].Add((timestamp, timestamp.ToString("yyyyMMddHHmmssfff")));
                                }
                                else
                                {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                    values[channels[i]].Add((timestamp, data[i].Value<string>()));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                }
                            }
                        }
                        else
                        {
                            for (var i = 1; i < data.Count; i++)
                            {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                values[channels[i - 1]].Add((timestamp, data[i].Value<string>()));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"{e}");
                }
            }

            return values;
        }


        private async Task<(HttpStatusCode, string)> GetRequest(string requestUri)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            // Cancellationtoken falls Server nicht erreichbar ist.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            (HttpStatusCode, string) result = (HttpStatusCode.NotFound, string.Empty);

            try
            {
                var response = await _client.GetAsync(requestUri, cts.Token).ConfigureAwait(true);

                result.Item1 = response.StatusCode;

                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                using var reader = new StreamReader(stream);
                result.Item2 = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Logging.Log.LogWarning($"[{nameof(MicrotronicsApiClient)}]({nameof(GetRequest)}): Microtronics api=> Timeout");
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(MicrotronicsApiClient)}]({nameof(GetRequest)}): Microtronics api=>  unsuccessfull: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }
    }
}