// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Reflection;
using System.Threading.Tasks;
using BDA.CliApp;
using BDA.IotDevice.Core;
using Biss.Cli;


namespace BDA.IotDevice.AppPi;

/// <summary>
///     <para>Gateway Console Applikation</para>
///     Klasse App. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class IotDeviceCliApp : CliAppBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly string[] _args;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private readonly IotDeviceCore _stateMachine;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    /// <summary>
    ///     IotDevice wird als Console-Applikation gestartet
    /// </summary>
    /// <param name="args"></param>
#pragma warning disable CS8618, CS9264
    public IotDeviceCliApp(string[] args)
#pragma warning restore CS8618, CS9264
    {
        _args = args;
        //_stateMachine = new IotDeviceStateMachine(ExGwServiceIotDeviceConfig.GetTestConfig());
    }

    /// <summary>
    ///     CLI - Menübefehle initialisieren (ESC-Beenden, H-Hilfe, L-Logging aktivieren) werden automatisch erzeugt
    /// </summary>
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public override Action? InitCommands(CliCommands rootCliCommands)
    {
        if (rootCliCommands == null!)
        {
            throw new ArgumentNullException(nameof(rootCliCommands));
        }

        //Test Commands
        rootCliCommands.Commands.Add(new CliCommand
        {
            Key = ConsoleKey.D1, Description = "Test Ebene 0 - 1", Action = () =>
            {
                Console.WriteLine("1");
                return Task.CompletedTask;
            },
            EndAction = () =>
            {
                Console.WriteLine("1 End");
                return Task.CompletedTask;
            }
        });


        var a = new Action(() =>
        {
            ConsoleColor.Cyan.WriteLine("BDA IotDevice (C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH");
            ConsoleColor.Cyan.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        });
        return a;
    }

    /// <summary>
    ///     App starten
    /// </summary>
    public override async Task StartCliApp()
    {
        await _stateMachine.StartMainLoop().ConfigureAwait(false);
    }

    /// <summary>
    ///     App beenden
    /// </summary>
    public override async Task<int> StopCliApp()
    {
        await _stateMachine.StopMainLoop().ConfigureAwait(false);
        return 0;
    }
}