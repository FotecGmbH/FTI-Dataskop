// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model;

namespace BDA.Common.Exchange.Interfaces
{
    /// <summary>
    ///     <para>Device Infos Interface</para>
    ///     Klasse IDeviceInfos. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IDeviceInfos
    {
        /// <summary>
        ///     Plattform Infos auslesen
        /// </summary>
        /// <returns></returns>
        ExDeviceInfo GetInfosDeviceInfo();
    }
}