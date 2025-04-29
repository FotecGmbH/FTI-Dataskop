// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.GRPC.Helpers;
using BDA.Service.Com.GRPC.Protos;
using Database.Tables;
using Google.Protobuf;
using NetTopologySuite.Geometries;

namespace BDA.Service.Com.GRPC.Extensions
{
    /// <summary>
    ///     <para>This class contains extension methods to convert company models</para>
    ///     Klasse ModelConverterExtensions. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ModelConverterExtensions
    {
        #region Measuremenet Result

        /// <summary>
        ///     Companies, tblProject zu ExRestProject
        /// </summary>
        /// <exception cref="ArgumentNullException">Wenn tblProject null ist</exception>
        /// <returns></returns>
        public static ProtoMeasurementResultModel ToProtoMeasurementResult(this TableMeasurementResult tblMeasurementResult)
        {
            if (tblMeasurementResult == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoProject)}): {nameof(tblMeasurementResult)}");
            }

            var measurementResult = new ProtoMeasurementResultModel
            {
                Location = new ProtoPositionModel
                {
                    Altitude = tblMeasurementResult.Location.Altitude,
                    Latitude = tblMeasurementResult.Location.Latitude,
                    Longitude = tblMeasurementResult.Location.Longitude,
                    Presision = tblMeasurementResult.Location.Precision,
                    TimeStamp = tblMeasurementResult.Location.TimeStamp.ConvertToUtcGoogleTimestamp(),
                },
                AdditionalProperties = tblMeasurementResult.AdditionalProperties,
                Value = CommonMethodsHelper.GetValueOfMeasurementResult(tblMeasurementResult),
                TimeStamp = tblMeasurementResult.TimeStamp.ConvertToUtcGoogleTimestamp(),
                ValueType = CommonHelper.ConvertValueTypeToProtoValueType(tblMeasurementResult.ValueType),
                ID = tblMeasurementResult.Id
            };

            switch (tblMeasurementResult.Location.Source)
            {
                case EnumPositionSource.Internet:
                    measurementResult.Location.Source = ProtoEnumPositionSource.PositionSourceInternet;
                    break;
                case EnumPositionSource.Lbs:
                    measurementResult.Location.Source = ProtoEnumPositionSource.PositionSourceLbs;
                    break;
                case EnumPositionSource.Modul:
                    measurementResult.Location.Source = ProtoEnumPositionSource.PositionSourceModul;
                    break;
                case EnumPositionSource.Pc:
                    measurementResult.Location.Source = ProtoEnumPositionSource.PositionSourcePc;
                    break;
            }

            return measurementResult;
        }

        #endregion Measurement Result


        /// <summary>
        ///     Companies, tbl zu Ex
        /// </summary>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ProtoGeoResultModel ToProtoGeoResult(this TableMeasurementResult tblMeasurementResult, Point sourcePoint)
        {
            if (tblMeasurementResult == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoGeoResult)}): {nameof(tblMeasurementResult)}");
            }


            var geoResult = new ProtoGeoResultModel
            {
                Coordinate = new ProtoCoordinate
                {
                    Longitude = (float) tblMeasurementResult.Location.Longitude,
                    Altitude = (float) tblMeasurementResult.Location.Altitude,
                    Latitude = (float) tblMeasurementResult.Location.Latitude
                },
                ID = tblMeasurementResult.Id,
                TimeStamp = tblMeasurementResult.TimeStamp.ConvertToUtcGoogleTimestamp(),
                ValueBit = tblMeasurementResult.Value.Bit ?? false,
                ValueNumber = (float) tblMeasurementResult.Value.Number!,
                ValueText = tblMeasurementResult.Value.Text,
                ValueType = CommonHelper.ConvertValueTypeToProtoValueType(tblMeasurementResult.ValueType),
                Distance = (float) tblMeasurementResult.SpatialPoint.Distance(sourcePoint)
            };

            if (tblMeasurementResult.Value.Binary != null)
            {
                geoResult.ValueBinary = ByteString.CopyFrom(tblMeasurementResult.Value.Binary);
            }

            return geoResult;
        }

        #region Company

        /// <summary>
        ///     Companies, tbl zu Ex
        /// </summary>
        /// <param name="tblCompany"></param>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ProtoCompanyModel ToProtoCompany(this TableCompany tblCompany)
        {
            if (tblCompany == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoCompany)}): {nameof(tblCompany)}");
            }

            var company = new ProtoCompanyModel
            {
                CompanyID = tblCompany.Id,
                Information = new ProtoInformation
                {
                    Name = tblCompany.Information.Name,
                    Description = tblCompany.Information.Description,
                    CreatedDate = tblCompany.Information.CreatedDate.ConvertToUtcGoogleTimestamp(),
                    UpdatedDate = tblCompany.Information.UpdatedDate.ConvertToUtcGoogleTimestamp()
                }
            };
            switch (tblCompany.CompanyType)
            {
                case EnumCompanyTypes.NoCompany:
                    company.CompanyType = ProtoEnumCompanyTypes.CompanyTypeNocompany;
                    break;
                case EnumCompanyTypes.PublicCompany:
                    company.CompanyType = ProtoEnumCompanyTypes.CompanyTypePublic;
                    break;
                case EnumCompanyTypes.Company:
                    company.CompanyType = ProtoEnumCompanyTypes.CompanyTypePrivate;
                    break;
            }

            return company;
        }

        public static ProtoBasicCompanyModel ToProtoBasicCompany(this TableCompany tblCompany)
        {
            if (tblCompany == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoBasicCompany)}): {nameof(tblCompany)}");
            }


            return new ProtoBasicCompanyModel
            {
                Information = new ProtoBasicInformation
                {
                    ID = tblCompany.Id,
                    Name = tblCompany.Information.Name
                }
            };
        }

        #endregion Company

        #region Project

        /// <summary>
        ///     Companies, tblProject zu ExRestProject
        /// </summary>
        /// <param name="tblProject">Projekt aus DB</param>
        /// <exception cref="ArgumentNullException">Wenn tblProject null ist</exception>
        /// <returns></returns>
        public static ProtoProjectModel ToProtoProject(this TableProject tblProject)
        {
            if (tblProject == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoProject)}): {nameof(tblProject)}");
            }

            var project = new ProtoProjectModel
            {
                AdditionalProperties = tblProject.AdditionalProperties,
                CompanyID = tblProject.TblCompanyId,
                Id = tblProject.Id,
                IsPublic = tblProject.IsPublic,
                IsPublished = tblProject.Published,
                PublishedDate = tblProject.PublishedDate.ConvertToUtcGoogleTimestamp(),
                Information = new ProtoInformation
                {
                    Name = tblProject.Information.Name,
                    Description = tblProject.Information.Description,
                    CreatedDate = tblProject.Information.CreatedDate.ConvertToUtcGoogleTimestamp(),
                    UpdatedDate = tblProject.Information.UpdatedDate.ConvertToUtcGoogleTimestamp()
                }
            };


            return project;
        }

        public static ProtoBasicProjectModel ToProtoBasicProject(this TableProject tblProject)
        {
            if (tblProject == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoBasicProject)}): {nameof(tblProject)}");
            }


            return new ProtoBasicProjectModel
            {
                Information = new ProtoBasicInformation
                {
                    ID = tblProject.Id,
                    Name = tblProject.Information.Name
                }
            };
        }

        #endregion Project

        #region Measuremenet Definition

        /// <summary>
        ///     Companies, tblProject zu ExRestProject
        /// </summary>S
        /// <exception cref="ArgumentNullException">Wenn tblProject null ist</exception>
        /// <returns></returns>
        public static ProtoMeasurementDefinitionModel ToProtoMeasurementDefinition(this TableMeasurementDefinition tblMeasurementDefinition)
        {
            if (tblMeasurementDefinition == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoMeasurementDefinition)}): {nameof(tblMeasurementDefinition)}");
            }

            var measurementDefinition = new ProtoMeasurementDefinitionModel
            {
                ID = tblMeasurementDefinition.Id,
                Information = new ProtoInformation
                {
                    CreatedDate = tblMeasurementDefinition.Information.CreatedDate.ConvertToUtcGoogleTimestamp(),
                    Description = tblMeasurementDefinition.Information.Description,
                    Name = tblMeasurementDefinition.Information.Name,
                    UpdatedDate = tblMeasurementDefinition.Information.UpdatedDate.ConvertToUtcGoogleTimestamp()
                },
                AdditionalProperties = tblMeasurementDefinition.AdditionalProperties,
                MeasurementInterval = tblMeasurementDefinition.MeasurementInterval,
                ValueType = CommonHelper.ConvertValueTypeToProtoValueType(tblMeasurementDefinition.ValueType)
            };


            switch (tblMeasurementDefinition.DownstreamType)
            {
                case EnumIotDeviceDownstreamTypes.Arduino:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeArduino;
                    break;
                case EnumIotDeviceDownstreamTypes.Custom:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeCustom;
                    break;
                case EnumIotDeviceDownstreamTypes.DotNet:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeDotnet;
                    break;
                case EnumIotDeviceDownstreamTypes.Esp32:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeEsp32;
                    break;
                case EnumIotDeviceDownstreamTypes.I2C:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeI2C;
                    break;
                case EnumIotDeviceDownstreamTypes.Modbus:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeModbus;
                    break;
                case EnumIotDeviceDownstreamTypes.None:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeNone;
                    break;
                case EnumIotDeviceDownstreamTypes.Pi:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypePi;
                    break;
                case EnumIotDeviceDownstreamTypes.Spi:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeSpi;
                    break;
                case EnumIotDeviceDownstreamTypes.Virtual:
                    measurementDefinition.DownstreamType = ProtoEnumDownstreamTypes.DownstreamTypeVirtual;
                    break;
            }

            return measurementDefinition;
        }

        public static ProtoBasicMeasurementDefinitionModel ToProtoBasicMeasurementDefinition(this TableMeasurementDefinition tblMeasurementDefinition)
        {
            if (tblMeasurementDefinition == null!)
            {
                throw new ArgumentNullException($"[{nameof(ModelConverterExtensions)}]({nameof(ToProtoBasicMeasurementDefinition)}): {nameof(tblMeasurementDefinition)}");
            }


            return new ProtoBasicMeasurementDefinitionModel
            {
                Information = new ProtoBasicInformation
                {
                    ID = tblMeasurementDefinition.Id,
                    Name = tblMeasurementDefinition.Information.Name
                }
            };
        }

        #endregion Measurement Definition
    }
}