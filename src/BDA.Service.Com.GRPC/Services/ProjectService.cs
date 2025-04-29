// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Service.Com.Base.Extensions;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.GRPC.Extensions;
using BDA.Service.Com.GRPC.Helpers;
using BDA.Service.Com.GRPC.Protos;
using Database;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.GRPC.Services
{
    /// <summary>
    ///     <para>This class contains the implementation of the in projects.proto defined services/endpoints</para>
    ///     Klasse ProjectService. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ProjectService : Project.ProjectBase
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<CompanyService> _logger;

        public ProjectService(ILogger<CompanyService> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        ///     Gets the latest results of each measurement definition.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoMeasurementResultQueryReply> GetLatestMeasurementResultsOfProject(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementResultQueryReply();

            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(ProjectService)}] [{nameof(GetMeasurementDefinitionsOfProject)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }


            List<ProtoMeasurementResultModel> lstResult = new();
            var mdIds = await _db.TblMeasurementDefinitionToProjectAssignments.Where(a => a.TblProjctId == request.ID)
                .Select(a => a.TblMeasurementDefinitionId)
                .ToListAsync().ConfigureAwait(true);

            foreach (var mdId in mdIds)
            {
                var res = await _db.TblMeasurementResults
                    .Where(a => a.TblMeasurementDefinitionId == mdId)
                    .OrderByDescending(a => a.TimeStamp)
                    .Select(aa => aa.ToProtoMeasurementResult())
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (res != null)
                {
                    lstResult.Add(res);
                }
            }

            result.Count = lstResult.Count;
            result.Result = CommonHelper.CreateResult("Sucecss", true);
            result.MeasurementResults.AddRange(lstResult);

            return result;
        }

        /// <summary>
        ///     Gets the measurement definitions of a project
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoMeasurementDefinitionsReply> GetMeasurementDefinitionsOfProject(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementDefinitionsReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(ProjectService)}] [{nameof(GetMeasurementDefinitionsOfProject)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var mdIds = _db.TblMeasurementDefinitionToProjectAssignments.Where(a => a.TblProjctId == request.ID)
                .Select(a => a.TblMeasurementDefinitionId);

            var measurementDefinitions = _db.TblMeasurementDefinitions.Where(a => mdIds.Contains(a.Id))
                .Select(a => a.ToProtoMeasurementDefinition());
            result.MeasurementDefinitions.AddRange(measurementDefinitions);
            result.Result = CommonHelper.CreateResult("Success", true);
            return result;
        }

        /// <summary>
        ///     Get project by ID
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoProjectReply> GetProjectByID(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoProjectReply();

            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(ProjectService)}] [{nameof(GetProjectByID)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var project = await _db.TblProjects.FirstOrDefaultAsync(a => a.Id == request.ID).ConfigureAwait(false);
            if (project == null)
            {
                result.Result = CommonHelper.CreateResult($"Project not found in the database with ID: {request.ID}", false);
                return result;
            }

            result.Project = project.ToProtoProject();
            result.Result = CommonHelper.CreateResult("Success", true);
            return result;
        }

        /// <summary>
        ///     Gets the all the projects of a company and filters with the additional properties.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoProjectsReply> GetProjectsByCompanyID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            var result = new ProtoProjectsReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (user.HaveNoRightsInCompany(request.ID))
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var lstResults = await _db.TblProjects.Where(a => a.TblCompanyId == request.ID && a.AdditionalProperties.Contains(request.AdditionalProperties)).Select(p => p.ToProtoProject()).ToListAsync().ConfigureAwait(false);
            result.Projects.AddRange(lstResults);
            result.Result = CommonHelper.CreateResult("Success", true);
            return result;
        }

        /// <summary>
        ///     Update project with an additional property
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoResult> UpdateProjectByID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            // ReSharper disable once RedundantAssignment
            var result = new ProtoResult();

            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);


            if (request.ID <= 0 || request.AdditionalProperties == null)
            {
                result = CommonHelper.CreateResult($"[{nameof(ProjectService)}] [{nameof(UpdateProjectByID)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            if (!hasPermission)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var md = await _db.TblProjects.FirstOrDefaultAsync(a => a.Id == request.ID).ConfigureAwait(true);
            if (md != null)
            {
                md.AdditionalProperties = request.AdditionalProperties;
                await _db.SaveChangesAsync().ConfigureAwait(false);
                result = CommonHelper.CreateResult("Success", true);
            }
            else
            {
                result = CommonHelper.CreateResult($"Project not found in the database with ID: {request.ID}", false);
                return result;
            }

            return result;
        }
    }
}