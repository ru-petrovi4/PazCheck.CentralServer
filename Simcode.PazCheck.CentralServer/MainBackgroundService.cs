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
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Presentation;
using Ssz.Utils;
using Ssz.Utils.Logging;
using Simcode.PazCheck.CentralServer.Common;

namespace Simcode.PazCheck.CentralServer
{
    public class MainBackgroundService : BackgroundService
    {        
        #region construction and destruction

        public MainBackgroundService(IServiceProvider serviceProvider,
            AddonsManager addonsManager,
            JobsManager jobsManager,
            ILogger<MainBackgroundService> logger,
            IConfiguration configuration)
        {
            JobsManager = jobsManager;
            Logger = logger;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            _addonsManager = addonsManager;
        }

        #endregion

        #region public functions        

        public ThreadSafeDispatcher ThreadSafeDispatcher { get; } = new();

        public CsvDb CsvDb { get; private set; } = null!;

        //public event Func<DateTime, CancellationToken, Task> DoWork;

        #endregion        

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation(
                $"Queued Hosted Service is running.{Environment.NewLine}" +
                $"{Environment.NewLine}Tap W to add a work item to the " +
                $"background queue.{Environment.NewLine}");

            CsvDb = ActivatorUtilities.CreateInstance<CsvDb>(
                ServiceProvider, ServerConfigurationHelper.GetProgramDataDirectoryInfo(Configuration), ThreadSafeDispatcher);            

            _addonsManager.Initialize(null, @"Simcode.PazCheck.Addons.*.dll", CsvDb);

            await LoadFixtures.Fixtures(ServiceProvider, Configuration, _addonsManager, true);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(3);
                if (cancellationToken.IsCancellationRequested) break;

                await ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);

                DateTime nowUtc = DateTime.UtcNow;

                //if (modelTimeValueSubscription.ValueStatusTimestamp.ValueStatusCode == ValueStatusCode.Good)
                //{
                //    int modelTimeSeconds = modelTimeValueSubscription.ValueStatusTimestamp.Value.ValueAsInt32(false);
                //    if (modelTimeSeconds > 0)
                //        device.DoWork((UInt64)modelTimeSeconds * 1000, nowUtc, cancellationToken);
                //}
                //

                //await DoWork(nowUtc, cancellationToken);
            }

            //using var scope = ServiceProvider.CreateScope();
            //var context = scope.ServiceProvider.GetRequiredService<PazCheckDbContext>();
            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    var workItem = await JobsManager.DequeueAsync(cancellationToken);
            //    try
            //    {
            //        var workingItem = workItem(cancellationToken);
            //        Logger.LogInformation($"Task with id {workingItem.Id} is proccessing...");
            //        var taskData = await workingItem;
            //        var taskType = "Unknown";
            //        if (taskData.GetType() == typeof(LogTaskData))
            //        {
            //            var logTaskData = (LogTaskData) taskData;
            //            var logLoader = scope.ServiceProvider.GetRequiredService<ILogLoader>();
            //            await Progress.AddAsync(taskData.Id, "Loading log " + logTaskData.Name);
            //            var jobTask = logLoader.ImportFileLogAsync(logTaskData.FilePath, logTaskData.Name,
            //                logTaskData.UnitId,
            //                cancellationToken, new Progress<int>(async val =>
            //                {
            //                    await Progress.SetProgressAsync(taskData.Id, val);
            //                    Logger.LogInformation($"Progress {val} of Job {taskData.Id}");
            //                })
            //            );
            //            _ = jobTask.ContinueWith(async tsk =>
            //            {
            //                var jobInfo = await Progress.GetJobInfoAsync(taskData.Id);
            //                await Progress.RemoveAsync(taskData.Id);
            //                var job = new Job
            //                {
            //                    Guid = taskData.Id,
            //                    Name = jobInfo.Name,
            //                    IsSuccess = true,
            //                    Message = $"Log {logTaskData.Name} has been loaded successfully.",
            //                    Status = tsk.Status
            //                };
            //                await context.Jobs.AddAsync(job, cancellationToken);
            //                await context.SaveChangesAsync(cancellationToken);
            //                File.Delete(logTaskData.FilePath);
            //                Logger.LogInformation($"After log loaded clear dictionary {taskData.Id}");
            //            }, cancellationToken);
            //            taskType = "Log";
            //        }

            //        if (taskData.GetType() == typeof(AnalyzeTaskData))
            //        {
            //            var analyzeTaskData = (AnalyzeTaskData) taskData;
            //            var calculator = scope.ServiceProvider.GetRequiredService<ResultsCalculatorService>();
            //            var log = await context.Logs.Where(l => l.Id == analyzeTaskData.LogId).FirstAsync(cancellationToken: cancellationToken);
            //            var logName = "Undefined";
            //            if (log!=null)
            //            {
            //                logName = log.Name;
            //            }
            //            await Progress.AddAsync(taskData.Id, "Analizing log " + logName);
            //            var jobTask = calculator.CalculateResultsAsync(analyzeTaskData.LogId, analyzeTaskData.StartTime,
            //                analyzeTaskData.EndTime,
            //                cancellationToken, new Progress<int>(async val =>
            //                {
            //                    await Progress.SetProgressAsync(taskData.Id, val);
            //                    Logger.LogInformation($"Progress {val} of Job {taskData.Id}");
            //                })
            //            );
            //            _ = jobTask.ContinueWith(async tsk =>
            //            {
            //                var jobInfo = await Progress.GetJobInfoAsync(taskData.Id);
            //                await Progress.RemoveAsync(taskData.Id);
            //                var job = new Job
            //                {
            //                    Guid = taskData.Id,
            //                    Name = jobInfo.Name,
            //                    IsSuccess = true,
            //                    Message = $"Analyze of log {logName} has been proccessed successfully.",
            //                    Status = tsk.Status
            //                };
            //                await context.Jobs.AddAsync(job, cancellationToken);
            //                await context.SaveChangesAsync(cancellationToken);
            //                Logger.LogInformation($"After analyze proccessed clear dictionary {taskData.Id}");
            //            }, cancellationToken);
            //            taskType = "Analyze";
            //        }

            //        if (taskData is GetExpData getExpData)
            //        {
            //            //var dataServerSection = _configuration.GetSection(@"DataServer");
            //            //if (dataServerSection == null) return;
            //            //var emseventsDbConnectionString = dataServerSection[@"EmseventsDbConnectionString"];
            //            //if (String.IsNullOrEmpty(emseventsDbConnectionString)) return;
            //            //LogTimeInterval logTimeInterval = ExperionHelper.GetLogFromExprionDb(NullLogger.Instance, emseventsDbConnectionString);

            //            //if (logTimeInterval != null && logTimeInterval.LogEventsCollection.Count > 0)
            //            //{                            
            //            //    var logevents = logTimeInterval.LogEventsCollection.OrderBy(l => l.EventTime).ToList();
            //            //    var log = new Log
            //            //    {
            //            //        Name = "ExpLog",
            //            //        Unit = await context.Units.FirstAsync(u => u.Id == getExpData.unitId, cancellationToken: stoppingToken),
            //            //        Start = logevents.First().EventTime,
            //            //        End = logevents.Last().EventTime,
            //            //        Logevents = logevents
            //            //    };
            //            //    context.Logs.Add(log);

            //            //    await context.SaveChangesAsync(stoppingToken);
            //            //}
            //        }

            //        if (taskData is DiagramImportData diagramImportData)
            //        {
            //            CeMatrixTemp? ceMatrix = CeMatrixHelper.ParseCsvCeMatrix(Logger, Path.GetFileNameWithoutExtension(diagramImportData.FilePath),
            //                File.ReadAllText(diagramImportData.FilePath));
            //            if (ceMatrix != null)
            //            {
            //                var project = await context.Projects.FirstAsync(p => p.Id == diagramImportData.ProjectId);
            //                CeMatrixHelper.SaveCeMatrixToDb(context, project, ceMatrix, Logger);
            //            }
            //        }

            //        if (taskData.GetType() == typeof(TagsTaskData))
            //        {
            //            var tagsTaskData = (TagsTaskData) taskData;
            //            var scopedService = scope.ServiceProvider.GetRequiredService<TagsImporter>();
            //            var jobTask = scopedService.ImportTags(tagsTaskData.Project, cancellationToken);
            //            taskType = "Log";
            //        }

            //        if (taskData.GetType() == typeof(ProjectTaskData))
            //        {
            //            var projectTaskData = (ProjectTaskData) taskData;
            //            var scopedService = scope.ServiceProvider.GetRequiredService<IUnitHelper>();
            //            await Progress.AddAsync(taskData.Id, "Importing new project" + projectTaskData.Name);
            //            var jobTask = scopedService.ImportUnitAsync(projectTaskData.FilePath,
            //                cancellationToken, new Progress<int>(async val =>
            //                {
            //                    await Progress.SetProgressAsync(taskData.Id, val);
            //                    Logger.LogInformation($"Progress {val} of Job {taskData.Id}");
            //                })
            //            );
            //            _ = jobTask.ContinueWith(async tsk =>
            //            {
            //                var jobInfo = await Progress.GetJobInfoAsync(taskData.Id);
            //                await Progress.RemoveAsync(taskData.Id);
            //                var job = new Job
            //                {
            //                    Guid = taskData.Id,
            //                    Name = jobInfo.Name,
            //                    IsSuccess = true,
            //                    Message = $"Project has been imported successfully.",
            //                    Status = tsk.Status
            //                };
            //                await context.Jobs.AddAsync(job, cancellationToken);
            //                await context.SaveChangesAsync(cancellationToken);
            //                Logger.LogInformation($"After log loaded clear dictionary {taskData.Id}");
            //            }, cancellationToken);
            //            taskType = "Project";
            //        }

            //        Logger.LogInformation($"Found task with type: {taskType} and with Guid: {taskData.Id}.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
            //    }
            //}
        }        

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }

        #region private functions       

        private JobsManager JobsManager { get; }

        private AddonsManager _addonsManager;

        private ILogger<MainBackgroundService> Logger { get; }

        private IConfiguration Configuration { get; }

        private IServiceProvider ServiceProvider { get; }        

        #endregion
    }
}
