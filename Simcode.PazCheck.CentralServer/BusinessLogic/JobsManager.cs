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
using Grpc.Core;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class JobsManager
    {
        public JobsManager(IHostApplicationLifetime applicationLifetime)
        {
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;

            Task.Factory.StartNew(() =>
            {
                ExecuteJobAsync().Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public async void QueueJob(string jobId, string jobName, Func<CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(jobId, jobName, _applicationStopping_CancellationToken);

            await _jobInfosSyncRoot.WaitAsync();
            try
            {
                _jobInfos.Add(jobId, jobInfo);
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }       

            var cancellationToken = jobInfo.CancellationTokenSource.Token;
            var jobTask = Task.Run(() =>
            {
                asyncAction(cancellationToken, jobInfo).Wait();
            });
            _ = jobTask.ContinueWith(async t =>
            {                
                await _jobInfosSyncRoot.WaitAsync();
                try
                {
                    _jobInfos.Remove(jobId);
                }
                finally
                {
                    _jobInfosSyncRoot.Release();
                }

                Job job = await jobInfo.CreateJobAsync();
                if (job.FinishedTimeUtc is null)
                {
                    job.StatusCode = (uint)StatusCode.Unknown;
                    job.FinishedTimeUtc = DateTime.UtcNow;
                }                
                using (var dbContext = new PazCheckDbContext())
                {
                    await dbContext.Jobs.AddAsync(job, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }, cancellationToken);
        }

        //public void QueueJobWithDispatcher(string jobId, string jobName, Func<IDispatcher, CancellationToken, IJobProgress, Task> asyncAction)
        //{
        //    var jobInfo = new JobInfo(this, jobName, _applicationStopping_CancellationToken);
        //    _jobInfos.Add(jobId, jobInfo);              
        //    ThreadSafeDispatcher.BeginAsyncInvoke(ct => asyncAction(ThreadSafeDispatcher, jobInfo.CancellationToken, jobInfo));                        
        //}        

        public async Task<Job?> GetProgressAsync(string jobId)
        {
            JobInfo? jobInfo;

            await _jobInfosSyncRoot.WaitAsync();
            try
            {
                _jobInfos.TryGetValue(jobId, out jobInfo);
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }

            if (jobInfo is not null)
                return await jobInfo.CreateJobAsync();

            using (var dbContext = new PazCheckDbContext())
            {
                var job = await dbContext.Jobs.Where(job => job.Id == jobId).FirstOrDefaultAsync();
                if (job is not null)
                    return job;
            }

            return null;
        }

        public async Task<List<Job>> GetJobsListAsync()
        {
            List<JobInfo> jobsInfosList;

            await _jobInfosSyncRoot.WaitAsync();
            try
            {
                jobsInfosList = _jobInfos.Values.ToList();
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }

            List<Job> jobsList = new();
            foreach (var jobInfo in jobsInfosList)
            {
                jobsList.Add(await jobInfo.CreateJobAsync());
            }
            return jobsList;
        }

        #region private functions

        private async Task ExecuteJobAsync()
        {
            while (true)
            {
                if (_applicationStopping_CancellationToken.IsCancellationRequested) break;
                await Task.Delay(3);
                if (_applicationStopping_CancellationToken.IsCancellationRequested) break;

                await _threadSafeDispatcher.InvokeActionsInQueueAsync(_applicationStopping_CancellationToken);                
            }
        }

        #endregion

        #region private fields

        private readonly CancellationToken _applicationStopping_CancellationToken;

        private readonly CaseInsensitiveDictionary<JobInfo> _jobInfos = new();

        private readonly SemaphoreSlim _jobInfosSyncRoot = new SemaphoreSlim(1);

        private ThreadSafeDispatcher _threadSafeDispatcher = new();

        #endregion
    }
}