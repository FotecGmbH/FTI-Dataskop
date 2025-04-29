// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using BDA.Gateway.App.Core;
using Exchange;

namespace BDA.Gateway.App;

internal class Program
{
    /// <summary>
    ///     Haupteinstiegspunkt der Applikation
    /// </summary>
    /// <param name="args">CLI Argumente</param>
    /// <returns>Fehlercode</returns>
    private static async Task<int> Main(string[] args)
    {
        var signalHostString = "dcsignalhost:";
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        string signalHostConnectionString = null!;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        Uri? uri = null;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        var signalHostSet = false;
        if (args.Length != 0)
        {
            if (args.Any(arg => arg.ToLower().Contains(signalHostString))) // Wenn gewollt kann connectionstring fuer dc signal Host angegeben werden
            {
                signalHostSet = true;
            }
        }

        if (!signalHostSet)
        {
            args = args.Append(signalHostString + AppSettings.Current().DcSignalHost).ToArray();
        }


        return await GatewayAppEntryPoint.Start(args);
    }
}