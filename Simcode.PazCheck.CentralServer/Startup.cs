#define LOCAL_IDENTITY_SERVER

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using JsonApiDotNetCore.Configuration;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Simcode.PazCheck.CentralServer.MicroServices;
using Ssz.Utils;
using Simcode.PazCheck.CentralServer.Presentation;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.IO;
using Ssz.Utils.Addons;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using System.Security.Cryptography;
using Ssz.Utils.Logging;
using Simcode.PazCheck.CentralServer.Common;
using Ssz.DataAccessGrpc.Client;
using Microsoft.AspNetCore.Http.Features;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using System.Text;
using IdentityServer4.Stores;
using System.Text.Json;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using System.IO.Compression;
using Ssz.DataAccessGrpc.ServerBase;
using Microsoft.OpenApi;
using Ssz.IdentityServer.Helpers.GSSAPI;




#if LOCAL_IDENTITY_SERVER
using Ssz.IdentityServer;
using IdentityServer4;
using IdentityServer4.Services;
#endif

namespace Simcode.PazCheck.CentralServer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "DV2002:Unmapped types", Justification = "System Class")]
    public class Startup
    {
        #region construction and destruction

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region public functions

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = false;
                //options.MaxReceiveMessageSize = null; // Default value
                //options.MaxSendMessageSize = 4 * 1024 * 1024; // 4 MB // Default value
                options.ResponseCompressionLevel = CompressionLevel.Fastest;
                options.ResponseCompressionAlgorithm = "gzip";
            });

            services.AddSingleton<MainServerWorker>();
            services.AddSingleton<IMainServerWorker>(sp => sp.GetRequiredService<MainServerWorker>());
            services.AddSingleton<IConfigurationProcessor, ConfigurationProcessor>();
            services.AddSingleton<Cache>();
            services.AddSingleton<IUsersAndRolesInfo, UsersAndRolesInfo>();            
            services.AddSingleton<AddonsManager>();
            services.AddSingleton<JobsManager>();            
            services.AddSingleton<MetaParamsToEvents>();
            services.AddSingleton<Reports>();
            services.AddSingleton<UserEventsLoggerCore>();
            services.AddSingleton<InformationSecurityEventsLoggerCore>();

            services.AddTransient<UserEventsLogger>();
            services.AddTransient<IInformationSecurityEventsLogger, InformationSecurityEventsLogger>();

            services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();
#if LOCAL_IDENTITY_SERVER
            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
                .AddInMemoryClients(IdentityServerConfig.GetClients(Configuration))
                .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes());
            builder.AddDeveloperSigningCredential();
            builder.AddProfileService<ADProfileService>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            
#if !RELEASE_PROD
            if (ConfigurationHelper.GetValue<bool>(Configuration, @"AuthorizationIsDisabled", false))
                _authorizationIsDisabled = true;
#endif

            services.AddAuthentication()
                .AddLocalApi(options =>
                {
                    options.ExpectedScope = "userapi";
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(@"MainPolicy", policy =>
                {
                    policy.AddAuthenticationSchemes(IdentityServer4.IdentityServerConstants.LocalApi.AuthenticationScheme);
                    //policy.RequireAuthenticatedUser(); // Commented for Swagger and Grafana. Need check
                    policy.RequireAssertion(MainPolicy_Assertion);                    
                });
            });


            //services.AddAuthentication()
            //    .AddLocalApi(IdentityServerConstants.LocalApi.AuthenticationScheme, _ => { });
            //    //.AddWsFederation(options =>
            //    //{
            //    //    options.Wtrealm = Configuration["wsfed:realm"];
            //    //    options.MetadataAddress = Configuration["wsfed:metadata"];
            //    //});

            services.AddSingleton(TimeProvider.System);
#else
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // VALTEMP
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost:50000";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.FromSeconds(10),
                    };
                });               

            services.AddAuthorization(options =>
            {
                options.AddPolicy("userapi", policy =>
                {
                    policy.AddAuthenticationSchemes(@"IdentityServerAccessToken");
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
            });
#endif

            // Experion SQL Server
            //services.AddDbContext<EmseventsContext>(opt =>
            //{
            //    opt.UseSqlServer(); // "Server=SRVEPKS01B;Database=emsevents;User ID=pazcheck;Password=Pier1@Exp!"
            //});

            // Blazor
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSignalR();

            services.AddDbContextFactory<PazCheckDbContext>(options =>
            {
#if !RELEASE_PROD
                options.EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
#endif
            });            

            IMvcCoreBuilder mvcBuilder = services.AddMvcCore(options =>
                {
                    options.UseCentralRoutePrefix(new RouteAttribute(RouteNamespace));
                })
                .AddJsonOptions(options => {
                    options.AllowInputFormatterExceptionMessages = true;
                    //options.JsonSerializerOptions.Converters.Add(new CaseInsensitiveOrderedDictionaryJsonConverter());
                })
#if DEBUG
                .AddApiExplorer() // For Swagger
#endif
                ;            
            services.AddJsonApi<PazCheckDbContext>(
                options =>
                {
                    options.Namespace = RouteNamespace; // CentralRoutePrefix not added here, because already root prefix
                    options.DefaultPageSize = null;
                    options.IncludeTotalResourceCount = true;
                    options.IncludeExceptionStackTraceInErrors = true;
                    options.ClientIdGeneration = ClientIdGenerationMode.Allowed;                    
                    options.SerializerOptions.DictionaryKeyPolicy = null; // Fix first letter lower-case
                    //options.SerializerOptions.Converters.Add(new CaseInsensitiveOrderedDictionaryJsonConverter());
                },
                discover => discover
                    .AddCurrentAssembly(), 
                mvcBuilder: mvcBuilder);

#if DEBUG
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v3", new OpenApiInfo { Title = "PazCheck API", Version = "v3.0.0" });
                
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, @"Simcode.PazCheck.CentralServer.Common.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, @"Simcode.PazCheck.CentralServer.xml"));

                //options.SupportNonNullableReferenceTypes();
                //options.MapType<CaseInsensitiveOrderedDictionary<string?>>(() => new OpenApiSchema { Type = "string" });
                //options.SchemaFilter<CaseInsensitiveOrderedDictionarySchemaFilter>();
                options.OperationFilter<SwaggerOperationFilter>();                           
            });
#endif

            services.AddCors();
#if LOCAL_IDENTITY_SERVER
            var cors = new DefaultCorsPolicyService(new LoggerFactory().CreateLogger<DefaultCorsPolicyService>())
            {
                AllowAll = true
            };
            services.AddSingleton<ICorsPolicyService>(cors);
#endif      
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, IConfiguration configuration)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<KerberosAuthMiddleware>();

            app.UseCors(
                a => a.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition")); // this is the important line

            // Blazor
            app.UseStaticFiles();
            
            app.UseMiddleware<GrafanaProxyMiddleware>();

            app.UseRouting();
            app.UseJsonApi();

#if DEBUG
            app.UseSwagger(); // http://localhost:5000/swagger/v3/swagger.json
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v3/swagger.json", "v3");
                //options.RoutePrefix = string.Empty;
            }); // http://localhost:5000/swagger
#endif

            app.UseAuthentication();
            app.UseAuthorization();
#if LOCAL_IDENTITY_SERVER
            app.UseIdentityServer();
#endif            
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // For NETSTANDARD2.0 Clients

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers().RequireAuthorization(@"MainPolicy");

                    endpoints.MapGrpcService<DataAccessService>();

                    // Blazor
                    endpoints.MapBlazorHub().RequireAuthorization(@"MainPolicy");
                    endpoints.MapFallbackToPage("/_Host").RequireAuthorization(@"MainPolicy");

                    endpoints.MapHub<MainHub>("/mainhub"); // TODO .RequireAuthorization(@"MainPolicy")
                });

            _cache = serviceProvider.GetRequiredService<Cache>();
        }

        #endregion

        #region private functions

        /// <summary>
        ///     Returns true if succeeded.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool MainPolicy_Assertion(AuthorizationHandlerContext context)
        {
            if (_authorizationIsDisabled)
                return true;

            var httpContext = context.Resource as Microsoft.AspNetCore.Http.DefaultHttpContext;
            if (httpContext == null) 
                return false;            

            var roles = HttpContextHelper.GetRoles(httpContext);
            string user = HttpContextHelper.GetUserLowerInvariant(httpContext);
            string sourceIpAddress = HttpContextHelper.GetSourceIpAddress(httpContext);            
            
            return _cache.CheckAccess(user, roles, sourceIpAddress, RouteNamespace, httpContext.Request);
        }

        #endregion

        private const string RouteNamespace = @"/api/v3";

        private bool _authorizationIsDisabled;

        private Cache _cache = null!;
    }
}

//public Startup(IConfiguration configuration)
//{
//    var contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
//}

//public Startup(IConfiguration configuration, IWebHostEnvironment env)
//{
//    var contentRoot = env.ContentRootPath;
//}