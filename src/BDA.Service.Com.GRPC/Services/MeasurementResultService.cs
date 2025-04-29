// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using BDA.Common.Exchange.Enum;
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
    public class MeasurementResultService : MeasurementResult.MeasurementResultBase
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<CompanyService> _logger;

        public MeasurementResultService(ILogger<CompanyService> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        ///     Not done
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoMeasurementResultReply> GetMeasurementResultByID(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoMeasurementResultReply();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(MeasurementResultService)}] [{nameof(GetMeasurementResultByID)}] ID cannot be less than or equal to 0", false);
                return result;
            }

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasMeasurmentResultPermission(user, _db, request.ID).ConfigureAwait(true);

            if (!hasPermission || !isValidToken)
            {
            }

            var mr = _db.TblMeasurementResults.FirstOrDefault(mr => mr.Id == request.ID);

            if (mr != null)
            {
                result.MeasurementResult = mr.ToProtoMeasurementResult();
                result.Result = CommonHelper.CreateResult("Success", true);
            }

            return result;
        }

        /// <summary>
        ///     Not done
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<ProtoResult> UpdateMeasurementResultByID(ProtoFilterRequestByID request, ServerCallContext context)
        {
            // ReSharper disable once RedundantAssignment
            var result = new ProtoResult();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (request.ID <= 0 || request.AdditionalProperties == null)
            {
                result = CommonHelper.CreateResult($"Invalid arguments: {nameof(request.ID)}: {request.ID}, {nameof(request.AdditionalProperties)}: {request.AdditionalProperties}", false);
                return result;
            }

            if (!isValidToken)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var hasPermission = await UserAccessControl.HasMeasurmentResultPermission(user, _db, Convert.ToInt64(request.ID)).ConfigureAwait(true);

            if (!hasPermission)
            {
                result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var md = await _db.TblMeasurementResults.FirstOrDefaultAsync(a => a.Id == request.ID).ConfigureAwait(true);
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

        /// <summary>
        ///     Not done
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<ProtoMeasurementResultQueryReply> QueryMeasurementResults(ProtoMeasurementResultQueryRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var result = new ProtoMeasurementResultQueryReply();

            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (request.BasicQueryOptions.ID == -1 && !user.IsAdmin)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var queryAble = _db.TblMeasurementResults.AsQueryable();

            queryAble = queryAble.ApplyOrderbyOnMeasurementResults(request.OrderBy);

            if (!request.AllValueTypes)
            {
                switch (request.ValueType)
                {
                    case ProtoEnumValueTypes.ValueTypeBit:
                        queryAble = queryAble.Where(a => a.ValueType == EnumValueTypes.Bit);
                        break;
                    case ProtoEnumValueTypes.ValueTypeData:
                        queryAble = queryAble.Where(a => a.ValueType == EnumValueTypes.Data);
                        break;
                    case ProtoEnumValueTypes.ValueTypeImage:
                        queryAble = queryAble.Where(a => a.ValueType == EnumValueTypes.Image);
                        break;
                    case ProtoEnumValueTypes.ValueTypeNumber:
                        queryAble = queryAble.Where(a => a.ValueType == EnumValueTypes.Number);
                        break;
                    case ProtoEnumValueTypes.ValueTypeText:
                        queryAble = queryAble.Where(a => a.ValueType == EnumValueTypes.Text);
                        break;
                }
            }

            if (!String.IsNullOrEmpty(request.AdditionalPropertyFilter))
            {
                queryAble = queryAble.Where(a => a.AdditionalProperties.Contains(request.AdditionalPropertyFilter));
            }


            var measurentResults = queryAble.Where(a => a.TblMeasurementDefinitionId == request.BasicQueryOptions.ID || request.BasicQueryOptions.ID == -1)
                .Select(aa => aa.ToProtoMeasurementResult())
                .Skip(request.BasicQueryOptions.Skip)
                .Take(request.BasicQueryOptions.Take);
            result.MeasurementResults.AddRange(measurentResults);
            result.Count = _db.TblMeasurementResults.Count(a => a.TblMeasurementDefinitionId == request.BasicQueryOptions.ID || request.BasicQueryOptions.ID == -1);

            result.Result = CommonHelper.CreateResult("Success", true);
            return result;
        }
    }
}