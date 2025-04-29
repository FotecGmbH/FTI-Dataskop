// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Interfaces;

namespace BDA.Gateway.Core;

/// <summary>
///     <para>Infos welche der Gateway gerne hätte beim ersten Start vom Ui/Cli hätte</para>
///     Klasse GatewayCoreFirstLaunchInfos. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GatewayCoreFirstLaunchInfos : IBissModel
{
    #region Properties

    /// <summary>
    ///     Secret des Gateway
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Beschreibung
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Position des Gateway
    /// </summary>
    public ExPosition? Position { get; set; }

    #endregion

    #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

    #endregion
}