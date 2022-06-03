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

        public void QueueJob(string jobId, string jobName, Func<CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(this, jobName, _applicationStopping_CancellationToken);
            JobInfos.Add(jobId, jobInfo);            
            var cancellationToken = jobInfo.CancellationToken;
            var jobTask = Task.Run(() =>
            {
                asyncAction(cancellationToken, jobInfo).Wait();
            });
            _ = jobTask.ContinueWith(async t =>
            {
                JobInfos.Remove(jobId);

                var job = new Job
                {
                    Guid = jobId,
                    Name = jobName,
                    IsSuccess = true,
                    Message = $"{jobName} has been completed successfully.",
                    Status = t.Status
                };
                using (var dbContext = new PazCheckDbContext())
                {
                    await dbContext.Jobs.AddAsync(job, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }, cancellationToken);
        }

        public void QueueJobWithDispatcher(string jobId, string jobName, Func<IDispatcher, CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(this, jobName, _applicationStopping_CancellationToken);
            JobInfos.Add(jobId, jobInfo);              
            ThreadSafeDispatcher.BeginAsyncInvoke(ct => asyncAction(ThreadSafeDispatcher, jobInfo.CancellationToken, jobInfo));                        
        }        

        public async Task<int> GetProgressAsync(string guid)
        {
            //await _signal.WaitAsync();
            JobInfos.TryGetValue(guid, out var jobInfo);
            if (jobInfo is not null)
            {
                if (jobInfo.FinishedDateTimeUtc != null)
                    return 100;
                return (int)jobInfo.ProgressPercent;
            }
            //_signal.Release();            

            using (var dbContext = new PazCheckDbContext())
            {
                var currentJob = await dbContext.Jobs.Where(job => job.Guid == guid).FirstOrDefaultAsync();
                if (currentJob != null)
                {
                    return 100;
                }
            }

            return 0;
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