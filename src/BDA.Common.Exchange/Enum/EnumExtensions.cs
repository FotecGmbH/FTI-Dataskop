// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Biss.Apps.ViewModel;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace BDA.Common.Exchange.Enum
{
    /// <summary>
    ///     <para>Extension-Methods für enums</para>
    ///     Klasse EnumExtensions. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Display name fuer enum Wert
        /// </summary>
        /// <param name="enumValue">enum</param>
        /// <returns>Display name</returns>
        public static string GetDisplayName(this System.Enum enumValue)
        {
            var text = enumValue.ToString();
            return enumValue.GetType().GetMember(text).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? text;
        }

        /// <summary>
        ///     Kurzer Display name fuer enum wert
        /// </summary>
        /// <param name="enumValue">enum wert</param>
        /// <returns>Display name</returns>
        public static string GetDisplayShortName(this System.Enum enumValue)
        {
            var text = enumValue.ToString();
            return enumValue.GetType().GetMember(text).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetShortName() ?? text;
        }

        /// <summary>
        /// AddKeys
        /// </summary>
        /// <param name="picker"></param>
        /// <param name="compareOperators"></param>
        public static void AddKeys(this VmPicker<EnumCompareOperator> picker, IEnumerable<EnumCompareOperator> compareOperators)
        {
            if (picker is null || compareOperators is null)
            {
                return;
            }

            foreach (var compareOperator in compareOperators)
            {
                var useShortName = compareOperator != EnumCompareOperator.Contains && compareOperator != EnumCompareOperator.NotContains;
                picker.AddKey(compareOperator, useShortName ? compareOperator.GetDisplayShortName() : compareOperator.GetDisplayName());
            }
        }
    }
}