using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Presentation;
using Ssz.Utils;
using Ssz.Utils.Logging;
using Ssz.Utils.Addons;
using Simcode.PazCheck.CentralServer.Common;
using System.Diagnostics;
using System.Collections.Specialized;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Microsoft.EntityFrameworkCore.Internal;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Ssz.Utils.Diagnostics;
using Ssz.Utils.DataAccess;
using Ssz.DataAccessGrpc.Client;
using Microsoft.Extensions.Primitives;

namespace Simcode.PazCheck.CentralServer
{
    public partial class MainServerWorker : IMainServerWorker
    {        
        #region construction and destruction

        public MainServerWorker(
            IServiceProvider serviceProvider,            
            AddonsManager addonsManager,
            JobsManager jobsManager,              
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILogger<MainServerWorker> logger,
            IConfiguration configuration,            
            IConfigurationProcessor configurationProcessor)
        {            
            ServiceProvider = serviceProvider;
            
            _jobsManager = jobsManager;            
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _configuration = configuration;            
            _addonsManager = addonsManager;            
            _configurationProcessor = configurationProcessor;            

            // Creates all directories and subdirectories in the specified path unless they already exist.
            Directory.CreateDirectory(PazCheckConstants.DirectoryName_CsvDb);
            CsvDb = ActivatorUtilities.CreateInstance<CsvDb>(
                ServiceProvider, PazCheckConstants.DirectoryName_CsvDb, _jobsManager.ServerWorker_ThreadSafeDispatcher);

            DataAccessProvider = ActivatorUtilities.CreateInstance<GrpcDataAccessProvider>(ServiceProvider);

            DataAccessServerWorker = ActivatorUtilities.CreateInstance<DataAccessServerWorker>(ServiceProvider);
        }

        #endregion

        #region public functions

        public IServiceProvider ServiceProvider { get; }

        public CsvDb CsvDb { get; } = null!;

        /// <summary>
        /// Can be accesed only from MainServerWorker thread.
        /// </summary>
        public IDataAccessProvider DataAccessProvider { get; }

        /// <summary>
        ///      Need synchronization: lock (((System.Collections.ICollection)SystemParams).SyncRoot)
        /// </summary>
        public CaseInsensitiveOrderedDictionary<Any> SystemParams { get; } = new();        

        public DataAccessServerWorker DataAccessServerWorker { get; }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await DataAccessServerWorker.InitializeAsync(cancellationToken);            

            //string examplesDirectoryFullName = ServerConfigurationHelper.GetExamplesDirectoryFullName(_configuration);
            //var userEventLogFileInfo = new FileInfo(Path.Combine(examplesDirectoryFullName, "UserEvent.log"));
            //if (userEventLogFileInfo.Exists) userEventLogFileInfo.Delete();
            //using var userEventUserFriendlyLogger = new SszLogger("UserEvent Logger", new SszLoggerOptions
            //{
            //    LogsDirectory = examplesDirectoryFullName,
            //    LogFileName = userEventLogFileInfo.Name,
            //    LogLevel = LogLevel.Information
            //});                                           

            if (ConfigurationHelper.IsMainProcess(_configuration))
            {
                string grafanaUrl = ConfigurationHelper.GetValue(_configuration, PazCheckConstants.ConfigurationKey_GrafanaUrl, @"");
                if (grafanaUrl != @"")
                {
#if !DEBUG                 
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var startInfo = new ProcessStartInfo(@"grafana\windows\bin\grafana-server.exe")
                    {
                        WorkingDirectory = @"grafana\windows\bin\"
                    };
                    try
                    {
                        _grafanaProcess = Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Cannot start grafana-server.");
                    }
                }
                else
                {
                    var startInfo = new ProcessStartInfo(@"grafana/linux/bin/grafana-server")
                    {
                        WorkingDirectory = @"grafana/linux/bin/"
                    };
                    try
                    {
                        _grafanaProcess = Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Cannot start grafana-server.");
                    }
                }
#endif
                }
            }

            // Must be after DB initialization and IMainServerWorker initialized.
            _addonsManager.Addons.CollectionChanged += AddonsManager_Addons_OnCollectionChanged;
            _addonsManager.Initialize(null,
                new AddonBase[] { new PazCheckCentralServerAddon(), new DiagnostAddon(), new MonitoringAddon(), new EMailsAddon(), new LoggingAddon() },
                CsvDb,
                _jobsManager.ServerWorker_ThreadSafeDispatcher,
                _configurationProcessor.ProcessValue,
                new AddonsManagerOptions
                {
                    AddonsSearchPattern = @"Simcode.PazCheck.Addons.*.dll",
                    CanModifyAddonsCsvFiles = ConfigurationHelper.IsMainProcess(_configuration)
                });
            ChangeToken.OnChange(
                () => _configuration.GetReloadToken(),
                () => {
                    _addonsManager.Dispatcher!.BeginInvoke(ct => _addonsManager.RefreshAddons());
                });

            DataAccessProvider.Initialize(
                null,
                @"local", // Any not null address
                @"local", // Any not null
                @"local", // Workstation name for messages filtering
                @"DCS",
                new CaseInsensitiveOrderedDictionary<string?>(),
                new DataAccessProviderOptions { LocalDataAccessServerWorker = DataAccessServerWorker },
                _jobsManager.ServerWorker_ThreadSafeDispatcher);            

            if (!Program.SuperUserIsEnabled)
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                            @"127.0.0.1",
                            @"localhost",
                            InformationSecurityEventsLogger.ComponentStartStop_EventId,
                            4,
                            true,
                            Properties.Resources.ComponentStarted_Event,
                            @"System",
                            PazCheckCentralServerAddon.AddonIdentifier,
                            @"",
                            Properties.Resources.ComponentStarted_EventDesc, Properties.Resources.PazCheckCentralServerAddon_Desc);
            }
            else
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                        @"127.0.0.1",
                        @"localhost",
                        InformationSecurityEventsLogger.ComponentStartStop_EventId,
                        9,
                        true,
                        Properties.Resources.ComponentStartedWithSuperUser_Event,
                        @"System",
                        PazCheckCentralServerAddon.AddonIdentifier,
                        @"",
                        Properties.Resources.ComponentStartedWithSuperUser_EventDesc, Properties.Resources.PazCheckCentralServerAddon_Desc);
            }
        }

        /// <summary>
        ///     Main thread.
        /// </summary>
        /// <param name="nowUtc"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (AddonBase addon in _addonsManager.AddonsThreadSafe)
            {
                await addon.DoWorkAsync(nowUtc, cancellationToken);
            }

            Cleanup(nowUtc, cancellationToken);
        }

        public async Task CloseAsync()
        {
            if (ConfigurationHelper.IsMainProcess(_configuration))
                _grafanaProcess?.Kill();

            await DataAccessProvider.CloseAsync();

            await DataAccessServerWorker.CloseAsync();

            _addonsManager.Close();

            _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                        @"127.0.0.1",
                        @"localhost",
                        InformationSecurityEventsLogger.ComponentStartStop_EventId,
                        7,
                        true,
                        Properties.Resources.ComponentStopped_Event,
                        @"System",
                        PazCheckCentralServerAddon.AddonIdentifier,
                        @"",
                        Properties.Resources.ComponentStopped_EventDesc, Properties.Resources.PazCheckCentralServerAddon_Desc);
            _logger.LogInformation("Queued Hosted Service is stopping.");
        }

        #endregion

        #region private functions      

        private void AddonsManager_Addons_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null)
                        foreach (AddonBase addon in e.NewItems.OfType<AddonBase>())
                        {
                            if (addon is DataAccessProviderGetter_AddonBase dataAccessProviderGetter_AddonBase)
                                OnDataAccessProviderGetter_Addon_Added(dataAccessProviderGetter_AddonBase);

                            _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                                @"127.0.0.1",
                                @"localhost",                                
                                InformationSecurityEventsLogger.ComponentStartStop_EventId,
                                4,
                                true,
                                Properties.Resources.ComponentStarted_Event,
                                @"System",
                                addon.Identifier,
                                @"",
                                Properties.Resources.ComponentStarted_EventDesc, addon.Desc);
                        }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is not null)
                        foreach (AddonBase addon in e.OldItems.OfType<AddonBase>())
                        {
                            if (addon is DataAccessProviderGetter_AddonBase dataAccessProviderGetter_AddonBase)
                                OnDataAccessProviderGetter_Addon_Removed(dataAccessProviderGetter_AddonBase);

                            _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                                @"127.0.0.1",
                                @"localhost",                                
                                InformationSecurityEventsLogger.ComponentStartStop_EventId,
                                4,
                                true,
                                Properties.Resources.ComponentStopped_Event,
                                @"System",
                                addon.Identifier,
                                @"",
                                Properties.Resources.ComponentStopped_EventDesc, addon.Desc);
                        }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        /// <summary>
        ///     WindowsService main thread.
        /// </summary>
        private void Cleanup(DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (nowUtc - _last_GC_CleanUpDateTimeUtc > TimeSpan.FromMinutes(5))
            {
                ComputerInfo computerInfo = new();

                if ((float)Process.GetCurrentProcess().WorkingSet64 / (float)computerInfo.TotalPhysicalMemory > 0.9f)
                    throw new OperationCanceledException();

                _last_GC_CleanUpDateTimeUtc = nowUtc; // Because long-running operation.                
            }
        }

        #endregion

        #region private fields        

        private readonly AddonsManager _addonsManager;        

        private readonly JobsManager _jobsManager;
        
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;       

        private readonly ILogger _logger;

        private readonly IConfiguration _configuration;
        private readonly IConfigurationProcessor _configurationProcessor;        

        private Process? _grafanaProcess;

        private DateTime _last_GC_CleanUpDateTimeUtc = DateTime.UtcNow;

        #endregion
    }
}


//if (ConfigurationHelper.GetValue<bool>(_configuration, @"InitializeByScriptMainDb", false))
//{
//    var stopwatch = new Stopwatch();
//    stopwatch.Start();
//    _logger.LogInformation("InitializeByScriptMainDb Started.");
//    PazCheckDbHelper.InitializeByScriptMainDb_IfNeeded(_serviceProvider);
//    stopwatch.Stop();
//    _logger.LogInformation("InitializeByScriptMainDb Finished. Ms = {0}", stopwatch.ElapsedMilliseconds);
//}