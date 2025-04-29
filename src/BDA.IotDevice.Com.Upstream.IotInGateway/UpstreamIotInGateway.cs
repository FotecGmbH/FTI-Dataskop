// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.IotDevice.Com.Upstream.Base;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.IotDevice.Com.Upstream.IotInGateway;

/// <summary>
///     Neue Messwerte an Gateway weiter geben
///     Klasse NewValuesArgs. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
///     Ereignis Argumente für NewValuesArgs
/// </summary>
public class NewValuesArgs : EventArgs
{
    #region Properties

    /// <summary>
    ///     Neue Werte
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public List<ExValue> Values { get; set; } = null!;
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion
}

/// <summary>
///     <para>Übertragung an das Gateway wenn das Iot Device direkt im Gateway läuft</para>
///     Klasse UpstreamIotInGateway. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class UpstreamIotInGateway : UpstreamBase
{
    /// <summary>
    ///     Basis für Datentransfer vom Gateway zum Server
    /// </summary>
    public UpstreamIotInGateway() : base(EnumIotDeviceUpstreamTypes.InGateway)
    {
        IsConnectedToGateway = true;
    }

    /// <summary>
    ///     Daten an den Gateway übertragen
    /// </summary>
    /// <param name="data">Daten</param>
    /// <returns>true - übertragen an GW, false - nicht möglich</returns>
    public override Task<bool> TransferData(List<ExValue> data)
    {
        if (IsConnectedToGateway && data != null! && data.Count > 0)
        {
            Logging.Log.LogTrace($"[{nameof(UpstreamIotInGateway)}]({nameof(TransferData)}): transferring data count: {data.Count}");
            OnNewValues(new NewValuesArgs {Values = data});
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    ///     Ereignis für neue Messwerte
    /// </summary>
    //public static event EventHandler<NewValuesArgs>? NewValues;
    public event EventHandler<NewValuesArgs>? NewValues;

    /// <summary>
    ///     Methode von Ereignis für neue Messwerte
    /// </summary>
    /// <param name="eventData"></param>
    protected virtual void OnNewValues(NewValuesArgs eventData)
    {
        Logging.Log.LogTrace($"[{nameof(UpstreamIotInGateway)}]({nameof(OnNewValues)}): received values");
        var handler = NewValues;
        handler?.Invoke(this, eventData);
    }
}