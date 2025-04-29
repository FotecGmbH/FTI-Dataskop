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
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.IotDevice.Core;
using Biss.Cli;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.Extensions.Logging;


namespace BDA.IotDevice.App;

/// <summary>
///     <para>Gateway Console Applikation</para>
///     Klasse App. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class IotDeviceCliApp : CliAppBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly string[] _args;
    private readonly IotDeviceCore _stateMachine = null!;

    /// <summary>
    ///     IotDevice wird als Console-Applikation gestartet
    /// </summary>
    /// <param name="args"></param>
    public IotDeviceCliApp(string[] args)
    {
        _args = args;
        // _stateMachine = new IotDeviceStateMachine(ExGwServiceIotDeviceConfig.GetTestConfig());

        _stateMachine = new IotDeviceCore(NameIoTDeviceAtFirstLaunch);

        _stateMachine.RefreshCliUi += StateMachineOnRefreshCliUi;
    }

    /// <summary>
    ///     Beim ersten Start des IotDevices diesem wenn geünscht einen Namen geben
    /// </summary>
    /// <param name="generatedName"></param>
    /// <returns></returns>
    public IoTDeviceCoreFirstLaunchInfos NameIoTDeviceAtFirstLaunch(string generatedName)
    {
        var result = new IoTDeviceCoreFirstLaunchInfos();

        Console.Clear();
        ConsoleColor.Cyan.WriteLine("BDA IoTDevice (C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH");
        ConsoleColor.Cyan.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");


        ConsoleColor.Green.WriteLine("First Launch of your new BDA-IoTDevice ...");

        var ind = generatedName.IndexOf('-'); // generated name is in the format  "IoTDevice-2348f81" so first part is generated name and second part is generated secret
        var generatedNamee = generatedName.Substring(0, ind);
        var generatedSecret = generatedName.Substring(ind, generatedName.Length - 1 - ind);

        result.Name = BissConsole.ReadText($"IoTDevice Name (Enter for \"{generatedNamee}\"): ");
        if (string.IsNullOrEmpty(result.Name))
        {
            result.Name = generatedNamee;
        }

        result.Secret = BissConsole.ReadText($"IoTDevice Secret (Enter for \"...{generatedSecret}\"): ");


        while (true)
        {
            ConsoleColor.Gray.WriteLine("IotDevice UpstreamCommunication: Press: [1] for TCP; [] ...");
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.D1)
            {
                result.UpstreamType = EnumIotDeviceUpstreamTypes.Tcp;
                result.HostNOrIPAddess = BissConsole.ReadText("Enter the IPAddress of the Gateway e.g. [192.168.0.4], OR Enter the Hostname of the Gateway: ");
                break;
            }

            if (key.Key == ConsoleKey.D2)
            {
                result.UpstreamType = EnumIotDeviceUpstreamTypes.Serial;
                break;
            }

            ConsoleColor.Red.WriteLine("You pressed a wrong key, please onky use the provided keys");
        }

        //ConsoleColor.Green.WriteLine("Check Location of IoTDevice ...");
        //var p = LocationHelper.GetPosition(); TODO location for iotdevice

        return result;
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

        rootCliCommands.Commands.Add(new CliCommand
        {
            Key = ConsoleKey.D2, Description = "IPAddresse oder Hostname ändern", Action = () =>
            {
                _stateMachine.Config.AdditionalConfiguration = BissConsole.ReadText("Neue IPAddresse oder HostName: ").ToJson();
                _stateMachine.ReceivedConfig(_stateMachine.Config);
                _stateMachine.InitUpstream();
                return Task.CompletedTask;
            },
            EndAction = () => { return Task.CompletedTask; }
        });

        var a = new Action(() =>
        {
            ConsoleColor.Cyan.WriteLine("BDA IotDevice (C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH");
            ConsoleColor.Cyan.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var color = ConsoleColor.DarkGreen;
            switch (_stateMachine.ConnectionState)
            {
                case EnumIoTDeviceConnectionStates.Connected:
                    break;
                case EnumIoTDeviceConnectionStates.Disconnected:
                    color = ConsoleColor.DarkRed;
                    break;
                case EnumIoTDeviceConnectionStates.Connecting:
                case EnumIoTDeviceConnectionStates.Disconecting:
                    color = ConsoleColor.DarkYellow;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"[{nameof(IotDeviceCliApp)}]({nameof(StateMachineOnRefreshCliUi)}): {nameof(_stateMachine.ConnectionState)} out of range");
            }

            var o = new List<ConsoleListItem>();
            o.Add(new ConsoleListItem("Verbindungsstatus", _stateMachine.ConnectionState.ToString(), color));

            o.Add(new ConsoleListItem("Serial Number", _stateMachine.Config.DbId.ToString()));
            o.Add(new ConsoleListItem("Secret", _stateMachine.Config.Secret));
            o.Add(new ConsoleListItem("Name", _stateMachine.Config.Name));
            o.Add(new ConsoleListItem("Config-Version", _stateMachine.Config.ConfigVersion.ToString()));
            o.Add(new ConsoleListItem("HostName / IPAddresse", _stateMachine.Config.AdditionalConfiguration));

            o.Add(new ConsoleListItem("Measure-Interval[100ms]", _stateMachine.Config.MeasurementInterval.ToString()));
            o.Add(new ConsoleListItem("SendInterval[s]", _stateMachine.Config.TransmissionInterval.ToString()));
            o.Add(new ConsoleListItem("Measurements-Count", _stateMachine.Config.MeasurementDefinition.Count.ToString()));
            BissConsole.WriteList(o);
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

    /// <summary>
    ///     Refresh CLI UI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StateMachineOnRefreshCliUi(object? sender, EventArgs e)
    {
        try
        {
            ShowCommands();
        }
        catch (Exception)
        {
            Logging.Log.LogWarning($"[{nameof(IotDeviceCliApp)}]({nameof(StateMachineOnRefreshCliUi)}): showcommand was not successfull");
        }
    }
}