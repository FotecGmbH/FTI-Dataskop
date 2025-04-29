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
    public class CompanyService : Company.CompanyBase
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ILogger<CompanyService> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }

        [BDAAuthorize]
        public override async Task<ProtoCompanyResult> GetCompanyByID(ProtoRequestByID request, ServerCallContext context)
        {
            var result = new ProtoCompanyResult();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (!user.IsAdmin || user.HaveNoRightsInCompany(request.ID))
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var tblCompany = await _db.TblCompanies.Where(a => a.Id == request.ID).FirstOrDefaultAsync().ConfigureAwait(false);

            if (tblCompany == null)
            {
                result.Result = CommonHelper.CreateResult($"Company not found in the database by ID: {request.ID}", false);
                return result;
            }

            var company = tblCompany.ToProtoCompany();
            result.Company = company;
            result.Result = CommonHelper.CreateResult("Success", true);

            return result;
        }


        [BDAAuthorize]
        public override async Task<ProtoCompaniesResult> GetAllCompanies(Empty request, ServerCallContext context)
        {
            var result = new ProtoCompaniesResult();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken || !user.IsAdmin)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            if (user.IsAdmin)
            {
                var companies = await _db.TblCompanies.ToArrayAsync().ConfigureAwait(true);

                foreach (var tableCompany in companies)
                {
                    result.Companies.Add(tableCompany.ToProtoCompany());
                }

                result.Result = CommonHelper.CreateResult("Success", true);

                return result;
            }

            var userPermissions = _db.TblPermissions.Include(a => a.TblCompany).AsNoTracking().Where(a => a.TblUserId == user.Id);

            if (!userPermissions.Any())
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            // Needed? Company with rights
            //return Ok(await userPermissions.Select(a => a.ToExRestCompanyWithUserRights()).ToListAsync().ConfigureAwait(true));
            var comp = _db.TblCompanies.Select(a => a.ToProtoCompany());
            result.Companies.AddRange(comp);

            result.Result = CommonHelper.CreateResult("Success", true);


            return result;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<ProtoProjectsResult> GetProjectsOfCompanyByID(ProtoRequestByID request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);
            var result = new ProtoProjectsResult();


            if (request.ID <= 0)
            {
                result.Result = CommonHelper.CreateResult($"[{nameof(GetProjectsOfCompanyByID)}] Requested ID was invalid", false);
                return result;
            }

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

            var projects = _db.TblProjects.Where(a => a.TblCompanyId == request.ID).Select(aa => aa.ToProtoProject());
            result.Projects.AddRange(projects);
            return result;
        }

        public override async Task<ProtoTreeViewResult> GetTreeview(Empty request, ServerCallContext context)
        {
            var result = new ProtoTreeViewResult();
            var isValidToken = context.GetHttpContext().TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                result.Result = CommonHelper.CreateResult("Unauthorized", false);
                return result;
            }

            var companies = new List<ProtoBasicCompanyModel>();


            // If super admin then we send all the companies back
            if (user.IsAdmin)
            {
                companies = _db.TblCompanies.Select(a => a.ToProtoBasicCompany()).ToListAsync().Result;
            }
            else // Otherwise we check all the permissions the user have.
            {
                foreach (var premission in user.Premissions)
                {
                    var company = _db.TblCompanies.FirstOrDefault(c => c.Id == premission.CompanyId);

                    if (company != null)
                    {
                        companies.Add(company.ToProtoBasicCompany());
                    }
                }
            }

            foreach (var company in companies)
            {
                var projects = await _db.TblProjects.Where(p => p.TblCompanyId == company.Information.ID)
                    .Select(p => p.ToProtoBasicProject()).ToListAsync().ConfigureAwait(true);

                foreach (var project in projects)
                {
                    var measurementDefIDs = await _db.TblMeasurementDefinitionToProjectAssignments
                        .Where(md => md.TblProjctId == project.Information.ID)
                        .Select(md => md.TblMeasurementDefinitionId)
                        .ToListAsync().ConfigureAwait(true);

                    var measurementDefs = _db.TblMeasurementDefinitions.Where(md => measurementDefIDs.Contains(md.Id))
                        .Select(md => md.ToProtoBasicMeasurementDefinition());
                    project.MeasurementDefinitions.AddRange(measurementDefs);
                }

                company.Projects.AddRange(projects);
            }

            result.Companies.AddRange(companies);
            result.Result = CommonHelper.CreateResult("Success", true);

            return result;
        }
    }
}