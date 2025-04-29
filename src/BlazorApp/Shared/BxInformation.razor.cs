// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Shared
{
    /// <summary>
    ///     Bx Informationen.
    /// </summary>
    public partial class BxInformation
    {
        #region Properties

        /// <summary>
        ///     Ex Information.
        /// </summary>
        [Parameter]
        public ExInformation ExInformation { get; set; } = null !;

        /// <summary>
        ///     Datenbank Id.
        /// </summary>
        [Parameter]
        public long DbId { get; set; }

        #endregion
    }
}