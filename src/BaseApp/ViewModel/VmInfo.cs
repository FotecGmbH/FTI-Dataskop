// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Apps.Attributes;
using Exchange;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>View Infos</para>
    ///     Klasse VmInfo. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewInfo")]
    public class VmInfo : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmInfo.DesignInstance}"
        /// </summary>
        public static VmInfo DesignInstance = new VmInfo();

        /// <summary>
        ///     VmInfo
        /// </summary>
        public VmInfo() : base("Infos", subTitle: "Allgemeine App Infos")
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     App Settings
        /// </summary>
        public AppSettings CurrentSettings => AppSettings.Current();

        #endregion
    }
}