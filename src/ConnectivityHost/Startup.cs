// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using BDA.Common.Exchange.GatewayService;
using BDA.GatewayService;
using BDA.GatewayService.Interfaces;
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.Com.Base.Helpers;
using BDA.Service.Com.GRPC.Services;
using BDA.Service.Com.MQTT;
using BDA.Service.Com.NewValueNotification;
using BDA.Service.EMail.Services;
using BDA.Service.Encryption;
using BDA.Service.TriggerAgent;
using Biss.Apps;
using Biss.Apps.Base.Connectivity.Interface;
using Biss.Apps.Blazor;
using Biss.Apps.Blazor.Extensions;
using Biss.Apps.Service.Connectivity;
using Biss.Dc.Core;
using Biss.Dc.Transport.Server.SignalR;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp;
using ConnectivityHost.Controllers;
using ConnectivityHost.Services;
using Database;
using Database.Tables;
using Exchange;
using Exchange.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using WebExchange;
using WebExchange.Interfaces;

namespace ConnectivityHost
{
    /// <summary>
    ///     <para>Startup</para>
    ///     Klasse Startup. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     Startup
        /// </summary>
        /// <param name="configuration">Config</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Logging.Log.LogTrace($"[{nameof(Startup)}]({nameof(Startup)}): [ServerApp] Launch App ...");
        }

        #region Properties

        /// <summary>
        ///     Config
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        ///     For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">Services</param>
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //Logging
            services.AddBissLog(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.AddApplicationInsights(
                    config => config.ConnectionString = Configuration["ApplicationInsights:ConnectionString"],
                    options => { options.FlushOnDispose = true; }
                );
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Appinsights
            services.AddApplicationInsightsTelemetry(options => { options.ConnectionString = Configuration["ApplicationInsights:ConnectionString"]; });

            services.InitBissApps(AppSettings.Current());

            //BISS Apps
            if (!VmBase.DisableConnectivityBuildInApp)
            {
                Logging.Log.LogInfo("Blazor: Init started");

                AppSettings.Current().DefaultViewNamespace = "ConnectivityHost.Pages.";
                AppSettings.Current().DefaultViewAssembly = "ConnectivityHost";

                var init = new BissInitializer();
                init.Initialize(AppSettings.Current(), new Language());

                VmProjectBase.InitializeApp().ConfigureAwait(true);

                Logging.Log.LogInfo("Blazor: Init finished");
            }

            services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()));
            services.AddRazorPages();
            services.AddServerSideBlazor();

            //Datenbank
            services.AddDbContext<Db>();

            //Connectivity
            services.AddDcSignalRCore<IServerRemoteCalls>(typeof(ServerRemoteCalls))
                .AddHubOptions<DcCoreHub<IServerRemoteCalls>>(o =>
                {
                    o.MaximumReceiveMessageSize = null;
                    o.EnableDetailedErrors = true;
                    o.MaximumParallelInvocationsPerClient = 5;
                });

            // configure DI for application services
            services.AddScoped<IAuthUserService, AuthUserService>();

            // Für Mails
            services.AddScoped<ICustomRazorEngine, CustomRazorEngine>();

            // Hintergrund-Service
            services.AddHostedService<BackgroundService<IServerRemoteCalls>>();

            services.AddMqttService(options =>
            {
                options.UserValidationFunction = async (username, password, serviceProvider) =>
                {
                    using var serviceScope = serviceProvider.CreateScope();
                    var context = serviceScope.ServiceProvider.GetRequiredService<Db>();

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (context is null)
                    {
                        return false;
                    }

                    var token = await context.TblAccessToken.Include(t => t.TblUser).FirstOrDefaultAsync(t => t.Token == password).ConfigureAwait(false);
                    return token != null && username == token.TblUser.LoginName;
                };

                options.TopicValidationFunction = async (clientId, topic, serviceProvider) =>
                {
                    try
                    {
                        using var serviceScope = serviceProvider.CreateScope();
                        var context = serviceScope.ServiceProvider.GetRequiredService<Db>();

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        if (context is null)
                        {
                            return false;
                        }

                        var token = await context.TblAccessToken
                            .Include(t => t.TblUser)
                            .FirstOrDefaultAsync(t => t.Token == clientId)
                            .ConfigureAwait(false);

                        if (token?.TblUser is null)
                        {
                            return false;
                        }

                        //admin darf alle topics
                        if (token.TblUser.IsAdmin)
                        {
                            return true;
                        }

                        //evtl. ändern wenn andere topics kommen können
                        var topicPathParts = topic.Split("/");
                        topicPathParts = topicPathParts.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        var prefix = topicPathParts[0];
                        var second = topicPathParts[1]; //should be "newValue"
                        // ReSharper disable once UnusedVariable
                        var third = topicPathParts[2]; // should be token
                        var fourth = topicPathParts[3]; //should be measurementDefinitionId

                        if (!long.TryParse(fourth, out var measurementDefinitionId))
                        {
                            return false;
                        }

                        if (prefix != options.TopicPrefix.Replace("/", string.Empty, StringComparison.InvariantCulture) || second != "newValue")
                        {
                            return false;
                        }

                        var r = await context.TblMeasurementDefinitions
                            .Include(x => x.TblIoTDevice)
                            .ThenInclude(x => x.TblGateway)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            .ThenInclude(x => x.TblCompany)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            .ThenInclude(x => x.TblPermissions)
                            .FirstOrDefaultAsync(x => x.Id == measurementDefinitionId)
                            .ConfigureAwait(false);


                        return r?.TblIoTDevice?.TblGateway?.TblCompany?.TblPermissions?.Any(x => x.TblUserId == token.TblUserId) ?? false;
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                        return false;
                    }
                };
            });

            services.AddNewValueNotificationService();

            // Authentication
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            //services.AddScoped<ILocalStorageService, LocalStorageService>();

            //services.AddSingleton<IDbProjectCacheConverter>(new DbProjectCacheConverter());

            services.AddMvc().AddControllersAsServices();

            services.AddControllers(o => { o.Conventions.Add(new ActionHidingConvention()); }).AddOData(oDataOptions =>
            {
                oDataOptions
                    .Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
                    .AddRouteComponents("api/odata", GetEdmModel());
            });

            services.AddSymmetricEncryption(WebSettings.Current().SymmetricEncryptionPrivateKey);

            services.AddRestAccess(new RestAccessService());

            services.AddGrpc();

            //Gatway Daten
            services.AddSingleton<IGatewayConnectedClientsManager, GatewayConnectedClientsManager>();

            //TriggerAgent
            services.AddSingleton<ITriggerAgent, TriggerAgent>();

            try
            {
                services.AddSwaggerGen(c =>
                {
                    try
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo {Title = "BDA API", Version = "v1", Description = "API für den Zugriff der gesammelten Daten von BDA. <br> Die API ist zugriffsgeschüzt. Durch Klick auf den Authorize Button erscheint ein Fenster, in diesem ein Token angegeben werden kann. Der Token kann im Konfigurationstool - BDA Konfig Tool - im Bereich 'ICH' erzeugt werden.<br>"});
                        // using System.Reflection;
                        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BDA.Service.Com.Rest.xml"));
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BDA.Service.Com.Base.xml"));
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BDA.Common.Exchange.xml"));

                        //Security
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Type = SecuritySchemeType.ApiKey,
                            Scheme = "Bearer",
                            BearerFormat = "Token",
                            In = ParameterLocation.Header,
                            Description = "Zugriffstoken hier einfügen - Wird vom Konfigurationstool zur Verfügung gestellt"
                        });
                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(Startup)}]({nameof(ConfigureServices)}): {e}");
                    }
                });
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(Startup)}]({nameof(ConfigureServices)}): {e}");
            }
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));

            app.UseCors();
            app.UseStaticFiles();

            app.UseRouting();

            //Authorization & Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
                endpoints.MapGrpcService<CompanyService>();
                endpoints.MapGrpcService<MeasurementResultService>();
                endpoints.MapGrpcService<MeasurementDefinitionService>();
                endpoints.MapGrpcService<ProjectService>();
                endpoints.MapGrpcService<GeoService>();
            });

            //DC Connectivity
            app.UseEndpoints(endpoints => { endpoints.MapHub<DcCoreHub<IServerRemoteCalls>>(DcHelper.DefaultHubRoute); });

            //Gateway Hub
            app.UseEndpoints(endpoints => { endpoints.MapHub<GatewayHub>($"/{GatewayConstants.HubName}"); });
            //Db.DeleteDatabase();
            //Db.CreateAndFillUp(); //IM ECHTBETRIEB AUSKOMMENTIEREN
            Db.SetupTemplates();
            Db.InitializeDbTriggers();
        }

        private IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.Ignore(typeof(Point));

            builder.EntitySet<TableIotDevice>(nameof(TableIotDevice));
            builder.EntitySet<TableMeasurementDefinition>(nameof(TableMeasurementDefinition));
            builder.EntitySet<TableMeasurementResult>(nameof(TableMeasurementResult)).EntityType.Ignore(x => x.SpatialPoint);

            var measurementResults = builder.EntitySet<TableMeasurementResult>(nameof(TableMeasurementResult));
            measurementResults.EntityType.Ignore(x => x.SpatialPoint);

            builder.EntityType<TableMeasurementResult>().Ignore(x => x.SpatialPoint);

            return builder.GetEdmModel();
        }
    }

    /// <summary>
    ///     Kontroller auf Swagger UI verbergen
    /// </summary>
    public class ActionHidingConvention : IActionModelConvention
    {
        #region Interface Implementations

        /// <summary>
        ///     Durchführen
        /// </summary>
        /// <param name="action">Action</param>
        public void Apply(ActionModel action)
        {
            //Liste der Controller, die versteckt werden sollen
            List<string> lstHideControllers = new();
            lstHideControllers.Add("Authentication");
            lstHideControllers.Add("Info");
            lstHideControllers.Add("WebLinks");

            // Controller verstecken
            if (lstHideControllers.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }
        }

        #endregion
    }
}