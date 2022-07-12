using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Ssz.Utils.Addons;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Project")]
    public class ProjectController : ControllerBase
    {
        #region construction and destruction

        public ProjectController(            
            JobsManager jobsManager,
            AddonsManager addonsManager,
            IHostApplicationLifetime applicationLifetime,
            ILogger<ProjectController> logger)
        {            
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        //[HttpGet("HasUnsavedChanges")]
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

        #endregion        

        #region private fields
        
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}
