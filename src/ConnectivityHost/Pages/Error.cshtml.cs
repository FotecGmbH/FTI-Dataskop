// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     Error Model
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        /// <summary>
        ///     CTOR
        /// </summary>
        /// <param name="logger">Logger</param>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        #region Properties

        /// <summary>
        ///     Request ID
        /// </summary>
        public string RequestId { get; set; } = null!;

        /// <summary>
        ///     Show ID
        /// </summary>

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        #endregion

        /// <summary>
        ///     HTTPGET
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}