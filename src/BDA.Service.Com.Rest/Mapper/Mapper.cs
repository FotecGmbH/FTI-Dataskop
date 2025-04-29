// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Service.Com.Base;
using BDA.Service.Com.Base.Helpers;
using Database.Converter;
using Database.Tables;

namespace BDA.Service.Com.Rest.Mapper
{
    /// <summary>
    ///     Mapper.cs
    /// </summary>
    public static class RestMapper
    {
        #region Measurement Result

        /// <summary>
        ///     Converts the TableMeasurementResult to ExMeasurementResult
        /// </summary>
        /// <param name="tblMeasurementResult">meausrementresult aus DB</param>
        /// <returns></returns>
        public static ExRestMeasurementResult ToExRestMeasurementResult(this TableMeasurementResult tblMeasurementResult)
        {
            if (tblMeasurementResult == null!)
            {
                throw new ArgumentNullException($"[{nameof(Mapper)}]({nameof(ToExRestMeasurementResult)}): {nameof(tblMeasurementResult)}");
            }

            return new ExRestMeasurementResult
            {
                Id = tblMeasurementResult.Id,
                Location = tblMeasurementResult.Location.ToExPosition(),
                Value = CommonMethodsHelper.GetValueOfMeasurementResult(tblMeasurementResult),
                ValueType = tblMeasurementResult.ValueType,
                AdditionalProperties = tblMeasurementResult.AdditionalProperties,
                TimeStamp = tblMeasurementResult.TimeStamp
            };
        }

        #endregion

        #region Company

        /// <summary>
        ///     CompaniesWithUserRigts, tbl zu Ex
        /// </summary>
        /// <param name="tblPermission">Benutzerrecht</param>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ExRestCompanyWithUserRights ToExRestCompanyWithUserRights(this TablePermission tblPermission)
        {
            if (tblPermission == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestCompany)}): {nameof(tblPermission)}");
            }

            var company = new ExRestCompanyWithUserRights
            {
                CompanyType = tblPermission.TblCompany.CompanyType,
                Id = tblPermission.TblCompany.Id,
                Information = tblPermission.TblCompany.Information.ToExInformation(),
                UserRight = tblPermission.UserRight
            };

            return company;
        }

        /// <summary>
        ///     Companies, tbl zu Ex
        /// </summary>
        /// <param name="tblCompany">Firma</param>
        /// <param name="userRight">Benutzerrecht</param>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ExRestCompanyWithUserRights ToExRestCompanyWithUserRights(this TableCompany tblCompany, EnumUserRight userRight)
        {
            if (tblCompany == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestCompanyWithUserRights)}): {nameof(tblCompany)}");
            }

            var company = new ExRestCompanyWithUserRights
            {
                CompanyType = tblCompany.CompanyType,
                Id = tblCompany.Id,
                Information = tblCompany.Information.ToExInformation(),
                UserRight = userRight
            };

            return company;
        }

        /// <summary>
        ///     Companies, tbl zu Ex
        /// </summary>
        /// <param name="tblCompany"></param>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ExRestCompany ToExRestCompany(this TableCompany tblCompany)
        {
            if (tblCompany == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestCompany)}): {nameof(tblCompany)}");
            }

            var company = new ExRestCompany
            {
                CompanyType = tblCompany.CompanyType,
                Id = tblCompany.Id,
                Information = tblCompany.Information.ToExInformation()
            };

            return company;
        }


        /// <summary>
        ///     Converts the TableCompany to ExRestCompany only with basic information
        /// </summary>
        /// <param name="tblCompany"></param>
        /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
        /// <returns></returns>
        public static ExBasicCompany ToExBasicCompany(this TableCompany tblCompany)
        {
            if (tblCompany == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestCompany)}): {nameof(tblCompany)}");
            }

            return new ExBasicCompany
            {
                Information = new ExBasicInformation
                {
                    ID = tblCompany.Id,
                    Name = tblCompany.Information.Name
                }
            };
        }

        #endregion Company

        #region Measurement Definition

        /// <summary>
        ///     Converts the TableMeasurementDefinition to ExRestMeasurmentDefinition only with basic information
        /// </summary>
        /// <param name="tblMeasurementDefinition">Messdefinition aus DB</param>
        /// <exception cref="ArgumentNullException">Wenn tblMeasurementDefinition null ist</exception>
        /// <returns>Konvertierte Messdefinition</returns>
        public static ExBasicMeasurementDefinition ToExBasicMeasurmentDefinition(this TableMeasurementDefinition tblMeasurementDefinition)
        {
            if (tblMeasurementDefinition == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestMeasurmentDefinition)}): {nameof(tblMeasurementDefinition)}");
            }

            return new ExBasicMeasurementDefinition
            {
                Information = new ExBasicInformation
                {
                    ID = tblMeasurementDefinition.Id,
                    Name = tblMeasurementDefinition.Information.Name
                }
            };
        }


        /// <summary>
        ///     Messdefinition, TableMeasurmentDefition zu ExRestMeasurementDefinition
        /// </summary>
        /// <param name="tblMeasurementDefinition">Messdefinition aus DB</param>
        /// <exception cref="ArgumentNullException">Wenn tblMeasurementDefinition null ist</exception>
        /// <returns>Konvertierte Messdefinition</returns>
        public static ExRestMeasurmentDefinition ToExRestMeasurmentDefinition(this TableMeasurementDefinition tblMeasurementDefinition)
        {
            if (tblMeasurementDefinition == null!)
            {
                throw new ArgumentNullException($"[{nameof(RestMapper)}]({nameof(ToExRestMeasurmentDefinition)}): {nameof(tblMeasurementDefinition)}");
            }

            var measurmentDefinition = new ExRestMeasurmentDefinition
            {
                AdditionalProperties = tblMeasurementDefinition.AdditionalProperties,
                Id = tblMeasurementDefinition.Id,
                ValueType = tblMeasurementDefinition.ValueType,
                DownstreamType = tblMeasurementDefinition.DownstreamType,
                Information = tblMeasurementDefinition.Information.ToExInformation(),
                MeasurementInterval = tblMeasurementDefinition.MeasurementInterval
            };

            return measurmentDefinition;
        }

        #endregion Measurement Definition

        #region Project

        /// <summary>
        ///     Companies, tblProject zu ExRestProject
        /// </summary>
        /// <param name="tblProject">Projekt aus DB</param>
        /// <exception cref="ArgumentNullException">Wenn tblProject null ist</exception>
        /// <returns></returns>
        public static ExRestProject ToExRestProject(this TableProject tblProject)
        {
            if (tblProject == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToExRestCompany)}): {nameof(tblProject)}");
            }

            var project = new ExRestProject
            {
                AdditionalProperties = tblProject.AdditionalProperties,
                CompanyId = tblProject.TblCompanyId,
                Id = tblProject.Id,
                Information = tblProject.Information.ToExInformation(),
                IsPublic = tblProject.IsPublic,
                Published = tblProject.Published,
                PublishedDate = tblProject.PublishedDate
            };

            return project;
        }

        /// <summary>
        ///     Converts the TableProject to ExRestProject only with basic information
        /// </summary>
        /// <param name="tblProject">Projekt aus DB</param>
        /// <exception cref="ArgumentNullException">Wenn tblProject null ist</exception>
        /// <returns></returns>
        public static ExBasicProject ToExBasicProject(this TableProject tblProject)
        {
            if (tblProject == null!)
            {
                throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToExRestCompany)}): {nameof(tblProject)}");
            }

            return new ExBasicProject
            {
                Information = new ExBasicInformation
                {
                    ID = tblProject.Id,
                    Name = tblProject.Information.Name
                }
            };
        }

        #endregion Project
    }
}