// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbCompany
{
    /// <summary>
    ///     Companies, tbl zu Ex
    /// </summary>
    /// <param name="tblCompany"></param>
    /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
    /// <returns></returns>
    public static ExCompany ToExCompany(this TableCompany tblCompany)
    {
        if (tblCompany == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToExCompany)}): {nameof(tblCompany)}");
        }

        var company = new ExCompany
        {
            CompanyType = tblCompany.CompanyType,
            Information = tblCompany.Information.ToExInformation()
        };

        //if (tblCompany.TblProjects != null!)
        //{
        //    foreach (var project in tblCompany.TblProjects)
        //    {
        //        company.Projects.Add(project.ToExProject());
        //    }
        //}

        //if (tblCompany.TblGateways != null!)
        //{
        //    foreach (var tblGateway in tblCompany.TblGateways)
        //    {
        //        company.Gateways.Add(tblGateway.ToExGateway());
        //    }
        //}

        return company;
    }
}