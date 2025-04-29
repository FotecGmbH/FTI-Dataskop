// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDA.CliApp;

/// <summary>
///     <para>CliAppBase Kommt in Biss.Cli</para>
///     Klasse CliAppBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public abstract class CliAppBase
{
    private readonly CliCommands _rootCommands = new();
    private CliCommands _currentCommands = null!;
    private CliCommand? _executingCommand;

    /// <summary>
    ///     Commands bei Bedarf noch manuell anzeigen
    /// </summary>
    public void ShowCommands(bool onlyIfRoot = true)
    {
        if (onlyIfRoot && _currentCommands.IsRoot)
        {
            _currentCommands.ShowCommands();
        }
        else if (!onlyIfRoot)
        {
            _currentCommands.ShowCommands();
        }
    }

    /// <summary>
    ///     Enlosschleife bis ESC
    /// </summary>
    /// <returns></returns>
    public async Task<int> RunCliApp()
    {
        _currentCommands = _rootCommands;
        _currentCommands.RunBeforShowCommands = InitCommands(_rootCommands);
        await StartCliApp().ConfigureAwait(false);

        _currentCommands.ShowCommands();
        do
        {
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.Escape || (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C))
            {
                if (_executingCommand != null && _executingCommand.EndAction != null)
                {
                    await _executingCommand.EndAction.Invoke().ConfigureAwait(false);
                }


                if (_currentCommands.Parent == null)
                {
                    //End Loop
                    break;
                }

                _currentCommands = _currentCommands.Parent;
                _currentCommands.ShowCommands();
                continue;
            }

            var c = _currentCommands.Commands.FirstOrDefault(f => f.Key == key.Key);
            if (c != null)
            {
                _executingCommand = c;
                var tmp = _currentCommands.GetSubCommandsForKey(key.Key);
                if (tmp != null)
                {
                    _currentCommands = tmp;
                    if (_executingCommand.Action != null!)
                    {
                        await _executingCommand.Action.Invoke().ConfigureAwait(false);
                    }

                    _currentCommands.ShowCommands();
                }
                else
                {
                    await _executingCommand.Action.Invoke().ConfigureAwait(false);
                    if (_executingCommand.EndAction != null)
                    {
                        await _executingCommand.EndAction.Invoke().ConfigureAwait(false);
                    }

                    _executingCommand = null;
                    Console.WriteLine();
                }
            }
        } while (true);


        return await StopCliApp().ConfigureAwait(false);
    }

    /// <summary>
    ///     CLI - Menübefehle initialisieren (ESC-Beenden, H-Hilfe, L-Logging aktivieren) werden automatisch erzeugt
    /// </summary>
    public abstract Action? InitCommands(CliCommands rootCliCommands);

    /// <summary>
    ///     App starten
    /// </summary>
    public abstract Task StartCliApp();

    /// <summary>
    ///     App beenden
    /// </summary>
    public abstract Task<int> StopCliApp();
}