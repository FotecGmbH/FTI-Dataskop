// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;

namespace BDA.IotDevice.App;

internal class Program
{
    /// <summary>
    ///     Haupteinstiegspunkt der Applikation
    /// </summary>
    /// <param name="args">CLI Argumente</param>
    /// <returns>Fehlercode</returns>
    private static async Task<int> Main(string[] args)
    {
        var app = new IotDeviceCliApp(args);
        return await app.RunCliApp().ConfigureAwait(false);
    }
}