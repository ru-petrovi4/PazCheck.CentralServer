using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssz.Utils;
using Simcode.PazCheck.CentralServer.Common;
using Microsoft.Extensions.Hosting;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class JobsManager
    {
        public JobsManager(IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime)
        {
            ServiceProvider = serviceProvider;

            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

            Task.Factory.StartNew(() =>
            {
                ExecuteJobAsync().Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public void QueueJob(string jobId, Func<CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(this, _applicationStopping_CancellationToken);
            JobInfos.Add(jobId, jobInfo);            
            var cancellationToken = jobInfo.CancellationToken;
            Task.Run(() =>
            {
                asyncAction(cancellationToken, jobInfo).Wait();
            });            
        }

        public void QueueJobWithDispatcher(string jobId, Func<IDispatcher, CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(this, _applicationStopping_CancellationToken);
            JobInfos.Add(jobId, jobInfo);              
            ThreadSafeDispatcher.BeginAsyncInvoke(ct => asyncAction(ThreadSafeDispatcher, jobInfo.CancellationToken, jobInfo));                        
        }        

        public async Task<int> GetProgressAsync(string guid)
        {
            //await _signal.WaitAsync();
            JobInfos.TryGetValue(guid, out var jobInfo);
            if (jobInfo is null)
                return 100;
            //_signal.Release();
            if (jobInfo.FinishedDateTimeUtc != null)
                return 100;
            return (int)jobInfo.ProgressPercent;
            //using var scope = ServiceProvider.CreateScope();
            //var context = scope.ServiceProvider.GetRequiredService<PazCheckDbContext>();
            //var currentJob = await context.Jobs.Where(job => job.Guid == guid).FirstOrDefaultAsync();
            //if (currentJob != null)
            //{
            //    return 101;
            //}

            //return -1;
        }        

        public Task<List<JobInfo>> GetJobsListAsync()
        {
            //await _signal.WaitAsync();
            var jobsList = JobInfos.Values.ToList();
            //_signal.Release();
            return Task.FromResult(jobsList);
        }        

        #region private functions

        private async Task ExecuteJobAsync()
        {
            while (true)
            {
                if (_applicationStopping_CancellationToken.IsCancellationRequested) break;
                await Task.Delay(3);
                if (_applicationStopping_CancellationToken.IsCancellationRequested) break;

                await ThreadSafeDispatcher.InvokeActionsInQueueAsync(_applicationStopping_CancellationToken);                
            }
        }

        private readonly CancellationToken _applicationStopping_CancellationToken;

        private CaseInsensitiveDictionary<JobInfo> JobInfos { get; } = new();

        private IServiceProvider ServiceProvider { get; }

        private ThreadSafeDispatcher ThreadSafeDispatcher { get; } = new();

        #endregion
    }
}