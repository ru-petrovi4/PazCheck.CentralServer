using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Presentation
{    
    public partial class ProjectController : ControllerBase
    {
        #region public functions

        [HttpGet(@"CompareVersions/{projectId}")]
        public async Task<IActionResult> CompareVersionsAsync(int projectId, uint projectVersionNum1, uint projectVersionNum2)
        {
            uint minProjectVersionNum = Math.Min(projectVersionNum1, projectVersionNum2);
            uint maxProjectVersionNum = Math.Max(projectVersionNum1, projectVersionNum2);            
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    //ProjectVersion minProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == minVersionNum);
                    //ProjectVersion maxProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == maxVersionNum);

                    List<object> result = new();

                    result.Add(await CompareVersionsBaseActuatorsAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum));
                    result.Add(await CompareVersionsTagsAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum));
                    result.Add(await CompareVersionsCeMatricesAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum));

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid projectId: {0}", projectId);

                return NotFound();
            }            
        }

        #endregion

        #region private functions

        private async Task<object> CompareVersionsBaseActuatorsAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint maxProjectVersionNum)
        {
            var minBaseActuators = await dbContext.BaseActuators.Where(ba => (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > minProjectVersionNum)).ToArrayAsync();

            return new object();
        }

        private async Task<object> CompareVersionsTagsAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint maxProjectVersionNum)
        {
            await Task.Delay(0);
            return new object();
        }

        private async Task<object> CompareVersionsCeMatricesAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint maxProjectVersionNum)
        {
            await Task.Delay(0);
            return new object();
        }

        #endregion
    }
}
