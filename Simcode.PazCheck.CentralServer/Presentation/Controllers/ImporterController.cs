using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Importer")]
    public class ImporterController : ControllerBase
    {
        #region construction and destruction

        public ImporterController(
            TagsImporter tagsImporter,
            JobsManager jobsManager,
            AddonsManager addonsManager,
            IHostApplicationLifetime applicationLifetime,
            ILogger<ImporterController> logger)
        {
            _tagsImporter = tagsImporter;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        [HttpPost("ImportEventsJournalFiles/{unitId}")]
        public async Task<IActionResult> ImportEventsJournalFilesAsync(List<IFormFile> formFiles, string unitId)
        {
            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();
            foreach (var formFile in formFiles)
            {
                if (formFile.Length > 0)
                {
                    var fileFullName = Path.GetRandomFileName();
                    await using var tempFileStream = System.IO.File.Create(fileFullName);
                    await formFile.CopyToAsync(tempFileStream, _applicationStopping_CancellationToken);
                    var jobId = Guid.NewGuid().ToString();
                    jobIds.Add(jobId);
                    if (!_applicationStopping_CancellationToken.IsCancellationRequested)
                    {
                        _jobsManager.QueueJob(jobId, "Import Events Journal File",
                            async (cancellationToken, jobProgress) =>
                            {
                                var eventMessagesProcessingAddon = _addonsManager.Addons.OfType<EventMessagesProcessingAddonBase>().OrderBy(a => a.IsDummy).FirstOrDefault();
                                if (eventMessagesProcessingAddon is null)
                                {
                                    // TODO TEMP CODE
                                    if (jobProgress is not null)
                                        await jobProgress.ReportAsync(100, null, null, false);
                                    return;
                                }

                                using (var dbContext = new PazCheckDbContext())
                                {
                                    Unit unit;
                                    try
                                    {
                                        unit = dbContext.Units.Single(u => u.Id == unitId);
                                    }
                                    catch
                                    {
                                        _logger.LogError("Invali unitId: {0}", unitId);
                                        return;
                                    }

                                    using var stream = System.IO.File.OpenRead(fileFullName);
                                    await eventMessagesProcessingAddon.ImportEventsJournalFileAsync(stream, formFile.FileName, dbContext, unit, cancellationToken, jobProgress);
                                }
                            });
                    }
                }
            }

            return Ok(new { count = formFiles.Count, size, guids = jobIds.ToArray() });
        }

        //[HttpPost("Project")]
        //public async Task<IActionResult> ImportProjectAsync(IFormFile formFile)
        //{
        //    string guid = @"";

        //    if (formFile.Length > 0)
        //    {
        //        var filePath = Path.GetRandomFileName();
        //        await using var tempFileStream = System.IO.File.Create(filePath);
        //        await formFile.CopyToAsync(tempFileStream, _applicationStopping_CancellationToken);
        //        guid = Guid.NewGuid().ToString();                
        //        if (!_applicationStopping_CancellationToken.IsCancellationRequested)
        //        {
        //            //_jobsManager.QueueJob(async token =>
        //            //    new ProjectTaskData { Id = guid, FilePath = filePath, Name = formFile.FileName }
        //            //);
        //        }
        //    }

        //    return Ok(new { size = formFile.Length, guid });
        //}

        //[HttpPost("tags/{prjId}")]
        //public async Task<IActionResult> ImportTagsAsync(List<IFormFile> files, int prjId)
        //{
        //    var size = files.Sum(f => f.Length);
        //    var jobIds = new List<string>();
        //    foreach (var formFile in files)
        //    {
        //        var jobId = Guid.NewGuid().ToString();
        //        jobIds.Add(jobId);
        //        var stream = formFile.OpenReadStream();
        //        _jobsManager.QueueJob(jobId, "Import Tags",
        //            (cancellationToken, jobProgress) => _tagsImporter.ImportTagsAsync(stream, prjId, jobProgress, cancellationToken)
        //            );
        //    }

        //    return Ok(new { count = files.Count, filesSize = size, guids = jobIds.ToArray() });
        //}        

        //[HttpGet("task")]
        //public async Task<IActionResult> ImportTaskAsync() //Не помню, зачем этот метод
        //{
        //    var guid = Guid.NewGuid().ToString();
        //    var isStarted = false;
        //    if (!_applicationStopping_CancellationToken.IsCancellationRequested)
        //    {
        //        isStarted = true;
        //        //_jobsManager.QueueJob(async token =>
        //        //{
        //        //    _logger.LogInformation("Forming Background Task {Guid} is starting.", guid);
        //        //    if (!token.IsCancellationRequested)
        //        //    {
        //        //        try
        //        //        {
        //        //            await Task.Delay(TimeSpan.FromSeconds(1), token);
        //        //        }
        //        //        catch (Exception e)
        //        //        {
        //        //            _logger.LogInformation("Exception: {Ex} is starting.", e);
        //        //        }

        //        //        _logger.LogInformation("Data for Background Task {Guid} has been formed.", guid);
        //        //    }

        //        //    return new LogTaskData { Id = guid };
        //        //});
        //    }

        //    return Ok(new { result = "Ok", isStarted, guid });
        //}

        //[HttpPost("diagram/{prjId}")]
        //public async Task<IActionResult> ImportDiagramAsync(List<IFormFile> files, int prjId)
        //{
        //    var size = files.Sum(f => f.Length);
        //    var guids = new List<string>();
        //    foreach (var formFile in files)
        //    {
        //        if (formFile.Length > 0)
        //        {
        //            var filePath = Path.GetRandomFileName();
        //            await using var tempFileStream = System.IO.File.Create(filePath);
        //            await formFile.CopyToAsync(tempFileStream, _applicationStopping_CancellationToken);
        //            var guid = Guid.NewGuid().ToString();
        //            guids.Add(guid);
        //            if (!_applicationStopping_CancellationToken.IsCancellationRequested)
        //            {
        //                //_jobsManager.QueueJob(async token =>
        //                //    new LogTaskData
        //                //    { Id = guid, FilePath = filePath, Name = formFile.FileName, UnitId = prjId }
        //                //);
        //            }
        //        }
        //    }
        //    return Ok(new { count = files.Count, size, guids = guids.ToArray() });
        //}

        #endregion        

        #region private fields

        private readonly TagsImporter _tagsImporter;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;

        private readonly ILogger _logger;

        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}


//public IActionResult Import()
//{
//    var path = HttpContext.Request.Path;
//    return Ok(path);
//}
