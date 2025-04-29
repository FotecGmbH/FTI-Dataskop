// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using BDA.Common.Exchange.Enum;
using BDA.Service.AppConnectivity.DataConnector;
using Biss.Apps;
using Biss.Dc.Server;
using Biss.Dc.Transport.Server.SignalR;
using Biss.Log.Producer;
using ConnectivityHost;
using Database;
using Database.Common;
using Database.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


// ReSharper disable once CheckNamespace
namespace Tests.UiConnectivityHost
{
    /// <summary>
    ///     <para>Basis für Test im Connectivity Host</para>
    ///     Klasse UtTestHost. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UtTestHost
    {
        internal UtConnectivityHostBase Th = null!;

        protected UtTestHost()
        {
            Th = UtConnectivityHostBase.GetConnectivityHost();
        }
    }

    /// <summary>
    ///     <para>Basis für Test im Connectivity Host</para>
    ///     Klasse UtConnectivityHosteBase. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UtConnectivityHostBase : IDisposable
    {
        private static UtConnectivityHostBase _testHost = null!;
        internal readonly Db Db = null!;
        internal readonly ServerRemoteCalls Dc = null!;
        internal readonly DcConnectionsSignalR<IServerRemoteCalls> DcConnectionsSignalR = null!;

        internal readonly bool Debugging;
        internal readonly IHost Host = null!;
        internal readonly IServiceProvider Services = null!;
        private bool _disposedValue;

        private protected UtConnectivityHostBase()
        {
            VmBase.DisableConnectivityBuildInApp = true;

            var para = new[] {"--urls", "http://localhost:5001/"};

            //Services = Program.CreateHostBuilder(new string[] { }).Build().Services;
            Host = Program.CreateHostBuilder(para).Build();
            Services = Host.Services;

            Assert.IsNotNull(Services, "Can not start ConnectivityHost!");

            var dc = Services.GetServices(typeof(IServerRemoteCalls)).FirstOrDefault();

            var dcConnectionsSignalR = Services.GetServices(typeof(IDcConnections)).FirstOrDefault();

            Assert.IsNotNull(dc, "DC Methods not found via dependency injection.");
            Assert.IsNotNull(dcConnectionsSignalR, "DC SingnalR not found via dependency injection.");

            Dc = (ServerRemoteCalls) dc;
            DcConnectionsSignalR = (DcConnectionsSignalR<IServerRemoteCalls>) dcConnectionsSignalR;

            Db = new Db();
            Assert.IsNotNull(Db, "Db is NULL!");

            Debugging = Debugger.IsAttached;

            if (Debugging)
            {
                Logging.Log.LogInfo($"[{GetType().Name}]({nameof(UtConnectivityHostBase)}): Debugger is attached!");
            }
        }

        #region Properties

        /// <summary>
        ///     Host gestartet
        /// </summary>
        public bool HostRunnig { get; set; }

        #endregion

        public static UtConnectivityHostBase GetConnectivityHost()
        {
            if (_testHost == null!)
            {
                _testHost = new UtConnectivityHostBase();
            }

            return _testHost;
        }

        public static async Task InitDatabase(Db db)
        {
            var context = db ?? throw new ArgumentNullException(nameof(db));

            if (!context.TblCompanies.Any())
            {
                context.TblCompanies.Add(new TableCompany
                {
                    CompanyType = EnumCompanyTypes.Company,
                    Information = new DbInformation {CreatedDate = DateTime.Now, Description = "TestCompany", Name = "TestCompany", UpdatedDate = DateTime.Now}
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            if (!context.TblUsers.Any())
            {
                context.TblUsers.Add(new TableUser
                {
                    FirstName = "testuserFirstname",
                    LastName = "testUserLastname",
                    CreatedAtUtc = DateTime.UtcNow,
                    IsAdmin = true
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            if (!context.TblGateways.Any())
            {
                context.TblGateways.Add(new TableGateway
                {
                    TblCompany = context.TblCompanies.First(),
                    Information = new DbInformation {CreatedDate = DateTime.Now, Description = "TestGateway", Name = "TestGateway", UpdatedDate = DateTime.Now},
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            if (!context.TblIotDevices.Any())
            {
                context.TblIotDevices.Add(new TableIotDevice
                {
                    Information = new DbInformation {CreatedDate = DateTime.Now, Description = "TestIotDevice", Name = "TestIotDevice", UpdatedDate = DateTime.Now},
                    TblGateway = context.TblGateways.First()
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            if (!context.TblMeasurementDefinitions.Any())
            {
                context.TblMeasurementDefinitions.Add(new TableMeasurementDefinition
                {
                    Information = new DbInformation {CreatedDate = DateTime.Now, Description = "TestMeasurementDefinition", Name = "TestMeasurementDefinition", UpdatedDate = DateTime.Now},
                    TblIoTDevice = context.TblIotDevices.First()
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<bool> StartHostAsync()
        {
            var h = GetConnectivityHost();

            try
            {
                await h.Host.StartAsync().ConfigureAwait(true);
            }
            catch
            {
                h.HostRunnig = false;
            }

            h.HostRunnig = true;
            return h.HostRunnig;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        protected virtual async void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (HostRunnig)
                    {
                        await Host.StopAsync(new TimeSpan(0, 0, 2)).ConfigureAwait(false);
                        Host.Dispose();
                    }

                    await Db.DisposeAsync().ConfigureAwait(false);
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        #region Interface Implementations

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UtConnectivityHostBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}