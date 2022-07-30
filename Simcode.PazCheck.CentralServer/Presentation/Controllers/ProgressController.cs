using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

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
            Job? job = await _jobsManager.GetProgressAsync(jobId);
            if (job is not null)
                return Ok(new { data = job });
            else
                return BadRequest();
        }

        #endregion        

        #region private fields

        private ILogger _logger;

        private JobsManager _jobsManager;

        #endregion
    }
}
