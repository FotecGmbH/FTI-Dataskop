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
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Enums;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
// ReSharper disable RedundantAssignment

namespace BDA.Service.Com.Rest.Controllers;

/// <summary>
///     <para>Geoabfragen</para>
///     Klasse GeoController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class GeoController : Controller
{
    /// <summary>
    ///     Datenbank Context
    /// </summary>
    private readonly Db _db;

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="db">Database</param>
    public GeoController(Db db)
    {
        _db = db;
    }

    /// <summary>
    ///     Abfrage Anhand einer Position mit Filtermöglichkeiten
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lng">Longitude</param>
    /// <param name="radius">Radius in Grad (SRID 4326)</param>
    /// <param name="altMin">Minimale Höhe</param>
    /// <param name="altMax">Maximale Höhe</param>
    /// <param name="projectId">Projekt ID</param>
    /// <param name="take">Wie viele Datenreiehen werden genommen</param>
    /// <param name="skip">Wie viele Datenreihen werden übersprungen</param>
    /// <param name="valueType">Wertetyp</param>
    /// <param name="filterAdditionalProperties">Filter für zusätzliche Properties (contains)</param>
    /// <param name="timeStampFrom">Zeiteinschränkung Von</param>
    /// <param name="timeStampTo">Zeiteinschränkung Bis</param>
    /// <returns>Liste von Messwerten mit Distanz</returns>
    [HttpGet("/api/geo/area/{lat}/{lng}/{radius}/{altMin}/{altMax}/{projectId}/{take}/{skip}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> GeoArea(double lat = 47.83768085082139, double lng = 16.251903664907417, double radius = 500, double altMin = 0, double altMax = 0, int projectId = -1, int take = 20, int skip = 0, [FromQuery] EnumQueryValueTypes valueType = EnumQueryValueTypes.All, [FromQuery] string filterAdditionalProperties = "", [FromQuery] DateTime? timeStampFrom = null, [FromQuery] DateTime? timeStampTo = null)
    {
        if (take > 5000)
        {
            return BadRequest("Take darf nicht > 5000 sein");
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (!user.IsAdmin)
        {
            //Projektübergreifend darf nur Admin
            if (projectId == -1)
            {
                return UserAccessControl.Unauthorized();
            }

            var tblProject = await _db.TblProjects.AsNoTracking().FirstOrDefaultAsync(a => a.Id == projectId).ConfigureAwait(true);
            if (tblProject == null)
            {
                return UserAccessControl.Unauthorized();
            }

            var companyId = tblProject.TblCompanyId;

            if (!user.IsAdmInCompany(companyId) && !user.CanUserReadInCompany(companyId) && !user.CanUserWriteInCompany(companyId))
            {
                return UserAccessControl.Unauthorized();
            }
        }

        //SRID = 4326
        Point sourceSpatialPoint = new(lng, lat) {SRID = 4326};

        // ReSharper disable once RedundantAssignment
        List<ExRestGeoResult> ret = new();

        //Queryalbe -> Hat den Vorteil, dass man nicht alles verschachteln muss
        var queryAbleMeasurments = _db.TblMeasurementResults.AsQueryable().AsNoTracking();


        //Filter auf Projekt
        if (projectId != -1)
        {
            var tblMeasurmentDefinitionIds = _db.TblMeasurementDefinitionToProjectAssignments
                .Where(aaa => aaa.TblProjctId == projectId).Select(aa => aa.TblMeasurementDefinitionId);

            queryAbleMeasurments = queryAbleMeasurments.Where(a => tblMeasurmentDefinitionIds.Contains(a.TblMeasurementDefinitionId));
        }

        if (radius >= 0) //Filter mit Radius          groeßte einschraenkung
        {
            // ToList() weil EntityFramework SpatialPoint.Distance andere Einheit liefert
            queryAbleMeasurments = queryAbleMeasurments.ToList().Where(a => a.SpatialPoint.Distance(sourceSpatialPoint) <= radius).AsQueryable();
        }

        //Einschränkung auf Wertetyp
        if (valueType != EnumQueryValueTypes.All)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a.ValueType == (EnumValueTypes) valueType);
        }

        //Filter auf zusätzliche Properties
        if (!String.IsNullOrEmpty(filterAdditionalProperties))
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a.AdditionalProperties.Contains(filterAdditionalProperties));
        }

        //Filter Zeitstempel
        if (timeStampFrom != null)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a.TimeStamp >= timeStampFrom.Value);
        }

        if (timeStampTo != null)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a.TimeStamp <= timeStampTo.Value);
        }

        //Filter Höhe
        if (altMin != 0 && altMax != 0 && altMin <= altMax)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a.Location.Altitude <= altMax && a.Location.Altitude >= altMin);
        }

        var lstResult = queryAbleMeasurments
            .Skip(skip).Take(take)
            .Select(a => new ExRestGeoResult(
                a.Id,
                a.TblMeasurementDefinitionId,
                a.TimeStamp,
                a.SpatialPoint.Distance(sourceSpatialPoint),
                a.Value.Binary,
                a.Value.Text,
                a.Value.Number,
                a.Value.Bit,
                a.ValueType,
                a.Location.Latitude,
                a.Location.Longitude,
                a.Location.Altitude,
                a.AdditionalProperties
            )).ToList();


        //lstResult = lstResult.OrderBy(a => a.Distance).ToList();
        ret = lstResult;

        return Ok(ret);
    }

    /// <summary>
    ///     Abfrage innerhalb eines Radius, von jeder MeasurementDefinition das letzte MeasurementResult
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lng">Longitude</param>
    /// <param name="radius">Radius in Grad (SRID 4326)</param>
    /// <param name="altMin">Minimale Höhe</param>
    /// <param name="altMax">Maximale Höhe</param>
    /// <param name="projectId">Projekt ID</param>
    /// <param name="take">Wie viele Datenreiehen werden genommen</param>
    /// <param name="skip">Wie viele Datenreihen werden übersprungen</param>
    /// <param name="valueType">Wertetyp</param>
    /// <param name="filterAdditionalProperties">Filter für zusätzliche Properties (contains)</param>
    /// <param name="timeStampFrom">Zeiteinschränkung Von</param>
    /// <param name="timeStampTo">Zeiteinschränkung Bis</param>
    /// <returns>Liste von Messwerten mit Distanz</returns>
    [HttpGet("/api/geo/arealatest/{lat}/{lng}/{radius}/{altMin}/{altMax}/{projectId}/{take}/{skip}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> GeoAreaLatest(double lat = 47.83768085082139, double lng = 16.251903664907417, double radius = 500, double altMin = 0, double altMax = 0, int projectId = -1, int take = 20, int skip = 0, [FromQuery] EnumQueryValueTypes valueType = EnumQueryValueTypes.All, [FromQuery] string filterAdditionalProperties = "", [FromQuery] DateTime? timeStampFrom = null, [FromQuery] DateTime? timeStampTo = null)
    {
        if (take > 5000)
        {
            return BadRequest("Take darf nicht > 5000 sein");
        }

        var user = (ExUser) HttpContext.Items["User"]!;

        if (!user.IsAdmin)
        {
            //Projektübergreifend darf nur Admin
            if (projectId == -1)
            {
                return UserAccessControl.Unauthorized();
            }

            var tblProject = await _db.TblProjects.AsNoTracking().FirstOrDefaultAsync(a => a.Id == projectId).ConfigureAwait(true);
            if (tblProject == null)
            {
                return UserAccessControl.Unauthorized();
            }

            var companyId = tblProject.TblCompanyId;

            if (!user.IsAdmInCompany(companyId) && !user.CanUserReadInCompany(companyId) && !user.CanUserWriteInCompany(companyId))
            {
                return UserAccessControl.Unauthorized();
            }
        }

        //SRID = 4326
        Point sourceSpatialPoint = new(lng, lat) {SRID = 4326};

        List<ExRestGeoResult> ret = new();

        //Queryalbe -> Hat den Vorteil, dass man nicht alles verschachteln muss

        var mrIds = new List<long?>();

        //Filter auf Projekt
        if (projectId != -1)
        {
            var mds = _db.TblMeasurementDefinitionToProjectAssignments
                .Where(aaa => aaa.TblProjctId == projectId)
                .Select(aa => aa.TblMeasurmentDefinitions).ToList();
            mds.RemoveAll(id => id == null!);
            mrIds = mds.Select(md => md!.TblLatestMeasurementResultId).ToList();
        }
        else
        {
            mrIds = _db.TblMeasurementDefinitions.Select(m => m.TblLatestMeasurementResultId).ToList();
        }

        mrIds.RemoveAll(id => id == null!); // falls id ist null

        var queryAbleMeasurments = mrIds.Select(m => _db.TblMeasurementResults.Find(m)).Where(mr => mr != null).AsQueryable();


        if (radius >= 0) //Filter mit Radius          groeßte einschraenkung
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.SpatialPoint.Distance(sourceSpatialPoint) <= radius);
        }

        queryAbleMeasurments = queryAbleMeasurments.GroupBy(q => q!.TblMeasurementDefinitionId).SelectMany(q => q.OrderByDescending(t => t!.TimeStamp).Take(1));

        //Einschränkung auf Wertetyp
        if (valueType != EnumQueryValueTypes.All)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.ValueType == (EnumValueTypes) valueType);
        }

        //Filter auf zusätzliche Properties
        if (!String.IsNullOrEmpty(filterAdditionalProperties))
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.AdditionalProperties.Contains(filterAdditionalProperties));
        }

        //Filter Zeitstempel
        if (timeStampFrom != null)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.TimeStamp >= timeStampFrom.Value);
        }

        if (timeStampTo != null)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.TimeStamp <= timeStampTo.Value);
        }

        //Filter Höhe
        if (altMin != 0 && altMax != 0 && altMin <= altMax)
        {
            queryAbleMeasurments = queryAbleMeasurments.Where(a => a!.Location.Altitude <= altMax && a.Location.Altitude >= altMin);
        }

        var lstResult = queryAbleMeasurments.Where(q => q != null)
            .Skip(skip).Take(take)
            .Select(a => new ExRestGeoResult(
                a!.Id,
                a.TblMeasurementDefinitionId,
                a.TimeStamp,
                a.SpatialPoint.Distance(sourceSpatialPoint),
                a.Value.Binary,
                a.Value.Text,
                a.Value.Number,
                a.Value.Bit,
                a.ValueType,
                a.Location.Latitude,
                a.Location.Longitude,
                a.Location.Altitude,
                a.AdditionalProperties
            ))
            .ToList();
        //lstResult = lstResult.OrderBy(a => a.Distance).ToList();
        ret = lstResult;

        return Ok(ret);
    }
}