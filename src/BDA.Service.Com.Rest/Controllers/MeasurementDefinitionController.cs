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
using BDA.Common.Exchange.Model;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Mapper;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Rest.Controllers;

/// <summary>
///     <para>Abfragen für Messdefinitionen</para>
///     Klasse MeasurementDefinitionController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class MeasurementDefinitionController : Controller
{
    /// <summary>
    ///     Datenbank Context
    /// </summary>
    private readonly Db _db;

    /// <summary>
    ///     Konstruktor
    /// </summary>
    /// <param name="db">DB</param>
    public MeasurementDefinitionController(Db db)
    {
        _db = db;
    }

    /// <summary>
    ///     Messdefinition aktualisieren (Zusätzliche Eigenschaften)
    /// </summary>
    /// <param name="id">ID der Messdefinition</param>
    /// <param name="filterBody">Zusätzliche Eigenschaften</param>
    /// <returns>Erfolgreich</returns>
    [HttpPost("/api/measurementdefinition/update/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementDefinitionUpdate(long id, [FromBody] ExRestFilterBody? filterBody)
    {
        if (id <= 0 || filterBody == null)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasMeasurmentDefinitionPermission((ExUser) HttpContext.Items["User"]!, _db, id, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var md = await _db.TblMeasurementDefinitions.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);
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
    ///     Informationen über die Messdefinition abfragen
    /// </summary>
    /// <param name="id">ID der Messdefinition</param>
    /// <returns>Informationen der Messdefinition</returns>
    [HttpGet("/api/measurementdefinition/{id}")]
    //[EnableQuery]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementDefinitionHttpGet(long id)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasMeasurmentDefinitionPermission((ExUser) HttpContext.Items["User"]!, _db, id).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var tmp = _db.TblMeasurementDefinitions.FirstOrDefault(a => a.Id == id);
        if (tmp == null)
        {
            return Ok();
        }

        return Ok(tmp.ToExRestMeasurmentDefinition());
        //var md = await _db.TblMeasurementDefinitions.AsNoTracking().Select(a => a.ToExRestMeasurmentDefinition()).FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);


        //return Ok(md);
    }

    /// <summary>
    ///     Anfrage der Messdefinitionen anhand des zusätzlichen Eigenschaften (contains)
    /// </summary>
    /// <param name="projectId">Projekt ID</param>
    /// <param name="filter">Filter für Eigenschaften (Contains)</param>
    /// <returns>Lieste von Messdefinitionen</returns>
    [HttpGet("/api/measurementdefinition/list/prop/{projectId}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementDefinitionListResult(long projectId, [FromQuery] string filter)
    {
        var user = (ExUser) HttpContext.Items["User"]!;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (filter == null)
        {
            filter = String.Empty;
        }

        if (projectId == -1 && !user.IsAdmin)
        {
            return UserAccessControl.Unauthorized();
        }

        if (projectId != -1 && !await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, projectId).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        if (projectId != -1)
        {
            var mdIds = _db.TblMeasurementDefinitionToProjectAssignments
                .Where(a => a.TblProjctId == projectId)
                .Select(a => a.TblMeasurementDefinitionId);
            var lstResults = await _db.TblMeasurementDefinitions
                .Where(a => mdIds.Contains(a.Id) && a.AdditionalProperties.Contains(filter))
                .Select(a => a.ToExRestMeasurmentDefinition())
                .ToListAsync().ConfigureAwait(false);

            return Ok(lstResults);
        }
        else
        {
            var lstResults = await _db.TblMeasurementDefinitions
                .Where(a => a.AdditionalProperties.Contains(filter))
                .Select(a => a.ToExRestMeasurmentDefinition())
                .ToListAsync().ConfigureAwait(false);

            return Ok(lstResults);
        }
    }

    /// <summary>
    ///     Anfrage der aktuellsten Werte
    /// </summary>
    /// <param name="projectId">Projekt ID</param>
    /// <param name="filter">Filter für Eigenschaften (Contains)</param>
    /// <returns>Liste an Messwerten</returns>
    [HttpGet("/api/measurementdefinition/latestresult/prop/{projectId}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementDefinitionLatestResult(long projectId, [FromQuery] string? filter)
    {
        if (filter == null)
        {
            filter = String.Empty;
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (projectId == -1 && !user.IsAdmin)
        {
            return UserAccessControl.Unauthorized();
        }

        if (projectId != -1 && !await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, projectId).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        if (projectId == -1)
        {
            var mrIds = _db.TblMeasurementDefinitions.AsNoTracking().Where(a => a.AdditionalProperties.Contains(filter)).Select(a => a.Id).ToList();

            var queryAble = _db.TblMeasurementResults.AsNoTracking().AsQueryable();

            List<ExRestMeasurementResult> lstResults = new();

            foreach (var mrId in mrIds)
            {
                var latestValue = await queryAble
                    .Where(a => a.TblMeasurementDefinitionId == mrId)
                    .OrderByDescending(a => a.TimeStamp)
                    .Select(aa => aa.ToExRestMeasurementResult())
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (latestValue != null)
                {
                    lstResults.Add(latestValue);
                }
            }

            return Ok(lstResults);
        }
        else
        {
            var mdIds = _db.TblMeasurementDefinitionToProjectAssignments
                .Where(a => a.TblProjctId == projectId)
                .Select(a => a.TblMeasurementDefinitionId);
            var mrIds = _db.TblMeasurementDefinitions
                .Where(a => mdIds
                    .Contains(a.Id) && a.AdditionalProperties
                    .Contains(filter)).Select(a => a.Id).ToList();

            var queryAble = _db.TblMeasurementResults.AsQueryable();

            List<ExRestMeasurementResult> lstResults = new();

            foreach (var mrId in mrIds)
            {
                var latestValue = await queryAble
                    .Where(a => a.TblMeasurementDefinitionId == mrId)
                    .OrderByDescending(a => a.TimeStamp)
                    .Select(aa => aa.ToExRestMeasurementResult())
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (latestValue != null)
                {
                    lstResults.Add(latestValue);
                }
            }

            return Ok(lstResults);
        }
    }


    /// <summary>
    ///     Anfrage der aktuellsten Werte
    /// </summary>
    /// <param name="id">ID der Messdefinition</param>
    /// <returns>Letze Messergebnisse</returns>
    [HttpGet("/api/measurementdefinition/latestresult/{id}")]
    [EnableQuery]
    [BDAAuthorize]
    public virtual async Task<IActionResult> MeasurementDefinitionLatestResult(long id = -1)
    {
        var user = (ExUser) HttpContext.Items["User"]!;

        if (id == -1 && !user.IsAdmin)
        {
            return UserAccessControl.Unauthorized();
        }

        if (id != -1 && !await UserAccessControl.HasMeasurmentDefinitionPermission(user, _db, id, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var measurementDef = await _db.TblMeasurementDefinitions.FindAsync(id).ConfigureAwait(false);

        if (measurementDef == null)
        {
            return NotFound("Measurementdefinition not found");
        }

        var latestRes = await _db.TblMeasurementResults.FindAsync(measurementDef.TblLatestMeasurementResultId).ConfigureAwait(false);

        if (latestRes == null)
        {
            return NotFound("definition has no results");
        }

        var latestValue = new ExRestMeasurementResult
        {
            Location = new ExPosition
            {
                Altitude = latestRes.Location.Altitude,
                Latitude = latestRes.Location.Latitude,
                Longitude = latestRes.Location.Longitude,
                TimeStamp = latestRes.Location.TimeStamp,
                Presision = latestRes.Location.Precision,
                Source = latestRes.Location.Source
            },
            AdditionalProperties = latestRes.AdditionalProperties,
            Value = CommonMethodsHelper.GetValueOfMeasurementResult(latestRes),
            ValueType = latestRes.ValueType,
            TimeStamp = latestRes.TimeStamp,
            Id = latestRes.Id
        };

        return Ok(latestValue);
    }
}