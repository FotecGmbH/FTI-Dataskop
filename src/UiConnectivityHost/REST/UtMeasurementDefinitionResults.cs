// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.Com.Base;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Tests.UiConnectivityHost;

namespace UiConnectivityHost.REST
{
    [TestClass]
    public class UtMeasurementDefinitionResults : UtTestHost
    {
        // ReSharper disable once NotAccessedField.Local
        private static HttpClient? _client;

        #region Properties

        public static string? Hostaddress { get; set; }
        public static UtConnectivityHostBase? HostBase { get; set; }

        #endregion

        [ClassInitialize] // [AssemblyInitialize] once per Assembly => [ClassInitialize] once per Class => [TestInitialize] once per test
        public static async Task Initialize(TestContext context)
        {
            HostBase = UtConnectivityHostBase.GetConnectivityHost();
            Assert.IsNotNull(HostBase);
            await HostBase.StartHostAsync().ConfigureAwait(false);
            await UtConnectivityHostBase.InitDatabase(HostBase.Db);

            var addresses = HostBase.Host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;

            Assert.IsNotNull(addresses);
            Hostaddress = addresses.FirstOrDefault();

            Assert.IsNotNull(Hostaddress);
            _client = new HttpClient {BaseAddress = new Uri(Hostaddress + "/api/")};
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestAddMeasurementResult()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //Arrange
            Assert.IsNotNull(HostBase);
            Assert.IsTrue(HostBase.HostRunnig);

            // ReSharper disable once UnusedVariable
            var result = new ExRestMeasurementResult
            {
                Location = new ExPosition
                {
                    Altitude = 200.1,
                    Latitude = 16.3,
                    Longitude = 45.2,
                    Presision = 10.2,
                    Source = EnumPositionSource.Modul,
                    TimeStamp = DateTime.Now
                }
            };


            // ReSharper disable once UnusedVariable
            var results = new List<int>();

            //Act


            //Assert
        }
    }
}