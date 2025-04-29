// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.GatewayService;
using Biss.Interfaces;

namespace BDA.GatewayService.Interfaces;

/// <summary>
///     IGatwayConnectedClientsManager
/// </summary>
public interface IGatewayConnectedClientsManager
{
    /// <summary>
    ///     Alle aktuell angemeldeten Gateways
    /// </summary>
    /// <returns></returns>
    List<ExHubGatewayInfos> GetGateways();

    /// <summary>
    ///     Infos zu einem Gerät anlegen oder aktualisieren
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    /// <param name="infos">Infos zum Gerät</param>
    void AddOrUpdateGateway(string sessionId, ExHubGatewayInfos infos);

    /// <summary>
    ///     Ein Gerät entfernen
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    void RemoveGateway(string sessionId);

    /// <summary>
    ///     Aktuelle Infos zu einer Session
    /// </summary>
    /// <param name="sessionId">Eindeutige Id der Session - zB Context.ConnectionId bei SignalR</param>
    /// <returns></returns>
    ExHubGatewayInfos GetGatewayInfos(string sessionId);

    /// <summary>
    ///     Gateway gerade via SignalR verbunden?
    /// </summary>
    /// <param name="gatewayId"></param>
    /// <returns></returns>
    bool IsGatewayOnline(long gatewayId);

    /// <summary>
    ///     Daten senden
    /// </summary>
    /// <param name="gatewayId">Db Id des Gateway</param>
    /// <param name="method">Name der Methode - mit nameof(GatewayConstants.xxxxx)</param>
    /// <param name="data">Daten welche gesendet werden sollen</param>
    /// <returns></returns>
    Task<bool> Send(long gatewayId, string method, IBissModel data);

    /// <summary>
    ///     Config an ein Gateway senden
    /// </summary>
    /// <param name="gatewayId">Db Id des Gateway</param>
    /// <param name="config">(Neue) Config</param>
    /// <returns></returns>
    Task<bool> SendConfig(long gatewayId, ExGwServiceGatewayConfig config);

    /// <summary>
    ///     Downlink Message an ein Gateway fuer ein Iot device senden
    /// </summary>
    /// <param name="gatewayId">Db Id des gateway</param>
    /// <param name="message">Nachricht</param>
    /// <returns></returns>
    Task<bool> SendDownlinkMessage(long gatewayId, ExDownlinkMessageForDevice message);
}