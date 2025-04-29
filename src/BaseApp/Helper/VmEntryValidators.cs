// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Exchange.Resources;

namespace BaseApp.Helper
{
    /// <summary>
    ///     <para>VmEntry allegemeine Prüffunktionen</para>
    ///     Klasse VmEntryValidators. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class VmEntryValidators
    {
        /// <summary>
        ///     String nach Int
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncInt(string arg)
        {
            if (!int.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine ganze Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Int
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncByte(string arg)
        {
            if (!byte.TryParse(arg, out _))
            {
                return ("Ungültig. Muss ein Byte sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Double
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncDouble(string arg)
        {
            if (!double.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Double
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncFloatButAlwaysTrue(string arg)
        {
            if (!float.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine Zahl sein!", true);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nicht leer
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncStringEmpty(string arg)
        {
            if (String.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }
    }
}