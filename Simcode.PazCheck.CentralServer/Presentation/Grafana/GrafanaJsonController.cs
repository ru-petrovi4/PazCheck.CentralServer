using CommunityToolkit.HighPerformance.Helpers;
using IdentityServer4;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Presentation.Grafana.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel.Resolution;
using Ssz.Utils.Dispatcher;
using Simcode.PazCheck.CentralServer.MicroServices;
using System.Diagnostics.CodeAnalysis;

namespace Simcode.PazCheck.CentralServer.Presentation;

/// <summary>
///     Grafana plugin simpod-json-datasource, v0.2.6
///     grafana-cli plugins install simpod-json-datasource
/// </summary>
/// <remarks>
///     Grafana has two built-in time range variables: $__from and $__to. 
///     They are currently always interpolated as epoch milliseconds by default, but you can control date formatting.
/// </remarks>
[Route("GrafanaJson")]
public partial class GrafanaJsonController : ControllerBase
{
    #region construction and destruction

    public GrafanaJsonController(
        IMainServerWorker mainServerWorker,
        Cache cache,
        JobsManager jobsManager,
        AddonsManager addonsManager,            
        IDbContextFactory<PazCheckDbContext> dbContextFactory,            
        IHostApplicationLifetime applicationLifetime,
        IInformationSecurityEventsLogger informationSecurityEventsLogger,            
        ILogger<GrafanaJsonController> logger)
    {
        _mainServerWorker = mainServerWorker;
        _cache = cache;
        _jobsManager = jobsManager;
        _addonsManager = addonsManager;            
        _dbContextFactory = dbContextFactory;            
        _informationSecurityEventsLogger = informationSecurityEventsLogger;
        _logger = logger;
        _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

        _longRunningTask = new(_logger);
    }

    #endregion

    #region public functions           

    public const string Metric_Main = @"Main";
    public const decimal Metric_Main_Id = 1;  

    /// <summary>
    ///     Used for "Test connection" on the datasource config page.
    /// </summary>
    /// <returns></returns>
    [DefaultMethod_RoleBusinessFunctions(
        new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpGet]        
    public Task<IActionResult> DatasourceHealthAsync()
    {            
        return Task.FromResult<IActionResult>(StatusCode(200));
    }

    /// <summary>
    ///     Returns available metrics.
    /// </summary>
    /// <param name="listMetricsRequest"></param>
    /// <returns></returns>
    [DefaultMethod_RoleBusinessFunctions(
        new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpPost(@"search")]        
    public IActionResult ListMetrics([FromBody] ListMetricsRequest listMetricsRequest)
    {
        var result = new List<ListMetrics200ResponseInner>
        {
            new ListMetrics200ResponseInner(new ListMetrics200ResponseInnerOneOf(Metric_Main, Metric_Main_Id)),                                
        };
        var json = result.ToJson(Formatting.Indented);
        return Content(json, "application/json");
    }

    /// <summary>
    ///     Returns panel data or annotations.
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <returns></returns>
    [DefaultMethod_RoleBusinessFunctions(
        new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpPost(@"query")]        
    public async Task<IActionResult> QueryAsync([FromBody] QueryRequest queryRequest)
    {
        if (queryRequest?.Targets is null)
            return NoContent();

        List<Query200ResponseInner> result = new();

        foreach (var target in queryRequest.Targets)
        {
            switch (target.Target)
            {
                case Metric_Main_Id:
                    result.Add(await Query_MainAsync(queryRequest, target));
                    break;                                                            
            }
        }

        var json = result.ToJson(Formatting.Indented);
        return Content(json, "application/json");
    }

    /// <summary>
    ///    Returns data for Variable of type Query.
    /// </summary>
    /// <param name="annotationsRequest"></param>
    /// <returns></returns>
    [DefaultMethod_RoleBusinessFunctions(
    new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpPost(@"annotations")]
    public async Task<IActionResult> AnnotationsAsync([FromBody] AnnotationsRequest annotationsRequest)
    {            
        List<Annotations200ResponseInner> result = new();

        switch (annotationsRequest?.Annotation?.Name)
        {
            case @"Events":
                result.AddRange(await Annotations_PcObjectEvent(annotationsRequest));
                break;                
        }

        var json = result.ToJson(Formatting.Indented);
        return Content(json, "application/json");
    }

    /// <summary>
    ///     Returning tag keys for ad hoc filters.
    /// </summary>
    /// <returns></returns>
    [DefaultMethod_RoleBusinessFunctions(
    new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpPost(@"tag-keys")]
    public IActionResult TagKeys()
    {
        string json = """
[
    {"type":"string","text":"Тип события"},
    {"type":"string","text":"Деблокировочный ключ"}
]
""";        
        return Content(json, "application/json");
    }

    /// <summary>
    ///     Returning tag values for ad hoc filters.
    /// </summary>
    /// <param name="tagValuesRequest"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [DefaultMethod_RoleBusinessFunctions(
        new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
        )]
    [RestrictToLocalhost] // Excessive. Do not forget to include restrictions to access this to nginx.conf.
    [HttpPost(@"tag-values")]
    public IActionResult TagValues([FromBody] TagValuesRequest tagValuesRequest)
    {
        string json = @"";
        switch (tagValuesRequest.Key)
        {
            case "Тип события":
                json = """
[
    {"text": "Успешное срабатывание"},
    {"text": "Срабатывание с опозданием"},
    {"text": "Отказ"}
]
""";
                break;
        }
        return Content(json, "application/json");
    }

    #endregion

    #region private functions

    private Task<Query200ResponseInner> Query_MainAsync(QueryRequest queryRequest, QueryRequestTargetsInner queryRequestTarget)
    {
        MonitoringAddon? monitoringAddon = _addonsManager.AddonsThreadSafe
            .FirstOrDefault(a => String.Equals(a.Identifier, MonitoringAddon.AddonIdentifier, StringComparison.InvariantCultureIgnoreCase)) as MonitoringAddon;
        if (monitoringAddon is null || monitoringAddon.Monitoring is null)
            return Task.FromResult(new Query200ResponseInner(
                                new Query200ResponseInnerAnyOf(@"", new List<List<decimal>>())));

        var taskCompletionSource = new TaskCompletionSource<Query200ResponseInner>();

        monitoringAddon.Monitoring.Grafana_LongRunningTask.ThreadSafeDispatcher.BeginInvokeEx(async ct =>
        {
            try
            {
                // OR list of filters
                List<CaseInsensitiveOrderedDictionary<List<string>>> parsedJsonFilterInfo = null!;
                // AND dictionary of filters
                CaseInsensitiveOrderedDictionary<List<string>>? partParsedJsonFilterInfo = null;

                int? parentEntityId = null;

                int maxDataPoints = 1920;
                try
                {
                    if (queryRequest.MaxDataPoints > 0 && queryRequest.MaxDataPoints < maxDataPoints)
                        maxDataPoints = (int)queryRequest.MaxDataPoints;

                    var scopedVarsJsonElement = (JsonElement)queryRequest.ScopedVars!;
                    if (scopedVarsJsonElement.TryGetProperty(@"id", out JsonElement idJsonElement))
                        parentEntityId = new Any(idJsonElement.GetString(@"value")).ValueAsInt32(false);

                    // OR list of filters
                    parsedJsonFilterInfo = FilterHelper.Parse((JsonElement)queryRequestTarget.Data!);
                    if (parsedJsonFilterInfo.Count > 0)
                        partParsedJsonFilterInfo = parsedJsonFilterInfo.First();
                }
                catch
                {                        
                }

                if (partParsedJsonFilterInfo is null)
                {
                    partParsedJsonFilterInfo = new();
                    parsedJsonFilterInfo = new() { partParsedJsonFilterInfo };
                }

                string cacheKey = NameValueCollectionHelper.GetNameValueCollectionString(
                    partParsedJsonFilterInfo
                        .Where(kvp => !String.Equals(kvp.Key, PazCheckConstants.CriterionName_From, StringComparison.InvariantCultureIgnoreCase) &&
                            !String.Equals(kvp.Key, PazCheckConstants.CriterionName_To, StringComparison.InvariantCultureIgnoreCase))
                        .SelectMany(kvp => kvp.Value.Select(it => (kvp.Key, (string?)it)))
                        .Concat([(@"parentEntityId", (string?)(new Any(parentEntityId).ValueAsString(false)))])
                     );

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_QType, out List<string>? queryType_Values);
                var queryType_UpperCase = queryType_Values?.FirstOrDefault()?.ToUpperInvariant() ?? @"";

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_QString, out List<string>? queryString_Values);

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_FilterMode, out List<string>? filterMode_Values);

                partParsedJsonFilterInfo.Remove(PazCheckConstants.QueryPartName_Format, out List<string>? format_Values);

                Filter filter = FilterHelper.Create(parsedJsonFilterInfo!);
                filter.ParentEntityId = parentEntityId;

                // Array of AND filters
                var partCriterionCollection = filter.CriterionCollection?.FirstOrDefault();

                var nowUtc = DateTime.UtcNow;
                //long now_UnixTimeMilliseconds = new DateTimeOffset(nowUtc).ToUnixTimeMilliseconds();

                long fromTime_UnixTimeMilliseconds = new DateTimeOffset(PazCheckDbHelper.GetBeginTimeUtc(partCriterionCollection) ?? nowUtc).ToUnixTimeMilliseconds();
                long toTime_UnixTimeMilliseconds = new DateTimeOffset(PazCheckDbHelper.GetEndTimeUtc(partCriterionCollection) ?? nowUtc).ToUnixTimeMilliseconds();                

                switch (queryType_UpperCase)
                {
                    // !!!Warning!!! Early return!
                    case PazCheckConstants.QueryType_InformationSecurityEvents_UpperCase:
                        var results = await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(InformationSecurityEvent), filter, projectVersionNum: null, needOrdering: true);
                        taskCompletionSource.SetResult(new Query200ResponseInner(
                            new Query200ResponseInnerAnyOf1(
                                Query200ResponseInnerAnyOf1.TypeEnum.Table,
                                columns: new List<Query200ResponseInnerAnyOf1ColumnsInner>()
                                {
                                        new Query200ResponseInnerAnyOf1ColumnsInner(text: "time", type: "time"),
                                        new Query200ResponseInnerAnyOf1ColumnsInner(text: "event", type: "string"),
                                        new Query200ResponseInnerAnyOf1ColumnsInner(text: "level", type: "string"),
                                        new Query200ResponseInnerAnyOf1ColumnsInner(text: "User", type: "string"),
                                },
                                rows: results.OfType<InformationSecurityEvent>().Select(r => new Object[] { new DateTimeOffset(r.EventTimeUtc).ToUnixTimeMilliseconds(), r.EventDesc, GetLevel(r), r.User }).ToList())));

                        //| Time(time) | Body(string) | Поле1 | Поле2 |
                        //| -------------| ---------------| -------| -------|
                        //| 1635312236502 | "лог сообщение" | info | host1 |
                        //| 1635319376502 | "другое событие" | error | host2 |
                        return;
                    case PazCheckConstants.QueryType_Value_UpperCase:
                        fromTime_UnixTimeMilliseconds = toTime_UnixTimeMilliseconds - 1;
                        break;
                }

                if (fromTime_UnixTimeMilliseconds >= toTime_UnixTimeMilliseconds)
                    fromTime_UnixTimeMilliseconds = toTime_UnixTimeMilliseconds - 1;

                ImmutableTimeSeriesCache? immutableTimeSeriesCache = null;

                await _cache.GrafanaCache_SyncRoot.WaitAsync();
                try
                {
                    _cache.GrafanaCache.TryGetValue(cacheKey, out immutableTimeSeriesCache);
                }
                finally
                {
                    _cache.GrafanaCache_SyncRoot.Release();
                }               

                CacheQueryResult cacheQueryResult = immutableTimeSeriesCache?.Query(fromTime_UnixTimeMilliseconds, toTime_UnixTimeMilliseconds) ?? default;

                List<List<decimal>> resultDatapoints;
                bool resultDatapointsIsImmutable = false;
                if (cacheQueryResult.IsEmpty)
                {
                    resultDatapoints = await Query_MainAsync(
                        readOnlyDbContext,
                        queryType_UpperCase,
                        filter,
                        queryString_Values,                        
                        new MissingRange(fromTime_UnixTimeMilliseconds, toTime_UnixTimeMilliseconds),
                        returnPreFromDatapoint: true);
                    if (resultDatapoints.Count > 1)
                    {
                        await _cache.GrafanaCache_SyncRoot.WaitAsync();
                        try
                        {
                            _cache.GrafanaCache[cacheKey] = new ImmutableTimeSeriesCache(resultDatapoints);
                            resultDatapointsIsImmutable = true;
                        }
                        finally
                        {
                            _cache.GrafanaCache_SyncRoot.Release();
                        }
                    }
                }
                else
                {
                    if (cacheQueryResult.LeftMissingRange is null && cacheQueryResult.RightMissingRange is null) 
                    {

                        // fromTime_UnixTimeMilliseconds >= immutableTimeSeriesCache.CachedFromTimeInclusive_UnixTimeMilliseconds
                        // toTime_UnixTimeMilliseconds <= immutableTimeSeriesCache.CachedToTimeInclusive_UnixTimeMilliseconds
                        resultDatapoints = immutableTimeSeriesCache!.Datapoints.GetRange(cacheQueryResult.CachedDatapointsSegment.Offset, cacheQueryResult.CachedDatapointsSegment.Length);
                    }
                    else
                    {
                        List<List<decimal>>? leftMissingRange_Datapoints = cacheQueryResult.LeftMissingRange is null ? null : await Query_MainAsync(
                            readOnlyDbContext,
                            queryType_UpperCase,
                            filter,
                            queryString_Values,                            
                            cacheQueryResult.LeftMissingRange.Value,
                            returnPreFromDatapoint: true);
                        List<List<decimal>>? rightMissingRange_Datapoints = cacheQueryResult.RightMissingRange is null ? null : await Query_MainAsync(
                            readOnlyDbContext,
                            queryType_UpperCase,
                            filter,
                            queryString_Values,                            
                            cacheQueryResult.RightMissingRange.Value,
                            returnPreFromDatapoint: false);

                        if (leftMissingRange_Datapoints is not null && rightMissingRange_Datapoints is null)
                        {
                            resultDatapoints = new List<List<decimal>>(leftMissingRange_Datapoints.Count + cacheQueryResult.CachedDatapointsSegment.Length);
                            resultDatapoints.AddRange(leftMissingRange_Datapoints);
                            for (int i = 0; i < cacheQueryResult.CachedDatapointsSegment.Length; i++)
                                resultDatapoints.Add(immutableTimeSeriesCache!.Datapoints[cacheQueryResult.CachedDatapointsSegment.Offset + i]);

                            var datapoints = new List<List<decimal>>(leftMissingRange_Datapoints.Count + immutableTimeSeriesCache!.Datapoints.Count);
                            datapoints.AddRange(leftMissingRange_Datapoints);
                            datapoints.AddRange(immutableTimeSeriesCache!.Datapoints);

                            if (datapoints.Count > 1)
                            {
                                await _cache.GrafanaCache_SyncRoot.WaitAsync();
                                try
                                {
                                    _cache.GrafanaCache[cacheKey] = new ImmutableTimeSeriesCache(datapoints);                                    
                                }
                                finally
                                {
                                    _cache.GrafanaCache_SyncRoot.Release();
                                }
                            }
                        }
                        else if (leftMissingRange_Datapoints is null && rightMissingRange_Datapoints is not null)
                        {
                            resultDatapoints = new List<List<decimal>>(cacheQueryResult.CachedDatapointsSegment.Length + rightMissingRange_Datapoints.Count);
                            for (int i = 0; i < cacheQueryResult.CachedDatapointsSegment.Length; i++)
                                resultDatapoints.Add(immutableTimeSeriesCache!.Datapoints[cacheQueryResult.CachedDatapointsSegment.Offset + i]);
                            resultDatapoints.AddRange(rightMissingRange_Datapoints);

                            var datapoints = new List<List<decimal>>(immutableTimeSeriesCache!.Datapoints.Count + rightMissingRange_Datapoints.Count);
                            datapoints.AddRange(immutableTimeSeriesCache!.Datapoints);
                            datapoints.AddRange(rightMissingRange_Datapoints);

                            if (datapoints.Count > 1)
                            {
                                await _cache.GrafanaCache_SyncRoot.WaitAsync();
                                try
                                {
                                    _cache.GrafanaCache[cacheKey] = new ImmutableTimeSeriesCache(datapoints);                                    
                                }
                                finally
                                {
                                    _cache.GrafanaCache_SyncRoot.Release();
                                }
                            }
                        }
                        else
                        {
                            resultDatapoints = new List<List<decimal>>(leftMissingRange_Datapoints!.Count + immutableTimeSeriesCache!.Datapoints.Count + rightMissingRange_Datapoints!.Count);
                            resultDatapoints.AddRange(leftMissingRange_Datapoints);
                            resultDatapoints.AddRange(immutableTimeSeriesCache!.Datapoints);
                            resultDatapoints.AddRange(rightMissingRange_Datapoints);

                            if (resultDatapoints.Count > 1)
                            {
                                await _cache.GrafanaCache_SyncRoot.WaitAsync();
                                try
                                {
                                    _cache.GrafanaCache[cacheKey] = new ImmutableTimeSeriesCache(resultDatapoints);
                                    resultDatapointsIsImmutable = true;
                                }
                                finally
                                {
                                    _cache.GrafanaCache_SyncRoot.Release();
                                }
                            }
                        }
                    }
                }
                
                if (resultDatapoints.Count > maxDataPoints && toTime_UnixTimeMilliseconds > fromTime_UnixTimeMilliseconds)
                {
                    var newResultDatapoints = new List<List<decimal>>(maxDataPoints);

                    decimal deltaMilliseconds = (toTime_UnixTimeMilliseconds - fromTime_UnixTimeMilliseconds) / (maxDataPoints - 1);

                    decimal time = toTime_UnixTimeMilliseconds;
                    int i = resultDatapoints.Count - 1;
                    while (time >= fromTime_UnixTimeMilliseconds && i >= 0)
                    {
                        while (i >= 0)
                        {
                            var datapoint = resultDatapoints[i];
                            if (datapoint[1] <= time)
                            {
                                newResultDatapoints.Add(new List<decimal>() { datapoint[0], Math.Round(datapoint[1], 0) });
                                break;
                            }
                            i -= 1;
                        }

                        time -= deltaMilliseconds;
                    }

                    resultDatapoints = newResultDatapoints;
                    resultDatapointsIsImmutable = false;
                }                

                if (resultDatapoints.Count > 0)
                {
                    var lastDataPoint = resultDatapoints[^1];
                    if (lastDataPoint[1] < toTime_UnixTimeMilliseconds)
                    {
                        if (resultDatapointsIsImmutable)
                        {
                            var newResultDatapoints = new List<List<decimal>>(resultDatapoints.Count + 1);
                            newResultDatapoints.AddRange(resultDatapoints);
                            resultDatapoints = newResultDatapoints;
                        }
                        resultDatapoints.Add(new List<decimal> { lastDataPoint[0], toTime_UnixTimeMilliseconds });
                    }
                }

                taskCompletionSource.SetResult(new Query200ResponseInner(
                        new Query200ResponseInnerAnyOf(@"", resultDatapoints)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job unhandled exception.");
                taskCompletionSource.SetResult(new Query200ResponseInner(
                        new Query200ResponseInnerAnyOf(@"", new List<List<decimal>>())));
            }                
        });

        return taskCompletionSource.Task;            
    }

    /// <summary>
    ///     Preconditions: missingRange.FromTimeInclusive_UnixTimeMilliseconds строго меньше missingRange.ToTimeInclusive_UnixTimeMilliseconds
    ///     Может вернуть пустую коллекцию.
    ///     Позднейшая точка может совпадать, а может быть раньше missingRange.ToTimeInclusive_UnixTimeMilliseconds
    ///     returnPreFromDatapoint=false Ранняя точка строго позже missingRange.FromTimeInclusive_UnixTimeMilliseconds
    ///     returnPreFromDatapoint=true Ранняя точка или совпадает или раньше missingRange.FromTimeInclusive_UnixTimeMilliseconds
    /// </summary>
    /// <param name="readOnlyDbContext"></param>
    /// <param name="queryType_UpperCase"></param>
    /// <param name="filter"></param>
    /// <param name="queryString_Values"></param>    
    /// <param name="missingRange"></param>
    /// <param name="returnPreFromDatapoint"></param>
    /// <returns></returns>
    private async Task<List<List<decimal>>> Query_MainAsync(
        PazCheckDbContext readOnlyDbContext, 
        string queryType_UpperCase, 
        Filter filter,        
        List<string>? queryString_Values,        
        MissingRange missingRange,
        bool returnPreFromDatapoint)
    {
        var datapoints = new List<List<decimal>>();

        var fromTime_UnixTimeMilliseconds = missingRange.FromTime_UnixTimeMilliseconds;
        var toTime_UnixTimeMilliseconds = missingRange.ToTime_UnixTimeMilliseconds;        

        switch (queryType_UpperCase)
        { 
            case PazCheckConstants.QueryType_Values_UpperCase:
            case PazCheckConstants.QueryType_Value_UpperCase:
                if (queryString_Values is not null && queryString_Values.Count > 0)
                {
                    FilterHelper.Prepare(filter, _cache.DbCache);

                    var pcObjects = (await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObject), filter, null)).ToList();

                    List<FloatJournalParamValue> journalParamValues = new();                    
                    
                    if (pcObjects.Count == 1 && queryString_Values.Count == 1) // Optimization for simple case
                    {
                        string queryString_Value = queryString_Values[0];
                        PcObject pcObject = (PcObject)pcObjects[0];

                        if (queryString_Value.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                        {
                            int iterationCount = 0;
                            (string stored_ParamName, Func<float, float>? trendCalculatedConversion) =
                                PazCheckDbHelper.GetJournalParamValuesCollections_Info(
                                            pcObject,
                                            queryString_Value,
                                            ref iterationCount);

                            var journalParamValuesCollection = pcObject.JournalParamValuesCollections.FirstOrDefault(c => String.Equals(stored_ParamName, c.ParamName, StringComparison.InvariantCultureIgnoreCase));
                            if (journalParamValuesCollection is not null)
                            {
                                if (returnPreFromDatapoint)
                                {
                                    if (journalParamValuesCollection.CurrentValue is not null &&
                                            !float.IsNaN(journalParamValuesCollection.CurrentValue.Value) &&
                                            !float.IsInfinity(journalParamValuesCollection.CurrentValue.Value) &&
                                            fromTime_UnixTimeMilliseconds >= journalParamValuesCollection.CurrentValue_TimestampUtc)
                                    {
                                        journalParamValues.Add(new FloatJournalParamValue
                                        {
                                            TimestampUtc = journalParamValuesCollection.CurrentValue_TimestampUtc,
                                            Value = journalParamValuesCollection.CurrentValue.Value
                                        });
                                    }
                                    else
                                    {
                                        var q = readOnlyDbContext.FloatJournalParamValues
                                                .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc <= fromTime_UnixTimeMilliseconds)
                                                .OrderByDescending(v => v.TimestampUtc)
                                                .Take(1);
                                        q = q.Union(readOnlyDbContext.FloatJournalParamValues
                                                    .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc > fromTime_UnixTimeMilliseconds && v.TimestampUtc <= toTime_UnixTimeMilliseconds))
                                            .OrderByDescending(v => v.TimestampUtc);
                                        journalParamValues.AddRange(await q.ToListAsync());
                                    }
                                }
                                else
                                {
                                    if (journalParamValuesCollection.CurrentValue is not null &&
                                            !float.IsNaN(journalParamValuesCollection.CurrentValue.Value) &&
                                            !float.IsInfinity(journalParamValuesCollection.CurrentValue.Value) &&
                                            fromTime_UnixTimeMilliseconds >= journalParamValuesCollection.CurrentValue_TimestampUtc)
                                    {
                                        // Nothing to add.
                                    }
                                    else
                                    {
                                        var q = readOnlyDbContext.FloatJournalParamValues
                                                                    .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc > fromTime_UnixTimeMilliseconds && v.TimestampUtc <= toTime_UnixTimeMilliseconds)
                                                                    .OrderByDescending(v => v.TimestampUtc);
                                        journalParamValues.AddRange(await q.ToListAsync());
                                    }
                                }

                                foreach (var journalParamValue in journalParamValues.OrderBy(jpv => jpv.TimestampUtc))
                                {
                                    float journalParamValue_Value = journalParamValue.Value;
                                    // Incorrect data fix.
                                    if (float.IsNaN(journalParamValue_Value) || float.IsInfinity(journalParamValue_Value))
                                        continue;
                                    if (trendCalculatedConversion is not null)
                                        journalParamValue_Value = trendCalculatedConversion(journalParamValue_Value);

                                    decimal time = journalParamValue.TimestampUtc;
                                    decimal value = (decimal)journalParamValue_Value;                                    
                                    datapoints.Add(new List<decimal> { value, time });
                                }
                            }
                        }
                        else // !queryString_Value.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase)
                        {
                            decimal value;
                            string valueString = PazCheckDbHelper.GetParamValue<string>(pcObject.ParamsDictionary, pcObject.BasePcObject?.ParamsDictionary, queryString_Value, @"");
                            value = new Any(valueString).ValueAsDecimal(false);
                            var currentDataPoint = new List<decimal> { value, toTime_UnixTimeMilliseconds };
                            datapoints.Add(currentDataPoint);
                        }
                    }
                    else
                    {
                        foreach (PcObject pcObject in pcObjects)
                            foreach (var journalParamValuesCollection in pcObject.JournalParamValuesCollections)
                            {
                                var journalParamCondition = queryString_Values.FirstOrDefault(v => String.Equals(v, journalParamValuesCollection.ParamName, StringComparison.InvariantCultureIgnoreCase));
                                if (String.IsNullOrEmpty(journalParamCondition))
                                    continue;

                                JournalParamValuesCollectionInfo journalParamValuesCollectionInfo = new();

                                List<FloatJournalParamValue> fList;
                                if (returnPreFromDatapoint)
                                {
                                    if (journalParamValuesCollection.CurrentValue is not null &&
                                            !float.IsNaN(journalParamValuesCollection.CurrentValue.Value) &&
                                            !float.IsInfinity(journalParamValuesCollection.CurrentValue.Value) &&
                                            fromTime_UnixTimeMilliseconds >= journalParamValuesCollection.CurrentValue_TimestampUtc)
                                    {
                                        fList = new List<FloatJournalParamValue> { 
                                            new FloatJournalParamValue
                                            {
                                                TimestampUtc = journalParamValuesCollection.CurrentValue_TimestampUtc,
                                                Value = journalParamValuesCollection.CurrentValue.Value
                                            }};
                                    }
                                    else
                                    {
                                        var q = readOnlyDbContext.FloatJournalParamValues
                                            .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc <= fromTime_UnixTimeMilliseconds)
                                            .OrderByDescending(v => v.TimestampUtc)
                                            .Take(1);
                                        q = q.Union(readOnlyDbContext.FloatJournalParamValues
                                                    .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc > fromTime_UnixTimeMilliseconds && v.TimestampUtc <= toTime_UnixTimeMilliseconds))
                                            .OrderByDescending(v => v.TimestampUtc);
                                        fList = await q.ToListAsync();
                                    }
                                }
                                else
                                {
                                    if (journalParamValuesCollection.CurrentValue is not null &&
                                            !float.IsNaN(journalParamValuesCollection.CurrentValue.Value) &&
                                            !float.IsInfinity(journalParamValuesCollection.CurrentValue.Value) &&
                                            fromTime_UnixTimeMilliseconds >= journalParamValuesCollection.CurrentValue_TimestampUtc)
                                    {
                                        fList = new List<FloatJournalParamValue>();
                                    }
                                    else
                                    {
                                        var q = readOnlyDbContext.FloatJournalParamValues
                                                                    .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc > fromTime_UnixTimeMilliseconds && v.TimestampUtc <= toTime_UnixTimeMilliseconds)
                                                                    .OrderByDescending(v => v.TimestampUtc);
                                        fList = await q.ToListAsync();
                                    }
                                }
                                
                                fList.ForEach(v => v.Obj = journalParamValuesCollectionInfo); // Because of Entity Framework optimizations.
                                journalParamValues.AddRange(fList);
                            }

                        List<decimal>? currentDataPoint = null;
                        foreach (var journalParamValue in journalParamValues.OrderBy(v => v.TimestampUtc))
                        {                            
                            // Incorrect data fix.
                            if (float.IsNaN(journalParamValue.Value) || float.IsInfinity(journalParamValue.Value))
                                continue;

                            var journalParamValuesCollectionInfo = (JournalParamValuesCollectionInfo)journalParamValue.Obj!;

                            decimal time = journalParamValue.TimestampUtc;
                            decimal value = (decimal)journalParamValue.Value;
                            if (currentDataPoint is null)
                            {
                                journalParamValuesCollectionInfo.CurrentValue = value;
                                currentDataPoint = new List<decimal> { value, time };
                                if (datapoints.Count == 0 || datapoints[^1][1] < time)
                                    datapoints.Add(currentDataPoint);
                            }
                            else
                            {
                                decimal valueDelta = value - journalParamValuesCollectionInfo.CurrentValue;
                                journalParamValuesCollectionInfo.CurrentValue = value;
                                if (time > currentDataPoint[1])
                                {
                                    currentDataPoint = new List<decimal> { currentDataPoint[0] + valueDelta, time };
                                    if (datapoints.Count == 0 || datapoints[datapoints.Count - 1][1] < time)
                                        datapoints.Add(currentDataPoint);
                                }
                                else
                                {
                                    currentDataPoint[0] += valueDelta;
                                }
                            }
                        }
                    }                             
                }
                break;
            //case PazCheckCentralServerConstants.QueryType_SafetyIndexFromChildren:                            
            case PazCheckConstants.QueryType_ObjectsCount_UpperCase:
                {
                    FilterHelper.Prepare(filter, _cache.DbCache);

                    decimal count = (await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObject), filter, null)).Count();

                    datapoints.Add(new List<decimal> { count, toTime_UnixTimeMilliseconds });
                }
                break;
            case PazCheckConstants.QueryType_EventsCount_UpperCase:
                {
                    FilterHelper.Prepare(filter, _cache.DbCache);

                    decimal count = (await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObjectEvent), filter, null)).Count();

                    datapoints.Add(new List<decimal> { count, toTime_UnixTimeMilliseconds });
                }
                break;
            case PazCheckConstants.QueryType_ActiveEventsCount_UpperCase:
                {
                    List<FloatJournalParamValue> journalParamValues = new();

                    FilterHelper.Prepare(filter, _cache.DbCache);

                    foreach (PcObjectEvent pcObjectEvent in await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObjectEvent), filter, null))
                    {
                        journalParamValues.Add(new FloatJournalParamValue { TimestampUtc = new DateTimeOffset(pcObjectEvent.BeginTimeUtc).ToUnixTimeMilliseconds(), Value = 1 });
                        if (pcObjectEvent.EndTimeUtc is not null)
                            journalParamValues.Add(new FloatJournalParamValue { TimestampUtc = new DateTimeOffset(pcObjectEvent.EndTimeUtc.Value).ToUnixTimeMilliseconds(), Value = -1 });
                    }

                    List<decimal>? currentDataPoint = null;
                    foreach (var journalParamValue in journalParamValues.OrderBy(v => v.TimestampUtc))
                    {
                        long time = journalParamValue.TimestampUtc;
                        decimal value = (decimal)journalParamValue.Value;
                        if (currentDataPoint is null)
                        {
                            currentDataPoint = new List<decimal> { value, time };
                            if (returnPreFromDatapoint || (!returnPreFromDatapoint && time > fromTime_UnixTimeMilliseconds))
                                datapoints.Add(currentDataPoint);
                        }
                        else
                        {
                            decimal valueDelta = value;
                            if (time > currentDataPoint[1])
                            {
                                currentDataPoint = new List<decimal> { currentDataPoint[0] + valueDelta, time };
                                if (returnPreFromDatapoint || (!returnPreFromDatapoint && time > fromTime_UnixTimeMilliseconds))
                                    datapoints.Add(currentDataPoint);
                            }
                            else
                            {
                                currentDataPoint[0] += valueDelta;
                            }
                        }
                    }
                }
                break;
        }

        return datapoints;
    }

    private static string GetLevel(InformationSecurityEvent r)
    {
        switch (r.Severity)
        {
            case 1:
            case 2:
            case 3:
                return "debug";
            case 4:
            case 5:
            case 6:
                return "info";
            case 7:
            case 8:
                return "warn";
            case 9:
            case 10:
                return "error";
            default:
                return "unknown";
        };
    }

    private async Task<IEnumerable<Annotations200ResponseInner>> Annotations_PcObjectEvent(AnnotationsRequest annotationsRequest)
    {
        List<Annotations200ResponseInner> result = new();

        string? pcObjectIdentifier = null;
        string? eventType = null;
        //TimeSpan interval = TimeSpan.FromDays(30);
        try
        {
            var jsonElement = (JsonElement)annotationsRequest.Variables!;
            pcObjectIdentifier = jsonElement.GetProperty(PazCheckConstants.CriterionName_Unit).GetString(@"value");
            //interval = new Any(jsonElement.GetProperty(PropertyName_Interval).GetString(@"value"));
                            
            eventType = annotationsRequest.Annotation?.Query;
        }
        catch
        {
        }            

        if (!String.IsNullOrEmpty(pcObjectIdentifier) &&
            !String.IsNullOrEmpty(eventType))
        {
            //if (_cache.DbCache.PcObjectsDictionary1.TryGetValue(pcObjectIdentifier, out int pcObjectId))
            //{
            //    using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            //    readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            //    DateTime fromTimeUtc = annotationsRequest.Range!.From;
            //    DateTime toTimeUtc = annotationsRequest.Range!.To;
            //    FilterInfo filter = new()
            //    {
            //        CriteriaInfosList = [
            //            new CriteriaInfo { CriteriaName = @"Id", ValuesList = [new Any(pcObjectId).ValueAsString(false)] },
            //            //new CriteriaInfo { CriteriaName = Common.Properties.Resources.CriteriaName_EventType, ValuesList = [eventType] },
            //            //new CriteriaInfo { CriteriaName = PazCheckCentralServerConstants.ParamName_BeginTime, ValuesList = [ DateTimeHelper.GetString(fromTimeUtc) ] },
            //            //new CriteriaInfo { CriteriaName = PazCheckCentralServerConstants.ParamName_EndTime, ValuesList = [ DateTimeHelper.GetString(toTimeUtc) ] },
            //            //new CriteriaInfo { CriteriaName = Common.Properties.Resources.ParamDesc_EventStatus, ValuesList = [new Any(toTimeUtc).ValueAsString(false, "g")] }
            //            //new CriteriaInfo { CriteriaName = Common.Properties.Resources.ParamDesc_SafetyIndex, ValuesList = [new Any(toTimeUtc).ValueAsString(false, "g")] }
            //        ]
            //    };

            //    foreach (PcObjectEvent pcObjectEvent in
            //        (await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObjectEvent), filter, null))
            //            .OfType<PcObjectEvent>()
            //            )
            //    {
            //        //var desc = PazCheckDbHelper.GetDesc(new Any(pcObjectEvent.ParamsDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_CommandActuationType)).ValueAs<TriggeredType>(false));
            //        //result.Add(new Annotations200ResponseInner()
            //        //{
            //        //    Text = desc,
            //        //    Title = pcObjectEvent.Title,
            //        //    IsRegion = false,
            //        //    Time = new Any(new DateTimeOffset(pcObjectEvent.BeginTimeUtc).ToUnixTimeMilliseconds()).ValueAsString(false),
            //        //    Tags = new List<string>() { desc }
            //        //});
            //    }
            //}
        }

        return result;
    }

    #endregion

    #region private fields

    private readonly IMainServerWorker _mainServerWorker;
    private readonly Cache _cache;
    private readonly JobsManager _jobsManager;
    private readonly AddonsManager _addonsManager;       
    private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;        
    private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
    private readonly ILogger _logger;
    private readonly CancellationToken _applicationStopping_CancellationToken;

    private readonly LongRunningTask _longRunningTask;

    #endregion

    private class JournalParamValuesCollectionInfo
    {
        public decimal CurrentValue = 0;
    }
}


//var scopedVarsJsonElement = (JsonElement)queryRequest.ScopedVars!;
//if (!partParsedJsonFilterInfo.ContainsKey(PazCheckCentralServerConstants.CriterionName_RootPcObject_Identifier) &&
//        scopedVarsJsonElement.TryGetProperty(@"object", out JsonElement objectJsonElement))
//    partParsedJsonFilterInfo.Add(PazCheckCentralServerConstants.CriterionName_RootPcObject_Identifier, new() { objectJsonElement.GetString(@"value") ?? "" });
//if (!partParsedJsonFilterInfo.ContainsKey(PazCheckCentralServerConstants.CriterionName_From))
//    partParsedJsonFilterInfo.Add(PazCheckCentralServerConstants.CriterionName_From, new() { new Any(queryRequest.Range!.From).ValueAsString(false) });
//if (!partParsedJsonFilterInfo.ContainsKey(PazCheckCentralServerConstants.CriterionName_To))
//    partParsedJsonFilterInfo.Add(PazCheckCentralServerConstants.CriterionName_To, new() { new Any(queryRequest.Range!.To).ValueAsString(false) });

//criteriaDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_BeginTime, out string? beginTimeString);
//criteriaDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_EndTime, out string? endTimeString);
//DateTime toTimeUtc = DateTimeHelper.GetDateTimeUtc(endTimeString);
//if (toTimeUtc == default)
//    toTimeUtc = nowUtc;
//DateTime fromTimeUtc = DateTimeHelper.GetDateTimeUtc(beginTimeString);
//if (fromTimeUtc == default)
//    fromTimeUtc = toTimeUtc;
//criteriaDictionary[PazCheckCentralServerConstants.ParamName_BeginTime] = DateTimeHelper.GetString(fromTimeUtc);
//criteriaDictionary[PazCheckCentralServerConstants.ParamName_EndTime] = DateTimeHelper.GetString(toTimeUtc);

//criteriaDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_BeginTime, out string? beginTimeString);
//criteriaDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_EndTime, out string? endTimeString);
//DateTime toTimeUtc = DateTimeHelper.GetDateTimeUtc(endTimeString);
//if (toTimeUtc == default)
//    toTimeUtc = nowUtc;
//DateTime fromTimeUtc = DateTimeHelper.GetDateTimeUtc(beginTimeString);
//if (fromTimeUtc == default)
//    fromTimeUtc = toTimeUtc;
//criteriaDictionary[PazCheckCentralServerConstants.ParamName_BeginTime] = DateTimeHelper.GetString(fromTimeUtc);
//criteriaDictionary[PazCheckCentralServerConstants.ParamName_EndTime] = DateTimeHelper.GetString(toTimeUtc);





//return queryResponse;

//private static T GetValue<T>(System.Text.Json.JsonElement? jsonElement, string key, T defaultValue)
//            where T : notnull
//{
//    if (jsonElement is null)
//        return defaultValue;
//    var valueString = jsonElement.Value.GetString(key);
//    if (String.IsNullOrEmpty(valueString))
//        return defaultValue;
//    var result = new Any(valueString!).ValueAs<T>(false);
//    if (result is null)
//        return defaultValue;
//    return result;
//}

//List<object> row = new();
//foreach (var it in
//    (await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, typeof(PcObjectEvent), filter, null))
//        .OfType<PcObjectEvent>()
//        .Select(e => (PazCheckDbHelper.GetDesc(new Any(e.ParamsDictionary.TryGetValue(PazCheckCentralServerConstants.ParamName_CommandActuationType)).ValueAs<TriggeredType>(false)), e))
//        .GroupBy(i => i.Item1))
//{
//    columns.Add(new Query200ResponseInnerAnyOf1ColumnsInner(it.Key, @"number"));
//    row.Add(it.Count());
//}
//rows.Add(row.ToArray());


//public const string PropertyName_ParamK = @"k";

//public const string PropertyName_ParamBias = @"bias";

//private async Task<Query200ResponseInner> Query_Param(QueryRequest queryRequest, QueryRequestTargetsInner queryRequestTarget)
//{
//    string? pcObjectIdentifier = null;
//    string? paramName = null;
//    decimal paramK = 1;
//    decimal paramBias = 0;
//    try
//    {
//        var jsonElement = (JsonElement)queryRequestTarget.Data!;
//        pcObjectIdentifier = jsonElement.GetString(PazCheckCentralServerConstants.ParamName_PcObjectIdentifier);

//        paramName = jsonElement.GetString(PropertyName_Param);

//        paramK = GetValue<decimal>(jsonElement, PropertyName_ParamK, 1);
//        paramBias = GetValue<decimal>(jsonElement, PropertyName_ParamBias, 0);

//        if (String.IsNullOrEmpty(paramName))
//            paramName = PazCheckCentralServerConstants.ParamName_SafetyIndex;
//        if (String.IsNullOrEmpty(pcObjectIdentifier))
//        {
//            var scopedVarsJsonElement = (JsonElement)queryRequest.ScopedVars!;
//            pcObjectIdentifier = scopedVarsJsonElement.GetProperty(PazCheckCentralServerConstants.ParamName_PcObjectIdentifier).GetString(@"value");
//        }
//    }
//    catch
//    {
//    }

//    List<object[]> rows = new();

//    if (!String.IsNullOrEmpty(pcObjectIdentifier) &&
//        !String.IsNullOrEmpty(paramName))
//    {
//        if (_cache.PcObjectIdsDictionary.TryGetValue(pcObjectIdentifier, out int pcObjectId))
//        {
//            using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
//            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

//            var pcObject = await readOnlyDbContext.PcObjects
//                .FirstOrDefaultAsync(o => o.Id == pcObjectId);

//            if (pcObject is not null)
//            {
//                if (String.Equals(paramName, PazCheckCentralServerConstants.ParamName_SafetyIndex, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    rows.Add([PazCheckCentralServerConstants.ParamName_SafetyIndex, (decimal)pcObject.SafetyIndex * paramK + paramBias]);
//                }
//            }
//        }
//    }

//    var queryResponse =
//        new Query200ResponseInner(
//            new Query200ResponseInnerAnyOf1(
//                Query200ResponseInnerAnyOf1.TypeEnum.Table,
//                new List<Query200ResponseInnerAnyOf1ColumnsInner>() {
//                        new Query200ResponseInnerAnyOf1ColumnsInner(@"Name", @"string"),
//                        new Query200ResponseInnerAnyOf1ColumnsInner(@"Value", @"number"),
//                    },
//                rows));

//    return queryResponse;
//}

