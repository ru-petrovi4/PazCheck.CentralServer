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
using System.Text.Json;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using System.Collections.Immutable;

namespace Simcode.PazCheck.CentralServer.MicroServices
{
    public class MetaParamsToEvents : IDisposable
    {
        #region construction and destruction

        public MetaParamsToEvents(
            IConfiguration configuration,
            IMainServerWorker mainServerWorker,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,            
            IHubContext<MainHub> hubContext,
            IHostApplicationLifetime applicationLifetime,
            ILogger<MetaParamsToEvents> logger)
        {
            _configuration = configuration;
            _mainServerWorker = mainServerWorker;
            _dbContextFactory = dbContextFactory;            
            _hubContext = hubContext;            
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

            _callbackWorkingTask = CallbackWorkingTaskMainAsync(_callbackWorkingTask_CancellationTokenSource.Token);
        }

        public void Dispose()
        {
            _callbackWorkingTask_CancellationTokenSource.Cancel();
        }

        #endregion        

        #region private functions  

        private async Task CallbackWorkingTaskMainAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(2000, cancellationToken);
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
            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var metaParamsList = await readOnlyDbContext.MetaParams.ToListAsync();

            List<string> paused_MetaParamNames = new List<string>();

            foreach (var metaParam in metaParamsList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string metaParamPrefix = PazCheckConstants.MetaParamNameBase_Paused + PazCheckConstants.MetaParamName_FieldsSeparator;
                if (metaParam.ParamName.StartsWith(metaParamPrefix))
                {
                    if (new Any(metaParam.Value).ValueAsBoolean(false))
                        paused_MetaParamNames.Add(metaParam.ParamName.Substring(metaParamPrefix.Length));
                }
            }

            foreach (var metaParam in metaParamsList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool sendInternal = false;
                bool sendHub = false;
                string? processedMetaParamValue;

                if (paused_MetaParamNames.Any(p => metaParam.ParamName.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                switch (metaParam.Type)
                {
                    case PazCheckConstants.MetaParam_Type_InternalEvent:
                        if (_processedMetaParamValues.TryGetValue(metaParam.ParamName, out processedMetaParamValue))
                        {
                            if (processedMetaParamValue == metaParam.Value)
                            {
                                sendInternal = false;
                            }
                            else
                            {
                                _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                                sendInternal = true;
                            }
                        }
                        else
                        {
                            _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                            sendInternal = false;
                        }
                        break;
                    case PazCheckConstants.MetaParam_Type_MainProcess_InternalEvent:
                        if (ConfigurationHelper.IsMainProcess(_configuration))
                        {
                            if (_processedMetaParamValues.TryGetValue(metaParam.ParamName, out processedMetaParamValue))
                            {
                                if (processedMetaParamValue == metaParam.Value)
                                {
                                    sendInternal = false;
                                }
                                else
                                {
                                    _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                                    sendInternal = true;
                                }
                            }
                            else
                            {
                                _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                                sendInternal = false;
                            }                            
                        }                        
                        break;
                    case PazCheckConstants.MetaParam_Type_HubEvent:
                        if (_processedMetaParamValues.TryGetValue(metaParam.ParamName, out processedMetaParamValue))
                        {
                            if (processedMetaParamValue == metaParam.Value)
                            {
                                sendHub = false;
                            }
                            else
                            {
                                _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                                sendHub = true;
                            }
                        }
                        else
                        {
                            _processedMetaParamValues[metaParam.ParamName] = metaParam.Value;
                            sendHub = true;
                        }                        
                        break;
                }  

                if (sendInternal || sendHub)
                {
                    MetaParamArg? metaParamHubArg;
                    if (metaParam.HasArg)
                        metaParamHubArg = readOnlyDbContext.MetaParamArgs.FirstOrDefault(a => a.ParamName == metaParam.ParamName);
                    else
                        metaParamHubArg = null;

                    var hubArgFields = NameValueCollectionHelper.Parse(metaParamHubArg?.Arg);

                    if (sendInternal)
                    {
                        var eventMessage = new EventMessage(new Ssz.Utils.DataAccess.EventId()
                        {
                            OccurrenceId = metaParam.Value
                        })
                        {
                            EventType = EventType.SystemEvent,
                            OccurrenceTimeUtc = metaParam._LastChangeTimeUtc,
                            Fields = hubArgFields
                        };

                        _mainServerWorker.NotifyEventMessages(@"local", // Only for local subscribers (MainServerWorker.DataAccessProvider)
                            new EventMessagesCollection
                            {
                                EventMessages = new List<EventMessage> { eventMessage }
                            });
                    }

                    if (sendHub)
                    {
                        try
                        {
                            IClientProxy clientProxy;
                            if (String.IsNullOrEmpty(metaParamHubArg?.ExcludeConnectionIds))
                            {
                                if (metaParam.Group == @"")
                                    clientProxy = _hubContext.Clients.All;
                                else
                                    clientProxy = _hubContext.Clients.Group(metaParam.Group);
                            }
                            else
                            {
                                var excludeConnectionIds = CsvHelper.ParseCsvLine(",", metaParamHubArg.ExcludeConnectionIds).Select(s => s ?? @"");
                                if (metaParam.Group == @"")
                                    clientProxy = _hubContext.Clients.AllExcept(excludeConnectionIds);
                                else
                                    clientProxy = _hubContext.Clients.GroupExcept(metaParam.Group, excludeConnectionIds);
                            }

                            if (hubArgFields.Count == 0)
                            {
                                await clientProxy.SendAsync(metaParam.Method);
                            }
                            else
                            {
                                if (metaParam.Method == PazCheckConstants.HubMethod_Monitoring_DataChanged)
                                {
                                    await clientProxy.SendAsync(
                                            metaParam.Method,
                                            CsvHelper.ParseCsvLine(@",", hubArgFields.TryGetValue(@"Ids")).Select(s => new Any(s).ValueAsInt32(false)).ToArray(),
                                            CsvHelper.ParseCsvLine(@",", hubArgFields.TryGetValue(@"SafetyIndices")).Select(s => new Any(s).ValueAsSingle(false)).ToArray(),
                                            CsvHelper.ParseCsvLine(@",", hubArgFields.TryGetValue(@"SafetyIndexDescs")).Select(s => s ?? @"").ToArray());
                                }
                                else
                                {
                                    await clientProxy.SendAsync(metaParam.Method, hubArgFields);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "MainHubCallback DoWorkAsync Exception.");
                        }
                    }                                        
                }
            }
        }     

        #endregion

        #region private fields

        private readonly IConfiguration _configuration;
        private readonly IMainServerWorker _mainServerWorker;

        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;        

        private readonly IHubContext<MainHub> _hubContext;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        private Task _callbackWorkingTask = null!;

        private readonly CancellationTokenSource _callbackWorkingTask_CancellationTokenSource = new CancellationTokenSource();

        private readonly Dictionary<string, string> _processedMetaParamValues = new();        

        #endregion
    }    
}