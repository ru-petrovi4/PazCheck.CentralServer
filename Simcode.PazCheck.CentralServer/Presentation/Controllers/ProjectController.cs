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
    public partial class ProjectController : ControllerBase
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

        [HttpGet(@"HasUnversionedChanges/{projectId}")]
        public IActionResult HasUnversionedChanges(int projectId)
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
                    bool hasUnsavedChanges = dbContext.BaseActuators.Any(ba => ba.Project == project && ba._LastChangeTimeUtc > lastVersionTimeUtc);
                    if (hasUnsavedChanges)
                        return Ok(true);
                    hasUnsavedChanges = dbContext.Tags.Any(t => t.Project == project && t._LastChangeTimeUtc > lastVersionTimeUtc);
                    if (hasUnsavedChanges)
                        return Ok(true);
                    hasUnsavedChanges = dbContext.CeMatrices.Any(m => m.Project == project && m._LastChangeTimeUtc > lastVersionTimeUtc);
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

        [HttpPost(@"SaveUnversionedChanges/{projectId}")]
        public async Task<IActionResult> SaveUnversionedChangesAsync(int projectId, string user, string comment)
        {
            try
            {
                using (var dbContext = new PazCheckDbContext
                    {
                        IsLastChangeTimeUtcUpdatingDisabled = true
                    })
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
                        User = user ?? @"",
                        Comment = comment ?? @""
                    };
                    project.LastProjectVersion = projectVersion;
                    project.ProjectVersions.Add(projectVersion);

                    foreach (var baseActuator in dbContext.BaseActuators.Where(ba => ba.Project == project && ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(ba => ba.BaseActuatorParams)
                        .Include(ba => ba.BaseActuatorDbFileReferences))
                    {
                        foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, baseActuatorParam);
                        }

                        foreach (var baseActuatorDbFileReference in baseActuator.BaseActuatorDbFileReferences)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, baseActuatorDbFileReference);
                        }

                        baseActuator._LockedByUser = @"";
                        SaveUnversionedChanges(dbContext, projectVersion.VersionNum, baseActuator);
                    }

                    foreach (var tag in dbContext.Tags.Where(t => t.Project == project && t._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.TagParams)
                        .Include(t => t.ActuatorParams)                        
                        .Include(t => t.TagConditions))
                    {
                        foreach (var tagParam in tag.TagParams)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, tagParam);
                        }

                        foreach (var actuatorParam in tag.ActuatorParams)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, actuatorParam);
                        }                        

                        foreach (var tagCondition in tag.TagConditions)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, tagCondition);
                        }

                        tag._LockedByUser = @"";
                        SaveUnversionedChanges(dbContext, projectVersion.VersionNum, tag);
                    }

                    foreach (var ceMatrix in dbContext.CeMatrices.Where(m => m.Project == project && m._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.CeMatrixParams)
                        .Include(t => t.CeMatrixDbFileReferences)
                        .Include(t => t.Causes)
                        .ThenInclude(c => c.SubCauses)
                        .Include(t => t.Effects)
                        .Include(t => t.Intersections))
                    {
                        foreach (var ceMatrixParam in ceMatrix.CeMatrixParams)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, ceMatrixParam);
                        }

                        foreach (var ceMatrixDbFileReference in ceMatrix.CeMatrixDbFileReferences)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, ceMatrixDbFileReference);
                        }

                        foreach (var cause in ceMatrix.Causes)
                        {
                            foreach (var subCause in cause.SubCauses)
                            {
                                SaveUnversionedChanges(dbContext, projectVersion.VersionNum, subCause);
                            }

                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, cause);
                        }

                        foreach (var effect in ceMatrix.Effects)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, effect);
                        }

                        foreach (var intersection in ceMatrix.Intersections)
                        {
                            SaveUnversionedChanges(dbContext, projectVersion.VersionNum, intersection);
                        }

                        ceMatrix._LockedByUser = @"";
                        SaveUnversionedChanges(dbContext, projectVersion.VersionNum, ceMatrix);
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

        [HttpPost(@"ClearUnversionedChanges/{projectId}")]
        public async Task<IActionResult> ClearUnversionedChangesAsync(int projectId)
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

                    foreach (var baseActuator in dbContext.BaseActuators.Where(ba => ba.Project == project && ba._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(ba => ba.BaseActuatorParams)
                        .Include(ba => ba.BaseActuatorDbFileReferences))
                    {
                        foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
                        {
                            ClearUnversionedChanges(dbContext, baseActuatorParam);
                        }

                        foreach (var baseActuatorDbFileReferences in baseActuator.BaseActuatorDbFileReferences)
                        {
                            ClearUnversionedChanges(dbContext, baseActuatorDbFileReferences);
                        }

                        baseActuator._LockedByUser = @"";
                        ClearUnversionedChanges(dbContext, baseActuator);
                    }

                    foreach (var tag in dbContext.Tags.Where(t => t.Project == project && t._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.TagParams)
                        .Include(t => t.ActuatorParams)
                        .Include(t => t.TagConditions))
                    {
                        foreach (var tagParam in tag.TagParams)
                        {
                            ClearUnversionedChanges(dbContext, tagParam);
                        }

                        foreach (var actuatorParam in tag.ActuatorParams)
                        {
                            ClearUnversionedChanges(dbContext, actuatorParam);
                        }                        

                        foreach (var tagCondition in tag.TagConditions)
                        {
                            ClearUnversionedChanges(dbContext, tagCondition);
                        }

                        tag._LockedByUser = @"";
                        ClearUnversionedChanges(dbContext, tag);
                    }

                    foreach (var ceMatrix in dbContext.CeMatrices.Where(m => m.Project == project && m._LastChangeTimeUtc > lastVersionTimeUtc)
                        .Include(t => t.CeMatrixParams)
                        .Include(t => t.CeMatrixDbFileReferences)
                        .Include(t => t.Causes)
                        .ThenInclude(c => c.SubCauses)
                        .Include(t => t.Effects)
                        .Include(t => t.Intersections))
                    {
                        foreach (var ceMatrixParam in ceMatrix.CeMatrixParams)
                        {
                            ClearUnversionedChanges(dbContext, ceMatrixParam);
                        }

                        foreach (var ceMatrixDbFileReference in ceMatrix.CeMatrixDbFileReferences)
                        {
                            ClearUnversionedChanges(dbContext, ceMatrixDbFileReference);
                        }

                        foreach (var cause in ceMatrix.Causes)
                        {
                            foreach (var subCause in cause.SubCauses)
                            {
                                ClearUnversionedChanges(dbContext, subCause);
                            }

                            ClearUnversionedChanges(dbContext, cause);
                        }

                        foreach (var effect in ceMatrix.Effects)
                        {
                            ClearUnversionedChanges(dbContext, effect);
                        }

                        foreach (var intersection in ceMatrix.Intersections)
                        {
                            ClearUnversionedChanges(dbContext, intersection);
                        }

                        ceMatrix._LockedByUser = @"";
                        ClearUnversionedChanges(dbContext, ceMatrix);
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

        #endregion

        #region private functions

        private void SaveUnversionedChanges(PazCheckDbContext dbContext, uint projectVersionNum, VersionEntityBase versionEntity)
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

        private void ClearUnversionedChanges(PazCheckDbContext dbContext, VersionEntityBase versionEntity)
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
