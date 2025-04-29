// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.IotDevice.Com.Downstream.Base
{
    /// <summary>
    ///     <para>Basis für Messwertanbindung und Erfassung</para>
    ///     Klasse DownstreamBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract class DownstreamBase
    {
        private readonly int _delayMs;
        private readonly List<ExValue> _internalBuffer = new();
        private readonly Func<List<ExValue>, bool, Task> _transferMethod;
        private CancellationTokenSource _recordingToken = null!;

        /// <summary>
        ///     Basis für Messwertanbindung und Erfassung
        /// </summary>
        /// <param name="transferMethod">Erfasste Daten weiter geben</param>
        /// <param name="config">Konfiguration für diesen Messwert</param>
        /// <param name="stopToken">Globaler Stop Token</param>
        protected DownstreamBase(Func<List<ExValue>, bool, Task> transferMethod, ExGwServiceMeasurementDefinitionConfig config, CancellationTokenSource stopToken)
        {
            if (transferMethod == null!)
            {
                throw new ArgumentNullException($"[{nameof(DownstreamBase)}]({nameof(DownstreamBase)}): {nameof(transferMethod)}");
            }

            if (config == null!)
            {
                throw new ArgumentNullException($"[{nameof(DownstreamBase)}]({nameof(DownstreamBase)}): {nameof(config)}");
            }

            if (stopToken == null!)
            {
                throw new ArgumentNullException($"[{nameof(DownstreamBase)}]({nameof(DownstreamBase)}): {nameof(stopToken)}");
            }

            if (config.MeasurementInterval <= 0)
            {
                throw new InvalidDataException($"Wrong value of {nameof(config.MeasurementInterval)}");
            }

            Config = config;
            _transferMethod = transferMethod;

            stopToken.Token.Register(StopRecordingMeasurements);
            _delayMs = Config.MeasurementInterval * 100;
            StartRecordingMeasurements();
        }

        #region Properties

        /// <summary>
        ///     Worker Task
        /// </summary>
        public Task WorkerTask { get; private set; } = Task.CompletedTask;

        /// <summary>
        ///     Läuft die Messwerterfassung
        /// </summary>
        public bool IsRecordRunning { get; set; }

        /// <summary>
        ///     Aktuelle Konfiguration des Messwerts
        /// </summary>
        public ExGwServiceMeasurementDefinitionConfig Config { get; }

        #endregion

        /// <summary>
        ///     Messwert-Aufzeichung starten
        /// </summary>
        public void StartRecordingMeasurements()
        {
            if (IsRecordRunning)
            {
                return;
            }

            _recordingToken = new CancellationTokenSource();
            WorkerTask = Task.Run(async () =>
            {
                IsRecordRunning = true;

                do
                {
                    try
                    {
                        await Task.Delay(_delayMs, _recordingToken.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignored
                    }

                    if (_recordingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var r = await GetValue().ConfigureAwait(false);
                    if (r == null!)
                    {
                        Logging.Log.LogWarning($"[{nameof(DownstreamBase)}]({nameof(StartRecordingMeasurements)}): ExValue is NULL!");
                        continue;
                    }

                    _internalBuffer.Add(r);

                    switch (Config.TransmissionType)
                    {
                        case EnumTransmission.Instantly:
                            await TransferValuesToGateway(_internalBuffer, true).ConfigureAwait(false);
                            break;
                        case EnumTransmission.Elapsedtime:
                            await TransferValuesToGateway(_internalBuffer, false).ConfigureAwait(true);
                            break;
                        case EnumTransmission.NumberOfMeasurements:
                            throw new NotFiniteNumberException();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } while (!_recordingToken.IsCancellationRequested);

                IsRecordRunning = false;
            });
        }

        /// <summary>
        ///     Messwert-Aufzeichung stoppen/pausieren
        /// </summary>
        public void StopRecordingMeasurements()
        {
            if (!IsRecordRunning)
            {
                return;
            }

            _recordingToken.Cancel();
        }

        /// <summary>
        ///     Messwert im System abfragen
        /// </summary>
        /// <returns></returns>
        protected abstract Task<ExValue> GetValue();

        /// <summary>
        ///     Erfasste Messwerte an das Gateway zu übertagen bzw. in den globalen Iot Device Buffer legen
        /// </summary>
        /// <param name="values"></param>
        /// <param name="transferInstantly"></param>
        /// <returns></returns>
        private async Task TransferValuesToGateway(List<ExValue> values, bool transferInstantly)
        {
            await _transferMethod(values, transferInstantly).ConfigureAwait(false);
            _internalBuffer.Clear();
        }
    }
}