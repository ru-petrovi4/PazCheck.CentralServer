using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Addons;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            var maxBaseActuators = await dbContext.BaseActuators.Where(ba => (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= maxProjectVersionNum) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > maxProjectVersionNum)).ToArrayAsync();

            var existedBaseActuators = maxBaseActuators.Intersect(minBaseActuators, IdEqualityComparer<BaseActuator>.Instance).ToArray();
            var deletedBaseActuators = minBaseActuators.Except(existedBaseActuators, IdEqualityComparer<BaseActuator>.Instance);
            var addedBaseActuators = maxBaseActuators.Except(existedBaseActuators, IdEqualityComparer<BaseActuator>.Instance);

            foreach (var existedBaseActuator in existedBaseActuators)
            {
                dbContext.Entry(existedBaseActuator).Collection(ba => ba.BaseActuatorParams).Load();
                CompareVersionsParams(dbContext, existedBaseActuator.BaseActuatorParams, minProjectVersionNum, maxProjectVersionNum);
            }

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

        private object CompareVersionsParams<TParam>(PazCheckDbContext dbContext, List<TParam> paramsList, uint minProjectVersionNum, uint maxProjectVersionNum)
            where TParam: Param 
        {
            var minParams = paramsList.Where(ba => (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            var maxParams = paramsList.Where(ba => (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= maxProjectVersionNum) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > maxProjectVersionNum)).ToArray();

            var existedMaxParams = maxParams.Intersect(minParams, ParamNameEqualityComparer<TParam>.Instance).ToArray();
            var deletedMinParams = minParams.Except(existedMaxParams, ParamNameEqualityComparer<TParam>.Instance);
            var addedMaxParams = maxParams.Except(existedMaxParams, ParamNameEqualityComparer<TParam>.Instance);

            foreach (var existedMaxParam in existedMaxParams)
            {
                var existedMinParam = minParams.First(p => p.ParamName == existedMaxParam.ParamName);
                if (existedMaxParam.Value != existedMinParam.Value)
                {

                }
            }

            return new object();
        }

        #endregion

        private class IdEqualityComparer<TVersionEntityBase> : IEqualityComparer<TVersionEntityBase>
            where TVersionEntityBase : VersionEntityBase
        {
            public static readonly IdEqualityComparer<TVersionEntityBase> Instance = new();

            public bool Equals(TVersionEntityBase? leftObj, TVersionEntityBase? rightObj)
            {                               
                return leftObj?.Id == rightObj?.Id;
            }

            public int GetHashCode(TVersionEntityBase obj)
            {
                return 0;
            }
        }

        private class ParamNameEqualityComparer<TParam> : IEqualityComparer<TParam>
            where TParam : Param
        {
            public static readonly IdEqualityComparer<TParam> Instance = new();

            public bool Equals(TParam? leftObj, TParam? rightObj)
            {
                return leftObj?.ParamName == rightObj?.ParamName;
            }

            public int GetHashCode(TParam obj)
            {
                return 0;
            }
        }

    }
}
