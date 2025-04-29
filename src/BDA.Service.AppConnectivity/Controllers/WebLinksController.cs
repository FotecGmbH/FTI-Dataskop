// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.EMail.Services;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerRemoteCalls = BDA.Service.AppConnectivity.DataConnector.ServerRemoteCalls;

namespace BDA.Service.AppConnectivity.Controllers
{
    /// <summary>
    ///     <para>Controller für Aktionen via Web-Links</para>
    ///     Klasse WebLinksController. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class WebLinksController : Controller
    {
        private readonly ServerRemoteCalls _hub;

        /// <summary>
        ///     Razor Engine
        /// </summary>
        private readonly ICustomRazorEngine _razorEngine;

        /// <summary>
        ///     Konstruktor - RemoteCalls und RazorEngine (Mails) per DI
        /// </summary>
        /// <param name="connections">DC Verbindung</param>
        /// <param name="calls">RemoteProcedureCalls</param>
        /// <param name="razorEngine">RazorEngine</param>
        public WebLinksController(IDcConnections connections, ICustomRazorEngine razorEngine, IServerRemoteCalls calls)
        {
            _hub = (ServerRemoteCalls) calls;
            _razorEngine = razorEngine;
            _hub.SetClientConnection(connections);
        }

        /// <summary>
        ///     Für Testzwecke
        /// </summary>
        /// <returns></returns>
        [Route("api/testmail")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Testmail()
        {
            var nt = new NotificationTestService();
            await nt.SendTestEmail(_razorEngine, new TableUser {FirstName = "Manuel", LastName = "Fasching", LoginName = "fasching@fotec.at"}, "Hallo!!!").ConfigureAwait(false);

            return View("Message", "Account wurde nicht gefunden.");
        }

        /// <summary>
        ///     EMail gedrückt zum Bestätigen des Users
        /// </summary>
        /// <param name="deviceId">Device Id (Datenbank)</param>
        /// <param name="userId">User Id (Datenbank)</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/UserValidateEMail/{deviceId}/{userId}/{token}")]
        [HttpGet]
        public async Task<IActionResult> UserValidateEMail(long deviceId, long userId, string token)
        {
            Logging.Log.LogInfo($"[{nameof(WebLinksController)}]({nameof(UserValidateEMail)}): {deviceId},{userId},{token}");
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var data = await db.TblUsers.Where(i => i.Id == userId).Include(x => x.TblUserImage).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);

            if (data == null)
            {
                return View("Message", "Account wurde nicht gefunden.");
            }

            if (data.LoginConfirmed)
            {
                return View("Message", "Account wurde bereits erfolgreich aktiviert. Bitte fahren Sie in der App fort.");
            }

            if (token != data.ConfirmationToken)
            {
                return View("Message", "Aktivierungslink ist nicht gültig. Bitte fordern Sie einen neuen Aktivierungslink an.");
            }

            data.LoginConfirmed = true;
            data.ConfirmationToken = string.Empty;

            //Könnte auch später passieren in einem WebFrontend zb (Beispiel smartflower)
            data.Locked = false;

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(WebLinksController)}]({nameof(UserValidateEMail)}):  {e}");
                return View("Message", "Beim Verarbeiten der Daten ist ein Problem aufgetreten. Versuchen Sie es bitte später erneut.");
            }

            if (deviceId > 0)
            {
                await _hub.SendDcExUser(data.ToExUser(), deviceId).ConfigureAwait(false);
            }

            return View("Message", "Account wurde erfolgreich aktiviert. Bitte fahren Sie in der App fort.");
        }

        /// <summary>
        ///     EMail gedrückt zum Bestätigen des Users
        /// </summary>
        /// <param name="userId">User Id (Datenbank)</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/UserValidateEMail/{userId}/{token}")]
        [HttpGet]
        public async Task<IActionResult> UserValidateEMail(long userId, string token)
        {
            Logging.Log.LogInfo($"[{nameof(WebLinksController)}]({nameof(UserValidateEMail)}): {userId},{token}");
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var data = await db.TblUsers.Where(i => i.Id == userId).Include(x => x.TblUserImage).Include(y => y.TblDevices).FirstOrDefaultAsync().ConfigureAwait(false);

            if (data == null)
            {
                return View("Message", "Account wurde nicht gefunden.");
            }

            if (data.LoginConfirmed)
            {
                return View("Message", "Account wurde bereits erfolgreich aktiviert. Bitte fahren Sie in der App fort.");
            }

            if (token != data.ConfirmationToken)
            {
                return View("Message", "Aktivierungslink ist nicht gültig. Bitte fordern Sie einen neuen Aktivierungslink an.");
            }

            data.LoginConfirmed = true;
            data.ConfirmationToken = string.Empty;

            //Könnte auch später passieren in einem WebFrontend zb (Beispiel smartflower)
            data.Locked = false;

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(WebLinksController)}]({nameof(UserValidateEMail)}):  {e}");
                return View("Message", "Beim Verarbeiten der Daten ist ein Problem aufgetreten. Versuchen Sie es bitte später erneut.");
            }

            return View("Message", "Account wurde erfolgreich aktiviert. Bitte fahren Sie in der App fort.");
        }

        /// <summary>
        ///     EMail gedrückt zum Rücksetzen des User Passworts
        /// </summary>
        /// <param name="userId">User Id (Datenbank)</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/UserResetPassword/{userId}/{token}")]
        [HttpGet]
        public async Task<IActionResult> UserResetPassword(int userId, string token)
        {
            // ReSharper disable once RedundantAssignment
            TableUser? user = null;
            Logging.Log.LogInfo($"[{nameof(WebLinksController)}]({nameof(UserResetPassword)}):  {userId},{token}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            user = await db.TblUsers.Where(t => userId == t.Id).FirstOrDefaultAsync().ConfigureAwait(true);

            if (user == null)
            {
                return View("Message", "Ungültige Anforderung.");
            }

            if (token != user.ConfirmationToken)
            {
                return View("Message", "Aktivierungslink ist ungültig. Bitte fordern Sie einen neuen Link an.");
            }

            UserAccountEmailService ems = new(_razorEngine);
            await ems.SendPasswordResetConfirmationMail(user).ConfigureAwait(true);

            return View("Message", "Passwort wurde erfolgreich zurückgesetzt. Bitte überprüfen Sie Ihren Posteingang.");
        }
    }
}