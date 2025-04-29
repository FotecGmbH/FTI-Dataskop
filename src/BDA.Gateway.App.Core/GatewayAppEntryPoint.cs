// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Biss.Cli;

//using NgrokSharp.DTO;

namespace BDA.Gateway.App.Core
{
    /// <summary>
    ///     <para>GatewayAppEntryPoint</para>
    ///     Klasse GatewayAppEntryPoint. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class GatewayAppEntryPoint
    {
        /// <summary>
        /// Start
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Start(string[] args)
        {
            var result = 0;
            var app = new GatewayCliApp(args);
            var start = DateTime.Now;
            Exception? appException = null;
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += delegate
            {
                // call methods to clean up
            };
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