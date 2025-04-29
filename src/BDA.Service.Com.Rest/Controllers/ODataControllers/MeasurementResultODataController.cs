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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Rest.Controllers.ODataControllers
{
    /// <summary>
    /// MeasurementResultODataController
    /// </summary>
    public class MeasurementResultODataController : ODataController
    {
        private readonly Db _db;

        /// <summary>
        /// MeasurementResultODataController
        /// </summary>
        /// <param name="db"></param>
        public MeasurementResultODataController(Db db)
        {
            _db = db;
        }

        /*MaxExpansionDepth=2 weil man sonst mit "?$expand=tblmeasurementdefinition($expand=tbliotdevice($expand=tblmeasurementdefinitions))" auch auf Messwerte
            von MesswertDefinitionen ohne "Freigabe für Forschungseinrichtungen" kommt
        */
        /// <summary>
        /// Get
        /// </summary>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 2)]
        [HttpGet("api/odata/measurementresults")]
        [BDAAuthorize]
        public IQueryable /*<TableMeasurementResult>*/ Get()
        {
            HttpContext.TryGetExUserFromHttpContext(out var user);

            IQueryable /*<TableMeasurementResult>*/
                // ReSharper disable once RedundantAssignment
                result = null!;

            if (user.IsAdmin)
            {
                result = _db.TblMeasurementResults
                        .AsNoTracking()
                        .Select(x => new {x.Id, x.TblMeasurementDefinitionId, x.Location, x.Value, x.AdditionalConfiguration, x.AdditionalProperties, x.TimeStamp, x.ValueType, x.TblMeasurementDefinition})
                    ;
            }
            else
            {
                var companyId = _db.TblPermissions
                    .AsNoTracking()
                    .Where(a => a.TblUserId == user.Id)
                    .Select(a => a.TblCompanyId)
                    .FirstOrDefault();

                var permission = _db.TblGateways
                        .AsNoTracking()
                        .Where(a => a.TblCompanyId == companyId)
                        .SelectMany(a => a.TblIotDevices)
                        .SelectMany(x => x.TblMeasurementDefinitions)
                        .SelectMany(x => x.TblMeasurements)
                        //folgende Zeile derzeit notwendig, da SpatialPoint-Property beim Json-Serialisieren Error wirft 
                        //Der Versuch das Property zu ignorieren (siehe Startup.cs) war nicht erfolgreich
                        .Select(x => new {x.Id, x.TblMeasurementDefinitionId, x.Location, x.Value, x.AdditionalConfiguration, x.AdditionalProperties, x.TimeStamp, x.ValueType, x.TblMeasurementDefinition})
                    ;

                result = permission;
            }

            return result;
        }
    }
}