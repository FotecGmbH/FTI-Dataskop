// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
using BDA.Service.Com.MQTT;
using BDA.Service.Com.Rest.Controllers;
using Exchange;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost
{
    /// <summary>
    ///     <para>Program</para>
    ///     Klasse Program. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args">Args</param>
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();


            var logger = host.Services.GetRequiredService<ILogger<MeasurementResultController>>();
            var ct = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
            MeasurementResultController.Init(ct, logger);


            var mqttService = host.Services.GetRequiredService<IMqttService>();
            await mqttService.StartAsync().ConfigureAwait(false);
            await host.RunAsync().ConfigureAwait(false);
            await mqttService.StopAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Host Builder erzeugen
        /// </summary>
        /// <param name="args">args</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls(AppSettings.Current().DcSignalHost).UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    webBuilder.ConfigureKestrel(options => { options.ConfigureEndpointDefaults(endpoints => { endpoints.Protocols = HttpProtocols.Http1AndHttp2; }); });
                });
        }
    }
}