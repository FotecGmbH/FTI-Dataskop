// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDA.Service.AppConnectivity.DataConnector;
using Biss.Apps.Service.Push;
using Biss.Dc.Server;
using Biss.Dc.Transport.Server.SignalR;
using Biss.Log.Producer;
using Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerRemoteCalls = BDA.Service.AppConnectivity.DataConnector.ServerRemoteCalls;
// ReSharper disable HeuristicUnreachableCode

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     <para>Hintergrundservice</para>
    ///     Klasse BackgroundService. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BackgroundService<T> : IHostedService, IDisposable
    {
        private readonly IDcConnections _clientConnection;
        private readonly ServerRemoteCalls _dc;

        // ReSharper disable once NotAccessedField.Local
        private readonly IHubContext<DcCoreHub<T>> _hubContext;
        private readonly DateTime _startDateTime = DateTime.UtcNow;
        private int _counter10Min;
        private bool _disposedValue;

        /// <summary>
        ///     Timer
        /// </summary>
        private Timer _timer = null!;

        /// <summary>
        ///     Konstruktor, Services werden injected
        /// </summary>
        /// <param name="clientConnection"></param>
        /// <param name="hubcontext"></param>
        /// <param name="serviceScopeFactory"></param>
        public BackgroundService(IDcConnections clientConnection, IHubContext<DcCoreHub<T>> hubcontext, IServiceScopeFactory serviceScopeFactory)
        {
            _clientConnection = clientConnection;
            _hubContext = hubcontext;

            if (serviceScopeFactory == null!)
            {
                throw new ArgumentException($"[{nameof(BackgroundService)}]({nameof(BackgroundService)}): {nameof(serviceScopeFactory)} is NULL!");
            }

            var scope = serviceScopeFactory.CreateScope();
            var s = scope.ServiceProvider.GetService<IServerRemoteCalls>();
            if (scope == null! || s == null!)
            {
                throw new ArgumentException($"[{nameof(BackgroundService)}]({nameof(BackgroundService)}): {nameof(scope)}  is NULL!");
            }

            _dc = (ServerRemoteCalls) s;
            _dc.SetClientConnection(_clientConnection);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        ///     Führt arbeiten im angegebenen Interval durch
        ///     (Für eventhandler kann async void verwendet werden)
        /// </summary>
        /// <param name="state"></param>
        private async void DoWork(object state)
        {
            return;
#pragma warning disable CS0162 // Unreachable code detected
            try
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                var cntDevices = db.TblDevices.Count();

                if (!DcConnected())
                {
                    Logging.Log.LogWarning($"[{nameof(BackgroundService)}]({nameof(DoWork)}): DC is not connected!");
                }

                Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): Running time: {(DateTime.UtcNow - _startDateTime):g}");
                Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): Devices in Db: {cntDevices}");
                Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): Devices online: {_clientConnection.GetClients().Count}");

                var users = await db.TblUsers
                    .Where(u => u.Locked == false)
                    .Include(i => i.TblDevices)
                    .ToListAsync().ConfigureAwait(true);

                _counter10Min++;

                foreach (var user in users)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (user == null)
                    {
                        continue;
                    }

                    if (_counter10Min >= 10)
                    {
                        if (user.Setting10MinPush)
                        {
                            var tokens = user.TblDevices.Where(d => !string.IsNullOrWhiteSpace(d.DeviceToken)).Select(d => d.DeviceToken).ToList();
                            if (tokens.Count == 0)
                            {
                                continue;
                            }

                            var fail = 0;
                            var success = 0;
                            var result = await PushService.Instance.SendMessageToDevices("10 Minuten", "10 Minuten Push Nachricht", tokens).ConfigureAwait(true);
                            fail = result.FailureCount;
                            success = result.FailureCount;
                            Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): 10 Minuten Push: {success} erfolgreich, {fail} nicht erfolgreich!");
                        }
                    }
                }

                if (_counter10Min >= 10)
                {
                    _counter10Min = 0;
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        ///     DC ist bereit
        /// </summary>
        /// <returns></returns>
        private bool DcConnected()
        {
            if (_dc != null! && _dc.ClientConnection != null!)
            {
                return true;
            }

            return false;
        }

        #region Interface Implementations

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Start
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(StartAsync)}): Timed Hosted Service running.");
            _timer = new Timer(DoWork!, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Stop
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(StopAsync)}): Timed Hosted Service is stopping.");
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        #endregion
    }
}