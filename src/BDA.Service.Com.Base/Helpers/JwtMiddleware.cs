// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Converter;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Base.Helpers
{
    /// <summary>
    /// JwtMiddleware
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        /// <summary>
        /// JwtMiddleware
        /// </summary>
        /// <param name="next"></param>
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, Db db)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await attachUserToContext(context, db, token);
            }

            await _next(context);
        }
        /// <summary>
        /// attachUserToContext
        /// </summary>
        /// <param name="context"></param>
        /// <param name="db"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once InconsistentNaming
        private async Task attachUserToContext(HttpContext context, Db db, string token)
        {
            try
            {
                // attach user to context on successful jwt validation
                var accessToken = await db.TblAccessToken.FirstOrDefaultAsync(a => a.Token == token).ConfigureAwait(true);
                if (accessToken != null)
                {
                    var user = await db.TblUsers.Include(a => a.TblAccessToken)
                        .Include(a => a.TblDevices)
                        .Include(a => a.TblPermissions)
                        .FirstOrDefaultAsync(a => a.Id == accessToken.TblUserId).ConfigureAwait(true);
                    context.Items["User"] = user!.ToExUser();
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}