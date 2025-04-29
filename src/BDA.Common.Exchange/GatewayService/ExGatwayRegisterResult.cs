// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Ergbnis wenn ein Gateway registriert wird</para>
    ///     Klasse ExGatwayRegisterResult. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExGatwayRegisterResult : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Secret / DB Id passen nicht zusammen - Gateway muss offline gehen!
        /// </summary>
        public bool Invalid { get; set; }

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