// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Interfaces;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Global Config Infos für eine Firma</para>
    ///     Klasse ExGlobalConfig. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGlobalConfig : IBissModel, IBissSelectable, IAdditionalConfiguration
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Typ der Configuration
        /// </summary>
        public EnumGlobalConfigTypes ConfigType { get; set; }

        /// <summary>
        ///     Informationen (Name, Beschreibung, ...)
        /// </summary>
        public ExInformation Information { get; set; } = new ExInformation();

        /// <summary>
        ///     Inhalt der Konfiguration (Zugangsdaten, JSON, Connection-String, ...)
        /// </summary>
        public string AdditionalConfiguration { get; set; } = string.Empty;

        /// <summary>
        ///     Version der Config
        /// </summary>
        public long ConfigVersion { get; set; }

        /// <summary>
        ///     Firma der die Konfiguration zugewiesen ist
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Gerade in Verwendung in mindestens einem Iot-Device
        /// </summary>
        public bool IsUsedInIotDevice { get; set; }


        /// <summary>Ist das aktuelle Element selektiert</summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;


#pragma warning restore CS0067
#pragma warning restore CS0414

#pragma warning disable CS0414
        /// <inheritdoc />
        public event EventHandler<BissSelectableEventArgs> Selected = null!;
#pragma warning restore CS0414

        #endregion
    }
}