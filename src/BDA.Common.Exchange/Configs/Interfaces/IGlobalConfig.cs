// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Enums;

namespace BDA.Common.Exchange.Configs.Interfaces
{
    /// <summary>
    ///     <para>Globale Config</para>
    ///     Interface IGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IGlobalConfig
    {
        #region Properties

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; }

        #endregion
    }
}