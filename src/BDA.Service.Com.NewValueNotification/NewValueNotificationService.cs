// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Concurrent;
using System.Net.Http.Json;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.Com.MQTT;
using Biss.Log.Producer;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace BDA.Service.Com.NewValueNotification
{
    public class NewValueNotificationService : INewValueNotificationService
    {
        private readonly ConcurrentQueue<(long measurementDefinitionId, ExMeasurement measurement)> _concurrentQueue;
        private readonly IMqttService _mqttService;
        private int _workTaskIsRunnning;

        public NewValueNotificationService(IMqttService mqttService)
        {
            _concurrentQueue = new ConcurrentQueue<(long measurementDefinitionId, ExMeasurement measurement)>();
            _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
        }

        private async Task ProcessQueueAsync()
        {
            if (Interlocked.CompareExchange(ref _workTaskIsRunnning, 1, 0) == 0)
            {
                try
                {
                    await using var db = new Db();

                    var js = new JsonTypeSafeConverter();

                    while (_concurrentQueue.TryDequeue(out var item))
                    {
                        var tableNewValueNotifications = await db.TblNewValueNotifications
                            .AsNoTracking()
                            // ReSharper disable once AccessToModifiedClosure
                            .Where(x => x.TblMeasurementDefinitionId == item.measurementDefinitionId)
                            .ToListAsync()
                            .ConfigureAwait(false);


                        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                        if (tableNewValueNotifications?.Any() != true || item.measurement is null)
                        {
                            return;
                        }

                        foreach (var tableNewValueNotification in tableNewValueNotifications)
                        {
                            try
                            {
                                switch (tableNewValueNotification.NewValueNotificationType)
                                {
                                    case EnumNewValueNotificationType.Webhook:
                                        var newValueNotificationWebHook = js.Deserialize<ExNewValueNotificationWebHook>(tableNewValueNotification.AdditionalConfiguration);

                                        if (string.IsNullOrWhiteSpace(newValueNotificationWebHook?.WebHookUrl))
                                        {
                                            break;
                                        }

                                        if (newValueNotificationWebHook.IsNotificationDesired(item.measurement))
                                        {
                                            using var httpClient = new HttpClient();
                                            await httpClient.PostAsJsonAsync(new Uri(newValueNotificationWebHook.WebHookUrl), item.measurement).ConfigureAwait(false);
                                        }

                                        break;

                                    case EnumNewValueNotificationType.Mqtt:
                                        var newValueNotificationMqtt = js.Deserialize<ExNewValueNotificationMqtt>(tableNewValueNotification.AdditionalConfiguration);

                                        if (newValueNotificationMqtt is null)
                                        {
                                            break;
                                        }

                                        if (newValueNotificationMqtt.IsNotificationDesired(item.measurement))
                                        {
                                            //alle gültigen Token des Users holen
                                            var tokens = await db.TblAccessToken
                                                .Include(t => t.TblUser)
                                                .Where(t => t.TblUserId == tableNewValueNotification.TblUserId && t.GuiltyUntilUtc >= DateTime.UtcNow)
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                                            if (tokens?.Any() != true)
                                            {
                                                break;
                                            }

                                            foreach (var token in tokens)
                                            {
                                                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                                                if (token.TblUser is null)
                                                {
                                                    break;
                                                }

                                                await _mqttService.PublishMeasurementResultAsync(item.measurement, token.Token, item.measurementDefinitionId).ConfigureAwait(false);
                                            }
                                        }

                                        break;

                                    case EnumNewValueNotificationType.Email:
                                        var newValueNotificationEmail = js.Deserialize<ExNewValueNotificationEmail>(tableNewValueNotification.AdditionalConfiguration);

                                        if (string.IsNullOrWhiteSpace(newValueNotificationEmail?.EmailAddress))
                                        {
                                            break;
                                        }

                                        if (newValueNotificationEmail.IsNotificationDesired(item.measurement))
                                        {
                                            var bem = WebConstants.Email;

                                            var subject = "BDA - Benachrichtigung neuer Messwert";
                                            var html = $"<h1>Neuer Messwert für Messwert-Definitions-Id {item.measurementDefinitionId}</h1>" +
                                                $"<p>Measurement Id: {item.measurement.Id}</p>" +
                                                $"<p>Messwert: {item.measurement.Value}</p>" +
                                                $"<p>Zeitpunkt: {item.measurement.TimeStamp.ToLongDateString()}</p>";

                                            if (!string.IsNullOrWhiteSpace(item.measurement.SourceInfo))
                                            {
                                                html += $"<p>Quelle: {item.measurement.SourceInfo}</p>";
                                            }

                                            html += $"<p>Location: {item.measurement.Location}</p>";
                                            html += $"<p>Location Google-Link: {item.measurement.Location.GoogleString}</p>";

                                            var emailSuccess = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {newValueNotificationEmail.EmailAddress}, subject, html).ConfigureAwait(false);
                                            if (!emailSuccess)
                                            {
                                                Logging.Log.LogError($"[{nameof(NewValueNotificationService)}]({nameof(ProcessQueueAsync)}): Email could not be sent to {newValueNotificationEmail.EmailAddress}");
                                            }
                                        }

                                        break;

                                    case EnumNewValueNotificationType.Push:
                                    case EnumNewValueNotificationType.Sms:
                                    default:
                                        //not implemented yet
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Logging.Log.LogError($"{e}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"{e}");
                }
                finally
                {
                    _workTaskIsRunnning = 0;
                }
            }
        }

        #region Interface Implementations

        public async Task SendNewValueNotificationAsync(long measurementDefinitionId, ExMeasurement newValue)
        {
            _concurrentQueue.Enqueue((measurementDefinitionId, newValue));
            await ProcessQueueAsync().ConfigureAwait(false);
        }

        #endregion
    }
}