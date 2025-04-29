// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Konverter für dataconvertertemplates</para>
    ///     Klasse ConverterDbDataconverter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbDataconverter
    {
        /// <summary>
        ///     Konvertieren in ExDeviceInfo
        /// </summary>
        /// <param name="t">Daten</param>
        /// <returns></returns>
        public static ExDataconverter ToExDataconverter(this TableDataconverter t)
        {
            if (t == null!)
            {
                throw new NullReferenceException($"[{nameof(ConverterDbDataconverter)}]({nameof(ToExDataconverter)}): {nameof(TableDataconverter)} is null!");
            }

            var result = new ExDataconverter
            {
                Description = t.Description,
                CodeSnippet = t.CodeSnippet,
                Displayname = t.Displayname,
                Templates = new List<ExMeasurementDefinitionTemplate>()
            };
            foreach (var template in t.Templates)
            {
                result.Templates.Add(template.ToExMeasurementDefinitionTemplate());
            }

            return result;
        }

        /// <summary>
        ///     ExGlobalConfig in eine TableCompanyGlobalConfig übernehmen
        /// </summary>
        /// <param name="i">ExGlobalConfig</param>
        /// <param name="t">TableCompanyGlobalConfig</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableDataconverter(this ExDataconverter i, TableDataconverter t)
        {
            if (i == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbDataconverter)}]({nameof(ExDataconverter)}): {nameof(i)} is null");
            }

            if (t == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbDataconverter)}]({nameof(TableDataconverter)}): {nameof(t)} is null");
            }

            t.CodeSnippet = i.CodeSnippet;
            t.Description = i.Description;
            t.Displayname = i.Displayname;

            t.Templates = new List<TableMeasurementDefinitionTemplate>();

            foreach (var def in i.Templates)
            {
                var tblDefinitionTemplate = new TableMeasurementDefinitionTemplate();
                def.ToTableMeasurementDefinitionTemplate(tblDefinitionTemplate);
                t.Templates.Add(tblDefinitionTemplate);
            }
        }
    }
}