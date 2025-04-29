// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Enum;
using Database;
using Database.Common;
using Database.Tables;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BDA.Common.DataImporter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse WasserstandNoeImporter. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class WasserstandNoeImporter
    {
        public static Dictionary<(string, string), List<TableMeasurementResult>> ConvertToTableMeasurementResult(Dictionary<(string, string), List<WasserstandNoeDataPoint>> dataPoints)
        {
            var tableMeasurementResults = new Dictionary<(string, string), List<TableMeasurementResult>>();

            foreach (var pairDataPoint in dataPoints)
            {
                tableMeasurementResults.Add(pairDataPoint.Key, new List<TableMeasurementResult>());
                foreach (var dataPoint in pairDataPoint.Value)
                {
                    var tableMeasurementResult = new TableMeasurementResult
                    {
                        TimeStamp = DateTime.Parse(dataPoint.date),
                        Location = new DbPosition
                        {
                            Longitude = dataPoint.location.@long,
                            Latitude = dataPoint.location.lat
                        },
                        SpatialPoint = new Point(dataPoint.location.@long, dataPoint.location.lat) {SRID = 4326},
                        ValueType = EnumValueTypes.Number
                    };

                    if (!double.TryParse(dataPoint.value, NumberStyles.Any, CultureInfo.InvariantCulture, out var valuee))
                    {
                        continue;
                    }

                    tableMeasurementResult.Value = new DbValue {Number = valuee};

                    //dataPoint.Name;
                    tableMeasurementResults[pairDataPoint.Key].Add(tableMeasurementResult);
                }
            }

            return tableMeasurementResults;
        }

        public static void ImportDataToDatabase(Dictionary<(string, string), List<TableMeasurementResult>> deviceValues, long gatewayId, bool withCreatingMeasurementDefinitions)
        {
            var db = new Db();

            var counter = 0;


            var gateway = db.TblGateways.FirstOrDefault(gw => gw.Id == gatewayId);

            if (gateway == null)
            {
                Console.WriteLine("gateway not found");
                return;
            }

            foreach (var deviceValue in deviceValues)
            {
                if (deviceValue.Value.Count == 0)
                {
                    continue;
                }

                var devices = db.TblIotDevices.Include(idev => idev.TblMeasurementDefinitions).ThenInclude(md => md.Information);

                var device = devices.FirstOrDefault(dev => dev.Information.Name == deviceValue.Key.Item1);

                if (device == null)
                {
                    var newDevice = new TableIotDevice
                    {
                        Information = new DbInformation
                        {
                            CreatedDate = DateTime.Now,
                            Name = deviceValue.Key.Item1
                        },
                        FallbackPosition = deviceValue.Value.FirstOrDefault()?.Location!,
                        Upstream = EnumIotDeviceUpstreamTypes.Tcp, // wird nicht benötigt
                        TransmissionType = EnumTransmission.Elapsedtime,
                        Plattform = EnumIotDevicePlattforms.Esp32,
                        TblGatewayId = gateway.Id
                    };
                    db.TblIotDevices.Add(newDevice);

                    db.SaveChanges();

                    devices = db.TblIotDevices.Include(idev => idev.TblMeasurementDefinitions).ThenInclude(md => md.Information);

                    device = devices.FirstOrDefault(dev => dev.Information.Name == deviceValue.Key.Item1);
                }

                foreach (var tableMeasurementResult in deviceValue.Value)
                {
                    if (withCreatingMeasurementDefinitions)
                    {
                        // ReSharper disable once SimplifyLinqExpressionUseAll
                        if (!device!.TblMeasurementDefinitions.Any(md => md.Information.Name == deviceValue.Key.Item2))
                        {
                            db.TblMeasurementDefinitions.Add(new TableMeasurementDefinition
                            {
                                Information = new DbInformation
                                {
                                    CreatedDate = DateTime.Now,
                                    Name = deviceValue.Key.Item2,
                                    Description = deviceValue.Key.Item2
                                },
                                ValueType = tableMeasurementResult.ValueType,
                                TblIotDeviceId = device.Id
                            });
                            db.SaveChanges();
                        }
                    }

                    var md = db.TblMeasurementDefinitions.Include(md => md.Information).FirstOrDefault(md => md.Information.Name == deviceValue.Key.Item2 && md.TblIotDeviceId == device!.Id);

                    if (md == null)
                    {
                        Console.WriteLine("MeasurementDefinition not found");
                        continue;
                    }

                    tableMeasurementResult.TblMeasurementDefinitionId = md.Id;

                    var mr = db.TblMeasurementResults.AsNoTracking().Any(mr => mr.TblMeasurementDefinitionId == tableMeasurementResult.TblMeasurementDefinitionId && mr.TimeStamp.Equals(tableMeasurementResult.TimeStamp));

                    if (!mr) // wenn es noch keinen Wert für diese Definition und Zeit gibt, dann erstelle das MeasurementResult
                    {
                        counter++;
                        db.TblMeasurementResults.Add(tableMeasurementResult);

                        if (counter > 100)
                        {
                            db.SaveChanges();
                            counter = 0;
                        }
                    }
                    else
                    {
                        
                    }
                }
            }


            db.SaveChanges();
        }
    }
}