// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.GatewayService.Drei;
using Biss.Log.Producer;
using FluffySpoon.Ngrok;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace BDA.Gateway.Com.Drei
{
    /// <summary>
    ///     <para>Klasse welche Ngroktunnel startet und stoppt</para>
    ///     Klasse NgrokHandler. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class NgrokHandler
    {
        private WebApplication? _app;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// is active
        /// </summary>
        public bool IsActive;

        /// <summary>
        ///     Neuen Uplink von Drei erhalten
        /// </summary>
        public event EventHandler<EventArgsNewPostDataReceived>? NewPostDataReceived;

        /// <summary>
        /// start tunnel
        /// </summary>
        /// <param name="apitoken"></param>
        /// <param name="tunnelName"></param>
        /// <param name="port"></param>
        /// <param name="restartIfRunning"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> StartTunnelAsync(string apitoken = "", string tunnelName = "dreiserverblabla", string port = "8080", bool restartIfRunning = true)
        {
            if (IsActive && !restartIfRunning)
            {
                throw new InvalidOperationException("Tunnel is already active");
            }

            if (restartIfRunning)
            {
                await StopTunnelAsync().ConfigureAwait(false);
            }


            _cts = new CancellationTokenSource();

            foreach (var process in Process.GetProcessesByName("ngrok"))
            {
                process.Kill();
            }


            IsActive = true;
            var builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new LoggerRedirectCustomLoggerProvider());

            builder.Services.AddNgrok(options => { options.ShowNgrokWindow = true; });


            _app = builder.Build();

            var ngrokService = _app.Services.GetRequiredService<INgrokService>();

            await ngrokService.InitializeAsync();

            var tunnel = await ngrokService.StartAsync(new Uri($"http://localhost:{port}"), _cts.Token);

            //this opens a tunnel for the given URL
            Logging.Log.LogInfo("Ngrok tunnel URL for localhost:80 is: " + tunnel.PublicUrl);

            _app.Urls.Add($"http://0.0.0.0:{port}");

            _app.MapPost("/", async context =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var data = await reader.ReadToEndAsync().ConfigureAwait(false);

                OnNewPostDataReceived(new ExDreiUplinkFrame(data));

                Logging.Log.LogInfo($"[{nameof(NgrokHandler)}]({nameof(StartTunnelAsync)}): Got data from Drei: {data}");


                context.Response.StatusCode = (int) HttpStatusCode.OK;
            });

            _app.MapGet("/ping", () => "pong");


            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _app.RunAsync(_cts.Token).ConfigureAwait(false); // otherwise, UI thread is blocking
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (TaskCanceledException)
            {
                Logging.Log.LogInfo($"[{nameof(NgrokHandler)}]({nameof(StartTunnelAsync)}): Stopping HttpServer");
            }

            return tunnel.PublicUrl;
        }

        /// <summary>
        ///   Stop tunnel
        /// </summary>
        /// <returns></returns>
        public async Task StopTunnelAsync()
        {
            _cts?.Cancel();

            if (_app is not null)
            {
                await _app.StopAsync().ConfigureAwait(false);
                await _app.WaitForShutdownAsync().ConfigureAwait(false);
                var ngrokService = _app.Services.GetRequiredService<INgrokService>();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ngrokService.StopAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <summary>
        ///     Invoke NewPostDataReceived
        /// </summary>
        /// <param name="uplinkFrame"></param>
        protected virtual void OnNewPostDataReceived(ExDreiUplinkFrame uplinkFrame)
        {
            NewPostDataReceived?.Invoke(this, new EventArgsNewPostDataReceived(uplinkFrame));
        }
    }

    /// <summary>
    /// EventArgsNewPostDataReceived
    /// </summary>
    public class EventArgsNewPostDataReceived
    {
        /// <summary>
        ///    EventArgsNewPostDataReceived
        /// </summary>
        /// <param name="uplinkframe"></param>
        public EventArgsNewPostDataReceived(ExDreiUplinkFrame uplinkframe)
        {
            UplinkFrame = uplinkframe;
        }

        #region Properties

        /// <summary>
        ///     Uplinkframe
        /// </summary>
        public ExDreiUplinkFrame UplinkFrame { get; set; }

        #endregion
    }
}