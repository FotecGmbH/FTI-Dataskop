// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Downstreams.DotNet;
using BDA.Common.Exchange.Configs.Downstreams.Virtual;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Plattform;
using Biss.Cli;
using Biss.Serialize;

namespace BDA.Common.ConfigMaker
{
    internal class Program
    {
        /// <summary>
        ///     Haupteinstiegspunkt der Applikation
        /// </summary>
        /// <param name="args">CLI Argumente</param>
        /// <returns>Fehlercode</returns>
        private static async Task<int> Main(string[] args)
        {
            var t = new GcPlattformDotNet();
            var m = t.BuildInExMeasurementDefinitions.First();

            var x = m.AdditionalConfiguration;
            var r = BissDeserialize.FromJson<GcDownstreamDotNet>(x);
            // ReSharper disable once UnusedVariable
            var b = r.ToStateMachine(EnumIotDeviceDownstreamTypes.DotNet);

            var f = new GcDownstreamVirtualFloat();
            // ReSharper disable once UnusedVariable
            var bb = f.ToStateMachine(EnumIotDeviceDownstreamTypes.Virtual);


            //Debugger.Break();

            //return 0;


            var result = 0;
            var app = new ConfigMakerApp(args);
            var start = DateTime.Now;
            Exception? appException = null;

            try
            {
                result = await app.RunCliApp().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                appException = e;
            }

            if (appException != null)
            {
                ConsoleColor.DarkRed.WriteLine(appException.ToString());
            }

            Console.WriteLine($"\r\nApp was running for {(DateTime.Now - start)}");

            if (Debugger.IsAttached && appException != null)
            {
                Debugger.Break();
            }

            Console.WriteLine("App Closed! Enter to go back!");
            Console.ReadLine();

            return result;
        }
    }
}