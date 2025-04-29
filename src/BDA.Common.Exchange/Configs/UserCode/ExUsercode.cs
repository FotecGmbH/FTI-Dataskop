// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;
using Newtonsoft.Json;

namespace BDA.Common.Exchange.Configs.UserCode
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ExUsercode. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUsercode : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Der Codeabschnitt der die Results instanziert
        /// </summary>
        public string Header { get; set; } = "";

        /// <summary>
        ///     Der Codeabschnitt der die Daten Parsed ( Bearbeitbar vom User)
        /// </summary>
        public string UserCode { get; set; } = "";

        /// <summary>
        ///     Schlussteile des Codes ( Keine relevanten Funktionsaufrufe lediglich um eine valide Syntax einzuhalten)
        /// </summary>
        public string Footer { get; set; } = "";

        /// <summary>
        ///     Vollständiges Codesnipped
        /// </summary>
        [JsonIgnore]
        public string CompleteCode => string.Join(Environment.NewLine, Header, UserCode, Footer);

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}