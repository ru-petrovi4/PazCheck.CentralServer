using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Progress")]
    public class ProgressController : ControllerBase
    {
        #region construction and destruction

        public ProgressController(ILogger<ProgressController> logger, JobsManager jobsManager)
        {
            _logger = logger;
            _jobsManager = jobsManager;
        }

        #endregion

        #region public functions

        [HttpGet]
        public async Task<IActionResult> GetJobsListAsync()
        {
            var jobsList = await _jobsManager.GetJobsListAsync();
            return Ok(new { data = jobsList });
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobProgressAsync(string jobId)
        {
            var progress = await _jobsManager.GetProgressAsync(jobId);
            var isFinished = false;
            var isStarted = true;
            if (progress >= 100)
            {
                progress = 100;
                isFinished = true;
            }
            else if (progress <= 0)
            {
                progress = 0;
                isStarted = false;
            }
            return Ok(new { guid = jobId, progress, isFinished, isStarted });
        }

        #endregion        

        #region private fields

        private ILogger _logger;

        private JobsManager _jobsManager;

        #endregion
    }
}
