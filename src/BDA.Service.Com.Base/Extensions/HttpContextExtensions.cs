// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model;
using Biss.Log.Producer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BDA.Service.Com.Base.Extensions
{
    /// <summary>
    ///     <para>This class contains extension methods for the HttpContext class.</para>
    ///     Klasse HttpContextExtensions. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool TryGetExUserFromHttpContext(this HttpContext context, out ExUser user)
        {
            try
            {
                var exUser = (ExUser) context.Items["User"]!;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (exUser != null)
                {
                    user = exUser;
                    return true;
                }

                user = null!;
                return false;
            }
            catch (InvalidCastException e)
            {
                Logging.Log.LogError($"{e}");
                user = null!;
                return false;
            }
        }
    }
}