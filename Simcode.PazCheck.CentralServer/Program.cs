using Ssz.Utils.ConfigurationCrypter;
using Ssz.Utils.ConfigurationCrypter.CertificateLoaders;
using Ssz.Utils.ConfigurationCrypter.ConfigurationCrypters;
using Ssz.Utils.ConfigurationCrypter.ConfigurationCrypters.Yaml;
using Ssz.Utils.ConfigurationCrypter.Crypters;
using Ssz.Utils.ConfigurationCrypter.Extensions;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Presentation;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Ssz.Dcs.CentralServer.Common.Helpers;

[assembly: AssemblyCopyright("Все права защищены. ООО \"Симкод\"")]

namespace Simcode.PazCheck.CentralServer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "DV2002:Unmapped types", Justification = "System Class")]
    class Program
    {
        #region public functions 

        /// <summary>
        ///     Имя файла сертификата .pfx из которого используются закрытый и открытый ключи для шифрования и расшифровки секций конфигурационного файла, которые не должны храниться в открытом виде
        /// </summary>
        public const string ConfigurationCrypterCertificateFileName = @"appsettings.pfx";

        public static IHost Host { get; private set; } = null!;

        public static string EnvironmentName { get; private set; } = null!;

        public static bool SuperUserIsEnabled { get; private set; }

        public static bool TestUsersIsEnabled { get; private set; }

        #endregion

        //#region public functions

        //public static void SafeShutdown()
        //{
        //    var task = Host.StopAsync();
        //}

        //#endregion

        #region private functions

        private static async Task Main(string[] args)
        {
            ConfigurationHelper.SetCurrentDirectory(args);

            //AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Host = CreateHostBuilder(args).Build();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var logger = Host.Services.GetRequiredService<ILogger<Program>>();
            IConfiguration configuration = Host.Services.GetRequiredService<IConfiguration>();
            CultureHelper.InitializeUICulture(configuration, logger);

            LoggerHelper.LogProgramInformation(logger, configuration, args, EnvironmentName);

#if !RELEASE_PROD            
            TestUsersIsEnabled = true;
            SuperUserIsEnabled = true;
#endif

            ILoggersSet loggersSet = new LoggersSet(new UserFriendlyLogger((logLevel, eventId, line) => Console.WriteLine(line)), null);
            try
            {
                if (args.Any(a => a == @"-e"))
                {
                    EncryptAppsettings(loggersSet);
                    Console.WriteLine(Common.Properties.Resources.EncryptAppsettingsSuccess);
                }
                else if (args.Any(a => a == @"-d"))
                {
                    DecryptAppsettings(loggersSet);
                    Console.WriteLine(Common.Properties.Resources.DecryptAppsettingsSuccess);
                }
                else if (args.Any(a => a == @"-p"))
                {
                    SetPostgreCryptoPassword(Host.Services, configuration, loggersSet);
                    Console.WriteLine(Common.Properties.Resources.SetPostgreCryptoPasswordSuccess);
                }
                else if (args.Any(a => a == @"-u"))
                {
                    PazCheckDbHelper.InitializeMainDb(Host.Services, loggersSet);                    
                    Console.WriteLine(Common.Properties.Resources.DbInitializationSuccess);
                }                                
                else
                {
                    if (args.Any(a => a == @"--superuser") || args.Any(a => a == @"-su"))
                    {
                        Console.WriteLine("Запущено в режиме, позволяющем роль суперпользователя.");
                        SuperUserIsEnabled = true;                        
                    }

                    if (ConfigurationHelper.IsMainProcess(configuration))
                        PazCheckDbHelper.UpdateMainDb(Host.Services, new LoggersSet(logger, null));

                    await Host.RunAsync();
                }
            }
            catch (Exception ex) 
            {
                loggersSet.Logger.LogCritical(ex, Properties.Resources.UnhandledException);
            }            
        }

        //private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        //{
        //    Exception e = (Exception)args.ExceptionObject;            
        //}

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var switchMappings = new Dictionary<string, string>()
            {
                { ConfigurationConstants.ConfigurationKeyMapping_CurrentDirectory, ConfigurationConstants.ConfigurationKey_CurrentDirectory }
            };

            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    EnvironmentName = ConfigurationHelper.GetEnvironmentName(hostingContext.HostingEnvironment);

                    config.Sources.Clear();

                    config.AddEncryptedAppSettings(hostingContext.HostingEnvironment, crypter =>
                    {
                        crypter.CertificatePath = ConfigurationCrypterCertificateFileName;
                        crypter.KeysToDecrypt = GetKeysToEncrypt();
                    });

                    config.AddCommandLine(args, switchMappings);
                })
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
                    services.AddHostedService<DataAccessBackgroundService>();
                });
        }

        private static List<string> GetKeysToEncrypt()
        {   
            return new()
            {
                $"Kestrel:Certificates:Default:Password",
                $"ConnectionStrings:MainDbConnection",
                $"ApiProxyId",
                $"ActiveDirectory:ServiceAccount:Password",
                $"Encrypted"
            };
        }             

        private static void EncryptAppsettings(ILoggersSet loggersSet)
        {
            ICertificateLoader certificateLoader = new FilesystemCertificateLoader(ConfigurationCrypterCertificateFileName);
            IConfigurationCrypter configurationCrypter = new YamlConfigurationCrypter(new RSACrypter(certificateLoader));
            foreach (var yamlConfigurationFilePath in ConfigurationHelper.GetYamlConfigurationFilePaths(null))
            {
                configurationCrypter.EncryptKeys(yamlConfigurationFilePath, GetKeysToEncrypt().ToHashSet());
            }
        }

        private static void DecryptAppsettings(ILoggersSet loggersSet)
        {            
            ICertificateLoader certificateLoader = new FilesystemCertificateLoader(ConfigurationCrypterCertificateFileName);
            IConfigurationCrypter configurationCrypter = new YamlConfigurationCrypter(new RSACrypter(certificateLoader));
            foreach (var yamlConfigurationFilePath in ConfigurationHelper.GetYamlConfigurationFilePaths(null))
            {
                configurationCrypter.DecryptKeys(yamlConfigurationFilePath, GetKeysToEncrypt().ToHashSet());
            }
        }

        private static void SetPostgreCryptoPassword(IServiceProvider serviceProvider, IConfiguration configuration, ILoggersSet loggersSet)
        {
            Console.Write(Properties.Resources.NewPassword);
            String password1 = GetPasswordFromCommandLine();
            if (String.IsNullOrEmpty(password1))
                return;            
            Console.Write(Properties.Resources.RepeatNewPassword);
            String password2 = GetPasswordFromCommandLine();
            if (password1 != password2)
            {
                Console.WriteLine(Properties.Resources.PasswordsDoNotMatch);
                return;
            }
            PazCheckDbHelper.SetPostgreCryptoPassword(password1, serviceProvider, configuration, loggersSet);            
        }

        private static string GetPasswordFromCommandLine()
        {
            var password = new List<char>();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (password.Count > 0)
                    {
                        password.RemoveAt(password.Count - 1);
                        //Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000') // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                {
                    password.Add(i.KeyChar);
                    //Console.Write("*");
                }
            }
            Console.WriteLine();
            return new String(password.ToArray());
        }

        /// <summary>
        ///     Main thread.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="configuration"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        private static async Task QuickTestAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILoggersSet loggersSet)
        {
            await Task.Delay(0);
            //string programDataDirectoryFullName = ConfigurationHelper.GetProgramDataDirectoryFullName(configuration);
            //using SszLogger informationSszLogger = new(@"Information text file logger", new SszLoggerOptions
            //{
            //    LogsDirectory = programDataDirectoryFullName,
            //    LogFileName = @"testlog_information.txt",
            //    DeleteOldFilesAtStart = true,
            //    LogLevel = LogLevel.Information,
            //    LogLevelIsExclusive = true
            //});
            //using SszLogger warningSszLogger = new(@"Warning text file logger", new SszLoggerOptions
            //{
            //    LogsDirectory = programDataDirectoryFullName,
            //    LogFileName = @"testlog_warning.txt",
            //    DeleteOldFilesAtStart = true,
            //    LogLevel = LogLevel.Warning,
            //    LogLevelIsExclusive = true
            //});
            //using SszLogger errorSszLogger = new(@"Error text file logger", new SszLoggerOptions
            //{
            //    LogsDirectory = programDataDirectoryFullName,
            //    LogFileName = @"testlog_error.txt",
            //    DeleteOldFilesAtStart = true,
            //    LogLevel = LogLevel.Error,
            //    LogLevelIsExclusive = true
            //});
            //loggersSet.SetUserFriendlyLogger(new WrapperUserFriendlyLogger(
            //    informationSszLogger,
            //    warningSszLogger,
            //    errorSszLogger
            //    ));

            //DefaultDispatcher dispatcher = new();
            //var csvDb = ActivatorUtilities.CreateInstance<CsvDb>(
            //    serviceProvider, new DirectoryInfo(ConfigurationHelper.GetProgramDataDirectoryFullName(configuration)), dispatcher);
            //var addonsManager = serviceProvider.GetRequiredService<AddonsManager>();
            //addonsManager.Initialize(loggersSet.UserFriendlyLogger,
            //    new AddonBase[] { new PazCheckCentralServerAddon(), new DiagnostAddon(), new MonitoringAddon(), new EMailsAddon(), new LoggingAddon() },
            //    csvDb,
            //    dispatcher,
            //    null,
            //    new AddonsManagerOptions
            //    {
            //        AddonsSearchPattern = @"Simcode.PazCheck.Addons.*.dll",
            //        CanModifyAddonsCsvFiles = true
            //    });

            //await PazCheckDbHelper.QuickTestAsync(addonsManager, serviceProvider, configuration, loggersSet);
        }

#endregion
    }
}