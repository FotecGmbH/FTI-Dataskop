// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;

namespace BDA.Common.Exchange.Configs.Helper
{
    /// <summary>
    ///     <para>Byte - Hilfsfunktionen</para>
    ///     Klasse GcByteHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class GcByteHelper
    {
        /// <summary>
        ///     Bytes in HEX String für Debug Ausgaben
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToHexString(byte[] data)
        {
            if (data == null!)
            {
                return string.Empty;
            }

            var result = string.Empty;
            foreach (var b in data)
            {
                result += "0x" + b.ToString("X2") + " ";
            }

            return result;
        }
    }
}