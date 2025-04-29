// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.Model.ConfigApp
{
    /// <summary>
    ///     <para>Zugriffstoken der Benutzer</para>
    ///     Klasse ExAccessToken. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExAccessToken : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     Id des Token in der DB
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        ///     Gültig bis
        /// </summary>
        public DateTime GuiltyUntilUtc { get; set; }

        /// <summary>
        /// IsSelected
        /// </summary>
        public bool IsSelected { get; set; }
        /// <summary>
        /// CanEnableIsSelect
        /// </summary>
        public bool CanEnableIsSelect { get; set; }

        #endregion

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;

        /// <summary>
        /// Selected
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event EventHandler<BissSelectableEventArgs> Selected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS0067
#pragma warning restore CS0414
    }
}