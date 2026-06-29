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
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;
using System.Collections.Frozen;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using Ssz.DataAccessGrpc.Client;
using Ssz.Utils.Dispatcher;

namespace Simcode.PazCheck.CentralServer.MicroServices;

public partial class Monitoring
{
    #region construction and destruction

    public Monitoring(
        IMainServerWorker mainServerWorker,
        IDbContextFactory<PazCheckDbContext> dbContextFactory,
        Cache cache,
        IConfiguration configuration,            
        IHostApplicationLifetime applicationLifetime,
        IInformationSecurityEventsLogger informationSecurityEventsLogger,            
        ILogger<Monitoring> logger)
    {
        _mainServerWorker = mainServerWorker;                        
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _configuration = configuration;            
        _informationSecurityEventsLogger = informationSecurityEventsLogger;
        _logger = logger;
        _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;            

        _paramPooledPolicy = new Common.Serialization.ParamPooledPolicy();
        _serializationParamsPool = new SingleThreadObjectPool<Common.Serialization.Param>(_paramPooledPolicy, 100000);            

        _dataAccessProvider = ActivatorUtilities.CreateInstance<GrpcDataAccessProvider>(mainServerWorker.ServiceProvider);

        _longRunningTask = new(_logger);
        Grafana_LongRunningTask = new(_logger);
    }

    #endregion

    #region public functions       

    public readonly LongRunningTask Grafana_LongRunningTask;

    public void Initialize(CaseInsensitiveOrderedDictionary<string?> optionsSubstituted)
    {
        _optionsSubstituted = optionsSubstituted;

        _dataAccessProvider.Initialize(
            null,
            @"local", // Any not null address
            @"local", // Any not null
            @"local", // Workstation name for messages filtering
            @"DCS",
            new CaseInsensitiveOrderedDictionary<string?>(),
            new DataAccessProviderOptions { LocalDataAccessServerWorker = ((MainServerWorker)_mainServerWorker).DataAccessServerWorker },
            _longRunningTask.ThreadSafeDispatcher);

        _longRunningTask.Initialize(LongRunning_TaskMainAsync);
        Grafana_LongRunningTask.Initialize(Grafana_LongRunning_TaskMainAsync);
    }        

    public void Close()
    {
        _longRunningTask.Close();
        Grafana_LongRunningTask.Close();
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

                await DoWorkAsync(DateTime.UtcNow, cancellationToken);
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

    private async Task Grafana_LongRunning_TaskMainAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(200, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                await Grafana_LongRunningTask.ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Monitoring.Grafana_LongRunning_TaskMainAsync(...) Exception");
            }
        }
    }

    private async Task DoWorkAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        TimeSpan updatePeriod = new Any(_optionsSubstituted.TryGetValue(MonitoringAddon.UpdatePeriod_OptionName)).ValueAs<TimeSpan>(false);
        if (nowUtc < _lastDoWorkDateTimeUtc + updatePeriod)
            return;
        _lastDoWorkDateTimeUtc = nowUtc;

        bool cleanUp = false;
        if (nowUtc > _lastCleanUpDateTimeUtc + TimeSpan.FromHours(24))
        {
            cleanUp = true;
            _lastCleanUpDateTimeUtc = nowUtc;
        }

        var newReadOnlyDbContext = _dbContextFactory.CreateDbContext(); // Not using!!!
        newReadOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;

        if (ConfigurationHelper.IsMainProcess(_configuration))
        {
            Stopwatch stopwatch = new Stopwatch();

            var loggersSet = new LoggersSet(_logger, null);

            foreach (var kvp in _pcObject_JournalParam_ValueSubscriptionsDictionary)
            {
                kvp.Value.IsUsed = false;
            }

            List<PcObject> dataChanged_PcObjects = new(1000);

            bool units_BasePcObjects_PcObjects_Changed = false;

            DbCache dbCache = _cache.DbCache;

            var paramDescs_Units_BasePcObjects_PcObjects_Guid = newReadOnlyDbContext.MetaParams.Single(mp => mp.ParamName == PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid).Value;
            if (paramDescs_Units_BasePcObjects_PcObjects_Guid != _dbContext_ReadOnly.ParamDescs_Units_BasePcObjects_PcObjects_Guid)
            {
                units_BasePcObjects_PcObjects_Changed = true;

                stopwatch.Restart();

                _dbContext_ReadOnly.Close();
                await _dbContext_ReadOnly.InitializeAsync(newReadOnlyDbContext, paramDescs_Units_BasePcObjects_PcObjects_Guid);               

                dbCache = _dbContext_ReadOnly.CreateDbCache();                

                // Must be after _pcObjects =
                _dbContext_ToModify.Close();
                var dbContext = _dbContextFactory.CreateDbContext(); // not using!!!
                dbContext.IsInformationSecurityEventsLoggingDisabled = true;
                await _dbContext_ToModify.InitializeAsync(dbContext);

                stopwatch.Stop();
                _logger.LogDebug($"Monitoring.DoWorkAsync(...) dbCache = new(): {stopwatch.ElapsedMilliseconds} ms");
            }
            else
            {
                newReadOnlyDbContext.Dispose();
            }

            stopwatch.Restart();

            _dbContext_ToModify.DbContextChanged = false;

            foreach (var pcObject in dbCache.PcObjectsDictionary2.Values)
            {
                var basePcObjectParamsDictionary = pcObject.BasePcObject.ParamsDictionary;
                var pcObjectParamsDictionary = pcObject.ParamsDictionary;

                _dbContext_ToModify.Current_PcObject = _dbContext_ToModify.PcObjectsDictionary[pcObject.Id];

                // Safety Index K
                double safetyIndexK = PazCheckDbHelper.GetParamValue<double>(
                    pcObjectParamsDictionary,
                    basePcObjectParamsDictionary,
                    PazCheckConstants.ParamName_SafetyIndexK,
                    0.0);
                double safetyIndexK2 = PazCheckDbHelper.GetParamValue<double>(
                    pcObjectParamsDictionary,
                    basePcObjectParamsDictionary,
                    PazCheckConstants.ParamName_SafetyIndexK2,
                    1.0);
                safetyIndexK = safetyIndexK * safetyIndexK2;
                if (Math.Round(pcObject.K, 8) != Math.Round(safetyIndexK, 8))
                {
                    pcObject.K = safetyIndexK;
                    
                    _dbContext_ToModify.Current_PcObject.K = pcObject.K;
                    _dbContext_ToModify.DbContextChanged = true;
                }

                #region JournalParams processing                    

                pcObject.Temp_NewSafetyIndex = -100;

                foreach (BasePcObjectJournalParam basePcObjectJournalParam in pcObject.BasePcObject.JournalParams)
                {
                    PcObjectJournalParam? pcObjectJournalParam = pcObject.JournalParams.FirstOrDefault(jp => String.Equals(jp.ParamName, basePcObjectJournalParam.ParamName, StringComparison.InvariantCultureIgnoreCase));
                    if (pcObjectJournalParam is not null)
                        continue;

                    //localStopwatch.Start();
                    await ProcessTrendAsync(
                            _dbContext_ReadOnly.ReadOnlyDbContext,
                            _dbContext_ToModify,
                            dbCache,
                            pcObject,
                            pcObjectParamsDictionary,
                            basePcObjectParamsDictionary,
                            basePcObjectJournalParam,
                            nowUtc,
                            loggersSet);
                    //localStopwatch.Stop();
                }

                foreach (PcObjectJournalParam pcObjectJournalParam in pcObject.JournalParams)
                {
                    //localStopwatch.Start();
                    await ProcessTrendAsync(
                            _dbContext_ReadOnly.ReadOnlyDbContext,
                            _dbContext_ToModify,
                            dbCache,
                            pcObject,
                            pcObjectParamsDictionary,
                            basePcObjectParamsDictionary,
                            pcObjectJournalParam,
                            nowUtc,
                            loggersSet);
                    //localStopwatch.Stop();
                }

                #endregion

                #region SafetyIndex Update                    

                double newSafetyIndex;
                string newSafetyIndexDesc;
                if (pcObject.Temp_NewSafetyIndex >= PazCheckDbHelper.GetParamValue<double>(
                            pcObjectParamsDictionary,
                            basePcObjectParamsDictionary,
                            PazCheckConstants.ParamName_SafetyIndex_Green_Percentage,
                            99.999999))
                {
                    newSafetyIndex = 100.0;
                    newSafetyIndexDesc = PazCheckDbHelper.GetParamValue(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        PazCheckConstants.ParamName_SafetyIndex_Green_Desc);                        
                }
                else if (pcObject.Temp_NewSafetyIndex >= PazCheckDbHelper.GetParamValue<double>(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        PazCheckConstants.ParamName_SafetyIndex_Yellow_Percentage,
                        99.999999))
                {
                    newSafetyIndex = 50.0;
                    newSafetyIndexDesc = PazCheckDbHelper.GetParamValue(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        PazCheckConstants.ParamName_SafetyIndex_Yellow_Desc);                        
                }
                else if (pcObject.Temp_NewSafetyIndex >= PazCheckDbHelper.GetParamValue<double>(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        PazCheckConstants.ParamName_SafetyIndex_Red_Percentage,
                        -0.000001))
                {
                    newSafetyIndex = 0.0;
                    newSafetyIndexDesc = PazCheckDbHelper.GetParamValue(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        PazCheckConstants.ParamName_SafetyIndex_Red_Desc);                        
                }
                else
                {
                    newSafetyIndex = -100.0;
                    newSafetyIndexDesc = Common.Properties.Resources.SafetyIndexDesc_NoData;                        
                }

                if (Math.Round(pcObject.SafetyIndex, 1) != Math.Round(newSafetyIndex, 1) ||
                    pcObject.SafetyIndexDesc != newSafetyIndexDesc)
                {
                    pcObject.SafetyIndex = newSafetyIndex;                        
                    pcObject.SafetyIndexDesc = newSafetyIndexDesc;
                    pcObject.SafetyIndex_LastChangeTimeUtc = nowUtc;

                    dataChanged_PcObjects.Add(pcObject);

                    _dbContext_ToModify.Current_PcObject.SafetyIndex = pcObject.SafetyIndex;                        
                    _dbContext_ToModify.Current_PcObject.SafetyIndexDesc = pcObject.SafetyIndexDesc;
                    _dbContext_ToModify.Current_PcObject.SafetyIndex_LastChangeTimeUtc = pcObject.SafetyIndex_LastChangeTimeUtc;
                    _dbContext_ToModify.DbContextChanged = true;
                }

                #endregion

                if (cleanUp)
                {
                    foreach (var journalParamValuesCollection in pcObject.JournalParamValuesCollections)
                    {
                        await CleanUpTrendAsync(
                                _dbContext_ReadOnly.ReadOnlyDbContext,
                                journalParamValuesCollection,
                                nowUtc);
                    }
                    _dbContext_ReadOnly.Reset_ParamDescs_Units_BasePcObjects_PcObjects_Guid();
                }
            }

            stopwatch.Stop();
            _logger.LogDebug($"Monitoring.DoWorkAsync(...) ProcessTrendAsync: {stopwatch.ElapsedMilliseconds} ms");

            foreach (var kvp in _pcObject_JournalParam_ValueSubscriptionsDictionary.ToArray())
            {
                var pcObjectTrendSubscription = kvp.Value;
                if (!pcObjectTrendSubscription.IsUsed)
                {
                    pcObjectTrendSubscription.Dispose();
                    _pcObject_JournalParam_ValueSubscriptionsDictionary.Remove(kvp.Key);
                }
            }

            if (dataChanged_PcObjects.Count > 0)
            {
                (string, string?)[] dataChangedMessage = [
                    ( @"Ids", CsvHelper.FormatForCsv(@",", dataChanged_PcObjects.Select(po => po.Id).OfType<object>()) ),
                    ( @"SafetyIndices", CsvHelper.FormatForCsv(@",", dataChanged_PcObjects.Select(po => (float)po.SafetyIndex).OfType<object>()) ),
                    ( @"SafetyIndexDescs", CsvHelper.FormatForCsv(@",", dataChanged_PcObjects.Select(po => po.SafetyIndexDesc)) )
                ];

                var monitoring_Data_MetaParam = _dbContext_ToModify.DbContext.MetaParams.Single(mp => mp.ParamName == PazCheckConstants.MetaParamNameBase_Monitoring_Data);
                monitoring_Data_MetaParam.Value = Guid.NewGuid().ToString();
                monitoring_Data_MetaParam._LastChangeTimeUtc = nowUtc;
                _dbContext_ToModify.DbContext.MetaParamArgs.Single(a => a.ParamName == PazCheckConstants.MetaParamNameBase_Monitoring_Data).Arg =
                    NameValueCollectionHelper.GetNameValueCollectionString(dataChangedMessage);

                _dbContext_ToModify.DbContextChanged = true;
            }

            //_logger.LogDebug($"{localStopwatch.ElapsedMilliseconds} ms");

            if (_dbContext_ToModify.DbContextChanged)
            {
                stopwatch.Restart();

                await _dbContext_ToModify.DbContext.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogDebug($"Monitoring.DoWorkAsync(...) _dbContext.SaveChanges(): {stopwatch.ElapsedMilliseconds} ms");
            }

            if (units_BasePcObjects_PcObjects_Changed)
            {
                _cache.DbCache = dbCache;

                await Sync_BasePcObjects_PcObjects_WithActiveProjectVersionAsync(); // Active ProjectVersion contained in Units.                                     
            }
        }
        else
        {
            var paramDescs_Units_BasePcObjects_PcObjects_Guid = newReadOnlyDbContext.MetaParams.Single(mp => mp.ParamName == PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid).Value;
            var journalParamValuesCollection_Guid = newReadOnlyDbContext.MetaParams.Single(mp => mp.ParamName == PazCheckConstants.MetaParamNameBase_JournalParamValuesCollection_Guid).Value;
            if (paramDescs_Units_BasePcObjects_PcObjects_Guid != _dbContext_ReadOnly.ParamDescs_Units_BasePcObjects_PcObjects_Guid ||
                journalParamValuesCollection_Guid != _journalParamValuesCollection_Guid)
            {                
                _journalParamValuesCollection_Guid = journalParamValuesCollection_Guid;

                _dbContext_ReadOnly.Close();
                await _dbContext_ReadOnly.InitializeAsync(newReadOnlyDbContext, paramDescs_Units_BasePcObjects_PcObjects_Guid);

                DbCache dbCache = _dbContext_ReadOnly.CreateDbCache();

                _cache.DbCache = dbCache;
            }
            else
            {
                newReadOnlyDbContext.Dispose();
            }
        }
    }

    private async Task ProcessTrendAsync(
        PazCheckDbContext readOnlyDbContext,
        DbContext_ToModify dbContext_ToModify,
        DbCache dbCache,            
        PcObject pcObject,
        IReadOnlyDictionary<string, string?> pcObjectParamsDictionary,
        IReadOnlyDictionary<string, string?> basePcObjectParamsDictionary,
        JournalParam journalParam,
        DateTime nowUtc,
        ILoggersSet loggersSet)
    {
        var metadataFieldsDictionary = journalParam.MetadataFieldsDictionary;                        
        var in_ = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_In);
        in_ = pcObject.ComputeValueOfSszQueries(in_, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var converter = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_Converter);
        converter = pcObject.ComputeValueOfSszQueries(converter, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var valueMaxString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValueMax);
        valueMaxString = pcObject.ComputeValueOfSszQueries(valueMaxString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var valueMinString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValueMin);
        valueMinString = pcObject.ComputeValueOfSszQueries(valueMinString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var valueEUString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValueEU);
        valueEUString = pcObject.ComputeValueOfSszQueries(valueEUString, pcObjectParamsDictionary, basePcObjectParamsDictionary);    
        
        var valueUpdatePeriodString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValuePeriod);
        valueUpdatePeriodString = pcObject.ComputeValueOfSszQueries(valueUpdatePeriodString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var valueUpdatePeriod = new Any(valueUpdatePeriodString).ValueAs<TimeSpan>(false);
        var valueUpdateDeadbandPercentString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValueDeadbandPercentage);
        valueUpdateDeadbandPercentString = pcObject.ComputeValueOfSszQueries(valueUpdateDeadbandPercentString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var valueUpdateDeadbandString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_ValueDeadband);
        valueUpdateDeadbandString = pcObject.ComputeValueOfSszQueries(valueUpdateDeadbandString, pcObjectParamsDictionary, basePcObjectParamsDictionary);

        var trendUpdatePeriodString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_TrendPeriod);
        trendUpdatePeriodString = pcObject.ComputeValueOfSszQueries(trendUpdatePeriodString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var trendUpdatePeriod = new Any(trendUpdatePeriodString).ValueAs<TimeSpan>(false);
        var trendUpdateDeadbandPercentString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_TrendDeadbandPercentage);
        trendUpdateDeadbandPercentString = pcObject.ComputeValueOfSszQueries(trendUpdateDeadbandPercentString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        var trendUpdateDeadbandString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_TrendDeadband);
        trendUpdateDeadbandString = pcObject.ComputeValueOfSszQueries(trendUpdateDeadbandString, pcObjectParamsDictionary, basePcObjectParamsDictionary);

        var trendEnabledString = ConfigurationHelper.GetValue<string>(metadataFieldsDictionary, PazCheckConstants.ParamName_TrendEnabled, @"false");
        trendEnabledString = pcObject.ComputeValueOfSszQueries(trendEnabledString, pcObjectParamsDictionary, basePcObjectParamsDictionary);
        bool trendEnabled = new Any(trendEnabledString).ValueAsBoolean(false);
        var trendStorePeriodString = ConfigurationHelper.GetValue<string>(metadataFieldsDictionary, PazCheckConstants.ParamName_TrendStorePeriod, @"1M");
        trendStorePeriodString = pcObject.ComputeValueOfSszQueries(trendStorePeriodString, pcObjectParamsDictionary, basePcObjectParamsDictionary);

        string id = CsvHelper.FormatForCsv(
            @",",
            [   pcObject.Identifier,                    
                in_,
                converter,
                valueMaxString,
                valueMinString,
                valueEUString,
                
                valueUpdatePeriod,
                valueUpdateDeadbandPercentString,
                valueUpdateDeadbandString,

                trendUpdatePeriod,
                trendUpdateDeadbandPercentString,
                trendUpdateDeadbandString,

                trendEnabled,
                trendStorePeriodString
                ]);

        JournalParamValuesCollection? journalParamValuesCollection = pcObject.JournalParamValuesCollections.FirstOrDefault(jp => String.Equals(jp.ParamName, journalParam.ParamName, StringComparison.InvariantCultureIgnoreCase));
        JournalParamValuesCollection? journalParamValuesCollection_ToModify = null;
        if (journalParamValuesCollection is null)
        {
            journalParamValuesCollection = new JournalParamValuesCollection
            {
                ParamName = journalParam.ParamName
            };
            journalParamValuesCollection.PcObject = pcObject;
            pcObject.JournalParamValuesCollections.Add(journalParamValuesCollection);

            journalParamValuesCollection_ToModify = new JournalParamValuesCollection
            {
                ParamName = journalParam.ParamName,
                PcObject = _dbContext_ToModify.Current_PcObject, // Optimization
            };                                
            _dbContext_ToModify.Current_PcObject.JournalParamValuesCollections.Add(journalParamValuesCollection_ToModify);         
            dbContext_ToModify.DbContextChanged = true;
        }

        PcObject_JournalParam_ValueSubscription? pcObject_JournalParam_ValueSubscription;
        if (!_pcObject_JournalParam_ValueSubscriptionsDictionary.TryGetValue(id, out pcObject_JournalParam_ValueSubscription))
        {
            pcObject_JournalParam_ValueSubscription = new PcObject_JournalParam_ValueSubscription(
                dbCache,
                pcObject,
                _dataAccessProvider,                    
                in_,                    
                converter,
                valueMaxString,
                valueMinString,
                valueEUString,                    
                valueUpdatePeriod,
                valueUpdateDeadbandPercentString,
                valueUpdateDeadbandString,
                trendUpdatePeriod,
                trendUpdateDeadbandPercentString,
                trendUpdateDeadbandString,
                trendEnabled,
                trendStorePeriodString);

            _pcObject_JournalParam_ValueSubscriptionsDictionary.Add(id, pcObject_JournalParam_ValueSubscription);
        }
        pcObject_JournalParam_ValueSubscription.IsUsed = true;

        if (!StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp.StatusCode) ||
                nowUtc - pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp.TimestampUtc >= pcObject_JournalParam_ValueSubscription.ValueUpdatePeriod ||
                !StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp.StatusCode) ||
                nowUtc - pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp.TimestampUtc >= pcObject_JournalParam_ValueSubscription.TrendUpdatePeriod)
        {
            float? currentValue = await pcObject_JournalParam_ValueSubscription.CalculateCurrentValueAsync(
                journalParamValuesCollection,
                readOnlyDbContext,
                dbCache,
                _mainServerWorker,
                pcObject,
                nowUtc,
                loggersSet);
            if (currentValue is not null &&
                !float.IsNaN(currentValue.Value) &&
                !float.IsInfinity(currentValue.Value) &&
                (pcObject_JournalParam_ValueSubscription.ValueMaxString == @"" || !StatusCodes.IsUncertain(pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.StatusCode)) &&
                (pcObject_JournalParam_ValueSubscription.ValueMinString == @"" || !StatusCodes.IsUncertain(pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.StatusCode)) &&
                (pcObject_JournalParam_ValueSubscription.ValueEUString == @"" || !StatusCodes.IsUncertain(pcObject_JournalParam_ValueSubscription.EU_ValueSubscription.ValueStatusTimestamp.StatusCode)))
            {
                // !!! Any Param Name that contains 'SafetyIndex' !!!
                if (journalParamValuesCollection.ParamName.Contains(PazCheckConstants.ParamName_SafetyIndex, StringComparison.InvariantCultureIgnoreCase))
                {
                    double safetyIndex = Math.Round(currentValue.Value, 1);
                    if (safetyIndex > 100.0)
                        safetyIndex = 100.0;
                    pcObject.Temp_NewSafetyIndex = safetyIndex;
                }

                // Store journalParamValuesCollection.CurrentValue
                bool storeCurrentValue = false;
                // Store journalParamValuesCollection.FloatValues
                bool storeTrend = false;

                if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp.StatusCode))
                {
                    if (nowUtc - pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp.TimestampUtc >= pcObject_JournalParam_ValueSubscription.ValueUpdatePeriod)
                    {
                        var lastStored_Value = pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp.Value.ValueAsSingle(false);
                        if (pcObject_JournalParam_ValueSubscription.ValueUpdateDeadbandString != @"")
                        {
                            storeCurrentValue = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value, new Any(pcObject_JournalParam_ValueSubscription.ValueUpdateDeadbandString).ValueAsSingle(false));
                        }
                        else if (pcObject_JournalParam_ValueSubscription.ValueUpdateDeadbandPercentString != @"")
                        {
                            if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.StatusCode) &&
                                    StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.StatusCode))
                            {
                                var currentValueDeadband = (pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.Value.ValueAsSingle(false) -
                                    pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.Value.ValueAsSingle(false)) *
                                    new Any(pcObject_JournalParam_ValueSubscription.ValueUpdateDeadbandPercentString).ValueAsSingle(false) / 100.0f;
                                storeCurrentValue = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value, currentValueDeadband);
                            }
                        }
                        else
                        {
                            storeCurrentValue = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value);
                        }
                    }
                }
                else
                {
                    storeCurrentValue = true;
                }
                if (storeCurrentValue)
                {
                    pcObject_JournalParam_ValueSubscription.LastStored_CurrentValue_ValueStatusTimestamp = new ValueStatusTimestamp(new Any(currentValue.Value), StatusCodes.Good, nowUtc);
                    journalParamValuesCollection.CurrentValue_TimestampUtc = new DateTimeOffset(nowUtc).ToUnixTimeMilliseconds();
                    journalParamValuesCollection.CurrentValue = currentValue.Value;

                    if (journalParamValuesCollection_ToModify is null)
                        journalParamValuesCollection_ToModify = _dbContext_ToModify.Current_PcObject.JournalParamValuesCollections
                                .Single(c => String.Equals(c.ParamName, journalParamValuesCollection.ParamName, StringComparison.InvariantCultureIgnoreCase));
                    journalParamValuesCollection_ToModify.CurrentValue_TimestampUtc = new DateTimeOffset(nowUtc).ToUnixTimeMilliseconds();
                    journalParamValuesCollection_ToModify.CurrentValue = currentValue;
                }

                if (pcObject_JournalParam_ValueSubscription.TrendEnabled)
                {                        
                    if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp.StatusCode))
                    {
                        if (nowUtc - pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp.TimestampUtc >= pcObject_JournalParam_ValueSubscription.TrendUpdatePeriod)
                        {
                            var lastStored_Value = pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp.Value.ValueAsSingle(false);
                            if (pcObject_JournalParam_ValueSubscription.TrendUpdateDeadbandString != @"")
                            {
                                storeTrend = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value, new Any(pcObject_JournalParam_ValueSubscription.TrendUpdateDeadbandString).ValueAsSingle(false));
                            }
                            else if (pcObject_JournalParam_ValueSubscription.TrendUpdateDeadbandPercentString != @"")
                            {
                                if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.StatusCode) &&
                                        StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.StatusCode))
                                {
                                    var trendDeadband = (pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.Value.ValueAsSingle(false) -
                                        pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.Value.ValueAsSingle(false)) *
                                        new Any(pcObject_JournalParam_ValueSubscription.TrendUpdateDeadbandPercentString).ValueAsSingle(false) / 100.0f;
                                    storeTrend = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value, trendDeadband);
                                }
                            }
                            else
                            {
                                storeTrend = SszOperatorHelper.Compare(currentValue.Value, SszOperator.NotEqual, lastStored_Value);
                            }
                        }
                    }
                    else
                    {
                        storeTrend = true;
                    }
                    if (storeTrend)
                    {
                        pcObject_JournalParam_ValueSubscription.LastStored_Trend_ValueStatusTimestamp = new ValueStatusTimestamp(new Any(currentValue.Value), StatusCodes.Good, nowUtc);
                        journalParamValuesCollection.FloatValues.Add(
                                new FloatJournalParamValue
                                {
                                    TimestampUtc = new DateTimeOffset(nowUtc).ToUnixTimeMilliseconds(),
                                    Value = currentValue.Value
                                });

                        if (journalParamValuesCollection_ToModify is null)
                            journalParamValuesCollection_ToModify = _dbContext_ToModify.Current_PcObject.JournalParamValuesCollections
                                    .Single(c => String.Equals(c.ParamName, journalParamValuesCollection.ParamName, StringComparison.InvariantCultureIgnoreCase));
                        journalParamValuesCollection_ToModify.FloatValues.Add(new FloatJournalParamValue
                        {
                            TimestampUtc = new DateTimeOffset(nowUtc).ToUnixTimeMilliseconds(),
                            Value = currentValue.Value,
                        });
                    }                                              
                }

                if (storeCurrentValue || storeTrend)
                {
                    journalParamValuesCollection.ParamName = journalParam.ParamName; // For case sensivity                                            
                    var jpvc_metadataFieldsDictionary = journalParamValuesCollection.MetadataFieldsDictionary;
                    if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.StatusCode))
                        jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_ValueMax] = pcObject_JournalParam_ValueSubscription.Max_ValueSubscription.ValueStatusTimestamp.Value.ValueAsString(false);
                    if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.StatusCode))
                        jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_ValueMin] = pcObject_JournalParam_ValueSubscription.Min_ValueSubscription.ValueStatusTimestamp.Value.ValueAsString(false);
                    if (StatusCodes.IsGood(pcObject_JournalParam_ValueSubscription.EU_ValueSubscription.ValueStatusTimestamp.StatusCode))
                        jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_ValueEU] = pcObject_JournalParam_ValueSubscription.EU_ValueSubscription.ValueStatusTimestamp.Value.ValueAsString(false);
                    if (storeTrend)
                    {
                        jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_TrendStorePeriod] = pcObject_JournalParam_ValueSubscription.TrendStorePeriodString;
                        if (String.IsNullOrEmpty(jpvc_metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_TrendBeginTimeUtc)))
                            jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_TrendBeginTimeUtc] = new Any(nowUtc).ValueAsString(false);
                        jpvc_metadataFieldsDictionary[PazCheckConstants.ParamName_TrendEndTimeUtc] = new Any(nowUtc).ValueAsString(false);
                    }                        
                    journalParamValuesCollection.MetadataFieldsDictionary = jpvc_metadataFieldsDictionary;                        

                    if (journalParamValuesCollection_ToModify is null)
                        journalParamValuesCollection_ToModify = _dbContext_ToModify.Current_PcObject.JournalParamValuesCollections
                                    .Single(c => String.Equals(c.ParamName, journalParamValuesCollection.ParamName, StringComparison.InvariantCultureIgnoreCase));
                    journalParamValuesCollection_ToModify.ParamName = journalParamValuesCollection.ParamName;
                    journalParamValuesCollection_ToModify.MetadataFieldsDictionary = jpvc_metadataFieldsDictionary;                        
                    dbContext_ToModify.DbContextChanged = true;
                }
            }
        }
    }       

    private async Task CleanUpTrendAsync(
        PazCheckDbContext readOnlyDbContext,
        JournalParamValuesCollection journalParamValuesCollection,
        DateTime nowUtc)
    {
        var metadataFieldsDictionary = journalParamValuesCollection.MetadataFieldsDictionary;
        var trendStorePeriodString = metadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_TrendStorePeriod);            
        var trendStorePeriod = new Any(trendStorePeriodString).ValueAs<TimeSpan>(false);
        if (trendStorePeriod > TimeSpan.Zero)
        {
            long timeUtc_Optimized = new DateTimeOffset(nowUtc - trendStorePeriod).ToUnixTimeMilliseconds();

            await readOnlyDbContext.FloatJournalParamValues
                .Where(v => v.JournalParamValuesCollection == journalParamValuesCollection &&
                    v.TimestampUtc < timeUtc_Optimized).ExecuteDeleteAsync();
            //await dbContext.Int32JournalParamValues
            //    .Where(v => v.JournalParamValuesCollection == journalParamValuesCollection &&
            //        v.TimestampUtc < timeUtc).ExecuteDeleteAsync();
            //await dbContext.StringJournalParamValues
            //    .Where(v => v.JournalParamValuesCollection == journalParamValuesCollection &&
            //
        }
    }

    private async Task Sync_BasePcObjects_PcObjects_WithActiveProjectVersionAsync()
    {
        Common.Serialization.SerializationRootObject serializationRootObject = new();
        serializationRootObject.BasePcObjects = new List<Common.Serialization.BasePcObject>();
        serializationRootObject.PcObjects = new List<Common.Serialization.PcObject>();
        foreach (var rootPcObject in _cache.DbCache.PcObjectsDictionary2.Values.Where(o => String.IsNullOrEmpty(o.ParamsDictionary.TryGetValue(PazCheckConstants.ParamName_PcObjectParent))))
        {
            ProjectAllParamValues? projectAllParamValues = null;
            ProjectVersion? activeProjectVersion = rootPcObject.Unit.ActiveProjectVersion;
            if (activeProjectVersion is not null)
            {
                projectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(activeProjectVersion.ProjectId, activeProjectVersion.VersionNum, _dbContext_ReadOnly.ReadOnlyDbContext, LoggersSet.Empty);

                foreach (var kvp in projectAllParamValues.SafetyControllersParams)
                {
                    if (kvp.Key.EndsWith(PazCheckConstants.IdentifierEnding_Template, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var serializationBasePcObject = new Common.Serialization.BasePcObject()
                        {
                            Identifier = kvp.Key,
                            Unit = rootPcObject.Unit.Identifier,
                            Params = kvp.Value.Select(p =>
                            {
                                var param_ = _serializationParamsPool.Get();
                                param_.Name = p.ParamName;
                                param_.Value = p.Value;
                                return param_;
                            })
                            .ToList()
                        };
                        serializationRootObject.BasePcObjects.Add(serializationBasePcObject);
                    }
                    else
                    {
                        var serializationPcObject = new Common.Serialization.PcObject()
                        {
                            Identifier = kvp.Key,
                            Unit = rootPcObject.Unit.Identifier,
                            Params = kvp.Value.Select(p =>
                            {
                                var param_ = _serializationParamsPool.Get();
                                param_.Name = p.ParamName;
                                param_.Value = p.Value;
                                return param_;
                            })
                            .ToList()
                        };
                        serializationRootObject.PcObjects.Add(serializationPcObject);
                    }
                }
            }
        }

        if (serializationRootObject.BasePcObjects.Any(bo => !PazCheckDbHelper.CheckBasePcObject(bo, _cache.DbCache)) ||
            serializationRootObject.PcObjects.Any(o => !PazCheckDbHelper.CheckPcObject(o, _cache.DbCache)))
        {
            await SerializationHelper.ImportSerializationRootObjectAsync(
                    serializationRootObject,
                    new Common.Serialization.ImportMetadata()
                    {
                        RootCollectionMode = Common.Serialization.CollectionMode.Replace,
                        ChildCollectionMode = Common.Serialization.CollectionMode.Replace,
                        DataCollectionMode = Common.Serialization.CollectionMode.Update,
                    },
                    _dbContextFactory,
                    @"",
                    null,
                    CancellationToken.None,
                    NullJobProgress.Instance,
                    LoggersSet.Empty,
                    new Common.Serialization.ImportSerializationRootObjectResult(),
                    preview: false);
        }

        foreach (var pcObject in serializationRootObject.PcObjects)
            foreach (var param_ in pcObject.Params!)
            {
                _serializationParamsPool.Return(param_);
            }

        foreach (var basePcObject in serializationRootObject.BasePcObjects)
            foreach (var param_ in basePcObject.Params!)
            {
                _serializationParamsPool.Return(param_);
            }
    }

    #endregion

    #region private fields

    private readonly IMainServerWorker _mainServerWorker;                
    private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
    private readonly Cache _cache;
    private readonly IConfiguration _configuration;        
    private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
    private readonly ILogger _logger;
    private readonly CancellationToken _applicationStopping_CancellationToken;
    private readonly DbContext_ReadOnly _dbContext_ReadOnly = new();
    /// <summary>
    /// MainProcess only
    /// </summary>
    private readonly DbContext_ToModify _dbContext_ToModify = new();

    public IDataAccessProvider _dataAccessProvider;

    private DateTime _lastDoWorkDateTimeUtc = DateTime.UtcNow;
    private DateTime _lastCleanUpDateTimeUtc = DateTime.MinValue;              
    
    private string? _journalParamValuesCollection_Guid;

    /// <summary>
    ///     [id, PcObjectSubscription]
    /// </summary>
    private readonly CaseInsensitiveOrderedDictionary<PcObject_JournalParam_ValueSubscription> _pcObject_JournalParam_ValueSubscriptionsDictionary = new();

    private readonly Common.Serialization.ParamPooledPolicy _paramPooledPolicy;
    private readonly SingleThreadObjectPool<Common.Serialization.Param>  _serializationParamsPool;

    private CaseInsensitiveOrderedDictionary<string?> _optionsSubstituted = null!;

    private readonly LongRunningTask _longRunningTask;

    #endregion

    public class PcObject_JournalParam_ValueSubscription : IDisposable
    {
        public PcObject_JournalParam_ValueSubscription(
            DbCache dbCache,
            PcObject pcObject,
            IDataAccessProvider dataAccessProvider,                
            string in_,
            string converter,
            string valueMaxString,
            string valueMinString,
            string valueEUString,                
            TimeSpan valueUpdatePeriod,
            string valueUpdateDeadbandPercentString,
            string valueUpdateDeadbandString,
            TimeSpan trendUpdatePeriod,
            string trendUpdateDeadbandPercentString,
            string trendUpdateDeadbandString,
            bool trendEnabled,
            string trendStorePeriodString)
        {                
            In = in_;                
            Converter = converter;
            ValueMaxString = valueMaxString;
            ValueMinString = valueMinString;
            ValueEUString = valueEUString;                

            ValueUpdatePeriod = valueUpdatePeriod;
            ValueUpdateDeadbandPercentString = valueUpdateDeadbandPercentString;
            ValueUpdateDeadbandString = valueUpdateDeadbandString;

            TrendUpdatePeriod = trendUpdatePeriod;
            TrendUpdateDeadbandPercentString = trendUpdateDeadbandPercentString;
            TrendUpdateDeadbandString = trendUpdateDeadbandString;

            TrendEnabled = trendEnabled;
            TrendStorePeriodString = trendStorePeriodString;                

            foreach (var inPart in in_.Split(PazCheckConstants.PartsSeparator, StringSplitOptions.None))
            {
                if (String.IsNullOrEmpty(inPart))
                    continue;                                   

                // OR list of filters
                List<CaseInsensitiveOrderedDictionary<List<string>>> parsedJsonFilterInfo;
                // AND dictionary of filters
                CaseInsensitiveOrderedDictionary<List<string>> partParsedJsonFilterInfo;

                PazCheckDbHelper.ParseInPart(
                    inPart,
                    out parsedJsonFilterInfo,
                    out partParsedJsonFilterInfo);

                Query query = new();

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_QType, out List<string>? queryType_Values);
                query.QueryType_UpperCase = queryType_Values?.FirstOrDefault()?.ToUpperInvariant();

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_QString, out List<string>? queryString_Values);
                query.QueryString_Values = queryString_Values;

                if (query.QueryType_UpperCase == PazCheckConstants.QueryType_DataAccess_UpperCase)
                {
                    if (queryString_Values is not null &&
                            queryString_Values.Count > 0)
                        ValueSubscriptions.Add(new ValueSubscription(dataAccessProvider, queryString_Values[0]));
                }
                else
                {
                    partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_FilterMode, out List<string>? filterMode_Values);
                    query.FilterMode_Values = filterMode_Values;

                    partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_Format, out List<string>? format_Values);
                    query.Format_Values = format_Values;
                    query.Filter = FilterHelper.Create(parsedJsonFilterInfo);
                    query.Filter.ParentEntityId = pcObject.Id;
                    FilterHelper.Prepare(query.Filter, dbCache);

                    query.Query_ValueSubscription = new ValueSubscription(dataAccessProvider, @"");
                    ValueSubscriptions.Add(query.Query_ValueSubscription);

                    if (_notDataAccessQueries is null)
                        _notDataAccessQueries = new();
                    _notDataAccessQueries.Add(query);
                }
            }
            
            Value_Converter = PazCheckDbHelper.GetSszConverter(converter);

            Max_ValueSubscription = new ValueSubscription(dataAccessProvider, valueMaxString);
            Min_ValueSubscription = new ValueSubscription(dataAccessProvider, valueMinString);
            EU_ValueSubscription = new ValueSubscription(dataAccessProvider, valueEUString);
        }

        public void Dispose()
        {
            foreach (var valueSubscription in ValueSubscriptions)
            {
                valueSubscription.Dispose();
            }
            Max_ValueSubscription.Dispose();
            Min_ValueSubscription.Dispose();
            EU_ValueSubscription.Dispose();
        }

        #region public functions

        public string In { get; }
        public string Converter { get; }
        public string ValueMaxString { get; }
        public string ValueMinString { get; }
        public string ValueEUString { get; } 
        
        public TimeSpan ValueUpdatePeriod { get; }
        public string ValueUpdateDeadbandPercentString { get; }
        public string ValueUpdateDeadbandString { get; }

        public TimeSpan TrendUpdatePeriod { get; }
        public string TrendUpdateDeadbandPercentString { get; }
        public string TrendUpdateDeadbandString { get; }

        /// <summary>
        ///     Whether save historic trend values to DB
        /// </summary>
        public bool TrendEnabled { get; }
        public string TrendStorePeriodString { get; }

        public ValueStatusTimestamp LastStored_CurrentValue_ValueStatusTimestamp = new ValueStatusTimestamp { StatusCode = StatusCodes.Uncertain };
        public ValueStatusTimestamp LastStored_Trend_ValueStatusTimestamp = new ValueStatusTimestamp { StatusCode = StatusCodes.Uncertain };

        /// <summary>
        ///     Includes Query ValueSubscriptions
        /// </summary>
        public readonly List<ValueSubscription> ValueSubscriptions = new();                        

        public SszConverter? Value_Converter { get; set; }

        public ValueSubscription Max_ValueSubscription = null!;

        public ValueSubscription Min_ValueSubscription = null!;

        public ValueSubscription EU_ValueSubscription = null!;

        public bool IsUsed { get; set; }            

        /// <summary>
        ///     Calculates current value.
        /// </summary>
        /// <param name="journalParamValuesCollection"></param>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="dbCache"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="pcObject"></param>
        /// <param name="nowUtc"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public async Task<float?> CalculateCurrentValueAsync(
                JournalParamValuesCollection journalParamValuesCollection,
                PazCheckDbContext readOnlyDbContext,
                DbCache dbCache,                    
                IMainServerWorker mainServerWorker,
                PcObject pcObject,
                DateTime nowUtc,
                ILoggersSet loggersSet)
        {                
            if (_notDataAccessQueries is not null)
            {
                foreach (var notDataAccessQuery in _notDataAccessQueries)
                {
                    switch (notDataAccessQuery.QueryType_UpperCase)
                    {
                        case PazCheckConstants.QueryType_System_UpperCase:
                            if (notDataAccessQuery.QueryString_Values is not null && notDataAccessQuery.QueryString_Values.Count > 0)
                            {
                                decimal sum = 0;
                                int systemValuesCount = 0;
                                foreach (var queryString_Value in notDataAccessQuery.QueryString_Values)
                                {
                                    Any systemValue;
                                    lock (((System.Collections.ICollection)mainServerWorker.SystemParams).SyncRoot)
                                    {
                                        systemValue = mainServerWorker.SystemParams.GetValueOrDefault(queryString_Value);
                                    };

                                    if (systemValue.ValueTypeCode != Any.TypeCode.Empty)
                                    {
                                        systemValuesCount += 1;
                                        sum += systemValue.ValueAsDecimal(false);
                                    }                                        
                                }   

                                if (systemValuesCount > 0)
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)sum), StatusCodes.Good, nowUtc));
                                else
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any(), StatusCodes.Uncertain, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_Values_UpperCase:
                        case PazCheckConstants.QueryType_Value_UpperCase:
                            if (notDataAccessQuery.QueryString_Values is not null && notDataAccessQuery.QueryString_Values.Count > 0)
                            {
                                float sum = 0;
                                int valuesCount = 0;
                                var pcObjects = (await PazCheckDbHelper.FilterAsync(
                                    readOnlyDbContext, 
                                    dbCache, 
                                    typeof(PcObject), 
                                    notDataAccessQuery.Filter, 
                                    null,
                                    pcObject)).OfType<PcObject>().ToList();
                                foreach (PcObject po in pcObjects)
                                    foreach (var queryString_Value in notDataAccessQuery.QueryString_Values)                                            
                                    {
                                        if (queryString_Value.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            var journalParamValuesCollection2 = po.JournalParamValuesCollections.FirstOrDefault(jpvc => String.Equals(queryString_Value, jpvc.ParamName, StringComparison.InvariantCultureIgnoreCase));
                                            if (journalParamValuesCollection2 is null)
                                                continue;

                                            if (journalParamValuesCollection2.CurrentValue is null)
                                                continue;

                                            valuesCount += 1;
                                            sum += journalParamValuesCollection2.CurrentValue.Value;
                                        }
                                        else
                                        {
                                            valuesCount += 1;

                                            string valueString = PazCheckDbHelper.GetParamValue<string>(pcObject.ParamsDictionary, pcObject.BasePcObject?.ParamsDictionary, queryString_Value, @"");
                                            sum += new Any(valueString).ValueAsSingle(false);
                                        }                                             
                                    }

                                if (valuesCount > 0)
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any(sum), StatusCodes.Good, nowUtc));
                                else
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any(), StatusCodes.Uncertain, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_SafetyIndexFromChildren_UpperCase:
                            {                                    
                                List<PcObject> childrenForSafetyIndex = PazCheckDbHelper.GetChildrenForSafetyIndex(pcObject);
                                double safetyIndexSum = 0.0;
                                double kSum = 0.0;
                                foreach (var childPcObject in childrenForSafetyIndex)
                                {
                                    var childSafetyIndex = PazCheckDbHelper.GetSafetyIndex(childPcObject);
                                    if (childSafetyIndex > -0.000001 && childPcObject.K > 0.000001)
                                    {
                                        safetyIndexSum += childSafetyIndex * childPcObject.K;
                                        kSum += childPcObject.K;
                                    }
                                }
                                if (kSum > 0.000001)
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)(safetyIndexSum / kSum)), StatusCodes.Good, nowUtc));
                                else
                                    notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any(), StatusCodes.Uncertain, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_SafetyIndexFromChildrenV2_UpperCase:
                            {
                                List<PcObject> childrenForSafetyIndex = PazCheckDbHelper.GetChildrenForSafetyIndex(pcObject);
                                double safetyIndex = 100.0;                                    
                                foreach (var childPcObject in childrenForSafetyIndex)
                                {
                                    var childSafetyIndex = PazCheckDbHelper.GetSafetyIndex(childPcObject);
                                    if (childSafetyIndex > -0.000001 && childPcObject.K > 0.000001)
                                    {
                                        safetyIndex -= (100.0 - childSafetyIndex) * childPcObject.K / 100.0;                                            
                                    }
                                }
                                if (safetyIndex < 0.0)
                                    safetyIndex = 0.0;
                                notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)safetyIndex), StatusCodes.Good, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_ObjectsCount_UpperCase:
                            {
                                int count = (await PazCheckDbHelper.FilterAsync(
                                    readOnlyDbContext, 
                                    dbCache, 
                                    typeof(PcObject), 
                                    notDataAccessQuery.Filter, 
                                    null,
                                    pcObject)).Count();

                                notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)count), StatusCodes.Good, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_EventsCount_UpperCase:
                            {
                                int count = (await PazCheckDbHelper.FilterAsync(
                                    readOnlyDbContext, 
                                    dbCache, 
                                    typeof(PcObjectEvent), 
                                    notDataAccessQuery.Filter, 
                                    null,
                                    pcObject)).Count();

                                notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)count), StatusCodes.Good, nowUtc));
                            }
                            break;
                        case PazCheckConstants.QueryType_ActiveEventsCount_UpperCase:
                            {
                                int count = (await PazCheckDbHelper.FilterAsync(
                                    readOnlyDbContext, 
                                    dbCache, 
                                    typeof(PcObjectEvent), 
                                    notDataAccessQuery.Filter, 
                                    null,
                                    pcObject)).Count();

                                notDataAccessQuery.Query_ValueSubscription.Update(new ValueStatusTimestamp(new Any((float)count), StatusCodes.Good, nowUtc));
                            }
                            break;
                    }
                }                    
            }

            float? currentValue = null;

            if (Value_Converter is not null)
            {
                if (ValueSubscriptions.Count == 0 || ValueSubscriptions.All(vs => StatusCodes.IsGood(vs.ValueStatusTimestamp.StatusCode)))
                {
                    using var s1 = loggersSet.Logger.BeginScope(("PcObject", journalParamValuesCollection.PcObject.Identifier));
                    using var s2 = loggersSet.UserFriendlyLogger.BeginScope((Common.Properties.Resources.PcObject, journalParamValuesCollection.PcObject.Identifier));
                    var convertedValue = Value_Converter.Convert(
                        ValueSubscriptions.Select(vs => vs.ValueStatusTimestamp.Value.ValueAsObject()).ToArray(),
                        null,
                        loggersSet);
                    if (convertedValue != SszConverter.DoNothing)
                        currentValue = new Any(convertedValue).ValueAsSingle(false);
                }                    
            }
            else if (ValueSubscriptions.Count > 0 &&
                StatusCodes.IsGood(ValueSubscriptions[0].ValueStatusTimestamp.StatusCode))
            {
                currentValue = ValueSubscriptions[0].ValueStatusTimestamp.Value.ValueAsSingle(false);
            }

            return currentValue;
        }

        #endregion            

        #region private fields            

        private List<Query>? _notDataAccessQueries;

        #endregion
    }

    private class Query
    {
        /// <summary>
        ///     Do not dipose! Query_ValueSubscriptions is added to PcObject_JournalParam_ValueSubscription.ValueSubscriptions list.
        /// </summary>
        public ValueSubscription Query_ValueSubscription = null!;

        /// <summary>
        ///     Not null, if QueryValue_ValueSubscription is not null
        /// </summary>
        public Filter Filter = null!;
        public string? QueryType_UpperCase;
        public List<string>? QueryString_Values;            
        public List<string>? FilterMode_Values;
        public List<string>? Format_Values;
    }

    public class ValueSubscription : IValueSubscription, IDisposable
    {
        #region construction and destruction

        public ValueSubscription(IDataAccessProvider? dataAccessProvider,
            string elementId)
        {
            DataAccessProvider = dataAccessProvider;
            ElementId = elementId;

            if (ElementId != @"")
                DataAccessProvider?.AddItem(ElementId, this);
        }

        public void Dispose()
        {
            if (ElementId != @"")
                DataAccessProvider?.RemoveItem(this);
        }

        #endregion

        #region public functions

        public IDataAccessProvider? DataAccessProvider { get; }

        public string ElementId { get; }

        /// <summary>
        ///     Id actually used for subscription.
        /// </summary>
        public string MappedElementIdOrConst { get; private set; } = @"";

        public ValueStatusTimestamp ValueStatusTimestamp { get; private set; } = new ValueStatusTimestamp { StatusCode = StatusCodes.Uncertain };

        public void Update(string mappedElementIdOrConst)
        {
            MappedElementIdOrConst = mappedElementIdOrConst;
        }

        public void Update(ValueStatusTimestamp valueStatusTimestamp)
        {
            ValueStatusTimestamp = valueStatusTimestamp;
        }

        #endregion
    }        

    private class DbContext_ReadOnly
    {
        public bool IsInitialized { get; private set; }

        public PazCheckDbContext ReadOnlyDbContext { get; private set; } = null!;

        public string? ParamDescs_Units_BasePcObjects_PcObjects_Guid { get; private set; }        

        public async Task InitializeAsync(PazCheckDbContext readOnlyDbContext, string paramDescs_Units_BasePcObjects_PcObjects_Guid)
        {
            ReadOnlyDbContext = readOnlyDbContext;
            ParamDescs_Units_BasePcObjects_PcObjects_Guid = paramDescs_Units_BasePcObjects_PcObjects_Guid;

            _basePcObjects = await ReadOnlyDbContext.BasePcObjects
                    .Where(po => !po._IsDeleted)
                    .Include(po => po.Unit)
                    .Include(po => po.JournalParams)
                    .ToArrayAsync();

            _pcObjects = await ReadOnlyDbContext.PcObjects
                .Where(po => !po._IsDeleted)
                .Include(po => po.Unit)
                .Include(po => po.JournalParamValuesCollections)
                .Include(po => po.JournalParams)
                .Include(po => po.BasePcObject)
                .ThenInclude(bpo => bpo.JournalParams)
                .Include(po => po.PcObjectEvents.Where(e => e._IsDeleted == false && e.EndTimeUtc == null)) // Used in PazCheckDbHelper.FilterPcObjectEventsAsync()
                .Include(po => po.Parent)
                .Include(po => po.Children.Where(chpo => chpo._IsDeleted == false))
                .OrderByDescending(po => po.Level)
                .ToArrayAsync();

            IsInitialized = true;
        }

        public void Close()
        {
            if (IsInitialized)
            {
                ReadOnlyDbContext.Dispose();                
                IsInitialized = false;
            }
        }

        public void Reset_ParamDescs_Units_BasePcObjects_PcObjects_Guid()
        {
            ParamDescs_Units_BasePcObjects_PcObjects_Guid = null;
        }

        public DbCache CreateDbCache()
        {
            DbCache dbCache = new();

            dbCache.UnitsDictionary = ReadOnlyDbContext.Units
                .Include(u => u.ActiveProjectVersion)
                .ThenInclude(pv => pv!.Project)
                .ToFrozenDictionary(u => u.Identifier, u => u, StringComparer.InvariantCultureIgnoreCase);

            dbCache.PcObjectEventTypesDictionary = ReadOnlyDbContext.PcObjectEventTypes
                .ToFrozenDictionary(t => t.Type, u => u, StringComparer.InvariantCultureIgnoreCase);

            dbCache.ParamDescs = ReadOnlyDbContext.ParamDescs.Select(pd => KeyValuePair.Create(pd.Id, pd)).ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
            //dbCache.ReverseParamDescs = readOnlyDbContext.ParamDescs.Select(pd => KeyValuePair.Create(pd.Desc, pd)).ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);                                    

            // [Unit.Identifier + '.' + BasePcObject.Identifier, BasePcObject]                  
            dbCache.BasePcObjectsDictionary1 = _basePcObjects.ToFrozenDictionary(bo => bo.Unit.Identifier + "." + bo.Identifier, StringComparer.InvariantCultureIgnoreCase);
            dbCache.BasePcObjectsDictionary2 = _basePcObjects.ToFrozenDictionary(bo => bo.Id);

            // [Unit.Identifier + '.' + PcObject.Identifier, PcObject]                    
            dbCache.PcObjectsDictionary1 = _pcObjects.ToFrozenDictionary(o => o.Unit.Identifier + "." + o.Identifier, StringComparer.InvariantCultureIgnoreCase);
            dbCache.PcObjectsDictionary2 = _pcObjects.ToFrozenDictionary(o => o.Id);

            return dbCache;
        }

        private BasePcObject[] _basePcObjects = null!;
        private PcObject[] _pcObjects = null!;
    }

    private class DbContext_ToModify
    {
        public bool IsInitialized { get; private set; }

        public PazCheckDbContext DbContext { get; set; } = null!;

        public bool DbContextChanged { get; set; }

        public FrozenDictionary<int, PcObject> PcObjectsDictionary { get; set; } = null!;

        public PcObject Current_PcObject { get; set; } = null!;

        public async Task InitializeAsync(PazCheckDbContext dbContext)
        {
            DbContext = dbContext;
            DbContextChanged = false;
            PcObjectsDictionary = (await dbContext.PcObjects
                        .Where(po => !po._IsDeleted)
                        .Include(po => po.JournalParamValuesCollections)
                        .ToArrayAsync())
                        .ToFrozenDictionary(po => po.Id);
            Current_PcObject = null!;

            IsInitialized = true;
        }

        public void Close()
        {
            if (IsInitialized)
            {
                DbContext.Dispose();
                IsInitialized = false;
            }            
        }        
    }
}

//private bool UpdateBasePcObject(BasePcObject basePcObject, ProjectAllParamValues projectAllParamValues)
//{
//    if (!projectAllParamValues.SafetyControllersParams.GetValueOrDefault(basePcObject.Identifier, out List<VersionedParamBase>? paramsList))
//        return false;

//    bool changed = false;

//    var paramsDictionary = basePcObject.ParamsDictionary;
//    foreach (var param_ in paramsList)
//    {
//        paramsDictionary.TryGetValue(param_.ParamName, out string? value);
//        if (value != param_.Value)
//        {
//            paramsDictionary[param_.ParamName] = param_.Value;
//            changed = true;
//        }
//    }
//    basePcObject.ParamsDictionary = paramsDictionary;

//    return changed;
//}

//private bool UpdatePcObject(PcObject pcObject, ProjectAllParamValues projectAllParamValues)
//{
//    if (!projectAllParamValues.SafetyControllersParams.GetValueOrDefault(pcObject.Identifier, out List<VersionedParamBase>? paramsList))
//        return false;

//    bool changed = false;

//    var paramsDictionary = pcObject.ParamsDictionary;
//    foreach (var param_ in paramsList)
//    {
//        paramsDictionary.TryGetValue(param_.ParamName, out string? value);
//        if (value != param_.Value)
//        {
//            paramsDictionary[param_.ParamName] = param_.Value;
//            changed = true;
//        }
//    }
//    pcObject.ParamsDictionary = paramsDictionary;

//    return changed;
//}

//switch (AnyHelper.GetTransportType(converted_ValueStatusTimestamp.Value))
//{
//    case TransportType.Double:
//        journalParamValuesCollection.FloatValues.Add(
//            new FloatJournalParamValue { TimestampUtc = converted_ValueStatusTimestamp.TimestampUtc, Value = converted_ValueStatusTimestamp.Value.ValueAsSingle(false) });
//        break;
//    case TransportType.UInt32:
//        journalParamValuesCollection.Int32Values.Add(
//            new Int32JournalParamValue { TimestampUtc = converted_ValueStatusTimestamp.TimestampUtc, Value = converted_ValueStatusTimestamp.Value.ValueAsInt32(false) });
//        break;
//    case TransportType.Object:
//        journalParamValuesCollection.StringValues.Add(
//            new StringJournalParamValue { TimestampUtc = converted_ValueStatusTimestamp.TimestampUtc, Value = converted_ValueStatusTimestamp.Value.ValueAsString(false) });
//        break;
//}

////private void FillInEventSourceModelAndDb(EventSourceModel eventSourceModel)
////{
////    Unit? unit = _dbContext.Units
////        .Include(u => u.PcObjects)
////        .FirstOrDefault(u => u.Identifier == PazCheckDbHelper.DefaultUnitIdentifier);
////    if (unit == null)
////    {
////        _logger.LogError("Invalid UnitId.");
////        return;
////    }

////    _dbContext.RemoveRange(unit.PcObjects);            

////    _dbContext.SaveChanges();

////    var tagBasePcObject = _dbContext.BasePcObjects.Single(b => b.Identifier == "Tag");
////    var areaBasePcObject = _dbContext.BasePcObjects.Single(b => b.Identifier == "Area");

////    
////    var r = new Random((int)DateTime.Now.Ticks);

////    var values = CsvDb.GetValues("SafetyTags.csv");
////    foreach (var kvp in values)
////    {
////        var line = kvp.Value;
////        if (line.Count < 3) continue;
////        string tag = line[0] ?? "";
////        string type = line[1] ?? "";
////        double k = new Any(line[2]).ValueAsDouble(false);
////        if (k == 0.0) k = 1.0;
////        string area = line.Count >= 4 ? line[3] ?? "" : "";
////        if (!String.Equals(type, "tag", StringComparison.InvariantCultureIgnoreCase)) continue;

////        var fullAreaList = new List<string>();
////        while (area != "")
////        {
////            fullAreaList.Add(area);
////            area = CsvDb.GetValue("SafetyTags.csv", area, 3) ?? "";
////        }
////        string fullArea = "";
////        if (fullAreaList.Count > 0)
////        {
////            fullArea = String.Join('/', fullAreaList.AsEnumerable().Reverse().ToArray());
////        }

////        var tagPcObject = new PcObject
////        {
////            BasePcObject = tagBasePcObject,
////            Identifier = tag,
////            Title = tag,
////            K = k,
////            AlarmIntensity = 1,
////            
////            AlarmLevel = r.Next(0, 3)                    
////        };
////        unit.PcObjects.Add(tagPcObject);

////        EventSourceObject eventSourceObject = eventSourceModel.GetOrCreateEventSourceObject(tag, fullArea);
////        eventSourceObject.Obj = tagPcObject;
////        eventSourceObject.Subscriptions.Add(tagPcObject, new EventSourceModelSubscriptionInfo(EventSourceModel.AlarmMaxCategoryId_SubscriptionType, EventSourceModelSubscriptionScope.Active));

////        if (line.Count >= 5)
////        {
////            string prop = line[4] ?? "";
////            if (prop == "") continue;
////            string ll = "";
////            if (line.Count >= 6) ll = line[5] ?? "";
////            string l = "";
////            if (line.Count >= 7) l = line[6] ?? "";
////            string h = "";
////            if (line.Count >= 8) h = line[7] ?? "";
////            string hh = "";
////            if (line.Count >= 9) hh = line[8] ?? "";
////            string vh = "";
////            if (line.Count >= 10) vh = line[9] ?? "";
////            string vhh = "";
////            if (line.Count >= 11) vhh = line[10] ?? "";

////            AddValueSubscriptions(tag, prop, ll, l, h, hh, vh, vhh, eventSourceModel);
////        }
////    }

////    foreach (var kvp in eventSourceModel.EventSourceAreas)
////    {
////        EventSourceArea eventSourceArea = kvp.Value;

////        var area = eventSourceArea.Area.Split('/').Last();
////        double k = new Any(CsvDb.GetValue("SafetyTags.csv", area, 2)).ValueAsDouble(false);
////        if (k == 0.0) k = 1.0;
////        var areaPcObject = new PcObject
////        {
////            BasePcObject = areaBasePcObject,
////            Identifier = area,
////            Title = area,
////            K = k,
////            AlarmIntensity = 1,
////            //TEMPCODE
////            AlarmLevel = r.Next(0, 3)                    
////        };
////        unit.PcObjects.Add(areaPcObject);

////        eventSourceArea.Obj = areaPcObject;
////        eventSourceArea.Subscriptions.Add(areaPcObject, new EventSourceModelSubscriptionInfo(EventSourceModel.AlarmMaxCategoryId_SubscriptionType, EventSourceModelSubscriptionScope.Active));
////    }

////    foreach (var kvp in eventSourceModel.EventSourceObjects)
////    {
////        EventSourceObject eventSourceObject = kvp.Value;
////        PcObject tagPcObject = eventSourceObject.Obj as PcObject ?? throw new InvalidOperationException();

////        var areas = new Dictionary<int, EventSourceArea>();
////        foreach (var kvp2 in eventSourceObject.EventSourceAreas)
////        {
////            if (kvp2.Key == @"")
////            {
////                areas[0] = kvp2.Value;
////            }
////            else
////            {
////                var areaParts = kvp2.Key.Split('/');
////                areas[areaParts.Length] = kvp2.Value;
////            }
////        }
////        foreach (var kvp2 in areas.OrderByDescending(kvp3 => kvp3.Key))
////        {
////            PcObject areaSection = kvp2.Value.Obj as PcObject ?? throw new InvalidOperationException();
////            if (areaSection.Children.Contains(tagPcObject)) break;
////            areaSection.Children.Add(tagPcObject);
////            tagPcObject.Parent = areaSection;
////            //tagPcObject.Level = kvp2.Key + 1;

////            tagPcObject = areaSection;
////        }
////    }

////    _dbContext.SaveChanges();
////}

//private void AddValueSubscriptions(string tag, string prop, string ll, string l, string h, string hh, string vh, string vhh, EventSourceModel eventSourceModel)
//{
//    var dataAccessProvider = eventSourceModel.DataAccessProvider;
//    if (ll != "")
//    {
//        double v = new Any(ll).ValueAsDouble(false);
//        //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//        //{
//        //    if (StatusCodes.IsGood(newVal.StatusCode))
//        //    {
//        //        bool active = newVal.Value.ValueAsDouble(false) <= v;
//        //        SendEventMessage(tag, AlarmConditionType.LowLow, active, 2, eventSourceModel);
//        //    }
//        //}));
//    }
//    if (l != "")
//    {
//        double v = new Any(l).ValueAsDouble(false);
//        //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//        //{
//        //    if (StatusCodes.IsGood(newVal.StatusCode))
//        //    {
//        //        bool active = newVal.Value.ValueAsDouble(false) <= v;
//        //        SendEventMessage(tag, AlarmConditionType.Low, active, 1, eventSourceModel);
//        //    }
//        //}));
//    }
//    if (h != "")
//    {
//        double v = new Any(h).ValueAsDouble(false);
//        //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//        //{
//        //    if (StatusCodes.IsGood(newVal.StatusCode))
//        //    {
//        //        bool active = newVal.Value.ValueAsDouble(false) >= v;
//        //        SendEventMessage(tag, AlarmConditionType.High, active, 1, eventSourceModel);
//        //    }
//        //}));
//    }
//    if (hh != "")
//    {
//        double v = new Any(hh).ValueAsDouble(false);
//        //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//        //{
//        //    if (StatusCodes.IsGood(newVal.StatusCode))
//        //    {
//        //        bool active = newVal.Value.ValueAsDouble(false) >= v;
//        //        SendEventMessage(tag, AlarmConditionType.HighHigh, active, 2, eventSourceModel);
//        //    }
//        //}));
//    }
//    if (vh != "")
//    {
//        double v = new Any(vh).ValueAsDouble(false);
//        if (v > 0)
//        {
//            //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//            //{
//            //    if (StatusCodes.IsGood(newVal.StatusCode) &&
//            //        StatusCodes.IsGood(oldVal.StatusCode) &&
//            //        newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) >= v)
//            //        SendEventMessage(tag, AlarmConditionType.PositiveRate, true, 1, eventSourceModel);
//            //}));
//        }
//        else if (v < 0)
//        {
//            //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//            //{
//            //    if (StatusCodes.IsGood(newVal.StatusCode) &&
//            //        StatusCodes.IsGood(oldVal.StatusCode) &&
//            //        newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) < v)
//            //        SendEventMessage(tag, AlarmConditionType.NegativeRate, true, 1, eventSourceModel);
//            //}));
//        }
//    }
//    if (vhh != "")
//    {
//        double v = new Any(vhh).ValueAsDouble(false);
//        if (v > 0)
//        {
//            //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//            //{
//            //    if (StatusCodes.IsGood(newVal.StatusCode) &&
//            //        StatusCodes.IsGood(oldVal.StatusCode) &&
//            //        newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) >= v)
//            //        SendEventMessage(tag, AlarmConditionType.PositiveRate, true, 2, eventSourceModel);
//            //}));
//        }
//        else if (v < 0)
//        {
//            //_valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
//            //{
//            //    if (StatusCodes.IsGood(newVal.StatusCode) &&
//            //        StatusCodes.IsGood(oldVal.StatusCode) &&
//            //        newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) < v)
//            //        SendEventMessage(tag, AlarmConditionType.NegativeRate, true, 2, eventSourceModel);
//            //}));
//        }
//    }
//}

//private readonly List<ValueSubscription> _valueSubscriptionsCollection = new();

//private void SendEventMessage(string tag, AlarmConditionType alarmConditionType, bool active, uint alarmCategoryId, EventSourceModel eventSourceModel)
//{
//    EventSourceObject eventSourceObject = eventSourceModel.GetOrCreateEventSourceObject(tag, "");

//    eventSourceModel.ProcessEventSourceObject(eventSourceObject, alarmConditionType, alarmCategoryId,
//                active, true, DateTime.UtcNow, out bool alarmConditionChanged, out bool unackedChanged);
//}