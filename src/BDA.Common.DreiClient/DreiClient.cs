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
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace BDA.Common.DreiClient
{
    /// <summary>
    ///     Dreiclient
    /// </summary>
    public class DreiClient : IDisposable
    {
        private const string UsernamePrefix = "drei-api/";
        private const string BaseAddress = "https://drei.thingpark.com/iot-flow/v1/";
        private const string LoginUrl = "oauth/tokenSwagger";
        private readonly string _password;

        private readonly Timer _refreshLoginTimer = new Timer(TimeSpan.FromHours(1));
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly string _username;
        private ApiClient? _client;

        /// <summary>
        ///     Dreiclient
        /// </summary>
        /// <param name="username">Drei username</param>
        /// <param name="password">Drei passwort</param>
        public DreiClient(string username, string password)
        {
            _refreshLoginTimer.AutoReset = true;
            _refreshLoginTimer.Elapsed += async (_, _) =>
            {
                Logging.Log.LogInfo($"[{nameof(DreiClient)}]({nameof(DreiClient)}): Refreshing Login (every 1 Hour)");
                await Login().ConfigureAwait(false);
            };
            _refreshLoginTimer.Start();

            _username = username;
            _password = password;
        }

        #region Properties

        /// <summary>
        ///     Client ist eingelogged und hat einen güligen accesstoken. Wird einmal stündlich automatisch erneuert.
        /// </summary>
        public bool LoggedIn { get; private set; }

        #endregion

        /// <summary>
        ///     Erstellt auf Drei den passenden Flow. Muss jedesmal wenn sich die Config ändert ausgeführt werden
        /// </summary>
        /// <param name="gatewayId">Gatewayid für den namen</param>
        /// <param name="devEuis">Dev euis der verbundenen Devices um die Daten richtig umzuleiten</param>
        /// <exception cref="InvalidOperationException">Login wurde nicht ausgeführt</exception>
        public async Task SetupGatewayFlow(string gatewayId, List<string> devEuis)
        {
            Logging.Log.LogInfo($"[{nameof(DreiClient)}]({nameof(SetupGatewayFlow)}): Creating DreiGatewayflow: EUIs: {string.Join(", ", devEuis)}");
            if (LoggedIn == false)
            {
                await Login().ConfigureAwait(false);
            }

            if (_client is null)
            {
                throw new InvalidOperationException("Apiclient is null. Error while logging in.");
            }

            var matchers = new FlowMatchers();
            foreach (var devEui in devEuis)
            {
                matchers.Add(new KeyMatcher {Key = $"lora:{devEui}"});
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
                var flowlist = await _client.ListFlowsAsync(cts.Token).ConfigureAwait(false);
                var gatewayFlow = flowlist.FirstOrDefault(f => f.Name == gatewayId);

                var connectionList = await _client.ListConnectionsAsync(cts.Token).ConfigureAwait(false);
                var gatewayConnection = connectionList.FirstOrDefault(f => f.Name == gatewayId);

                if (gatewayConnection is null)
                {
                    throw new InvalidOperationException("Dreiclient tried to create a flow without creating a connection first");
                }

                if (gatewayFlow is null)
                {
                    await _client.CreateFlowAsync(new FlowRequest
                    {
                        Name = gatewayId,
                        Matchers = matchers,
                        Connectors = new FlowConnectors {new FlowConnector {ConnectionId = gatewayConnection.Id, Id = gatewayConnection.ConnectorId}},
                        SkipDecoding = true
                    }, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    gatewayFlow.Connectors = null; //Change!!!
                    await _client.UpdateFlowAsync(
                        new FlowRequest
                        {
                            Name = gatewayFlow.Name,
                            Description = gatewayFlow.Description,
                            Matchers = gatewayFlow.Matchers,
                            Connectors = new FlowConnectors {new FlowConnector {ConnectionId = gatewayConnection.Id, Id = gatewayConnection.ConnectorId}},
                            SkipDecoding = gatewayFlow.SkipDecoding,
                            AdditionalProperties = gatewayFlow.AdditionalProperties,
                            Driver = gatewayFlow.Driver,
                            UpOperations = gatewayFlow.UpOperations
                        }, gatewayFlow.Id, cts.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Logging.Log.LogError($"[{nameof(DreiClient)}]({nameof(SetupGatewayFlow)}): Dreiserver did not respond in time.");
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 201)
                {
                    // Success
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(DreiClient)}]({nameof(SetupGatewayFlow)}): Unknown error: {e.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Legt die passende Connection an. Muss nur einemal nach dem Start des Gateways ausgeführt werden
        /// </summary>
        /// <param name="url">Die öffentliche Addresse des Webservers des Gateways</param>
        /// <param name="gatewayId">Gatewayid für die benennung der Connection</param>
        /// <exception cref="InvalidOperationException">Login wurde nicht ausgeführt</exception>
        public async Task SetupGatewayConnection(string url, string gatewayId)
        {
            Logging.Log.LogInfo($"[{nameof(DreiClient)}]({nameof(SetupGatewayConnection)}): Creating SetupGatewayConnection. URL: {url}");
            if (LoggedIn == false)
            {
                await Login().ConfigureAwait(false);
            }

            if (_client is null)
            {
                throw new InvalidOperationException("Apiclient is null. Error while logging in.");
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var connectionList = await _client.ListConnectionsAsync(cts.Token).ConfigureAwait(false);
                var gatewayConnection = connectionList.FirstOrDefault(c => c.Name == gatewayId);

                if (gatewayConnection is null)
                {
                    await _client.CreateConnectionAsync(new ConnectionRequest
                    {
                        Name = gatewayId,
                        Active = true,
                        Configuration = new ConnectionConfig {DestinationUrl = url, Headers = new Dictionary<string, string> {{"X-Thing", "{DevEUI}"}}},
                        ConnectorId = "actility-http-iot"
                    }, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    await _client.UpdateConnectionAsync(new ConnectionRequest
                    {
                        Name = gatewayConnection.Name,
                        Active = true,
                        AdditionalProperties = gatewayConnection.AdditionalProperties,
                        Configuration = new ConnectionConfig {DestinationUrl = url, Headers = new Dictionary<string, string> {{"X-Thing", "{DevEUI}"}}},
                        ConnectorId = "actility-http-iot"
                    }, gatewayConnection.Id, cts.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Logging.Log.LogError($"[{nameof(DreiClient)}]({nameof(SetupGatewayFlow)}): Dreiserver did not respond in time.");
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 201)
                {
                    // Success
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(DreiClient)}]({nameof(SetupGatewayFlow)}): Unknown error: {e.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task Login()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            Logging.Log.LogInfo($"[{nameof(DreiClient)}]({nameof(Login)}): Logging in to Dreiapi");

            // Authstring drei-apu/<username>:<password>
            var authenticationString = $"{UsernamePrefix}{_username}:{_password}";

            // Erzeuge httpclient für den login prozess. Authorization über basic token. Auth token ist hierbei Base64 Encodiert
            // ReSharper disable once UsingStatementResourceInitialization
            using var loginClient = new HttpClient {BaseAddress = new Uri(BaseAddress)};
            loginClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString)));
            var values = new List<KeyValuePair<string, string>> {new("grant_type", "client_credentials")};

            // Cancellationtoken falls Server nicht erreichbar ist.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            try
            {
                // Login via Postrequest
                var response = await loginClient.PostAsync(LoginUrl, new FormUrlEncodedContent(values), cts.Token).ConfigureAwait(false);

                // Response enthält einen Accesstoken im Body. Dieser JWT Token kann zur authorisierung genutzt werden.
                var responseJsonToken = JObject.Parse(await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false));

                var accessToken = responseJsonToken.GetValue("access_token", StringComparison.InvariantCulture)?.ToString();
                if (accessToken is null)
                {
                    throw new InvalidOperationException("Could not parse Drei response.");
                }

                // Erzeuge neuen Httpclient für den Apiclient
                var httpClient = new HttpClient {BaseAddress = new Uri(BaseAddress)};
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                // Erstelle generierten Apiclient mit gesetztem Authorization Token 
                _client = new ApiClient(httpClient);
                LoggedIn = true;
            }
            catch (TaskCanceledException)
            {
                Logging.Log.LogWarning($"[{nameof(DreiClient)}]({nameof(Login)}): Could not login to Drei => Timeout");
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(DreiClient)}]({nameof(Login)}): Login to Drei unsuccessfull: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #region Interface Implementations

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _refreshLoginTimer.Stop();
            _refreshLoginTimer.Dispose();
            _semaphore.Dispose();
        }

        #endregion
    }

    /// <summary>
    ///     DTO für servercommunication
    /// </summary>
    public record ConnectionConfig
    {
        #region Properties

        /// <summary>
        ///     Headers
        /// </summary>
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        ///     DestinationUrl
        /// </summary>
        [JsonProperty("destinationURL")]
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string DestinationUrl { get; set; } = "";
#pragma warning restore CA1056 // URI-like properties should not be strings

        #endregion
    }
}