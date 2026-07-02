using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Frozen;
using Ssz.Utils.ClosedXML;
using System.Linq.Expressions;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions

        /// <summary>
        ///     Если user пустой, то анализирует все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="projectId"></param>
        /// <param name="user"></param>
        /// <returns></returns>        
        public static bool HasUnversionedChanges(PazCheckDbContext readOnlyDbContext, int projectId, string user)
        {
            Project project = readOnlyDbContext.Projects
                        .Single(p => p.Id == projectId);

            if (String.IsNullOrEmpty(user))
            {
                bool hasUnsavedChanges = readOnlyDbContext.CeMatrices.Any(m => m.Project == project && m._HasUnversionedChanges);
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.Tags.Any(t => t.Project == project && t._HasUnversionedChanges);
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.BaseActuators.Any(ba => ba.Project == project && ba._HasUnversionedChanges);
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.SafetyControllers.Any(sc => sc.Project == project && sc._HasUnversionedChanges);
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.Legends.Any(sc => sc.Project == project && sc._HasUnversionedChanges);
                if (hasUnsavedChanges)
                    return true;

                return false;
            }
            else
            {
                bool hasUnsavedChanges = readOnlyDbContext.CeMatrices.Any(m => m.Project == project && m._HasUnversionedChanges &&
                    (m._LastChangeUser == user || m._LastChangeUser == @""));
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.Tags.Any(t => t.Project == project && t._HasUnversionedChanges &&
                    (t._LastChangeUser == user || t._LastChangeUser == @""));
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.BaseActuators.Any(ba => ba.Project == project && ba._HasUnversionedChanges &&
                    (ba._LastChangeUser == user || ba._LastChangeUser == @""));
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.SafetyControllers.Any(sc => sc.Project == project && sc._HasUnversionedChanges &&
                    (sc._LastChangeUser == user || sc._LastChangeUser == @""));
                if (hasUnsavedChanges)
                    return true;
                hasUnsavedChanges = readOnlyDbContext.Legends.Any(l => l.Project == project && l._HasUnversionedChanges &&
                    (l._LastChangeUser == user || l._LastChangeUser == @""));
                if (hasUnsavedChanges)
                    return true;

                return false;
            }
        }

        /// <summary>
        ///     Если user пустой, то сохраняет все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="projectId"></param>
        /// <param name="projectVersionType"></param>
        /// <param name="informationSecurityContext"></param>
        /// <param name="user"></param>
        /// <param name="comment"></param>        
        /// <returns></returns>
        public static async Task SaveUnversionedChangesAsync(PazCheckDbContext dbContext, 
            int projectId, 
            ProjectVersionType projectVersionType,
            InformationSecurityContext? informationSecurityContext,
            string user, 
            string comment)
        {
            ProjectVersion? lastProjectVersion = dbContext.ProjectVersions.Where(pv => pv.ProjectId == projectId).OrderByDescending(pv => pv.TimeUtc).FirstOrDefault();
            var projectVersion = new ProjectVersion()
            {
                ProjectId = projectId,
                ProjectVersionType = projectVersionType.Type,
                VersionNum = (lastProjectVersion?.VersionNum ?? 0) + 1,
                TimeUtc = DateTime.UtcNow,
                User = informationSecurityContext?.User ?? @"",
                Comment = comment ?? @""
            };
            CaseInsensitiveOrderedDictionary<string?> paramsDictionary = new();
            foreach (var paramInfo in projectVersionType.StandardParamInfos)
            {
                paramsDictionary[paramInfo.ParamName] = paramInfo.DefaultValue;
            }
            projectVersion.ParamsDictionary = paramsDictionary;

            bool hasUnversionedChanges = false;

            foreach (CeMatrix ceMatrix in dbContext.CeMatrices.Where(GetUnversionedChangesPredicate<CeMatrix>(user, projectId))
                    .Include(t => t.CeMatrixParams)
                    .Include(t => t.CeMatrixDbFileReferences)
                    .Include(t => t.Rows)
                    .Include(t => t.Columns)
                    .Include(t => t.Cells)
                    .Include(t => t.CeMatrixComments))
            {
                SaveUnversionedChanges_CeMatrix(dbContext, projectVersion.VersionNum, ceMatrix);
                hasUnversionedChanges = true;
            }

            foreach (Tag tag in dbContext.Tags.Where(GetUnversionedChangesPredicate<Tag>(user, projectId))
                .Include(t => t.TagParams)                
                .Include(t => t.TagConditions)
                .Include(t => t.TagDbFileReferences))
            {
                SaveUnversionedChanges_Tag(dbContext, projectVersion.VersionNum, tag);
                hasUnversionedChanges = true;
            }

            foreach (BaseActuator baseActuator in dbContext.BaseActuators.Where(GetUnversionedChangesPredicate<BaseActuator>(user, projectId))
                    .Include(ba => ba.BaseActuatorParams)
                    .Include(ba => ba.BaseActuatorDbFileReferences))
            {
                SaveUnversionedChanges_BaseActuator(dbContext, projectVersion.VersionNum, baseActuator);
                hasUnversionedChanges = true;
            }

            foreach (SafetyController safetyController in dbContext.SafetyControllers.Where(GetUnversionedChangesPredicate<SafetyController>(user, projectId))
                    .Include(sc => sc.SafetyControllerParams)
                    .Include(sc => sc.SafetyControllerDbFileReferences))
            {
                SaveUnversionedChanges_SafetyController(dbContext, projectVersion.VersionNum, safetyController);
                hasUnversionedChanges = true;
            }

            foreach (Legend legend in dbContext.Legends.Where(GetUnversionedChangesPredicate<Legend>(user, projectId))
                .Include(ba => ba.LegendParams)
                .Include(ba => ba.LegendDbFileReferences))
            {
                SaveUnversionedChanges_Legend(dbContext, projectVersion.VersionNum, legend);
                hasUnversionedChanges = true;
            }

            if (hasUnversionedChanges)
            {
                dbContext.ProjectVersions.Add(projectVersion);

                await dbContext.SaveChangesAsync();

                //_informationSecurityEventsLogger.InformationSecurityEvent(user ?? @"", HttpContextHelper.GetSourceIpAddress(HttpContext), InformationSecurityEventPazCheckCentralServerConstants.SaveUnversionedChanges_EventId, true, Properties.Resources.SaveUnversionedChanges_Event);
            }

            await RemoveLockedByUserOnProjectVersionedEntitiesAsync(dbContext, projectId, user);
        }

        /// <summary>
        ///     Если user пустой, то очищает все изменения. Иначе только для данного пользователя или без пользователя.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="projectId"></param>
        /// <param name="user"></param>
        /// <param name="loggersSet"></param>        
        /// <returns></returns>        
        public static async Task ClearUnversionedChangesAsync(IDbContextFactory<PazCheckDbContext> dbContextFactory, int projectId, string user, ILoggersSet loggersSet)
        {
            try
            {
                //using var dbContextTransaction = dbContext.Database.BeginTransaction();

                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    await dbContext.CeMatrices.Where(GetUnversionedChangesPredicate<CeMatrix>(user, projectId))
                        .Where(e => e._CreateProjectVersionNum == null)                        
                        .ExecuteDeleteAsync();
                    
                    if (String.IsNullOrEmpty(user))
                    {
                        await dbContext.CeMatrixParams.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.CeMatrixDbFileReferences.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Rows.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Columns.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Cells.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.CeMatrixComments.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                    else
                    {
                        await dbContext.CeMatrixParams.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.CeMatrixDbFileReferences.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Rows.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Columns.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.Cells.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.CeMatrixComments.Where(e => e.CeMatrix.ProjectId == projectId && e.CeMatrix._HasUnversionedChanges && (e.CeMatrix._LastChangeUser == user || e.CeMatrix._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }

                    await dbContext.Tags.Where(GetUnversionedChangesPredicate<Tag>(user, projectId))
                        .Where(e => e._CreateProjectVersionNum == null)                        
                        .ExecuteDeleteAsync();

                    if (String.IsNullOrEmpty(user))
                    {
                        await dbContext.TagParams.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.TagConditions.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.TagDbFileReferences.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                    else
                    {
                        await dbContext.TagParams.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges && (e.Tag._LastChangeUser == user || e.Tag._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.TagConditions.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges && (e.Tag._LastChangeUser == user || e.Tag._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.TagDbFileReferences.Where(e => e.Tag.ProjectId == projectId && e.Tag._HasUnversionedChanges && (e.Tag._LastChangeUser == user || e.Tag._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }

                    await dbContext.BaseActuators.Where(GetUnversionedChangesPredicate<BaseActuator>(user, projectId))
                        .Where(e => e._CreateProjectVersionNum == null)                        
                        .ExecuteDeleteAsync();

                    if (String.IsNullOrEmpty(user))
                    {
                        await dbContext.BaseActuatorParams.Where(e => e.BaseActuator.ProjectId == projectId && e.BaseActuator._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.BaseActuatorDbFileReferences.Where(e => e.BaseActuator.ProjectId == projectId && e.BaseActuator._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                    else
                    {
                        await dbContext.BaseActuatorParams.Where(e => e.BaseActuator.ProjectId == projectId && e.BaseActuator._HasUnversionedChanges && (e.BaseActuator._LastChangeUser == user || e.BaseActuator._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.BaseActuatorDbFileReferences.Where(e => e.BaseActuator.ProjectId == projectId && e.BaseActuator._HasUnversionedChanges && (e.BaseActuator._LastChangeUser == user || e.BaseActuator._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();                        
                    }

                    await dbContext.SafetyControllers.Where(GetUnversionedChangesPredicate<SafetyController>(user, projectId))
                        .Where(e => e._CreateProjectVersionNum == null)                        
                        .ExecuteDeleteAsync();

                    if (String.IsNullOrEmpty(user))
                    {
                        await dbContext.SafetyControllerParams.Where(e => e.SafetyController.ProjectId == projectId && e.SafetyController._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.SafetyControllerDbFileReferences.Where(e => e.SafetyController.ProjectId == projectId && e.SafetyController._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                    else
                    {
                        await dbContext.SafetyControllerParams.Where(e => e.SafetyController.ProjectId == projectId && e.SafetyController._HasUnversionedChanges && (e.SafetyController._LastChangeUser == user || e.SafetyController._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.SafetyControllerDbFileReferences.Where(e => e.SafetyController.ProjectId == projectId && e.SafetyController._HasUnversionedChanges && (e.SafetyController._LastChangeUser == user || e.SafetyController._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }

                    await dbContext.Legends.Where(GetUnversionedChangesPredicate<Legend>(user, projectId))
                        .Where(e => e._CreateProjectVersionNum == null)                        
                        .ExecuteDeleteAsync();

                    if (String.IsNullOrEmpty(user))
                    {
                        await dbContext.LegendParams.Where(e => e.Legend.ProjectId == projectId && e.Legend._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.LegendDbFileReferences.Where(e => e.Legend.ProjectId == projectId && e.Legend._HasUnversionedChanges)
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                    else
                    {
                        await dbContext.LegendParams.Where(e => e.Legend.ProjectId == projectId && e.Legend._HasUnversionedChanges && (e.Legend._LastChangeUser == user || e.Legend._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();

                        await dbContext.LegendDbFileReferences.Where(e => e.Legend.ProjectId == projectId && e.Legend._HasUnversionedChanges && (e.Legend._LastChangeUser == user || e.Legend._LastChangeUser == @""))
                            .Where(e => e._CreateProjectVersionNum == null)
                            .ExecuteDeleteAsync();
                    }
                }

                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    dbContext.IsLastChangeFieldsUpdatingDisabled = true;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    foreach (CeMatrix ceMatrix in dbContext.CeMatrices.Where(GetUnversionedChangesPredicate<CeMatrix>(user, projectId))
                            .Include(t => t.CeMatrixParams)
                            .Include(t => t.CeMatrixDbFileReferences)
                            .Include(t => t.Rows)
                            .Include(t => t.Columns)
                            .Include(t => t.Cells)
                            .Include(t => t.CeMatrixComments))
                    {
                        ClearUnversionedChanges_CeMatrix(dbContext, ceMatrix);
                    }

                    foreach (Tag tag in dbContext.Tags.Where(GetUnversionedChangesPredicate<Tag>(user, projectId))
                            .Include(t => t.TagParams)
                            .Include(t => t.TagConditions)
                            .Include(t => t.TagDbFileReferences))
                    {
                        ClearUnversionedChanges_Tag(dbContext, tag);
                    }

                    foreach (BaseActuator baseActuator in dbContext.BaseActuators.Where(GetUnversionedChangesPredicate<BaseActuator>(user, projectId))
                            .Include(ba => ba.BaseActuatorParams)
                            .Include(ba => ba.BaseActuatorDbFileReferences))
                    {
                        ClearUnversionedChanges_BaseActuator(dbContext, baseActuator);
                    }

                    foreach (SafetyController safetyController in dbContext.SafetyControllers.Where(GetUnversionedChangesPredicate<SafetyController>(user, projectId))
                            .Include(ba => ba.SafetyControllerParams)
                            .Include(ba => ba.SafetyControllerDbFileReferences))
                    {
                        ClearUnversionedChanges_SafetyController(dbContext, safetyController);
                    }

                    foreach (Legend legend in dbContext.Legends.Where(GetUnversionedChangesPredicate<Legend>(user, projectId))
                            .Include(ba => ba.LegendParams)
                            .Include(ba => ba.LegendDbFileReferences))
                    {
                        ClearUnversionedChanges_Legend(dbContext, legend);
                    }

                    await dbContext.SaveChangesAsync();

                    await RemoveLockedByUserOnProjectVersionedEntitiesAsync(dbContext, projectId, user);
                }                
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogError(ex, "ClearUnversionedChanges error.");                
            }
        }

        public static async Task RemoveLockedByUserOnProjectVersionedEntitiesAsync(PazCheckDbContext dbContext, int projectId, string user)
        {
            await dbContext.CeMatrices.Where(GetLockedByUserPredicate<CeMatrix>(user, projectId)).ExecuteUpdateAsync(e => e.SetProperty(
                e2 => e2._LockedByUser,
                e2 => @""));

            await dbContext.Tags.Where(GetLockedByUserPredicate<Tag>(user, projectId)).ExecuteUpdateAsync(e => e.SetProperty(
                e2 => e2._LockedByUser,
                e2 => @""));

            await dbContext.BaseActuators.Where(GetLockedByUserPredicate<BaseActuator>(user, projectId)).ExecuteUpdateAsync(e => e.SetProperty(
                e2 => e2._LockedByUser,
                e2 => @""));

            await dbContext.SafetyControllers.Where(GetLockedByUserPredicate<SafetyController>(user, projectId)).ExecuteUpdateAsync(e => e.SetProperty(
                e2 => e2._LockedByUser,
                e2 => @""));

            await dbContext.Legends.Where(GetLockedByUserPredicate<Legend>(user, projectId)).ExecuteUpdateAsync(e => e.SetProperty(
                e2 => e2._LockedByUser,
                e2 => @""));
        }

        /// <summary>
        ///     Задает новую активную версию для проекта
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="project"></param>
        /// <param name="oldProjectVersion"></param>
        /// <param name="projectVersion"></param>
        /// <param name="projectVersionNum"></param>
        /// <param name="user"></param>
        /// <param name="dbContextFactory"></param>
        /// <param name="dbCache"></param>
        /// <returns></returns>
        public static async Task SetActiveProjectVersionAsync(
            PazCheckDbContext dbContext, 
            Project project,
            ProjectVersion? oldProjectVersion,
            ProjectVersion projectVersion,
            UInt32 projectVersionNum,
            string user,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            DbCache dbCache)
        {  
            UInt32 oldProjectVersionNum = 0;
            if (oldProjectVersion is not null)
            {
                oldProjectVersion.Status = 0;

                if (oldProjectVersion.ProjectId == projectVersion.ProjectId)
                    oldProjectVersionNum = oldProjectVersion.VersionNum;
            }

            if (projectVersionNum > oldProjectVersionNum + 1)
            {
                foreach (var middleProjectVersion in dbContext.ProjectVersions.Where(pv => pv.ProjectId == project.Id && pv.VersionNum > oldProjectVersionNum && pv.VersionNum < projectVersionNum))
                {
                    middleProjectVersion.Active_TimeUtc = DateTime.UtcNow;
                    middleProjectVersion.Active_SupervisorUser = user;
                }
            }
            else if (projectVersionNum < oldProjectVersionNum)
            {
                foreach (var laterProjectVersion in dbContext.ProjectVersions.Where(pv => pv.ProjectId == project.Id && pv.VersionNum > projectVersionNum && pv.VersionNum <= oldProjectVersionNum))
                {
                    laterProjectVersion.Active_TimeUtc = null;
                    laterProjectVersion.Active_SupervisorUser = null;
                }
            }

            projectVersion.Status = 2;
            projectVersion.Active_TimeUtc = DateTime.UtcNow;
            projectVersion.Active_SupervisorUser = user;
            project.Unit.ActiveProjectVersion = projectVersion;

            await dbContext.SaveChangesAsync();

            _ = Sync_BasePcObjects_PcObjects_WithActiveProjectVersionAsync(
                project.Unit.Identifier,
                project.Id, 
                projectVersionNum, 
                dbContextFactory, 
                dbCache);
        }

        public static async Task CompareVersionsCeMatricesAsync(
            PazCheckDbContext readOnlyDbContext,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,            
            List<ItemVersionComparisonInfo> infos)
        {
            List<CeMatrix> maxConstCeMatrices = await readOnlyDbContext.CeMatrices.Where(GetMaxConstPredicate<CeMatrix>(itemVersionComparisonArgs))                    
                .Include(m => m.CeMatrixDbFileReferences)
                .Include(m => m.CeMatrixComments)
                .ToListAsync();

            List<CeMatrix> minExceptConstCeMatrices = await readOnlyDbContext.CeMatrices.Where(GetMinExceptConstPredicate<CeMatrix>(itemVersionComparisonArgs))                
                .Include(m => m.CeMatrixDbFileReferences)
                .Include(m => m.CeMatrixComments)
                .ToListAsync();

            List<CeMatrix> maxExceptConstCeMatrices = await readOnlyDbContext.CeMatrices.Where(GetMaxExceptConstPredicate<CeMatrix>(itemVersionComparisonArgs))                
                .Include(m => m.CeMatrixDbFileReferences)
                .Include(m => m.CeMatrixComments)
                .ToListAsync();

            maxExceptConstCeMatrices.Intersect(
                minExceptConstCeMatrices,
                out List<CeMatrix> maxUniqueCollection,
                out List<CeMatrix> maxIntersectionCollection,
                out List<CeMatrix> minUniqueCollection,
                out List<CeMatrix> minIntersectionCollection,
                StringIdentifierEqualityComparer<CeMatrix>.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                null,
                m => m.Identifier,
                m => maxProjectAllParamValues.CeMatricesParamValues.GetValueOrDefault(m.Identifier + "." + PazCheckConstants.ParamName_Title) ?? @"",
                m => @"",
                itemVersionComparisonArgs,
                infos);

            foreach (var index in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                await CompareVersionsCeMatrixAsync(
                    readOnlyDbContext, 
                    minIntersectionCollection[index], 
                    maxIntersectionCollection[index],
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, infos);
            }

            foreach (var ceMatrix in maxConstCeMatrices)
            {
                await CompareVersionsCeMatrixAsync(
                    readOnlyDbContext, 
                    ceMatrix, 
                    ceMatrix,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }
        }

        public static async Task CompareVersionsCeMatrixAsync(
            PazCheckDbContext readOnlyDbContext, 
            CeMatrix minCeMatrix, 
            CeMatrix maxCeMatrix,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,            
            List<ItemVersionComparisonInfo> infos)
        {
            List<ItemVersionComparisonInfo> ceMatrixParams_Infos = new();
            CompareVersionsParams(
                minProjectAllParamValues.CeMatricesParams.GetValueOrDefault(minCeMatrix.Identifier)!,
                maxProjectAllParamValues.CeMatricesParams.GetValueOrDefault(maxCeMatrix.Identifier)!,
                paramDescs,
                itemVersionComparisonArgs,
                maxCeMatrix.Id,
                ceMatrixParams_Infos);

            List<ItemVersionComparisonInfo> ceMatrixDbFileReferences_Infos = new();
            CompareVersionsDbFileReferences(
                minCeMatrix.CeMatrixDbFileReferences, 
                maxCeMatrix.CeMatrixDbFileReferences,
                itemVersionComparisonArgs,
                maxCeMatrix.Id,
                ceMatrixDbFileReferences_Infos);

            List<ItemVersionComparisonInfo> rows_Infos = new();            
            await CompareVersionsEntitiesAsync<Row>(
                readOnlyDbContext.Rows.Where(r => r.CeMatrix == minCeMatrix),
                readOnlyDbContext.Rows.Where(r => r.CeMatrix == maxCeMatrix),
                itemVersionComparisonArgs,
                RowOrColumnEqualityComparer.Instance,
                RowOrColumnValueEqualityComparer.Instance,
                maxCeMatrix.Id,
                e => RowOrColumnHelper.GetDesc(e),
                e => @"",
                e => @"",
                rows_Infos);

            List<ItemVersionComparisonInfo> columns_Infos = new();
            await CompareVersionsEntitiesAsync<Column>(
                readOnlyDbContext.Columns.Where(r => r.CeMatrix == minCeMatrix),
                readOnlyDbContext.Columns.Where(r => r.CeMatrix == maxCeMatrix),
                itemVersionComparisonArgs,
                RowOrColumnEqualityComparer.Instance,
                RowOrColumnValueEqualityComparer.Instance,
                maxCeMatrix.Id,
                e => RowOrColumnHelper.GetDesc(e),
                e => @"",
                e => @"",
                columns_Infos);

            List<ItemVersionComparisonInfo> cells_Infos = new();
            await CompareVersionsEntitiesAsync(readOnlyDbContext.Cells.Where(c => c.CeMatrix == minCeMatrix)
                    .Include(r => r.Row)
                    .Include(r => r.Column),
                readOnlyDbContext.Cells.Where(c => c.CeMatrix == maxCeMatrix)
                    .Include(r => r.Row)
                    .Include(r => r.Column),
                itemVersionComparisonArgs,
                CellEqualityComparer.Instance,
                CellValueEqualityComparer.Instance,
                maxCeMatrix.Id,
                i => "Строка: " + RowOrColumnHelper.GetDesc(i.Row) + @"; Столбец: " + RowOrColumnHelper.GetDesc(i.Column),
                i => @"",
                i => i.Value,
                cells_Infos);

            List<ItemVersionComparisonInfo> ceMatrixComments_Infos = new();            
            CompareVersionsEntities(
                minCeMatrix.CeMatrixComments,
                maxCeMatrix.CeMatrixComments,
                itemVersionComparisonArgs,
                StringIdentifierEqualityComparer<CeMatrixComment>.Instance,
                null,
                maxCeMatrix.Id,
                i => i.Identifier,
                i => @"",
                i => i.Value,
                ceMatrixComments_Infos);

            if (ceMatrixParams_Infos.Count > 0 || ceMatrixDbFileReferences_Infos.Count > 0 || rows_Infos.Count > 0 ||
                columns_Infos.Count > 0 || cells_Infos.Count > 0 || ceMatrixComments_Infos.Count > 0)
            {
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(CeMatrix),
                    OldObjectId = minCeMatrix.Id,
                    NewObjectId = maxCeMatrix.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    ParentNewObjectId = null,
                    ObjectName = maxCeMatrix.Identifier,
                    ObjectDesc = maxProjectAllParamValues.CeMatricesParamValues.GetValueOrDefault(maxCeMatrix.Identifier + "." + PazCheckConstants.ParamName_Title) ?? @"",
                });
                infos.AddRange(ceMatrixParams_Infos);
                infos.AddRange(ceMatrixDbFileReferences_Infos);
                infos.AddRange(rows_Infos);
                infos.AddRange(columns_Infos);
                infos.AddRange(cells_Infos);
                infos.AddRange(ceMatrixComments_Infos);
            }
        }

        public static async Task CompareVersionsTagsAsync(
            PazCheckDbContext readOnlyDbContext,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,            
            List<ItemVersionComparisonInfo> infos)
        {
            List<Tag> maxConstTags = await readOnlyDbContext.Tags.Where(GetMaxConstPredicate<Tag>(itemVersionComparisonArgs)) 
                .Include(t => t.TagDbFileReferences)
                .ToListAsync();

            List<Tag> minExceptConstTags = await readOnlyDbContext.Tags.Where(GetMinExceptConstPredicate<Tag>(itemVersionComparisonArgs))
                .Include(t => t.TagDbFileReferences)
                .ToListAsync();

            List<Tag> maxExceptConstTags = await readOnlyDbContext.Tags.Where(GetMaxExceptConstPredicate<Tag>(itemVersionComparisonArgs))                 
                .Include(t => t.TagDbFileReferences)
                .ToListAsync();

            maxExceptConstTags.Intersect(
                minExceptConstTags,
                out List<Tag> maxUniqueCollection,
                out List<Tag> maxIntersectionCollection,
                out List<Tag> minUniqueCollection,
                out List<Tag> minIntersectionCollection,
                StringIdentifierEqualityComparer<Tag>.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                null,
                t => t.Identifier,
                t => maxProjectAllParamValues.TagsParamValues.GetValueOrDefault(t.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @"",
                t => @"",
                itemVersionComparisonArgs,
                infos);            

            foreach (var index in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                CompareVersionsTag(
                    readOnlyDbContext, 
                    minIntersectionCollection[index], 
                    maxIntersectionCollection[index],
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }

            foreach (var tag in maxConstTags)
            {
                CompareVersionsTag(
                    readOnlyDbContext, 
                    tag, 
                    tag,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }
        }

        public static void CompareVersionsTag(
            PazCheckDbContext readOnlyDbContext, 
            Tag minTag, 
            Tag maxTag,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,            
            List<ItemVersionComparisonInfo> infos)
        {
            List<ItemVersionComparisonInfo> tagParams_Infos = new();
            CompareVersionsParams(
                minProjectAllParamValues.TagsParams.GetValueOrDefault(minTag.Identifier)!,
                maxProjectAllParamValues.TagsParams.GetValueOrDefault(maxTag.Identifier)!,
                paramDescs,
                itemVersionComparisonArgs,
                maxTag.Id,
                tagParams_Infos);            

            List<ItemVersionComparisonInfo> tagConditions_Infos = new();
            CompareVersionsTagConditions(
                minProjectAllParamValues.TagConditions.GetValueOrDefault(minTag.Identifier)!,
                maxProjectAllParamValues.TagConditions.GetValueOrDefault(maxTag.Identifier)!,
                itemVersionComparisonArgs,                
                maxTag.Id,                
                tagConditions_Infos);

            List<ItemVersionComparisonInfo> tagDbFileReferences_Infos = new();
            CompareVersionsDbFileReferences(
                minTag.TagDbFileReferences, 
                maxTag.TagDbFileReferences,
                itemVersionComparisonArgs,
                maxTag.Id,
                tagDbFileReferences_Infos);

            if (tagParams_Infos.Count > 0 || tagConditions_Infos.Count > 0 || tagDbFileReferences_Infos.Count > 0)
            {
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(Tag),
                    OldObjectId = minTag.Id,
                    NewObjectId = maxTag.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    ParentNewObjectId = null,
                    ObjectName = maxTag.Identifier,
                    ObjectDesc = maxProjectAllParamValues.TagsParamValues.GetValueOrDefault(maxTag.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @"",
                });
                infos.AddRange(tagParams_Infos);                
                infos.AddRange(tagConditions_Infos);
                infos.AddRange(tagDbFileReferences_Infos);
            }
        }

        public static async Task CompareVersionsBaseActuatorsAsync(
            PazCheckDbContext readOnlyDbContext,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,            
            List<ItemVersionComparisonInfo> infos)
        {
            List<BaseActuator> maxConstBaseActuators = await readOnlyDbContext.BaseActuators.Where(GetMaxConstPredicate<BaseActuator>(itemVersionComparisonArgs))                    
                .Include(ba => ba.BaseActuatorDbFileReferences)
                .ToListAsync();

            List<BaseActuator> minExceptConstBaseActuators = await readOnlyDbContext.BaseActuators.Where(GetMinExceptConstPredicate<BaseActuator>(itemVersionComparisonArgs))                
                .Include(ba => ba.BaseActuatorDbFileReferences)
                .ToListAsync();

            List<BaseActuator> maxExceptConstBaseActuators = await readOnlyDbContext.BaseActuators.Where(GetMaxExceptConstPredicate<BaseActuator>(itemVersionComparisonArgs))                
                .Include(ba => ba.BaseActuatorDbFileReferences)
                .ToListAsync();

            maxExceptConstBaseActuators.Intersect(
                minExceptConstBaseActuators,
                out List<BaseActuator> maxUniqueCollection,
                out List<BaseActuator> maxIntersectionCollection,
                out List<BaseActuator> minUniqueCollection,
                out List<BaseActuator> minIntersectionCollection,
                StringIdentifierEqualityComparer<BaseActuator>.Instance);

            AddDeletedAndAddedToResult(minUniqueCollection,
                maxUniqueCollection,
                null,
                ba => ba.Identifier,
                ba => maxProjectAllParamValues.BaseActuatorsParamValues.GetValueOrDefault(ba.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @"",
                ba => @"",
                itemVersionComparisonArgs,
                infos);

            foreach (var index in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                CompareVersionsBaseActuator(
                    readOnlyDbContext, 
                    minIntersectionCollection[index],
                    maxIntersectionCollection[index],
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }

            foreach (var baseActuator in maxConstBaseActuators)
            {
                CompareVersionsBaseActuator(
                    readOnlyDbContext, 
                    baseActuator, 
                    baseActuator,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }
        }

        public static void CompareVersionsBaseActuator(
            PazCheckDbContext readOnlyDbContext, 
            BaseActuator minBaseActuator, 
            BaseActuator maxBaseActuator,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs, 
            List<ItemVersionComparisonInfo> infos)
        {
            List<ItemVersionComparisonInfo> baseActuatorParams_Infos = new();
            CompareVersionsParams(
                minProjectAllParamValues.BaseActuatorsParams.GetValueOrDefault(minBaseActuator.Identifier)!,
                maxProjectAllParamValues.BaseActuatorsParams.GetValueOrDefault(maxBaseActuator.Identifier)!,
                paramDescs,
                itemVersionComparisonArgs,
                maxBaseActuator.Id,
                baseActuatorParams_Infos);

            List<ItemVersionComparisonInfo> baseActuatorDbFileReferences_Infos = new();
            CompareVersionsDbFileReferences(
                minBaseActuator.BaseActuatorDbFileReferences, 
                maxBaseActuator.BaseActuatorDbFileReferences,
                itemVersionComparisonArgs,
                maxBaseActuator.Id,
                baseActuatorDbFileReferences_Infos);

            if (baseActuatorParams_Infos.Count > 0 || baseActuatorDbFileReferences_Infos.Count > 0)
            {
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(BaseActuator),
                    OldObjectId = minBaseActuator.Id,
                    NewObjectId = maxBaseActuator.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    ParentNewObjectId = null,
                    ObjectName = maxBaseActuator.Identifier,
                    ObjectDesc = maxProjectAllParamValues.BaseActuatorsParamValues.GetValueOrDefault(maxBaseActuator.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @""
                });
                infos.AddRange(baseActuatorParams_Infos);
                infos.AddRange(baseActuatorDbFileReferences_Infos);
            }
        }

        public static async Task CompareVersionsSafetyControllersAsync(
            PazCheckDbContext readOnlyDbContext,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs, 
            List<ItemVersionComparisonInfo> infos)
        {
            List<SafetyController> maxConstSafetyControllers = await readOnlyDbContext.SafetyControllers.Where(GetMaxConstPredicate<SafetyController>(itemVersionComparisonArgs))                    
                .Include(sc => sc.SafetyControllerDbFileReferences)
                .ToListAsync();

            List<SafetyController> minExceptConstSafetyControllers = await readOnlyDbContext.SafetyControllers.Where(GetMinExceptConstPredicate<SafetyController>(itemVersionComparisonArgs))                
                .Include(sc => sc.SafetyControllerDbFileReferences)
                .ToListAsync();

            List<SafetyController> maxExceptConstSafetyControllers = await readOnlyDbContext.SafetyControllers.Where(GetMaxExceptConstPredicate<SafetyController>(itemVersionComparisonArgs))                
                .Include(sc => sc.SafetyControllerDbFileReferences)
                .ToListAsync();

            maxExceptConstSafetyControllers.Intersect(
                minExceptConstSafetyControllers,
                out List<SafetyController> maxUniqueCollection,
                out List<SafetyController> maxIntersectionCollection,
                out List<SafetyController> minUniqueCollection,
                out List<SafetyController> minIntersectionCollection,
                StringIdentifierEqualityComparer<SafetyController>.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                null,
                sc => sc.Identifier,
                sc => maxProjectAllParamValues.SafetyControllersParamValues.GetValueOrDefault(sc.Identifier + "." + PazCheckConstants.ParamName_Title) ?? @"",
                sc => @"",
                itemVersionComparisonArgs,
                infos);

            foreach (var index in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                CompareVersionsSafetyController(
                    readOnlyDbContext, 
                    minIntersectionCollection[index], 
                    maxIntersectionCollection[index],
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }

            foreach (var safetyController in maxConstSafetyControllers)
            {
                CompareVersionsSafetyController(
                    readOnlyDbContext, 
                    safetyController, 
                    safetyController,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs, 
                    infos);
            }
        }

        public static void CompareVersionsSafetyController(
            PazCheckDbContext readOnlyDbContext, 
            SafetyController minSafetyController, 
            SafetyController maxSafetyController,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs, 
            List<ItemVersionComparisonInfo> infos)
        {
            List<ItemVersionComparisonInfo> safetyControllerParams_Infos = new();
            CompareVersionsParams(
                minProjectAllParamValues.SafetyControllersParams.GetValueOrDefault(minSafetyController.Identifier)!,
                maxProjectAllParamValues.SafetyControllersParams.GetValueOrDefault(maxSafetyController.Identifier)!,
                paramDescs,
                itemVersionComparisonArgs,
                maxSafetyController.Id,
                safetyControllerParams_Infos);

            List<ItemVersionComparisonInfo> safetyControllerDbFileReferences_Infos = new();
            CompareVersionsDbFileReferences(
                minSafetyController.SafetyControllerDbFileReferences, 
                maxSafetyController.SafetyControllerDbFileReferences,
                itemVersionComparisonArgs,
                maxSafetyController.Id,
                safetyControllerDbFileReferences_Infos);

            if (safetyControllerParams_Infos.Count > 0 || safetyControllerDbFileReferences_Infos.Count > 0)
            {
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(SafetyController),
                    OldObjectId = minSafetyController.Id,
                    NewObjectId = maxSafetyController.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    ParentNewObjectId = null,
                    ObjectName = maxSafetyController.Identifier,
                    ObjectDesc = maxProjectAllParamValues.SafetyControllersParamValues.GetValueOrDefault(maxSafetyController.Identifier + "." + PazCheckConstants.ParamName_Title) ?? @""
                });
                infos.AddRange(safetyControllerParams_Infos);
                infos.AddRange(safetyControllerDbFileReferences_Infos);
            }
        }

        public static async Task CompareVersionsLegendsAsync(
            PazCheckDbContext readOnlyDbContext,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs, 
            List<ItemVersionComparisonInfo> infos)
        {
            List<Legend> maxConstLegends = await readOnlyDbContext.Legends.Where(GetMaxConstPredicate<Legend>(itemVersionComparisonArgs))
                .ToListAsync();

            List<Legend> minExceptConstLegends = await readOnlyDbContext.Legends.Where(GetMinExceptConstPredicate<Legend>(itemVersionComparisonArgs))
                .ToListAsync();

            List<Legend> maxExceptConstLegends = await readOnlyDbContext.Legends.Where(GetMaxExceptConstPredicate<Legend>(itemVersionComparisonArgs))
                .ToListAsync();

            maxExceptConstLegends.Intersect(
                minExceptConstLegends,
                out List<Legend> maxUniqueCollection,
                out List<Legend> maxIntersectionCollection,
                out List<Legend> minUniqueCollection,
                out List<Legend> minIntersectionCollection,
                StringIdentifierEqualityComparer<Legend>.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                null,
                sc => sc.Identifier,
                sc => maxProjectAllParamValues.LegendsParamValues.GetValueOrDefault(sc.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @"",
                sc => @"",
                itemVersionComparisonArgs,
                infos);

            foreach (var index in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                CompareVersionsLegend(
                    readOnlyDbContext,
                    minIntersectionCollection[index],
                    maxIntersectionCollection[index],
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs,
                    infos);
            }

            foreach (var legend in maxConstLegends)
            {
                CompareVersionsLegend(
                    readOnlyDbContext,
                    legend,
                    legend,
                    minProjectAllParamValues,
                    maxProjectAllParamValues,
                    paramDescs,
                    itemVersionComparisonArgs,
                    infos);
            }
        }

        public static void CompareVersionsLegend(
            PazCheckDbContext readOnlyDbContext,
            Legend minLegend,
            Legend maxLegend,
            ProjectAllParamValues minProjectAllParamValues,
            ProjectAllParamValues maxProjectAllParamValues,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            List<ItemVersionComparisonInfo> infos)
        {
            List<ItemVersionComparisonInfo> legendParams_Infos = new();
            CompareVersionsParams(
                minProjectAllParamValues.LegendsParams.GetValueOrDefault(minLegend.Identifier)!,
                maxProjectAllParamValues.LegendsParams.GetValueOrDefault(maxLegend.Identifier)!,
                paramDescs,
                itemVersionComparisonArgs,
                maxLegend.Id,
                legendParams_Infos);

            List<ItemVersionComparisonInfo> legendDbFileReferences_Infos = new();
            CompareVersionsDbFileReferences(
                minLegend.LegendDbFileReferences,
                maxLegend.LegendDbFileReferences,
                itemVersionComparisonArgs,
                maxLegend.Id,
                legendDbFileReferences_Infos);

            if (legendParams_Infos.Count > 0 || legendDbFileReferences_Infos.Count > 0)
            {
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(Legend),
                    OldObjectId = minLegend.Id,
                    NewObjectId = maxLegend.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    ParentNewObjectId = null,
                    ObjectName = maxLegend.Identifier,
                    ObjectDesc = maxProjectAllParamValues.LegendsParamValues.GetValueOrDefault(maxLegend.Identifier + "." + PazCheckConstants.ParamName_Desc) ?? @""
                });
                infos.AddRange(legendParams_Infos);
                infos.AddRange(legendDbFileReferences_Infos);
            }
        }

        public static void GetVersions<TEntity>(List<TEntity> entitiesList, HashSet<uint> projectVersionNums, string user)
            where TEntity : VersionedEntityBase
        {
            foreach (var entity in entitiesList)
                GetVersions(entity, projectVersionNums, user);
        }

        public static void GetVersions<TEntity>(TEntity entity, HashSet<uint> projectVersionNums, string user)
            where TEntity : VersionedEntityBase
        {
            if (entity._CreateProjectVersionNum is not null)
            {
                UInt32 minProjectVersionNum = entity._CreateProjectVersionNum.Value;
                projectVersionNums.Add(minProjectVersionNum);
                UInt32? maxProjectVersionNum = entity._DeleteProjectVersionNum;
                if (maxProjectVersionNum is not null)
                    projectVersionNums.Add(maxProjectVersionNum.Value);
            }

            if (entity._HasUnversionedChanges)
            {
                if (String.IsNullOrEmpty(user))
                    projectVersionNums.Add(0);
                else if (entity._LastChangeUser == user || entity._LastChangeUser == @"")
                    projectVersionNums.Add(0);
            }
        }

        /// <summary>
        ///      Returns (File Name, FileData)
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="versionedEntityType"></param>
        /// <param name="versionedEntityId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="dbCache"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<(string?, Stream?)> ExportVersionComparisonToFileAsync(PazCheckDbContext readOnlyDbContext, 
            Type versionedEntityType,
            int versionedEntityId,
            UInt32 minProjectVersionNum,
            UInt32? maxProjectVersionNum,
            DbCache dbCache,
            string user)           
        {
            List<ItemVersionComparisonInfo> infos = new();
            string identifier = @"";
            switch (versionedEntityType.Name) 
            {
                case nameof(Project):
                    {
                        Project project = await readOnlyDbContext.Projects                                
                                .SingleAsync(m => m.Id == versionedEntityId);
                        identifier = project.Title;

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = versionedEntityId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == versionedEntityId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        await CompareVersionsCeMatricesAsync(readOnlyDbContext,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                        await CompareVersionsTagsAsync(readOnlyDbContext,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                        await CompareVersionsBaseActuatorsAsync(readOnlyDbContext,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                        await CompareVersionsSafetyControllersAsync(readOnlyDbContext,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                        await CompareVersionsLegendsAsync(readOnlyDbContext,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                    }
                    break;
                case nameof(CeMatrix):
                    {
                        CeMatrix maxCeMatrix = await readOnlyDbContext.CeMatrices                                
                                .Include(m => m.CeMatrixDbFileReferences)
                                .Include(m => m.CeMatrixComments)
                                .SingleAsync(m => m.Id == versionedEntityId);
                        identifier = maxCeMatrix.Identifier;
                        CeMatrix minCeMatrix = await readOnlyDbContext.CeMatrices
                                .Where(GetVersionEntityPredicate<CeMatrix>(minProjectVersionNum, maxCeMatrix.ProjectId, identifier))                                
                                .Include(m => m.CeMatrixDbFileReferences)
                                .Include(m => m.CeMatrixComments)
                                .SingleAsync();

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = maxCeMatrix.ProjectId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxCeMatrix.ProjectId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        await CompareVersionsCeMatrixAsync(readOnlyDbContext, 
                            minCeMatrix, 
                            maxCeMatrix,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);                        
                    }
                    break;
                case nameof(Tag):
                    {
                        Tag maxTag = await readOnlyDbContext.Tags 
                                .Include(t => t.TagDbFileReferences)
                                .SingleAsync(t => t.Id == versionedEntityId);
                        identifier = maxTag.Identifier;
                        Tag minTag = await readOnlyDbContext.Tags
                                .Where(PazCheckDbHelper.GetVersionEntityPredicate<Tag>(minProjectVersionNum, maxTag.ProjectId, identifier))
                                .Include(t => t.TagDbFileReferences)
                                .SingleAsync();

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = maxTag.ProjectId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxTag.ProjectId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        CompareVersionsTag(readOnlyDbContext, 
                            minTag, 
                            maxTag,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                    }
                    break;
                case nameof(BaseActuator):
                    {
                        BaseActuator maxBaseActuator = await readOnlyDbContext.BaseActuators                                
                                .Include(ba => ba.BaseActuatorDbFileReferences)
                                .SingleAsync(ba => ba.Id == versionedEntityId);
                        identifier = maxBaseActuator.Identifier;
                        BaseActuator minBaseActuator = await readOnlyDbContext.BaseActuators
                                .Where(PazCheckDbHelper.GetVersionEntityPredicate<BaseActuator>(minProjectVersionNum, maxBaseActuator.ProjectId, identifier))                                
                                .Include(ba => ba.BaseActuatorDbFileReferences)
                                .SingleAsync();

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = maxBaseActuator.ProjectId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxBaseActuator.ProjectId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        CompareVersionsBaseActuator(readOnlyDbContext, 
                            minBaseActuator, 
                            maxBaseActuator,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                    }
                    break;
                case nameof(SafetyController):
                    {
                        SafetyController maxSafetyController = await readOnlyDbContext.SafetyControllers                                
                                .Include(sc => sc.SafetyControllerDbFileReferences)
                                .SingleAsync(sc => sc.Id == versionedEntityId);
                        identifier = maxSafetyController.Identifier;
                        SafetyController minSafetyController = await readOnlyDbContext.SafetyControllers
                                .Where(PazCheckDbHelper.GetVersionEntityPredicate<SafetyController>(minProjectVersionNum, maxSafetyController.ProjectId, identifier))                                
                                .Include(sc => sc.SafetyControllerDbFileReferences)
                                .SingleAsync();

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = maxSafetyController.ProjectId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxSafetyController.ProjectId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        CompareVersionsSafetyController(readOnlyDbContext, 
                            minSafetyController, 
                            maxSafetyController,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs, 
                            infos);
                    }
                    break;
                case nameof(Legend):
                    {
                        Legend maxLegend = await readOnlyDbContext.Legends
                                .Include(sc => sc.LegendDbFileReferences)
                                .SingleAsync(sc => sc.Id == versionedEntityId);
                        identifier = maxLegend.Identifier;
                        Legend minLegend = await readOnlyDbContext.Legends
                                .Where(PazCheckDbHelper.GetVersionEntityPredicate<Legend>(minProjectVersionNum, maxLegend.ProjectId, identifier))
                                .Include(sc => sc.LegendDbFileReferences)
                                .SingleAsync();

                        ItemVersionComparisonArgs itemVersionComparisonArgs = new()
                        {
                            ProjectId = maxLegend.ProjectId,
                            MinProjectVersionNum = minProjectVersionNum,
                            MaxProjectVersionNum = maxProjectVersionNum,
                            User = user,
                            ProjectVersions = readOnlyDbContext.ProjectVersions.Where(pv => pv.ProjectId == maxLegend.ProjectId).ToDictionary(pv => pv.VersionNum)
                        };

                        var minProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MinProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);
                        var maxProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                                itemVersionComparisonArgs.ProjectId,
                                itemVersionComparisonArgs.MaxProjectVersionNum,
                                readOnlyDbContext,
                                LoggersSet.Empty);

                        CompareVersionsLegend(readOnlyDbContext,
                            minLegend,
                            maxLegend,
                            minProjectAllParamValues,
                            maxProjectAllParamValues,
                            dbCache.ParamDescs,
                            itemVersionComparisonArgs,
                            infos);
                    }
                    break;
            }

            byte[] resultBytes;
            using (XLWorkbook workbook = new())
            {
                ItemVersionComparisonInfo[] filteredInfo;
                var worksheet = workbook.Worksheets.Add(ExcelHelper.MakeValidSheetName("Результаты сравнения"));                

                int row = -1;

                foreach (var e in infos.Where(i => i.ParentNewObjectId == null && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Modified))
                {
                    string rootEnitityTypeDesc = e.ObjectType;
                    if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(e.ObjectType, out PropertyInfo? rootPopertyInfo))
                    {
                        var pcDisplayNameAttribute = rootPopertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                        rootEnitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                    }

                    row += 2;
                    worksheet.Cell(row, 1).Value = "Изменено:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = "";
                    worksheet.Cell(row, 3).Value = rootEnitityTypeDesc;
                    worksheet.Cell(row, 4).Value = e.ObjectName;
                    worksheet.Cell(row, 5).Value = e.ObjectDesc;

                    // Modified
                    filteredInfo = infos
                        .Where(i => i.ParentNewObjectId == e.NewObjectId && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Modified)                        
                        .ToArray();
                    if (filteredInfo.Length > 0)
                    {
                        row += 1;                        

                        worksheet.Cell(row, 1).Value = "";
                        worksheet.Cell(row, 2).Value = "";
                        worksheet.Cell(row, 3).Value = "";
                        worksheet.Cell(row, 4).Value = "";
                        worksheet.Cell(row, 5).Value = "";
                        worksheet.Cell(row, 6).Value = "Стар. значение";
                        worksheet.Cell(row, 7).Value = "Нов. значение";
                        worksheet.Cell(row, 8).Value = "Время изменения";
                        worksheet.Cell(row, 8).Value = "Кем изменено";
                        worksheet.Cell(row, 10).Value = "Комментарий";
                        worksheet.Cell(row, 11).Value = "Время утверждения";
                        worksheet.Cell(row, 12).Value = "Кем утверждено";
                        worksheet.Range($"B{row}:K{row}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                        foreach (var info in filteredInfo)
                        {
                            row += 1;
                            string enitityTypeDesc = info.ObjectType;
                            if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(info.ObjectType, out PropertyInfo? propertyInfo))
                            {
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                enitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                            }
                            worksheet.Cell(row, 1).Value = "Изменено:";
                            worksheet.Cell(row, 1).Style.Font.Bold = true;
                            worksheet.Cell(row, 2).Value = e.ObjectName;
                            worksheet.Cell(row, 3).Value = enitityTypeDesc;
                            worksheet.Cell(row, 4).Value = info.ObjectName;
                            worksheet.Cell(row, 5).Value = info.ObjectDesc;
                            worksheet.Cell(row, 6).Value = info.OldValue;
                            worksheet.Cell(row, 7).Value = info.NewValue;
                            worksheet.Cell(row, 8).Value = info.ChangeTimeUtc.ToLocalTime().ToString(@"G");
                            worksheet.Cell(row, 9).Value = info.ChangedBy_User;
                            worksheet.Cell(row, 10).Value = info.Change_Comment;
                            worksheet.Cell(row, 11).Value = info.ApproveTimeUtc.HasValue ? info.ApproveTimeUtc.Value.ToLocalTime().ToString(@"G") : @"";
                            worksheet.Cell(row, 12).Value = info.ApprovedBy_User;
                        }
                    }

                    // Added
                    filteredInfo = infos
                        .Where(i => i.ParentNewObjectId == e.NewObjectId && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Added)                        
                        .ToArray();
                    if (filteredInfo.Length > 0)
                    {
                        row += 1;                        

                        worksheet.Cell(row, 1).Value = "";
                        worksheet.Cell(row, 2).Value = "";
                        worksheet.Cell(row, 3).Value = "";
                        worksheet.Cell(row, 4).Value = "";
                        worksheet.Cell(row, 5).Value = "";
                        worksheet.Cell(row, 6).Value = "";
                        worksheet.Cell(row, 7).Value = "Значение";
                        worksheet.Cell(row, 8).Value = "Время добавления";
                        worksheet.Cell(row, 9).Value = "Кем добавлено";
                        worksheet.Cell(row, 10).Value = "Комментарий";
                        worksheet.Cell(row, 11).Value = "Время утверждения";
                        worksheet.Cell(row, 12).Value = "Кем утверждено";
                        worksheet.Range($"B{row}:K{row}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                        foreach (var info in filteredInfo)
                        {
                            row += 1;
                            string enitityTypeDesc = info.ObjectType;
                            if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(info.ObjectType, out PropertyInfo? propertyInfo))
                            {
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                enitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                            }
                            worksheet.Cell(row, 1).Value = "Добавлено:";
                            worksheet.Cell(row, 1).Style.Font.Bold = true;
                            worksheet.Cell(row, 2).Value = e.ObjectName;
                            worksheet.Cell(row, 3).Value = enitityTypeDesc;
                            worksheet.Cell(row, 4).Value = info.ObjectName;
                            worksheet.Cell(row, 5).Value = info.ObjectDesc;
                            worksheet.Cell(row, 6).Value = info.OldValue;
                            worksheet.Cell(row, 7).Value = info.NewValue;
                            worksheet.Cell(row, 8).Value = info.ChangeTimeUtc.ToLocalTime().ToString(@"G");
                            worksheet.Cell(row, 9).Value = info.ChangedBy_User;
                            worksheet.Cell(row, 10).Value = info.Change_Comment;
                            worksheet.Cell(row, 11).Value = info.ApproveTimeUtc.HasValue ? info.ApproveTimeUtc.Value.ToLocalTime().ToString(@"G") : @"";
                            worksheet.Cell(row, 12).Value = info.ApprovedBy_User;
                        }
                    }

                    // Removed
                    filteredInfo = infos
                        .Where(i => i.ParentNewObjectId == e.NewObjectId && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Deleted)                        
                        .ToArray();
                    if (filteredInfo.Length > 0)
                    {
                        row += 1;                        

                        worksheet.Cell(row, 1).Value = "";
                        worksheet.Cell(row, 2).Value = "";
                        worksheet.Cell(row, 3).Value = "";
                        worksheet.Cell(row, 4).Value = "";
                        worksheet.Cell(row, 5).Value = "";
                        worksheet.Cell(row, 6).Value = "Значение";
                        worksheet.Cell(row, 7).Value = "";
                        worksheet.Cell(row, 8).Value = "Время удаления";
                        worksheet.Cell(row, 9).Value = "Кем удалено";
                        worksheet.Cell(row, 10).Value = "Комментарий";
                        worksheet.Cell(row, 11).Value = "Время утверждения";
                        worksheet.Cell(row, 12).Value = "Кем утверждено";
                        worksheet.Range($"B{row}:K{row}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                        foreach (var info in filteredInfo)
                        {
                            row += 1;
                            string enitityTypeDesc = info.ObjectType;
                            if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(info.ObjectType, out PropertyInfo? propertyInfo))
                            {
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                enitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                            }
                            worksheet.Cell(row, 1).Value = "Удалено:";
                            worksheet.Cell(row, 1).Style.Font.Bold = true;
                            worksheet.Cell(row, 2).Value = e.ObjectName;
                            worksheet.Cell(row, 3).Value = enitityTypeDesc;
                            worksheet.Cell(row, 4).Value = info.ObjectName;
                            worksheet.Cell(row, 5).Value = info.ObjectDesc;
                            worksheet.Cell(row, 6).Value = info.OldValue;
                            worksheet.Cell(row, 7).Value = info.NewValue;
                            worksheet.Cell(row, 8).Value = info.ChangeTimeUtc.ToLocalTime().ToString(@"G");
                            worksheet.Cell(row, 9).Value = info.ChangedBy_User;
                            worksheet.Cell(row, 10).Value = info.Change_Comment;
                            worksheet.Cell(row, 11).Value = info.ApproveTimeUtc.HasValue ? info.ApproveTimeUtc.Value.ToLocalTime().ToString(@"G") : @"";
                            worksheet.Cell(row, 12).Value = info.ApprovedBy_User;
                        }
                    }
                }

                row += 1;

                foreach (var e in infos.Where(i => i.ParentNewObjectId == null && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Added))
                {
                    string rootEnitityTypeDesc = e.ObjectType;
                    if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(e.ObjectType, out PropertyInfo? rootPopertyInfo))
                    {
                        var pcDisplayNameAttribute = rootPopertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                        rootEnitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                    }

                    row += 1;
                    worksheet.Cell(row, 1).Value = "Добавлено:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = "";
                    worksheet.Cell(row, 3).Value = rootEnitityTypeDesc;
                    worksheet.Cell(row, 4).Value = e.ObjectName;
                    worksheet.Cell(row, 5).Value = e.ObjectDesc;
                }

                row += 1;

                foreach (var e in infos.Where(i => i.ParentNewObjectId == null && i.ChangeType == ItemVersionComparisonInfo.ChangeType_Deleted))
                {
                    string rootEnitityTypeDesc = e.ObjectType;
                    if (PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(e.ObjectType, out PropertyInfo? rootPopertyInfo))
                    {
                        var pcDisplayNameAttribute = rootPopertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                        rootEnitityTypeDesc = pcDisplayNameAttribute.DisplayName;
                    }

                    row += 1;
                    worksheet.Cell(row, 1).Value = "Удалено:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = "";
                    worksheet.Cell(row, 3).Value = rootEnitityTypeDesc;
                    worksheet.Cell(row, 4).Value = e.ObjectName;
                    worksheet.Cell(row, 5).Value = e.ObjectDesc;
                }

                worksheet.AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                resultBytes = stream.ToArray();
            }            

            List<(string, byte[])> filesData = new();

            filesData.Add((@"Результаты сравнения.xlsx", resultBytes));

            return (@"Результаты сравнения " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip", SerializationHelper.ExportZip(filesData));
        }

        public static Dictionary<string, string?> GetChanges(EntityEntry entry, PropertyValues? databaseValues)
        {
            Dictionary<string, string?> result = new();
            if (databaseValues is null)
                return result;
            foreach (var propertyEntry in entry.Properties)
            {
                var propertyInfo = propertyEntry.Metadata.PropertyInfo;
                if (propertyInfo is not null && !propertyInfo.Name.StartsWith(@"_"))
                {
                    var databaseValue = databaseValues[propertyInfo.Name];
                    if (!Object.Equals(databaseValue, propertyEntry.CurrentValue))
                    {
                        var pcDisplayNameAttribute = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>();
                        string key;
                        if (pcDisplayNameAttribute is not null)
                            key = pcDisplayNameAttribute.DisplayName;
                        else
                            key = propertyInfo.Name;
                        result.Add(key, databaseValue + " -> " + propertyEntry.CurrentValue);
                    }                    
                }
            }
            return result;
        }

        public static void SafeRemoveRange<T>(PazCheckDbContext dbContext, List<T> entities)
            where T : class
        {
            foreach (var entity in entities)
            {
                var entry = dbContext.Entry(entity);
                if (entry.State == EntityState.Added)
                    entry.State = EntityState.Detached;
                else
                    entry.State = EntityState.Deleted;
            }
        }

        #endregion

        #region private functions

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetLockedByUserPredicate<TProjectVersionEntityBase>(string? user, int projectId)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (String.IsNullOrEmpty(user))
                return e => e.ProjectId == projectId && e._LockedByUser != @"";
            else
                return e => e.ProjectId == projectId && e._LockedByUser == user;
        }

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetUnversionedChangesPredicate<TProjectVersionEntityBase>(string? user, int projectId)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (String.IsNullOrEmpty(user))
                return e => e.ProjectId == projectId && e._HasUnversionedChanges;
            else
                return e => e.ProjectId == projectId && e._HasUnversionedChanges && (e._LastChangeUser == user || e._LastChangeUser == @"");
        }

        private static void SaveUnversionedChanges_CeMatrix(PazCheckDbContext dbContext, uint projectVersionNum, CeMatrix ceMatrix)
        {
            foreach (var ceMatrixParam in ceMatrix.CeMatrixParams)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, ceMatrixParam, ceMatrix._IsDeleted);
            }

            foreach (var ceMatrixDbFileReference in ceMatrix.CeMatrixDbFileReferences)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, ceMatrixDbFileReference, ceMatrix._IsDeleted);
            }

            foreach (var row in ceMatrix.Rows)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, row, ceMatrix._IsDeleted);
            }

            foreach (var column in ceMatrix.Columns)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, column, ceMatrix._IsDeleted);
            }

            foreach (var cell in ceMatrix.Cells)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, cell, ceMatrix._IsDeleted);
            }

            foreach (var ceMatrixComment in ceMatrix.CeMatrixComments)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, ceMatrixComment, ceMatrix._IsDeleted);
            }
            
            SaveUnversionedChanges(dbContext, projectVersionNum, ceMatrix, false);
        }

        private static void SaveUnversionedChanges_Tag(PazCheckDbContext dbContext, uint projectVersionNum, Tag tag)
        {
            foreach (var tagParam in tag.TagParams)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, tagParam, tag._IsDeleted);
            }            

            foreach (var tagCondition in tag.TagConditions)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, tagCondition, tag._IsDeleted);
            }

            foreach (var tagDbFileReference in tag.TagDbFileReferences)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, tagDbFileReference, tag._IsDeleted);
            }
            
            SaveUnversionedChanges(dbContext, projectVersionNum, tag, false);
        }

        private static void SaveUnversionedChanges_BaseActuator(PazCheckDbContext dbContext, uint projectVersionNum, BaseActuator baseActuator)
        {
            foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, baseActuatorParam, baseActuator._IsDeleted);
            }

            foreach (var baseActuatorDbFileReference in baseActuator.BaseActuatorDbFileReferences)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, baseActuatorDbFileReference, baseActuator._IsDeleted);
            }
            
            SaveUnversionedChanges(dbContext, projectVersionNum, baseActuator, false);
        }

        private static void SaveUnversionedChanges_SafetyController(PazCheckDbContext dbContext, uint projectVersionNum, SafetyController safetyController)
        {
            foreach (var safetyControllerParam in safetyController.SafetyControllerParams)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, safetyControllerParam, safetyController._IsDeleted);
            }

            foreach (var safetyControllerDbFileReference in safetyController.SafetyControllerDbFileReferences)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, safetyControllerDbFileReference, safetyController._IsDeleted);
            }
            
            SaveUnversionedChanges(dbContext, projectVersionNum, safetyController, false);
        }

        private static void SaveUnversionedChanges_Legend(PazCheckDbContext dbContext, uint projectVersionNum, Legend legend)
        {
            foreach (var legendParam in legend.LegendParams)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, legendParam, legend._IsDeleted);
            }

            foreach (var legendDbFileReference in legend.LegendDbFileReferences)
            {
                SaveUnversionedChanges(dbContext, projectVersionNum, legendDbFileReference, legend._IsDeleted);
            }

            SaveUnversionedChanges(dbContext, projectVersionNum, legend, false);
        }

        private static void SaveUnversionedChanges(PazCheckDbContext dbContext, uint projectVersionNum, VersionedEntityBase versionEntity, bool parentIsDeleted)
        {
            if (parentIsDeleted)
                versionEntity._IsDeleted = true;
            if (versionEntity._CreateProjectVersionNum is null)
            {
                if (versionEntity._IsDeleted)
                    dbContext.Entry(versionEntity).State = EntityState.Deleted;
                else
                    versionEntity._CreateProjectVersionNum = projectVersionNum;
            }
            else if (versionEntity._DeleteProjectVersionNum is null)
            {
                if (versionEntity._IsDeleted)
                    versionEntity._DeleteProjectVersionNum = projectVersionNum;
            }
            versionEntity._LastSavedChangeTimeUtc = versionEntity._LastChangeTimeUtc;
            versionEntity._LastSavedChangeUser = versionEntity._LastChangeUser;
            versionEntity._HasUnversionedChanges = false;
        }

        private static void ClearUnversionedChanges_CeMatrix(PazCheckDbContext dbContext, CeMatrix ceMatrix)
        {
            foreach (var ceMatrixParam in ceMatrix.CeMatrixParams)
            {
                ClearUnversionedChanges(dbContext, ceMatrixParam);
            }

            foreach (var ceMatrixDbFileReference in ceMatrix.CeMatrixDbFileReferences)
            {
                ClearUnversionedChanges(dbContext, ceMatrixDbFileReference);
            }

            foreach (var row in ceMatrix.Rows)
            {
                ClearUnversionedChanges(dbContext, row);
            }

            foreach (var column in ceMatrix.Columns)
            {
                ClearUnversionedChanges(dbContext, column);
            }

            foreach (var cell in ceMatrix.Cells)
            {
                ClearUnversionedChanges(dbContext, cell);
            }

            foreach (var ceMatrixComment in ceMatrix.CeMatrixComments)
            {
                ClearUnversionedChanges(dbContext, ceMatrixComment);
            }
            
            ClearUnversionedChanges(dbContext, ceMatrix);
        }

        private static void ClearUnversionedChanges_Tag(PazCheckDbContext dbContext, Tag tag)
        {
            foreach (var tagParam in tag.TagParams)
            {
                ClearUnversionedChanges(dbContext, tagParam);
            }            

            foreach (var tagCondition in tag.TagConditions)
            {
                ClearUnversionedChanges(dbContext, tagCondition);
            }

            foreach (var tagDbFileReference in tag.TagDbFileReferences)
            {
                ClearUnversionedChanges(dbContext, tagDbFileReference);
            }
            
            ClearUnversionedChanges(dbContext, tag);
        }

        private static void ClearUnversionedChanges_BaseActuator(PazCheckDbContext dbContext, BaseActuator baseActuator)
        {
            foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
            {
                ClearUnversionedChanges(dbContext, baseActuatorParam);
            }

            foreach (var baseActuatorDbFileReferences in baseActuator.BaseActuatorDbFileReferences)
            {
                ClearUnversionedChanges(dbContext, baseActuatorDbFileReferences);
            }
            
            ClearUnversionedChanges(dbContext, baseActuator);
        }

        private static void ClearUnversionedChanges_SafetyController(PazCheckDbContext dbContext, SafetyController safetyController)
        {
            foreach (var safetyControllerParam in safetyController.SafetyControllerParams)
            {
                ClearUnversionedChanges(dbContext, safetyControllerParam);
            }

            foreach (var safetyControllerDbFileReferences in safetyController.SafetyControllerDbFileReferences)
            {
                ClearUnversionedChanges(dbContext, safetyControllerDbFileReferences);
            }
            
            ClearUnversionedChanges(dbContext, safetyController);
        }

        private static void ClearUnversionedChanges_Legend(PazCheckDbContext dbContext, Legend legend)
        {
            foreach (var legendParam in legend.LegendParams)
            {
                ClearUnversionedChanges(dbContext, legendParam);
            }

            foreach (var legendDbFileReferences in legend.LegendDbFileReferences)
            {
                ClearUnversionedChanges(dbContext, legendDbFileReferences);
            }

            ClearUnversionedChanges(dbContext, legend);
        }

        private static void ClearUnversionedChanges(PazCheckDbContext dbContext, VersionedEntityBase versionEntity)
        {
            if (versionEntity._CreateProjectVersionNum is not null &&
                    versionEntity._DeleteProjectVersionNum is null &&
                    versionEntity._IsDeleted)
            {
                versionEntity._IsDeleted = false;
            }
            versionEntity._LastChangeTimeUtc = versionEntity._LastSavedChangeTimeUtc;
            versionEntity._LastChangeUser = versionEntity._LastSavedChangeUser;
            versionEntity._HasUnversionedChanges = false;
        }

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetMinPredicate<TProjectVersionEntityBase>(int projectId, string identifier, UInt32 minProjectVersionNum)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            return e => e.ProjectId == projectId && e.Identifier == identifier &&
                                e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= minProjectVersionNum &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > minProjectVersionNum);
        }

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetMaxConstPredicate<TProjectVersionEntityBase>(ItemVersionComparisonArgs itemVersionComparisonArgs)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (itemVersionComparisonArgs.MaxProjectVersionNum is null)
            {
                if (String.IsNullOrEmpty(itemVersionComparisonArgs.User))
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum && e._IsDeleted == false && 
                        e._HasUnversionedChanges;
                else
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum && e._IsDeleted == false &&
                        e._HasUnversionedChanges && (e._LastChangeUser == itemVersionComparisonArgs.User || e._LastChangeUser == @"");                
            }
            else
            {
                return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > itemVersionComparisonArgs.MaxProjectVersionNum.Value);
            }
        }

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetMaxExceptConstPredicate<TProjectVersionEntityBase>(ItemVersionComparisonArgs itemVersionComparisonArgs)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (itemVersionComparisonArgs.MaxProjectVersionNum is null)
            {
                if (String.IsNullOrEmpty(itemVersionComparisonArgs.User))
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && (e._CreateProjectVersionNum == null || e._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) && e._IsDeleted == false &&
                        e._HasUnversionedChanges;
                else
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && (e._CreateProjectVersionNum == null || e._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) && e._IsDeleted == false &&
                        e._HasUnversionedChanges && (e._LastChangeUser == itemVersionComparisonArgs.User || e._LastChangeUser == @"");
            }
            else
            {
                return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum &&
                    e._CreateProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > itemVersionComparisonArgs.MaxProjectVersionNum.Value);
            }
        }

        private static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetMinExceptConstPredicate<TProjectVersionEntityBase>(ItemVersionComparisonArgs itemVersionComparisonArgs)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (itemVersionComparisonArgs.MaxProjectVersionNum is null)
            {
                if (String.IsNullOrEmpty(itemVersionComparisonArgs.User))
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                        (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) && e._IsDeleted == true;
                else
                    return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                        (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) && e._IsDeleted == true &&
                        (!e._HasUnversionedChanges || e._LastChangeUser == itemVersionComparisonArgs.User || e._LastChangeUser == @"");                
            }
            else
            {
                return e => e.ProjectId == itemVersionComparisonArgs.ProjectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                    e._DeleteProjectVersionNum != null && e._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum &&
                    e._DeleteProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value;
            }
        }               

        private static void AddDeletedAndAddedToResult<TVersionEntity>(
            IEnumerable<TVersionEntity> deletedCollection,
            IEnumerable<TVersionEntity> addedCollection,
            int? parentNewObjectId,
            Func<TVersionEntity, string> getObjectName,
            Func<TVersionEntity, string> getObjectDesc,
            Func<TVersionEntity, string> getObjectValue,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            List<ItemVersionComparisonInfo> infos)
            where TVersionEntity : VersionedEntityBase
        {
            foreach (var deleted in deletedCollection)
            {
                ProjectVersion? projectVersion = null;
                if (deleted._DeleteProjectVersionNum is not null)
                    projectVersion = itemVersionComparisonArgs.ProjectVersions[deleted._DeleteProjectVersionNum.Value];
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = deleted.GetType().Name,
                    OldObjectId = deleted.Id,
                    NewObjectId = deleted.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Deleted,
                    ParentNewObjectId = parentNewObjectId,
                    ObjectName = getObjectName(deleted),
                    ObjectDesc = getObjectDesc(deleted),
                    OldValue = getObjectValue(deleted),
                    ChangeTimeUtc = deleted._LastChangeTimeUtc,
                    ChangedBy_User = deleted._LastChangeUser,
                    Change_Comment = projectVersion?.Comment ?? @"",
                    ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                    ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                });
            }

            foreach (var added in addedCollection)
            {
                ProjectVersion? projectVersion = null;
                if (added._CreateProjectVersionNum is not null)
                    projectVersion = itemVersionComparisonArgs.ProjectVersions[added._CreateProjectVersionNum.Value];
                infos.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = added.GetType().Name,
                    OldObjectId = null,
                    NewObjectId = added.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Added,
                    ParentNewObjectId = parentNewObjectId,
                    ObjectName = getObjectName(added),
                    ObjectDesc = getObjectDesc(added),
                    NewValue = getObjectValue(added),
                    ChangeTimeUtc = added._CreateTimeUtc,
                    ChangedBy_User = added._CreateUser,
                    Change_Comment = projectVersion?.Comment ?? @"",
                    ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                    ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                });
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="minParamsList"></param>
        /// <param name="maxParamsList"></param>
        /// <param name="paramDescs"></param>        
        /// <param name="itemVersionComparisonArgs"></param>
        /// <param name="parentNewObjectId"></param>
        /// <param name="infos"></param>
        private static void CompareVersionsParams<TParam>(
            List<TParam> minParamsList, 
            List<TParam> maxParamsList,
            FrozenDictionary<string, ParamDesc> paramDescs,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            int? parentNewObjectId,
            List<ItemVersionComparisonInfo> infos)
                where TParam : VersionedParamBase
        {
            Func<TParam, string> getObjectName = p => p.ParamName;
            Func<TParam, string> getObjectDesc = p => paramDescs.GetValueOrDefault(p.ParamName)?.Desc ?? @"";
            //{
            //    string? desc = paramDescs.GetValueOrDefault(p.ParamName)?.Desc;
            //    return !String.IsNullOrEmpty(desc) ? desc : p.ParamName;
            //};
            Func<TParam, string> getObjectValue = p => p.Value;

            maxParamsList.Intersect(
                minParamsList,
                out List<TParam> maxUniqueCollection,
                out List<TParam> maxIntersectionCollection,
                out List<TParam> minUniqueCollection,
                out List<TParam> minIntersectionCollection,
                ParamNameEqualityComparer<TParam>.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                parentNewObjectId,
                getObjectName,
                getObjectDesc,
                getObjectValue,
                itemVersionComparisonArgs,
                infos);

            foreach (var intersectParamIndex in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                var intersectMinEntity = minIntersectionCollection[intersectParamIndex];
                var intersectMaxEntity = maxIntersectionCollection[intersectParamIndex];
                if (!ParamValueEqualityComparer<TParam>.Instance.Equals(intersectMinEntity, intersectMaxEntity))
                {
                    ProjectVersion? projectVersion = null;
                    if (intersectMaxEntity._CreateProjectVersionNum is not null)
                        projectVersion = itemVersionComparisonArgs.ProjectVersions[intersectMaxEntity._CreateProjectVersionNum.Value];
                    infos.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = intersectMaxEntity.GetType().Name,
                        OldObjectId = intersectMinEntity.Id,
                        NewObjectId = intersectMaxEntity.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                        ParentNewObjectId = parentNewObjectId,
                        ObjectName = getObjectName(intersectMaxEntity),
                        ObjectDesc = getObjectDesc(intersectMaxEntity),
                        OldValue = getObjectValue(intersectMinEntity),
                        NewValue = getObjectValue(intersectMaxEntity),
                        ChangeTimeUtc = intersectMaxEntity._CreateTimeUtc,
                        ChangedBy_User = intersectMaxEntity._CreateUser,
                        Change_Comment = projectVersion?.Comment ?? @"",
                        ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                        ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                    });
                }
            }
        }

        private static void CompareVersionsTagConditions(
            List<TagCondition> minParamsList,
            List<TagCondition> maxParamsList,            
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            int? parentNewObjectId,
            List<ItemVersionComparisonInfo> infos)
        {            
            Func<TagCondition, string> getObjectName = tc => tc.ToString();
            Func<TagCondition, string> getObjectDesc = tc => @"";
            Func<TagCondition, string> getObjectValue = tc => @"CanBeCause: " + tc.CanBeCause +
                "; CanBeEffect: " + tc.CanBeEffect;

            maxParamsList.Intersect(
                minParamsList,
                out List<TagCondition> maxUniqueCollection,
                out List<TagCondition> maxIntersectionCollection,
                out List<TagCondition> minUniqueCollection,
                out List<TagCondition> minIntersectionCollection,
                TagConditionEqualityComparer.Instance);

            AddDeletedAndAddedToResult(
                minUniqueCollection,
                maxUniqueCollection,
                parentNewObjectId,
                getObjectName,
                getObjectDesc,
                getObjectValue,
                itemVersionComparisonArgs,
                infos);

            foreach (var intersectParamIndex in Enumerable.Range(0, maxIntersectionCollection.Count))
            {
                var intersectMinEntity = minIntersectionCollection[intersectParamIndex];
                var intersectMaxEntity = maxIntersectionCollection[intersectParamIndex];
                if (!TagConditionValueEqualityComparer.Instance.Equals(intersectMinEntity, intersectMaxEntity))
                {
                    ProjectVersion? projectVersion = null;
                    if (intersectMaxEntity._CreateProjectVersionNum is not null)
                        projectVersion = itemVersionComparisonArgs.ProjectVersions[intersectMaxEntity._CreateProjectVersionNum.Value];
                    infos.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = intersectMaxEntity.GetType().Name,
                        OldObjectId = intersectMinEntity.Id,
                        NewObjectId = intersectMaxEntity.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                        ParentNewObjectId = parentNewObjectId,
                        ObjectName = getObjectName(intersectMaxEntity),
                        ObjectDesc = getObjectDesc(intersectMaxEntity),
                        OldValue = getObjectValue(intersectMinEntity),
                        NewValue = getObjectValue(intersectMaxEntity),
                        ChangeTimeUtc = intersectMaxEntity._CreateTimeUtc,
                        ChangedBy_User = intersectMaxEntity._CreateUser,
                        Change_Comment = projectVersion?.Comment ?? @"",
                        ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                        ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                    });
                }
            }
        }

        private static void CompareVersionsDbFileReferences<TDbFileReference>(
            List<TDbFileReference> minDbFileReferences, 
            List<TDbFileReference> maxDbFileReferences,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            int? parentNewObjectId,
            List<ItemVersionComparisonInfo> infos)
                where TDbFileReference : VersionDbFileReference
        {
            CompareVersionsEntities<TDbFileReference>(
                minDbFileReferences, 
                maxDbFileReferences,
                itemVersionComparisonArgs,
                DbFileReferenceFilePathEqualityComparer<TDbFileReference>.Instance,
                DbFileReferenceFileContentEqualityComparer<TDbFileReference>.Instance,
                parentNewObjectId,
                dbf => dbf.Name,
                dbf => @"",
                dbf => new Any(dbf.LastWriteTimeUtc.ToLocalTime()).ValueAsString(true, @"G"),
                infos);
        }

        private static async Task CompareVersionsEntitiesAsync<TVersionEntity>(
            IQueryable<TVersionEntity> minEntities, 
            IQueryable<TVersionEntity> maxEntities,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            IEqualityComparer<TVersionEntity> identifierEqualityComparer,
            IEqualityComparer<TVersionEntity>? valueEqualityComparer,
            int? parentNewObjectId,
            Func<TVersionEntity, string> getObjectName,
            Func<TVersionEntity, string> getObjectDesc,
            Func<TVersionEntity, string> getObjectValue,
            List<ItemVersionComparisonInfo> infos)
                where TVersionEntity : VersionedEntityBase
        {
            List<TVersionEntity> minEntitiesList;
            List<TVersionEntity> maxEntitiesList;
            if (itemVersionComparisonArgs.MaxProjectVersionNum is null)
            {
                minEntitiesList = await minEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                        (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) &&
                        t._IsDeleted == true)
                    .ToListAsync();

                maxEntitiesList = await maxEntities.Where(t => t._IsDeleted == false &&
                        (t._CreateProjectVersionNum == null || t._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum))
                    .ToListAsync();
            }
            else
            {
                minEntitiesList = await minEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                        t._DeleteProjectVersionNum != null && t._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum &&
                        t._DeleteProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value)
                    .ToListAsync();

                maxEntitiesList = await maxEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value &&
                        (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > itemVersionComparisonArgs.MaxProjectVersionNum.Value) &&
                        (t._CreateProjectVersionNum == null || t._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum))
                    .ToListAsync();
            }

            maxEntitiesList.Intersect(minEntitiesList,
                out List<TVersionEntity> maxUniqueCollection,
                out List<TVersionEntity> maxIntersectionCollection,
                out List<TVersionEntity> minUniqueCollection,
                out List<TVersionEntity> minIntersectionCollection,
                identifierEqualityComparer,
                valueEqualityComparer);

            AddDeletedAndAddedToResult(minUniqueCollection,
                maxUniqueCollection,
                parentNewObjectId,
                getObjectName,
                getObjectDesc,
                getObjectValue,
                itemVersionComparisonArgs,
                infos);

            if (valueEqualityComparer is not null)
                foreach (var intersectParamIndex in Enumerable.Range(0, maxIntersectionCollection.Count))
                {
                    var intersectMinEntity = minIntersectionCollection[intersectParamIndex];
                    var intersectMaxEntity = maxIntersectionCollection[intersectParamIndex];
                    if (!valueEqualityComparer.Equals(intersectMinEntity, intersectMaxEntity))
                    {
                        ProjectVersion? projectVersion = null;
                        if (intersectMaxEntity._CreateProjectVersionNum is not null)
                            projectVersion = itemVersionComparisonArgs.ProjectVersions[intersectMaxEntity._CreateProjectVersionNum.Value];
                        infos.Add(new ItemVersionComparisonInfo
                        {
                            ObjectType = intersectMaxEntity.GetType().Name,
                            OldObjectId = intersectMinEntity.Id,
                            NewObjectId = intersectMaxEntity.Id,
                            ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                            ParentNewObjectId = parentNewObjectId,
                            ObjectName = getObjectName(intersectMaxEntity),
                            ObjectDesc = getObjectDesc(intersectMaxEntity),
                            OldValue = getObjectValue(intersectMinEntity),
                            NewValue = getObjectValue(intersectMaxEntity),
                            ChangeTimeUtc = intersectMaxEntity._CreateTimeUtc,
                            ChangedBy_User = intersectMaxEntity._CreateUser,
                            Change_Comment = projectVersion?.Comment ?? @"",
                            ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                            ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                        });
                    }
                }
        }

        private static void CompareVersionsEntities<TVersionEntity>(
            List<TVersionEntity> minEntities, 
            List<TVersionEntity> maxEntities,
            ItemVersionComparisonArgs itemVersionComparisonArgs,
            IEqualityComparer<TVersionEntity> identifierEqualityComparer,
            IEqualityComparer<TVersionEntity>? valueEqualityComparer,
            int? parentNewObjectId,
            Func<TVersionEntity, string> getObjectName,
            Func<TVersionEntity, string> getObjectDesc,
            Func<TVersionEntity, string> getObjectValue,
            List<ItemVersionComparisonInfo> infos)
                where TVersionEntity : VersionedEntityBase
        {
            List<TVersionEntity> minEntitiesList;
            List<TVersionEntity> maxEntitiesList;
            if (itemVersionComparisonArgs.MaxProjectVersionNum is null)
            {
                minEntitiesList = minEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                    (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum) &&
                    t._IsDeleted == true)
                    .ToList();

                maxEntitiesList = maxEntities.Where(t => t._IsDeleted == false &&
                    (t._CreateProjectVersionNum == null || t._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum))
                    .ToList();
            }
            else
            {
                minEntitiesList = minEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MinProjectVersionNum &&
                    t._DeleteProjectVersionNum != null && t._DeleteProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum &&
                    t._DeleteProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value)
                    .ToList();

                maxEntitiesList = maxEntities.Where(t => t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= itemVersionComparisonArgs.MaxProjectVersionNum.Value &&
                    (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > itemVersionComparisonArgs.MaxProjectVersionNum.Value) &&
                    (t._CreateProjectVersionNum == null || t._CreateProjectVersionNum > itemVersionComparisonArgs.MinProjectVersionNum))
                    .ToList();
            }

            maxEntitiesList.Intersect(minEntitiesList,
                out List<TVersionEntity> maxUniqueCollection,
                out List<TVersionEntity> maxIntersectionCollection,
                out List<TVersionEntity> minUniqueCollection,
                out List<TVersionEntity> minIntersectionCollection,
                identifierEqualityComparer);

            AddDeletedAndAddedToResult(minUniqueCollection,
                maxUniqueCollection,
                parentNewObjectId,
                getObjectName,
                getObjectDesc,
                getObjectValue,
                itemVersionComparisonArgs,
                infos);

            if (valueEqualityComparer is not null)
                foreach (var intersectParamIndex in Enumerable.Range(0, maxIntersectionCollection.Count))
                {
                    var intersectMinEntity = minIntersectionCollection[intersectParamIndex];
                    var intersectMaxEntity = maxIntersectionCollection[intersectParamIndex];
                    if (!valueEqualityComparer.Equals(intersectMinEntity, intersectMaxEntity))
                    {
                        ProjectVersion? projectVersion = null;
                        if (intersectMaxEntity._CreateProjectVersionNum is not null)
                            projectVersion = itemVersionComparisonArgs.ProjectVersions[intersectMaxEntity._CreateProjectVersionNum.Value];
                        infos.Add(new ItemVersionComparisonInfo
                        {
                            ObjectType = intersectMaxEntity.GetType().Name,
                            OldObjectId = intersectMinEntity.Id,
                            NewObjectId = intersectMaxEntity.Id,
                            ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                            ParentNewObjectId = parentNewObjectId,
                            ObjectName = getObjectName(intersectMaxEntity),
                            ObjectDesc = getObjectDesc(intersectMaxEntity),
                            OldValue = getObjectValue(intersectMinEntity),
                            NewValue = getObjectValue(intersectMaxEntity),
                            ChangeTimeUtc = intersectMaxEntity._CreateTimeUtc,
                            ChangedBy_User = intersectMaxEntity._CreateUser,
                            Change_Comment = projectVersion?.Comment ?? @"",
                            ApproveTimeUtc = projectVersion?.Active_TimeUtc,
                            ApprovedBy_User = projectVersion?.Active_SupervisorUser ?? @""
                        });
                    }
                }
        }        

        private static async Task Sync_BasePcObjects_PcObjects_WithActiveProjectVersionAsync(
            string unitIdentifier,
            int projectId,
            UInt32 projectVersionNum,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            DbCache dbCache)
        {
            await using var readOnlyDbContext = dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            Common.Serialization.SerializationRootObject serializationRootObject = new();
            serializationRootObject.BasePcObjects = new List<Common.Serialization.BasePcObject>();
            serializationRootObject.PcObjects = new List<Common.Serialization.PcObject>();
            var projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(projectId, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);

            foreach (var kvp in projectAllParamValues.SafetyControllersParams)
            {
                if (kvp.Key.EndsWith(PazCheckConstants.IdentifierEnding_Template, StringComparison.InvariantCultureIgnoreCase))
                {
                    var serializationBasePcObject = new Common.Serialization.BasePcObject()
                    {
                        Identifier = kvp.Key,
                        Unit = unitIdentifier,
                        Params = kvp.Value.Select(p =>
                        {
                            var param_ = new Common.Serialization.Param();
                            param_.Name = p.ParamName;
                            param_.Value = p.Value;
                            return param_;
                        })
                        .ToList()
                    };
                    serializationRootObject.BasePcObjects.Add(serializationBasePcObject);
                }
                else
                {
                    var serializationPcObject = new Common.Serialization.PcObject()
                    {
                        Identifier = kvp.Key,
                        Unit = unitIdentifier,
                        Params = kvp.Value.Select(p =>
                        {
                            var param_ = new Common.Serialization.Param();
                            param_.Name = p.ParamName;
                            param_.Value = p.Value;
                            return param_;
                        })
                        .ToList()
                    };
                    serializationRootObject.PcObjects.Add(serializationPcObject);
                }
            }

            if (serializationRootObject.BasePcObjects.Any(bo => !PazCheckDbHelper.CheckBasePcObject(bo, dbCache)) ||
                serializationRootObject.PcObjects.Any(o => !PazCheckDbHelper.CheckPcObject(o, dbCache)))
            {
                await SerializationHelper.ImportSerializationRootObjectAsync(
                        serializationRootObject,
                        new Common.Serialization.ImportMetadata()
                        {
                            RootCollectionMode = Common.Serialization.CollectionMode.Replace,
                            ChildCollectionMode = Common.Serialization.CollectionMode.Replace,
                            DataCollectionMode = Common.Serialization.CollectionMode.Update,
                        },
                        dbContextFactory,
                        @"",
                        null,
                        CancellationToken.None,
                        NullJobProgress.Instance,
                        LoggersSet.Empty,
                        new Common.Serialization.ImportSerializationRootObjectResult(),
                        preview: false);
            }            
        }

        #endregion
    }
}



//private class TagConditionSymbolToDisplayEqualityComparer : IEqualityComparer<TagCondition>
//{
//    public static readonly TagConditionSymbolToDisplayEqualityComparer Instance = new();

//    public bool Equals(TagCondition? leftObj, TagCondition? rightObj)
//    {
//        return leftObj?.SymbolToDisplay == rightObj?.SymbolToDisplay &&
//            leftObj?.CanBeCause == rightObj?.CanBeCause &&
//            leftObj?.CanBeEffect == rightObj?.CanBeEffect;
//    }

//    public int GetHashCode(TagCondition obj)
//    {
//        return 0;
//    }
//}

//pcObjects = FilterByPcObjectEventParamValue(pcObjects, filter, Common.Properties.Resources.ParamDesc_EmergencyShutdownLevel, Common.PazCheckCentralServerConstants.ParamName_EmergencyShutdownLevel);

//var resultSource = filter.GetValuesList(Common.Properties.Resources.ResultSource).FirstOrDefault();
//if (resultSource is not null)
//{
//    pcObjects = pcObjects.Where(o => o.PcObjectEvents.Any(e =>
//        e.ParamsDictionary
//            .Any(kvp => String.Equals(kvp.Key, CentralServer.Common.PazCheckCentralServerConstants.ParamName_StrictResultSource, StringComparison.InvariantCultureIgnoreCase) &&
//                (kvp.Value?.Contains(resultSource, StringComparison.InvariantCultureIgnoreCase) ?? false)) ||
//        e.ParamsDictionary
//            .Any(kvp => String.Equals(kvp.Key, CentralServer.Common.PazCheckCentralServerConstants.ParamName_PossibleResultSources, StringComparison.InvariantCultureIgnoreCase) &&
//                (kvp.Value?.Contains(resultSource, StringComparison.InvariantCultureIgnoreCase) ?? false)))).ToList();
//}
//var resultComment = filter.GetValuesList(Common.Properties.Resources.ResultComment).FirstOrDefault();
//if (resultComment is not null)
//{
//    pcObjects = pcObjects.Where(po => po.PcObjectEvents.Any(poe =>
//        String.Equals(poe.PcObjectEventType.Type, CentralServer.Common.PazCheckCentralServerConstants.PcObjectEventType_EmergencyShutdown, StringComparison.InvariantCultureIgnoreCase) &&
//        poe.Desc.Contains(resultComment, StringComparison.InvariantCultureIgnoreCase))).ToList();
//}