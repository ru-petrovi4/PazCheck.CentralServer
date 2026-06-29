using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;

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

        /// <summary>
        ///     Get all current jobs Ids.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetRunningJobsList")]
        public async Task<IActionResult> GetRunningJobsListAsync()
        {
            var user = HttpContextHelper.GetUserLowerInvariant(HttpContext);

            var jobsList = await _jobsManager.GetJobsListAsync(user);
            return Ok(new { data = jobsList });
        }

        /// <summary>        
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetJobProgress/{jobId}")]        
        public async Task<IActionResult> GetJobProgressAsync(string jobId)
        {
            Job? job = await _jobsManager.GetJobProgressAsync(jobId);
            if (job is not null)
                return Ok(new { data = job });
            else
                return BadRequest();
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Projects_Supervise),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_Calculate),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) }
            )]
        [HttpGet(@"CancelJob/{jobId}")]
        public async Task<IActionResult> CancelJobAsync(string jobId)
        {
            await _jobsManager.CancelJobAsync(jobId);
            return Ok();
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Projects_Supervise),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_Calculate),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) }
            )]
        [HttpGet(@"ContinueJob/{jobId}")]
        public async Task<IActionResult> ContinueJobAsync(string jobId)
        {
            await _jobsManager.ContinueJobAsync(jobId);
            return Ok();
        }

        #endregion        

        #region private fields

        private ILogger _logger;

        private JobsManager _jobsManager;

        #endregion
    }
}
