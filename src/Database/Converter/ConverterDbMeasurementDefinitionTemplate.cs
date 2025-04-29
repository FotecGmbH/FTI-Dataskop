// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Common;
using Database.Tables;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Konverter für Measurementdefinitiontemplates</para>
    ///     Klasse ConverterMeasurementDefinitionTemplate. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbMeasurementDefinitionTemplate
    {
        /// <summary>
        ///     Konvertieren in ExDeviceInfo
        /// </summary>
        /// <param name="t">Daten</param>
        /// <returns></returns>
        public static ExMeasurementDefinitionTemplate ToExMeasurementDefinitionTemplate(this TableMeasurementDefinitionTemplate t)
        {
            if (t == null!)
            {
                throw new NullReferenceException($"[{nameof(ConverterDbDataconverter)}]({nameof(ToExMeasurementDefinitionTemplate)}): {nameof(TableMeasurementDefinitionTemplate)} is null!");
            }

            return new ExMeasurementDefinitionTemplate
            {
                ValueType = t.ValueType,
                Information = t.Information.ToExInformation()
            };
        }


        /// <summary>
        ///     ExMeasurementDefinitionTemplate in eine TableMeasurementDefinitionTemplate übernehmen
        /// </summary>
        /// <param name="i">ExMeasurementDefinitionTemplate</param>
        /// <param name="t">TableMeasurementDefinitionTemplate</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableMeasurementDefinitionTemplate(this ExMeasurementDefinitionTemplate i, TableMeasurementDefinitionTemplate t)
        {
            if (i == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinitionTemplate)}]({nameof(ExMeasurementDefinitionTemplate)}): {nameof(i)} is null");
            }

            if (t == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbMeasurementDefinitionTemplate)}]({nameof(TableMeasurementDefinitionTemplate)}): {nameof(t)} is null");
            }

            t.Information = new DbInformation();
            i.Information.ToDbInformation(t.Information);

            t.ValueType = i.ValueType;
            t.TblDataconverterId = i.DataconverterId;
        }
    }
}