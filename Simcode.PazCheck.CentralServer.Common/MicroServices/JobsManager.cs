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
using System.Reflection;
using System.Runtime;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices
{
    /// <summary>
    ///     Thread-safe
    ///     <para>In DB finished jobs stored 24h.</para>
    ///     <para>If <see cref="StatusCodes.GoodMoreData"/> job paused and waiting operator action.</para>
    ///     <para>Then set status code to <see cref="StatusCodes.BadRequestCancelledByClient"/> if operation is cancelled by user.</para>
    ///     <para>Then set status code to <see cref="StatusCodes.GoodCallAgain"/> if operation is continued by user.</para>
    /// </summary>
    public class JobsManager
    {
        #region construction and destruction

        public JobsManager(IDbContextFactory<PazCheckDbContext> dbContextFactory, ILogger<JobsManager> logger)
        { 
            _dbContextFactory = dbContextFactory;
            _logger = logger;

            ServerWorker_ThreadSafeDispatcher = new(_logger);
            AdditionalLongRunning_ThreadSafeDispatcher = new(_logger);
        }

        #endregion        

        #region public functions

        /// <summary>
        ///     For tasks in ServerWorker's thread.
        /// </summary>
        public ThreadSafeDispatcher ServerWorker_ThreadSafeDispatcher { get; }

        /// <summary>
        ///     For long-running tasks in additional thread.        
        /// </summary>
        public ThreadSafeDispatcher AdditionalLongRunning_ThreadSafeDispatcher { get; }        

        /// <summary>
        ///     Queue job in ServerWorker's thread.
        ///     asyncAction is awaited in InvokeActionsInQueueAsync(...)
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobTitle"></param>
        /// <param name="user"></param>
        /// <param name="asyncAction"></param>
        public async void QueueJobIn_ServerWorkerThread(string jobId, string jobTitle, string user, Func<CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(jobId, jobTitle, user);

            await AddJobToRunningJobInfosAsync(jobInfo);

            var taskCompletionSource = new TaskCompletionSource();

            ServerWorker_ThreadSafeDispatcher.BeginInvokeEx(async ct =>
            {
                try
                {                                       
                    await asyncAction(jobInfo.Job_CancellationTokenSource.Token, jobInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job unhandled exception.");
                }

                taskCompletionSource.SetResult();
            });            

            await taskCompletionSource.Task;

            await RemoveJobFromRunningJobInfosAsync(jobInfo);
        }

        /// <summary>
        ///     Queue long-running task in additional thread.
        ///     asyncAction is awaited in InvokeActionsInQueueAsync(...)
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobTitle"></param>
        /// <param name="user"></param>
        /// <param name="asyncAction"></param>
        public async void QueueJobIn_AdditionalLongRunningThread(string jobId, string jobTitle, string user, Func<CancellationToken, IJobProgress, Task> asyncAction)
        {
            var jobInfo = new JobInfo(jobId, jobTitle, user);

            await AddJobToRunningJobInfosAsync(jobInfo);

            var taskCompletionSource = new TaskCompletionSource();

            AdditionalLongRunning_ThreadSafeDispatcher.BeginInvokeEx(async ct =>
            {
                try
                {
                    await asyncAction(jobInfo.Job_CancellationTokenSource.Token, jobInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job unhandled exception.");
                }

                taskCompletionSource.SetResult();
            });            

            await taskCompletionSource.Task;

            await RemoveJobFromRunningJobInfosAsync(jobInfo);
        }        

        public async Task<List<Job>> GetJobsListAsync(string user)
        {
            DateTime dateTime = DateTime.UtcNow - TimeSpan.FromMinutes(5);
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                return await dbContext.Jobs.Where(job => job.User == user && (!job.EndTimeUtc.HasValue || job.EndTimeUtc > dateTime)).ToListAsync();
            }
        }

        public async Task<Job?> GetJobProgressAsync(string jobId)
        {            
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                return await dbContext.Jobs.Where(job => job.Id == jobId).FirstOrDefaultAsync();                
            }
        }

        public async Task CancelJobAsync(string jobId)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                // SQL injection safe
                await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"JobStatusCode\" = {StatusCodes.BadRequestCancelledByClient} WHERE \"Id\" = {jobId} AND \"EndTimeUtc\" IS NULL");
            }
        }

        public async Task ContinueJobAsync(string jobId)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                // SQL injection safe
                await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"JobStatusCode\" = {StatusCodes.GoodCallAgain} WHERE \"Id\" = {jobId} AND \"EndTimeUtc\" IS NULL");
            }
        }

        public async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (nowUtc < _lastDoWorkDateTimeUtc + TimeSpan.FromSeconds(1))
                return;
            _lastDoWorkDateTimeUtc = nowUtc;

            bool cleanUp = false;
            if (nowUtc > _lastCleanUpDateTimeUtc + TimeSpan.FromSeconds(60))
            {
                cleanUp = true;
                _lastCleanUpDateTimeUtc = nowUtc;
            }            

            await _jobInfosSyncRoot.WaitAsync();

            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                if (_runningJobInfos.Count > 0)
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext())
                    {
                        var jobs = await dbContext.Jobs.Where(j => (j.JobStatusCode == StatusCodes.BadRequestCancelledByClient ||
                                j.JobStatusCode == StatusCodes.GoodCallAgain) &&
                                j.EndTimeUtc == null)
                            .ToListAsync();
                        foreach (var job in jobs)
                        {
                            if (job.JobStatusCode == StatusCodes.BadRequestCancelledByClient)
                            {
                                if (_runningJobInfos.Remove(job.Id, out JobInfo? jobInfo))
                                {
                                    jobInfo.Job_CancellationTokenSource.Cancel();
                                    jobInfo.Job_ContinuationSemaphoreSlim.Release();
                                    // SQL injection safe
                                    await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"Jobs\" WHERE \"Id\" = {job.Id}");
                                    //await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"EndTimeUtc\" = {DateTime.UtcNow} WHERE \"Id\" = {job.Id}");                                        
                                }
                            }
                            else if (job.JobStatusCode == StatusCodes.GoodCallAgain)
                            {
                                if (_runningJobInfos.TryGetValue(job.Id, out JobInfo? jobInfo))
                                {
                                    jobInfo.Job_ContinuationSemaphoreSlim.Release();

                                    // SQL injection safe
                                    await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"JobStatusCode\" = {StatusCodes.Good} WHERE \"Id\" = {job.Id}");
                                }
                            }
                        }

                        foreach (var runningJobInfo in _runningJobInfos.Values.ToArray())
                        {
                            Job job = await runningJobInfo.CreateJobAsync();
                            await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"ProgressPercent\" = {job.ProgressPercent}, \"ProgressLabel\" = {job.ProgressLabel}, \"ProgressDetail\" = {job.ProgressDetail}, \"JobStatusCode\" = {job.JobStatusCode} WHERE \"Id\" = {job.Id}");
                        }
                    }
                }

                if (cleanUp)
                {
                    using var dbContext = _dbContextFactory.CreateDbContext();
                    DateTime dateTime = nowUtc - TimeSpan.FromHours(24);
                    await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"Jobs\" WHERE \"EndTimeUtc\" < {dateTime}");
                    await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"Jobs\" WHERE \"BeginTimeUtc\" < {dateTime}"); // Mulfunctioned jobs.
                }
            }
            catch
            {
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }
        }

        #endregion

        #region private functions

        private async Task AddJobToRunningJobInfosAsync(JobInfo jobInfo)
        {
            await _jobInfosSyncRoot.WaitAsync();
            try
            {
                _runningJobInfos.Add(jobInfo.JobId, jobInfo);
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }

            Job job = await jobInfo.CreateJobAsync();
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.Jobs.AddAsync(job);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task RemoveJobFromRunningJobInfosAsync(JobInfo jobInfo)
        {
            bool removed;
            await _jobInfosSyncRoot.WaitAsync();
            try
            {
                removed = _runningJobInfos.Remove(jobInfo.JobId);
            }
            finally
            {
                _jobInfosSyncRoot.Release();
            }

            if (removed)
            {
                var job = await jobInfo.CreateJobAsync();
                if (job.EndTimeUtc is null)
                {
                    job.JobStatusCode = Ssz.Utils.StatusCodes.BadUnexpectedError;
                    job.EndTimeUtc = DateTime.UtcNow;
                }
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    // SQL injection safe
                    await dbContext.Database.ExecuteSqlAsync($"UPDATE \"Jobs\" SET \"ProgressPercent\" = {job.ProgressPercent}, \"ProgressLabel\" = {job.ProgressLabel}, \"ProgressDetail\" = {job.ProgressDetail}, \"JobStatusCode\" = {job.JobStatusCode}, \"EndTimeUtc\" = {job.EndTimeUtc} WHERE \"Id\" = {job.Id}");
                }
            }
        }

        #endregion

        #region private fields
        
        private readonly ILogger _logger;
        
        private readonly CaseInsensitiveOrderedDictionary<JobInfo> _runningJobInfos = new();

        private readonly SemaphoreSlim _jobInfosSyncRoot = new SemaphoreSlim(1);
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;

        private DateTime _lastDoWorkDateTimeUtc = DateTime.UtcNow;
        private DateTime _lastCleanUpDateTimeUtc = DateTime.MinValue;
        
        #endregion
    }
}

//private DateTime _lastLongCleanUpDateTimeUtc = DateTime.MinValue;

//bool longCleanUp = false;
//if (nowUtc > _lastLongCleanUpDateTimeUtc + TimeSpan.FromHours(24))
//{
//    longCleanUp = true;
//    _lastLongCleanUpDateTimeUtc = nowUtc;
//}

//public void QueueJobInMainBackgroundServiceThread(string jobId, string jobName, Func<IDispatcher, CancellationToken, IJobProgress, Task> asyncAction)
//{
//    var jobInfo = new JobInfo(this, jobName, _applicationStopping_CancellationToken);
//    _jobInfos.Add(jobId, jobInfo);              
//    ThreadSafeDispatcher.BeginInvokeEx(ct => asyncAction(ThreadSafeDispatcher, jobInfo.CancellationToken, jobInfo));                        
//}  


//public async Task DoInMainBackgroundServiceThreadAsync(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken)
//{
//    SemaphoreSlim workEnded = new(0);
//    MainBackgroundService_ThreadSafeDispatcher.BeginInvokeEx(async ct =>
//    {
//        try
//        {
//            await asyncAction(cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Work unhandled exception.");
//        }
//        workEnded.Release();
//    });
//    await workEnded.WaitAsync();
//}