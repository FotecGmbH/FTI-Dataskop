// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using System.Linq;
using BDA.Common.Exchange.Enum;
using Database.Common;
using Database.Tables;

namespace BDA.Service.Com.Base.Helpers
{
    /// <summary>
    ///     <para>This class contains helper methods to be used for both REST and gRPC interface</para>
    ///     Klasse CommonMethodsHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class CommonMethodsHelper
    {
        /// <summary>
        ///     Wert umwandeln
        /// </summary>
        /// <param name="mr">Wert mit Wertetyp</param>
        /// <returns>Wert als String</returns>
        public static string GetValueOfMeasurementResult(TableMeasurementResult mr)
        {
            var value = mr.Value;
            var valueType = mr.ValueType;

            switch (valueType)
            {
                case EnumValueTypes.Number:
                    if (value.Number != null)
                    {
                        return value.Number.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    break;
                case EnumValueTypes.Data:
                case EnumValueTypes.Image:
                    if (value.Binary is {Length: > 0})
                    {
                        return Convert.ToBase64String(value.Binary);
                    }

                    break;
                case EnumValueTypes.Text:
                    return value.Text;
                case EnumValueTypes.Bit:
                    if (value.Bit != null)
                    {
                        return value.Bit.Value.ToString();
                    }

                    break;
                default:
                    return value.Text;
            }

            return "";
        }


        /// <summary>
        /// GetValueOfMeasurementResult
        /// </summary>
        /// <param name="mrValue"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static DbValue GetValueOfMeasurementResult(string mrValue, EnumValueTypes valueType)
        {
            switch (valueType)
            {
                case EnumValueTypes.Number:
                    if (double.TryParse(mrValue, out var nrResult))
                    {
                        return new DbValue
                        {
                            Number = nrResult,
                        };
                    }

                    if (double.TryParse(mrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var nrResultInvariant))
                    {
                        return new DbValue
                        {
                            Number = nrResultInvariant,
                        };
                    }

                    break;
                case EnumValueTypes.Data:
                case EnumValueTypes.Image:
                    return new DbValue
                    {
                        Binary = Convert.FromBase64String(mrValue),
                    };
                case EnumValueTypes.Text:
                    return new DbValue
                    {
                        Text = mrValue,
                    };
                case EnumValueTypes.Bit:
                    if (bool.TryParse(mrValue, out var bResult))
                    {
                        return new DbValue
                        {
                            Bit = bResult,
                        };
                    }

                    break;
                default:
                    return new DbValue
                    {
                        Text = mrValue,
                    };
            }

            return new DbValue();
        }


        /// <summary>
        /// ApplyOrderbyOnMeasurementResults
        /// </summary>
        /// <param name="tblMeasurementResults"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public static IQueryable<TableMeasurementResult> ApplyOrderbyOnMeasurementResults(this IQueryable<TableMeasurementResult> tblMeasurementResults, string orderBy)
        {
            if (!string.IsNullOrEmpty(orderBy) && orderBy.Contains(' '))
            {
                orderBy = orderBy.ToLower();

                var orderbySplitted = orderBy.Split(' ');

                var orderByAsc = !(orderbySplitted.Length > 1 && orderbySplitted[1] == "desc");

                switch (orderbySplitted[0])
                {
                    case "timestamp":
                        tblMeasurementResults = orderByAsc ? tblMeasurementResults.OrderBy(a => a.TimeStamp) : tblMeasurementResults.OrderByDescending(a => a.TimeStamp);
                        break;
                    case "id":
                        tblMeasurementResults = orderByAsc ? tblMeasurementResults.OrderBy(a => a.Id) : tblMeasurementResults.OrderByDescending(a => a.Id);
                        break;
                }
            }

            return tblMeasurementResults;
        }
    }
}