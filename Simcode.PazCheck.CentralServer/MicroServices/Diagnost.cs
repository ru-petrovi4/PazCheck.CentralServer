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
using System.Collections.Generic;
using System.Text;
using Ssz.Utils.DataAccess;
using System.Runtime.CompilerServices;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using System.Collections.Specialized;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Ssz.Utils.Dispatcher;

namespace Simcode.PazCheck.CentralServer.MicroServices
{
    public class Diagnost
    {
        #region construction and destruction

        public Diagnost(
            IMainServerWorker mainServerWorker,            
            AddonsManager addonsManager,
            Cache cache,
            JobsManager jobsManager,            
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IConfiguration configuration,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<Diagnost> logger)
        {
            _mainServerWorker = mainServerWorker;            
            _addonsManager = addonsManager;
            _cache = cache;
            _jobsManager = jobsManager;            
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

            _longRunningTask = new(_logger);
        }

        #endregion

        #region public functions

        public void Initialize(CaseInsensitiveOrderedDictionary<string?> addonOptionsSubstituted)
        {
            _addonOptionsSubstituted = addonOptionsSubstituted;
            
            _mainServerWorker.DataAccessProvider.EventMessagesCallback += DataAccessProvider_OnEventMessagesCallback;

            _longRunningTask.Initialize(LongRunning_TaskMainAsync);
        }

        public async Task DoWorkAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            if (!ConfigurationHelper.IsMainProcess(_configuration))
                return;
            
            if (nowUtc < _lastDoWorkDateTimeUtc + TimeSpan.FromSeconds(60)) // 1 minute
                return;
            _lastDoWorkDateTimeUtc = nowUtc;            

            await using var dbContext = _dbContextFactory.CreateDbContext();
            
            var unitEvents_MaxRecordsCount = ConfigurationHelper.GetValue<int>(_addonOptionsSubstituted, DiagnostAddon.UnitEvents_MaxRecordsCount_OptionName, 0);
            var unitEvents_MaxStorageTime = ConfigurationHelper.GetValue<TimeSpan>(_addonOptionsSubstituted, DiagnostAddon.UnitEvents_MaxStorageTime_OptionName, default(TimeSpan));            
            if (unitEvents_MaxRecordsCount > 0 && unitEvents_MaxRecordsCount < Int32.MaxValue)
            {
                var count = await dbContext.UnitEvents.CountAsync();
                if (count > unitEvents_MaxRecordsCount)
                    await dbContext.UnitEvents                        
                        .OrderBy(le => le.UnitEventsInterval.LoadTimeUtc)
                        .Take(count - unitEvents_MaxRecordsCount).ExecuteDeleteAsync();

                //var unitEvents_WarningRecordsCount = new Any(_addonOptionsSubstituted.TryGetValue(DiagnostAddon.UnitEvents_WarningRecordsCount_OptionName)).ValueAsInt32(false);
                //if (unitEvents_WarningRecordsCount > 0)
                //{
                //    if (count > unitEvents_WarningRecordsCount)
                //    {
                //        CaseInsensitiveOrderedDictionary<string?> params_ = new()
                //        {
                //            { @"EventType", @"UnitEvents_WarningRecordsCount" }
                //        };

                //        _informationSecurityEventsLogger.InformationSecurityEvent("System",
                //                        @"127.0.0.1",
                //                        @"localhost",
                //                        InformationSecurityEventsLogger.WarningRecordsCount_EventId,
                //                        9,
                //                        false,
                //                        Common.Properties.Resources.UnitEvents_WarningRecordsCount_EventName,
                //                        @"System",
                //                        @"UnitEvents",
                //                        NameValueCollectionHelper.GetNameValueCollectionString(params_),
                //                        Common.Properties.Resources.UnitEvents_WarningRecordsCount_EventDesc);

                //        EventMessagesCollection eventMessagesCollection = new();
                //        eventMessagesCollection.EventMessages.Add(new EventMessage(new Ssz.Utils.DataAccess.EventId())
                //        {
                //            EventType = EventType.SimpleAlarm,
                //            Fields = params_
                //        });
                //        _mainServerWorker.LocalDataAccessProvider.NotifyEventMessages(eventMessagesCollection);
                //    }
                //}                
            }
            if (unitEvents_MaxStorageTime > TimeSpan.Zero)
            {
                DateTime timeUtc = DateTime.UtcNow - unitEvents_MaxStorageTime;
                await dbContext.UnitEventsIntervals.Where(i => i.LoadTimeUtc < timeUtc).ExecuteDeleteAsync();
            }
        }

        public void Close()
        {
            _mainServerWorker.DataAccessProvider.EventMessagesCallback -= DataAccessProvider_OnEventMessagesCallback;

            _longRunningTask.Close();
        }

        #endregion

        #region private functions               

        private async Task LongRunning_TaskMainAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(1000, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _longRunningTask.ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, @"Monitoring.LongRunning_TaskMainAsync(...) Exception");
                }
            }
        }

        private void DataAccessProvider_OnEventMessagesCallback(object? sender, EventMessagesCallbackEventArgs args)
        {
            if (!ConfigurationHelper.IsMainProcess(_configuration))
                return;

            var eventMessagesCollection = args.EventMessagesCollection;

            _longRunningTask.ThreadSafeDispatcher.BeginInvokeEx(
                async cancellationToken =>
                {
                    string? eventsSource = eventMessagesCollection.CommonFields?.TryGetValue(@"EventsSource");
                    if (String.IsNullOrEmpty(eventsSource))
                        return;

                    EventMessagesProcessingAddonBase? eventMessagesProcessingAddon = _addonsManager.Addons.OfType<EventMessagesProcessingAddonBase>().FirstOrDefault(a => a.CanProcessEventMessageFrom(eventsSource));                    

                    UnitEventsInterval? unitEventsInterval = null;
                    if (eventMessagesProcessingAddon is not null)
                        unitEventsInterval = await eventMessagesProcessingAddon.ProcessEventMessagesAsync(eventsSource, eventMessagesCollection.EventMessages, CancellationToken.None, null);                    

                    string? unitIdentifierLower = eventMessagesCollection.CommonFields?.TryGetValue(@"UnitIdentifier")?.ToLowerInvariant();
                    if (String.IsNullOrEmpty(unitIdentifierLower))
                        return;

                    await using var dbContext = _dbContextFactory.CreateDbContext();
                    Unit unit;
                    try
                    {
                        unit = dbContext.Units.Single(u => u.IdentifierLower == unitIdentifierLower);
                    }
                    catch
                    {
                        _logger.LogError("Invali unitId: {0}", unitIdentifierLower);
                        return;
                    }

                    var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                    string arg = NameValueCollectionHelper.GetNameValueCollectionString(
                    [
                        (PazCheckConstants.ParamName_UnitIdentifier, unit.Identifier),                        
                    ]);
                    PazCheckDbHelper.AddOrUpdateMetaParam(
                        dbContext,
                        metaParams,
                        paramName: PazCheckDbHelper.GetMetaParamName([PazCheckConstants.MetaParamNameBase_Diagnost_UnitEventsAutoLoaded, unit.Identifier]),
                        paramValue: Guid.NewGuid().ToString(),
                        paramType: @"",
                        isTemp: false, // Not deleted periodically
                        group: @"",
                        method: @"",
                        hasArg: true,
                        excludeConnectionIds: @"",
                        arg: arg);                    

                    string user = Common.Properties.Resources.ObjectSystem;

                    Common.FileContentType contentType =
                                new() { Id = @"EventsJournal", Desc = Common.Properties.Resources.EventsJournal_ContentType };
                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                                    @"127.0.0.1",
                                    @"localhost",
                                    InformationSecurityEventsLogger.DataImported_AllRolesAccessEventId,
                                    6,
                                    true,
                                    Common.Properties.Resources.ImportRemoteFile_EventName,
                                    @"System",
                                    unit.Title,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                    {
                                                (@"ContentType", contentType.Id)
                                    }),
                                    Common.Properties.Resources.ImportRemoteFile_EventDesc, contentType.Desc);

                    if (unitEventsInterval is not null)
                    {
                        string? sourceAddonInstanceId = eventMessagesCollection.CommonFields?.TryGetValue(@"SourceAddonInstanceId");
                        if (!String.IsNullOrEmpty(sourceAddonInstanceId))
                        {
                            try
                            {
                                var t = ((DataAccessProviderBase)sender!).PassthroughAsync(sourceAddonInstanceId, @"SetAddonVariables", Encoding.UTF8.GetBytes(
                                    NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveOrderedDictionary<string?>()
                                    {
                                    { @"%(MaxProcessedTimeUtc)", new Any(unitEventsInterval.EndTimeUtc).ValueAsString(false) }
                                    })
                                ));
                            }
                            catch
                            {
                            }
                        }
                        unitEventsInterval.Unit = unit; // For performance
                        unitEventsInterval.Source = eventMessagesCollection.CommonFields?.TryGetValue(@"EventsSourceToDisplay") ?? Properties.Resources.Autoloading;
                        unit.UnitEventsIntervals.Add(unitEventsInterval);
                    }                        
                    
                    await dbContext.SaveChangesAsync();

                    CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.Addons.OfType<CeMatrixRuntimeAddonBase>().FirstOrDefault();
                    if (ceMatrixRuntimeAddon is null || unit.ActiveProjectVersionId is null)
                        return;                        
                                   
                    if (unitEventsInterval is not null)
                    {
                        var data = ceMatrixRuntimeAddon.CsvDb.GetData(CeMatrixRuntimeAddonBase.UrgentTagsFileName);
                        var urgentTags = data.Keys.Where(k => k != @"").ToArray();

                        UnitEvent? urgentUnitEvent = unitEventsInterval.UnitEvents
                            .Where(e => urgentTags.Contains(e.TagName) && e.ConditionIsActive == true)
                            .FirstOrDefault();
                        if (urgentUnitEvent is null)
                            return;

                        if (_applicationStopping_CancellationToken.IsCancellationRequested)
                            return;

                        var jobId = Guid.NewGuid().ToString();

                        _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.CalculateResults_JobTitle, user,
                            async (cancellationToken, jobProgress) =>
                            {
                                CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                                if (ceMatrixRuntimeAddon is null)
                                {
                                    _logger.LogError("Invalid addon Type: {0}", "CeMatrixRuntimeAddonBase");
                                    if (jobProgress is not null)
                                        await jobProgress.SetJobProgressAsync(null, null, null, StatusCodes.BadInvalidArgument);
                                    return;
                                }

                                bool succeeded = false;
                                int projectId = 0;
                                string projectTitle = @"";
                                DateTime beginTimeUtc = unitEventsInterval.BeginTimeUtc;
                                DateTime endTimeUtc = unitEventsInterval.EndTimeUtc;

                                Result? result = null;

                                try
                                {
                                    await using var dbContext = _dbContextFactory.CreateDbContext();

                                    ProjectVersion activeProjectVersion;
                                    try
                                    {
                                        activeProjectVersion = dbContext.ProjectVersions
                                                        .Include(pv => pv.Project)
                                                        .Single(pv => pv.Id == unit.ActiveProjectVersionId.Value);
                                    }
                                    catch
                                    {
                                        _logger.LogError("Invalid ProjectVersion.Id: {0}", unit.ActiveProjectVersionId.Value);
                                        if (jobProgress is not null)
                                            await jobProgress.SetJobProgressAsync(null, null, null, StatusCodes.BadInvalidArgument);
                                        return;
                                    }

                                    Project project = activeProjectVersion.Project;
                                    projectId = project.Id;
                                    projectTitle = project.Title;

                                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                                            @"127.0.0.1",
                                            @"localhost",
                                            InformationSecurityEventsLogger.Calculation_AllRolesAccessEventId,
                                            6,
                                            true,
                                            Properties.Resources.DiagnostCalculation_AutoBegin_Event,
                                            user,
                                            Common.Properties.Resources.Project + @": " + projectTitle,
                                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                                {
                                                (@"Action", @"DiagnostAnalysis"),
                                                (@"ProjectId", new Any(projectId).ValueAsString(false)),
                                                (@"BeginTimeUtc", new Any(beginTimeUtc).ValueAsString(false)),
                                                (@"EndTimeUtc", new Any(endTimeUtc).ValueAsString(false)),
                                                }),
                                            Properties.Resources.DiagnostCalculation_AutoBegin_EventDesc, projectTitle, beginTimeUtc, endTimeUtc);

                                    var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());

                                    result = await ceMatrixRuntimeAddon.CalculateResultsAsync(_dbContextFactory,
                                        project.Id,
                                        activeProjectVersion.VersionNum,
                                        beginTimeUtc,
                                        endTimeUtc,
                                        user,
                                        _cache.DbCache,
                                        _jobsManager,
                                        cancellationToken,
                                        jobProgress,
                                        loggersSet
                                        );

                                    succeeded = loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Error, count_LogLevel_GreaterThanOrEqualTo: true) == 0;

                                    if (result is not null)
                                    {
                                        CaseInsensitiveOrderedDictionary<string?> argDicitionary = new()
                                        {
                                        { PazCheckConstants.CriterionName_ResultId, result.Id.ToString() },
                                        { PazCheckConstants.ParamName_UnitIdentifier, unit.Identifier },
                                        { @"AnalysisTime", new Any(result.AlalysisTimeUtc.ToLocalTime()).ValueAsString(true, @"g") },
                                        { @"BeginTime", new Any(result.BeginTimeUtc.ToLocalTime()).ValueAsString(true, @"G") },
                                        { @"EndTime", new Any(result.EndTimeUtc.ToLocalTime()).ValueAsString(true, @"G") },
                                        };

                                        foreach (var kvp in result.StatisticsDictionary)
                                        {
                                            argDicitionary[kvp.Key] = kvp.Value;
                                        }

                                        metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                                        string arg = NameValueCollectionHelper.GetNameValueCollectionString(argDicitionary);
                                        PazCheckDbHelper.AddOrUpdateMetaParam(
                                            dbContext,
                                            metaParams,
                                            paramName: PazCheckDbHelper.GetMetaParamName([ PazCheckConstants.MetaParamNameBase_Diagnost_UnitEventsAutoAnalyzed, unit.Identifier ]),
                                            paramValue: Guid.NewGuid().ToString(),
                                            paramType: @"",
                                            isTemp: true, // Deleted periodically
                                            group: @"",
                                            method: @"",
                                            hasArg: true,
                                            excludeConnectionIds: @"",
                                            arg: arg);

                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, @"CalculateResults error. projectId: {0}", projectId);
                                }
                                finally
                                {
                                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                                            @"127.0.0.1",
                                            @"localhost",
                                            InformationSecurityEventsLogger.Calculation_AllRolesAccessEventId,
                                            3,
                                            succeeded,
                                            Properties.Resources.DiagnostCalculation_Event,
                                            user,
                                            Common.Properties.Resources.Project + @": " + projectTitle,
                                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                                {
                                                (@"Action", @"DiagnostAnalysis"),
                                                (@"ProjectId", new Any(projectId).ValueAsString(false)),
                                                (@"BeginTimeUtc", new Any(beginTimeUtc).ValueAsString(false)),
                                                (@"EndTimeUtc", new Any(endTimeUtc).ValueAsString(false)),
                                                }),
                                            Properties.Resources.DiagnostCalculation_EventDesc, projectTitle, beginTimeUtc, endTimeUtc);
                                }
                            });
                    }                                                                            
                });
        }

        #endregion

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;        
        private readonly AddonsManager _addonsManager;
        private readonly Cache _cache;
        private readonly JobsManager _jobsManager;        
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        private CaseInsensitiveOrderedDictionary<string?> _addonOptionsSubstituted = null!;

        private DateTime _lastDoWorkDateTimeUtc;

        private readonly LongRunningTask _longRunningTask;

        #endregion
    }
}