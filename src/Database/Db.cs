// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Configs.Downstreams.Custom;
using BDA.Common.Exchange.Enum;
using Biss.Apps.Base;
using Biss.Common;
using Biss.Log.Producer;
using Biss.Serialize;
using Database.Common;
using Database.Tables;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace Database
{
    /// <summary>
    ///     <para>Projektweite Datenbank - Entity Framework Core</para>
    ///     Klasse Db. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class Db : DbContext
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool UsePostGres = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        ///     Db Context initialisieren - für SQL Server
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder != null!)
            {
                if (UsePostGres)
                {
                    optionsBuilder.UseNpgsql(WebConstants.ConnectionStringPostgres, x => x.UseNetTopologySuite().MigrationsAssembly("Database.Postgres.Migration"));
                }
                else
                {
                    optionsBuilder.UseSqlServer(WebConstants.ConnectionString, x => x.UseNetTopologySuite());
                    //optionsBuilder.LogTo(s=>Debug.WriteLine(s));
                }
            }
        }

        #region Erzeugen, Neu Erzeugen, Löschen

        /// <summary>
        ///     Datenbank löschen
        /// </summary>
        /// <returns>Erfolg</returns>
        public static bool DeleteDatabase()
        {
            Logging.Log.LogTrace($"[{nameof(Db)}]({nameof(DeleteDatabase)}): Delete Database");
            using var db = new Db();
            try
            {
                db.Database.EnsureDeleted();
            }
            catch (Exception e)
            {
                Logging.Log.LogWarning($"[{nameof(Db)}]({nameof(DeleteDatabase)}): Error deleting database: {e}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Datenbank wird bei Aufruf erzugt bzw. gelöscht und neu erzeugt
        /// </summary>
        /// <returns>Erfolg</returns>
        public static bool CreateAndFillUp()
        {
            using var db = new Db();

            var createDb = true;

            if (createDb)
            {
                //DB anlegen

                var connstrBldr = new SqlConnectionStringBuilder(db.Database.GetConnectionString())
                {
                    InitialCatalog = "master"
                };

                Logging.Log.LogTrace($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): Create Database");

                using (var conn = new SqlConnection(connstrBldr.ConnectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 360;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    cmd.CommandText = $"CREATE DATABASE [{db.Database.GetDbConnection().Database}] (EDITION = 'basic')";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    cmd.ExecuteNonQuery();
                }

                Logging.Log.LogTrace($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): Create tables");

                db.Database.EnsureCreated();

                Logging.Log.LogTrace($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): Initially fill up the database");

                #region TableUser

                var admin = new TableUser
                {
                    LoginName = "biss@fotec.at",
                    FirstName = "Biss",
                    LastName = "Admin",
                    Locked = false,
                    PasswordHash = AppCrypt.CumputeHash("biss"),
                    DefaultLanguage = "de",
                    LoginConfirmed = true,
                    IsAdmin = true,
                    AgbVersion = "1.0.0",
                    CreatedAtUtc = DateTime.UtcNow,
                    RefreshToken = AppCrypt.GeneratePassword(),
                    JwtToken = AppCrypt.GeneratePassword()
                };
                db.TblUsers.Add(admin);

                #endregion

                #region TableSetting

                var settings = EnumUtil.GetValues<EnumDbSettings>();
                foreach (var setting in settings)
                {
                    var v = new Version(1, 0);
                    switch (setting)
                    {
                        case EnumDbSettings.Agb:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                            break;
                        case EnumDbSettings.CurrentAppVersion:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                            break;
                        case EnumDbSettings.MinAppVersion:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                            break;
                        case EnumDbSettings.CommonMessage:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = ""});
                            break;
                        case EnumDbSettings.ConfigAppWindows:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = "https://dataskopblob.blob.core.windows.net/dataskopblobbeta/ConfigAppWindows.zip"});
                            break;
                        case EnumDbSettings.GatewayAppWindows:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = "https://dataskopblob.blob.core.windows.net/dataskopblobbeta/GatewayAppWindows.zip"});
                            break;
                        case EnumDbSettings.GatewayAppLinux:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = ""});
                            break;
                        case EnumDbSettings.SensorTemplateFipiTtn:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = "https://dataskopblob.blob.core.windows.net/dataskopblobbeta/SensorTemplateFipiTtn.zip"});
                            break;
                        case EnumDbSettings.InterfaceGrpc:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = "https://dataskopblob.blob.core.windows.net/dataskopblobbeta/ProtoFiles.zip"});
                            break;
                        case EnumDbSettings.ConfigAppWeb:
                            db.TblSettings.Add(new TableSetting {Key = setting, Value = "https://dataskopblazorapp.azurewebsites.net/"});
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                #endregion

                #region TableCompany

                var noCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "NoCompany",
                        Description = "Neue Gateways tauchen hier auf und können dann einer Firma zugewiesen werden",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.NoCompany
                };
                var publicCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "Öffentliche Firma",
                        Description = "Alle Daten dieser Firma sind öffentlich",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.PublicCompany
                };

                var fotecCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "FOTEC",
                        Description = "FOTEC Forschungs- und Technologietransfer GmbH",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                var fhspCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "FH St. Pölten",
                        Description = "Fachhochschule St. Pölten GmbH",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                var fhKrems = new TableCompany
                {
                    Information = new()
                    {
                        Name = "IMC FH Krems",
                        Description = "IMC FH Krems GmbH",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                var uniDuk = new TableCompany
                {
                    Information = new()
                    {
                        Name = "DUK",
                        Description = "Universität für Weiterbildung",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                var ixLabCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "IXLab",
                        Description = "IXLab",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                var wnCompany = new TableCompany
                {
                    Information = new()
                    {
                        Name = "Wiener Neustadt",
                        Description = "Wiener Neustadt",
                        CreatedDate = DateTime.UtcNow
                    },
                    CompanyType = EnumCompanyTypes.Company
                };
                db.TblCompanies.Add(noCompany);
                db.TblCompanies.Add(publicCompany);
                db.TblCompanies.Add(fotecCompany);
                db.TblCompanies.Add(fhspCompany);
                db.TblCompanies.Add(fhKrems);
                db.TblCompanies.Add(uniDuk);
                db.TblCompanies.Add(ixLabCompany);
                db.TblCompanies.Add(wnCompany);

                #endregion

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Logging.Log.LogWarning($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): Error creating database: {e}");
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting
        ///     model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a model is explicitly set on the options for this context (via
        ///         <see
        ///             cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />
        ///         )
        ///         then this method will not be run. However, it will still run when creating a compiled model.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more
        ///         information and
        ///         examples.
        ///     </para>
        /// </remarks>
        /// <param name="builder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TableMeasurementResult>().ToTable(tb => tb.HasTrigger("measurementresult_insert_trigger"));
        }

        /// <summary>
        ///     Initialisiert Db Triggers
        /// </summary>
        public static void InitializeDbTriggers()
        {
            using var db = new Db();

            db.Database.ExecuteSqlRaw(""" 
                CREATE OR ALTER TRIGGER [dbo].[measurementresult_insert_trigger]
                ON [dbo].[MeasurementResult]
                AFTER  INSERT
                AS
                    BEGIN
                SET NOCOUNT ON
                    UPDATE MDef
                SET    MDef.tbllatestmeasurementresultid = MRes.id
                FROM   measurementdefinition MDef
                INNER JOIN inserted MRes
                ON MRes.tblmeasurementdefinitionid = MDef.id
                END 
                """);
        }

        /// <summary>
        ///     Initialisiert das Devicerepository
        /// </summary>
        public static void SetupTemplates()
        {
            using var db = new Db();

            foreach (var iotdevice in db.TblIotDevices)
            {
                iotdevice.TblDataconverter = null;
                iotdevice.TblDataconverterId = null;
            }

            db.TblDataconverters.RemoveRange(db.TblDataconverters);
            db.TblMeasurementDefinitionTemplates.RemoveRange(db.TblMeasurementDefinitionTemplates);

            db.SaveChanges();

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "LHT65") is null)
            {
                var tempTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temperatur", Description = ""}, ValueType = EnumValueTypes.Number};
                var humidityTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Feuchtigkeit", Description = ""}, ValueType = EnumValueTypes.Number};
                var batteryTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Batteriespannung", Description = ""}, ValueType = EnumValueTypes.Number};
                var parserLHT65 = new TableDataconverter
                {
                    Displayname = "LHT65",
                    Description = "Temperaturesensor",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        tempTemplate,
                        humidityTemplate,
                        batteryTemplate
                    },
                    CodeSnippet = "\tresults[0].MeasurementNumber = ((input[2]<<24>>16 | input[3])/100.0);\r\n\tresults[1].MeasurementNumber = ((input[4]<<8 | input[5])/10.0);\r\n\tresults[2].MeasurementNumber = ((input[0]<<8 | input[1]) & 0x3FFF)/1000.0;\r\n"
                };
                db.TblDataconverters.Add(parserLHT65);
            }

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "LDS01") is null)
            {
                var modeTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Modus", Description = "Der Modus in dem sich der Sensor befindet (1, 2, 3)."}, ValueType = EnumValueTypes.Number};
                var batteryTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Batteriespannung", Description = "Die Spannung der Sensorstromquelle."}, ValueType = EnumValueTypes.Number};
                var doorOpenTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "TürOffen", Description = "Wert der angibt ob aktuell die Tür geöffnet ist."}, ValueType = EnumValueTypes.Bit};
                var doorOpenTimesTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "AnzahlÖffnungen", Description = "Wie oft die Türe geöffnet wurde."}, ValueType = EnumValueTypes.Number};
                var doorOpenDurationTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ZeitraumTürOffen(m)", Description = "Wie lange war die Türe insgesamt offen. In Minuten."}, ValueType = EnumValueTypes.Number};
                var waterLeakTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "WasserLeck", Description = "Ob derzeit ein Wasserleck detektiert wird."}, ValueType = EnumValueTypes.Bit};
                var waterLeakTimesTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "AnzahlLeckerkennungen", Description = "Wie oft wurde ein Wasserleck erkannt."}, ValueType = EnumValueTypes.Number};
                var waterLeakDurationTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ZeitraumLeckErkennung(m)", Description = "Wie lange wurde insgesamt Wasser detektiert. In Minuten."}, ValueType = EnumValueTypes.Number};


                var parserLDS01 = new TableDataconverter
                {
                    Displayname = "LDS01",
                    Description = "Türsensor",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        modeTemplate,
                        batteryTemplate,
                        doorOpenTemplate,
                        doorOpenTimesTemplate,
                        doorOpenDurationTemplate,
                        waterLeakTemplate,
                        waterLeakTimesTemplate,
                        waterLeakDurationTemplate,
                    },
                    CodeSnippet = "var value = (input[0] << 8 | input[1]) & 0x3FFF;\r\n        var bat = value / 1000.0;//Battery,units:V\r\n\r\n        var door_open_status = (input[0] & 0x80) > 1;\r\n        var water_leak_status = (input[0] & 0x40) > 1;\r\n\r\n        var mod = input[2];\r\n\r\n        //init\r\n        results[0].MeasurementNumber = mod;\r\n        results[1].MeasurementNumber = bat;\r\n        results[2].MeasurementBool = false;\r\n        results[3].MeasurementNumber = -1;\r\n        results[4].MeasurementNumber = -1;\r\n        results[5].MeasurementBool = false;\r\n        results[6].MeasurementNumber = -1;\r\n        results[7].MeasurementNumber = -1;\r\n\r\n\r\n        if (mod == 1)\r\n        {\r\n            var open_times = input[3] << 16 | input[4] << 8 | input[5];\r\n            var open_duration = input[6] << 16 | input[7] << 8 | input[8];//units:min\r\n            results[2].MeasurementBool = door_open_status;\r\n            results[3].MeasurementNumber = open_times;\r\n            results[4].MeasurementNumber = open_duration;\r\n        }\r\n        else if (mod == 2)\r\n        {\r\n            var leak_times = input[3] << 16 | input[4] << 8 | input[5];\r\n            var leak_duration = input[6] << 16 | input[7] << 8 | input[8];//units:min\r\n            results[5].MeasurementBool = water_leak_status;\r\n            results[6].MeasurementNumber = leak_times;\r\n            results[7].MeasurementNumber = leak_duration;\r\n        }\r\n        else if (mod == 3)\r\n        {\r\n            results[2].MeasurementBool = door_open_status;\r\n            results[5].MeasurementBool = water_leak_status; }"
                };
                db.TblDataconverters.Add(parserLDS01);
            }

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "LDDS75") is null)
            {
                var batteryTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Batteriespannung", Description = "Die Spannung der Sensorstromquelle."}, ValueType = EnumValueTypes.Number};
                var distanceTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Abstand(mm)", Description = "Der gemessene Abstand in mm."}, ValueType = EnumValueTypes.Number};
                var errorTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Fehlermeldung", Description = "Falls vorhanden wird hier eine Fehlermeldung ausgegeben."}, ValueType = EnumValueTypes.Text};

                var parserLDDS75 = new TableDataconverter
                {
                    Displayname = "LDDS75",
                    Description = "Abstandssensor",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        batteryTemplate,
                        distanceTemplate,
                        errorTemplate
                    },
                    CodeSnippet = "var value = (input[0] << 8 | input[1]) & 0x3FFF;\r\n        results[0].MeasurementNumber = value / 1000.0;\r\n\r\n        value = input[2] << 8 | input[3];\r\n        results[1].MeasurementNumber = value;\r\n\r\n\r\n        results[2].MeasurementText = \"Kein Fehler\";\r\n        if (value == 0)\r\n            results[2].MeasurementText = \"No Sensor\";\r\n        else if (value == 20)\r\n            results[2].MeasurementText = \"Invalid Reading\";"
                };
                db.TblDataconverters.Add(parserLDDS75);
            }

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "Essecca Sensor") is null)
            {
                var dio1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_1", Description = ""}, ValueType = EnumValueTypes.Number};
                var dio2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_2", Description = ""}, ValueType = EnumValueTypes.Number};
                var dio3Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_3", Description = ""}, ValueType = EnumValueTypes.Number};
                var dio4Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_4", Description = ""}, ValueType = EnumValueTypes.Number};
                var vibrationTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Vibration", Description = "SW420 Sensor"}, ValueType = EnumValueTypes.Number};
                var adxxTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX X", Description = "ADXL345 Sensor X value"}, ValueType = EnumValueTypes.Number};
                var adxyTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Y", Description = "ADXL345 Sensor Y value"}, ValueType = EnumValueTypes.Number};
                var adxzTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Z", Description = "ADXL345 Sensor Z value"}, ValueType = EnumValueTypes.Number};
                var temp1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 1", Description = "GY-21 HTU21 Sensor temperature 1 value"}, ValueType = EnumValueTypes.Number};
                var temp2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 2", Description = "NTC Sensor temperature 2 value"}, ValueType = EnumValueTypes.Number};
                var current1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Current 1", Description = "ACS712 Sensor current 1 value"}, ValueType = EnumValueTypes.Number};
                var current2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Current 2", Description = "ACS712 Sensor current 1 value"}, ValueType = EnumValueTypes.Number};
                var voltage1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Voltage 1", Description = "voltage 1 value"}, ValueType = EnumValueTypes.Number};
                var voltage2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Voltage 2", Description = "voltage 2 value"}, ValueType = EnumValueTypes.Number};


                var additionalConfigs = new List<GcDownstreamCustom>();
                for (byte i = 0; i < 14; i++)
                {
                    additionalConfigs.Add(new GcDownstreamCustom
                    {
                        ValueType = EnumValueTypes.Number,
                        StateMachineId = (byte) (i + 1)
                    });
                }

                var parserCustom = new TableDataconverter
                {
                    Displayname = "Essecca Sensor",
                    Description = "Essecca Sensor",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        dio1Template,
                        dio2Template,
                        dio3Template,
                        dio4Template,
                        vibrationTemplate,
                        adxxTemplate,
                        adxyTemplate,
                        adxzTemplate,
                        temp1Template,
                        temp2Template,
                        current1Template,
                        current2Template,
                        voltage1Template,
                        voltage2Template
                    },
                    CodeSnippet = additionalConfigs.ToJson()
                };
                db.TblDataconverters.Add(parserCustom);
            }

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "Funktüre XS4 One") is null)
            {
                var temp1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 1", Description = "GY-21 HTU21 Sensor temperature 1 value"}, ValueType = EnumValueTypes.Number};
                var temp2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 2", Description = "NTC Sensor temperature 2 value"}, ValueType = EnumValueTypes.Number};
                var vibrationTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Vibration", Description = "SW420 Sensor"}, ValueType = EnumValueTypes.Number};
                var adxxTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX X", Description = "ADXL345 Sensor X value"}, ValueType = EnumValueTypes.Number};
                var adxyTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Y", Description = "ADXL345 Sensor Y value"}, ValueType = EnumValueTypes.Number};
                var adxzTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Z", Description = "ADXL345 Sensor Z value"}, ValueType = EnumValueTypes.Number};
                var current1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Current 1", Description = "ACS712 Sensor current 1 value"}, ValueType = EnumValueTypes.Number};
                var voltage1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Voltage 1", Description = "voltage 1 value"}, ValueType = EnumValueTypes.Number};
                var dio1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_1", Description = ""}, ValueType = EnumValueTypes.Number};


                var customCodes = new byte[] {9, 10, 5, 6, 7, 8, 11, 13, 1};

                var additionalConfigs = new List<GcDownstreamCustom>();
                foreach (var by in customCodes)
                {
                    additionalConfigs.Add(new GcDownstreamCustom
                    {
                        ValueType = EnumValueTypes.Number,
                        StateMachineId = by
                    });
                }

                var parserCustom = new TableDataconverter
                {
                    Displayname = "Funktüre XS4 One",
                    Description = "Funktüre XS4 One",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        temp1Template,
                        temp2Template,
                        vibrationTemplate,
                        adxxTemplate,
                        adxyTemplate,
                        adxzTemplate,
                        current1Template,
                        voltage1Template,
                        dio1Template
                    },
                    CodeSnippet = additionalConfigs.ToJson()
                };
                db.TblDataconverters.Add(parserCustom);
            }

            if (db.TblDataconverters.FirstOrDefault(conv => conv.Displayname == "verkabelte Türe") is null)
            {
                var temp1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 1", Description = "GY-21 HTU21 Sensor temperature 1 value"}, ValueType = EnumValueTypes.Number};
                var temp2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Temp 2", Description = "NTC Sensor temperature 2 value"}, ValueType = EnumValueTypes.Number};
                var vibrationTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Vibration", Description = "SW420 Sensor"}, ValueType = EnumValueTypes.Number};
                var adxxTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX X", Description = "ADXL345 Sensor X value"}, ValueType = EnumValueTypes.Number};
                var adxyTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Y", Description = "ADXL345 Sensor Y value"}, ValueType = EnumValueTypes.Number};
                var adxzTemplate = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "ADX Z", Description = "ADXL345 Sensor Z value"}, ValueType = EnumValueTypes.Number};
                var current1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Current 1", Description = "ACS712 Sensor current 1 value"}, ValueType = EnumValueTypes.Number};
                var voltage1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "Voltage 1", Description = "voltage 1 value"}, ValueType = EnumValueTypes.Number};
                var dio1Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_1", Description = ""}, ValueType = EnumValueTypes.Number};
                var dio2Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_2", Description = ""}, ValueType = EnumValueTypes.Number};
                var dio3Template = new TableMeasurementDefinitionTemplate {Information = new DbInformation {CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, Name = "DIO_IN_3", Description = ""}, ValueType = EnumValueTypes.Number};


                var customCodes = new byte[] {9, 10, 5, 6, 7, 8, 11, 15, 1, 2, 3};

                var additionalConfigs = new List<GcDownstreamCustom>();
                foreach (var by in customCodes)
                {
                    additionalConfigs.Add(new GcDownstreamCustom
                    {
                        ValueType = EnumValueTypes.Number,
                        StateMachineId = by
                    });
                }

                var parserCustom = new TableDataconverter
                {
                    Displayname = "verkabelte Türe",
                    Description = "verkabelte Türe",
                    Templates = new List<TableMeasurementDefinitionTemplate>
                    {
                        temp1Template,
                        temp2Template,
                        vibrationTemplate,
                        adxxTemplate,
                        adxyTemplate,
                        adxzTemplate,
                        current1Template,
                        voltage1Template,
                        dio1Template,
                        dio2Template,
                        dio3Template
                    },
                    CodeSnippet = additionalConfigs.ToJson()
                };
                db.TblDataconverters.Add(parserCustom);
            }

            db.SaveChanges();
        }

        #endregion

        #region Tabellen

        /// <summary>
        ///     Tabelle AccessToken
        /// </summary>
        public virtual DbSet<TableAccessToken> TblAccessToken { get; set; } = null!;

        /// <summary>
        ///     Tabelle Firma
        /// </summary>
        public virtual DbSet<TableCompany> TblCompanies { get; set; } = null!;

        /// <summary>
        ///     Tabelle Firmenkonfiguration
        /// </summary>
        public virtual DbSet<TableCompanyGlobalConfig> TblCompanyGlobalConfigs { get; set; } = null!;

        /// <summary>
        ///     Tabelle Gateway
        /// </summary>
        public virtual DbSet<TableGateway> TblGateways { get; set; } = null!;

        /// <summary>
        ///     Tabelle IoT Geräte
        /// </summary>
        public virtual DbSet<TableIotDevice> TblIotDevices { get; set; } = null!;

        /// <summary>
        ///     Tabelle mit allen Parsern
        /// </summary>
        public virtual DbSet<TableDataconverter> TblDataconverters { get; set; } = null!;

        /// <summary>
        ///     Tabelle mit Measurementdefinition Templates
        /// </summary>
        public virtual DbSet<TableMeasurementDefinitionTemplate> TblMeasurementDefinitionTemplates { get; set; } = null!;

        /// <summary>
        ///     Tabelle Messungen
        /// </summary>
        public virtual DbSet<TableMeasurementResult> TblMeasurementResults { get; set; } = null!;

        /// <summary>
        ///     Tabelle Berechtigung
        /// </summary>
        public virtual DbSet<TablePermission> TblPermissions { get; set; } = null!;

        /// <summary>
        ///     Tabelle Projekte
        /// </summary>
        public virtual DbSet<TableProject> TblProjects { get; set; } = null!;

        /// <summary>
        ///     Tabelle Sensoren
        /// </summary>
        public virtual DbSet<TableMeasurementDefinition> TblMeasurementDefinitions { get; set; } = null!;

        /// <summary>
        ///     Tabelle Zurodnung Projekt zu Messdefinition
        /// </summary>
        public virtual DbSet<TableMeasurementDefinitionToProjectAssignment> TblMeasurementDefinitionToProjectAssignments { get; set; } = null!;

        /// <summary>
        ///     Tabelle User
        /// </summary>
        public virtual DbSet<TableUser> TblUsers { get; set; } = null!;

        /// <summary>
        ///     Tabelle Settings
        /// </summary>
        public virtual DbSet<TableSetting> TblSettings { get; set; } = null!;

        /// <summary>
        ///     Tabelle Devices
        /// </summary>
        public virtual DbSet<TableDevice> TblDevices { get; set; } = null!;

        /// <summary>
        ///     Tabelle Files z.B. Userbild
        /// </summary>
        public virtual DbSet<TableFile> TblFiles { get; set; } = null!;

        /// <summary>
        ///     Tabelle Benachrichtigungen bei neuem Messwert
        /// </summary>
        public virtual DbSet<TableNewValueNotification> TblNewValueNotifications { get; set; } = null!;

        #endregion
    }
}