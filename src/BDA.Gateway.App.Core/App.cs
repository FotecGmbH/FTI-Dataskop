// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BDA.CliApp;
using BDA.Common.Exchange.Enum;
using BDA.Gateway.Com.Base;
using BDA.Gateway.Core;
using Biss.Cli;

namespace BDA.Gateway.App.Core;

/// <summary>
///     <para>Gateway Console Applikation</para>
///     Klasse App. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayCliApp : CliAppBase
{
    // ReSharper disable once NotAccessedField.Local
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly string[] _args;
    private readonly GatewayCore _gwc;
    private EnumGatewayConnectionStates _connectionStateBase = EnumGatewayConnectionStates.Disconnected;
    private bool _simpleMode;

    /// <summary>
    ///     Gateway als CLI Applikation
    /// </summary>
    /// <param name="args"></param>
    public GatewayCliApp(string[] args)
    {
        _args = args;
        var signalHostString = "dcsignalhost:";
        string signalHostConnectionString = null!;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        Uri? uri = null;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        if (_args.Length != 0)
        {
            if (_args.Any(arg => arg.ToLower().Contains("mode:simple"))) // Option fuer simple mode
            {
                _simpleMode = true;
            }

            if (_args.Any(arg => arg.ToLower().Contains(signalHostString))) // Wenn gewollt kann connectionstring fuer dc signal Host angegeben werden
            {
                var signalHostArg = String.Concat(_args.FirstOrDefault(arg => arg.ToLower().Contains(signalHostString))!.Skip(signalHostString.Length));
                if (Uri.TryCreate(signalHostArg, UriKind.RelativeOrAbsolute, out _))
                {
                    signalHostConnectionString = signalHostArg;
                }
            }
        }

        //if (signalHostConnectionString == null)
        //    signalHostConnectionString = AppSettings.Current().DcSignalHost;

        if (_simpleMode)
        {
            _gwc = new GatewayCore(signalHostConnectionString, FirstLaunchConfiguration, NameGatewayAtFirstLaunch);
        }
        else
        {
            _gwc = new GatewayCore(signalHostConnectionString, NameGatewayAtFirstLaunch);
        }

        _gwc.RefreshCliUi += GwcOnRefreshCliUi;
        _gwc.Restart += GwcOnRestart;

        GatewayComBase.GatewayConnectionChanged += GatewayComBaseOnConnectionChanged;
    }

    /// <summary>
    ///     Beim ersten Start des Gateway diesem wenn geünscht einen Namen geben
    /// </summary>
    /// <param name="generatedName"></param>
    /// <returns></returns>
    public GatewayCoreFirstLaunchInfos NameGatewayAtFirstLaunch(string generatedName)
    {
        // ReSharper disable once RedundantAssignment
        var result = string.Empty;

        Console.Clear();
        ConsoleColor.Cyan.WriteLine("BDA Gateway (C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH");
        ConsoleColor.Cyan.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");


        ConsoleColor.Green.WriteLine("First Launch of your new BDA-Gateway ...");
        result = BissConsole.ReadText($"Gateway name (Enter for \"{generatedName}\"): ");
        var desc = BissConsole.ReadText("Gateway description (Enter for empty): ");

        ConsoleColor.Green.WriteLine("Check Location of gateway ...");
        var p = LocationHelper.GetPosition();

        return new GatewayCoreFirstLaunchInfos
        {
            Name = string.IsNullOrEmpty(result) ? generatedName : result,
            Description = desc,
            Position = p
        };
    }

    /// <summary>
    ///     Beim ersten Start des Gateway für den Simple Mode
    ///     =>sprich anstatt ein neues Gateway zu erzeugen wird der pfad zu der Konfigurationsdatei angegeben
    ///     =>über Eingabe "advanced" gelangt man zu den Standard
    /// </summary>
    /// <returns></returns>
    public string FirstLaunchConfiguration()
    {
        Console.Clear();
        ConsoleColor.Gray.Write("Geben Sie bitte den ");
        ConsoleColor.Green.Write("Pfad zu ihrer Konfigurationsdatei ");
        ConsoleColor.Gray.WriteLine("an");
        ConsoleColor.Gray.WriteLine("(Format = C:\\temp\\gwconfig.json) (oder geben Sie \"advanced\" ein um in den advanced mode zu gelangen)");
        ConsoleColor.Gray.WriteLine("Sie haben noch keine Konfigurationsdatei? Einfach auf der Webseite oder ihrer App den Button \"Download Gateway Konfiguration\" klicken");

        var res = BissConsole.ReadText("Pfad: ");

        while (true)
        {
            if (res == "\"advanced\"" || res == "advanced")
            {
                return "advanced";
            }

            if (File.Exists(res))
            {
                return res;
            }

            ConsoleColor.DarkYellow.WriteLine("Falsche eingabe, bitte überprüfen Sie ob richtig geschrieben und versuchen Sie erneut");
            res = BissConsole.ReadText("(Format = C:\\Downloads\\gwconfig.json): ");
        }
    }

    /// <summary>
    ///     CLI - Menübefehle initialisieren (ESC-Beenden, H-Hilfe, L-Logging aktivieren) werden automatisch erzeugt
    /// </summary>
    public override Action InitCommands(CliCommands rootCliCommands)
    {
        if (rootCliCommands == null!)
        {
            throw new ArgumentNullException(nameof(rootCliCommands));
        }

        // ReSharper disable once RedundantAssignment
        CliCommand simpleCommand = null!;
        Action advanceAction = null!;

        simpleCommand = new CliCommand
        {
            Key = ConsoleKey.D1,
            Description = "Advanced Mode",
            Action = () =>
            {
                _simpleMode = false;
                // ReSharper disable once AccessToModifiedClosure
                advanceAction.Invoke(); // fuegt alle commands von advanced hinzu
                _gwc.OnRefreshCliUi();
                return Task.CompletedTask;
            },
            EndAction = () => Task.CompletedTask
        };

        advanceAction = () =>
        {
            rootCliCommands.Commands.Clear();
            //Test Commands
            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.D1,
                Description = "Test Ebene 0 - 1",
                Action = () =>
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
            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.D2,
                Description = "Simple Mode",
                Action = () =>
                {
                    _simpleMode = true;
                    rootCliCommands.Commands.Clear();
                    rootCliCommands.Commands.Add(simpleCommand);
                    _gwc.OnRefreshCliUi();
                    return Task.CompletedTask;
                },
                EndAction = () => { return Task.CompletedTask; }
            });

            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.P,
                Description = "Stop Virtual Iot Devices",
                Action = () =>
                {
                    _gwc.StopAllVirtualIotDevices();
                    return Task.CompletedTask;
                },
                EndAction = () =>
                {
                    ShowCommands();
                    return Task.CompletedTask;
                }
            });
            rootCliCommands.Commands.Add(new CliCommand
            {
                Key = ConsoleKey.S,
                Description = "Start Virtual Iot Devices",
                Action = () =>
                {
                    _gwc.StartAllVirtualIotDevices();
                    return Task.CompletedTask;
                },
                EndAction = () =>
                {
                    ShowCommands();
                    return Task.CompletedTask;
                }
            });
        };

        if (_simpleMode)
        {
            rootCliCommands.Commands.Add(simpleCommand);
        }
        else
        {
            advanceAction.Invoke();
        }

        var a = new Action(ShowInfos);
        return a;
    }


    /// <summary>
    ///     App starten
    /// </summary>
    public override async Task StartCliApp()
    {
        await _gwc.Start().ConfigureAwait(true);
    }

    /// <summary>
    ///     App beenden
    /// </summary>
    public override async Task<int> StopCliApp()
    {
        await _gwc.Shutdown().ConfigureAwait(false);
        return 0;
    }

    private void GwcOnRestart(object? sender, EventArgs e)
    {
        Console.Clear();
        ConsoleColor.Red.WriteLine("BDA Gateway wurde gelöscht. Starte App neu.");

        //Start process, friendly name is something like MyApp.exe (from current bin directory)
        Process.Start(AppDomain.CurrentDomain.FriendlyName);

        //Close the current process
        Environment.Exit(0);
    }

    private void GwcOnRefreshCliUi(object? sender, EventArgs e)
    {
        ShowCommands();
    }

    private void GatewayComBaseOnConnectionChanged(object? sender, EnumGatewayConnectionStates e)
    {
        _connectionStateBase = e;
    }

    private void ShowInfos()
    {
        ConsoleColor.Cyan.WriteLine("BDA Gateway (C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH");
        ConsoleColor.Cyan.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

        var o = new List<ConsoleListItem>();

        Console.WriteLine();

        // Verbindungsstatus zum Server
        var color = ConsoleColor.DarkGreen;
        switch (_connectionStateBase)
        {
            case EnumGatewayConnectionStates.Connected:
                break;
            case EnumGatewayConnectionStates.Disconnected:
                color = ConsoleColor.DarkRed;
                break;
            case EnumGatewayConnectionStates.Connecting:
            case EnumGatewayConnectionStates.Disconecting:
                color = ConsoleColor.DarkYellow;
                break;
            default:
                throw new ArgumentOutOfRangeException($"[{nameof(GatewayCliApp)}]({nameof(ShowInfos)}): {nameof(_connectionStateBase)} out of range");
        }

        o.Add(new ConsoleListItem("Verbindungsstatus", _connectionStateBase.ToString(), color));


        var position = "?";
        if (_gwc.Config.Position != null)
        {
            position = "https://www.google.at/maps/@" + $"{_gwc.Config.Position.Latitude.ToString(new CultureInfo("en"))},{_gwc.Config.Position.Longitude.ToString(new CultureInfo("en"))}z";
        }

        //Config Infos
        if (!_simpleMode)
        {
            o.Add(new ConsoleListItem("Serial Number", _gwc.Config.DbId.ToString()));
            o.Add(new ConsoleListItem("Secret", _gwc.Config.Secret));
            o.Add(new ConsoleListItem("Name", _gwc.Config.Name));
            o.Add(new ConsoleListItem("Description", _gwc.Config.Description));
            o.Add(new ConsoleListItem("Position", position));
            o.Add(new ConsoleListItem("Config-Version", _gwc.Config.ConfigVersion.ToString()));

            foreach (var iot in _gwc.EmbeddedIotDevices)
            {
                o.Add(new ConsoleListItem($"Embedded Iot {iot.IotConfig.DbId} - {iot.IotConfig.Name}", $"Running: {iot.IsConnected}"));
            }
        }


        BissConsole.WriteList(o);
        Console.WriteLine();
    }
}