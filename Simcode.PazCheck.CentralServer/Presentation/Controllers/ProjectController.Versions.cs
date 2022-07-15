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

        /// <summary>
        ///     Сравнивает 2 версии проекта. Если maxProjectVersionNum не задан, то сравнивается с текущими несхораненными изменениями.
        ///     Если minProjectVersionNum задать 0, то сравнивается с пустой базой.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <returns></returns>
        [HttpGet(@"CompareVersions/{projectId}")]
        public async Task<IActionResult> CompareVersionsAsync(int projectId, uint minProjectVersionNum, uint? maxProjectVersionNum)
        {           
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    //ProjectVersion minProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == minVersionNum);
                    //ProjectVersion maxProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == maxVersionNum);

                    List<ItemVersionComparisonInfo> result = new();

                    await CompareVersionsBaseActuatorsAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum, result);
                    await CompareVersionsTagsAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum, result);
                    await CompareVersionsCeMatricesAsync(dbContext, project, minProjectVersionNum, maxProjectVersionNum, result);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid projectId: {0}", projectId);

                return NotFound();
            }            
        }

        /// <summary>
        ///     Сравнивает 2 версии типа исполнительных механизмов. Если maxProjectVersionNum не задан, то сравнивается с текущими несхораненными изменениями.
        ///     Если minProjectVersionNum задать 0, то сравнивается с пустой базой.
        /// </summary>
        /// <param name="baseActuatorId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <returns></returns>
        [HttpGet(@"CompareVersions_BaseActuator/{baseActuatorId}")]
        public async Task<IActionResult> CompareVersions_BaseActuatorAsync(int baseActuatorId, uint minProjectVersionNum, uint? maxProjectVersionNum)
        {
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    //Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    //ProjectVersion minProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == minVersionNum);
                    //ProjectVersion maxProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == maxVersionNum);

                    List<ItemVersionComparisonInfo> result = new();

                    await Task.Delay(0);

                    //await CompareVersionsBaseActuatorsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsTagsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsCeMatricesAsync(dbContext, project, projectVersionNum, null, result);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid baseActuatorId: {0}", baseActuatorId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии тэга. Если maxProjectVersionNum не задан, то сравнивается с текущими несхораненными изменениями.
        ///     Если minProjectVersionNum задать 0, то сравнивается с пустой базой.
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <returns></returns>
        [HttpGet(@"CompareVersions_Tag/{tagId}")]
        public async Task<IActionResult> CompareVersions_TagAsync(int tagId, uint minProjectVersionNum, uint? maxProjectVersionNum)
        {            
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    //Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    //ProjectVersion minProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == minVersionNum);
                    //ProjectVersion maxProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == maxVersionNum);

                    List<ItemVersionComparisonInfo> result = new();

                    await Task.Delay(0);

                    //await CompareVersionsBaseActuatorsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsTagsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsCeMatricesAsync(dbContext, project, projectVersionNum, null, result);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid tagId: {0}", tagId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Сравнивает 2 версии матрицы ПСС. Если maxProjectVersionNum не задан, то сравнивается с текущими несхораненными изменениями.
        ///     Если minProjectVersionNum задать 0, то сравнивается с пустой базой.
        /// </summary>
        /// <param name="ceMatrixId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <returns></returns>
        [HttpGet(@"CompareVersions_CeMatrix/{ceMatrixId}")]
        public async Task<IActionResult> CompareVersions_CeMatrixAsync(int ceMatrixId, uint minProjectVersionNum, uint? maxProjectVersionNum)
        {
            try
            {
                using (var dbContext = new PazCheckDbContext())
                {
                    //Project project = dbContext.Projects.Single(p => p.Id == projectId);
                    //ProjectVersion minProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == minVersionNum);
                    //ProjectVersion maxProjectVersion = dbContext.ProjectVersions.Single(pv => pv.Project == project && pv.VersionNum == maxVersionNum);

                    List<ItemVersionComparisonInfo> result = new();

                    await Task.Delay(0);
                    //await CompareVersionsBaseActuatorsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsTagsAsync(dbContext, project, projectVersionNum, null, result);
                    //await CompareVersionsCeMatricesAsync(dbContext, project, projectVersionNum, null, result);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid ceMatrixId: {0}", ceMatrixId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что то менялось в этом элементе.
        /// </summary>
        /// <param name="baseActuatorId"></param>
        /// <returns></returns>
        [HttpGet(@"GetVersions_BaseActuator/{baseActuatorId}")]
        public async Task<IActionResult> GetVersions_BaseActuator(int baseActuatorId)
        {
            try
            {
                HashSet<uint> projectVersionNums = new();

                using (var dbContext = new PazCheckDbContext())
                {
                    BaseActuator baseActuator = await dbContext.BaseActuators.Where(ba => ba.Id == baseActuatorId)
                        .Include(ba => ba.BaseActuatorParams)
                        .Include(ba => ba.BaseActuatorDbFileReferences)
                        .FirstAsync();
                    if (baseActuator._CreateProjectVersionNum is not null)
                    {
                        GetVersions(baseActuator, projectVersionNums);
                        
                        GetVersions(baseActuator.BaseActuatorParams, projectVersionNums);
                        
                        GetVersions(baseActuator.BaseActuatorDbFileReferences, projectVersionNums);
                    }                    

                    return Ok(projectVersionNums.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid baseActuatorId: {0}", baseActuatorId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что то менялось в этом элементе.
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpGet(@"GetVersions_Tag/{tagId}")]
        public async Task<IActionResult> GetVersions_Tag(int tagId)
        {
            try
            {
                HashSet<uint> projectVersionNums = new();

                using (var dbContext = new PazCheckDbContext())
                {
                    Tag tag = await dbContext.Tags.Where(t => t.Id == tagId)
                        .Include(ba => ba.TagParams)
                        .Include(ba => ba.ActuatorParams)
                        .Include(ba => ba.TagConditions)
                        .FirstAsync();
                    if (tag._CreateProjectVersionNum is not null)
                    {
                        GetVersions(tag, projectVersionNums);
                        
                        GetVersions(tag.TagParams, projectVersionNums);
                        
                        GetVersions(tag.ActuatorParams, projectVersionNums);
                        
                        GetVersions(tag.TagConditions, projectVersionNums);
                    }

                    return Ok(projectVersionNums.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid baseActuatorId: {0}", tagId);

                return NotFound();
            }
        }

        /// <summary>
        ///     Получает массив номеров версий проекта, в которых что то менялось в этом элементе.
        /// </summary>
        /// <param name="ceMatrixId"></param>
        /// <returns></returns>
        [HttpGet(@"GetVersions_CeMatrix/{ceMatrixId}")]
        public async Task<IActionResult> GetVersions_CeMatrix(int ceMatrixId)
        {
            try
            {
                HashSet<uint> projectVersionNums = new();

                using (var dbContext = new PazCheckDbContext())
                {
                    CeMatrix ceMatrix = await dbContext.CeMatrices.Where(ba => ba.Id == ceMatrixId)
                        .Include(m => m.CeMatrixParams)
                        .Include(m => m.CeMatrixDbFileReferences)                        
                        .Include(m => m.Causes)
                        .ThenInclude(c => c.SubCauses)
                        .Include(m => m.Effects)
                        .Include(m => m.Intersections)
                        .FirstAsync();
                    if (ceMatrix._CreateProjectVersionNum is not null)
                    {
                        GetVersions(ceMatrix, projectVersionNums);
                        
                        GetVersions(ceMatrix.CeMatrixParams, projectVersionNums);
                        
                        GetVersions(ceMatrix.CeMatrixDbFileReferences, projectVersionNums);
                        
                        foreach (var cause in ceMatrix.Causes)
                        {
                            GetVersions(cause, projectVersionNums);
                            GetVersions(cause.SubCauses, projectVersionNums);
                        }
                        
                        GetVersions(ceMatrix.Effects, projectVersionNums);
                        
                        GetVersions(ceMatrix.Intersections, projectVersionNums);
                    }

                    return Ok(projectVersionNums.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Invalid ceMatrixId: {0}", ceMatrixId);

                return NotFound();
            }
        }

        #endregion

        #region private functions

        private async Task CompareVersionsBaseActuatorsAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint? maxProjectVersionNum,
                        List<ItemVersionComparisonInfo> result)
        {
            var minBaseActuators = await dbContext.BaseActuators.Where(ba => ba.Project == project && (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > minProjectVersionNum)).ToArrayAsync();

            BaseActuator[]? maxBaseActuators;
            if (maxProjectVersionNum is not null)
                maxBaseActuators = await dbContext.BaseActuators.Where(ba => ba.Project == project && (ba._CreateProjectVersionNum != null && ba._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (ba._DeleteProjectVersionNum == null || ba._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArrayAsync();
            else
                maxBaseActuators = await dbContext.BaseActuators.Where(ba => ba.Project == project && !ba._IsDeleted).ToArrayAsync();

            var intersectBaseActuators = maxBaseActuators.Intersect(minBaseActuators, IdEqualityComparer<BaseActuator>.Instance).ToArray();

            CompareVersionsCollections(intersectBaseActuators, minBaseActuators, maxBaseActuators, IdEqualityComparer<BaseActuator>.Instance, result);            

            foreach (var intersectBaseActuator in intersectBaseActuators)
            {
                bool hasChanges = false;

                dbContext.Entry(intersectBaseActuator).Collection(ba => ba.BaseActuatorParams).Load();                
                if (CompareVersionsParams(intersectBaseActuator.BaseActuatorParams, minProjectVersionNum, maxProjectVersionNum, result))
                    hasChanges = true;

                dbContext.Entry(intersectBaseActuator).Collection(ba => ba.BaseActuatorDbFileReferences).Load();
                if (CompareVersionsEntities(intersectBaseActuator.BaseActuatorDbFileReferences, minProjectVersionNum, maxProjectVersionNum,
                        DbFileReferenceEqualityComparer<DbFileReference>.Instance,
                        result))
                    hasChanges = true;

                if (hasChanges)
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = nameof(BaseActuator),
                        OldObjectId = intersectBaseActuator.Id,
                        NewObjectId = intersectBaseActuator.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified
                    });
                }                   
            }            
        }        

        private async Task CompareVersionsTagsAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint? maxProjectVersionNum, List<ItemVersionComparisonInfo> result)
        {
            var minTags = await dbContext.Tags.Where(t => t.Project == project && (t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > minProjectVersionNum)).ToArrayAsync();

            Tag[] maxTags;
            if (maxProjectVersionNum is not null)
                maxTags = await dbContext.Tags.Where(t => t.Project == project && (t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArrayAsync();
            else
                maxTags = await dbContext.Tags.Where(t => t.Project == project && !t._IsDeleted).ToArrayAsync();

            var intersectTags = maxTags.Intersect(minTags, TagNameEqualityComparer.Instance).ToArray();

            CompareVersionsCollections(intersectTags, minTags, maxTags, TagNameEqualityComparer.Instance, result);

            foreach (var intersectTag in intersectTags)
            {
                bool hasChanges = false;

                dbContext.Entry(intersectTag).Collection(ba => ba.TagParams).Load();
                if (CompareVersionsParams(intersectTag.TagParams, minProjectVersionNum, maxProjectVersionNum,                        
                        result))
                    hasChanges = true;

                dbContext.Entry(intersectTag).Collection(ba => ba.ActuatorParams).Load();
                if (CompareVersionsParams(intersectTag.ActuatorParams, minProjectVersionNum, maxProjectVersionNum,                        
                        result))
                    hasChanges = true;

                dbContext.Entry(intersectTag).Collection(ba => ba.TagConditions).Load();
                if (CompareVersionsEntities(intersectTag.TagConditions, minProjectVersionNum, maxProjectVersionNum,
                        IdEqualityComparer<TagCondition>.Instance,
                        result))
                    hasChanges = true;

                if (hasChanges)
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = nameof(Tag),
                        OldObjectId = intersectTag.Id,
                        NewObjectId = intersectTag.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified
                    });
                }
            }
        }

        private async Task CompareVersionsCeMatricesAsync(PazCheckDbContext dbContext, Project project, uint minProjectVersionNum, uint? maxProjectVersionNum, List<ItemVersionComparisonInfo> result)
        {
            var minCeMatrices = await dbContext.CeMatrices.Where(m => m.Project == project && (m._CreateProjectVersionNum != null && m._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (m._DeleteProjectVersionNum == null || m._DeleteProjectVersionNum > minProjectVersionNum)).ToArrayAsync();

            CeMatrix[] maxCeMatrices;
            if (maxProjectVersionNum is not null)
                maxCeMatrices = await dbContext.CeMatrices.Where(m => m.Project == project && (m._CreateProjectVersionNum != null && m._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (m._DeleteProjectVersionNum == null || m._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArrayAsync();
            else
                maxCeMatrices = await dbContext.CeMatrices.Where(m => m.Project == project && !m._IsDeleted).ToArrayAsync();

            var intersectCeMatrices = maxCeMatrices.Intersect(minCeMatrices, IdEqualityComparer<CeMatrix>.Instance).ToArray();

            CompareVersionsCollections(intersectCeMatrices, minCeMatrices, maxCeMatrices, IdEqualityComparer<CeMatrix>.Instance, result);

            foreach (var intersectCeMatrix in intersectCeMatrices)
            {
                bool hasChanges = false;

                dbContext.Entry(intersectCeMatrix).Collection(ba => ba.CeMatrixParams).Load();
                if (CompareVersionsParams(intersectCeMatrix.CeMatrixParams, minProjectVersionNum, maxProjectVersionNum,                        
                        result))
                    hasChanges = true;

                dbContext.Entry(intersectCeMatrix).Collection(ba => ba.CeMatrixDbFileReferences).Load();
                if (CompareVersionsEntities(intersectCeMatrix.CeMatrixDbFileReferences, minProjectVersionNum, maxProjectVersionNum,
                        DbFileReferenceEqualityComparer<DbFileReference>.Instance,
                        result))
                    hasChanges = true;

                var intersectCeMatrix_Causes = dbContext.Causes.Where(c => c.CeMatrix == intersectCeMatrix).Include(c => c.SubCauses).ToList();                
                if (CompareVersionsCauses(dbContext, intersectCeMatrix_Causes, minProjectVersionNum, maxProjectVersionNum,                        
                        result))
                    hasChanges = true;

                dbContext.Entry(intersectCeMatrix).Collection(ba => ba.Effects).Load();
                if (CompareVersionsEntities(intersectCeMatrix.Effects, minProjectVersionNum, maxProjectVersionNum,
                        IdEqualityComparer<Effect>.Instance,
                        result))
                    hasChanges = true;

                dbContext.Entry(intersectCeMatrix).Collection(ba => ba.Intersections).Load();
                if (CompareVersionsEntities(intersectCeMatrix.Intersections, minProjectVersionNum, maxProjectVersionNum,
                        IdEqualityComparer<Intersection>.Instance,
                        result))
                    hasChanges = true;

                if (hasChanges)
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = nameof(CeMatrix),
                        OldObjectId = intersectCeMatrix.Id,
                        NewObjectId = intersectCeMatrix.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified
                    });
                }
            }
        }

        /// <summary>
        ///     Returns whether has changes.
        /// </summary>
        /// <typeparam name="TVersionEntity"></typeparam>
        /// <param name="minMaxCollection"></param>
        /// <param name="minCollection"></param>
        /// <param name="maxCollection"></param>
        /// <param name="equalityComparer"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool CompareVersionsCollections<TVersionEntity>(TVersionEntity[] minMaxCollection, TVersionEntity[] minCollection, TVersionEntity[] maxCollection,
            IEqualityComparer<TVersionEntity> equalityComparer,
            List<ItemVersionComparisonInfo> result)
            where TVersionEntity : VersionEntityBase
        {
            bool hasChanges = false;

            var deletedCollection = minCollection.Except(minMaxCollection, equalityComparer);
            foreach (var deleted in deletedCollection)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = deleted.GetType().Name,
                    OldObjectId = deleted.Id,
                    NewObjectId = null,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Deleted
                });
                hasChanges = true;
            }
            var addedCollection = maxCollection.Except(minMaxCollection, equalityComparer);
            foreach (var added in addedCollection)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = added.GetType().Name,
                    OldObjectId = null,
                    NewObjectId = added.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Added
                });
                hasChanges = true;
            }

            return hasChanges;
        }

        /// <summary>
        ///     Returns whether has changes.
        /// </summary>
        /// <typeparam name="TParam"></typeparam>        
        /// <param name="paramsList"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool CompareVersionsParams<TParam>(List<TParam> paramsList, uint minProjectVersionNum, uint? maxProjectVersionNum, List<ItemVersionComparisonInfo> result)
            where TParam: Param 
        {
            bool hasChanges = false;

            var minParams = paramsList.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            TParam[] maxParams;
            if (maxProjectVersionNum is not null)
                maxParams = paramsList.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxParams = paramsList.Where(p => !p._IsDeleted).ToArray();

            var intersectMaxParams = maxParams.Intersect(minParams, ParamNameEqualityComparer<TParam>.Instance).ToArray();
            if (CompareVersionsCollections(intersectMaxParams, minParams, maxParams, ParamNameEqualityComparer<TParam>.Instance, result))
                hasChanges = true;

            foreach (var intersectMaxParam in intersectMaxParams)
            {
                var intersectMinParam = minParams.First(p => p.ParamName == intersectMaxParam.ParamName);
                if (intersectMaxParam.Value != intersectMinParam.Value || intersectMaxParam.Eu != intersectMinParam.Eu)
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = intersectMaxParam.GetType().Name,
                        OldObjectId = intersectMinParam.Id,
                        NewObjectId = intersectMaxParam.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified
                    });
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        private bool CompareVersionsCauses(PazCheckDbContext dbContext, List<Cause> causesList, uint minProjectVersionNum, uint? maxProjectVersionNum,            
            List<ItemVersionComparisonInfo> result)            
        {
            bool hasChanges = false;

            var minCauses = causesList.Where(c => (c._CreateProjectVersionNum != null && c._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (c._DeleteProjectVersionNum == null || c._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            Cause[] maxCauses;
            if (maxProjectVersionNum is not null)
                maxCauses = causesList.Where(c => (c._CreateProjectVersionNum != null && c._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (c._DeleteProjectVersionNum == null || c._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxCauses = causesList.Where(c => !c._IsDeleted).ToArray();

            var intersectMaxCauses = maxCauses.Intersect(minCauses, IdEqualityComparer<Cause>.Instance).ToArray();
            if (CompareVersionsCollections(intersectMaxCauses, minCauses, maxCauses, IdEqualityComparer<Cause>.Instance, result))
                hasChanges = true;

            foreach (var intersectMaxCause in intersectMaxCauses)
            {                
                dbContext.Entry(intersectMaxCause).Collection(c => c.SubCauses).Load();
                if (CompareVersionsEntities(intersectMaxCause.SubCauses, minProjectVersionNum, maxProjectVersionNum,
                        IdEqualityComparer<SubCause>.Instance,
                        result))                    
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = nameof(Cause),
                        OldObjectId = intersectMaxCause.Id,
                        NewObjectId = intersectMaxCause.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified
                    });
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        private bool CompareVersionsEntities<TEntity>(List<TEntity> entitiesList, uint minProjectVersionNum, uint? maxProjectVersionNum,
            IEqualityComparer<TEntity> equalityComparer,
            List<ItemVersionComparisonInfo> result)
            where TEntity : VersionEntityBase
        {
            bool hasChanges = false;

            var minEntities = entitiesList.Where(e => (e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            TEntity[] maxEntities;
            if (maxProjectVersionNum is not null)
                maxEntities = entitiesList.Where(e => (e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxEntities = entitiesList.Where(e => !e._IsDeleted).ToArray();

            var intersectMaxEntities = maxEntities.Intersect(minEntities, equalityComparer).ToArray();
            if (CompareVersionsCollections(intersectMaxEntities, minEntities, maxEntities, equalityComparer, result))
                hasChanges = true;

            return hasChanges;
        }

        private void GetVersions<TEntity>(List<TEntity> entitiesList, HashSet<uint> projectVersionNums)
            where TEntity : VersionEntityBase
        {
            foreach (var entity in entitiesList)
                GetVersions(entity, projectVersionNums);
        }

        private void GetVersions<TEntity>(TEntity entity, HashSet<uint> projectVersionNums)
            where TEntity : VersionEntityBase
        {
            if (entity._CreateProjectVersionNum is not null)
            {
                uint minProjectVersionNum = entity._CreateProjectVersionNum.Value;
                projectVersionNums.Add(minProjectVersionNum);
                uint? maxProjectVersionNum = entity._DeleteProjectVersionNum;
                if (maxProjectVersionNum is not null)
                    projectVersionNums.Add(maxProjectVersionNum.Value);
            }
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

        private class DbFileReferenceEqualityComparer<TDbFileReference> : IEqualityComparer<TDbFileReference>
            where TDbFileReference : DbFileReference
        {
            public static readonly DbFileReferenceEqualityComparer<TDbFileReference> Instance = new();

            public bool Equals(TDbFileReference? leftObj, TDbFileReference? rightObj)
            {
                return leftObj?.DbFileId == rightObj?.DbFileId;
            }

            public int GetHashCode(TDbFileReference obj)
            {
                return 0;
            }
        }

        private class TagNameEqualityComparer : IEqualityComparer<Tag>
        {
            public static readonly TagNameEqualityComparer Instance = new();

            public bool Equals(Tag? leftObj, Tag? rightObj)
            {
                return leftObj?.TagName == rightObj?.TagName;
            }

            public int GetHashCode(Tag obj)
            {
                return 0;
            }
        }

        public class ItemVersionComparisonInfo
        {
            /// <summary>
            ///     The entity is being tracked by the context and exists in the database. It has
            ///     been marked for deletion from the database.
            /// </summary>
            public const string ChangeType_Deleted = @"Deleted";

            /// <summary>
            ///    The entity is being tracked by the context and exists in the database. Some or
            ///    all of its property values have been modified.
            /// </summary>
            public const string ChangeType_Modified = @"Modified";

            /// <summary>
            ///     The entity is being tracked by the context but does not yet exist in the database.
            /// </summary>
            public const string ChangeType_Added = @"Added";

            public string ObjectType { get; set; } = @"";

            public int? OldObjectId { get; set; }

            public int? NewObjectId { get; set; }

            public string ChangeType { get; set; } = @"";

            /// <summary>
            ///     0,1,2..
            /// </summary>
            public uint Level { get; set; }

            public string ObjectName { get; set; } = @"";

            public string ObjectDesc { get; set; } = @"";

            public string OldValue { get; set; } = @"";

            public string NewValue { get; set; } = @"";
        }
    }
}
