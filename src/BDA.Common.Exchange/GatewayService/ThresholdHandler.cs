// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Configs.Enums;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Handler fuer Schwellwerte</para>
    ///     Klasse ThresholdHandler. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ThresholdHandler
    {
        private readonly Dictionary<long, List<float>> ValsDic = new Dictionary<long, List<float>>();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public (bool, EnumThresholdType) CheckThreshold(ExGwServiceMeasurementDefinitionConfig mesDef, float value)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (!ValsDic.ContainsKey(mesDef.DbId))
            {
                ValsDic.Add(mesDef.DbId, new List<float>());
            }

            var currDic = ValsDic[mesDef.DbId];

            if (currDic.Count > 3)
            {
                currDic.RemoveAt(0);
            }

            if (!mesDef.IfThresholdDelta)
            {
                currDic.Clear(); // if no delta threshold needed, we dont need to store values 
            }

            if (mesDef.IfThresholdDelta && currDic.Any())
            {
                if (Math.Abs(value - currDic.Last()) > mesDef.ThresholdDeltaValue)
                {
                    currDic.Add(value);
                    return (true, EnumThresholdType.Delta);
                }
            }

            currDic.Add(value);

            if (mesDef.IfThresholdExceed)
            {
                if (value > mesDef.ThresholdExceedValue)
                {
                    return (true, EnumThresholdType.Exceed);
                }
            }

            if (mesDef.IfThresholdFallBelow)
            {
                if (value < mesDef.ThresholdFallBelowValue)
                {
                    return (true, EnumThresholdType.FallBelow);
                }
            }

            return (false, EnumThresholdType.None);
        }
    }
}