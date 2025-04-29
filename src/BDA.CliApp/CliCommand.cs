// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biss.Cli;
using Biss.Core.Logging.Events;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.CliApp;

/// <summary>
///     <para>Alle Befehle</para>
///     Klasse CliCommands. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class CliCommands
{
    private static BissEventsLoggerConfiguration? _bisslogConfig;
    private static bool _liveLogging;

    private static readonly object _showLock = new object();
    private readonly Dictionary<ConsoleKey, CliCommands> _subCommands = new();

    #region Properties

    /// <summary>
    ///     Aktuelle Befehel in dieser Ebene
    /// </summary>
    public List<CliCommand> Commands { get; } = new();

    /// <summary>
    ///     Sind das die ROOT Befehele?
    /// </summary>
    public bool IsRoot => Parent == null;

    /// <summary>
    ///     Parent Befehle (null beim Root)
    /// </summary>
    public CliCommands? Parent { get; set; }

    /// <summary>
    ///     Wird immer ausgeführt bevor die Commands angezeigt werden
    /// </summary>
    public Action? RunBeforShowCommands { get; set; }

    #endregion

    /// <summary>
    ///     Ein Sub-Befehl in einfügen
    /// </summary>
    /// <param name="key"></param>
    /// <param name="c"></param>
    public void AddSubCommand(ConsoleKey key, CliCommand c)
    {
        lock (_showLock)
        {
            if (Commands.All(a => a.Key != key))
            {
                throw new Exception($"Wrong configuration - add first the command with key {key} into Commands List");
            }

            CliCommands sc;
            if (_subCommands.ContainsKey(key))
            {
                sc = _subCommands[key];
            }
            else
            {
                sc = new CliCommands
                {
                    Parent = this
                };
                _subCommands.Add(key, sc);
            }

            sc.Commands.Add(c);
        }
    }

    /// <summary>
    ///     Subcommands für eine bestimmte Taste
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public CliCommands? GetSubCommandsForKey(ConsoleKey key)
    {
        // ReSharper disable once RedundantAssignment
        CliCommands? result = null;
        _subCommands.TryGetValue(key, out result);
        return result;
    }

    /// <summary>
    ///     Mögliche Befehle anzeigen
    /// </summary>
    public void ShowCommands()
    {
        lock (_showLock)
        {
            Console.Clear();
            RunBeforShowCommands?.Invoke();

            if (IsRoot)
            {
                if (Commands.All(c => c.Key != ConsoleKey.L))
                {
                    var l = new CliCommand {Key = ConsoleKey.L, Description = "Start Logging", Action = StartLogging, EndAction = StopLogging};
                    Commands.Add(l);
                    AddSubCommand(ConsoleKey.L, new CliCommand {Description = "Clean Console", Key = ConsoleKey.C, Action = CleanConsole});
                }
            }

            if (Commands.All(c => c.Key != ConsoleKey.H))
            {
                Commands.Add(new CliCommand {Key = ConsoleKey.H, Description = "Show current commands", Action = ShowHelp});
            }

            if (Commands.All(c => c.Key != ConsoleKey.Escape))
            {
                if (IsRoot)
                {
                    Commands.Add(new CliCommand {Key = ConsoleKey.Escape, Description = "Exit App"});
                }
                else
                {
                    Commands.Add(new CliCommand {Key = ConsoleKey.Escape, Description = "Back"});
                }
            }

            var o = Commands.Select(s => s.ToConsoleListItem).ToList();
            BissConsole.WriteList(o);
            ConsoleColor.DarkGreen.Write("Choose Command: ");

            if (_liveLogging)
            {
                ConsoleColor.DarkGreen.WriteLine("\r\nLive Logging ...\r\n");
            }
        }
    }

    /// <summary>
    ///     Commands (wieder) anzeigen
    /// </summary>
    /// <returns></returns>
    private Task ShowHelp()
    {
        ShowCommands();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Logging beenden
    /// </summary>
    /// <returns></returns>
    private Task StopLogging()
    {
        _liveLogging = false;
        _bisslogConfig!.NewLogEntry -= BisslogConfigOnNewLogEntry;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Console löschen
    /// </summary>
    /// <returns></returns>
    private Task CleanConsole()
    {
        Console.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Logging starten
    /// </summary>
    /// <returns></returns>
    private Task StartLogging()
    {
        if (_bisslogConfig == null)
        {
            _bisslogConfig = new BissEventsLoggerConfiguration {LogLevel = LogLevel.Trace};
            Logging.Init(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace).AddProvider(new BissEventsLoggerProvider(_bisslogConfig)).SetMinimumLevel(LogLevel.Trace));
        }

        _liveLogging = true;
        _bisslogConfig.NewLogEntry += BisslogConfigOnNewLogEntry;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Log in Console ausgeben
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void BisslogConfigOnNewLogEntry(object? sender, BissEventsLoggerEventArgs e)
    {
        var o = $"{e.TimeStamp.ToShortTimeString()} {e.Message}";

        switch (e.LogLevel)
        {
            case LogLevel.Trace:
                ConsoleColor.Gray.WriteLine(o);
                break;
            case LogLevel.Debug:
                ConsoleColor.Gray.WriteLine(o);
                break;
            case LogLevel.Information:
                ConsoleColor.Blue.WriteLine(o);
                break;
            case LogLevel.Warning:
                ConsoleColor.Yellow.WriteLine(o);
                break;
            case LogLevel.Error:
                ConsoleColor.Red.WriteLine(o);
                break;
            case LogLevel.Critical:
                ConsoleColor.DarkRed.WriteLine(o);
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

/// <summary>
///     <para>Ein einzeler Befehl (Taste)</para>
///     Klasse CliCommand. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class CliCommand
{
    #region Properties

    //public List<CliCommand> SubCommands { get; set; } = new List<CliCommand>();

    //public List<CliCommand> Parent { get; set; } = null!;


    /// <summary>
    ///     Beschreibung des Befehls
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Taste
    /// </summary>
    public ConsoleKey Key { get; set; }

    /// <summary>
    ///     Funktion
    /// </summary>
    public Func<Task> Action { get; set; } = null!;

    /// <summary>
    ///     Funktion
    /// </summary>
    public Func<Task>? EndAction { get; set; }


    /// <summary>
    ///     DESCRIPTION
    /// </summary>
    public ConsoleListItem ToConsoleListItem => new($"{Description}", Key.ToString());

    #endregion
}