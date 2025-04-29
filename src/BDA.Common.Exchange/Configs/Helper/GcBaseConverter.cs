// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Serialize;

namespace BDA.Common.Exchange.Configs.Helper
{
    /// <summary>
    ///     <para>Hlfsklasse zum deserialisieren von T. Für alle "AdditionalConfiguration" Felder in der Datenbank</para>
    ///     Klasse CommandBaseConverter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GcBaseConverter<T>
    {
        private readonly string _data;

        /// <summary>
        ///     Hlfsklasse zum deserialisieren von ExIotDeviceCommandBase
        /// </summary>
        /// <param name="data"></param>
        public GcBaseConverter(string data)
        {
            _data = data;
            Base = BissDeserialize.FromJson<T>(data);
        }

        #region Properties

        /// <summary>
        ///     T Basis Infos um zu wissen in welchen eigentlichen Typ konvertiert werden kann
        /// </summary>
        public T Base { get; }

        #endregion

        /// <summary>
        ///     Zu einer von T abgeleiteten Klasse konvertieren
        /// </summary>
        /// <typeparam name="T2">Von T abgeleitet</typeparam>
        /// <returns></returns>
        public T2 ConvertTo<T2>()
        {
            return BissDeserialize.FromJson<T2>(_data);
        }
    }
}