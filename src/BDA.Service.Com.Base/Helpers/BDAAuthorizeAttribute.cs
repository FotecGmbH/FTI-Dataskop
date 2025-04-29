// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BDA.Service.Com.Base.Helpers
{
    /// <summary>
    ///     Klasse BDAAuthorizeAttribute.cs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BDAAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        #region Interface Implementations

        /// <summary>
        ///     Authorize Attribut
        /// </summary>
        /// <param name="context">Kontext</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentException(null, nameof(context));
            }

            var user = context.HttpContext!.Items!["User"] as ExUser;
            if (user == null)
            {
                // not logged in
                context.Result = new JsonResult(new {message = "Unauthorized"}) {StatusCode = StatusCodes.Status401Unauthorized};
            }
        }

        #endregion
    }
}