// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Enums;
using BDA.Service.Com.Rest.Helpers;
using BDA.Service.Com.Rest.Mapper;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange.Interfaces;

namespace BDA.Service.Com.Rest.Controllers;

/// <summary>
///     <para>Abfragen für Messwerte</para>
///     Klasse MeasurementResultController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class MeasurementResultController : Controller
{
    private static ILogger<MeasurementResultController>? _staticLogger;
    private static CancellationToken _cancellationToken;

    /// <summary>
    ///     Behaelter fuer MeasurementResult, GatewayId, MeasurementId
    /// </summary>
    public static readonly ConcurrentBag<(TableMeasurementResult, long?)> WaitingValues = new ConcurrentBag<(TableMeasurementResult, long?)>();

    /// <summary>
    ///     worker fuer MeasurementResults POST create
    /// </summary>
    private static Task? _worker;

    /// <summary>
    ///     Datenbank Context
    /// </summary>
    private readonly Db _db;

    private readonly ILogger<MeasurementResultController> _logger;

    private readonly ITriggerAgent _triggerAgent;

    /// <summary>
    ///     Konstruktor
    /// </summary>
    /// <param name="db">Datenbank</param>
    /// <param name="triggerAgent">Trigger Agent</param>
    /// <param name="logger"></param>
    public MeasurementResultController(Db db, ITriggerAgent triggerAgent, ILogger<MeasurementResultController> logger)
    {
        _db = db;
        _triggerAgent = triggerAgent ?? throw new ArgumentNullException(nameof(triggerAgent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Statische Initialisierungsmethode um den Cancelationtoken zu erhalten
    /// </summary>
    /// <param name="ct"></param>
    /// <param name="logger"></param>
    public static void Init(CancellationToken ct, ILogger<MeasurementResultController> logger)
    {
        _staticLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cancellationToken = ct;
    }

    /// <summary>
    ///     Messergebnis ändern (Zusätzliche Eigenschaften)
    /// </summary>
    /// <param name="id">ID des Messresultats</param>
    /// <param name="filterBody">Zusätzliche Eigenschaften</param>
    /// <returns>Erfolgreich</returns>
    [HttpPost("/api/measurementresult/update/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementResultUpdate(long id, [FromBody] ExRestFilterBody? filterBody)
    {
        if (id <= 0 || filterBody == null)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasMeasurmentResultPermission((ExUser) HttpContext.Items["User"]!, _db, id, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var md = await _db.TblMeasurementResults.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);
        if (md != null)
        {
            md.AdditionalProperties = filterBody.AdditionalProperties;
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
        else
        {
            return Ok(false);
        }

        return Ok(true);
    }

    /// <summary>
    ///     Details zu einem Messergebnis
    /// </summary>
    /// <param name="id">ID</param>
    /// <returns>Messergebnis</returns>
    [HttpGet("/api/measurementresult/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementResultGet(long id)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasMeasurmentResultPermission((ExUser) HttpContext.Items["User"]!, _db, id).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var mr = await _db.TblMeasurementResults.AsNoTracking().Select(aa => new ExRestMeasurementResult
        {
            Location = new ExPosition
            {
                Altitude = aa.Location.Altitude,
                Latitude = aa.Location.Latitude,
                Longitude = aa.Location.Longitude,
                TimeStamp = aa.Location.TimeStamp,
                Presision = aa.Location.Precision,
                Source = aa.Location.Source
            },
            AdditionalProperties = aa.AdditionalProperties,
            Value = CommonMethodsHelper.GetValueOfMeasurementResult(aa),
            ValueType = aa.ValueType,
            TimeStamp = aa.TimeStamp,
            Id = aa.Id
        }).FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);

        return Ok(mr);
    }

    /// <summary>
    ///     Abfrage für Messungen
    /// </summary>
    /// <param name="id">ID der Messdefinition</param>
    /// <param name="take">Anzahl der Daten die abgefragt werden</param>
    /// <param name="skip">Anzahl der Daten die übersprungen werden</param>
    /// <param name="valueType">Datentyp</param>
    /// <param name="filterAdditionalProperty">Filter für zusätzliche Eigenschaften (contains)</param>
    /// <returns>Liste an Messergebnissen</returns>
    [HttpGet("/api/measurementresult/query/{id}/{take}/{skip}")]
    //[EnableQuery]
    [BDAAuthorize]
    public virtual IActionResult MeasurementResultQuery(long id = -1, int take = 20, int skip = 0, [FromQuery] EnumQueryValueTypes valueType = EnumQueryValueTypes.All, [FromQuery] string filterAdditionalProperty = "")
    {
        if (take > 5000)
        {
            return BadRequest("Take darf nicht > 5000 sein");
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (id == -1 && !user.IsAdmin)
        {
            return UserAccessControl.Unauthorized();
        }

        var result = new ExRestMeasurementQueryResult();

        //QueryAble -> Hat den Vorteile mehrere Filter zu setzen, vermeidet verschachtelungen

        var queryAble = _db.TblMeasurementResults.AsQueryable().AsNoTracking();

        queryAble = queryAble.Where(a => a.TblMeasurementDefinitionId == id || id == -1);

        result.Count = _db.TblMeasurementResults.Count(a => a.TblMeasurementDefinitionId == id || id == -1);


        if (valueType != EnumQueryValueTypes.All)
        {
            queryAble = queryAble.Where(a => a.ValueType == (EnumValueTypes) valueType);
        }

        if (!String.IsNullOrEmpty(filterAdditionalProperty))
        {
            queryAble = queryAble.Where(a => a.AdditionalProperties.Contains(filterAdditionalProperty));
        }

        var xx = queryAble
            .Select(aa => new ExRestMeasurementResult
            {
                Location = new ExPosition
                {
                    Altitude = aa.Location.Altitude,
                    Latitude = aa.Location.Latitude,
                    Longitude = aa.Location.Longitude,
                    TimeStamp = aa.Location.TimeStamp,
                    Presision = aa.Location.Precision,
                    Source = aa.Location.Source
                },
                AdditionalProperties = aa.AdditionalProperties,
                Value = CommonMethodsHelper.GetValueOfMeasurementResult(aa),
                ValueType = aa.ValueType,
                TimeStamp = aa.TimeStamp,
                Id = aa.Id,
            })
            .Skip(skip)
            .Take(take);

        result.MeasurementResults = xx;


        return Ok(result);
    }

    /// <summary>
    ///     Abfrage für Messungen
    /// </summary>
    /// <param name="id">ID der Messdefinition</param>
    /// <param name="take">Anzahl der Daten die abgefragt werden</param>
    /// <param name="skip">Anzahl der Daten die übersprungen werden</param>
    /// <param name="valueType">Datentyp</param>
    /// <param name="filterAdditionalProperty">Filter für zusätzliche Eigenschaften (contains)</param>
    /// <param name="orderby">Sortierung timestamp/id desc/asc</param>
    /// <param name="startTime">ab welchem Zeitpunkt soll gesucht werden Format: 2023-06-05T12:00:51.831</param>
    /// <param name="endTime">bis zu welchem Zeitpunkt soll gesucht werden Format: 2023-06-05T12:00:51.831</param>
    /// <returns>Liste an Messergebnissen</returns>
    [HttpGet("/api/measurementresult/query/advanced/{id}/{take}/{skip}")]
    //[EnableQuery]
    [BDAAuthorize]
    public virtual IActionResult MeasurementResultQueryAdvanced(long id = -1, int take = 20, int skip = 0, [FromQuery] EnumQueryValueTypes valueType = EnumQueryValueTypes.All, [FromQuery] string filterAdditionalProperty = "", [FromQuery] string orderby = "timestamp desc", [FromQuery] DateTime? startTime = null!, [FromQuery] DateTime? endTime = null!)
    {
        if (take > 5000)
        {
            return BadRequest("Take darf nicht > 5000 sein");
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (id == -1 && !user.IsAdmin)
        {
            return UserAccessControl.Unauthorized();
        }

        var result = new ExRestMeasurementQueryResult();

        //QueryAble -> Hat den Vorteile mehrere Filter zu setzen, vermeidet verschachtelungen

        var queryAble = _db.TblMeasurementResults.AsQueryable().AsNoTracking();

        queryAble = queryAble.Where(a => a.TblMeasurementDefinitionId == id || id == -1);

        result.Count = _db.TblMeasurementResults.Count(a => a.TblMeasurementDefinitionId == id || id == -1);

        queryAble = queryAble.ApplyOrderbyOnMeasurementResults(orderby);

        if (startTime != null && endTime != null && startTime < endTime)
        {
            queryAble = queryAble.Where(q => q.TimeStamp < endTime && q.TimeStamp > startTime);
        }

        if (valueType != EnumQueryValueTypes.All)
        {
            queryAble = queryAble.Where(a => a.ValueType == (EnumValueTypes) valueType);
        }

        if (!String.IsNullOrEmpty(filterAdditionalProperty))
        {
            queryAble = queryAble.Where(a => a.AdditionalProperties.Contains(filterAdditionalProperty));
        }

        var xx = queryAble
            .Select(aa => aa.ToExRestMeasurementResult())
            .Skip(skip)
            .Take(take);

        result.MeasurementResults = xx;


        return Ok(result);
    }


    /// <summary>
    ///     Messergebnis hinzufügen
    /// </summary>
    /// <param name="definitionId">ID Messdefinition</param>
    /// <param name="measurementResult">Messresultat</param>
    /// <returns></returns>
    [HttpPost("/api/measurementresult/create/{definitionId}")]
    public virtual async Task<IActionResult> MeasurementResultCreate(long definitionId, ExRestMeasurementResult measurementResult)
    {
        if (definitionId <= 0)
        {
            return BadRequest();
        }

        if (!await UserAccessControl.HasMeasurmentDefinitionPermission((ExUser) HttpContext.Items["User"]!, _db, definitionId, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var md = await _db.TblMeasurementDefinitions.AsNoTracking().Include(def => def.TblIoTDevice).FirstOrDefaultAsync(a => a.Id == definitionId).ConfigureAwait(true);
        if (md == null)
        {
            return NotFound($"Could not find the MeasurementDefinition of the {nameof(definitionId)}");
        }

        try
        {
            if (measurementResult.Location.Altitude == 0 &&
                measurementResult.Location.Latitude == 0 &&
                measurementResult.Location.Longitude == 0)
            {
                var iotDev = await _db.TblIotDevices.FindAsync(md.TblIotDeviceId).ConfigureAwait(true);

                if (iotDev == null)
                {
                    return NotFound("Could not find the IoTDevice of the MeasurementDefinition");
                }

                measurementResult.Location = iotDev.FallbackPosition.ToExPosition();
            }

            WaitingValues.Add((measurementResult.GetTableModel(md.Id), md.TblIoTDevice.TblGatewayId)); // MeasurementResult, GatewayId fuer TriggerAgent

            StartBackgroundWorker(_triggerAgent);

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Creating Measurementresult");
            return BadRequest();
        }
    }

    /// <summary>
    ///     Messergebnis hinzufügen
    /// </summary>
    /// <param name="definitionId">ID Messdefinition</param>
    /// <param name="measurementResult">Messresultat</param>
    /// <returns></returns>
    [HttpPost("/api/measurementresult/create/simple/{definitionId}")]
    public virtual async Task<IActionResult> MeasurementResultCreateSimple(long definitionId, ExRestMeasurementResult measurementResult)
    {
        if (definitionId <= 0)
        {
            return BadRequest();
        }

        if (!await UserAccessControl.HasMeasurmentDefinitionPermission((ExUser) HttpContext.Items["User"]!, _db, definitionId, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        //var md = await _db.TblMeasurementDefinitions.AsNoTracking().FirstOrDefaultAsync(a => a.Id == definitionId).ConfigureAwait(true);

        var md = await _db.TblMeasurementDefinitions.FindAsync(definitionId).ConfigureAwait(true);
        if (md == null)
        {
            return NotFound($"Could not find the MeasurementDefinition of the {nameof(definitionId)}");
        }

        try
        {
            if (measurementResult.Location.Altitude == 0 &&
                measurementResult.Location.Latitude == 0 &&
                measurementResult.Location.Longitude == 0)
            {
                var iotDev = await _db.TblIotDevices.FindAsync(md.TblIotDeviceId).ConfigureAwait(true);

                if (iotDev == null)
                {
                    return NotFound("Could not find the IoTDevice of the MeasurementDefinition");
                }

                measurementResult.Location = iotDev.FallbackPosition.ToExPosition();
            }

            _db.TblMeasurementResults.Add(measurementResult.GetTableModel(md.Id));
            await _db.SaveChangesAsync().ConfigureAwait(true);

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Creating Measurementresult");
            return BadRequest();
        }
    }

    /// <summary>
    ///     Messergebnisse löschen bis bestimmten Datum
    /// </summary>
    /// <param name="definitionId">ID Measurement Definition</param>
    /// <param name="dateTime">Zeitpunkt</param>
    /// <returns></returns>
    [HttpDelete("/api/measurementresult/delete/{definitionId}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementResultDelete(long definitionId, string dateTime)
    {
        // ReSharper disable once RedundantAssignment
        var date = DateTime.MinValue;
        var cultureInfo = new CultureInfo("de-DE");
        date = DateTime.Parse(dateTime, cultureInfo);
        if (definitionId <= 0)
        {
            return BadRequest();
        }

        if (!await UserAccessControl.HasMeasurmentDefinitionPermission((ExUser) HttpContext.Items["User"]!, _db, definitionId, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var md = await _db.TblMeasurementDefinitions.Include(def => def.TblIoTDevice).FirstOrDefaultAsync(a => a.Id == definitionId).ConfigureAwait(true);
        if (md == null)
        {
            return BadRequest("Could not find MeasurementDefinition");
        }


        // ReSharper disable once UnusedVariable
        var t = Task.Run(async () =>
        {
            await using var db = new Db();

            // ReSharper disable once NotAccessedVariable
            var counter = 0;
            foreach (var result in db.TblMeasurementResults.Where(a => a.TblMeasurementDefinitionId == definitionId))
            {
                if (result.TimeStamp < date)
                {
                    db.TblMeasurementResults.Remove(result);
                }

                counter++;
            }


            await db.SaveChangesAsync().ConfigureAwait(false);
        });


        return Ok();
    }

    /// <summary>
    ///     Startet backgroundworker, fuer MeasurementResult POST create
    /// </summary>
    /// <param name="triggerAgent"></param>
    private static void StartBackgroundWorker(ITriggerAgent triggerAgent)
    {
        if (_worker == null || _worker.Status != TaskStatus.Running && _worker.Status != TaskStatus.WaitingForActivation)
        {
            _worker = Worker(triggerAgent);
        }
    }

    /// <summary>
    ///     Backgroundworker, fuer MeasurementResult POST create
    /// </summary>
    /// <param name="triggerAgent">Trigger Agent</param>
    /// <returns>Task</returns>
    private static async Task Worker(ITriggerAgent triggerAgent)
    {
        _staticLogger?.LogInfo($"[{nameof(MeasurementResultController)}]({nameof(Worker)}): BackgroundWorker started");
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, _cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
            }

            try
            {
                if (!WaitingValues.IsEmpty)
                {
                    await using var currDb = new Db();
                    var addedElements = new List<(TableMeasurementResult, long?)>();
                    var counter = 0;

                    while (WaitingValues.TryTake(out var value))
                    {
                        currDb.TblMeasurementResults.Add(value.Item1);
                        addedElements.Add(value);
                        counter++;
                    }

                    await currDb.SaveChangesAsync().ConfigureAwait(true);
                    _staticLogger?.LogInfo($"[{nameof(MeasurementResultController)}]({nameof(Worker)}): BackgroundWorker wrote to db Values Count: {counter}");

                    var gatewayGroups = addedElements.GroupBy(m => m.Item2); // group by the gateway Ids

                    foreach (var gatewayGroup in gatewayGroups)
                    {
                        if (gatewayGroup.Key != null)
                        {
                            var mdList = gatewayGroup.Select(g =>
                            {
                                var val = new ExMeasurement
                                {
                                    Id = g.Item1.Id,
                                    TimeStamp = g.Item1.TimeStamp,
                                    Value = CommonMethodsHelper.GetValueOfMeasurementResult(g.Item1),
                                    Location = new ExPosition
                                    {
                                        Altitude = g.Item1.Location.Altitude,
                                        Latitude = g.Item1.Location.Latitude,
                                        Longitude = g.Item1.Location.Longitude,
                                        Presision = g.Item1.Location.Precision,
                                        Source = g.Item1.Location.Source,
                                        TimeStamp = g.Item1.Location.TimeStamp
                                    }
                                };
                                return (g.Item1.TblMeasurementDefinitionId, val);
                            });

                            await triggerAgent.NewMeasurementsFromGateway(gatewayGroup.Key.Value, mdList.ToList()).ConfigureAwait(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _staticLogger?.LogError(e, $"[{nameof(MeasurementResultController)}]({nameof(Worker)}): Error in Backgroundworker");
            }
        }

        _staticLogger?.LogInfo($"[{nameof(MeasurementResultController)}]({nameof(Worker)}): BackgroundWorker stopped");
    }


    //[HttpGet("/api/testmqtt/{id}/{value}")]
    //public virtual async Task<IActionResult> TestMqtt(int id, string value)
    //{
    //    await _triggerAgent.NewMeasurementsFromGateway(1, new List<(long, ExMeasurement)>()
    //                                                {
    //                                                    (id, new ExMeasurement()
    //                                                        {
    //                                                            Id = 1,
    //                                                            Value = value,
    //                                                            SourceInfo = "TestSource",
    //                                                            TimeStamp = DateTime.UtcNow,
    //                                                            Location = new ExPosition()
    //                                                                       {
    //                                                                           Altitude = 0,
    //                                                                           Latitude = 48,
    //                                                                           Longitude = 16,
    //                                                                           Presision = 0,
    //                                                                           Source = EnumPositionSource.Internet,
    //                                                                           TimeStamp = DateTime.UtcNow
    //                                                                       }
    //                                                        })

    //                                                }).ConfigureAwait(false);

    //    return Ok();

    //}
}