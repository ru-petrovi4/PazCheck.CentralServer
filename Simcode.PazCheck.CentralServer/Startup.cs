#define LOCAL_IDENTITY_SERVER

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using JsonApiDotNetCore.Configuration;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Ssz.Utils;
using Simcode.PazCheck.CentralServer.Presentation;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.IO;
#if LOCAL_IDENTITY_SERVER
using Simcode.IdentityServer;
using IdentityServer4;
using IdentityServer4.Services;
#endif

namespace Simcode.PazCheck.CentralServer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "DV2002:Unmapped types", Justification = "System Class")]
    public class Startup
    {
        #region public functions

        public void ConfigureServices(IServiceCollection services)
        {
#if LOCAL_IDENTITY_SERVER
            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
                .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes);
            builder.AddDeveloperSigningCredential();
            builder.AddProfileService<ADProfileService>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();

            services.AddAuthentication()
                .AddLocalApi(options =>
                {
                    options.ExpectedScope = "userapi";
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("userapi", policy =>
                {
                    policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
            });
#else
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // VALTEMP
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost:50000";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
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

            services.AddDbContext<PazCheckDbContext>();

            IMvcCoreBuilder mvcBuilder = services.AddMvcCore()
                .AddApiExplorer(); // For Swagger
            services.AddJsonApi<PazCheckDbContext>(
                options =>
                {
                    options.Namespace = "api/v3";
                    options.DefaultPageSize = null;
                    options.IncludeTotalResourceCount = true;
                },
                discover => discover.AddCurrentAssembly(), mvcBuilder: mvcBuilder);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v3", new OpenApiInfo { Title = "PazCheck API", Version = "v3.0.0" });

                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
            });

            services.AddCors();
#if LOCAL_IDENTITY_SERVER
            var cors = new DefaultCorsPolicyService(new LoggerFactory().CreateLogger<DefaultCorsPolicyService>())
            {
                AllowAll = true
            };
            services.AddSingleton<ICorsPolicyService>(cors);
#endif
            //services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            //services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();

            services.AddSingleton<AddonsManager>();
            services.AddSingleton<JobsManager>();
            services.AddSingleton<Licence>();
            services.AddScoped<TagsImporter>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }            

            app.UseCors(a => a.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseRouting();
            app.UseJsonApi();
            app.UseSwagger(); // http://localhost:5000/swagger/v3/swagger.json
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v3/swagger.json", "v3");
                //options.RoutePrefix = string.Empty;
            }); // http://localhost:5000/swagger            
            app.UseAuthentication();
            app.UseAuthorization();
#if LOCAL_IDENTITY_SERVER
            app.UseIdentityServer();
#endif

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        #endregion
    }
}