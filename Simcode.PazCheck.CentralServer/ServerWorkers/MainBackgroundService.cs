using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.MicroServices;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public class MainBackgroundService : BackgroundService
    {
        #region construction and destruction

        public MainBackgroundService(
            ILogger<MainBackgroundService> logger,
            MainServerWorker mainServerWorker,            
            JobsManager jobsManager,
            Cache cache,
            MetaParamsToEvents metaParamsToEvents)
        {
            Logger = logger;
            _mainServerWorker = mainServerWorker;            
            _jobsManager = jobsManager;            
            _cache = cache;
            _metaParamsToEvents = metaParamsToEvents;
        }

        #endregion

        #region public functions

        public ILogger<MainBackgroundService> Logger { get; }

        #endregion

        #region protected functions

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogDebug("ExecuteAsync begin.");

            await _mainServerWorker.InitializeAsync(cancellationToken);

            _additionalShortRunning_Task = AdditionalShortRunning_TaskMainAsync(cancellationToken);

            _additionalLongRunning_Task = AdditionalLongRunning_TaskMainAsync(cancellationToken);

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(20, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _jobsManager.ServerWorker_ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);

                    DateTime nowUtc = DateTime.UtcNow;

                    await _mainServerWorker.DoWorkAsync(nowUtc, cancellationToken);
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, @"MainBackgroundService.ExecuteAsync(...) Exception");
                }
            }            

            await _mainServerWorker.CloseAsync();
        }

        #endregion

        #region private functions

        private async Task AdditionalShortRunning_TaskMainAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(20, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    var nowUtc = DateTime.UtcNow;

                    await _jobsManager.DoWorkAsync(nowUtc, cancellationToken);
                    await _cache.DoWorkAsync(nowUtc, cancellationToken);
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, @"MainBackgroundService.AdditionalShortRunning_TaskMainAsync(...) Exception");
                }
            }            
        }

        private async Task AdditionalLongRunning_TaskMainAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(3, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _jobsManager.AdditionalLongRunning_ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, @"MainBackgroundService.AdditionalLongRunning_TaskMainAsync(...) Exception");
                }
            }
        }

        #endregion

        #region private fields

        private readonly MainServerWorker _mainServerWorker;        
        private readonly JobsManager _jobsManager;        
        private readonly Cache _cache;
        private readonly MetaParamsToEvents _metaParamsToEvents; // Do not delete, needed for internal thread.
        
        private Task? _additionalLongRunning_Task;
        private Task? _additionalShortRunning_Task;

        #endregion
    }
}
