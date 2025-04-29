// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Service.EMail.Services
{
    /// <summary>
    ///     Custom Razor Engine -> Razor zu HTML für Mails
    /// </summary>
    public interface ICustomRazorEngine
    {
        /// <summary>
        ///     View in HTML generieren
        /// </summary>
        /// <typeparam name="TModel">Model Typ</typeparam>
        /// <param name="viewName">View Name</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        Task<string> RazorViewToHtmlAsync<TModel>(string viewName, TModel model);
    }
}