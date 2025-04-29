// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;

// ReSharper disable once CheckNamespace
namespace BDA.Service.Com.Base
{
    /// <summary>
    ///     <para>Resultat einer Geoabfragen</para>
    ///     Klasse ExGeoResult.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExRestGeoResult
    {
        /// <summary>
        ///     Konsturkor
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="measurementDefinitionId">Id der MeasuremenetDefinition</param>
        /// <param name="timeStamp">Zeitstempel</param>
        /// <param name="distance">Distanz</param>
        /// <param name="valueBinary">Wert (Binär)</param>
        /// <param name="valueText">Wert (Text)</param>
        /// <param name="valueNumber">Wert (Zahl)</param>
        /// <param name="valueBit">Wert (Bool)</param>
        /// <param name="valueType">Wertetyp</param>
        /// <param name="lat">Latitude</param>
        /// <param name="lng">Longitude</param>
        /// <param name="alt">Altitude</param>
        /// <param name="additionalProperties">zusatzinformationen</param>
        public ExRestGeoResult(long id, long measurementDefinitionId, DateTime timeStamp, double distance, byte[]? valueBinary, string valueText, double? valueNumber, bool? valueBit, EnumValueTypes valueType, double lat, double lng, double alt, string additionalProperties)
        {
            Id = id;
            MeasurementDefinitionId = measurementDefinitionId;
            TimeStamp = timeStamp;
            Distance = distance;
            ValueType = valueType;
            Lat = lat;
            Lng = lng;
            Alt = alt;
            AdditionalProperties = additionalProperties;
            switch (valueType)
            {
                case EnumValueTypes.Number:
                    if (valueNumber != null)
                    {
                        Value = valueNumber.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    break;
                case EnumValueTypes.Data:
                case EnumValueTypes.Image:
                    if (valueBinary is {Length: > 0})
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        Value = Convert.ToBase64String(valueBinary);
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    break;
                case EnumValueTypes.Text:
                    Value = valueText;
                    break;
                case EnumValueTypes.Bit:
                    if (valueBit != null)
                    {
                        Value = valueBit.Value.ToString();
                    }

                    break;
                default:
                    Value = valueText;
                    break;
            }
        }

        #region Properties

        /// <summary>
        ///     Id
        /// </summary>
        public long Id { get; }

        /// <summary>
        ///     Id of the MeasurementDefinition
        /// </summary>
        public long MeasurementDefinitionId { get; }

        /// <summary>
        ///     Zeitpunkt
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        ///     Distanz
        /// </summary>
        public double Distance { get; }

        /// <summary>
        ///     Wert
        /// </summary>
        public string Value { get; set; } = String.Empty;

        /// <summary>
        ///     Wertetyp
        /// </summary>
        public EnumValueTypes ValueType { get; }

        /// <summary>
        ///     Latitude
        /// </summary>
        public double Lat { get; }

        /// <summary>
        ///     Longitude
        /// </summary>
        public double Lng { get; }

        /// <summary>
        ///     Altitude
        /// </summary>
        public double Alt { get; }

        /// <summary>
        ///     Zusatzinformationen
        /// </summary>
        public string AdditionalProperties { get; }

        #endregion

        /// <summary>
        ///     Wert umwandeln
        /// </summary>
        /// <returns>Wert als String</returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // ReSharper disable once UnusedMember.Local
        private string GetValue(ExValue value, EnumValueTypes valueType)
        {
            return "";
        }
    }
}