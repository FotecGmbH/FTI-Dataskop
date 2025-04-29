// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.GatewayService;

namespace BDA.Gateway.Com.Base.Model;

/// <summary>
///     Neue Config vom BDA Service
///     Klasse NewConfigEventArgs. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
///     Ereignis Argumente für NewConfigEventArgs
/// </summary>
public class NewConfigEventArgs : EventArgs
{
    #region Properties

    /// <summary>
    ///     Config vom Server
    /// </summary>
    public ExGwServiceGatewayConfig GatewayConfig { get; set; } = null!;

    #endregion
}