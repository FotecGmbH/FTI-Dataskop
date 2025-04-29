// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse GeosphereApiResponse. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GeosphereApiResponse
    {
        #region Properties

        /// <summary>
        /// media_type
        /// </summary>
        public string media_type { get; set; } = string.Empty;

        /// <summary>
        /// type
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// version
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// timestamps
        /// </summary>
        public List<string> timestamps { get; set; } = null!;

        /// <summary>
        /// features
        /// </summary>
        public List<JObject> features { get; set; } = null!;

        #endregion
    }
}