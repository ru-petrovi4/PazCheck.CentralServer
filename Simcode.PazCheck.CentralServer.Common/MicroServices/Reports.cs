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
using System.Text.Json;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Abstractions;
using System.Text.RegularExpressions;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices;

public class Reports : IDisposable
{
    #region construction and destruction

    public Reports(
        IServiceProvider serviceProvider,
        IConfiguration configuration,        
        IDbContextFactory<PazCheckDbContext> dbContextFactory,
        AddonsManager addonsManager,
        Cache cache,        
        IHostApplicationLifetime applicationLifetime,
        IInformationSecurityEventsLogger informationSecurityEventsLogger,        
        ILogger<Reports> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;        
        _dbContextFactory = dbContextFactory;
        _addonsManager = addonsManager;
        _cache = cache;        
        _informationSecurityEventsLogger = informationSecurityEventsLogger;
        _logger = logger;
        _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

        _workingTask = WorkingTaskMainAsync(_workingTask_CancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _workingTask_CancellationTokenSource.Cancel();
    }

    #endregion

    #region public functions

    public void Subscribe(UserNotificationSubscriptionInfo userNotificationSubscriptionInfo)
    {
        Filter? sourceEvent_PreparedFilter = null;
        if (userNotificationSubscriptionInfo is EventOccurs_UserNotificationSubscriptionInfo eventOccurs_UserNotificationSubscriptionInfo)
        {
            if (eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_FilterInfo is not null)
            {
                sourceEvent_PreparedFilter = FilterHelper.Create(eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_FilterInfo);
                FilterHelper.Prepare(sourceEvent_PreparedFilter, _cache.DbCache);
            }
        }
        else if (userNotificationSubscriptionInfo is EventDoesNotOccur_UserNotificationSubscriptionInfo eventDoesNotOccur_UserNotificationSubscriptionInfo)
        {
            if (eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_FilterInfo is not null)
            {
                sourceEvent_PreparedFilter = FilterHelper.Create(eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_FilterInfo);
                FilterHelper.Prepare(sourceEvent_PreparedFilter, _cache.DbCache);
            }
        }

        lock (_userNotificationSubscriptionsCollection)
        {
            _userNotificationSubscriptionsCollection[userNotificationSubscriptionInfo.SubscriptionId] =
                new UserNotificationSubscription()
                {
                    UserNotificationSubscriptionInfo = userNotificationSubscriptionInfo,
                    SourceEvent_PreparedFilter = sourceEvent_PreparedFilter,
                };
        }        
    }

    public void Unsubscribe(string subscriptionId)
    {
        lock (_userNotificationSubscriptionsCollection)
        {
            _userNotificationSubscriptionsCollection.Remove(subscriptionId);
        }
    }

    public async Task TestAsync(string testOptions, ILoggersSet loggersSet)
    {
        await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
        readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var metaParamsList = await readOnlyDbContext.MetaParams.ToListAsync();

        UserNotificationSubscription[] userNotificationSubscriptions;
        lock (_userNotificationSubscriptionsCollection)
        {
            userNotificationSubscriptions = _userNotificationSubscriptionsCollection.Values.ToArray();
        }
        foreach (var userNotificationSubscription in userNotificationSubscriptions)
        {   
            CaseInsensitiveOrderedDictionary<string?>? sourceEvent_Fields = null;
            if (userNotificationSubscription.UserNotificationSubscriptionInfo is TimePoint_UserNotificationSubscriptionInfo timePoint_UserNotificationSubscriptionInfo)
            {                
            }
            else if (userNotificationSubscription.UserNotificationSubscriptionInfo is EventOccurs_UserNotificationSubscriptionInfo eventOccurs_UserNotificationSubscriptionInfo)
            {
                foreach (MetaParam eventMetaParam in metaParamsList
                    .Where(p => PazCheckDbHelper.IsMatchCaseInsensitive(p.ParamName, eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_Type, eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_Type_Regex))
                    .OrderByDescending(p => p._LastChangeTimeUtc))
                {
                    CaseInsensitiveOrderedDictionary<string?>? arg = null;

                    if (eventMetaParam.HasArg)
                    {
                        var metaParamArg = readOnlyDbContext.MetaParamArgs.FirstOrDefault(a => a.ParamName == eventMetaParam.ParamName);
                        if (metaParamArg is not null)
                            arg = NameValueCollectionHelper.Parse(metaParamArg.Arg);
                    }

                    if (Filter(arg, userNotificationSubscription.SourceEvent_PreparedFilter))
                    {
                        sourceEvent_Fields = arg;                        
                        break;
                    }
                }
            }
            else if (userNotificationSubscription.UserNotificationSubscriptionInfo is EventDoesNotOccur_UserNotificationSubscriptionInfo eventDoesNotOccur_UserNotificationSubscriptionInfo)
            {
                foreach (MetaParam eventMetaParam in metaParamsList
                    .Where(p => PazCheckDbHelper.IsMatchCaseInsensitive(p.ParamName, eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_Type, eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_Type_Regex))
                    .OrderByDescending(p => p._LastChangeTimeUtc))
                {
                    CaseInsensitiveOrderedDictionary<string?>? arg = null;

                    if (eventMetaParam.HasArg)
                    {
                        var metaParamArg = readOnlyDbContext.MetaParamArgs.FirstOrDefault(a => a.ParamName == eventMetaParam.ParamName);
                        if (metaParamArg is not null)
                            arg = NameValueCollectionHelper.Parse(metaParamArg.Arg);
                    }

                    if (Filter(arg, userNotificationSubscription.SourceEvent_PreparedFilter))
                    {
                        sourceEvent_Fields = arg;                        
                        break;
                    }
                }
            }

            // Each constant starts with '%(' and ends with ')'.
            CaseInsensitiveOrderedDictionary<string?> sourceEvent_Constants;
            if (sourceEvent_Fields is not null)
                sourceEvent_Constants = SszQueryHelper.FieldsToConstants(sourceEvent_Fields);
            else
                sourceEvent_Constants = new();            

            List<string> debugInfo = sourceEvent_Constants.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList();
            userNotificationSubscription.UserNotificationSubscriptionInfo.AddDebugInfo(debugInfo);

            DateTime nowUtc = DateTime.UtcNow;
            GetCurrentPeriodBeginDateTimeUtc(
                        nowUtc - TimeSpan.FromSeconds(60),
                        userNotificationSubscription.UserNotificationSubscriptionInfo.PeriodicEvents_Period,
                        nowUtc,
                        debugInfo); // Call for debug info only

            await NotifyUsersAsync(
                    userNotificationSubscription.UserNotificationSubscriptionInfo,
                    sourceEvent_Constants,
                    CancellationToken.None,
                    loggersSet,
                    debugInfo,
                    true,
                    testOptions);
        }
    }

    #endregion

    #region private functions  

    private async Task WorkingTaskMainAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1000, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                await DoWorkAsync(DateTime.UtcNow, cancellationToken);
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, @"ServerContext Callback Thread Exception");
            }
        }

        _logger.LogDebug(@"ServerContext Callback Thread Exit");
    }

    private async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (!ConfigurationHelper.IsMainProcess(_configuration))
            return;

        await using var readOnlyDbContext = _dbContextFactory.CreateDbContext(); var dbContext = _dbContextFactory.CreateDbContext();
        bool dbContextIsChanged = false;

        var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);

        //var original_ReportSubscription_MetaParamsList = metaParamsList.Where(mp => mp.S);

        UserNotificationSubscription[] userNotificationSubscriptions;
        lock (_userNotificationSubscriptionsCollection)
        {
            userNotificationSubscriptions = _userNotificationSubscriptionsCollection.Values.ToArray();
        }
        foreach (var userNotificationSubscription in userNotificationSubscriptions)
        {
            string paramName = PazCheckDbHelper.GetMetaParamName([ PazCheckConstants.MetaParamNameBase_ReportSubscription, userNotificationSubscription.UserNotificationSubscriptionInfo.SubscriptionId ]);
            var metaParam = metaParams.GetValueOrDefault(paramName);
            if (metaParam is null)
            {
                metaParam = new MetaParam
                {
                    ParamName = paramName,
                    Type = PazCheckConstants.MetaParam_Type_MainProcess_ReportSubscription,
                    _LastChangeTimeUtc = DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
                    IsTemp = false,
                    HasArg = false,
                };
                dbContext.MetaParams.Add(metaParam);
                metaParams.Add(paramName, metaParam);
                dbContextIsChanged = true;
            }            

            DateTime? periodicEvents_BeginDateTimeUtc = null;
            CaseInsensitiveOrderedDictionary<string?>? sourceEvent_Fields = null;
            if (userNotificationSubscription.UserNotificationSubscriptionInfo is TimePoint_UserNotificationSubscriptionInfo timePoint_UserNotificationSubscriptionInfo)
            {
                periodicEvents_BeginDateTimeUtc = timePoint_UserNotificationSubscriptionInfo.PeriodicEvents_BeginDateTimeUtc;
            }
            else if (userNotificationSubscription.UserNotificationSubscriptionInfo is EventOccurs_UserNotificationSubscriptionInfo eventOccurs_UserNotificationSubscriptionInfo)
            {                
                foreach (MetaParam eventMetaParam in metaParams.Values
                    .Where(p => PazCheckDbHelper.IsMatchCaseInsensitive(p.ParamName, eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_Type, eventOccurs_UserNotificationSubscriptionInfo.SourceEvent_Type_Regex))
                    .OrderByDescending(p => p._LastChangeTimeUtc))
                {
                    CaseInsensitiveOrderedDictionary<string?>? arg = null; 

                    if (eventMetaParam.HasArg)
                    {
                        var metaParamArg = dbContext.MetaParamArgs.FirstOrDefault(a => a.ParamName == eventMetaParam.ParamName);
                        if (metaParamArg is not null)
                            arg = NameValueCollectionHelper.Parse(metaParamArg.Arg);
                    }

                    if (Filter(arg, userNotificationSubscription.SourceEvent_PreparedFilter))
                    {
                        sourceEvent_Fields = arg;
                        periodicEvents_BeginDateTimeUtc = eventMetaParam._LastChangeTimeUtc;
                        break;
                    }                    
                }                    
            }
            else if (userNotificationSubscription.UserNotificationSubscriptionInfo is EventDoesNotOccur_UserNotificationSubscriptionInfo eventDoesNotOccur_UserNotificationSubscriptionInfo)
            {
                foreach (MetaParam eventMetaParam in metaParams.Values
                    .Where(p => PazCheckDbHelper.IsMatchCaseInsensitive(p.ParamName, eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_Type, eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_Type_Regex))
                    .OrderByDescending(p => p._LastChangeTimeUtc))
                {
                    CaseInsensitiveOrderedDictionary<string?>? arg = null;

                    if (eventMetaParam.HasArg)
                    {
                        var metaParamArg = dbContext.MetaParamArgs.FirstOrDefault(a => a.ParamName == eventMetaParam.ParamName);
                        if (metaParamArg is not null)
                            arg = NameValueCollectionHelper.Parse(metaParamArg.Arg);
                    }

                    if (Filter(arg, userNotificationSubscription.SourceEvent_PreparedFilter))
                    {
                        sourceEvent_Fields = arg;
                        periodicEvents_BeginDateTimeUtc = eventMetaParam._LastChangeTimeUtc + eventDoesNotOccur_UserNotificationSubscriptionInfo.SourceEvent_AllowedTimeSpan;
                        break;
                    }
                }
            }

            if (periodicEvents_BeginDateTimeUtc.HasValue)
            {
                // Each constant starts with '%(' and ends with ')'
                CaseInsensitiveOrderedDictionary<string?> sourceEvent_Constants;
                if (sourceEvent_Fields is not null)
                    sourceEvent_Constants = SszQueryHelper.FieldsToConstants(sourceEvent_Fields);
                else
                    sourceEvent_Constants = new();

                List<string> debugInfo = sourceEvent_Constants.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList();
                userNotificationSubscription.UserNotificationSubscriptionInfo.AddDebugInfo(debugInfo);

                DateTime? currentPeriodBeginDateTimeUtc = GetCurrentPeriodBeginDateTimeUtc(
                        periodicEvents_BeginDateTimeUtc.Value,
                        userNotificationSubscription.UserNotificationSubscriptionInfo.PeriodicEvents_Period,
                        nowUtc,
                        debugInfo);
                if (currentPeriodBeginDateTimeUtc.HasValue &&
                        metaParam._LastChangeTimeUtc < currentPeriodBeginDateTimeUtc.Value &&
                        nowUtc > currentPeriodBeginDateTimeUtc.Value)
                {
                    // Need processing
                    metaParam.Value = Guid.NewGuid().ToString();
                    metaParam._LastChangeTimeUtc = nowUtc;
                    dbContextIsChanged = true;                    

                    try
                    {
                        await NotifyUsersAsync(
                            userNotificationSubscription.UserNotificationSubscriptionInfo, 
                            sourceEvent_Constants, 
                            cancellationToken,
                            null,
                            debugInfo,
                            false,
                            null);
                    }
                    catch
                    {

                    }                    
                }
            }
        }

        if (dbContextIsChanged)
            await dbContext.SaveChangesAsync();
    }    

    private static DateTime? GetCurrentPeriodBeginDateTimeUtc(
        DateTime periodicEvents_BeginDateTimeUtc, 
        string? periodicEvents_Period, 
        DateTime nowUtc,
        List<string> debugInfo)
    {
        if (String.IsNullOrEmpty(periodicEvents_Period))
        {
            debugInfo.Add($"Notification Period: not periodic");
            return periodicEvents_BeginDateTimeUtc;
        }

        TimeSpan periodTimeSpan = new Any(periodicEvents_Period).ValueAs<TimeSpan>(false);
        debugInfo.Add($"Notification Period: {new Any(periodTimeSpan).ValueAsString(false)}");
        if (periodTimeSpan.TotalSeconds < 1.0)
            return periodicEvents_BeginDateTimeUtc;

        if (nowUtc < periodicEvents_BeginDateTimeUtc)
            return null;

        DateTime? currentPeriodBeginDateTimeUtc = null;

        if (periodicEvents_Period.Trim().EndsWith("M"))
        {
            var periodDigitsString = periodicEvents_Period.Substring(0, periodicEvents_Period.Length - 1).Trim();
            if (periodDigitsString.All(ch => Char.IsDigit(ch)))
            {
                int monthCount = new Any(periodDigitsString).ValueAsInt32(false);
                int monthsSinceBegin = ((nowUtc.Year - periodicEvents_BeginDateTimeUtc.Year) * 12) + (nowUtc.Month - periodicEvents_BeginDateTimeUtc.Month);
                int countSinceBegin = monthsSinceBegin / monthCount;
                currentPeriodBeginDateTimeUtc = periodicEvents_BeginDateTimeUtc.AddMonths(countSinceBegin * monthCount);

                // Проверяем, если текущий момент перед началом текущего периода, берем предыдущий
                if (nowUtc < currentPeriodBeginDateTimeUtc.Value)
                {
                    currentPeriodBeginDateTimeUtc = currentPeriodBeginDateTimeUtc.Value.AddMonths(-monthCount);
                }
            }
        }

        if (currentPeriodBeginDateTimeUtc is null)
        {
            long countSinceBegin = (long)((new DateTimeOffset(nowUtc).ToUnixTimeSeconds() - new DateTimeOffset(periodicEvents_BeginDateTimeUtc).ToUnixTimeSeconds()) / periodTimeSpan.TotalSeconds);
            currentPeriodBeginDateTimeUtc = periodicEvents_BeginDateTimeUtc + periodTimeSpan * countSinceBegin;
        }

        debugInfo.Add($"CurrentPeriodBeginDateTimeUtc: {new Any(currentPeriodBeginDateTimeUtc).ValueAsString(false)}");

        return currentPeriodBeginDateTimeUtc;
    }

    private bool Filter(CaseInsensitiveOrderedDictionary<string?>? paramsDictionary, Filter? preparedFilter)
    {
        bool allCriteriaInfosList = true;

        var criterionCollection = preparedFilter?.CriterionCollection?.FirstOrDefault();
        if (criterionCollection is not null)
        {
            foreach (var criteriaInfo in criterionCollection)
            {
                bool anyParams = false;

                foreach (var paramDesc in criteriaInfo.Temp_ParamDescs!)
                {
                    string? value;
                    if (paramsDictionary is not null &&
                            paramsDictionary.TryGetValue(paramDesc.Id, out value) &&
                            SszOperatorHelper.Compare(
                                value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList,
                                paramDesc.DataType))
                    {
                        anyParams = true;
                        break;
                    }
                }

                if (!anyParams)
                {
                    allCriteriaInfosList = false;
                    break;
                }
            }
        }                           

        return allCriteriaInfosList;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="userNotificationSubscriptionInfo"></param>
    /// <param name="sourceEvent_Constants">Each constant starts with '%(' and ends with ')'</param>
    /// <param name="cancellationToken"></param>
    /// <param name="loggersSet"></param>
    /// <param name="debugInfo"></param> 
    /// <param name="isTestMode"></param>
    /// <param name="testOptions"></param> 
    /// <returns></returns>
    private async Task NotifyUsersAsync(
        UserNotificationSubscriptionInfo userNotificationSubscriptionInfo,
        CaseInsensitiveOrderedDictionary<string?> sourceEvent_Constants, 
        CancellationToken cancellationToken,
        ILoggersSet? loggersSet,
        List<string> debugInfo,
        bool isTestMode,
        string? testOptions)
    {
        if (userNotificationSubscriptionInfo.EventHandler is null)
            return;

        ILoggersSet localLoggersSet;
        if (loggersSet is not null)
            localLoggersSet = loggersSet;
        else
            localLoggersSet = new LoggersSet(_logger, _serviceProvider.GetRequiredService<UserEventsLogger>());

        var getConstantValue =
            (string c, IterationInfo iterationInfo) =>
            {
                string? result = sourceEvent_Constants.TryGetValue(c);
                if (!String.IsNullOrEmpty(result))
                    return result;

                using var constantScope = localLoggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_Constant, c));
                if (isTestMode)
                    localLoggersSet.LoggerAndUserFriendlyLogger.LogInformation(Common.Properties.Resources.Error_IdentifierUnknown);
                else
                    localLoggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_IdentifierUnknown);

                return c;
            };

        CaseInsensitiveOrderedDictionary<string?> updatedConstants = new CaseInsensitiveOrderedDictionary<string?>(sourceEvent_Constants);
        if (userNotificationSubscriptionInfo.Constants is not null)
            foreach (var kvp in userNotificationSubscriptionInfo.Constants)
            {
                updatedConstants[kvp.Key] = SszQueryHelper.ComputeValueOfSszQueries(kvp.Value, getConstantValue);
            }

        List<(string, byte[])> attachments = new();
        if (userNotificationSubscriptionInfo.ReportInfos is not null)
        {
            foreach (ReportInfo reportInfo in userNotificationSubscriptionInfo.ReportInfos)
            {
                IReportsExportAddon? reportsExportAddon = _addonsManager.AddonsThreadSafe.FirstOrDefault(a => String.Equals(a.Identifier, reportInfo.AddonIdentifier)) as IReportsExportAddon;
                if (reportsExportAddon is null)
                {
                    localLoggersSet.Logger.LogError($"Cannot find addon with identifier {reportInfo.AddonIdentifier} that implements interface IReportsExportAddon. Report skipped.");
                    localLoggersSet.UserFriendlyLogger.LogError(Properties.Resources.InvalidAddonIdentifier, reportInfo.AddonIdentifier);
                    continue;
                }
                try
                {  
                    Filter filter = FilterHelper.Create(reportInfo.FilterInfo, v =>
                    {
                        return SszQueryHelper.ComputeValueOfSszQueries(v, getConstantValue);
                    });

                    // Can add constants
                    attachments.AddRange(await reportsExportAddon.ExportReportAsync(
                        _dbContextFactory,
                        _cache.DbCache,                        
                        reportInfo.DestinationTypeIdentifier,
                        filter,
                        updatedConstants,
                        cancellationToken,
                        localLoggersSet));
                }
                catch (Exception ex)
                {
                    localLoggersSet.Logger.LogError(ex, $"ExportReport failed. AddonIdentifier: {reportInfo.AddonIdentifier}; DestinationTypeIdentifier: {reportInfo.DestinationTypeIdentifier}");
                    localLoggersSet.UserFriendlyLogger.LogError(ex,
                        Common.Properties.Resources.Error_ExportReport + "\n" +
                        Common.Properties.Resources.Error_SeeApplicationLogFilesForDetails);
                }
            }
        }

        await userNotificationSubscriptionInfo.EventHandler(this, new UserNotificationEventArgs()
        {
            SubscriptionId = userNotificationSubscriptionInfo.SubscriptionId,
            Constants = updatedConstants,
            Attachments = attachments.ToArray(),
            LoggersSet = loggersSet,
            DebugInfo = debugInfo,
            IsTestMode = isTestMode,
            TestOptions = testOptions,            
        });
    }

    #endregion

    #region private fields

    private readonly IServiceProvider _serviceProvider;

    private readonly IConfiguration _configuration;
    
    private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;

    private readonly AddonsManager _addonsManager;
    private readonly Cache _cache;    

    private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
    private readonly ILogger _logger;
    private readonly CancellationToken _applicationStopping_CancellationToken;

    private Task _workingTask = null!;

    private readonly CancellationTokenSource _workingTask_CancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    ///     [SubscriptionId, UserNotificationSubscription]
    /// </summary>
    private readonly Dictionary<string, UserNotificationSubscription> _userNotificationSubscriptionsCollection = new();

    #endregion

    private class UserNotificationSubscription
    {
        public UserNotificationSubscriptionInfo UserNotificationSubscriptionInfo = null!;

        public Filter? SourceEvent_PreparedFilter;
    }
}

public class UserNotificationEventArgs : EventArgs
{
    public string SubscriptionId { get; init; } = null!;    

    /// <summary>
    ///     Each constant starts with '%(' and ends with ')'
    /// </summary>
    public CaseInsensitiveOrderedDictionary<string?> Constants { get; init; } = null!;

    public (string, byte[])[] Attachments { get; init; } = null!;

    /// <summary>
    ///     Use this LoggersSet if not null
    /// </summary>
    public ILoggersSet? LoggersSet { get; init; }

    public List<string> DebugInfo { get; init; } = null!;

    public bool IsTestMode { get; init; }

    /// <summary>
    ///     Test Options if test mode
    /// </summary>
    public string? TestOptions { get; init; }    
}

public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);

public class UserNotificationSubscriptionInfo
{
    public string SubscriptionId { get; init; } = null!;

    public string? PeriodicEvents_Period { get; init; }

    public ReportInfo[]? ReportInfos { get; init; }

    /// <summary>
    ///     Each constant starts with '%(' and ends with ')'.
    /// </summary>
    public CaseInsensitiveOrderedDictionary<string?>? Constants { get; init; }

    public AsyncEventHandler<UserNotificationEventArgs>? EventHandler { get; init; }

    public virtual void AddDebugInfo(List<string> debugInfo)
    {        
    }
}

public class ReportInfo
{
    public string AddonIdentifier { get; set; } = @"";

    public string DestinationTypeIdentifier { get; set; } = @"";

    public List<CaseInsensitiveOrderedDictionary<List<string>>>? FilterInfo { get; set; }
}

public class TimePoint_UserNotificationSubscriptionInfo : UserNotificationSubscriptionInfo
{
    public DateTime PeriodicEvents_BeginDateTimeUtc { get; init; }

    public override void AddDebugInfo(List<string> debugInfo)
    {
        base.AddDebugInfo(debugInfo);
        
        debugInfo.Add($"SourceEvent_Category: Time");
        debugInfo.Add($"Notification BeginDateTimeUtc: {new Any(PeriodicEvents_BeginDateTimeUtc).ValueAsString(false)}");
    }
}

public class EventOccurs_UserNotificationSubscriptionInfo : UserNotificationSubscriptionInfo
{
    /// <summary>
    ///     Corresponds MetaParam.ParamName
    /// </summary>
    public string SourceEvent_Type { get; init; } = null!;

    /// <summary>
    ///     Corresponds MetaParam.ParamName
    /// </summary>
    public Regex? SourceEvent_Type_Regex { get; init; }

    public List<CaseInsensitiveOrderedDictionary<List<string>>>? SourceEvent_FilterInfo { get; init; }

    public override void AddDebugInfo(List<string> debugInfo)
    {
        base.AddDebugInfo(debugInfo);

        debugInfo.Add($"SourceEvent_Category: Event");        
    }
}

public class EventDoesNotOccur_UserNotificationSubscriptionInfo : UserNotificationSubscriptionInfo
{
    /// <summary>
    ///     Corresponds MetaParam.ParamName
    /// </summary>
    public string SourceEvent_Type { get; init; } = null!;

    /// <summary>
    ///     Corresponds MetaParam.ParamName
    /// </summary>
    public Regex? SourceEvent_Type_Regex { get; init; }

    public List<CaseInsensitiveOrderedDictionary<List<string>>>? SourceEvent_FilterInfo { get; init; }

    public TimeSpan SourceEvent_AllowedTimeSpan { get; init; }

    public override void AddDebugInfo(List<string> debugInfo)
    {
        base.AddDebugInfo(debugInfo);

        debugInfo.Add($"SourceEvent_Category: !Event");
        debugInfo.Add($"SourceEvent_AllowedTimeSpan: {new Any(SourceEvent_AllowedTimeSpan).ValueAsString(false)}");
    }
}


///// <summary>
//    ///     (CriterionInfo1 AND CriterionInfo2) OR (CriterionInfo3 AND CriterionInfo4)
//    /// </summary>
//    public List<CaseInsensitiveOrderedDictionary<List<string>>>? SourceEvent_FilterInfo { get; init; }

//try
//{
//    dbContext.MetaParamHubArgs.Add(new MetaParamHubArg
//    {
//        ParamName = paramName,
//        HubArg = periodicEventInfo.Fields
//    });
//}
//catch
//{
//}

//private async Task NotifyUsersAsync(Result result, string? testMailboxAddressString, DbCache dbCache, ILoggersSet loggersSet)
//    {
//        CaseInsensitiveOrderedDictionary<string?> params_ = new();
//        params_["AnalysisTime"] = new Any(result.AlalysisTimeUtc.ToLocalTime()).ValueAsString(true, @"g");
//        params_["BeginTime"] = new Any(result.BeginTimeUtc.ToLocalTime()).ValueAsString(true, @"G");
//        params_["EndTime"] = new Any(result.EndTimeUtc.ToLocalTime()).ValueAsString(true, @"G");
//        params_["Source"] = result.Source;
//        params_["Comment"] = result.Comment;
//        foreach (var kvp in result.StatisticsDictionary)
//        {
//            params_[kvp.Key] = kvp.Value;
//        }

//        var customImportExportAddon = _addonsManager.AddonsThreadSafe.OfType<CustomImportExportAddonBase>().FirstOrDefault();
//        if (customImportExportAddon is null)
//        {
//            loggersSet.LoggerAndUserFriendlyLogger.LogWarning(@"No plugin: CustomImportExportAddonBase");
//            throw new Exception(Common.Properties.Resources.SeeUserEventsLog);
//        }

//        var exportTypeFilesInfo = customImportExportAddon.GetExportReport_FilesInfos(IReportsExportAddon.ReportType_DiagnostResult).FirstOrDefault();
//        if (exportTypeFilesInfo is null)
//        {
//            loggersSet.LoggerAndUserFriendlyLogger.LogWarning(@"No exportTypeFilesInfo in CustomImportExportAddonBase");
//            throw new Exception(Common.Properties.Resources.SeeUserEventsLog);
//        }

//        // create attachment
//        (string, byte[]) export;
//        using (var readOnlyDbContext = _dbContextFactory.CreateDbContext())
//        {
//            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

//            export = await customImportExportAddon.ExportResultAsync(
//                readOnlyDbContext,
//                dbCache,
//                result,
//                exportTypeFilesInfo.DestinationTypeIdentifier,
//                _applicationStopping_CancellationToken,
//                loggersSet);
//        }

//        var eventMessage = new EventMessage(new Ssz.Utils.DataAccess.EventId()
//        {
//            OccurrenceId = Guid.NewGuid().ToString(),
//        })
//        {
//            EventType = EventType.SystemEvent,
//            OccurrenceTimeUtc = DateTime.UtcNow,
//            Fields = params_
//        };

//        _mainServerWorker.NotifyEventMessages(@"local", // Only for local subscribers (MainServerWorker.DataAccessProvider)
//            new EventMessagesCollection
//            {
//                EventMessages = new List<EventMessage> { eventMessage }
//            });
//    }