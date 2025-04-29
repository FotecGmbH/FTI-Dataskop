// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

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
    public class MeasurementDefinitionService : MeasurementDefinition.MeasurementDefinitionBase
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;


        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<CompanyService> _logger;

        public MeasurementDefinitionService(ILogger<CompanyService> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }


        public override async Task<ProtoMeasurementResultReply> GetLatestMeasurementResultsByID(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementResultReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasMeasurmentDefinitionPermission(user, _db, request.ID).ConfigureAwait(true);


            if (request.ID == -1 && !user.IsAdmin)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID != -1 && !hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var queryAble = _db.TblMeasurementResults.AsQueryable();


            result.MeasurementResult = await queryAble.Where(a => a.TblMeasurementDefinitionId == request.ID || request.ID == -1)
                .OrderByDescending(a => a.TimeStamp)
                .Select(aa => aa.ToProtoMeasurementResult())
                .FirstOrDefaultAsync().ConfigureAwait(false);


            result.Result = CommonHelper.CreateResult("Success", true);

            return result;
        }

        public override async Task<ProtoMeasurementResultQueryReply> GetLatestMeasurementResultsByProjectID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementResultQueryReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);


            if (request.ID == -1 && !user.IsAdmin)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID != -1 && !hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            List<ProtoMeasurementResultModel> lstResults = new();
            if (request.ID == -1)
            {
                var mrIds = _db.TblMeasurementDefinitions.Where(a => a.AdditionalProperties.Contains(request.AdditionalProperties)).Select(a => a.Id).ToList();

                var queryAble = _db.TblMeasurementResults.AsQueryable();


                foreach (var mrId in mrIds)
                {
                    var latestValue = await queryAble.Where(a => a.TblMeasurementDefinitionId == mrId)
                        .OrderByDescending(a => a.TimeStamp).Select(aa => aa.ToProtoMeasurementResult()).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (latestValue != null)
                    {
                        lstResults.Add(latestValue);
                    }
                }

                result.MeasurementResults.AddRange(lstResults);
                result.Result = CommonHelper.CreateResult("Success", true);
                result.Count = lstResults.Count;
                return result;
            }
            else
            {
                var mdIds = _db.TblMeasurementDefinitionToProjectAssignments.Where(a => a.TblProjctId == request.ID).Select(a => a.TblMeasurementDefinitionId);
                var mrIds = _db.TblMeasurementDefinitions.Where(a => mdIds.Contains(a.Id) && a.AdditionalProperties.Contains(request.AdditionalProperties)).Select(a => a.Id).ToList();

                var queryAble = _db.TblMeasurementResults.AsQueryable();


                foreach (var mrId in mrIds)
                {
                    var latestValue = await queryAble.Where(a => a.TblMeasurementDefinitionId == mrId)
                        .OrderByDescending(a => a.TimeStamp)
                        .Select(aa => aa.ToProtoMeasurementResult())
                        .FirstOrDefaultAsync().ConfigureAwait(false);
                    if (latestValue != null)
                    {
                        lstResults.Add(latestValue);
                    }
                }

                result.MeasurementResults.AddRange(lstResults);
                result.Result = CommonHelper.CreateResult("Success", true);
                result.Count = lstResults.Count;
                return result;
            }
        }

        /// <summary>
        ///     Get measurement definition by ID
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoMeasurementDefinitionReply> GetMeasurementDefinitionByID(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementDefinitionReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(MeasurementDefinitionService)}] [{nameof(GetMeasurementDefinitionByID)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasMeasurmentResultPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var mr = _db.TblMeasurementDefinitions.FirstOrDefault(mr => mr.Id == request.ID);

            if (mr != null)
            {
                result.MeasurementDefinition = mr.ToProtoMeasurementDefinition();
                result.Result = CommonHelper.CreateResult("Success", true);
            }

            return result;
        }

        public override async Task<ProtoMeasurementDefinitionsReply> GetMeasurementDefinitionsByProjectID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementDefinitionsReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasProjectPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }


            if (request.ID != -1)
            {
                var mdIds = _db.TblMeasurementDefinitionToProjectAssignments.Where(a => a.TblProjctId == request.ID)
                    .Select(a => a.TblMeasurementDefinitionId);

                var lstResults = await _db.TblMeasurementDefinitions.Where(a => mdIds.Contains(a.Id) && a.AdditionalProperties.Contains(request.AdditionalProperties))
                    .Select(a => a.ToProtoMeasurementDefinition())
                    .ToListAsync()
                    .ConfigureAwait(false);

                result.MeasurementDefinitions.AddRange(lstResults);
                result.Result = CommonHelper.CreateResult("Success", true);
            }
            else
            {
                var lstResults = await _db.TblMeasurementDefinitions.Where(a => a.AdditionalProperties.Contains(request.AdditionalProperties))
                    .Select(a => a.ToProtoMeasurementDefinition())
                    .ToListAsync()
                    .ConfigureAwait(false);

                result.MeasurementDefinitions.AddRange(lstResults);
                result.Result = CommonHelper.CreateResult("Success", true);
            }

            return result;
        }

        /// <summary>
        ///     Update measurement definition with an additional property
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoResult> UpdateMeasurementDefinitionByID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            // ReSharper disable once RedundantAssignment
            var result = new ProtoResult();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.ID <= 0 || request.AdditionalProperties == null)
            {
                result = CommonHelper.CreateResult($"Invalid arguments: {nameof(request.ID)}: {request.ID}, {nameof(request.AdditionalProperties)}: {request.AdditionalProperties}", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasMeasurmentResultPermission(user, _db, Convert.ToInt64(request.ID)).ConfigureAwait(true);

            if (!hasPermission)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var md = await _db.TblMeasurementDefinitions.FirstOrDefaultAsync(a => a.Id == request.ID).ConfigureAwait(true);
            if (md != null)
            {
                md.AdditionalProperties = request.AdditionalProperties;
                await _db.SaveChangesAsync().ConfigureAwait(false);
                result = CommonHelper.CreateResult("Success", true);
            }
            else
            {
                result = CommonHelper.CreateResult($"Measurement Result not found in the database by ID: {request.ID}", false);
                return result;
            }

            return result;
        }
    }
}