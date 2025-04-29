// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Interfaces;
using Database.Common;

namespace Database.Tables;

/// <summary>
///     <para>IoT Gerät</para>
///     Klasse TableIoTDevice. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("IotDevice")]
public class TableIotDevice : IAdditionalConfiguration
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Informationen (Name, Beschreibung, ...)
    /// </summary>
    public DbInformation Information { get; set; } = new();

    /// <summary>
    ///     Fallback Standort wenn Iot Device kein GPS besitzt
    /// </summary>
    public DbPosition FallbackPosition { get; set; } = new();

    /// <summary>
    ///     Attribute des IotDevice
    /// </summary>
    public DbDeviceCommon DeviceCommon { get; set; } = new();

    /// <summary>
    ///     Wie leitet das Iot Gerät Daten an ein Gateway weiter
    /// </summary>
    public EnumIotDeviceUpstreamTypes Upstream { get; set; }

    /// <summary>
    ///     Plattform des Iot Geräts
    /// </summary>
    public EnumIotDevicePlattforms Plattform { get; set; }

    /// <summary>
    ///     Zeitpunkt der Übertragung
    /// </summary>
    public EnumTransmission TransmissionType { get; set; }

    /// <summary>
    ///     Übertragungsinterval in Sekunden (Entweder alle X Mal oder X Sekunden)
    ///     Kleiner, gleich 0 => Sofort
    /// </summary>
    public int TransmissionInterval { get; set; }

    /// <summary>
    ///     Messinterval in Zehntel Sekunden (1=100ms)
    ///     Für Iot-Embedded "C" Geräte wird nur diese Zeit verwendet
    ///     Das Messintervall bei TableMeasurementDefinition spielt dann keine Rolle
    /// </summary>
    public int MeasurementInterval { get; set; }

    /// <summary>
    ///     Zusätzliche dynamische Konfiguration (JSON)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalConfiguration { get; set; } = string.Empty;

    /// <summary>
    ///     Zusätzliche generelle Daten (zB. aus Fremdsystemen)
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string AdditionalProperties { get; set; } = string.Empty;

    /// <summary>
    ///     Wenn der Iot Sensor über ein Drittsystem abgefragt wird (zB TTN) - Erfolgsmeldung
    /// </summary>
    public bool SuccessfullyRegisteredInThirdPartySystem { get; set; }

    /// <summary>
    ///     Firma die der Berechtigung zugewiesen ist
    /// </summary>
    public long? TblGatewayId { get; set; }

    /// <summary>
    ///     Gateway für die Datenübermittlung
    /// </summary>
    [ForeignKey(nameof(TblGatewayId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableGateway? TblGateway { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    /// <summary>
    ///     Id Des Dataconverters
    /// </summary>
    public long? TblDataconverterId { get; set; }

    /// <summary>
    ///     Gateway für die Datenübermittlung
    /// </summary>
    [ForeignKey(nameof(TblDataconverterId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableDataconverter? TblDataconverter { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Konfigurations ID
    /// </summary>
    public long? TblCompanyGlobalConfigId { get; set; }

    /// <summary>
    ///     Zurodnung zu einem Projekt
    /// </summary>
    [ForeignKey(nameof(TblCompanyGlobalConfigId))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public virtual TableCompanyGlobalConfig? TblCompanyGlobalConfigs { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    ///     Liste der Messdefinitionen
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableMeasurementDefinition> TblMeasurementDefinitions { get; set; } = new List<TableMeasurementDefinition>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion
}