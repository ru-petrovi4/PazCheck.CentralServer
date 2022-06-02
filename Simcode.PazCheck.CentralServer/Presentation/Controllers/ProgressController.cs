using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Simcode.PazCheck.CentralServer.BusinessLogic;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("progress")]
    public class ProgressController : ControllerBase
    {
        private JobsManager JobsManager { get; }

        public ProgressController(JobsManager jobsManager)
        {
            JobsManager = jobsManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobsListAsync()
        {
            var jobsList = await JobsManager.GetJobsListAsync();
            return Ok(new {data = jobsList});
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobProgressAsync(string jobId)
        {
            var progress = await JobsManager.GetProgressAsync(jobId);
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
            return Ok(new {guid = jobId, progress, isFinished, isStarted});
        }
    }
}
