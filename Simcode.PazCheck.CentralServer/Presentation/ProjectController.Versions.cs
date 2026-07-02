using IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using static Simcode.PazCheck.CentralServer.Presentation.ProjectController;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public partial class ProjectController : ControllerBase
    {
        #region public functions

        /// <summary>
        ///     Если user пустой, то анализирует все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"HasUnversionedChanges/{projectId}")]
        public IActionResult HasUnversionedChanges(int projectId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                return Ok(PazCheckDbHelper.HasUnversionedChanges(readOnlyDbContext, projectId, user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid projectId: {0}", projectId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Если user пустой, то сохраняет все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersionTypeId"></param>
        /// <param name="user"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost(@"SaveUnversionedChanges/{projectId}")]
        public async Task<IActionResult> SaveUnversionedChangesAsync(int projectId, int projectVersionTypeId, string user, string comment)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);            

            bool succeeded = false;
            string projectTitle = @"";
            try
            {
                var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    Project? project = dbContext.Projects
                        .SingleOrDefault(pv => pv.Id == projectId);
                    if (project is null)
                    {
                        _logger.LogError(@"Invalid projectId: {0}", projectId);
                        return NoContent();
                    }
                    projectTitle = project.Title;

                    ProjectVersionType? projectVersionType = dbContext.ProjectVersionTypes.SingleOrDefault(pvt => pvt.Id == projectVersionTypeId);
                    if (projectVersionType is null)
                    {
                        _logger.LogError(@"Invalid. projectVersionTypeId: {0}", projectVersionTypeId);
                        return NoContent();
                    }

                    await PazCheckDbHelper.SaveUnversionedChangesAsync(dbContext, projectId, projectVersionType, informationSecurityContext, user, comment);
                }

                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                    PazCheckDbHelper.AddOrUpdateMetaParam_Project(
                        dbContext,
                        metaParams,
                        new Common.Serialization.Project_ChangedMessage { ProjectId = projectId });

                    await dbContext.SaveChangesAsync();
                }

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SaveChanges error. projectId: {0}", projectId);

                //_informationSecurityEventsLogger.InformationSecurityEvent(user ?? @"", HttpContextHelper.GetSourceIpAddress(HttpContext), InformationSecurityEventPazCheckCentralServerConstants.SaveUnversionedChanges_EventId, false, Properties.Resources.SaveUnversionedChanges_Event);
            }
            finally
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                                informationSecurityContext.SourceIpAddress,
                                                informationSecurityContext.SourceHost,
                                                InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId,
                                                6,
                                                succeeded,
                                                Properties.Resources.SaveUnversionedChanges_EventName,
                                                user,
                                                "System",
                                                NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                                {
                                                    (@"ProjectId", $"{projectId}")
                                                }),
                                                Properties.Resources.SaveUnversionedChanges_EventDesc, projectTitle, comment);
            }

            return Ok();
        }

        /// <summary>
        ///     Если user пустой, то очищает все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost(@"ClearUnversionedChanges/{projectId}")]
        public async Task<IActionResult> ClearUnversionedChangesAsync(int projectId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);                   

            bool succeeded = false;
            string projectTitle = @"";
            try
            {
                var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    Project? project = dbContext.Projects
                        .SingleOrDefault(pv => pv.Id == projectId);
                    if (project is null)
                    {
                        _logger.LogError(@"Invalid projectId: {0}", projectId);
                        return NoContent();
                    }
                    projectTitle = project.Title;                    
                }

                await PazCheckDbHelper.ClearUnversionedChangesAsync(_dbContextFactory, projectId, user, loggersSet);

                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                    PazCheckDbHelper.AddOrUpdateMetaParam_Project(
                        dbContext,
                        metaParams,
                        new Common.Serialization.Project_ChangedMessage { ProjectId = projectId });

                    await dbContext.SaveChangesAsync();
                }

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SaveChanges error. projectId: {0}", projectId);                
            }
            finally
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                                informationSecurityContext.SourceIpAddress,
                                                informationSecurityContext.SourceHost,
                                                InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId,
                                                6,
                                                succeeded,
                                                Properties.Resources.ClearUnversionedChanges_EventName,
                                                user,
                                                "System",
                                                NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                                {
                                                    (@"ProjectId", $"{projectId}")
                                                }),
                                                Properties.Resources.ClearUnversionedChanges_EventDesc, projectTitle);
            }

            return Ok();
        }

        /// <summary>
        ///     Задает новую активную версию для проекта
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersionNum"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Supervise) }
            )]
        [HttpPost(@"SetActiveProjectVersion/{projectId}")]
        public async Task<IActionResult> SetActiveProjectVersionAsync(int projectId, UInt32 projectVersionNum)
        {
            bool succeeded = false;

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);            
            string projectTitle = @"";
            string projectVersionComment = @"";

            try
            {
                await using var dbContext = _dbContextFactory.CreateDbContext();
                dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                Project? project = dbContext.Projects
                    .Include(p => p.Unit)
                    .ThenInclude(u => u.ActiveProjectVersion)
                    .SingleOrDefault(pv => pv.Id == projectId);
                if (project is null)
                {
                    _logger.LogError(@"Invalid projectId: {0}", projectId);
                    return NoContent();
                }
                projectTitle = project.Title;

                ProjectVersion? oldProjectVersion = project.Unit.ActiveProjectVersion;
                ProjectVersion? projectVersion = dbContext.ProjectVersions.SingleOrDefault(pv => pv.ProjectId == projectId && pv.VersionNum == projectVersionNum);
                if (projectVersion is null)
                {
                    _logger.LogError(@"Invalid projectVersionNum: {0}", projectVersionNum);
                    return NoContent();
                }
                projectVersionComment = projectVersion.Comment;

                await PazCheckDbHelper.SetActiveProjectVersionAsync(
                    dbContext, 
                    project, 
                    oldProjectVersion, 
                    projectVersion, 
                    projectVersionNum, 
                    informationSecurityContext.User,
                    _dbContextFactory,
                    _cache.DbCache
                    );

                var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                string arg = NameValueCollectionHelper.GetNameValueCollectionString(
                    [
                        (PazCheckConstants.ParamName_UnitIdentifier, project.Unit.Identifier),
                        (PazCheckConstants.ParamName_ProjectTitle, project.Title),
                        ("ProjectId", $"{project.Id}"),                        
                        ("OldProjectVersionNum", oldProjectVersion is not null ? $"{oldProjectVersion.VersionNum}" : @""),
                        ("OldProjectVersionTimeUtc", oldProjectVersion is not null ? new Any(oldProjectVersion.TimeUtc).ValueAsString(false) : @""),
                        ("OldProjectVersionUser", oldProjectVersion is not null ? oldProjectVersion.User : @""),
                        ("OldProjectVersionActive_TimeUtc", oldProjectVersion is not null ? new Any(oldProjectVersion.Active_TimeUtc).ValueAsString(false) : @""),
                        ("OldProjectVersionActive_SupervisorUser", oldProjectVersion is not null ? oldProjectVersion.Active_SupervisorUser : @""),
                        ("OldProjectVersionActive_Comment", oldProjectVersion is not null ? oldProjectVersion.Comment : @""),
                        ("NewProjectVersionNum", $"{projectVersion.VersionNum}"),
                        ("NewProjectVersionTimeUtc", new Any(projectVersion.TimeUtc).ValueAsString(false)),
                        ("NewProjectVersionUser", projectVersion.User),
                        ("NewProjectVersionActive_TimeUtc", new Any(projectVersion.Active_TimeUtc).ValueAsString(false)),
                        ("NewProjectVersionActive_SupervisorUser", projectVersion.Active_SupervisorUser),
                        ("NewProjectVersionActive_Comment", projectVersion.Comment),
                    ]);
                PazCheckDbHelper.AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckDbHelper.GetMetaParamName([ PazCheckConstants.MetaParamNameBase_ActiveProjectVersionChanged, $"{project.Id}" ]),
                    paramValue: Guid.NewGuid().ToString(),
                    paramType: @"",
                    isTemp: false, // Not deleted periodically
                    group: @"",
                    method: @"",
                    hasArg: true,
                    excludeConnectionIds: @"",
                    arg: arg);

                await dbContext.SaveChangesAsync();

                _jobsManager.AdditionalLongRunning_ThreadSafeDispatcher.BeginInvokeEx(async ct =>
                {
                    var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                    using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                    try
                    {
                        await AddJournalParamsAsync(projectId, projectVersionNum, informationSecurityContext);
                    }
                    catch (Exception ex)
                    {
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, @"AddJournalParamsAsync error.");
                    }                    
                });

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SetActiveProjectVersion error. projectId: {0}", projectId);
            }
            finally
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(
                        informationSecurityContext.User,
                        HttpContextHelper.GetSourceIpAddress(HttpContext),
                        HttpContextHelper.GetSourceHost(HttpContext),
                        InformationSecurityEventsLogger.SupervisorApproval_AllRolesAccessEventId,
                        6,
                        succeeded,
                        Common.Properties.Resources.SupervisorApproveActiveProjectVersion_Event,
                        informationSecurityContext.User,
                        Common.Properties.Resources.Project + @": " + projectTitle,
                        NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                            {
                                (@"Action", @"ApproveProjectVersion"),
                                (@"ProjectId", new Any(projectId).ValueAsString(false)),
                                (@"NewProjectVersion", new Any(projectVersionNum).ValueAsString(false)),
                            }),
                        Properties.Resources.SupervisorApproveActiveProjectVersion_EventDesc, projectTitle, projectVersionNum, projectVersionComment);
            }            
            
            return Ok();
        }

        /// <summary>
        ///     Выбранную версию проекта делает текущей в выбранном проекте.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersionNum"></param>
        /// <param name="newProjectId"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost(@"SetCurrentProjectVersion/{projectId}")]
        public Task<IActionResult> SetCurrentProjectVersionAsync(int projectId, UInt32 projectVersionNum, int newProjectId)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext); 

            var jobId = Guid.NewGuid().ToString();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Common.Properties.Resources.SetCurrentProjectVersion_JobTitle, informationSecurityContext.User,
                async (cancellationToken, jobProgress) =>
                {
                    var userEventsLogger = _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>();
                    userEventsLogger.LogLevel = LogLevel.Error;
                    var loggersSet = new LoggersSet(_logger, userEventsLogger);
                    using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                    using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));                    

                    await SerializationHelper.SetCurrentProjectVersionAsync(
                                    _dbContextFactory,                                    
                                    projectId,
                                    projectVersionNum,
                                    newProjectId,
                                    cancellationToken,
                                    jobProgress,
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet);
                });

            return Task.FromResult<IActionResult>(Ok(new { jobId }));
        }

        /// <summary>
        ///     Сравнивает 2 версии проекта. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.
        ///     Если minProjectVersionNum задать 0, то сравнивается с пустой базой.
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions/{projectId}")]
        public async Task<IActionResult> CompareVersionsAsync(int projectId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = projectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == projectId)
                        .ToDictionary(pv => pv.VersionNum)
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                await PazCheckDbHelper.CompareVersionsCeMatricesAsync(readOnlyDbContext,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,                    
                    infos);
                await PazCheckDbHelper.CompareVersionsTagsAsync(readOnlyDbContext,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,                    
                    infos);
                await PazCheckDbHelper.CompareVersionsBaseActuatorsAsync(readOnlyDbContext,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,                    
                    infos);
                await PazCheckDbHelper.CompareVersionsSafetyControllersAsync(readOnlyDbContext,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,                    
                    infos);
                await PazCheckDbHelper.CompareVersionsLegendsAsync(readOnlyDbContext,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,                    
                    infos);

                return Ok(infos);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }            
        }

        /// <summary>        
        ///     Сравнивает 2 версии матрицы ПСС. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.        
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        ///     Если minProjectVersionNum 0, то сравнивается с последней сохраненной версией.
        ///     ceMatrixId - Id более новой версии матрицы.
        /// </summary>
        /// <param name="ceMatrixId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions_CeMatrix/{ceMatrixId}")]
        public async Task<IActionResult> CompareVersions_CeMatrixAsync(int ceMatrixId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                CeMatrix maxCeMatrix = await readOnlyDbContext.CeMatrices
                    .Include(m => m.CeMatrixDbFileReferences)
                    .Include(m => m.CeMatrixComments)
                    .SingleAsync(m => m.Id == ceMatrixId);

                var projectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxCeMatrix.ProjectId)
                        .ToDictionary(pv => pv.VersionNum);
                if (minProjectVersionNum == 0)
                    minProjectVersionNum = projectVersions.Keys.Max();

                CeMatrix minCeMatrix = await readOnlyDbContext.CeMatrices
                    .Where(PazCheckDbHelper.GetVersionEntityPredicate<CeMatrix>(minProjectVersionNum, maxCeMatrix.ProjectId, maxCeMatrix.Identifier))                                                 
                    .Include(m => m.CeMatrixDbFileReferences)                            
                    .Include(m => m.CeMatrixComments)
                    .SingleAsync();

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = maxCeMatrix.ProjectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = projectVersions
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                await PazCheckDbHelper.CompareVersionsCeMatrixAsync(readOnlyDbContext, 
                    minCeMatrix, 
                    maxCeMatrix, 
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs, 
                    infos);

                return Ok(infos);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии тэга. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.           
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        ///     Если minProjectVersionNum 0, то сравнивается с последней сохраненной версией.
        ///     tagId - Id более новой версии тэга.
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions_Tag/{tagId}")]
        public async Task<IActionResult> CompareVersions_TagAsync(int tagId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                Tag maxTag = await readOnlyDbContext.Tags 
                    .Include(t => t.TagDbFileReferences)                        
                    .SingleAsync(t => t.Id == tagId);

                var projectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxTag.ProjectId)
                        .ToDictionary(pv => pv.VersionNum);
                if (minProjectVersionNum == 0)
                    minProjectVersionNum = projectVersions.Keys.Max();

                Tag minTag = await readOnlyDbContext.Tags
                    .Where(PazCheckDbHelper.GetVersionEntityPredicate<Tag>(minProjectVersionNum, maxTag.ProjectId, maxTag.Identifier)) 
                    .Include(t => t.TagDbFileReferences)
                    .SingleAsync();

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = maxTag.ProjectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = projectVersions
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                PazCheckDbHelper.CompareVersionsTag(readOnlyDbContext, 
                    minTag, 
                    maxTag,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs, 
                    infos);

                return Ok(infos);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии типа исполнительных механизмов. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.        
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        ///     Если minProjectVersionNum 0, то сравнивается с последней сохраненной версией.
        ///     baseActuatorId - Id более новой версии.
        /// </summary>
        /// <param name="baseActuatorId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions_BaseActuator/{baseActuatorId}")]
        public async Task<IActionResult> CompareVersions_BaseActuatorAsync(int baseActuatorId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                BaseActuator maxBaseActuator = await readOnlyDbContext.BaseActuators                                           
                    .Include(ba => ba.BaseActuatorDbFileReferences)
                    .SingleAsync(ba => ba.Id == baseActuatorId);

                var projectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxBaseActuator.ProjectId)
                        .ToDictionary(pv => pv.VersionNum);
                if (minProjectVersionNum == 0)
                    minProjectVersionNum = projectVersions.Keys.Max();

                BaseActuator minBaseActuator = await readOnlyDbContext.BaseActuators
                    .Where(PazCheckDbHelper.GetVersionEntityPredicate<BaseActuator>(minProjectVersionNum, maxBaseActuator.ProjectId, maxBaseActuator.Identifier))                                            
                    .Include(ba => ba.BaseActuatorDbFileReferences)
                    .SingleAsync();

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = maxBaseActuator.ProjectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = projectVersions
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(
                        itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(
                        itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                PazCheckDbHelper.CompareVersionsBaseActuator(readOnlyDbContext, 
                    minBaseActuator, 
                    maxBaseActuator,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs, 
                    infos);

                return Ok(infos);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии типа исполнительных механизмов. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.        
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        ///     Если minProjectVersionNum 0, то сравнивается с последней сохраненной версией.
        ///     safetyControllerId - Id более новой версии.
        /// </summary>
        /// <param name="safetyControllerId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions_SafetyController/{safetyControllerId}")]
        public async Task<IActionResult> CompareVersions_SafetyControllerAsync(int safetyControllerId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                SafetyController maxSafetyController = await readOnlyDbContext.SafetyControllers                                           
                    .Include(sc => sc.SafetyControllerDbFileReferences)
                    .SingleAsync(sc => sc.Id == safetyControllerId);

                var projectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxSafetyController.ProjectId)
                        .ToDictionary(pv => pv.VersionNum);
                if (minProjectVersionNum == 0)
                    minProjectVersionNum = projectVersions.Keys.Max();

                SafetyController minSafetyController = await readOnlyDbContext.SafetyControllers
                    .Where(PazCheckDbHelper.GetVersionEntityPredicate<SafetyController>(minProjectVersionNum, maxSafetyController.ProjectId, maxSafetyController.Identifier))                                                 
                    .Include(sc => sc.SafetyControllerDbFileReferences)
                    .SingleAsync();

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = maxSafetyController.ProjectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = projectVersions
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                PazCheckDbHelper.CompareVersionsSafetyController(readOnlyDbContext, 
                    minSafetyController, 
                    maxSafetyController,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs, 
                    infos);

                return Ok(infos);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии условного обозначения. Если maxProjectVersionNum не задан или 0, то сравнивается с текущими несохраненными изменениями.        
        ///     Если user пустой, учитываются все изменения. Иначе только для данного пользователя или без пользователя.
        ///     Если minProjectVersionNum 0, то сравнивается с последней сохраненной версией.
        ///     legendId - Id более новой версии.
        /// </summary>
        /// <param name="legendId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"CompareVersions_Legend/{legendId}")]
        public async Task<IActionResult> CompareVersions_LegendAsync(int legendId, UInt32 minProjectVersionNum, UInt32? maxProjectVersionNum, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;
            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                Legend maxLegend = await readOnlyDbContext.Legends
                    .Include(sc => sc.LegendDbFileReferences)
                    .SingleAsync(sc => sc.Id == legendId);

                var projectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxLegend.ProjectId)
                        .ToDictionary(pv => pv.VersionNum);
                if (minProjectVersionNum == 0)
                    minProjectVersionNum = projectVersions.Keys.Max();

                Legend minLegend = await readOnlyDbContext.Legends
                    .Where(PazCheckDbHelper.GetVersionEntityPredicate<Legend>(minProjectVersionNum, maxLegend.ProjectId, maxLegend.Identifier))
                    .Include(sc => sc.LegendDbFileReferences)
                    .SingleAsync();

                ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                {
                    ProjectId = maxLegend.ProjectId,
                    MinProjectVersionNum = minProjectVersionNum,
                    MaxProjectVersionNum = maxProjectVersionNum,
                    User = user,
                    ProjectVersions = projectVersions
                };
                List<ItemVersionComparisonInfo> infos = new();

                var minProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MinProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);
                var maxProjectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(itemVersionComparisonArgs.ProjectId,
                        itemVersionComparisonArgs.MaxProjectVersionNum,
                        readOnlyDbContext,
                        LoggersSet.Empty);

                PazCheckDbHelper.CompareVersionsLegend(readOnlyDbContext,
                    minLegend,
                    maxLegend,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    _cache.DbCache.ParamDescs,
                    itemVersionComparisonArgs,
                    infos);

                return Ok(infos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"CompareVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что-то менялось в этом элементе.
        /// </summary>
        /// <param name="ceMatrixId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetVersions_CeMatrix/{ceMatrixId}")]
        public async Task<IActionResult> GetVersions_CeMatrix(int ceMatrixId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                HashSet<UInt32> projectVersionNums = new();

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                CeMatrix ceMatrix = await readOnlyDbContext.CeMatrices.Where(ba => ba.Id == ceMatrixId)
                    .Include(m => m.CeMatrixParams)
                    .Include(m => m.CeMatrixDbFileReferences)                        
                    .Include(m => m.Rows)                        
                    .Include(m => m.Columns)
                    .Include(m => m.Cells)
                    .Include(m => m.CeMatrixComments)
                    .FirstAsync();
                if (ceMatrix._CreateProjectVersionNum is not null)
                {
                    PazCheckDbHelper.GetVersions(ceMatrix, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.CeMatrixParams, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.CeMatrixDbFileReferences, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.Rows, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.Columns, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.Cells, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(ceMatrix.CeMatrixComments, projectVersionNums, user);
                }

                if (String.IsNullOrEmpty(user))
                {
                    if (ceMatrix._HasUnversionedChanges)
                        projectVersionNums.Add(0);
                }
                else
                {
                    if (ceMatrix._HasUnversionedChanges &&
                            (ceMatrix._LastChangeUser == user || ceMatrix._LastChangeUser == @""))
                        projectVersionNums.Add(0);
                }

                return Ok(projectVersionNums.OrderBy(n => n).ToArray());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что-то менялось в этом элементе.
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetVersions_Tag/{tagId}")]
        public async Task<IActionResult> GetVersions_Tag(int tagId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                HashSet<UInt32> projectVersionNums = new();

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                Tag tag = await readOnlyDbContext.Tags.Where(t => t.Id == tagId)
                    .Include(ba => ba.TagParams)                    
                    .Include(ba => ba.TagConditions)
                    .Include(ba => ba.TagDbFileReferences)
                    .FirstAsync();
                if (tag._CreateProjectVersionNum is not null)
                {
                    PazCheckDbHelper.GetVersions(tag, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(tag.TagParams, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(tag.TagConditions, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(tag.TagDbFileReferences, projectVersionNums, user);
                }

                if (String.IsNullOrEmpty(user))
                {
                    if (tag._HasUnversionedChanges)
                        projectVersionNums.Add(0);
                }
                else
                {
                    if (tag._HasUnversionedChanges &&
                            (tag._LastChangeUser == user || tag._LastChangeUser == @""))
                        projectVersionNums.Add(0);
                }

                return Ok(projectVersionNums.OrderBy(n => n).ToArray());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что-то менялось в этом элементе.
        /// </summary>
        /// <param name="baseActuatorId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetVersions_BaseActuator/{baseActuatorId}")]
        public async Task<IActionResult> GetVersions_BaseActuator(int baseActuatorId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                HashSet<UInt32> projectVersionNums = new();

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                BaseActuator baseActuator = await readOnlyDbContext.BaseActuators.Where(ba => ba.Id == baseActuatorId)
                    .Include(ba => ba.BaseActuatorParams)
                    .Include(ba => ba.BaseActuatorDbFileReferences)
                    .FirstAsync();
                if (baseActuator._CreateProjectVersionNum is not null)
                {
                    PazCheckDbHelper.GetVersions(baseActuator, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(baseActuator.BaseActuatorParams, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(baseActuator.BaseActuatorDbFileReferences, projectVersionNums, user);
                }

                if (String.IsNullOrEmpty(user))
                {
                    if (baseActuator._HasUnversionedChanges)
                        projectVersionNums.Add(0);
                }
                else
                {
                    if (baseActuator._HasUnversionedChanges &&
                            (baseActuator._LastChangeUser == user || baseActuator._LastChangeUser == @""))
                        projectVersionNums.Add(0);
                }

                return Ok(projectVersionNums.OrderBy(n => n).ToArray());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что-то менялось в этом элементе.
        /// </summary>
        /// <param name="safetyControllerId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetVersions_SafetyController/{safetyControllerId}")]
        public async Task<IActionResult> GetVersions_SafetyController(int safetyControllerId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                HashSet<UInt32> projectVersionNums = new();

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                SafetyController safetyController = await readOnlyDbContext.SafetyControllers.Where(sc => sc.Id == safetyControllerId)
                    .Include(ba => ba.SafetyControllerParams)
                    .Include(ba => ba.SafetyControllerDbFileReferences)
                    .FirstAsync();
                if (safetyController._CreateProjectVersionNum is not null)
                {
                    PazCheckDbHelper.GetVersions(safetyController, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(safetyController.SafetyControllerParams, projectVersionNums, user);

                    PazCheckDbHelper.GetVersions(safetyController.SafetyControllerDbFileReferences, projectVersionNums, user);
                }

                if (String.IsNullOrEmpty(user))
                {
                    if (safetyController._HasUnversionedChanges)
                        projectVersionNums.Add(0);
                }
                else
                {
                    if (safetyController._HasUnversionedChanges &&
                            (safetyController._LastChangeUser == user || safetyController._LastChangeUser == @""))
                        projectVersionNums.Add(0);
                }

                return Ok(projectVersionNums.OrderBy(n => n).ToArray());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetVersions error.");

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что-то менялось в этом элементе.
        /// </summary>
        /// <param name="legendId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetVersions_Legend/{legendId}")]
        public async Task<IActionResult> GetVersions_Legend(int legendId, string user)
        {
            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            try
            {
                HashSet<UInt32> projectVersionNums = new();

                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                Legend legend = await readOnlyDbContext.Legends.Where(sc => sc.Id == legendId)                        
                    .FirstAsync();
                if (legend._CreateProjectVersionNum is not null)
                {
                    PazCheckDbHelper.GetVersions(legend, projectVersionNums, user);
                }

                if (String.IsNullOrEmpty(user))
                {
                    if (legend._HasUnversionedChanges)
                        projectVersionNums.Add(0);
                }
                else
                {
                    if (legend._HasUnversionedChanges &&
                            (legend._LastChangeUser == user || legend._LastChangeUser == @""))
                        projectVersionNums.Add(0);
                }

                return Ok(projectVersionNums.OrderBy(n => n).ToArray());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetVersions error.");

                return NotFound();
            }
        }

        #endregion

        #region private functions

        private async Task AddJournalParamsAsync(
            int projectId, 
            uint projectVersionNum,
            InformationSecurityContext informationSecurityContext)
        {
            CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
            if (ceMatrixRuntimeAddon is null)
                return;

            await using var dbContext = _dbContextFactory.CreateDbContext();

            var project = await dbContext.Projects
                    .Where(p => p.Id == projectId)
                    .Include(p => p.Unit)
                    .SingleAsync();

            await PazCheckDbHelper.Create_Unit_BasePcObjectsAsync(
                project.Unit.Identifier,
                _dbContextFactory,
                informationSecurityContext.User,
                _cache.DbCache);

            Common.Serialization.SerializationRootObject serializationRootObject = new();
            serializationRootObject.PcObjects = new List<Common.Serialization.PcObject>();
            var (unit_PcObject, otherArea_PcObject) = PazCheckDbHelper.Create_Unit_Objects(project.Unit);
            serializationRootObject.PcObjects.Add(unit_PcObject);
            serializationRootObject.PcObjects.Add(otherArea_PcObject);

            ProjectAllParamValues projectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(projectId, projectVersionNum, dbContext, LoggersSet.Empty);
            foreach (var tagName in projectAllParamValues.TagConditions.Keys)
            {
                List<Common.Serialization.TagCondition> tagConditions = PazCheckDbHelper.GetTagConditions(                        
                        projectAllParamValues,                        
                        tagName,
                        ceMatrixRuntimeAddon.CsvDb,
                        LoggersSet.Empty);

                List<Common.Serialization.Param> journalParams = new();
                foreach (var it in tagConditions.GroupBy(tc => tc.ConditionCategory))
                {
                    string conditionCategory = it.Key;
                    if (String.IsNullOrEmpty(conditionCategory) || conditionCategory.StartsWith("!"))
                        continue;
                    string? daCondition = null;
                    foreach (var tagCondition in it) 
                    {
                        if (!String.IsNullOrEmpty(tagCondition.DaCondition))
                        {
                            if (daCondition is null)
                                daCondition = tagCondition.DaCondition;
                            // TODO
                            //else if (!String.Equals(daCondition, tagCondition.DaCondition, StringComparison.InvariantCultureIgnoreCase))
                            //    loggersSet.UserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidDaCondition, tagName, tagCondition.DaCondition);
                        }
                    }
                    if (!String.IsNullOrEmpty(daCondition) && 
                        SszQueryHelper.FindFirstLevelSpecialText(daCondition, '%', true).Count == 0)
                    {
                        var (elementId, converter) = GetConverter(daCondition);

                        journalParams.Add(
                            new Common.Serialization.Param()
                            {
                                Name = PazCheckConstants.ParamNamePrefix_Data + conditionCategory,
                                Value = NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveOrderedDictionary<string?>()
                                        {
                                            { PazCheckConstants.ParamName_In, elementId },
                                            { PazCheckConstants.ParamName_TrendEnabled, @"true" },
                                            { PazCheckConstants.ParamName_TrendStorePeriod, @"5y" },                                            
                                            { PazCheckConstants.ParamName_Converter, converter },
                                        })
                            });
                    }
                }                

                if (journalParams.Count > 0)
                {
                    PcObject? pcObject = null;

                    _cache.DbCache.PcObjectsDictionary1.TryGetValue(project.Unit.Identifier + "." + tagName, out pcObject);

                    if (pcObject is null)
                    {
                        var serializationPcObject = new Common.Serialization.PcObject()
                        {
                            Identifier = tagName,
                            Params = new List<Common.Serialization.Param>() 
                            { 
                                new Common.Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = tagName },
                                new CentralServer.Common.Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_OtherItem_Template },
                                new CentralServer.Common.Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = otherArea_PcObject.Identifier },
                            },                            
                            Unit = project.Unit.Identifier
                        };
                        serializationPcObject.Params.AddRange(journalParams);
                        serializationRootObject.PcObjects.Add(serializationPcObject);
                    }
                    else
                    {
                        var serializationPcObject = new Common.Serialization.PcObject()
                        {
                            Identifier = tagName,
                            Unit = unit_PcObject.Identifier,
                            Params = journalParams
                        };
                        serializationRootObject.PcObjects.Add(serializationPcObject);
                    }
                }                
            }            

            if (serializationRootObject.PcObjects.Any(o => !PazCheckDbHelper.CheckPcObject(o, _cache.DbCache)))
                await SerializationHelper.ImportSerializationRootObjectAsync(
                        serializationRootObject,
                        new Common.Serialization.ImportMetadata()
                        {
                            RootCollectionMode = Common.Serialization.CollectionMode.Update,
                            ChildCollectionMode = Common.Serialization.CollectionMode.Update,
                            DataCollectionMode = Common.Serialization.CollectionMode.Update,
                        },
                        _dbContextFactory,
                        informationSecurityContext.User,
                        null,
                        CancellationToken.None,
                        NullJobProgress.Instance,                        
                        LoggersSet.Empty,
                        new Common.Serialization.ImportSerializationRootObjectResult(),
                        preview: false);
        }

        private static (string, string) GetConverter(string elementAndOperatorAndOptionsAndValues)
        {
            var (elementId, operator_, options, values) = SszOperatorHelper.Parse(elementAndOperatorAndOptionsAndValues);

            if (elementId == @"")
                return (@"", @"");

            if (operator_ == SszOperator.None || values is null || values.Length == 0)
                return (elementId, @"");            

            var operand = Any.ConvertToBestType(values[0], false);
            switch (AnyHelper.GetTransportType(operand))
            {
                case TransportType.Double:
                    return (elementId, "d[0]" + SszOperatorHelper.ToString(operator_) + operand.ValueAsString(false));                    
                case TransportType.UInt32:
                    return (elementId, "i[0]" + SszOperatorHelper.ToString(operator_) + operand.ValueAsString(false));
                case TransportType.Object:
                    return (elementId, "s[0]" + SszOperatorHelper.ToString(operator_) + "\"" + operand.ValueAsString(false) + "\"");
                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion        
    }
}


//InfoString = "Изм. " + intersectMaxParam.ParamName + " = " + intersectMaxParam.Value + intersectMaxParam.Eu +
//                            ", старое " + intersectMinParam.Value + intersectMinParam.Eu,    

