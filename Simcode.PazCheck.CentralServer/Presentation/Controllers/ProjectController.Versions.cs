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

                    BaseActuator baseActuator = dbContext.BaseActuators.Single(ba => ba.Id == baseActuatorId);

                    List<ItemVersionComparisonInfo> result = new();

                    await CompareVersionsBaseActuatorAsync(dbContext, baseActuator, minProjectVersionNum, maxProjectVersionNum, result);                    

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

                    Tag tag = dbContext.Tags.Single(t => t.Id == tagId);

                    List<ItemVersionComparisonInfo> result = new();
                    
                    await CompareVersionsTagAsync(dbContext, tag, minProjectVersionNum, maxProjectVersionNum, result);                    

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

                    CeMatrix ceMatrix = dbContext.CeMatrices.Single(m => m.Id == ceMatrixId);

                    List<ItemVersionComparisonInfo> result = new();
                    
                    await CompareVersionsCeMatrixAsync(dbContext, ceMatrix, minProjectVersionNum, maxProjectVersionNum, result);

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

            CompareVersionsCollections(intersectBaseActuators, minBaseActuators, maxBaseActuators, IdEqualityComparer<BaseActuator>.Instance, 
                0,
                ba => ba.Code,
                ba => ba.Title,
                ba => @"",
                result);            

            foreach (var intersectBaseActuator in intersectBaseActuators)
            {
                await CompareVersionsBaseActuatorAsync(dbContext, intersectBaseActuator, minProjectVersionNum, maxProjectVersionNum, result);
            }            
        }

        private async Task CompareVersionsBaseActuatorAsync(PazCheckDbContext dbContext, BaseActuator baseActuator, uint minProjectVersionNum, uint? maxProjectVersionNum,
                        List<ItemVersionComparisonInfo> result)
        {
            List<ItemVersionComparisonInfo> resultBaseActuatorParams = new();
            var baseActuatorParamsList = await dbContext.BaseActuatorParams.Where(p => p.BaseActuator == baseActuator)
                .Include(p => p.ParamInfo).ToListAsync();
            CompareVersionsParams(baseActuatorParamsList, minProjectVersionNum, maxProjectVersionNum,
                1,
                resultBaseActuatorParams);

            List<ItemVersionComparisonInfo> resultBaseActuatorDbFileReferences = new();
            var baseActuatorDbFileReferencesList = dbContext.BaseActuatorDbFileReferences.Where(fr => fr.BaseActuator == baseActuator).ToList();
            CompareVersionsEntities(baseActuatorDbFileReferencesList, minProjectVersionNum, maxProjectVersionNum,
                    DbFileReferenceEqualityComparer<DbFileReference>.Instance,
                    1,
                    fr => fr.FileName,
                    fr => @"",
                    fr => @"",
                    resultBaseActuatorDbFileReferences);

            if (resultBaseActuatorParams.Count > 0 || resultBaseActuatorDbFileReferences.Count > 0)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(BaseActuator),
                    OldObjectId = baseActuator.Id,
                    NewObjectId = baseActuator.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    Level = 0,
                    ObjectName = baseActuator.Code,
                    ObjectDesc = baseActuator.Title
                });
                result.AddRange(resultBaseActuatorParams);
                result.AddRange(resultBaseActuatorDbFileReferences);
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

            CompareVersionsCollections(intersectTags, minTags, maxTags, TagNameEqualityComparer.Instance,
                0,
                t => t.TagName,
                t => t.Desc,
                t => @"",
                result);

            foreach (var intersectTag in intersectTags)
            {
                await CompareVersionsTagAsync(dbContext, intersectTag, minProjectVersionNum, maxProjectVersionNum, result);
            }
        }

        private async Task CompareVersionsTagAsync(PazCheckDbContext dbContext, Tag tag, uint minProjectVersionNum, uint? maxProjectVersionNum, List<ItemVersionComparisonInfo> result)
        {
            List<ItemVersionComparisonInfo> resultTagParams = new();
            var tagParamsList = await dbContext.TagParams.Where(p => p.Tag == tag)
                .Include(p => p.ParamInfo).ToListAsync();
            CompareVersionsParams(tagParamsList, minProjectVersionNum, maxProjectVersionNum,
                    1,
                    resultTagParams);

            List<ItemVersionComparisonInfo> resultActuatorParams = new();
            var actuatorParamsList = await dbContext.ActuatorParams.Where(p => p.Tag == tag)
                .Include(p => p.ParamInfo).ToListAsync();
            CompareVersionsParams(actuatorParamsList, minProjectVersionNum, maxProjectVersionNum,
                    1,
                    resultActuatorParams);

            List<ItemVersionComparisonInfo> resultTagConditions = new();
            var tagConditionsList = dbContext.TagConditions.Where(p => p.Tag == tag).Include(p => p.TagConditionInfo).ToList();
            CompareVersionsEntities(tagConditionsList, minProjectVersionNum, maxProjectVersionNum,
                    IdEqualityComparer<TagCondition>.Instance,
                    1,
                    tc => tc.Identifier,
                    tc => tc.TagConditionInfo?.Desc ?? @"",
                    tc => tc.Value,
                    resultTagConditions);

            if (resultTagParams.Count > 0 || resultActuatorParams.Count > 0 || resultTagConditions.Count > 0)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(Tag),
                    OldObjectId = tag.Id,
                    NewObjectId = tag.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    Level = 0,
                    ObjectName = tag.TagName,
                    ObjectDesc = tag.Desc,
                });
                result.AddRange(resultTagParams);
                result.AddRange(resultActuatorParams);
                result.AddRange(resultTagConditions);
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

            CompareVersionsCollections(intersectCeMatrices, minCeMatrices, maxCeMatrices, IdEqualityComparer<CeMatrix>.Instance,
                0,
                m => m.Title,
                m => m.Desc,
                m => @"",
                result);

            foreach (var intersectCeMatrix in intersectCeMatrices)
            {
                await CompareVersionsCeMatrixAsync(dbContext, intersectCeMatrix, minProjectVersionNum, maxProjectVersionNum, result);
            }
        }

        private async Task CompareVersionsCeMatrixAsync(PazCheckDbContext dbContext, CeMatrix ceMatrix, uint minProjectVersionNum, uint? maxProjectVersionNum, List<ItemVersionComparisonInfo> result)
        {
            List<ItemVersionComparisonInfo> resultCeMatrixParams = new();
            var ceMatrixParamsList = await dbContext.CeMatrixParams.Where(p => p.CeMatrix == ceMatrix)
                .Include(p => p.ParamInfo).ToListAsync();
            CompareVersionsParams(ceMatrixParamsList, minProjectVersionNum, maxProjectVersionNum,
                    1,
                    resultCeMatrixParams);

            List<ItemVersionComparisonInfo> resultCeMatrixDbFileReferences = new();
            var ceMatrixDbFileReferencesList = await dbContext.CeMatrixDbFileReferences.Where(fr => fr.CeMatrix == ceMatrix).ToListAsync();
            CompareVersionsEntities(ceMatrixDbFileReferencesList, minProjectVersionNum, maxProjectVersionNum,
                    DbFileReferenceEqualityComparer<DbFileReference>.Instance,
                    1,
                    fr => fr.FileName,
                    fr => @"",
                    fr => @"",
                    resultCeMatrixDbFileReferences);

            List<ItemVersionComparisonInfo> resultCauses = new();
            var intersectCeMatrix_Causes = await dbContext.Causes.Where(c => c.CeMatrix == ceMatrix)
                .Include(c => c.SubCauses).ToListAsync();
            CompareVersionsCauses(dbContext, intersectCeMatrix_Causes, minProjectVersionNum, maxProjectVersionNum,
                     resultCauses);

            List<ItemVersionComparisonInfo> resultEffects = new();
            var effectsList = await dbContext.Effects.Where(fr => fr.CeMatrix == ceMatrix).ToListAsync();
            CompareVersionsEntities(effectsList, minProjectVersionNum, maxProjectVersionNum,
                    IdEqualityComparer<Effect>.Instance,
                    1,
                    e => e.GetFullConditionStringToDiplay(),
                    e => @"",
                    e => @"",
                    resultEffects);

            List<ItemVersionComparisonInfo> resultIntersections = new();
            // Effects and Causes already loaded.
            var intersectionsList = await dbContext.Intersections.Where(fr => fr.CeMatrix == ceMatrix).ToListAsync();
            CompareVersionsEntities(intersectionsList, minProjectVersionNum, maxProjectVersionNum,
                    IdEqualityComparer<Intersection>.Instance,
                    1,
                    i => "Причина: " + i.Cause.Num + @"; Следствие: " + i.Effect.GetFullConditionStringToDiplay(),
                    i => @"",
                    i => @"",
                    resultIntersections);

            if (resultCeMatrixParams.Count > 0 || resultCeMatrixDbFileReferences.Count > 0 || resultCauses.Count > 0 ||
                resultEffects.Count > 0 || resultIntersections.Count > 0)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = nameof(CeMatrix),
                    OldObjectId = ceMatrix.Id,
                    NewObjectId = ceMatrix.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                    Level = 0,
                    ObjectName = ceMatrix.Title,
                    ObjectDesc = ceMatrix.Desc,
                });
                result.AddRange(resultCeMatrixParams);
                result.AddRange(resultCeMatrixDbFileReferences);
                result.AddRange(resultCauses);
                result.AddRange(resultEffects);
                result.AddRange(resultIntersections);
            }
        }

        private void CompareVersionsCollections<TVersionEntity>(TVersionEntity[] minMaxCollection, TVersionEntity[] minCollection, TVersionEntity[] maxCollection,
            IEqualityComparer<TVersionEntity> equalityComparer,
            uint level,
            Func<TVersionEntity, string> getObjectName,
            Func<TVersionEntity, string> getObjectDesc,
            Func<TVersionEntity, string> getObjectValue,
            List<ItemVersionComparisonInfo> result)
            where TVersionEntity : VersionEntityBase
        {
            var deletedCollection = minCollection.Except(minMaxCollection, equalityComparer);
            foreach (var deleted in deletedCollection)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = deleted.GetType().Name,
                    OldObjectId = deleted.Id,
                    NewObjectId = null,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Deleted,
                    Level = level,
                    ObjectName = getObjectName(deleted),
                    ObjectDesc = getObjectDesc(deleted),
                    OldValue = getObjectValue(deleted),
                });                
            }
            var addedCollection = maxCollection.Except(minMaxCollection, equalityComparer);
            foreach (var added in addedCollection)
            {
                result.Add(new ItemVersionComparisonInfo
                {
                    ObjectType = added.GetType().Name,
                    OldObjectId = null,
                    NewObjectId = added.Id,
                    ChangeType = ItemVersionComparisonInfo.ChangeType_Added,
                    Level = level,
                    ObjectName = getObjectName(added),
                    ObjectDesc = getObjectDesc(added),
                    NewValue = getObjectValue(added),
                });                
            }
        }

        /// <summary>
        ///     !!! ParamInfo must be loaded !!!
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="paramsList"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="level"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private void CompareVersionsParams<TParam>(List<TParam> paramsList, uint minProjectVersionNum, uint? maxProjectVersionNum, 
            uint level,
            List<ItemVersionComparisonInfo> result)
            where TParam: Param 
        {
            var minParams = paramsList.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            TParam[] maxParams;
            if (maxProjectVersionNum is not null)
                maxParams = paramsList.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxParams = paramsList.Where(p => !p._IsDeleted).ToArray();

            var intersectMaxParams = maxParams.Intersect(minParams, ParamNameEqualityComparer<TParam>.Instance).ToArray();
            CompareVersionsCollections(intersectMaxParams, minParams, maxParams, ParamNameEqualityComparer<TParam>.Instance,
                level,
                p => p.ParamName,
                p => p.ParamInfo?.Desc ?? @"",
                p => p.Value + @" " + p.Eu,
                result);

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
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                        Level = level,
                        ObjectName = intersectMaxParam.ParamName,
                        ObjectDesc = intersectMaxParam.ParamInfo?.Desc ?? @"",
                        OldValue = intersectMinParam.Value + @" " + intersectMinParam.Eu,
                        NewValue = intersectMaxParam.Value + @" " + intersectMaxParam.Eu,
                    });                    
                }
            }
        }

        /// <summary>
        ///     !!! SubCauses must be loaded !!!
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="causesList"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private void CompareVersionsCauses(PazCheckDbContext dbContext, List<Cause> causesList, uint minProjectVersionNum, uint? maxProjectVersionNum,            
            List<ItemVersionComparisonInfo> result)            
        {
            var minCauses = causesList.Where(c => (c._CreateProjectVersionNum != null && c._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (c._DeleteProjectVersionNum == null || c._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            Cause[] maxCauses;
            if (maxProjectVersionNum is not null)
                maxCauses = causesList.Where(c => (c._CreateProjectVersionNum != null && c._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (c._DeleteProjectVersionNum == null || c._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxCauses = causesList.Where(c => !c._IsDeleted).ToArray();

            var intersectMaxCauses = maxCauses.Intersect(minCauses, IdEqualityComparer<Cause>.Instance).ToArray();
            CompareVersionsCollections(intersectMaxCauses, minCauses, maxCauses, IdEqualityComparer<Cause>.Instance,
                1,
                c => c.Num.ToString(),
                c => @"",
                c => @"",
                result);

            foreach (var intersectMaxCause in intersectMaxCauses)
            {
                List<ItemVersionComparisonInfo> resultSubCauses = new();
                CompareVersionsEntities(intersectMaxCause.SubCauses, minProjectVersionNum, maxProjectVersionNum,
                        IdEqualityComparer<SubCause>.Instance,
                        2,
                        sc => sc.GetFullConditionStringToDiplay(),
                        sc => @"",
                        sc => @"",
                        resultSubCauses);

                if (resultSubCauses.Count > 0)
                {
                    result.Add(new ItemVersionComparisonInfo
                    {
                        ObjectType = nameof(Cause),
                        OldObjectId = intersectMaxCause.Id,
                        NewObjectId = intersectMaxCause.Id,
                        ChangeType = ItemVersionComparisonInfo.ChangeType_Modified,
                        Level = 1,
                        ObjectName = intersectMaxCause.Num.ToString()
                    });
                    result.AddRange(resultSubCauses);                    
                }
            }
        }

        private void CompareVersionsEntities<TVersionEntity>(List<TVersionEntity> entitiesList, uint minProjectVersionNum, uint? maxProjectVersionNum,
            IEqualityComparer<TVersionEntity> equalityComparer,
            uint level,
            Func<TVersionEntity, string> getObjectName,
            Func<TVersionEntity, string> getObjectDesc,
            Func<TVersionEntity, string> getObjectValue,
            List<ItemVersionComparisonInfo> result)
            where TVersionEntity : VersionEntityBase
        {
            var minEntities = entitiesList.Where(e => (e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= minProjectVersionNum) &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > minProjectVersionNum)).ToArray();

            TVersionEntity[] maxEntities;
            if (maxProjectVersionNum is not null)
                maxEntities = entitiesList.Where(e => (e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= maxProjectVersionNum.Value) &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > maxProjectVersionNum.Value)).ToArray();
            else
                maxEntities = entitiesList.Where(e => !e._IsDeleted).ToArray();

            var intersectMaxEntities = maxEntities.Intersect(minEntities, equalityComparer).ToArray();
            CompareVersionsCollections(intersectMaxEntities, minEntities, maxEntities, equalityComparer,
                level,
                getObjectName,
                getObjectDesc,
                getObjectValue,
                result);
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
