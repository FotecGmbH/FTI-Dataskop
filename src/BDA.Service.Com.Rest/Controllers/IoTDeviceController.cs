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
using BDA.Common.Exchange.Configs.Helper;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Extensions;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.Rest.Helpers;
using BDA.Service.Com.Rest.Mapper;
using Biss.Log.Producer;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange.Interfaces;

namespace BDA.Service.Com.Rest.Controllers
{
    /// <summary>
    /// IoTDeviceController
    /// </summary>
    public class IoTDeviceController : Controller
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        private readonly ITriggerAgent _triggerAgent;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="triggerAgent"></param>
        public IoTDeviceController(Db db, ITriggerAgent triggerAgent)
        {
            _db = db;
            _triggerAgent = triggerAgent;
        }

        /// <summary>
        ///     Listet alle IoT Geräte auf, auf die Benutzer Zugriff hat
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/iotdevice/list")]
        [BDAAuthorize]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> IotDeviceGet()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                return UserAccessControl.Unauthorized();
            }

            if (user.IsAdmin)
            {
                var iotDevices = new List<ExIotDevice>();
                foreach (var ioTDevice in _db.TblIotDevices)
                {
                    iotDevices.Add(ioTDevice.GetTableIotDevice());
                }

                return Ok(iotDevices);
            }

            var companyId = _db.TblPermissions.Where(a => a.TblUserId == user.Id).Select(a => a.TblCompanyId).FirstOrDefault();
            var permission = _db.TblGateways.Where(a => a.TblCompanyId == companyId).Select(a => a.TblIotDevices).FirstOrDefault();

            if (!permission!.Any())
            {
                return UserAccessControl.Unauthorized();
            }

            return Ok(permission);
        }

        /// <summary>
        ///     Abfrage von Messergebnissen eines IoT Gerätes
        /// </summary>
        /// <param name="id">IoT Device ID</param>
        /// <returns></returns>
        [HttpGet("api/iotdevice/measurementresults/{id}")]
        [BDAAuthorize]
        public async Task<IActionResult> IotDeviceGetMeasurementResults(long id)
        {
            var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                return UserAccessControl.Unauthorized();
            }

            if (!user.IsAdmin)
            {
                var ioTdeviceGatewayId = _db.TblIotDevices.Where(a => a.Id == id).Select(a => a.TblGatewayId).FirstOrDefault();
                var permission = _db.TblGateways.Where(a => a.Id == ioTdeviceGatewayId).Select(a => a.TblCompanyId);

                foreach (var perm in permission)
                {
                    var controll = _db.TblPermissions.Where(a => a.TblCompanyId == perm).Select(a => a.TblUserId).FirstOrDefault();
                    if (controll != user.Id)
                    {
                        return UserAccessControl.Unauthorized();
                    }
                }
            }

            var mdIds = _db.TblMeasurementDefinitions.Where(a => a.TblIotDeviceId == id).Select(a => a.Id);

            var queryAble = _db.TblMeasurementResults.AsQueryable().AsNoTracking();

            List<ExRestMeasurementResult> lstResults = new();

            foreach (var mrId in mdIds)
            {
                var latestValue = await queryAble.Where(a => a.TblMeasurementDefinitionId == mrId)
                    .OrderByDescending(a => a.TimeStamp).Select(aa => aa.ToExRestMeasurementResult()).FirstOrDefaultAsync().ConfigureAwait(false);
                if (latestValue != null)
                {
                    lstResults.Add(latestValue);
                }
            }

            return Ok(lstResults);
        }

        /// <summary>
        ///     Downlink senden
        /// </summary>
        /// <param name="id">ID des Iot Geraetes</param>
        /// <param name="base64MessageBytes">message bytes base 64 encoded to string</param>
        /// <returns>Erfolgreich</returns>
        [HttpPost("/api/iotdevice/sendDownlink/{id}/{base64MessageBytes}")]
        [BDAAuthorize]
        public virtual async Task<IActionResult> IoTDeviceSendDownlinkMeasurementDefinitionUpdate(long id, string base64MessageBytes)
        {
            var isValidToken = HttpContext.TryGetExUserFromHttpContext(out var user);

            if (!isValidToken)
            {
                return UserAccessControl.Unauthorized();
            }

            if (!user.IsAdmin)
            {
                var ioTdeviceGatewayId = _db.TblIotDevices.Where(a => a.Id == id).Select(a => a.TblGatewayId).FirstOrDefault();
                var permission = _db.TblGateways.Where(a => a.Id == ioTdeviceGatewayId).Select(a => a.TblCompanyId);

                foreach (var perm in permission)
                {
                    var controll = _db.TblPermissions.Where(a => a.TblCompanyId == perm).Select(a => a.TblUserId).FirstOrDefault();
                    if (controll != user.Id)
                    {
                        return UserAccessControl.Unauthorized();
                    }
                }
            }

            var messageBytes = Array.Empty<byte>();

            try
            {
                messageBytes = Convert.FromBase64String(base64MessageBytes);
            }
            catch (Exception e)
            {
                try
                {
                    base64MessageBytes = base64MessageBytes.Replace("%2B", "+").Replace("%3D", "=").Replace("%2F", "/");
                    messageBytes = Convert.FromBase64String(base64MessageBytes);
                }
                catch (Exception)
                {
                    Logging.Log.LogError($"{e}");
                    BadRequest("Message is no valid base 64");
                }
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (messageBytes == null || messageBytes.Length == 0)
            {
                return BadRequest("Message is empty");
            }

            var iotDevice = _db.TblIotDevices.FirstOrDefault(a => a.Id == id);

            if (iotDevice == null || iotDevice.TblGatewayId == null)
            {
                return BadRequest("could not find iot device");
            }

            var ttnConfig = new GcBaseConverter<GcTtnIotDevice>(iotDevice.AdditionalConfiguration);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ttnConfig == null)
            {
                return BadRequest("cannot send to other devices than TTN currently");
            }

            await _triggerAgent.SendDownlinkMessage((long) iotDevice.TblGatewayId, iotDevice.Id, messageBytes).ConfigureAwait(false);

            return Ok(true);
        }
    }
}