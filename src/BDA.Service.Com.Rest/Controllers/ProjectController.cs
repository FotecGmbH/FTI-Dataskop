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
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Mapper;
using Database;
using Database.Converter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Rest.Controllers;

/// <summary>
///     Project
/// </summary>
public class ProjectController : Controller
{
    /// <summary>
    ///     Datenbank Context
    /// </summary>
    private readonly Db _db;

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="db"></param>
    public ProjectController(Db db)
    {
        _db = db;
    }

    /// <summary>
    ///     Projekt ändern (Zusätzliche Eingenschaft)
    /// </summary>
    /// <param name="id">ID des Projektes</param>
    /// <param name="filterBody">Zusätzliche Eigenschaften</param>
    /// <returns>Erolgreich</returns>
    [HttpPost("/api/project/update/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> ProjectUpdate(long id, [FromBody] ExRestFilterBody? filterBody)
    {
        if (id <= 0 || filterBody == null)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, id, true).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var md = await _db.TblProjects.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);
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
    ///     Projekt
    /// </summary>
    /// <param name="id">ID des Projektes</param>
    /// <returns>Informationen über das Projekt</returns>
    [HttpGet("/api/project/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> ProjectGet(long id)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, id).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var project = await _db.TblProjects.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);
        if (project == null)
        {
            return Ok();
        }

        return Ok(project.ToExRestProject());
    }

    /// <summary>
    ///     Anfrage der Projekte anhand der zusätzlichen Eigenschaften (contains)
    /// </summary>
    /// <param name="companyId">Firmen ID</param>
    /// <param name="filter">Filter für Eigenschaften (Contains)</param>
    /// <returns>Liste mit Projekten</returns>
    [HttpGet("/api/project/list/prop/{companyId}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> ProjectListResult(long companyId, [FromQuery] string? filter)
    {
        if (filter == null)
        {
            filter = string.Empty;
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (!user.IsAdmInCompany(companyId) && !user.CanUserReadInCompany(companyId) && !user.CanUserWriteInCompany(companyId))
        {
            return UserAccessControl.Unauthorized();
        }

        var lstResults = await _db.TblProjects
            .Where(a => a.TblCompanyId == companyId && a.AdditionalProperties.Contains(filter))
            .ToListAsync().ConfigureAwait(false);

        return Ok(lstResults.Select(s => s.ToExRestProject()).ToList());
    }


    /// <summary>
    ///     Messdefinitionen für ein Projekt
    /// </summary>
    /// <param name="id">ID des Projektes</param>
    /// <returns>Liste an Messdefinitionen für das Projekt</returns>
    [HttpGet("/api/project/measurementdefinitions/{id}")]
    [EnableQuery]
    [BDAAuthorize]
    public async Task<IActionResult> ProjectMeasurementDefinitions(int id = -1)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, id).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var mdIds = _db.TblMeasurementDefinitionToProjectAssignments
            .Where(a => a.TblProjctId == id)
            .Select(a => a.TblMeasurementDefinitionId);

        return Ok(_db.TblMeasurementDefinitions
            .Where(a => mdIds.Contains(a.Id))
            .Select(a => a.ToExRestMeasurmentDefinition()));
    }

    /// <summary>
    ///     Informationen zu einem Projekt + Informationen zu jeder Messdefinition des Projekts + deren letztes Resultat
    /// </summary>
    /// <param name="projectid">ID des Projektes</param>
    /// <returns>Liste an Messdefinitionen für das Projekt</returns>
    [HttpGet("/api/project/measurementdefinitionslatestresults/{projectid}")]
    [EnableQuery]
    [BDAAuthorize]
    public async Task<IActionResult> ProjectMeasurementDefinitionWithlatestresults(int projectid = -1)
    {
        if (projectid <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, projectid).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        var project = await _db.TblProjects.AsNoTracking()
            .Include(p => p.TblMeasurementDefinitionToProjectAssignments)
            .ThenInclude(p => p.TblMeasurmentDefinitions)
            .ThenInclude(md => md!.Information)
            .FirstOrDefaultAsync(a => a.Id == projectid).ConfigureAwait(false);

        if (project == null)
        {
            return BadRequest("Could not find requested Project");
        }

        var result = new ExRestProjectWithLatestResults
        {
            AdditionalProperties = project.AdditionalProperties,
            CompanyId = project.TblCompanyId,
            Id = project.Id,
            Information = project.Information.ToExInformation(),
            IsPublic = project.IsPublic,
            Published = project.Published,
            PublishedDate = project.PublishedDate,
            MeasurementDefinitionsWithLatestResults = new List<ExRestMeasurementDefinitionWithLatestResult>()
        };

        foreach (var mdAssignment in project.TblMeasurementDefinitionToProjectAssignments)
        {
            var md = mdAssignment.TblMeasurmentDefinitions;
            var mdWithLatest = new ExRestMeasurementDefinitionWithLatestResult
            {
                AdditionalProperties = md!.AdditionalProperties,
                Id = md.Id,
                Information = md.Information.ToExInformation(),
                ValueType = md.ValueType,
                DownstreamType = md.DownstreamType,
                MeasurementInterval = md.MeasurementInterval,
            };
            var mdRes = await _db.TblMeasurementResults.FindAsync(md.TblLatestMeasurementResultId).ConfigureAwait(true);

            if (mdRes != null)
            {
                mdWithLatest.LatestResult = mdRes.ToExRestMeasurementResult();
            }

            result.MeasurementDefinitionsWithLatestResults.Add(mdWithLatest);
        }

        return Ok(result);
    }

    /// <summary>
    ///     Aktuellstes Messergebnisse pro Messdefinition eines Projektes (ohne referenz zu Messdefinition)
    /// </summary>
    /// <param name="id">ID des Projektes</param>
    /// <returns>Liste von Messergebnissen</returns>
    [HttpGet("/api/project/latestresults/{id}")]
    [EnableQuery]
    [BDAAuthorize]
    public async Task<IActionResult> ProjectLatestResults(int id = -1)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        if (!await UserAccessControl.HasProjectPermission((ExUser) HttpContext.Items["User"]!, _db, id).ConfigureAwait(true))
        {
            return UserAccessControl.Unauthorized();
        }

        List<ExRestMeasurementResult> lstResult = new();
        var mdIds = _db.TblMeasurementDefinitionToProjectAssignments.Where(a => a.TblProjctId == id).Select(a => a.TblMeasurementDefinitionId).ToList();

        foreach (var mdId in mdIds)
        {
            var res = await _db.TblMeasurementResults
                .Where(a => a.TblMeasurementDefinitionId == mdId)
                .OrderByDescending(a => a.TimeStamp)
                .Select(aa => aa.ToExRestMeasurementResult()).FirstOrDefaultAsync().ConfigureAwait(false);
            if (res != null)
            {
                lstResult.Add(res);
            }
        }

        return Ok(lstResult);
    }
}