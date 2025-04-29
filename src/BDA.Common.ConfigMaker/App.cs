// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BDA.CliApp;
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Interfaces;
using Biss.Cli;
using Biss.Interfaces;
using Biss.Serialize;

namespace BDA.Common.ConfigMaker;

/// <summary>
///     <para>Gateway Console Applikation</para>
///     Klasse App. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class ConfigMakerApp : CliAppBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly string[] _args;
    private ConfigGenerator<IGcDownstream> _downstreamConfigs = null!;
    private ConfigGenerator<IGlobalConfig> _globalConfigs = null!;

    /// <summary>
    ///     IotDevice wird als Console-Applikation gestartet
    /// </summary>
    /// <param name="args"></param>
    public ConfigMakerApp(string[] args)
    {
        _args = args;
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

        _globalConfigs = new ConfigGenerator<IGlobalConfig>();
        if (_globalConfigs.Data.Count > 0)
        {
            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.D1,
                Description = $"Global Configs ({_globalConfigs.Data.Count})",
                Action = GenarateGlobalConfig
            });
        }


        _downstreamConfigs = new ConfigGenerator<IGcDownstream>();
        if (_downstreamConfigs.Data.Count > 0)
        {
            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.D2,
                Description = $"Downstream Configs ({_downstreamConfigs.Data.Count})",
                Action = GenarateDownstreamConfig
            });
        }


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
    public override Task StartCliApp()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     App beenden
    /// </summary>
    public override Task<int> StopCliApp()
    {
        return Task.FromResult(0);
    }

    private Task GenarateGlobalConfig()
    {
        var o = new List<ConsoleListItem>();
        for (var i = 0; i < _globalConfigs.Data.Count; i++)
        {
            o.Add(new ConsoleListItem($"{i + 1}", _globalConfigs.Data[i].Label));
        }

        BissConsole.WriteList(o);

        var id = Convert.ToInt32(BissConsole.ReadText("Choose: "));
        var config = _globalConfigs.Data[id - 1];

        foreach (var property in config.Properties)
        {
            property.Value = BissConsole.ReadText($"{property.Label}: ");
        }

        var result = _globalConfigs.ProcessData(config);
        Console.WriteLine($"{((IBissModel) result).ToJson()}");

        return Task.CompletedTask;
    }

    private Task GenarateDownstreamConfig()
    {
        var o = new List<ConsoleListItem>();
        for (var i = 0; i < _downstreamConfigs.Data.Count; i++)
        {
            o.Add(new ConsoleListItem($"{i + 1}", _downstreamConfigs.Data[i].Label));
        }

        BissConsole.WriteList(o);

        var id = Convert.ToInt32(BissConsole.ReadText("Choose: "));
        var config = _downstreamConfigs.Data[id - 1];

        foreach (var property in config.Properties)
        {
            property.Value = BissConsole.ReadText($"{property.Label}: ");
        }

        var result = _globalConfigs.ProcessData(config);
        Console.WriteLine($"{((IBissModel) result).ToJson()}");

        return Task.CompletedTask;
    }
}