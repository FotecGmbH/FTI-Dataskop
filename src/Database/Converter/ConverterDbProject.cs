// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Model.ConfigApp;
using Database.Tables;

namespace Database.Converter;

/// <summary>
///     <para>Db Project konvertieren</para>
///     Klasse ConverterDbProject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbProject
{
    /// <summary>
    ///     TableProject zu ExProject
    /// </summary>
    /// <param name="tblProject"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Wenn tblProject null ist.</exception>
    public static ExProject ToExProject(this TableProject tblProject)
    {
        if (tblProject == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbProject)}]({nameof(ToExProject)}): {nameof(tblProject)} is null");
        }

        var exProject = new ExProject
        {
            AdditionalProperties = tblProject.AdditionalProperties,
            CompanyId = tblProject.TblCompanyId,
            Information = tblProject.Information.ToExInformation(),
            IsPublic = tblProject.IsPublic,
            Published = tblProject.Published,
            PublishedDate = tblProject.PublishedDate,
            MeasurementDefinitions = tblProject.TblMeasurementDefinitionToProjectAssignments.Where(w => w.TblMeasurementDefinitionId.HasValue).Select(s => s.TblMeasurementDefinitionId!.Value).ToList()
        };
        return exProject;
    }

    /// <summary>
    ///     TableProject nach ExProject
    /// </summary>
    /// <param name="p"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<TableMeasurementDefinitionToProjectAssignment> ToTableProject(this ExProject p, TableProject t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbProject)}]({nameof(ToTableProject)}): {nameof(t)}");
        }

        if (p == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbProject)}]({nameof(ToTableProject)}): {nameof(p)}");
        }

        var itemsToRemove = new List<TableMeasurementDefinitionToProjectAssignment>();

        t.AdditionalProperties = p.AdditionalProperties;
        t.TblCompanyId = p.CompanyId;
        p.Information.ToDbInformation(t.Information);
        t.IsPublic = p.IsPublic;
        t.Published = p.Published;
        t.PublishedDate = p.PublishedDate;

        foreach (var d in p.MeasurementDefinitions)
        {
            if (t.TblMeasurementDefinitionToProjectAssignments.All(a => a.TblMeasurementDefinitionId != d))
            {
                t.TblMeasurementDefinitionToProjectAssignments.Add(new TableMeasurementDefinitionToProjectAssignment
                {
                    TblMeasurementDefinitionId = d,
                    TblProjcts = t
                });
            }
        }

        //Gelöschte Elemente
        foreach (var l in t.TblMeasurementDefinitionToProjectAssignments.ToList())
        {
            if (l.TblMeasurementDefinitionId.HasValue)
            {
                if (!p.MeasurementDefinitions.Contains(l.TblMeasurementDefinitionId.Value))
                {
                    itemsToRemove.Add(l);
                }
            }
        }

        return itemsToRemove;
    }
}