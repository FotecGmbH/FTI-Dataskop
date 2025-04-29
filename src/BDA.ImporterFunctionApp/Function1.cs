// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.DataImporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace BDA.ImporterFunctionApp
{
    public class Function1
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public void Run([TimerTrigger("0 */6 * * * *")] TimerInfo myTimer)
        {
            var t = WasserstandNoeImporterFromApi.GetDataFromApi(DateTime.Now.AddDays(-2));
            t.Wait();

            var resss = WasserstandNoeImporter.ConvertToTableMeasurementResult(t.Result);

            WasserstandNoeImporter.ImportDataToDatabase(resss, 14, true);

            var dataset1 = "klima-v1-1h";

            var parametersInput1 = "FFX,D6X,WSZ,WSX,WSD,VVX,TTX,TDX,SUX,RSX,RSD,PPX,LT2,GSX,GSW";

            var stationIds1 = "7604";
            var stationIds2 = "905";
            long iotDeviceId1 = 255;
            long iotDeviceId2 = 389;
            var startInput = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm");
            var endInput = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
            GeosphereImporter.Import(dataset1, parametersInput1, stationIds1, startInput, endInput, iotDeviceId1, false);
            GeosphereImporter.Import(dataset1, parametersInput1, stationIds2, startInput, endInput, iotDeviceId2, false);
            Thread.Sleep(3000);

            var dataset2 = "klima-v1-10min";
            var parametersInput2 = "FFX,HSX,HSR,GSX";
            GeosphereImporter.Import(dataset2, parametersInput2, stationIds1, startInput, endInput, iotDeviceId1, false);
            GeosphereImporter.Import(dataset2, parametersInput2, stationIds2, startInput, endInput, iotDeviceId2, false);
            Thread.Sleep(3000);

            var dataset3 = "klima-v2-10min";
            var parametersInput3 = "FFX";
            GeosphereImporter.Import(dataset3, parametersInput3, stationIds1, startInput, endInput, iotDeviceId1, false);
            GeosphereImporter.Import(dataset3, parametersInput3, stationIds2, startInput, endInput, iotDeviceId2, false);

            Thread.Sleep(3000);

            var dataset4 = "klima-v2-1h";
            var parametersInput4 = "tl";
            GeosphereImporter.Import(dataset4, parametersInput4, stationIds1, startInput, endInput, iotDeviceId1, false);
            GeosphereImporter.Import(dataset4, parametersInput4, stationIds2, startInput, endInput, iotDeviceId2, false);
        }
    }
}