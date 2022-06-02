using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic.Safety;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace Simcode.PazCheck.CentralServer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "DV2002:Unmapped types", Justification = "System Class")]
    class Program
    {
        #region public functions 

        public static IHost Host { get; private set; } = null!;

        public static Options Options { get; private set; } = null!;

        #endregion

        //#region public functions

        //public static void SafeShutdown()
        //{
        //    var task = Host.StopAsync();
        //}

        //#endregion

        #region private functions

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureLogging(
                    builder =>
                        builder.ClearProviders()
                           .AddSszLogger()
                    )
                .UseSystemd()
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {                        
                        webBuilder.UseStartup<Startup>();
                    })
                .ConfigureServices((hostContext, services) =>
                {                    
                    services.AddHostedService<MainBackgroundService>();
                    services.AddHostedService<SafetyBackgroundService>();
                });
        }

        private static void Main(string[] args)
        {
            Host = CreateHostBuilder(args).Build();

            var logger = Host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("App starting with args: " + String.Join(" ", args));

            IConfiguration configuration = Host.Services.GetRequiredService<IConfiguration>();
            CultureHelper.InitializeUICulture(configuration);
            Options = new Options(configuration);

            Host.Run();
        }

        #endregion
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "DV2002:Unmapped types", Justification = "System Class")]
    public class Options
    {
        #region construction and destruction

        public Options(IConfiguration configuration)
        {
            
        }

        #endregion

        #region public functions
        

        #endregion
    }
}