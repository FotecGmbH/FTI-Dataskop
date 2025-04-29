// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BDA.Common.Exchange.Model;
using Biss.Apps.Base.Connectivity.Model;
using Biss.Apps.Connectivity.Sa;


namespace BaseApp.Connectivity
{
    /// <summary>
    ///     <para>Projektbezogene REST Calls</para>
    ///     Klasse SaProjectBase. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class SaProjectBase : RestAccessBase
    {
        /// <summary>
        ///     SA REST Test Funktion
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Task<ResultData<int>> AddAsGet(int a, int b)
        {
            return Wap.Get<int>("Add", new List<string> {a.ToString(new NumberFormatInfo()), b.ToString(new NumberFormatInfo())});
        }

        /// <summary>
        ///     SA REST Test Funktion
        /// </summary>
        /// <param name="demo"></param>
        /// <returns></returns>
        public Task<ResultData<ExDemoData>> DemoDataAsPost(ExDemoData demo)
        {
            return Wap.Post<ExDemoData>("DemoData", demo);
        }
    }
}