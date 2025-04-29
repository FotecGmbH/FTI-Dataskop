// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using BDA.Common.Exchange.Model.ConfigApp;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base;

/// <summary>
///     <para>Projekt</para>
///     Klasse ExRestProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class ExRestProject
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Informationen (Name, Beschreibung, ...)
    /// </summary>
    public ExInformation Information { get; set; } = new();

    /// <summary>
    ///     Veröffentlicht/Aktiv
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    ///     Veröffentlicht am
    /// </summary>
    public DateTime PublishedDate { get; set; }

    /// <summary>
    ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
    /// </summary>
    public string AdditionalProperties { get; set; } = string.Empty;

    /// <summary>
    ///     Firma der das Projekt zugewiesen ist
    /// </summary>
    public long CompanyId { get; set; }

    /// <summary>
    ///     Öffentlich, kann von jedem gesehen werden
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    ///     Liste von Messwertdefinitionen für die TreeView.
    /// </summary>
    public List<ExRestMeasurmentDefinition> MeasurementDefinitions { get; set; } = new List<ExRestMeasurmentDefinition>();

    #endregion
}