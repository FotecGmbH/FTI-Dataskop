// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Gateway.Com.Base.Model;
using Biss.Log.Producer;
using Biss.Serialize;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Com.Base;

/// <summary>
///     <para>Basiskommunikation um mit dem BDA Service zu kommunizieren (via Signal R)</para>
///     Klasse GatewayComBase. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public abstract partial class GatewayComBase
{
    private static LogLevel _logLevel = LogLevel.Warning;
    private static string _baseUrl = string.Empty;
    private static bool _initializeExecuted;
    private static HubConnection? _hubConnection;
    private static ConnectionState _signalRState = ConnectionState.Closed;
    private static readonly CancellationTokenSource CtsCloseHub = new CancellationTokenSource();
    private static bool _invalidConfig;

    /// <summary>
    ///     LocalDirectory
    /// </summary>
    protected static DirectoryInfo LocalDirectory = null!;

    /// <summary>
    ///     Basiskommunikation um mit dem BDA Service zu kommunizieren (via Signal R)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    // ReSharper disable once UnusedMember.Global
    protected GatewayComBase(AssemblyLoadContext? ctx)
    {
        if (!_initializeExecuted)
        {
            throw new InvalidOperationException("[GatewayComBase] Call GatewayComBase.Initialize before creating ComObjects");
        }

        _ctx = ctx;
    }

    #region Properties

    /// <summary>
    ///     Verbunden
    /// </summary>
    public static bool IsConnected2Service => _signalRState == ConnectionState.Open;

    #endregion

    /// <summary>
    ///     Verbindung zum BDA Service initialisieren
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="localDirectory">Lokale Pfad für zusätzliche Daten (Iot Device Configs, Cahces, ...)</param>
    /// <param name="logLevel"></param>
    /// <exception cref="NullReferenceException"></exception>
    public static void InitializeHubConnection(string baseUrl, DirectoryInfo localDirectory, LogLevel logLevel = LogLevel.Warning)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new NullReferenceException($"[{nameof(GatewayComBase)}]({nameof(InitializeHubConnection)}): SignalR {nameof(baseUrl)} is null or empty!");
        }

        LocalDirectory = localDirectory;

        _logLevel = logLevel;
        _baseUrl = baseUrl.EndsWith("/") ? $"{baseUrl}{GatewayConstants.HubName}" : $"{baseUrl}/{GatewayConstants.HubName}";
        _initializeExecuted = true;

        OpenGatewayConnection();
    }

    /// <summary>
    ///     Gateway a,m Sdp Service anmelden
    /// </summary>
    /// <returns></returns>
    public static async Task<ExGatwayRegisterResult> RegisterGateway(ExHubGatewayInfos currentInfos)
    {
        if (_hubConnection == null)
        {
            throw new NoNullAllowedException($"[{nameof(GatewayComBase)}]({nameof(RegisterGateway)}): {nameof(_hubConnection)} is null!");
        }

        if (!IsConnected2Service)
        {
            Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(RegisterGateway)}):Gatway not Connected to Service!");
            return null!;
        }

        try
        {
            var r = await _hubConnection.InvokeAsync<ExGatwayRegisterResult>(nameof(GatewayConstants.Reg), currentInfos).ConfigureAwait(true);
            if (r.Invalid)
            {
                Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(RegisterGateway)}): Invalid Secret for Gateway Db Id {r.DbId}");
                _invalidConfig = true;
                await CloseGatewayConnection().ConfigureAwait(false);
            }

            return r;
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(RegisterGateway)}): RegisterDevice Error: {e}");
            return null!;
        }
    }

    /// <summary>
    ///     Ereignis für Verbindungsstatus
    /// </summary>
#pragma warning disable CA1003 // Use generic event handler instances
    public static event EventHandler<EnumGatewayConnectionStates> GatewayConnectionChanged = null!;
#pragma warning restore CA1003 // Use generic event handler instances


    /// <summary>
    ///     Ereignis für neue Konfig vom Server empfangen
    /// </summary>
    public static event EventHandler<NewConfigEventArgs>? NewConfigForGateway;

    /// <summary>
    ///     Ereignis für neue Konfig vom Server empfangen
    /// </summary>
    public static event EventHandler<ExDownlinkMessageForDevice>? SendDownlinkMessage;

    /// <summary>
    ///     Verbindung schließen beim beenden der Gateway App
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> CloseGatewayConnection()
    {
        if (_signalRState != ConnectionState.Open)
        {
            return true;
        }

        _signalRState = ConnectionState.Closed;
        OnConnectionChanged(EnumGatewayConnectionStates.Disconecting);

        if (_hubConnection != null)
        {
            _hubConnection.Closed -= ConnectionClosedByHost;
        }

        try
        {
            await (_hubConnection?.StopAsync()!).ConfigureAwait(true);
            await _hubConnection.DisposeAsync().ConfigureAwait(true);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(CloseGatewayConnection)}): Gateway disconnecting Error: {e}");
            return false;
        }
        finally
        {
            OnConnectionChanged(EnumGatewayConnectionStates.Disconnected);
            _hubConnection = null;
        }

        return true;
    }

    /// <summary>
    ///     Messwerte an Server übertragen. Falls offline werden diese lokal gespeichert
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task TransferToBdaServiceMesurements(ExValuesTransfer data)
    {
        if (!IsConnected2Service)
        {
            //ToDo MKo Caching
            return;
        }

        try
        {
            await _hubConnection.SendAsync(nameof(GatewayConstants.Send), data, CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(TransferToBdaServiceMesurements)}): {e}");
        }
    }

    /// <summary>
    ///     Statusupdate eines Iot Geräts welches der Gateway verwaltet
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected static async Task UpdateIotDeviceState(ExHubIotDeviceState state)
    {
        if (!IsConnected2Service)
        {
            //ToDo MKo Caching
            return;
        }

        try
        {
            await _hubConnection.SendAsync(nameof(GatewayConstants.Du), state, CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(UpdateIotDeviceState)}): {e}");
        }
    }

    /// <summary>
    ///     Methode von Ereignis für neue Konfig vom Server empfangen
    /// </summary>
    /// <param name="eventData"></param>
    protected static void OnNewConfig(NewConfigEventArgs eventData)
    {
        var handler = NewConfigForGateway;
        handler?.Invoke(null, eventData);
    }

    /// <summary>
    ///     Methode von Ereignis für Verbindungsstatus des Gatewys
    /// </summary>
    /// <param name="eventData"></param>
    protected static void OnConnectionChanged(EnumGatewayConnectionStates eventData)
    {
        Logging.Log.LogTrace($"[{nameof(GatewayComBase)}]({nameof(OnConnectionChanged)}): Gateway Connection State: {eventData}");
        var handler = GatewayConnectionChanged;
        // ReSharper disable once ConstantConditionalAccessQualifier
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        handler?.Invoke(null, eventData);
    }

    /// <summary>
    /// NewDownlinkFromHub
    /// </summary>
    /// <param name="downlinkMessage"></param>
    private static void NewDownlinkFromHub(ExDownlinkMessageForDevice downlinkMessage)
    {
        var handler = SendDownlinkMessage;
        handler?.Invoke(null, downlinkMessage);
    }

    /// <summary>
    ///     Verbindung öffnen
    /// </summary>
    /// <returns></returns>
    private static void OpenGatewayConnection()
    {
        if (_signalRState == ConnectionState.Open)
        {
            return;
        }

        if (_invalidConfig)
        {
            Logging.Log.LogWarning($"[{nameof(GatewayComBase)}]({nameof(OpenGatewayConnection)}): Config Invalid! Can not open connection!");
            return;
        }

        Task.Run(async () =>
        {
            do
            {
                if (_signalRState == ConnectionState.Open)
                {
                    break;
                }

                OnConnectionChanged(EnumGatewayConnectionStates.Connecting);
                Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(OpenGatewayConnection)}): Connecting to {_baseUrl}");
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_baseUrl)
                    .AddJsonProtocol()
                    .ConfigureLogging(logging => { logging.AddDebug().SetMinimumLevel(_logLevel); })
                    .Build();

                _hubConnection.Closed += ConnectionClosedByHost;


                try
                {
                    _signalRState = ConnectionState.Connecting;
                    await _hubConnection.StartAsync(CtsCloseHub.Token).ConfigureAwait(true);

                    _hubConnection.On<ExGwServiceGatewayConfig>(nameof(GatewayConstants.Conf), NewConfigFromHub);
                    _hubConnection.On<ExDownlinkMessageForDevice>(nameof(GatewayConstants.Downlink), NewDownlinkFromHub);

                    _signalRState = ConnectionState.Open;
                    OnConnectionChanged(EnumGatewayConnectionStates.Connected);

                    break;
                }
                catch (Exception e)
                {
                    _hubConnection.Closed -= ConnectionClosedByHost;
                    _signalRState = ConnectionState.Closed;
                    Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(OpenGatewayConnection)}): Gateway: SignalR Connect Error: {e}");
                    OnConnectionChanged(EnumGatewayConnectionStates.Disconnected);
                }

                await Task.Delay(5000, CtsCloseHub.Token).ConfigureAwait(false);
            } while (!CtsCloseHub.IsCancellationRequested);
        });
    }


    /// <summary>
    ///     Neue Config vom Server empfangen
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void NewConfigFromHub(ExGwServiceGatewayConfig config)
    {
        Logging.Log.LogInfo($"[{nameof(GatewayComBase)}]({nameof(NewConfigFromHub)}): Gateway: Received new config with version {config.ConfigVersion}");
        OnNewConfig(new NewConfigEventArgs {GatewayConfig = config});
    }

    /// <summary>
    ///     Verbindung wurde vom Host abgebrochen
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private static Task ConnectionClosedByHost(Exception arg)
    {
        _signalRState = ConnectionState.Closed;
        OnConnectionChanged(EnumGatewayConnectionStates.Disconnected);

        OpenGatewayConnection();

        return Task.CompletedTask;
    }

    #region Eventuell noch fertig implementieren durch Mko?

    private static async IAsyncEnumerable<byte[]> ClientStreamData(Stream stream, int size = 1024)
    {
        var start = 0;
        var buffer = new byte[size];
        while (stream.CanRead)
        {
            var bytes = await stream.ReadAsync(buffer, start, size).ConfigureAwait(true);
            if (bytes == 0)
            {
                break;
            }

            yield return buffer;
        }
    }

#pragma warning disable IDE0051 // Remove unused private members
    // ReSharper disable once UnusedMember.Local
    private static async Task TransferMesurementsAsStreaming(ExValuesTransfer data)
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (!IsConnected2Service)
        {
            return;
        }

        var stringData = data.ToJson();
        var bytes = Encoding.UTF32.GetBytes(stringData);
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var ms = new MemoryStream(bytes);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        try
        {
            await _hubConnection.SendAsync(nameof(GatewayConstants.Send), ClientStreamData(ms, bytes.Length), CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayComBase)}]({nameof(TransferMesurementsAsStreaming)}): {e}");
            throw;
        }
    }

    #endregion
}