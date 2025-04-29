// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Service.Com.Base.Extensions;
using BDA.Service.Com.GRPC.Extensions;
using BDA.Service.Com.GRPC.Helpers;
using BDA.Service.Com.GRPC.Protos;
using Database;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BDA.Service.Com.GRPC.Services
{
    /// <summary>
    ///     <para>This class is responsible for the gRPC geo service.</para>
    ///     Klasse GeoService. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GeoService : Geo.GeoBase
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<CompanyService> _logger;

        public GeoService(ILogger<CompanyService> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }


        public override async Task<ProtoGeoReply> GetResultsInArea(ProtoGeoRequest request, ServerCallContext context)
        {
            var result = new ProtoGeoReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (!user.IsAdmin)
            {
                //Projektübergreifend darf nur Admin
                if (request.ProjectID == -1)
                {
                    result.Result = CommonHelper.CreateResult("Unauthorized", false);
                    return result;
                }

                var tblProject = await _db.TblProjects.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.ProjectID).ConfigureAwait(true);
                if (tblProject == null)
                {
                    result.Result = CommonHelper.CreateResult("Unauthorized", false);
                    return result;
                }

                var companyId = tblProject.TblCompanyId;

                if (user.HaveNoRightsInCompany(companyId))
                {
                    result.Result = CommonHelper.CreateResult("Unauthorized", false);
                    return result;
                }
            }


            //SRID = 4326
            Point sourceSpatialPoint = new(request.Longitude, request.Latitude) {SRID = 4326};

            //Queryalbe -> Hat den Vorteil, dass man nicht alles verschachteln muss
            var queryAbleMeasurments = _db.TblMeasurementResults.AsQueryable();

            //Einschränkung auf Wertetyp
            if (!request.IsAllTypes)
            {
                queryAbleMeasurments = queryAbleMeasurments.Where(a => a.ValueType == CommonHelper.ConvertProtoValueTypeToValueType(request.ValueType));
            }

            //Filter auf zusätzliche Properties
            if (!String.IsNullOrEmpty(request.AdditionalPropertyFilter))
            {
                queryAbleMeasurments = queryAbleMeasurments.Where(a => a.AdditionalProperties.Contains(request.AdditionalPropertyFilter));
            }

            //Filter auf Projekt
            if (request.ProjectID != -1)
            {
                var tblMeasurmentDefinitionIds = _db.TblMeasurementDefinitionToProjectAssignments
                    .Where(aaa => aaa.TblProjctId == request.ProjectID)
                    .Select(aa => aa.TblMeasurementDefinitionId);

                queryAbleMeasurments = queryAbleMeasurments.Where(a => tblMeasurmentDefinitionIds.Contains(a.TblMeasurementDefinitionId));
            }

            //Filter Zeitstempel
            if (request.TimeStampFrom != null)
            {
                queryAbleMeasurments = queryAbleMeasurments.Where(a => a.TimeStamp >= request.TimeStampFrom.ToDateTime().Date);
            }

            if (request.TimeStampTo != null)
            {
                queryAbleMeasurments = queryAbleMeasurments.Where(a => a.TimeStamp <= request.TimeStampTo.ToDateTime().Date);
            }


            //Filter Höhe
            if (request.AltitudeMinimum != 0 && request.AltitudeMaximum != 0 && request.AltitudeMinimum <= request.AltitudeMaximum)
            {
                queryAbleMeasurments = queryAbleMeasurments.Where(a => a.Location.Altitude <= request.AltitudeMaximum && a.Location.Altitude >= request.AltitudeMinimum);
            }

            //Filter ohne Radius
            if (request.Radius <= 0)
            {
                var lstResult = await queryAbleMeasurments
                    .AsNoTracking()
                    .Select(a => a.ToProtoGeoResult(sourceSpatialPoint))
                    .ToListAsync().ConfigureAwait(false);
                lstResult = lstResult.OrderBy(a => a.Distance).ToList();
                result.GeoResults.AddRange(lstResult);
            }
            else //Filter mit Radius
            {
                var lstResult = await queryAbleMeasurments
                    .Where(a => a.SpatialPoint.Distance(sourceSpatialPoint) <= request.Radius)
                    .AsNoTracking()
                    .Select(a => a.ToProtoGeoResult(sourceSpatialPoint))
                    .ToListAsync().ConfigureAwait(false);
                lstResult = lstResult.OrderBy(a => a.Distance).ToList();
                result.GeoResults.AddRange(lstResult);
            }

            result.Result = CommonHelper.CreateResult("Success", true);

            return result;
        }
    }
}