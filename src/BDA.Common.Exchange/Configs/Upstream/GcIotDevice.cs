// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.Interfaces;
using BDA.Common.Exchange.Configs.UserCode;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Configs.Upstream
{
    /// <summary>
    ///     Klasse für zusätzliche Einstellungen eines IoTDevices
    /// </summary>
    public class GcIotDevice : IBissModel, IGlobalConfig
    {
        #region Properties

        /// <summary>
        ///     Code zum parsen der Daten
        /// </summary>
        public ExUsercode? UserCode { get; set; }

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}