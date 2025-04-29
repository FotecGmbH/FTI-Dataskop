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
using BDA.Common.Exchange.GatewayService;
using BDA.GatewayService.Interfaces;
using Biss.Interfaces;
using Biss.Log.Producer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BDA.GatewayService;

/// <summary>
///     <para>Aktuell via SignalR verbundene Gateways</para>
///     Klasse GetwayConnectedClientsManager. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayConnectedClientsManager : IGatewayConnectedClientsManager
{
    private readonly object _addOrUpdateLock = new object();
    private readonly IHubContext<GatewayHub> _hubcontext;

    /// <summary>
    ///     Connection Id (zb via SignalR) an aktuell angemeldetes Gateway zuweisen und merken
    /// </summary>
    protected readonly Dictionary<string, ExHubGatewayInfos> ClientData = new Dictionary<string, ExHubGatewayInfos>();

    /// <summary>
    ///     Aktuell via SignalR verbundene Gateways
    /// </summary>
    /// <param name="hubcontext"></param>
    public GatewayConnectedClientsManager(IHubContext<GatewayHub> hubcontext)
    {
        _hubcontext = hubcontext;
    }

    /// <summary>
    ///     Daten senden
    /// </summary>
    /// <param name="hubContextId"></param>
    /// <param name="method"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task<bool> Send(string hubContextId, string method, IBissSerialize data)
    {
        try
        {
            await _hubcontext.Clients.Client(hubContextId).SendAsync(method, data).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(GatewayConnectedClientsManager)}]({nameof(Send)}): Dc-SignalR Send Message Error: {e}");
            return false;
        }

        return true;
    }

    #region Interface Implementations

    /// <summary>
    ///     Alle aktuell angemeldeten Gateways
    /// </summary>
    /// <returns></returns>
    public List<ExHubGatewayInfos> GetGateways()
    {
        lock (_addOrUpdateLock)
        {
            return ClientData.Values.ToList();
        }
    }

    /// <summary>
    ///     Infos zu einem Gerät anlegen oder aktualisieren
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    /// <param name="infos">Infos zum Gerät</param>
    public void AddOrUpdateGateway(string sessionId, ExHubGatewayInfos infos)
    {
        lock (_addOrUpdateLock)
        {
            // ReSharper disable once RedundantDictionaryContainsKeyBeforeAdding
            if (ClientData.ContainsKey(sessionId))
            {
                ClientData[sessionId] = infos;
            }
            else
            {
                ClientData.Add(sessionId, infos);
            }
        }
    }

    /// <summary>
    ///     Ein Gerät entfernen
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    public void RemoveGateway(string sessionId)
    {
        lock (_addOrUpdateLock)
        {
            if (ClientData.ContainsKey(sessionId))
            {
                ClientData.Remove(sessionId);
            }
        }
    }

    /// <summary>
    ///     Aktuelle Infos zu einer Session
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    /// <returns></returns>
    public ExHubGatewayInfos GetGatewayInfos(string sessionId)
    {
        lock (_addOrUpdateLock)
        {
            if (!ClientData.ContainsKey(sessionId))
            {
                throw new NullReferenceException($"[{nameof(GatewayConnectedClientsManager)}]({nameof(GetGatewayInfos)}): BUG! Darf nicht sein! Bitte mit MKo besprechen!");
            }

            return ClientData[sessionId];
        }
    }

    /// <summary>
    ///     Gateway gerade via SignalR verbunden?
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <returns></returns>
    public bool IsGatewayOnline(long gatewayId)
    {
        string? t;
        lock (_addOrUpdateLock)
        {
            t = ClientData.FirstOrDefault(f => f.Value.GatewayId == gatewayId).Key;
        }

        return !string.IsNullOrEmpty(t);
    }

    /// <summary>
    ///     Daten senden
    /// </summary>
    /// <param name="gatewayId">Db Id des Gateway</param>
    /// <param name="method">Name der Methode - mit nameof(GatewayConstants.xxxxx)</param>
    /// <param name="data">Daten welche gesendet werden sollen</param>
    /// <returns></returns>
    public Task<bool> Send(long gatewayId, string method, IBissModel data)
    {
        if (ClientData == null!)
        {
            throw new Exception($"[{nameof(GatewayConnectedClientsManager)}]({nameof(Send)}): {nameof(ClientData)} is null. Can not send Data!");
        }

        string? t;
        lock (_addOrUpdateLock)
        {
            t = ClientData.FirstOrDefault(f => f.Value.GatewayId == gatewayId).Key;
        }

        if (string.IsNullOrEmpty(t))
        {
            Logging.Log.LogWarning($"[{nameof(GatewayConnectedClientsManager)}]({nameof(Send)}): Gateway with Id {gatewayId} is offline. Can not send Data!");
            return Task.FromResult(false);
        }

        return Send(t, method, data);
    }

    /// <summary>
    ///     Config an ein Gateway senden
    /// </summary>
    /// <param name="gatewayId">Db Id des Gateway</param>
    /// <param name="config">(Neue) Config</param>
    /// <returns></returns>
    public Task<bool> SendConfig(long gatewayId, ExGwServiceGatewayConfig config)
    {
        return Send(gatewayId, nameof(GatewayConstants.Conf), config);
    }

    /// <summary>
    ///     Downlink Message an ein Gateway fuer ein iot device senden
    /// </summary>
    /// <param name="gatewayId">Db Id des Gateway</param>
    /// <param name="message">Downlink nachricht</param>
    /// <returns></returns>
    public Task<bool> SendDownlinkMessage(long gatewayId, ExDownlinkMessageForDevice message)
    {
        return Send(gatewayId, nameof(GatewayConstants.Downlink), message);
    }

    #endregion
}