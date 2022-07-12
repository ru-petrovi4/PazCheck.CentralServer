using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Addons;
using System;
using System.Linq;
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

        [HttpGet(@"HasUnsavedChanges/{projectId}")]
        public IActionResult HasUnsavedChanges(int projectId)
        {            
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    dbContext.Entry(project)
                        .Reference(p => p.LastProjectVersion)
                        .Load();
                    var lastVersionTimeUtc = project.LastProjectVersion?.TimeUtc ?? DateTime.MinValue;
                    bool hasUnsavedChanges = dbContext.BaseActuators.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
                    if (hasUnsavedChanges)
                        return Ok(true);
                    hasUnsavedChanges = dbContext.Tags.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
                    if (hasUnsavedChanges)
                        return Ok(true);
                    hasUnsavedChanges = dbContext.CeMatrices.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
                    if (hasUnsavedChanges)
                        return Ok(true);                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid projectId: {0}", projectId);

                return NotFound();
            }            

            return Ok(false);
        }

        [HttpPost(@"SaveChanges/{projectId}")]
        public async Task<IActionResult> SaveChangesAsync(int projectId, string user, string comment)
        {
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    Project project = dbContext.Projects.Single(p => p.Id == projectId);                    
                    dbContext.Entry(project)
                        .Reference(p => p.LastProjectVersion)
                        .Load();
                    var lastVersionTimeUtc = project.LastProjectVersion?.TimeUtc ?? DateTime.MinValue;
                    var projectVersion = new ProjectVersion()
                    {
                        VersionNum = (project.LastProjectVersion?.VersionNum ?? 0) + 1,
                        TimeUtc = DateTime.UtcNow,
                        User = user,
                        Comment = comment
                    };
                    project.LastProjectVersion = projectVersion;
                    project.ProjectVersions.Add(projectVersion);

                    foreach (var baseActuator in dbContext.BaseActuators.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(ba => ba.BaseActuatorParams))
                    {
                        foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, baseActuatorParam);
                        }

                        baseActuator._LockedByUser = @"";
                        SaveChanges(dbContext, projectVersion.VersionNum, baseActuator);
                    }

                    foreach (var tag in dbContext.Tags.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.ActuatorParams)
                        .Include(t => t.TagParams)
                        .Include(t => t.TagConditions))
                    {
                        foreach (var actuatorParam in tag.ActuatorParams)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, actuatorParam);
                        }

                        foreach (var tagParam in tag.TagParams)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, tagParam);
                        }

                        foreach (var tagCondition in tag.TagConditions)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, tagCondition);
                        }

                        tag._LockedByUser = @"";
                        SaveChanges(dbContext, projectVersion.VersionNum, tag);
                    }

                    foreach (var ceMatrix in dbContext.CeMatrices.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.Causes)
                        .ThenInclude(c => c.SubCauses)
                        .Include(t => t.Effects)
                        .Include(t => t.Intersections))
                    {
                        foreach (var cause in ceMatrix.Causes)
                        {
                            foreach (var subCause in cause.SubCauses)
                            {
                                SaveChanges(dbContext, projectVersion.VersionNum, subCause);
                            }

                            SaveChanges(dbContext, projectVersion.VersionNum, cause);
                        }

                        foreach (var effect in ceMatrix.Effects)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, effect);
                        }

                        foreach (var intersection in ceMatrix.Intersections)
                        {
                            SaveChanges(dbContext, projectVersion.VersionNum, intersection);
                        }

                        ceMatrix._LockedByUser = @"";
                        SaveChanges(dbContext, projectVersion.VersionNum, ceMatrix);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SaveChanges error. projectId: {0}", projectId);
            }

            return Ok();
        }        

        [HttpPost(@"ClearChanges/{projectId}")]
        public async Task<IActionResult> ClearChangesAsync(int projectId)
        {
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    dbContext.Entry(project)
                        .Reference(p => p.LastProjectVersion)
                        .Load();
                    var lastVersionTimeUtc = project.LastProjectVersion?.TimeUtc ?? DateTime.MinValue;                    

                    foreach (var baseActuator in dbContext.BaseActuators.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(ba => ba.BaseActuatorParams))
                    {
                        foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
                        {
                            ClearChanges(dbContext, baseActuatorParam);
                        }

                        baseActuator._LockedByUser = @"";
                        ClearChanges(dbContext, baseActuator);
                    }

                    foreach (var tag in dbContext.Tags.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.ActuatorParams)
                        .Include(t => t.TagParams)
                        .Include(t => t.TagConditions))
                    {
                        foreach (var actuatorParam in tag.ActuatorParams)
                        {
                            ClearChanges(dbContext, actuatorParam);
                        }

                        foreach (var tagParam in tag.TagParams)
                        {
                            ClearChanges(dbContext, tagParam);
                        }

                        foreach (var tagCondition in tag.TagConditions)
                        {
                            ClearChanges(dbContext, tagCondition);
                        }

                        tag._LockedByUser = @"";
                        ClearChanges(dbContext, tag);
                    }

                    foreach (var ceMatrix in dbContext.CeMatrices.Where(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.Causes)
                        .ThenInclude(c => c.SubCauses)
                        .Include(t => t.Effects)
                        .Include(t => t.Intersections))
                    {
                        foreach (var cause in ceMatrix.Causes)
                        {
                            foreach (var subCause in cause.SubCauses)
                            {
                                ClearChanges(dbContext, subCause);
                            }

                            ClearChanges(dbContext, cause);
                        }

                        foreach (var effect in ceMatrix.Effects)
                        {
                            ClearChanges(dbContext, effect);
                        }

                        foreach (var intersection in ceMatrix.Intersections)
                        {
                            ClearChanges(dbContext, intersection);
                        }

                        ceMatrix._LockedByUser = @"";
                        ClearChanges(dbContext, ceMatrix);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SaveChanges error. projectId: {0}", projectId);
            }

            return Ok();
        }
        
        //[HttpGet(@"CompareVersions/{projectId}")]
        //public async Task<IActionResult> CompareVersionsAsync(int projectId, uint version1, uint version2)
        //{
        //    uint minVersion = Math.Min(version1, version2);
        //    uint maxVersion = Math.Max(version1, version2);
        //    try
        //    {
        //        using (var dbContext = new PazCheckDbContext())
        //        {
        //            Project project = dbContext.Projects.Single(p => p.Id == projectId);
        //            dbContext.Entry(project)
        //                .Reference(p => p.LastProjectVersion)
        //                .Load();
        //            var lastVersionTimeUtc = project.LastProjectVersion?.TimeUtc ?? DateTime.MinValue;
        //            bool hasUnsavedChanges = dbContext.BaseActuators.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
        //            if (hasUnsavedChanges)
        //                return Ok(true);
        //            hasUnsavedChanges = dbContext.Tags.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
        //            if (hasUnsavedChanges)
        //                return Ok(true);
        //            hasUnsavedChanges = dbContext.CeMatrices.Any(ba => ba._LastChangeTimeUtc > lastVersionTimeUtc);
        //            if (hasUnsavedChanges)
        //                return Ok(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, @"Invalid projectId: {0}", projectId);

        //        return NotFound();
        //    }

        //    return Ok(false);
        //}

        #endregion

        #region private functions

        private void SaveChanges(PazCheckDbContext dbContext, uint projectVersionNum, VersionEntityBase versionEntity)
        {
            if (versionEntity._CreateProjectVersionNum is null)
            {
                if (versionEntity._IsDeleted)
                {
                    dbContext.Entry(versionEntity).State = EntityState.Deleted;
                }
                else
                {
                    versionEntity._CreateProjectVersionNum = projectVersionNum;
                }
            }
            else if (versionEntity._DeleteProjectVersionNum is null)
            {
                if (versionEntity._IsDeleted)
                {
                    versionEntity._DeleteProjectVersionNum = projectVersionNum;                    
                }                
            }
        }

        private void ClearChanges(PazCheckDbContext dbContext, VersionEntityBase versionEntity)
        {
            if (versionEntity._CreateProjectVersionNum is null)
            {
                dbContext.Entry(versionEntity).State = EntityState.Deleted;
            }
            else if (versionEntity._DeleteProjectVersionNum is null)
            {
                if (versionEntity._IsDeleted)
                {
                    versionEntity._IsDeleted = false;
                }
            }
        }

        #endregion

        #region private fields

        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}
