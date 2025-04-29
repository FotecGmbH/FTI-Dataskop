// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Linq;
using BDA.Service.Com.Base.Extensions;
using BDA.Service.Com.Base.Helpers;
using Database;
using Database.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Rest.Controllers.ODataControllers
{
    /// <summary>
    /// IotDeviceODataController
    /// </summary>
    public class IotDeviceODataController : ODataController
    {
        private readonly Db _db;

        /// <summary>
        /// IotDeviceODataController
        /// </summary>
        /// <param name="db"></param>
        public IotDeviceODataController(Db db)
        {
            _db = db;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 1)]
        [HttpGet("api/odata/Iotdevices")]
        [BDAAuthorize]
        public IQueryable<TableIotDevice> Get()
        {
            HttpContext.TryGetExUserFromHttpContext(out var user);

            // ReSharper disable once RedundantAssignment
            IQueryable<TableIotDevice> result = null!;

            if (user.IsAdmin)
            {
                result = _db.TblIotDevices.AsNoTracking();
            }
            else
            {
                var companyId = _db.TblPermissions
                    .Where(a => a.TblUserId == user.Id)
                    .Select(a => a.TblCompanyId)
                    .FirstOrDefault();

                var permission = _db.TblGateways
                    .AsNoTracking()
                    .Where(a => a.TblCompanyId == companyId)
                    .SelectMany(a => a.TblIotDevices);

                result = permission;
            }

            return result;
        }
    }
}