// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Extensions;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Mapper;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Rest.Controllers;

/// <summary>
///     <para>Firmen</para>
///     Klasse CompanyController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class CompanyController : Controller
{
    /// <summary>
    ///     Datenbank Context
    /// </summary>
    private readonly Db _db;

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="db">Database</param>
    public CompanyController(Db db)
    {
        _db = db;
    }

    /// <summary>
    ///     Firmeninformationen abfragen
    /// </summary>
    /// <param name="id">ID der Firma</param>
    /// <returns>Informationen über die Firma</returns>
    [HttpGet("/api/company/{id}")]
    [BDAAuthorize]
    public virtual async Task<IActionResult> CompanyGet(long id)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

        if (!isValidToken || user.HaveNoRightsInCompany(id))
        {
            return UserAccessControl.Unauthorized();
        }

        var company = await _db.TblCompanies.Where(a => a.Id == id).Select(aa => aa.ToExRestCompany()).FirstOrDefaultAsync().ConfigureAwait(false);

        return Ok(company);
    }

    /// <summary>
    ///     Projekte einer Firma abfragen
    /// </summary>
    /// <param name="id">ID der Firma</param>
    /// <returns>Projekte der Firma</returns>
    [HttpGet("/api/company/projects/{id}")]
    [EnableQuery]
    [BDAAuthorize]
    public IActionResult CompanyProjects(int id = -1)
    {
        if (id <= 0)
        {
            return Ok(false);
        }

        var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

        if (!isValidToken || user.HaveNoRightsInCompany(id))
        {
            return UserAccessControl.Unauthorized();
        }

        var res = _db.TblProjects.Where(a => a.TblCompanyId == id).Select(aa => aa.ToExRestProject());

        return Ok(res);
    }

    /// <summary>
    ///     Alle Firmen abfragen für die der Token gültig ist
    /// </summary>
    /// <returns>Liste der Firmen</returns>
    [HttpGet("/api/company/list")]
    [BDAAuthorize]
    public async Task<IActionResult> CompanyList()
    {
        var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

        if (!isValidToken)
        {
            return UserAccessControl.Unauthorized();
        }


        if (user.IsAdmin)
        {
            var companies = _db.TblCompanies.Select(a => a.ToExRestCompanyWithUserRights(EnumUserRight.ReadWrite));
            return Ok(companies);
        }

        var userPermissions = _db.TblPermissions.Include(a => a.TblCompany).AsNoTracking().Where(a => a.TblUserId == user.Id);

        if (!userPermissions.Any())
        {
            return UserAccessControl.Unauthorized();
        }

        return Ok(await userPermissions.Select(a => a.ToExRestCompanyWithUserRights()).ToListAsync().ConfigureAwait(true));
    }

    /// <summary>
    ///     Alle Firmen abfragen für Super-Admin für die der Token gültig ist
    /// </summary>
    /// <returns>Liste der Firmen</returns>
    [HttpGet("/api/company/treeview")]
    [BDAAuthorize]
    public virtual IActionResult CompanyTreeview()
    {
        var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

        if (!isValidToken)
        {
            return UserAccessControl.Unauthorized();
        }

        var companies = new List<ExBasicCompany>();


        // If super admin then we send all the companies back
        if (user.IsAdmin)
        {
            companies = _db.TblCompanies.Select(a => a.ToExBasicCompany()).ToListAsync().Result;
        }
        else // Otherwise we check all the permissions the user have.
        {
            foreach (var premission in user.Premissions)
            {
                var company = _db.TblCompanies.FirstOrDefault(c => c.Id == premission.CompanyId);

                if (company != null)
                {
                    companies.Add(company.ToExBasicCompany());
                }
            }
        }

        foreach (var company in companies)
        {
            var projects = _db.TblProjects.Where(p => p.TblCompanyId == company.Information.ID)
                .Select(p => p.ToExBasicProject()).ToList();

            foreach (var project in projects)
            {
                var measurementDefIDs = _db.TblMeasurementDefinitionToProjectAssignments
                    .Where(md => md.TblProjctId == project.Information.ID)
                    .Select(md => md.TblMeasurementDefinitionId)
                    .ToList();

                var measurementDefs = _db.TblMeasurementDefinitions.Where(md => measurementDefIDs.Contains(md.Id))
                    .Select(md => md.ToExBasicMeasurmentDefinition());
                project.MeasurementDefinitions.AddRange(measurementDefs);
            }

            company.Projects.AddRange(projects);
        }

        return Ok(companies);
    }
}