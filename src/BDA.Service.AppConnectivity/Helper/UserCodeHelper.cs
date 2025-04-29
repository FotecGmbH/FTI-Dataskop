// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Text;
using BDA.Common.Exchange.Configs.UserCode;
using Database.Tables;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse UserCodeHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class UserCodeHelper
    {
        /// <summary>
        /// GetUsercode
        /// </summary>
        /// <param name="definitions"></param>
        /// <param name="snipped"></param>
        /// <returns></returns>
        public static ExUsercode GetUsercode(TableMeasurementDefinition[] definitions, string? snipped = null)
        {
            var result = new ExUsercode();

            var headerBuilder = new StringBuilder("");
            headerBuilder.AppendLine("public static ExValue[] Convert(byte[] input)");
            headerBuilder.AppendLine("{");
            headerBuilder.AppendLine($"\tvar results = new ExValue[{definitions.Length}];");
            headerBuilder.AppendLine("");

            for (var i = 0; i < definitions.Length; i++)
            {
                headerBuilder.AppendLine($"\t// {definitions[i].Information.Name}");
                headerBuilder.AppendLine($"\tresults[{i}] = new ExValue() {{ Identifier = {definitions[i].Id}, ValueType = {definitions[i].ValueType.GetType().Name}.{definitions[i].ValueType}}};");
                headerBuilder.AppendLine("");
            }

            result.Header = headerBuilder.ToString();

            result.Footer = $"\treturn results; {Environment.NewLine}}}";

            if (snipped != null)
            {
                result.UserCode = snipped;
            }

            return result;
        }
    }
}