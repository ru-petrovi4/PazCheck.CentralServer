using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Frozen;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using System.Text.Json;
using Ssz.Utils.ClosedXML;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Internal;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions      

        public static T GetParamValue<T>(
                IReadOnlyDictionary<string, string?>? paramsDictionary,
                IReadOnlyDictionary<string, string?>? baseParamsDictionary,
                string paramName,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(
                paramsDictionary,
                baseParamsDictionary,
                paramName);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        public static T GetCeMatrixParamValue<T>(
                string identifierAndParam,
                ProjectAllParamValues projectAllParamValues,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(identifierAndParam,
                                        projectAllParamValues.CeMatricesParamValues,
                                        PazCheckConstants.ParamName_CeMatrixTemplate,
                                        PazCheckConstants.TypeIdentifier_CeMatrix);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        public static T GetTagParamValue<T>(
                string identifierAndParam,
                ProjectAllParamValues projectAllParamValues,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(identifierAndParam,
                                        projectAllParamValues.TagsParamValues,
                                        PazCheckConstants.ParamName_TagTemplate,
                                        PazCheckConstants.TypeIdentifier_Tag);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        public static T GetActuatorParamValue<T>(
                string identifierAndParam,
                ProjectAllParamValues projectAllParamValues,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(identifierAndParam,
                                        projectAllParamValues.BaseActuatorsParamValues,
                                        PazCheckConstants.ParamName_ActuatorTemplate,
                                        PazCheckConstants.TypeIdentifier_Actuator);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        public static T GetMonitoringObjectParamValue<T>(
                string identifierAndParam,
                ProjectAllParamValues projectAllParamValues,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(identifierAndParam,
                                        projectAllParamValues.SafetyControllersParamValues,
                                        PazCheckConstants.ParamName_PcObjectTemplate,
                                        PazCheckConstants.TypeIdentifier_PcObject);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        public static T GetLegendParamValue<T>(
                string identifierAndParam,
                ProjectAllParamValues projectAllParamValues,
                T defaultValue)
            where T : notnull
        {
            string valueString = GetParamValue(identifierAndParam,
                                        projectAllParamValues.LegendsParamValues,
                                        PazCheckConstants.ParamName_LegendTemplate,
                                        PazCheckConstants.TypeIdentifier_Legend);
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        /// <summary>
        ///     Calls dbContext.SaveChanges()
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbCache"></param>
        /// <param name="projectId"></param>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="entityType"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task VersionedEntities_SetIsDeletedAsync(PazCheckDbContext dbContext, DbCache dbCache, int projectId, EntitiesCollectionInfo entitiesCollectionInfo, Type entityType, string user)
        {
            if (entityType == typeof(CeMatrix))
                foreach (CeMatrix ceMatrix in dbContext.CeMatrices.Where(await GetProjectVersionedEntity_PredicateAsync<CeMatrix>(
                    projectId,
                    null,
                    entitiesCollectionInfo,
                    dbContext,
                    dbCache
                    ))
                        .Include(t => t.CeMatrixParams)
                        .Include(t => t.CeMatrixDbFileReferences)
                        .Include(t => t.Rows)
                        .Include(t => t.Columns)
                        .Include(t => t.Cells)
                        .Include(t => t.CeMatrixComments))
                {
                    SetIsDeleted_CeMatrix(dbContext, ceMatrix);
                }
            else if (entityType == typeof(Tag))
                foreach (Tag tag in dbContext.Tags.Where(await GetProjectVersionedEntity_PredicateAsync<Tag>(
                    projectId,
                    null,
                    entitiesCollectionInfo,
                    dbContext,
                    dbCache
                    ))
                    .Include(t => t.TagParams)                    
                    .Include(t => t.TagConditions)
                    .Include(t => t.TagDbFileReferences))
                {
                    SetIsDeleted_Tag(dbContext, tag);
                }
            else if (entityType == typeof(BaseActuator))
                foreach (BaseActuator baseActuator in dbContext.BaseActuators.Where(await GetProjectVersionedEntity_PredicateAsync<BaseActuator>(
                    projectId,
                    null,
                    entitiesCollectionInfo,
                    dbContext,
                    dbCache
                    ))
                        .Include(ba => ba.BaseActuatorParams)
                        .Include(ba => ba.BaseActuatorDbFileReferences))
                {
                    if (String.IsNullOrEmpty(baseActuator.Identifier)) // Do not delete empty base actuator
                        continue;
                    SetIsDeleted_BaseActuator(dbContext, baseActuator);
                }
            else if (entityType == typeof(SafetyController))
                foreach (SafetyController safetyController in dbContext.SafetyControllers.Where(await GetProjectVersionedEntity_PredicateAsync<SafetyController>(
                    projectId,
                    null,
                    entitiesCollectionInfo,
                    dbContext,
                    dbCache
                    ))
                        .Include(ba => ba.SafetyControllerParams)
                        .Include(ba => ba.SafetyControllerDbFileReferences))
                {
                    SetIsDeleted_SafetyController(dbContext, safetyController);
                }
            else if (entityType == typeof(Legend))
                foreach (Legend legend in dbContext.Legends.Where(await GetProjectVersionedEntity_PredicateAsync<Legend>(
                    projectId,
                    null,
                    entitiesCollectionInfo,
                    dbContext,
                    dbCache
                    )))
                {
                    SetIsDeleted_Legend(dbContext, legend);
                }

            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Calls dbContext.SaveChanges()
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="pazCheckDbContext_PropertyInfo"></param>        
        /// <returns></returns>
        public static async Task CreateDeleteInfoEntities_SetIsDeletedAsync(PazCheckDbContext dbContext, EntitiesCollectionInfo entitiesCollectionInfo, PropertyInfo pazCheckDbContext_PropertyInfo)
        {
            object? dbSet = pazCheckDbContext_PropertyInfo.GetValue(dbContext);
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();

            MethodInfo? method = typeof(PazCheckDbHelper).GetMethod(nameof(GetCreateDeleteInfoEntity_Predicate))!.MakeGenericMethod(entityType);
            object? predicate = method.Invoke(null,
            [
                entitiesCollectionInfo
            ]);

            method = ObjectHelper.GetMethodExt(typeof(Queryable), nameof(Queryable.Where),
            [
                typeof(IQueryable<ObjectHelper.T>),
                typeof(Expression<Func<ObjectHelper.T, bool>>)
            ])!.MakeGenericMethod(entityType);            
            var enumerable = (System.Collections.IEnumerable)method.Invoke(null, new object?[]
            {
                dbSet,
                predicate
            })!;
            foreach (ICreateDeleteInfoEntity entity in enumerable)
            {
                // TODO delete events                             
                entity._IsDeleted = true;
            }

            await dbContext.SaveChangesAsync();
        }

        public static async Task EntitiesDeleteAsync(
            PazCheckDbContext dbContext, 
            DbCache dbCache,
            EntitiesCollectionInfo entitiesCollectionInfo, 
            PropertyInfo pazCheckDbContext_PropertyInfo)
        {
            bool includeAll = entitiesCollectionInfo.IncludeAll;
            int[]? idsToInclude = entitiesCollectionInfo.IdsToInclude;
            int[]? idsToExclude = entitiesCollectionInfo.IdsToExclude;
            Filter? filter = entitiesCollectionInfo.Filter;

            if (entitiesCollectionInfo.IncludeAll)
            {
                if (filter is not null)
                {
                    FilterHelper.Prepare(filter, dbCache);

                    object? dbSet = pazCheckDbContext_PropertyInfo.GetValue(dbContext);
                    Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
                    idsToInclude = (await FilterAsync(dbContext, dbCache, entityType, filter, null)).Select(e => e.Id).ToArray();

                    string idsToIncludeString = String.Join(',', idsToInclude.Select(id => new Any(id).ValueAsString(false)));

                    if (idsToExclude is not null && idsToExclude.Length > 0)
                    {
                        string idsToExcludeString = String.Join(',', idsToExclude.Select(id => new Any(id).ValueAsString(false)));
                        // SQL injection safe
                        await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"{pazCheckDbContext_PropertyInfo.Name}\" WHERE \"Id\" IN ({idsToIncludeString}) AND \"Id\" NOT IN ({idsToExcludeString})");
                    }
                    else
                    {
                        // SQL injection safe
                        await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"{pazCheckDbContext_PropertyInfo.Name}\" WHERE \"Id\" IN ({idsToIncludeString})");
                    }
                }
                else
                {
                    if (idsToExclude is not null && idsToExclude.Length > 0)
                    {
                        string idsToExcludeString = String.Join(',', idsToExclude.Select(id => new Any(id).ValueAsString(false)));
                        // SQL injection safe
                        await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"{pazCheckDbContext_PropertyInfo.Name}\" WHERE \"Id\" NOT IN ({idsToExcludeString})");
                    }
                    else
                    {
                        // SQL injection safe
                        await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"{pazCheckDbContext_PropertyInfo.Name}\"");
                    }
                }                
            }
            else
            {
                if (idsToInclude is not null && idsToInclude.Length > 0)
                {
                    string idsToIncludeString = String.Join(',', idsToInclude.Select(id => new Any(id).ValueAsString(false)));
                    // SQL injection safe
                    await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"{pazCheckDbContext_PropertyInfo.Name}\" WHERE \"Id\" IN ({idsToIncludeString})");
                }
            }
        }

        public static async Task<System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>>> GetProjectVersionedEntity_PredicateAsync<TProjectVersionEntityBase>(
            int projectId, 
            UInt32? projectVersionNum, 
            EntitiesCollectionInfo entitiesCollectionInfo,
            PazCheckDbContext readOnlyDbContext,
            DbCache dbCache)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            bool includeAll = entitiesCollectionInfo.IncludeAll;
            int[]? idsToInclude = entitiesCollectionInfo.IdsToInclude;
            int[]? idsToExclude = entitiesCollectionInfo.IdsToExclude;
            Filter? filter = entitiesCollectionInfo.Filter;
            if (filter is not null)
                filter.ParentEntityId = projectId; // Normalize

            if (projectVersionNum is null)
            {
                if (includeAll)
                {
                    if (filter is not null)
                    {
                        FilterHelper.Prepare(filter, dbCache);

                        idsToInclude = (await FilterAsync(readOnlyDbContext, dbCache, typeof(TProjectVersionEntityBase), filter, null)).Select(e => e.Id).ToArray();
                        if (idsToExclude is not null && idsToExclude.Length > 0)
                            return t => idsToInclude.Contains(t.Id) && !idsToExclude.Contains(t.Id);
                        else
                            return t => idsToInclude.Contains(t.Id);
                    }
                    else
                    {
                        if (idsToExclude is not null && idsToExclude.Length > 0)
                            return t => t.ProjectId == projectId && t._IsDeleted == false && !idsToExclude.Contains(t.Id);
                        else
                            return t => t.ProjectId == projectId && t._IsDeleted == false;
                    }
                }
                else
                {
                    if (idsToInclude is not null && idsToInclude.Length > 0)
                        return t => t.ProjectId == projectId && t._IsDeleted == false && idsToInclude.Contains(t.Id);
                    else
                        return t => false;
                }
            }
            else
            {
                if (includeAll)
                {
                    if (filter is not null)
                    {
                        FilterHelper.Prepare(filter, dbCache);

                        idsToInclude = (await FilterAsync(readOnlyDbContext, dbCache, typeof(TProjectVersionEntityBase), filter, projectVersionNum)).Select(e => e.Id).ToArray();
                        if (idsToExclude is not null && idsToExclude.Length > 0)
                            return t => idsToInclude.Contains(t.Id) && !idsToExclude.Contains(t.Id);
                        else
                            return t => idsToInclude.Contains(t.Id);
                    }
                    else
                    {
                        if (idsToExclude is not null && idsToExclude.Length > 0)
                            return t => t.ProjectId == projectId && t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= projectVersionNum &&
                                (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > projectVersionNum) && !idsToExclude.Contains(t.Id);
                        else
                            return t => t.ProjectId == projectId && t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= projectVersionNum &&
                                (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > projectVersionNum);
                    }                    
                }
                else
                {
                    if (idsToInclude is not null && idsToInclude.Length > 0)
                        return t => t.ProjectId == projectId && t._CreateProjectVersionNum != null && t._CreateProjectVersionNum <= projectVersionNum &&
                            (t._DeleteProjectVersionNum == null || t._DeleteProjectVersionNum > projectVersionNum) && idsToInclude.Contains(t.Id);
                    else
                        return t => false;
                }
            }
        }

        public static System.Linq.Expressions.Expression<Func<TCreateDeleteInfoEntity, bool>> GetCreateDeleteInfoEntity_Predicate<TCreateDeleteInfoEntity>(
            EntitiesCollectionInfo entitiesCollectionInfo)
            where TCreateDeleteInfoEntity : ICreateDeleteInfoEntity
        {
            bool includeAll = entitiesCollectionInfo.IncludeAll;
            int[]? idsToInclude = entitiesCollectionInfo.IdsToInclude;
            int[]? idsToExclude = entitiesCollectionInfo.IdsToExclude;
            Filter? filter = entitiesCollectionInfo.Filter;

            if (includeAll)
            {
                // TODO
                //if (filter is not null)
                //{
                //    idsToInclude = (await FilterAsync(readOnlyDbContext, dbCache, typeof(TCreateDeleteInfoEntity), filter, null)).Select(e => e.Id).ToArray();
                //    if (idsToExclude is not null && idsToExclude.Length > 0)
                //        return t => idsToInclude.Contains(t.Id) && !idsToExclude.Contains(t.Id);
                //    else
                //        return t => idsToInclude.Contains(t.Id);
                //}
                //else
                //{
                //    if (idsToExclude is not null && idsToExclude.Length > 0)
                //        return t => t._IsDeleted == false && !idsToExclude.Contains(t.Id);
                //    else
                //        return t => t._IsDeleted == false;
                //}

                if (idsToExclude is not null && idsToExclude.Length > 0)
                    return t => t._IsDeleted == false && !idsToExclude.Contains(t.Id);
                else
                    return t => t._IsDeleted == false;
            }
            else
            {
                if (idsToInclude is not null && idsToInclude.Length > 0)
                    return t => t._IsDeleted == false && idsToInclude.Contains(t.Id);
                else
                    return t => false;
            }
        }

        /// <summary>
        ///     !!! Do not check ._IsDeleted property !!!
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="includeAll"></param>
        /// <param name="idsToInclude"></param>
        /// <param name="idsToExclude"></param>
        /// <returns></returns>
        public static System.Linq.Expressions.Expression<Func<TEntity, bool>> GetEntity_Predicate<TEntity>(
            bool includeAll,
            int[]? idsToInclude,
            int[]? idsToExclude)
            where TEntity : Identifiable<int>
        {
            if (includeAll)
            {
                if (idsToExclude is not null && idsToExclude.Length > 0)
                    return t => !idsToExclude.Contains(t.Id);
                else
                    return t => true;
            }
            else
            {
                if (idsToInclude is not null && idsToInclude.Length > 0)
                    return t => idsToInclude.Contains(t.Id);
                else
                    return t => false;
            }
        }                           

        /// <summary>
        ///     Includes template TagConditions. Generic constants %() are substituted.
        /// </summary>        
        /// <param name="projectAllParamValues"></param>        
        /// <param name="tagName"></param>
        /// <param name="csvDb"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static List<Serialization.TagCondition> GetTagConditions(
            ProjectAllParamValues projectAllParamValues,                 
            string tagName,
            CsvDb csvDb,            
            ILoggersSet loggersSet)
        {
            List<Serialization.TagCondition> resultTagConditionsList = new();            

            Func<string, IterationInfo, string> getConstantValue = (string constant, IterationInfo iterationInfo) =>
            {
                if (String.Equals(constant, PazCheckConstants.Constant_CauseCondition, StringComparison.InvariantCultureIgnoreCase) ||
                        String.Equals(constant, PazCheckConstants.Constant_EffectCondition, StringComparison.InvariantCultureIgnoreCase))
                    return constant;
                return GetConstantValue(constant, tagName, projectAllParamValues.TagsParamValues, PazCheckConstants.ParamName_TagTemplate, PazCheckConstants.TypeIdentifier_Tag, iterationInfo);
            };

            IterationInfo iterationInfo = new();            
            
            GatTemplateTag_TagConditions(                    
                    projectAllParamValues,                    
                    tagName,
                    getConstantValue,
                    csvDb,
                    iterationInfo,
                    resultTagConditionsList,                    
                    loggersSet);

            GatTag_TagConditions(                    
                    projectAllParamValues,
                    tagName,
                    getConstantValue,
                    csvDb,
                    iterationInfo,
                    resultTagConditionsList);

            return resultTagConditionsList;
        }

        public static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetVersionEntityPredicate<TProjectVersionEntityBase>(UInt32? projectVersionNum, int projectId, string identifier)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (projectVersionNum is not null)
                return e => e.ProjectId == projectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value) &&
                    e.Identifier == identifier;
            else
                return e => e.ProjectId == projectId && e._IsDeleted == false &&
                    e.Identifier == identifier;
        }

        public static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetVersionEntityPredicate<TProjectVersionEntityBase>(UInt32? projectVersionNum, int projectId)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            if (projectVersionNum is not null)
                return e => e.ProjectId == projectId && e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value);
            else
                return e => e.ProjectId == projectId && e._IsDeleted == false;
        }

        public static System.Linq.Expressions.Expression<Func<TVersionEntityBase, bool>> GetVersionEntityPredicate<TVersionEntityBase>(UInt32? projectVersionNum)
            where TVersionEntityBase : VersionedEntityBase
        {
            if (projectVersionNum is not null)
                return e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value);
            else
                return e => e._IsDeleted == false;
        }

        public static bool GetVersionEntityPredicate<TVersionEntityBase>(TVersionEntityBase e, UInt32? projectVersionNum)
            where TVersionEntityBase : VersionedEntityBase
        {
            if (projectVersionNum is not null)
                return e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value);
            else
                return e._IsDeleted == false;
        }

        public static void AddStandardParams<TParam>(List<TParam> _params, List<EntityFramework.ParamInfo> standardParamInfos)
            where TParam : VersionedParamBase, new()
        {
            foreach (var standardParamInfo in standardParamInfos)
            {
                TParam _param = new()
                {
                    ParamName = standardParamInfo.ParamName,                    
                    Value = standardParamInfo.DefaultValue,
                };
                _params.Add(_param);
            }
        }

        /// <summary>
        ///     result.Unit must be included.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="user"></param>
        /// <param name="dbCache"></param>
        /// <param name="result"></param>        
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task AddResultToRootPcObjectAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory, 
            string user,
            DbCache dbCache, 
            Result result, 
            ILoggersSet loggersSet)
        {
            CentralServer.Common.Serialization.SerializationRootObject serializationRootObject = new();

            await Create_Unit_BasePcObjectsAsync(
                result.Unit.Identifier,
                dbContextFactory,
                user,
                dbCache);

            serializationRootObject.PcObjects = new List<CentralServer.Common.Serialization.PcObject>();
            var (unit_PcObject, otherArea_PcObject) = Create_Unit_Objects(result.Unit);
            serializationRootObject.PcObjects.Add(unit_PcObject);
            serializationRootObject.PcObjects.Add(otherArea_PcObject);

            Add_EmergencyShutdown_Event(dbCache, unit_PcObject, result, loggersSet);

            if (serializationRootObject.PcObjects.Any(o => !CheckPcObject(o, dbCache)))
            {
                try
                {
                    await SerializationHelper.ImportSerializationRootObjectAsync(
                        serializationRootObject,
                        new CentralServer.Common.Serialization.ImportMetadata()
                        {
                            RootCollectionMode = CentralServer.Common.Serialization.CollectionMode.Update,
                            ChildCollectionMode = CentralServer.Common.Serialization.CollectionMode.Update,
                            DataCollectionMode = CentralServer.Common.Serialization.CollectionMode.Update,
                        },
                        dbContextFactory,
                        user,
                        null,
                        CancellationToken.None,
                        NullJobProgress.Instance,
                        LoggersSet.Empty,
                        new CentralServer.Common.Serialization.ImportSerializationRootObjectResult(),
                        preview: false);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, @"ImportSerializationRootObjectAsync error.");
                }
            }
        }

        public static void Add_EmergencyShutdown_Event(DbCache dbCache, CentralServer.Common.Serialization.PcObject unit_PcObject, Result result, ILoggersSet loggersSet)
        {
            var resultStatisticsDictionary = result.StatisticsDictionary;

            DateTime beginTimeUtc = new Any(resultStatisticsDictionary.TryGetValue(PazCheckConstants.ParamName_ResultEventTime)).ValueAs<DateTime>(false);
            if (beginTimeUtc != default)
            {
                //if (!force)
                //{
                //    PcObject? existingUnitPcObject = null;
                //    PcObjectEvent? existingPcObjectEvent = null;
                //    dbCache.PcObjectsDictionary1.TryGetValue(unit_PcObject.Unit + "." + unit_PcObject.Identifier, out existingUnitPcObject);
                //    if (existingUnitPcObject is not null)
                //        existingPcObjectEvent = existingUnitPcObject.PcObjectEvents                            
                //            .Where(poe =>
                //                poe.BeginTimeUtc >= beginTimeUtc - TimeSpan.FromSeconds(1) &&
                //                poe.BeginTimeUtc <= beginTimeUtc + TimeSpan.FromSeconds(1) &&
                //                poe.PcObjectEventTypeLower == PazCheckConstants.PcObjectEventType_EmergencyShutdown_LowerCase)
                //            .FirstOrDefault();
                //    if (existingPcObjectEvent is not null)
                //        return;
                //}

                var paramsDictionary = new CaseInsensitiveOrderedDictionary<string?>(resultStatisticsDictionary);
                paramsDictionary[PazCheckConstants.ParamName_EventTitle] = resultStatisticsDictionary.TryGetValue(PazCheckConstants.ParamName_ResultEventDesc) ?? @"";
                paramsDictionary[PazCheckConstants.ParamName_Reference_ResultId] = $"{result.Id}";

                CentralServer.Common.Serialization.PcObjectEvent pcObjectEvent = new()
                {
                    EventType = PazCheckConstants.PcObjectEventType_EmergencyShutdown,
                    BeginTimeUtc = new Any(beginTimeUtc).ValueAsString(false),
                    EndTimeUtc = new Any(beginTimeUtc).ValueAsString(false),
                    EventParams = paramsDictionary.Select(kvp => new CentralServer.Common.Serialization.Param()
                    {
                        Name = kvp.Key,
                        Value = kvp.Value ?? @""
                    }).ToList()
                };
                if (unit_PcObject.PcObjectEvents is null)
                    unit_PcObject.PcObjectEvents = new List<CentralServer.Common.Serialization.PcObjectEvent>();
                unit_PcObject.PcObjectEvents.Add(pcObjectEvent);
            }
        }

        //public static string[] GetActuatorInfos(PazCheckDbContext dbContext, Result result)
        //{
        //    List<string> actuatorInfos = new();
        //    foreach (var row in GetActuatorInfosCsv(dbContext, result))
        //    {
        //        actuatorInfos.Add(row[0] + ": " + GetTriggeredTypeDesc(new Any(row[1]).ValueAs<TriggeredType>(false)));
        //    }

        //    return actuatorInfos.ToArray();
        //}

        //private static string GetTriggeredTypeDesc(TriggeredType triggeredType)
        //{
        //    switch (triggeredType)
        //    {
        //        case TriggeredType.SuccessTriggered:
        //        case TriggeredType.SuccessFirstTriggered:

        //    }
        //}

        public static async Task<List<List<List<string?>>>> GetResultInfoAsync(
            AddonsManager addonsManager,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            DbCache dbCache,
            int resultId)
        {
            List<List<List<string?>>> resultInfo = new();

            ExportTypeFilesInfoEx? exportTypeFilesInfoEx = null;
            IReportsExportAddon? addon = null;
            foreach (IReportsExportAddon a in addonsManager.AddonsThreadSafe.OfType<IReportsExportAddon>())
            {
                exportTypeFilesInfoEx = ((IReportsExportAddon)a).GetExportTypeFilesInfoExs([ IReportsExportAddon.ReportCategory_DiagnostResult_UpperCase ]).FirstOrDefault();
                if (exportTypeFilesInfoEx is not null)
                {
                    addon = a;
                    break;
                }
            }
            
            if (exportTypeFilesInfoEx is null || addon is null)
                return resultInfo;

            Filter filter = FilterHelper.Create(new List<CaseInsensitiveOrderedDictionary<List<string>>>
            {
                new CaseInsensitiveOrderedDictionary<List<string>>()
                {
                    { PazCheckConstants.CriterionName_ResultId, [ resultId.ToString() ]}
                }
            });

            (string, byte[])[] export = await addon.ExportReportAsync(
                    dbContextFactory,
                    dbCache,                                       
                    exportTypeFilesInfoEx.ExportTypeFilesInfo.DestinationTypeIdentifier,
                    filter,
                    new CaseInsensitiveOrderedDictionary<string?>(),
                    CancellationToken.None,
                    LoggersSet.Empty);
           
            if (export.Length == 0)
                return resultInfo;

            using (var workbook = new XLWorkbook(new MemoryStream(export[0].Item2)))
            {
                foreach (var worksheet in workbook.Worksheets.Skip(1))
                {
                    List<List<string?>> table = new();
                    table.Add(new List<string?> { worksheet.Name });

                    IXLRange? usedRange = worksheet.RangeUsed();
                    if (usedRange is not null)
                        foreach (int row in Enumerable.Range(usedRange.RangeAddress.FirstAddress.RowNumber, usedRange.RowCount()))
                        {
                            List<string?> rowValues = new();

                            foreach (int column in Enumerable.Range(usedRange.RangeAddress.FirstAddress.ColumnNumber, usedRange.ColumnCount()))
                            {
                                var cell = worksheet.Cell(row, column);
                                rowValues.Add(ExcelHelper.GetCellValueForCsv(cell));
                            }

                            table.Add(rowValues);
                        }

                    if (table.Count > 1)
                        resultInfo.Add(table);
                }
            }

            return resultInfo;
        }

        /// <summary>        
        ///     If collectionToFilter is null, DB query or dbCache are used.
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="dbCache"></param>
        /// <param name="entityType"></param>
        /// <param name="preparedFilter"></param>
        /// <param name="projectVersionNum"></param>
        /// <param name="currentObject"></param>
        /// <param name="needOrdering"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<IOrderedEnumerable<Identifiable<int>>> FilterAsync(
            PazCheckDbContext readOnlyDbContext,
            DbCache dbCache,
            Type entityType, 
            Filter preparedFilter,
            UInt32? projectVersionNum,
            object? currentObject = null,
            bool needOrdering = false)
        {
            switch (entityType.Name)
            {
                case nameof(CeMatrix):
                    {
                        var projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(preparedFilter.ParentEntityId ?? 0, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);

                        IQueryable<CeMatrix> q;
                        if (projectVersionNum is null)
                        {
                            q = readOnlyDbContext.CeMatrices
                                .Where(GetVersionEntityPredicate<CeMatrix>(null, preparedFilter.ParentEntityId ?? 0))
                                .Include(m => m.Rows.Where(e => e._IsDeleted == false))
                                .Include(m => m.Columns.Where(e => e._IsDeleted == false))
                                .Include(m => m.Cells.Where(e => e._IsDeleted == false))
                                .Include(m => m.CeMatrixComments.Where(e => e._IsDeleted == false));
                        }
                        else
                        {
                            q = readOnlyDbContext.CeMatrices
                                .Where(GetVersionEntityPredicate<CeMatrix>(projectVersionNum, preparedFilter.ParentEntityId ?? 0))
                                .Include(m => m.Rows.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value)))
                                .Include(m => m.Columns.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value)))
                                .Include(m => m.Cells.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value)))
                                .Include(m => m.CeMatrixComments.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                    (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value)));
                        }

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var isTemplate_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_IsTemplate, StringComparison.InvariantCultureIgnoreCase));
                        if (isTemplate_Criterion is not null)
                        {
                            if (new Any(isTemplate_Criterion.ValuesList?.FirstOrDefault()).ValueAsBoolean(false))
                                q = q.Where(i => i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                            else
                                q = q.Where(i => !i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                        }

                        IEnumerable<CeMatrix> ceMatrices = await q.ToListAsync();

                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrices = ceMatrices
                                    .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         projectAllParamValues.CeMatricesParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                             p.ParamName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                             p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null ||
                                         e.Rows.Any(r => r.Header.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) ||
                                         e.Columns.Any(c => c.Header.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) ||
                                         e.Cells.Any(c => c.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)));
                            else if (String.Equals(preparedFilter.SearchBy, Properties.Resources.SearchBy_ExcludeContent, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrices = ceMatrices
                                    .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         projectAllParamValues.CeMatricesParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                             p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null);
                            else if (String.Equals(preparedFilter.SearchBy, Properties.Resources.SearchBy_IdentifierTitleDesc, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrices = ceMatrices
                                    .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         projectAllParamValues.CeMatricesParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                             (String.Equals(p.ParamName, PazCheckConstants.ParamName_Title, StringComparison.InvariantCultureIgnoreCase) ||
                                             String.Equals(p.ParamName, PazCheckConstants.ParamName_Desc, StringComparison.InvariantCultureIgnoreCase)) &&
                                             p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null);
                        }

                        var tagName_CriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_TagName, StringComparison.InvariantCultureIgnoreCase))
                            ?.ToList();
                        if (tagName_CriterionCollection?.Count > 0)
                        {
                            ceMatrices = ceMatrices.Where(m =>
                            {
                                bool all = true;
                                foreach (var criterion in tagName_CriterionCollection)
                                {
                                    bool succeeded =
                                        m.Rows.Where(r => r.TagCondition_SymbolToDisplay is not null).Any(r =>
                                        {
                                            foreach (string? ceMatrix_TagName in CsvHelper.ParseCsvLine(@",", r.Header))
                                            {
                                                if (SszOperatorHelper.Compare(ceMatrix_TagName, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList))
                                                    return true;
                                            }
                                            return false;
                                        }) ||
                                        m.Columns.Where(c => c.TagCondition_SymbolToDisplay is not null).Any(c =>
                                        {
                                            foreach (string? ceMatrix_TagName in CsvHelper.ParseCsvLine(@",", c.Header))
                                            {
                                                if (SszOperatorHelper.Compare(ceMatrix_TagName, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList))
                                                    return true;
                                            }
                                            return false;
                                        });
                                    if (!succeeded)
                                    {
                                        all = false;
                                        break;
                                    }

                                }
                                return all;
                            });
                        }

                        var filteredCriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();                        
                        if (filteredCriterionCollection?.Length > 0)
                            ceMatrices = ceMatrices
                                .Where(e => FilterByParams(
                                    projectAllParamValues.CeMatricesParamValues,
                                    e.Identifier,
                                    filteredCriterionCollection));
                        
                        return GetOrdered(
                            projectAllParamValues.CeMatricesParamValues,
                            needOrdering,
                            ceMatrices, 
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o._LastChangeTimeUtc).ThenBy(o => o.Identifier));                        
                    }
                case nameof(CeMatrixResult):
                    {   
                        IQueryable<CeMatrixResult> q = readOnlyDbContext.CeMatrixResults
                                .Where(mr => mr.ResultId == (preparedFilter.ParentEntityId ?? 0))
                                .Include(mr => mr.Result)
                                .Include(mr => mr.RowResults)
                                .Include(mr => mr.ColumnResults)
                                .Include(mr => mr.CellResults);

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        IEnumerable<CeMatrixResult> ceMatrixResults = await q.ToListAsync();

                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrixResults = ceMatrixResults
                                    .Where(mr => mr.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         mr.CeMatrixParamsDictionary.Any(kvp =>
                                             kvp.Key.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                             (kvp.Value?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)) ||
                                         mr.RowResults.Any(r => r.Header.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) ||
                                         mr.ColumnResults.Any(c => c.Header.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) ||
                                         mr.CellResults.Any(c => c.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)));
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_ExcludeContent, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrixResults = ceMatrixResults
                                    .Where(mr => mr.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||                                          
                                         mr.CeMatrixParamsDictionary.Any(kvp =>                                             
                                             (kvp.Value?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)));
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_IdentifierTitleDesc, StringComparison.InvariantCultureIgnoreCase))
                                ceMatrixResults = ceMatrixResults
                                    .Where(mr => mr.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         mr.CeMatrixParamsDictionary.Any(kvp =>
                                             (String.Equals(kvp.Key, PazCheckConstants.ParamName_Title, StringComparison.InvariantCultureIgnoreCase) ||
                                             String.Equals(kvp.Key, PazCheckConstants.ParamName_Desc, StringComparison.InvariantCultureIgnoreCase)) &&
                                             (kvp.Value?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)));
                        }

                        var tagName_CriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_TagName, StringComparison.InvariantCultureIgnoreCase))
                            ?.ToList();
                        if (tagName_CriterionCollection?.Count > 0)
                        {
                            ceMatrixResults = ceMatrixResults.Where(m =>
                            {
                                bool all = true;
                                foreach (var criterion in tagName_CriterionCollection)
                                {
                                    bool succeeded =
                                        m.RowResults.Where(r => r.TagCondition_SymbolToDisplay is not null).Any(r =>
                                        {
                                            foreach (string? tagName in CsvHelper.ParseCsvLine(@",", r.Header))
                                            {
                                                if (SszOperatorHelper.Compare(tagName, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList))
                                                    return true;
                                            }
                                            return false;
                                        }) ||
                                        m.ColumnResults.Where(c => c.TagCondition_SymbolToDisplay is not null).Any(c =>
                                        {
                                            foreach (string? tagName in CsvHelper.ParseCsvLine(@",", c.Header))
                                            {
                                                if (SszOperatorHelper.Compare(tagName, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList))
                                                    return true;
                                            }
                                            return false;
                                        });                                    
                                    if (!succeeded)
                                    {
                                        all = false;
                                        break;
                                    }

                                }
                                return all;
                            });
                        }

                        var filteredCriterionCollection1 = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();
                        if (filteredCriterionCollection1?.Length > 0)
                            ceMatrixResults = ceMatrixResults
                                .Where(e => FilterByParams(
                                    e.CeMatrixParamsDictionary,
                                    filteredCriterionCollection1));

                        List<Criterion> filteredCriterionCollection2 = new();

                        var filteredCriterion3 = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_TriggeredType, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault();
                        if (filteredCriterion3 is not null && filteredCriterion3.ValuesList is not null)
                        {
                            foreach (string value in filteredCriterion3.ValuesList)
                            {
                                var criterion = new Criterion()
                                {
                                    CriterionName = $"Statistics[{value}]",
                                    Operator = ">",
                                    ValuesList = [ "0" ]
                                };
                                FilterHelper.Prepare(criterion, dbCache);
                                filteredCriterionCollection2.Add(criterion);
                            }
                        }

                        var filteredCriterionCollection4 = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Statistics[") && ci.CriterionName.EndsWith("]"));
                        if (filteredCriterionCollection4 is not null)
                            filteredCriterionCollection2.AddRange(filteredCriterionCollection4);

                        if (filteredCriterionCollection2.Count > 0)
                            ceMatrixResults = ceMatrixResults
                                .Where(e => FilterByParams(
                                    e.StatisticsDictionary,
                                    filteredCriterionCollection2));

                        return GetOrdered(
                            e => e.CeMatrixParamsDictionary,
                            needOrdering,
                            ceMatrixResults,
                            preparedFilter,
                            defaultOrdering: e => e.OrderBy(o => o.Identifier));
                    }
                case nameof(Tag):
                    {
                        IQueryable<Tag> q = readOnlyDbContext.Tags.Where(GetVersionEntityPredicate<Tag>(projectVersionNum, preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var isTemplate_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_IsTemplate, StringComparison.InvariantCultureIgnoreCase));
                        if (isTemplate_Criterion is not null)
                        {
                            if (new Any(isTemplate_Criterion.ValuesList?.FirstOrDefault()).ValueAsBoolean(false))
                                q = q.Where(i => i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                            else
                                q = q.Where(i => !i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                        }

                        IEnumerable<Tag> tags = await q.ToListAsync();

                        ProjectAllParamValues projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(preparedFilter.ParentEntityId ?? 0, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);                        
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                            {
                                tags = tags
                                    .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         projectAllParamValues.TagsParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                             p.ParamName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                             p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null ||
                                         projectAllParamValues.TagConditions.GetValueOrDefault(e.Identifier)?.FirstOrDefault(tc =>
                                            tc.ToString().Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                            ) != null);
                            }
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_TagName, StringComparison.InvariantCultureIgnoreCase))
                            {                                
                                tags = tags
                                    .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                            }                            
                        }

                        var filteredCriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();
                        if (filteredCriterionCollection?.Length > 0)
                            tags = tags
                                .Where(e => FilterByParams(
                                    projectAllParamValues.TagsParamValues,
                                    e.Identifier,
                                    filteredCriterionCollection));

                        return GetOrdered(
                            projectAllParamValues.TagsParamValues,
                            needOrdering,
                            tags,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o._LastChangeTimeUtc).ThenBy(o => o.Identifier));
                    }
                case nameof(BaseActuator):
                    {
                        IQueryable<BaseActuator> q = readOnlyDbContext.BaseActuators.Where(GetVersionEntityPredicate<BaseActuator>(projectVersionNum, preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var isTemplate_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_IsTemplate, StringComparison.InvariantCultureIgnoreCase));
                        if (isTemplate_Criterion is not null)
                        {
                            if (new Any(isTemplate_Criterion.ValuesList?.FirstOrDefault()).ValueAsBoolean(false))
                                q = q.Where(i => i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                            else
                                q = q.Where(i => !i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                        }

                        IEnumerable<BaseActuator> baseActuators = await q.ToListAsync();

                        ProjectAllParamValues projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(preparedFilter.ParentEntityId ?? 0, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);

                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {                            
                            baseActuators = baseActuators
                                .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                     projectAllParamValues.BaseActuatorsParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                         p.ParamName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null);
                        }

                        var filteredCriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();
                        if (filteredCriterionCollection?.Length > 0)
                            baseActuators = baseActuators
                                .Where(e => FilterByParams(
                                    projectAllParamValues.BaseActuatorsParamValues,
                                    e.Identifier,
                                    filteredCriterionCollection));

                        return GetOrdered(
                            projectAllParamValues.BaseActuatorsParamValues,
                            needOrdering,
                            baseActuators,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o._LastChangeTimeUtc).ThenBy(o => o.Identifier));
                    }
                case nameof(SafetyController):
                    {
                        IQueryable<SafetyController> q = readOnlyDbContext.SafetyControllers.Where(GetVersionEntityPredicate<SafetyController>(projectVersionNum, preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var isTemplate_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_IsTemplate, StringComparison.InvariantCultureIgnoreCase));
                        if (isTemplate_Criterion is not null)
                        {
                            if (new Any(isTemplate_Criterion.ValuesList?.FirstOrDefault()).ValueAsBoolean(false))
                                q = q.Where(i => i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                            else
                                q = q.Where(i => !i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                        }

                        IEnumerable<SafetyController> safetyControllers = await q.ToListAsync();

                        ProjectAllParamValues projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(preparedFilter.ParentEntityId ?? 0, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);                        

                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {                            
                            safetyControllers = safetyControllers
                                .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                     projectAllParamValues.SafetyControllersParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                         p.ParamName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null);
                        }

                        var filteredCriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();
                        if (filteredCriterionCollection?.Length > 0)
                            safetyControllers = safetyControllers
                                .Where(e => FilterByParams(
                                    projectAllParamValues.SafetyControllersParamValues,
                                    e.Identifier,
                                    filteredCriterionCollection));

                        return GetOrdered(
                            projectAllParamValues.SafetyControllersParamValues,
                            needOrdering,
                            safetyControllers,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o._LastChangeTimeUtc).ThenBy(o => o.Identifier));
                    }
                case nameof(Legend):
                    {
                        IQueryable<Legend> q = readOnlyDbContext.Legends.Where(GetVersionEntityPredicate<Legend>(projectVersionNum, preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var isTemplate_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_IsTemplate, StringComparison.InvariantCultureIgnoreCase));
                        if (isTemplate_Criterion is not null)
                        {
                            if (new Any(isTemplate_Criterion.ValuesList?.FirstOrDefault()).ValueAsBoolean(false))
                                q = q.Where(i => i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                            else
                                q = q.Where(i => !i.IdentifierLower.EndsWith(PazCheckConstants.IdentifierEnding_Template_LowerCase));
                        }

                        IEnumerable<Legend> legends = await q.ToListAsync();

                        ProjectAllParamValues projectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(preparedFilter.ParentEntityId ?? 0, projectVersionNum, readOnlyDbContext, LoggersSet.Empty);                        

                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            legends = legends
                                .Where(e => e.Identifier.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                     projectAllParamValues.LegendsParams.GetValueOrDefault(e.Identifier)?.FirstOrDefault(p =>
                                         p.ParamName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                         p.Value.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)) != null)
                                .ToList();
                        }

                        var filteredCriterionCollection = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.Where(ci => ci.CriterionName.StartsWith("Params[") && ci.CriterionName.EndsWith("]"))
                            ?.ToArray();
                        if (filteredCriterionCollection?.Length > 0)
                            legends = legends
                                .Where(e => FilterByParams(
                                    projectAllParamValues.LegendsParamValues,
                                    e.Identifier,
                                    filteredCriterionCollection));

                        return GetOrdered(
                            projectAllParamValues.LegendsParamValues,
                            needOrdering,
                            legends,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o._LastChangeTimeUtc).ThenBy(o => o.Identifier));
                    }
                case nameof(PcObject):
                    {
                        List<PcObject> pcObjects;
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();
                        // DateTimeHelper.SafeMinDateTimeUtc, if not defined.                        
                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        // DateTime.UtcNow, if not defined.
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);

                        pcObjects = FilterPcObjects(
                                        readOnlyDbContext,
                                        dbCache,
                                        preparedFilter,                                        
                                        toTimeUtc,
                                        currentObject);

                        if ((partCriterionInfosCollection?.Any(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_EventType, StringComparison.InvariantCultureIgnoreCase)) ?? false) ||
                            String.Equals(preparedFilter.SearchBy, Common.Properties.Resources.SearchBy_PcObjectEventAll, StringComparison.InvariantCultureIgnoreCase) ||
                            String.Equals(preparedFilter.SearchBy, Common.Properties.Resources.SearchBy_EventDesc, StringComparison.InvariantCultureIgnoreCase) ||
                            (partCriterionInfosCollection?.Any(ci => ci.CriterionName.StartsWith(PazCheckConstants.CriterionName_EventParams + "[", StringComparison.InvariantCultureIgnoreCase) && ci.CriterionName.EndsWith("]")) ?? false))
                        {
                            var filteredPcObjectIds = (await FilterPcObjectEventsAsync(
                                    readOnlyDbContext,
                                    pcObjects,
                                    preparedFilter,
                                    fromTimeUtc,
                                    toTimeUtc))
                                .Select(poe => poe.PcObjectId)
                                .ToHashSet();
                            pcObjects = pcObjects.Where(po => filteredPcObjectIds.Contains(po.Id)).ToList();
                        }                        

                        return GetOrdered(
                            e => e.ParamsDictionary,
                            needOrdering,
                            pcObjects,
                            preparedFilter,
                            defaultOrdering: e => e.OrderBy(o => o.Identifier));
                    }
                case nameof(PcObjectEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        // DateTimeHelper.SafeMinDateTimeUtc, if not defined.                        
                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        // DateTime.UtcNow, if not defined.
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);

                        List<PcObject> pcObjects = FilterPcObjects(
                                        readOnlyDbContext,
                                        dbCache,
                                        preparedFilter,                                        
                                        toTimeUtc,
                                        currentObject);                        

                        IEnumerable<PcObjectEvent> pcObjectEvents = await FilterPcObjectEventsAsync(
                            readOnlyDbContext,
                            pcObjects, 
                            preparedFilter,
                            fromTimeUtc,
                            toTimeUtc);                        
                        
                        return GetOrdered(
                            e => e.ParamsDictionary,
                            needOrdering,
                            pcObjectEvents,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.BeginTimeUtc));
                    }
                case nameof(UnitEventsInterval):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        IQueryable<UnitEventsInterval> q;
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString) &&
                            preparedFilter.SearchBy == CentralServer.Common.Properties.Resources.SearchBy_All)
                        {
                            q = readOnlyDbContext.UnitEventsIntervals
                                    .Include(uei => uei.UnitEvents);
                        }
                        else
                        {
                            q = readOnlyDbContext.UnitEventsIntervals;
                        }
                        q = q.Where(uei => uei.UnitId == preparedFilter.ParentEntityId);

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(uei => false);
                        else if (toTimeUtc is null)
                            q = q.Where(uei => uei.BeginTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(uei => uei.BeginTimeUtc >= fromTimeUtc && uei.EndTimeUtc <= toTimeUtc.Value);

                        IEnumerable<UnitEventsInterval> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ise => ise.Source.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ise.Comment.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ise.UnitEvents.Any(ue => ue.TagName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                            ue.ConditionString.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                            ue.Message.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase)));
                            }
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_ExcludeContent_UnitEventsInterval, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ise => ise.Source.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ise.Comment.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                            }                            
                        }

                        if (fromTimeUtc is not null && 
                                String.Equals(preparedFilter.Aggregation, @"aggregate", StringComparison.InvariantCultureIgnoreCase) &&
                                preparedFilter.MaxRecordsCount > 0 && preparedFilter.MaxRecordsCount < 10000)
                        {
                            var nowUtc = DateTime.UtcNow;
                            if (toTimeUtc is null)
                                toTimeUtc = nowUtc;

                            long totalTicks = toTimeUtc.Value.Ticks - fromTimeUtc.Value.Ticks;
                            long deltaTicks = totalTicks / preparedFilter.MaxRecordsCount;

                            bool[] fill = new bool[preparedFilter.MaxRecordsCount + 3];
                            foreach (var it in l)
                            {
                                long beginTicks = it.BeginTimeUtc.Ticks - fromTimeUtc.Value.Ticks;                                
                                long endTicks = it.EndTimeUtc.Ticks - fromTimeUtc.Value.Ticks;

                                for (long ticks = Math.Max(0, beginTicks); ticks <= Math.Min(endTicks, totalTicks); ticks += deltaTicks)
                                {
                                    fill[ticks / deltaTicks] = true;
                                }
                            }
                            l = new List<UnitEventsInterval>(preparedFilter.MaxRecordsCount);
                            UnitEventsInterval? currentUnitEventsInterval = null;
                            bool prevF = false;
                            for (int i = 0; i < fill.Length; i += 1)
                            {
                                bool f = fill[i];
                                if (!prevF && f)
                                {
                                    currentUnitEventsInterval = new UnitEventsInterval
                                    {
                                        LoadTimeUtc = nowUtc,
                                        Source = @"",
                                        Comment = @"",
                                        BeginTimeUtc = new DateTime(fromTimeUtc.Value.Ticks + deltaTicks * i, DateTimeKind.Utc),
                                        UnitId = preparedFilter.ParentEntityId!.Value,
                                    };
                                }
                                else if (prevF && !f)
                                {
                                    currentUnitEventsInterval!.EndTimeUtc = new DateTime(fromTimeUtc.Value.Ticks + deltaTicks * i, DateTimeKind.Utc);
                                    ((List<UnitEventsInterval>)l).Add(currentUnitEventsInterval);
                                }
                                prevF = f;
                            }                            
                        }

                        return GetOrdered(
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.BeginTimeUtc));
                    }
                case nameof(UnitEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        IQueryable<UnitEvent> q = readOnlyDbContext.UnitEvents.Where(ue => ue.UnitEventsIntervalId == (preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        var prioritiesArray = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Priority, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.Select(s => new Any(s).ValueAsInt32(false))?.ToArray();                        
                        if (prioritiesArray?.Length > 0)
                            q = q.Where(ue => prioritiesArray.Contains(ue.Priority));
                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(ue => false);
                        else if (toTimeUtc is null)
                            q = q.Where(ue => ue.EventTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(ue => ue.EventTimeUtc >= fromTimeUtc && ue.EventTimeUtc <= toTimeUtc.Value);

                        IEnumerable<UnitEvent> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ue => ue.TagName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ue.ConditionString.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ue.Message.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                            }
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_TagName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ue => ue.TagName.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                            }
                        }
                        return GetOrdered(                            
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.EventTimeUtc));
                    }
                case nameof(UserEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        IQueryable<UserEvent> q;
                        if (!String.IsNullOrEmpty(preparedFilter.User))
                            q = readOnlyDbContext.UserEvents.Where(ue => ue.User == preparedFilter.User);
                        else
                            q = readOnlyDbContext.UserEvents;

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(ue => false);
                        else if (toTimeUtc is null)
                            q = q.Where(ue => ue.EventTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(ue => ue.EventTimeUtc >= fromTimeUtc && ue.EventTimeUtc <= toTimeUtc.Value);

                        var logLevelsArray = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_LogLevel, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.Select(s => new Any(s).ValueAsInt32(false))?.ToArray();
                        if (logLevelsArray?.Length > 0)
                            q = q.Where(ise => logLevelsArray.Contains(ise.LogLevel));
                        IEnumerable<UserEvent> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                            l = l.Where(ise => ise.Message.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        ise.Details.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));

                        return GetOrdered(
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.EventTimeUtc));
                    }
                case nameof(InformationSecurityEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();
                        
                        IQueryable<InformationSecurityEvent> q = readOnlyDbContext.InformationSecurityEvents;

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        string? succeededString = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Succeeded, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();
                        if (!String.IsNullOrEmpty(succeededString))
                        {
                            if (new Any(succeededString).ValueAsBoolean(false))
                                q = q.Where(ise => ise.Succeeded);
                            else
                                q = q.Where(ise => !ise.Succeeded);
                        }
                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(e => false);
                        else if (toTimeUtc is null)
                            q = q.Where(e => e.EventTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(e => e.EventTimeUtc >= fromTimeUtc && e.EventTimeUtc <= toTimeUtc.Value);

                        var severitiesArray = GetSeveritiesByDescs(partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Severity, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.ToArray());
                        if (severitiesArray is not null && severitiesArray.Length > 0)
                        {
                            var minSeverity = severitiesArray.Min();
                            q = q.Where(ise => ise.Severity >= minSeverity);
                        }
                        IEnumerable<InformationSecurityEvent> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                            l = l.Where(ise => ise.EventDesc.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                        return GetOrdered(
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.EventTimeUtc));
                    }
                case nameof(AllRolesAccessInformationSecurityEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        IQueryable<AllRolesAccessInformationSecurityEvent> q = readOnlyDbContext.AllRolesAccessInformationSecurityEvents;

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        string? succeededString = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Succeeded, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();
                        if (!String.IsNullOrEmpty(succeededString))
                        {
                            if (new Any(succeededString).ValueAsBoolean(false))
                                q = q.Where(ise => ise.Succeeded);
                            else
                                q = q.Where(ise => !ise.Succeeded);
                        }
                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(e => false);
                        else if (toTimeUtc is null)
                            q = q.Where(e => e.EventTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(e => e.EventTimeUtc >= fromTimeUtc && e.EventTimeUtc <= toTimeUtc.Value);

                        var severitiesArray = GetSeveritiesByDescs(partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Severity, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.ToArray());
                        if (severitiesArray is not null && severitiesArray.Length > 0)
                        {
                            var minSeverity = severitiesArray.Min();
                            q = q.Where(ise => ise.Severity >= minSeverity);
                        }
                        IEnumerable<AllRolesAccessInformationSecurityEvent> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                            l = l.Where(ise => ise.EventDesc.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase));
                        return GetOrdered(
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.EventTimeUtc));
                    }
                case nameof(ResultEvent):
                    {
                        var partCriterionInfosCollection = preparedFilter.CriterionCollection?.FirstOrDefault();

                        IQueryable<ResultEvent> q = readOnlyDbContext.ResultEvents
                            .Include(re => re.TriggeredUnitEvent) // Optimization, not needed
                            .Where(re => re.ResultId == (preparedFilter.ParentEntityId ?? 0));

                        var id_Criterion = preparedFilter.CriterionCollection?.FirstOrDefault()
                            ?.FirstOrDefault(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Id, StringComparison.InvariantCultureIgnoreCase));
                        if (id_Criterion is not null && id_Criterion.ValuesList is not null && id_Criterion.ValuesList.Length > 0)
                        {
                            if (id_Criterion.ValuesList.Length == 1)
                                q = q.Where(i => i.Id == new Any(id_Criterion.ValuesList[0]).ValueAsInt32(false));
                            else
                                q = q.Where(i => id_Criterion.ValuesList.Select(v => new Any(v).ValueAsInt32(false)).ToArray().Contains(i.Id));
                        }

                        DateTime? fromTimeUtc = GetBeginTimeUtc(partCriterionInfosCollection);
                        DateTime? toTimeUtc = GetEndTimeUtc(partCriterionInfosCollection);
                        if (fromTimeUtc is null)
                            q = q.Where(e => false);
                        else if (toTimeUtc is null)
                            q = q.Where(e => e.TriggeredTimeUtc >= fromTimeUtc);
                        else
                            q = q.Where(e => e.TriggeredTimeUtc >= fromTimeUtc && e.TriggeredTimeUtc <= toTimeUtc.Value);

                        var prioritiesArray = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Priority, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.Select(s => new Any(s).ValueAsInt32(false))?.ToArray();
                        if (prioritiesArray?.Length > 0)
                            q = q.Where(ise => prioritiesArray.Contains(ise.TriggeredUnitEvent!.Priority));
                        IEnumerable<ResultEvent> l = await q.ToListAsync();
                        if (!String.IsNullOrEmpty(preparedFilter.SearchString))
                        {
                            if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_All, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ise => (ise.TriggeredUnitEvent?.TagName?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                                        (ise.TriggeredUnitEvent?.ConditionString?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                                        (ise.TriggeredUnitEvent?.Message?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false));
                            }
                            else if (String.Equals(preparedFilter.SearchBy, CentralServer.Common.Properties.Resources.SearchBy_TagName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                l = l
                                    .Where(ise => (ise.TriggeredUnitEvent?.TagName?.Contains(preparedFilter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false));
                            }
                        }
                        return GetOrdered(
                            needOrdering,
                            l,
                            preparedFilter,
                            defaultOrdering: e => e.OrderByDescending(o => o.TriggeredTimeUtc));
                    }
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     DateTimeHelper.SafeMinDateTimeUtc, if not defined.
        ///     Returns null, if current time
        /// </summary>
        /// <param name="criterionInfosCollection"></param>
        /// <returns></returns>
        public static DateTime? GetBeginTimeUtc(Criterion[]? criterionInfosCollection)
        {
            string? beginTimeString = criterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_From, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();
            if (!String.IsNullOrEmpty(beginTimeString))
            {
                if (String.Equals(beginTimeString, @"now", StringComparison.InvariantCultureIgnoreCase))
                    return null;

                if (beginTimeString.All(c => c >= '0' && c <= '9'))
                    return DateTimeOffset.FromUnixTimeMilliseconds(new Any(beginTimeString).ValueAsInt64(false)).UtcDateTime;

                return new Any(beginTimeString).ValueAs<DateTime>(false);
            }
            else
            {
                return DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime;
            }
        }

        /// <summary>
        ///     DateTime.UtcNow, if not defined.
        ///     Returns null, if current time
        /// </summary>
        /// <param name="criterionInfosCollection"></param>
        /// <returns></returns>
        public static DateTime? GetEndTimeUtc(Criterion[]? criterionInfosCollection)
        {
            string? endTimeString = criterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_To, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();
            if (!String.IsNullOrEmpty(endTimeString))
            {
                if (String.Equals(endTimeString, @"now", StringComparison.InvariantCultureIgnoreCase))
                    return null;

                if (endTimeString.All(c => c >= '0' && c <= '9'))
                    return DateTimeOffset.FromUnixTimeMilliseconds(new Any(endTimeString).ValueAsInt64(false)).UtcDateTime;

                return new Any(endTimeString).ValueAs<DateTime>(false);
            }
            else
            {
                return null;
            }
        }               

        public static int[]? GetSeveritiesByDescs(string[]? severityDescs)
        {
            if (severityDescs is null)
                return null;
            List<int> result = new();
            foreach (var severityDesc in severityDescs)
            {
                if (severityDesc == Properties.Resources.Low_Severity)
                    result.AddRange(new int[] { 1, 2, 3 });
                else if (severityDesc == Properties.Resources.Medium_Severity)
                    result.AddRange(new int[] { 4, 5, 6 });
                else if (severityDesc == Properties.Resources.High_Severity)
                    result.AddRange(new int[] { 7, 8 });
                else if (severityDesc == Properties.Resources.VeryHigh_Severity)
                    result.AddRange(new int[] { 9, 10 });
            }
            return result.ToArray();
        }

        public static void SetIsDeleted_CeMatrix(PazCheckDbContext dbContext, CeMatrix ceMatrix)
        {
            foreach (var ceMatrixParam in ceMatrix.CeMatrixParams)
            {
                SetIsDeleted(dbContext, ceMatrixParam);
            }

            foreach (var ceMatrixDbFileReference in ceMatrix.CeMatrixDbFileReferences)
            {
                SetIsDeleted(dbContext, ceMatrixDbFileReference);
            }

            foreach (var row in ceMatrix.Rows)
            {
                SetIsDeleted(dbContext, row);
            }

            foreach (var column in ceMatrix.Columns)
            {
                SetIsDeleted(dbContext, column);
            }

            foreach (var cell in ceMatrix.Cells)
            {
                SetIsDeleted(dbContext, cell);
            }

            foreach (var ceMatrixComment in ceMatrix.CeMatrixComments)
            {
                SetIsDeleted(dbContext, ceMatrixComment);
            }
            
            SetIsDeleted(dbContext, ceMatrix);
        }

        public static void SetIsDeleted_Tag(PazCheckDbContext dbContext, Tag tag)
        {
            foreach (var tagParam in tag.TagParams)
            {
                SetIsDeleted(dbContext, tagParam);
            }

            foreach (var tagCondition in tag.TagConditions)
            {
                SetIsDeleted(dbContext, tagCondition);
            }

            foreach (var tagDbFileReference in tag.TagDbFileReferences)
            {
                SetIsDeleted(dbContext, tagDbFileReference);
            }
            
            SetIsDeleted(dbContext, tag);
        }

        public static void SetIsDeleted_BaseActuator(PazCheckDbContext dbContext, BaseActuator baseActuator)
        {
            foreach (var baseActuatorParam in baseActuator.BaseActuatorParams)
            {
                SetIsDeleted(dbContext, baseActuatorParam);
            }

            foreach (var baseActuatorDbFileReferences in baseActuator.BaseActuatorDbFileReferences)
            {
                SetIsDeleted(dbContext, baseActuatorDbFileReferences);
            }
            
            SetIsDeleted(dbContext, baseActuator);
        }

        public static void SetIsDeleted_SafetyController(PazCheckDbContext dbContext, SafetyController safetyController)
        {
            foreach (var safetyControllerParam in safetyController.SafetyControllerParams)
            {
                SetIsDeleted(dbContext, safetyControllerParam);
            }

            foreach (var safetyControllerDbFileReferences in safetyController.SafetyControllerDbFileReferences)
            {
                SetIsDeleted(dbContext, safetyControllerDbFileReferences);
            }
            
            SetIsDeleted(dbContext, safetyController);
        }

        public static void SetIsDeleted_Legend(PazCheckDbContext dbContext, Legend legend)
        {
            foreach (var legendParam in legend.LegendParams)
            {
                SetIsDeleted(dbContext, legendParam);
            }

            foreach (var legendDbFileReferences in legend.LegendDbFileReferences)
            {
                SetIsDeleted(dbContext, legendDbFileReferences);
            }

            SetIsDeleted(dbContext, legend);
        }

        public static void SetIsDeleted(PazCheckDbContext dbContext, VersionedEntityBase versionEntity)
        {
            if (versionEntity._CreateProjectVersionNum is null)
            {
                dbContext.Entry(versionEntity).State = EntityState.Deleted;
            }
            if (versionEntity._DeleteProjectVersionNum is null)
            {                
                versionEntity._IsDeleted = true;
            }
            versionEntity._HasUnversionedChanges = true;
        }            

        public static async Task<DbFile> GetOrCreateDbFile(PazCheckDbContext dbContext, Stream stream, string fileName)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            using SHA256 sha256 = SHA256.Create();
            var fileBytesHash_Base64 = Convert.ToBase64String(sha256.ComputeHash(bytes));
            DbFile? dbFile = await dbContext.DbFiles.FirstOrDefaultAsync(f => f.FileBytesHash_Base64 == fileBytesHash_Base64);
            if (dbFile is null)
                dbFile = new DbFile
                {
                    OriginalFileName = fileName,
                    FileBytesCount = bytes.Length,
                    FileBytesHash_Base64 = fileBytesHash_Base64,
                    DbFileContent = new DbFileContent { FileBytes_Base64 = Convert.ToBase64String(bytes) }
                };
            return dbFile;
        }

        /// <summary>
        ///     'Prefix':'ParamNameBase'['OptionalValue']
        ///     <para>Returns only 'ParamNameBase'</para>
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static string GetParamNameBase(string paramName)
        {
            int i = paramName.IndexOf(PazCheckConstants.ParamNamePrefixSeparator);
            if (i > -1)
                paramName = paramName.Substring(i + 1);
            i = paramName.IndexOf('[');
            if (i > -1)
                paramName = paramName.Substring(0, i);
            return paramName.Trim();
        }        

        public static string GetConstantValue(
            string constant, 
            string identifier,
            IReadOnlyDictionary<string, string> paramValues, 
            string templateIdentifier_ParamName,
            string constantBase,
            IterationInfo? iterationInfo = null)
        {
            iterationInfo = iterationInfo ?? new IterationInfo();
            iterationInfo.IterationN += 1;
            if (iterationInfo.IterationN > 64)
                return constant;

            if (String.Equals(constant, $"%({constantBase})", StringComparison.InvariantCultureIgnoreCase))
                return identifier;
            if (String.Equals(constant, $"%({constantBase}NUM)", StringComparison.InvariantCultureIgnoreCase))
            {
                int index = identifier.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
                if (index > 0)
                    return identifier.Substring(index);
                else
                    return constant;
            }
            if (constant.Length < 3)
                return constant;
            var identifierAndParam = constant.Substring(2, constant.Length - 3).Trim(); // Remove '%(' and ')'

            Func<string, IterationInfo, string> getConstantValue = (string constant, IterationInfo iterationInfo) =>
            {
                return GetConstantValue(constant, identifier, paramValues, templateIdentifier_ParamName, constantBase, iterationInfo);
            };

            int i = identifierAndParam.IndexOf('.');
            if (i == -1)
                return GetParamValueInternal(identifier, identifierAndParam, paramValues, templateIdentifier_ParamName, getConstantValue, iterationInfo);
            else if (i == 0)
                return GetParamValueInternal(identifier, identifierAndParam.Substring(1), paramValues, templateIdentifier_ParamName, getConstantValue, iterationInfo);
            else
                return GetParamValueInternal(identifierAndParam.Substring(0, i), identifierAndParam.Substring(i + 1), paramValues, templateIdentifier_ParamName, getConstantValue, iterationInfo);
        }

        public static string GetParamValue(
                IReadOnlyDictionary<string, string?>? paramsDictionary,
                IReadOnlyDictionary<string, string?>? baseParamsDictionary,
                string paramName)
        {
            string? paramValue = paramsDictionary?.GetValueOrDefault(paramName);
            if (!String.IsNullOrEmpty(paramValue))
                return paramValue;
            paramValue = baseParamsDictionary?.GetValueOrDefault(paramName);
            if (!String.IsNullOrEmpty(paramValue))
                return paramValue;
            return @"";
        }

        public static string GetParamValue(
            string identifierAndParam, 
            IReadOnlyDictionary<string, string> paramValues, 
            string templateIdentifier_ParamName,
            string constantBase)
        {
            int i = identifierAndParam.IndexOf('.');
            if (i == -1)
                return @"";
            string identifier = identifierAndParam.Substring(0, i).Trim();
            string param_ = identifierAndParam.Substring(i + 1).Trim();

            Func<string, IterationInfo, string> getConstantValue = (string constant, IterationInfo iterationInfo) =>
            {
                return GetConstantValue(constant, identifier, paramValues, templateIdentifier_ParamName, constantBase, iterationInfo);
            };
            
            return GetParamValueInternal(
                identifier,
                param_,
                paramValues,
                templateIdentifier_ParamName,
                getConstantValue,
                new IterationInfo());
        }

        public static void GetChildPcObjects(PcObject pcObject, List<PcObject> pcObjects)
        {
            foreach (var childPcObject in pcObject.Children
                        .Where(po => !po._IsDeleted))
            {
                pcObjects.Add(childPcObject);
                GetChildPcObjects(childPcObject, pcObjects);
            }
        }

        public static async Task Create_Unit_BasePcObjectsAsync(
            string unitIdentifier,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,  
            string user,
            DbCache dbCache)
        {
            Serialization.SerializationRootObject serializationRootObject = new();
            serializationRootObject.BasePcObjects = new List<Serialization.BasePcObject>()
                {
                    new Serialization.BasePcObject()
                    {
                        Identifier = PazCheckConstants.BasePcObject_Unit_Template,
                        Unit = unitIdentifier,
                        Params = new List<Serialization.Param>() { new Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = Properties.Resources.BasePcObject_Unit_Title } }
                    },
                    new Serialization.BasePcObject()
                    {
                        Identifier = PazCheckConstants.BasePcObject_OtherArea_Template,
                        Unit = unitIdentifier,
                        Params = new List<Serialization.Param>() { new Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = Properties.Resources.BasePcObject_OtherArea_Title } }
                    },
                    new Serialization.BasePcObject()
                    {
                        Identifier = PazCheckConstants.BasePcObject_OtherItem_Template,
                        Unit = unitIdentifier,
                        Params = new List<Serialization.Param>() { new Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = Properties.Resources.BasePcObject_OtherItem_Title } }
                    }
                };
            if (serializationRootObject.BasePcObjects.Any(bo => !PazCheckDbHelper.CheckBasePcObject(bo, dbCache)))
                await SerializationHelper.ImportSerializationRootObjectAsync(
                        serializationRootObject,
                        new Serialization.ImportMetadata()
                        {
                            RootCollectionMode = Serialization.CollectionMode.Update,
                            ChildCollectionMode = Serialization.CollectionMode.Update,
                            DataCollectionMode = Serialization.CollectionMode.Update,
                        },
                        dbContextFactory,
                        user,
                        null,
                        CancellationToken.None,
                        NullJobProgress.Instance,                        
                        LoggersSet.Empty,
                        new Serialization.ImportSerializationRootObjectResult(),
                        preview: false);
        }
       
        public static (Serialization.PcObject, Serialization.PcObject) Create_Unit_Objects(
            Unit unit)
        {
            Serialization.PcObject unit_PcObject = new Serialization.PcObject()
            {
                Identifier = unit.Identifier,
                Params = new List<Serialization.Param>() 
                { 
                    new Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = unit.Title },
                    new Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_Unit_Template },
                    new Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = @"" } // Root object    
                },                
                Unit = unit.Identifier                
            };
            
            Serialization.PcObject otherArea_PcObject = new Serialization.PcObject()
            {
                Identifier = PazCheckConstants.PcObject_OtherArea,
                Params = new List<Serialization.Param>() 
                { 
                    new Serialization.Param { Name = PazCheckConstants.ParamName_Title, Value = Properties.Resources.PcObject_OtherArea_Title },
                    new Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_OtherArea_Template },
                    new Serialization.Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = unit_PcObject.Identifier } 
                },                
                Unit = unit.Identifier                
            };

            return (unit_PcObject, otherArea_PcObject);
        }        

        public static bool FilterByParams(IDictionary<string, string?> paramsDictionary, IEnumerable<Criterion> filteredCriterionCollection)
        {
            bool allCriteriaInfosList = true;
            
            foreach (var criteriaInfo in filteredCriterionCollection)
            {
                bool anyParams = false;

                foreach (var paramDesc in criteriaInfo.Temp_ParamDescs!)
                {
                    if (paramsDictionary.TryGetValue(paramDesc.Id, out string? value) && SszOperatorHelper.Compare(
                                value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList,
                                paramDesc.DataType))
                    {
                        anyParams = true;
                        break;
                    }
                }

                if (!anyParams)
                {
                    allCriteriaInfosList = false;
                    break;
                }
            }

            return allCriteriaInfosList;
        }

        public static bool FilterByParams(IDictionary<string, string?> paramsDictionary, IDictionary<string, string?> baseParamsDictionary, IEnumerable<Criterion> filteredCriterionCollection)
        {
            bool allCriteriaInfosList = true;
            
            foreach (var criteriaInfo in filteredCriterionCollection)
            {
                bool anyParams = false;

                foreach (var paramDesc in criteriaInfo.Temp_ParamDescs!)
                {
                    string? value;
                    if (!paramsDictionary.ContainsKey(paramDesc.Id) &&
                            baseParamsDictionary.TryGetValue(paramDesc.Id, out value) &&
                            SszOperatorHelper.Compare(
                                value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList,
                                paramDesc.DataType))
                    {
                        anyParams = true;
                        break;
                    }

                    if (paramsDictionary.TryGetValue(paramDesc.Id, out value) &&
                            SszOperatorHelper.Compare(
                                value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList,
                                paramDesc.DataType))
                    {
                        anyParams = true;
                        break;
                    }
                }

                if (!anyParams)
                {
                    allCriteriaInfosList = false;
                    break;
                }
            }

            return allCriteriaInfosList;
        }

        /// <summary>
        ///     TODO add templates support.
        /// </summary>
        /// <param name="paramsDictionary"></param>
        /// <param name="identifier"></param>
        /// <param name="filteredCriterionCollection"></param>
        /// <returns></returns>
        public static bool FilterByParams(IReadOnlyDictionary<string, string> paramsDictionary, string identifier, IEnumerable<Criterion> filteredCriterionCollection)
        {
            bool allCriteriaInfosList = true;
            
            foreach (var criteriaInfo in filteredCriterionCollection)
            {
                bool anyParams = false;

                foreach (var paramDesc in criteriaInfo.Temp_ParamDescs!)
                {
                    if (paramsDictionary.TryGetValue(identifier + "." + paramDesc.Id, out string? value) && SszOperatorHelper.Compare(
                                value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList,
                                paramDesc.DataType))
                    {
                        anyParams = true;
                        break;
                    }
                }

                if (!anyParams)
                {
                    allCriteriaInfosList = false;
                    break;
                }
            }

            return allCriteriaInfosList;
        }

        public static SszConverter? GetSszConverter(string? converterString)
        {
            if (String.IsNullOrEmpty(converterString))
                return null;
            SszConverter? sszConverter = null;
            foreach (var v in converterString.Split(PazCheckConstants.PartsSeparator, StringSplitOptions.None).Select(it => it.Trim()))
            {
                if (String.IsNullOrEmpty(v))
                    continue;

                if (sszConverter is null)
                    sszConverter = new SszConverter();

                var parts = v.Split(PazCheckConstants.ConverterPartsSeparator, StringSplitOptions.None);
                if (parts.Length == 1)
                    sszConverter.Statements.Add(new SszStatement(@"true", parts[0].Trim(), 0));
                else
                    sszConverter.Statements.Add(new SszStatement(parts[0].Trim(),
                        parts[1].Trim(), 0));
            }
            if (sszConverter is not null && sszConverter.Statements.Count == 0)
            {
                sszConverter = null;
            }
            return sszConverter;
        }

        public static void ParseInPart(
            string inPart, 
            out List<CaseInsensitiveOrderedDictionary<List<string>>> parsedJsonFilterInfo,
            out CaseInsensitiveOrderedDictionary<List<string>> partParsedJsonFilterInfo)
        {
            inPart = inPart.Trim();
            if (!(inPart.StartsWith("{") || inPart.StartsWith("["))) // Syntactic sugar                        
            {
                if (inPart.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                    inPart = "{ \"" + PazCheckConstants.QueryPartName_QType + "\": \"" + PazCheckConstants.QueryType_Value + "\", " +
                        "\"" + PazCheckConstants.QueryPartName_QString + "\": " + JsonConvert.ToString(inPart) + "}";
                else
                    inPart = "{ \"" + PazCheckConstants.QueryPartName_QType + "\": \"" + PazCheckConstants.QueryType_DataAccess + "\", " +
                        "\"" + PazCheckConstants.QueryPartName_QString + "\": " + JsonConvert.ToString(inPart) + "}";
            }            

            try
            {
                // OR list of filters
                parsedJsonFilterInfo = FilterHelper.Parse(JsonDocument.Parse(inPart).RootElement);
                partParsedJsonFilterInfo = parsedJsonFilterInfo.First();

                var nowUtc = DateTime.UtcNow;

                if (!partParsedJsonFilterInfo.ContainsKey(PazCheckConstants.CriterionName_From))
                    partParsedJsonFilterInfo.Add(PazCheckConstants.CriterionName_From, new() { @"now" });
                if (!partParsedJsonFilterInfo.ContainsKey(PazCheckConstants.CriterionName_To))
                    partParsedJsonFilterInfo.Add(PazCheckConstants.CriterionName_To, new() { @"now" });
            }
            catch
            {
                partParsedJsonFilterInfo = new();
                parsedJsonFilterInfo = new() { partParsedJsonFilterInfo };
            }
        }

        public static (string stored_ParamName, Func<float, float>? trendCalculatedConversion) GetJournalParamValuesCollections_Info(            
            PcObject pcObject,
            string paramName,
            ref int iterationCount)
        {
            iterationCount += 1;
            if (iterationCount >= 10)
                return (paramName, null);

            string journalParamValuesCollections_ParamName = paramName;
            Func<float, float>? trendCalculatedConversion = null;
            var journalParam = pcObject.JournalParams.FirstOrDefault(c => String.Equals(paramName, c.ParamName, StringComparison.InvariantCultureIgnoreCase));
            if (journalParam is not null)
            {
                var journalParam_MetadataFieldsDictionary = journalParam.MetadataFieldsDictionary;
                bool trendCalculated = ConfigurationHelper.GetValue<bool>(journalParam_MetadataFieldsDictionary, PazCheckConstants.ParamName_TrendCalculated, false);
                if (trendCalculated)
                {
                    var in_ = journalParam_MetadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_In);
                    if (!String.IsNullOrEmpty(in_) && !in_.Contains(PazCheckConstants.PartsSeparator))
                    {
                        // OR list of filters
                        List<CaseInsensitiveOrderedDictionary<List<string>>> parsedJsonFilterInfo_Sub;
                        // AND dictionary of filters
                        CaseInsensitiveOrderedDictionary<List<string>> partParsedJsonFilterInfo_Sub;

                        ParseInPart(
                            in_,
                            out parsedJsonFilterInfo_Sub,
                            out partParsedJsonFilterInfo_Sub);

                        partParsedJsonFilterInfo_Sub.Remove(PazCheckConstants.QueryPartName_QType, out List<string>? queryType_Values);
                        partParsedJsonFilterInfo_Sub.Remove(PazCheckConstants.QueryPartName_QString, out List<string>? queryString_Values);

                        string? queryType_UpperCase_Sub = queryType_Values?.FirstOrDefault()?.ToUpperInvariant();
                        if ((queryType_UpperCase_Sub == PazCheckConstants.QueryType_Values_UpperCase ||
                                queryType_UpperCase_Sub == PazCheckConstants.QueryType_Value_UpperCase) &&
                            queryString_Values is not null &&
                            queryString_Values.Count == 1)
                        {   
                            journalParamValuesCollections_ParamName = queryString_Values[0];

                            (journalParamValuesCollections_ParamName, Func<float, float>? child_TrendCalculatedConversion) = GetJournalParamValuesCollections_Info(
                                pcObject,
                                journalParamValuesCollections_ParamName,
                                ref iterationCount);

                            var converter = journalParam_MetadataFieldsDictionary.TryGetValue(PazCheckConstants.ParamName_Converter);
                            var value_Converter = GetSszConverter(converter);
                            if (value_Converter is not null)
                                trendCalculatedConversion = v =>
                                {
                                    try
                                    {
                                        if (child_TrendCalculatedConversion is not null)
                                            v = child_TrendCalculatedConversion(v);

                                        var convertedValue = value_Converter.Convert(
                                            [v],
                                            null,
                                            LoggersSet.Empty);
                                        if (convertedValue != SszConverter.DoNothing)
                                            v = new Any(convertedValue).ValueAsSingle(false);
                                    }
                                    catch
                                    {
                                    }
                                    return v;
                                };
                            else if (child_TrendCalculatedConversion is not null)
                                trendCalculatedConversion = child_TrendCalculatedConversion;
                        }
                    }
                }
            }

            return (journalParamValuesCollections_ParamName, trendCalculatedConversion);
        }

        public static string GetMetaParamName(string[] metaParamNameParts)
        {
            string metaParamName = @"";
            foreach (var metaParamNamePart in metaParamNameParts)
            {
                metaParamName += metaParamNamePart + PazCheckConstants.MetaParamName_FieldsSeparator;
            }
            return metaParamName;
        }

        public static Regex? GetRegexOrNull(string? value)
        {
            if (!String.IsNullOrEmpty(value) && 
                    (value.StartsWith("{") || value.EndsWith("}")))
            {
                string regexString = value.Substring(1, value.Length - 2).Trim();
                return new Regex(regexString, RegexOptions.IgnoreCase);
            }
            else if (!String.IsNullOrEmpty(value) &&
                    (value.Contains("*") || value.Contains("?")))
            {
                string regexString = Regex.Escape(value);
                regexString = regexString.Replace("\\*", ".*").Replace("\\?", ".");
                regexString = "^" + regexString + "$";
                return new Regex(regexString, RegexOptions.IgnoreCase);
            }
            else
            {
                return null;
            }
        }

        public static bool IsMatchCaseInsensitive(string? input, string? value, Regex? value_Regex)
        {
            if (input is null)
                return false;

            if (value_Regex is not null)
                return value_Regex.IsMatch(input);

            return String.Equals(input, value, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region private functions

        private static IOrderedEnumerable<T> GetOrdered<T>(
                IReadOnlyDictionary<string, string> paramValues,
                bool needOrdering,
                IEnumerable<T> collection,
                Filter preparedFilter,
                Func<IEnumerable<T>, IOrderedEnumerable<T>> defaultOrdering)
            where T : ProjectVersionedEntityBase
        {
            if (!needOrdering)
                return new DummyOrderedEnumerable<T>(collection); // return collection;
            if (String.IsNullOrEmpty(preparedFilter.OrderBy))
                return defaultOrdering(collection);
            var orderByParts = CsvHelper.ParseCsvLine(@",", preparedFilter.OrderBy);
            var isOrderByDescendingParts = CsvHelper.ParseCsvLine(@",", preparedFilter.IsOrderByDescending);
            foreach (int i in Enumerable.Range(0, orderByParts.Length))
            {
                string? orderBy = orderByParts[i];
                if (String.IsNullOrEmpty(orderBy))
                    continue;
                bool isOrderByDescending = false;
                if (i < isOrderByDescendingParts.Length)
                    isOrderByDescending = new Any(isOrderByDescendingParts[i]).ValueAsBoolean(false);

                var propertyInfo = typeof(T).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, orderBy, StringComparison.InvariantCultureIgnoreCase));                    
                if (propertyInfo is not null)
                {
                    if (isOrderByDescending)
                        collection = collection.OrderByDescending(x => propertyInfo.GetValue(x, null));
                    else
                        collection = collection.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    if (isOrderByDescending)
                        collection = collection.OrderByDescending(x => paramValues.GetValueOrDefault(x.Identifier + "." + orderBy));
                    else
                        collection = collection.OrderBy(x => paramValues.GetValueOrDefault(x.Identifier + "." + orderBy));
                }
            }
            return (collection as IOrderedEnumerable<T>)!;
        }

        private static IOrderedEnumerable<T> GetOrdered<T>(
                Func<T, IReadOnlyDictionary<string, string?>> forEnitity_ParamValues,
                bool needOrdering,
                IEnumerable<T> collection,
                Filter preparedFilter,
                Func<IEnumerable<T>, IOrderedEnumerable<T>> defaultOrdering)            
        {
            if (!needOrdering)
                return new DummyOrderedEnumerable<T>(collection); // return collection;
            if (String.IsNullOrEmpty(preparedFilter.OrderBy))
                return defaultOrdering(collection);
            var orderByParts = CsvHelper.ParseCsvLine(@",", preparedFilter.OrderBy);
            var isOrderByDescendingParts = CsvHelper.ParseCsvLine(@",", preparedFilter.IsOrderByDescending);
            foreach (int i in Enumerable.Range(0, orderByParts.Length))
            {
                string? orderBy = orderByParts[i];
                if (String.IsNullOrEmpty(orderBy))
                    continue;
                bool isOrderByDescending = false;
                if (i < isOrderByDescendingParts.Length)
                    isOrderByDescending = new Any(isOrderByDescendingParts[i]).ValueAsBoolean(false);

                var propertyInfo = typeof(T).GetProperty(orderBy, BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo is not null)
                {
                    if (isOrderByDescending)
                        collection = collection.OrderByDescending(x => propertyInfo.GetValue(x, null));
                    else
                        collection = collection.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    if (isOrderByDescending)
                        collection = collection.OrderByDescending(x => forEnitity_ParamValues(x).GetValueOrDefault(orderBy));
                    else
                        collection = collection.OrderBy(x => forEnitity_ParamValues(x).GetValueOrDefault(orderBy));
                }
            }
            return (collection as IOrderedEnumerable<T>)!;
        }

        private static IOrderedEnumerable<T> GetOrdered<T>(            
            bool needOrdering,
            IEnumerable<T> collection,
            Filter preparedFilter,
            Func<IEnumerable<T>, IOrderedEnumerable<T>> defaultOrdering)
        {
            if (!needOrdering)
                return new DummyOrderedEnumerable<T>(collection); // return collection;
            if (String.IsNullOrEmpty(preparedFilter.OrderBy))
                return defaultOrdering(collection);
            var orderByParts = CsvHelper.ParseCsvLine(@",", preparedFilter.OrderBy);
            var isOrderByDescendingParts = CsvHelper.ParseCsvLine(@",", preparedFilter.IsOrderByDescending);
            foreach (int i in Enumerable.Range(0, orderByParts.Length))
            {
                string? orderBy = orderByParts[i];
                if (String.IsNullOrEmpty(orderBy))
                    continue;
                bool isOrderByDescending = false;
                if (i < isOrderByDescendingParts.Length)
                    isOrderByDescending = new Any(isOrderByDescendingParts[i]).ValueAsBoolean(false);

                var propertyInfo = typeof(T).GetProperty(orderBy, BindingFlags.Public | BindingFlags.Instance)!;
                if (isOrderByDescending)
                    collection = collection.OrderByDescending(x => propertyInfo.GetValue(x, null));
                else
                    collection = collection.OrderBy(x => propertyInfo.GetValue(x, null));
            }
            return (collection as IOrderedEnumerable<T>)!;
        }

        /// <summary>
        ///     toTimeUtc == null, if current time
        /// </summary>
        /// <param name="nowUtc"></param>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="pcObject"></param>
        /// <param name="filteredCriterionCollection"></param>
        /// <param name="toTimeUtc"></param>
        /// <returns></returns>
        private static bool FilterByJournalParams(DateTime nowUtc, PazCheckDbContext readOnlyDbContext, PcObject pcObject, IEnumerable<Criterion> filteredCriterionCollection, DateTime? toTimeUtc)
        {
            if (toTimeUtc is not null && toTimeUtc.Value > nowUtc - TimeSpan.FromSeconds(5))
                toTimeUtc = null;

            bool allCriteriaInfosList = true;

            foreach (var criteriaInfo in filteredCriterionCollection)
            {
                bool anyParams = false;

                foreach (var journalParamDesc in criteriaInfo.Temp_ParamDescs!)
                {
                    var journalParamValuesCollection = pcObject.JournalParamValuesCollections
                        .FirstOrDefault(vc => String.Equals(journalParamDesc.Id, vc.ParamName, StringComparison.InvariantCultureIgnoreCase));

                    if (journalParamValuesCollection is null)
                        continue;

                    if (toTimeUtc is null)
                    {
                        if (journalParamValuesCollection.CurrentValue is not null && SszOperatorHelper.Compare(
                                journalParamValuesCollection.CurrentValue.Value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList))
                        {
                            anyParams = true;
                            break;
                        }
                    }
                    else
                    {
                        long endTimeUtc_Optimized = new DateTimeOffset(toTimeUtc.Value).ToUnixTimeMilliseconds();
                        FloatJournalParamValue? fromJournalParamValue = readOnlyDbContext.FloatJournalParamValues
                            .Where(v => v.JournalParamValuesCollectionId == journalParamValuesCollection.Id && v.TimestampUtc <= endTimeUtc_Optimized)
                            .OrderByDescending(v => v.TimestampUtc)
                            .FirstOrDefault();

                        if (fromJournalParamValue is not null && SszOperatorHelper.Compare(
                                fromJournalParamValue.Value,
                                criteriaInfo.Temp_SszOperator,
                                criteriaInfo.Temp_SszOperatorOptions,
                                criteriaInfo.ValuesList))
                        {
                            anyParams = true;
                            break;
                        }
                    }
                }

                if (!anyParams)
                {
                    allCriteriaInfosList = false;
                    break;
                }
            }

            return allCriteriaInfosList;            
        }

        /// <summary>
        ///     Object search priority:
        ///     <para>1. CriterionName_RootPcObject_Identifier (Root), CriterionName_PcObject_Identifier (Identifier)</para>
        ///     <para>2. filter.ParentEntity</para>
        ///     <para>3. filter.ParentEntityId</para>
        ///     <para>toTimeUtc is null, if current time</para>
        ///     <para>CriterionName_Children default is False</para>
        /// </summary>        
        /// <param name="readOnlyDbContext"></param>
        /// <param name="dbCache"></param>
        /// <param name="filter"></param>        
        /// <param name="toTimeUtc"></param>
        /// <param name="currentObject"></param>        
        /// <returns></returns>
        private static List<PcObject> FilterPcObjects(
            PazCheckDbContext readOnlyDbContext,
            DbCache dbCache,
            Filter filter,            
            DateTime? toTimeUtc,
            object? currentObject)
        {
            var partCriterionInfosCollection = filter.CriterionCollection?.FirstOrDefault();

            PcObject? pcObject = null;

            var unitIdentifier = partCriterionInfosCollection
                                ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Unit, StringComparison.InvariantCultureIgnoreCase))
                                ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();
            var pcObjectIdentifier = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Identifier, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault();

            if (!String.IsNullOrEmpty(unitIdentifier) &&
                    !String.IsNullOrEmpty(pcObjectIdentifier))
            {
                dbCache.PcObjectsDictionary1.TryGetValue(unitIdentifier + "." + pcObjectIdentifier, out pcObject);
            }
            else
            {
                if (pcObject is null &&
                        filter.ParentEntityId.HasValue &&
                        currentObject is PcObject currentPcObject &&
                        currentPcObject.Id == filter.ParentEntityId.Value)
                    pcObject = currentPcObject;

                if (pcObject is null && filter.ParentEntityId.HasValue)
                    dbCache.PcObjectsDictionary2.TryGetValue(filter.ParentEntityId.Value, out pcObject);
            }

            IEnumerable<PcObject> pcObjects;
            if (pcObject is not null)
            {
                pcObjects = new List<PcObject> { pcObject };
                var children = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_Children, StringComparison.InvariantCultureIgnoreCase))
                            ?.FirstOrDefault()?.ValuesList?.FirstOrDefault() ?? "false";
                if (new Any(children).ValueAsBoolean(false))
                    GetChildPcObjects(pcObject, (List<PcObject>)pcObjects);
            }
            else
            {
                pcObjects = new List<PcObject>();
            }

            if (!String.IsNullOrEmpty(filter.SearchString) &&
                                    String.Equals(filter.SearchBy, Properties.Resources.SearchBy_PcObjectAll, StringComparison.InvariantCultureIgnoreCase))
                pcObjects = pcObjects
                    .Where(po => po.Identifier.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                         po.ParamsDictionary.Any(kvp =>
                                        (String.Equals(kvp.Key, PazCheckConstants.ParamName_Title, StringComparison.InvariantCultureIgnoreCase) || String.Equals(kvp.Key, PazCheckConstants.ParamName_Desc, StringComparison.InvariantCultureIgnoreCase)) &&
                                        (kvp.Value?.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)));

            var basePcObject_Identifier_CriterionCollection = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_PcObject_Template, StringComparison.InvariantCultureIgnoreCase))
                            ?.ToList();
            if (basePcObject_Identifier_CriterionCollection?.Count > 0)
            {
                var criterion = basePcObject_Identifier_CriterionCollection[0];
                pcObjects = pcObjects.Where(po => SszOperatorHelper.Compare(po.BasePcObject.Identifier, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList));                
            }

            var parentPcObject_Identifier_CriterionCollection = partCriterionInfosCollection
                            ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_PcObject_Parent, StringComparison.InvariantCultureIgnoreCase))
                            ?.ToList();
            if (parentPcObject_Identifier_CriterionCollection?.Count > 0)
            {
                var criterion = parentPcObject_Identifier_CriterionCollection[0];
                pcObjects = pcObjects.Where(po => SszOperatorHelper.Compare(po.Parent?.Identifier, criterion.Temp_SszOperator, criterion.Temp_SszOperatorOptions, criterion.ValuesList));
            }

            DateTime nowUtc = DateTime.UtcNow;

            var filteredCriterionCollection1 = partCriterionInfosCollection
                ?.Where(ci => ci.CriterionName.StartsWith(PazCheckConstants.CriterionName_Values + "[") && ci.CriterionName.EndsWith("]"))
                ?.ToArray();
            if (filteredCriterionCollection1?.Length > 0)
                pcObjects = pcObjects.Where(po => FilterByJournalParams(nowUtc, readOnlyDbContext, po, filteredCriterionCollection1, toTimeUtc));

            var filteredCriterionCollection2 = partCriterionInfosCollection
                ?.Where(ci => ci.CriterionName.StartsWith(PazCheckConstants.CriterionName_Params + "[") && ci.CriterionName.EndsWith("]"))
                ?.ToArray();
            if (filteredCriterionCollection2?.Length > 0)
                pcObjects = pcObjects.Where(po => FilterByParams(po.ParamsDictionary, po.BasePcObject.ParamsDictionary, filteredCriterionCollection2));

            return pcObjects.ToList();
        }

        /// <summary>
        ///     fromTimeUtc == null, if current time
        ///     toTimeUtc == null, if current time
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="pcObjects"></param>
        /// <param name="filter"></param>
        /// <param name="fromTimeUtc"></param>
        /// <param name="toTimeUtc"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<PcObjectEvent>> FilterPcObjectEventsAsync(
            PazCheckDbContext readOnlyDbContext,
            List<PcObject> pcObjects,
            Filter filter,
            DateTime? fromTimeUtc,
            DateTime? toTimeUtc)
        {
            if (fromTimeUtc is not null && fromTimeUtc.Value > DateTime.UtcNow - TimeSpan.FromSeconds(5))
                fromTimeUtc = null;
            if (toTimeUtc is not null && toTimeUtc.Value > DateTime.UtcNow - TimeSpan.FromSeconds(5))
                toTimeUtc = null;

            var partCriterionInfosCollection = filter.CriterionCollection?.FirstOrDefault();

            IEnumerable<PcObjectEvent> pcObjectEvents;
            if (fromTimeUtc is null && toTimeUtc is null) // Optimization
            {
                pcObjectEvents = new List<PcObjectEvent>(1000);

                var eventTypesLower = partCriterionInfosCollection
                                ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_EventType, StringComparison.InvariantCultureIgnoreCase))
                                ?.FirstOrDefault()?.ValuesList
                                ?.Select(et => et.ToLowerInvariant())
                                ?.ToArray();
                if (eventTypesLower is not null)
                {
                    if (eventTypesLower.Length == 1)
                    {
                        foreach (var pcObject in pcObjects)
                        {
                            ((List<PcObjectEvent>)pcObjectEvents).AddRange(pcObject.PcObjectEvents.Where(e => e._IsDeleted == false && e.EndTimeUtc == null && e.PcObjectEventTypeLower == eventTypesLower[0]));
                        }
                    }   
                    else if (eventTypesLower.Length > 1)
                    {
                        foreach (var pcObject in pcObjects)
                        {
                            ((List<PcObjectEvent>)pcObjectEvents).AddRange(pcObject.PcObjectEvents.Where(e => e._IsDeleted == false && e.EndTimeUtc == null && eventTypesLower.Contains(e.PcObjectEventTypeLower)));
                        }
                    }
                }
                else
                {
                    foreach (var pcObject in pcObjects)
                    {
                        ((List<PcObjectEvent>)pcObjectEvents).AddRange(pcObject.PcObjectEvents.Where(e => e._IsDeleted == false && e.EndTimeUtc == null));
                    }
                }              
            }
            else
            {
                IQueryable<PcObjectEvent> q;
                if (fromTimeUtc is null)
                {
                    if (pcObjects.Count == 1)
                    {
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && poe.PcObjectId == pcObjects[0].Id)
                            .Where(e => e.EndTimeUtc == null);
                    }
                    else
                    {
                        var pcObjectIds = pcObjects.Select(po => po.Id).ToList();
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && pcObjectIds.Contains(poe.PcObjectId))
                            .Where(e => e.EndTimeUtc == null);
                    }
                }
                else if (toTimeUtc is null)
                {
                    if (pcObjects.Count == 1)
                    {
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && poe.PcObjectId == pcObjects[0].Id)
                            .Where(e => e.BeginTimeUtc >= fromTimeUtc ||
                                    (e.EndTimeUtc != null && e.EndTimeUtc >= fromTimeUtc) ||
                                    (e.BeginTimeUtc < fromTimeUtc && e.EndTimeUtc == null));
                    }
                    else
                    {
                        var pcObjectIds = pcObjects.Select(po => po.Id).ToList();
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && pcObjectIds.Contains(poe.PcObjectId))
                            .Where(e => e.BeginTimeUtc >= fromTimeUtc ||
                                    (e.EndTimeUtc != null && e.EndTimeUtc >= fromTimeUtc) ||
                                    (e.BeginTimeUtc < fromTimeUtc && e.EndTimeUtc == null));
                    }
                }
                else
                {
                    if (pcObjects.Count == 1)
                    {
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && poe.PcObjectId == pcObjects[0].Id)
                            .Where(e => (e.BeginTimeUtc >= fromTimeUtc && e.BeginTimeUtc <= toTimeUtc) ||
                                    (e.EndTimeUtc != null && e.EndTimeUtc >= fromTimeUtc && e.EndTimeUtc <= toTimeUtc) ||
                                    (e.BeginTimeUtc < fromTimeUtc && (e.EndTimeUtc == null || e.EndTimeUtc > toTimeUtc)));
                    }
                    else
                    {
                        var pcObjectIds = pcObjects.Select(po => po.Id).ToList();
                        q = readOnlyDbContext.PcObjectEvents.Where(poe => poe._IsDeleted == false && pcObjectIds.Contains(poe.PcObjectId))
                            .Where(e => (e.BeginTimeUtc >= fromTimeUtc && e.BeginTimeUtc <= toTimeUtc) ||
                                    (e.EndTimeUtc != null && e.EndTimeUtc >= fromTimeUtc && e.EndTimeUtc <= toTimeUtc) ||
                                    (e.BeginTimeUtc < fromTimeUtc && (e.EndTimeUtc == null || e.EndTimeUtc > toTimeUtc)));
                    }
                }                    

                var eventTypesLower = partCriterionInfosCollection
                                ?.Where(ci => String.Equals(ci.CriterionName, PazCheckConstants.CriterionName_EventType, StringComparison.InvariantCultureIgnoreCase))
                                ?.FirstOrDefault()?.ValuesList
                                ?.Select(et => et.ToLowerInvariant())
                                ?.ToArray();
                if (eventTypesLower is not null)
                {                    
                    if (eventTypesLower.Length == 1)
                        q = q.Where(e => e.PcObjectEventTypeLower == eventTypesLower[0]);
                    else if (eventTypesLower.Length > 1)
                        q = q.Where(e => eventTypesLower.Contains(e.PcObjectEventTypeLower));
                }

                pcObjectEvents = await q.ToListAsync();
            }
            
            if (!String.IsNullOrEmpty(filter.SearchString))
            {
                if (String.Equals(filter.SearchBy, Common.Properties.Resources.SearchBy_PcObjectEventAll, StringComparison.InvariantCultureIgnoreCase))
                {
                    pcObjectEvents = pcObjectEvents.Where(poe =>
                                        poe.PcObjectEventType.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase) ||
                                        poe.ParamsDictionary.Any(kvp =>
                                            (String.Equals(kvp.Key, PazCheckConstants.ParamName_EventTitle, StringComparison.InvariantCultureIgnoreCase) || 
                                                String.Equals(kvp.Key, PazCheckConstants.ParamName_EventDesc, StringComparison.InvariantCultureIgnoreCase) ||
                                                String.Equals(kvp.Key, PazCheckConstants.ParamName_EventTypeTitle, StringComparison.InvariantCultureIgnoreCase) ||
                                                String.Equals(kvp.Key, PazCheckConstants.ParamName_EventTypeDesc, StringComparison.InvariantCultureIgnoreCase)) &&
                                            (kvp.Value?.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)));
                }
                else if (String.Equals(filter.SearchBy, Common.Properties.Resources.SearchBy_EventDesc, StringComparison.InvariantCultureIgnoreCase))
                {
                    pcObjectEvents = pcObjectEvents.Where(poe =>
                                        poe.ParamsDictionary.Any(kvp =>
                                            String.Equals(kvp.Key, PazCheckConstants.ParamName_EventDesc, StringComparison.InvariantCultureIgnoreCase) &&
                                            (kvp.Value?.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase) ?? false)));
                }
            }

            var filteredCriterionCollection = partCriterionInfosCollection
                ?.Where(ci => ci.CriterionName.StartsWith(PazCheckConstants.CriterionName_EventParams + "[") && ci.CriterionName.EndsWith("]"))
                ?.ToArray();
            if (filteredCriterionCollection?.Length > 0)
                return pcObjectEvents
                            .Where(poe => FilterByParams(poe.ParamsDictionary, filteredCriterionCollection));
            else
                return pcObjectEvents;                        
        }        

        private static string GetParamValueInternal(
            string identifier,
            string param_,
            IReadOnlyDictionary<string, string> paramValues, 
            string templateParamName,
            Func<string, IterationInfo, string>? getConstantValue,
            IterationInfo iterationInfo)
        {
            iterationInfo.IterationN += 1;
            if (iterationInfo.IterationN > 64)
                return @"";

            string? v = paramValues.GetValueOrDefault(identifier + @"." + param_);
            if (String.IsNullOrEmpty(v))
            {
                string? templateIdentifier = paramValues.GetValueOrDefault(identifier + @"." + templateParamName);
                if (!String.IsNullOrEmpty(templateIdentifier))
                    v = GetParamValueInternal(templateIdentifier, param_, paramValues, templateParamName, getConstantValue, iterationInfo);
            }
            else
            {
                v = SszQueryHelper.ComputeValueOfSszQueries(v, getConstantValue, null, iterationInfo);
            }
            return v ?? @"";
        }        

        private static void GatTemplateTag_TagConditions(
                    ProjectAllParamValues projectAllParamValues,                    
                    string tagName,
                    Func<string, IterationInfo, string>? getConstantValue,
                    CsvDb csvDb,
                    IterationInfo iterationInfo,
                    List<Serialization.TagCondition> resultTagConditionsList,                    
                    ILoggersSet loggersSet)
        {
            iterationInfo.IterationN += 1;
            if (iterationInfo.IterationN >= 64)
            {
                loggersSet.UserFriendlyLogger.LogWarning(Properties.Resources.TemplateTagsLooped);
                return;
            }

            var template = projectAllParamValues.TagsParamValues.GetValueOrDefault(tagName + @"." + PazCheckConstants.ParamName_TagTemplate);

            if (!String.IsNullOrEmpty(template))
            {
                GatTemplateTag_TagConditions(
                        projectAllParamValues,                        
                        template,
                        getConstantValue,
                        csvDb,
                        iterationInfo,
                        resultTagConditionsList,                        
                        loggersSet);

                GatTag_TagConditions(
                       projectAllParamValues,
                       template,
                       getConstantValue,
                       csvDb,
                       iterationInfo,
                       resultTagConditionsList);
            }
        }

        private static void GatTag_TagConditions(            
            ProjectAllParamValues projectAllParamValues,
            string tagName,
            Func<string, IterationInfo, string>? getConstantValue,
            CsvDb csvDb,
            IterationInfo iterationInfo,
            List<Serialization.TagCondition> resultTagConditionsList)
        {
            var tagConditions = projectAllParamValues.TagConditions.GetValueOrDefault(tagName);
            if (tagConditions is null)
            {
                //using var detailsScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_Details, tagName));
                //loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.Error_TagNotFoundInDb, tagName);
                return;
            }

            resultTagConditionsList.AddRange(tagConditions.Select(tc => new Serialization.TagCondition
            {
                ConditionCategory = tc.ConditionCategory,
                AeCondition = SszQueryHelper.ComputeValueOfSszQueries(tc.AeCondition, getConstantValue, csvDb, iterationInfo),
                DaCondition = SszQueryHelper.ComputeValueOfSszQueries(tc.DaCondition, getConstantValue, csvDb, iterationInfo),
                CanBeCause = tc.CanBeCause,
                CanBeEffect = tc.CanBeEffect,
                SymbolToDisplay = tc.SymbolToDisplay
            }));
        }                              

        #endregion
    }
}

///// <summary>
/////     Writes warnings to log: no tag or multiple tags.
///// </summary>
///// <param name="readOnlyDbContext"></param>
///// <param name="projectId"></param>
///// <param name="projectVersionNum"></param>
///// <param name="tagName"></param>
///// <param name="loggersSet"></param>
///// <returns></returns>
//public static Tag? GetTagOrNull(PazCheckDbContext readOnlyDbContext, int projectId, UInt32? projectVersionNum, string tagName, ILoggersSet loggersSet)
//{
//    if (String.IsNullOrEmpty(tagName))
//        return null;

//    var tags = readOnlyDbContext.Tags.Where(GetVersionEntityPredicate<Tag>(projectVersionNum, projectId, tagName))
//        .ToList();
//    if (tags.Count == 0)
//    {
//        loggersSet.UserFriendlyLogger.LogWarning(Properties.Resources.Error_TagNotFoundInDb, tagName);
//        return null;
//    }
//    if (tags.Count > 1)
//        loggersSet.UserFriendlyLogger.LogWarning(Properties.Resources.Error_MultipleTagsFoundInDb, tagName);
//    return tags[0];
//}

//public static BasePcObject GetBasePcObject(
//            PazCheckDbContext dbContext,
//            string basePcObject_Identifier,
//            string basePcObject_Title)
//{
//    var basePcObject = dbContext.BasePcObjects
//        .OrderBy(bo => bo._IsDeleted)
//        .ThenByDescending(bo => bo._LastChangeTimeUtc)
//        .FirstOrDefault(bo => bo.Identifier == basePcObject_Identifier);
//    if (basePcObject is null)
//    {
//        basePcObject = new()
//        {
//            Identifier = basePcObject_Identifier,
//        };
//        dbContext.BasePcObjects.Add(basePcObject);
//    }
//    else
//    {
//        basePcObject.Identifier = basePcObject_Identifier; // For case-sensivity issues                
//        basePcObject._IsDeleted = false;
//    }
//    var paramsDictionary = basePcObject.ParamsDictionary;
//    paramsDictionary[PazCheckCentralServerConstants.ParamName_Title] = basePcObject_Title;
//    basePcObject.ParamsDictionary = paramsDictionary;

//    return basePcObject;
//}

//public static PcObject GetPcObject(
//    PazCheckDbContext dbContext,
//    string parent_PcObject_Identifier,
//    string basePcObject_Identifier,
//    string basePcObject_Title,
//    string pcObject_Identifier,
//    string pcObject_Title)
//{
//    var basePcObject = GetBasePcObject(
//        dbContext,
//        basePcObject_Identifier,
//        basePcObject_Title);

//    PcObject? parent_PcObject = dbContext.PcObjects
//        .Where(po => !po._IsDeleted)
//        .FirstOrDefault(po => po.Identifier == parent_PcObject_Identifier);

//    var pcObject = dbContext.PcObjects
//        .OrderBy(po => po._IsDeleted)
//        .ThenByDescending(bo => bo._LastChangeTimeUtc)
//        .FirstOrDefault(po => po.Identifier == pcObject_Identifier);
//    if (pcObject is null)
//    {
//        pcObject = new()
//        {
//            Identifier = pcObject_Identifier,
//            BasePcObject = basePcObject,
//            Parent = parent_PcObject
//        };
//        dbContext.PcObjects.Add(pcObject);
//    }
//    else
//    {
//        pcObject.Identifier = pcObject_Identifier; // For case-sensivity issues                
//        pcObject.BasePcObject = basePcObject;
//        pcObject.Parent = parent_PcObject;
//        pcObject._IsDeleted = false;
//    }
//    var paramsDictionary = pcObject.ParamsDictionary;
//    paramsDictionary[PazCheckCentralServerConstants.ParamName_Title] = pcObject_Title;
//    pcObject.ParamsDictionary = paramsDictionary;

//    return pcObject;
//}

//public static (BasePcObject, PcObject) GetOther_BaseAndParentObjects(
//    PazCheckDbContext dbContext,
//    string rootPcObject_Identifier)
//{
//    string parent_BasePcObject_Identifier = CentralServer.Common.PazCheckCentralServerConstants.BasePcObject_OtherArea;
//    string parent_BasePcObject_Title = CentralServer.Common.Properties.Resources.BasePcObject_OtherArea_Title;
//    string basePcObject_Identifier = CentralServer.Common.PazCheckCentralServerConstants.BasePcObject_OtherItem;
//    string basePcObject_Title = CentralServer.Common.Properties.Resources.BasePcObject_OtherItem_Title;
//    string parent_PcObject_Identifier = CentralServer.Common.PazCheckCentralServerConstants.PcObject_OtherArea;
//    string parent_PcObject_Title = CentralServer.Common.Properties.Resources.PcObject_OtherArea_Title;

//    var area_BasePcObject = dbContext.BasePcObjects
//        .OrderBy(bo => bo._IsDeleted)
//        .ThenByDescending(bo => bo._LastChangeTimeUtc)
//        .FirstOrDefault(bo => bo.Identifier == parent_BasePcObject_Identifier);
//    if (area_BasePcObject is null)
//    {
//        area_BasePcObject = new()
//        {
//            Identifier = parent_BasePcObject_Identifier,
//        };
//        dbContext.BasePcObjects.Add(area_BasePcObject);
//    }
//    else
//    {
//        area_BasePcObject.Identifier = parent_BasePcObject_Identifier; // For case-sensivity issues                
//        area_BasePcObject._IsDeleted = false;
//    }
//    var paramsDictionary = area_BasePcObject.ParamsDictionary;
//    paramsDictionary[PazCheckCentralServerConstants.ParamName_Title] = parent_BasePcObject_Title;
//    area_BasePcObject.ParamsDictionary = paramsDictionary;

//    var item_BasePcObject = dbContext.BasePcObjects
//        .OrderBy(bo => bo._IsDeleted)
//        .ThenByDescending(bo => bo._LastChangeTimeUtc)
//        .FirstOrDefault(bo => bo.Identifier == basePcObject_Identifier);
//    if (item_BasePcObject is null)
//    {
//        item_BasePcObject = new()
//        {
//            Identifier = basePcObject_Identifier,
//        };
//        dbContext.BasePcObjects.Add(item_BasePcObject);
//    }
//    else
//    {
//        item_BasePcObject.Identifier = basePcObject_Identifier; // For case-sensivity issues                
//        item_BasePcObject._IsDeleted = false;
//    }
//    paramsDictionary = item_BasePcObject.ParamsDictionary;
//    paramsDictionary[PazCheckCentralServerConstants.ParamName_Title] = basePcObject_Title;
//    item_BasePcObject.ParamsDictionary = paramsDictionary;

//    PcObject? root_PcObject = dbContext.PcObjects
//        .Where(po => !po._IsDeleted)
//        .FirstOrDefault(po => po.Identifier == rootPcObject_Identifier);

//    var area_PcObject = dbContext.PcObjects
//        .OrderBy(po => po._IsDeleted)
//        .ThenByDescending(bo => bo._LastChangeTimeUtc)
//        .FirstOrDefault(po => po.Identifier == parent_PcObject_Identifier);
//    if (area_PcObject is null)
//    {
//        area_PcObject = new()
//        {
//            Identifier = parent_PcObject_Identifier,
//            BasePcObject = area_BasePcObject,
//            Parent = root_PcObject
//        };
//        dbContext.PcObjects.Add(area_PcObject);
//    }
//    else
//    {
//        area_PcObject.Identifier = parent_PcObject_Identifier; // For case-sensivity issues                
//        area_PcObject.BasePcObject = area_BasePcObject;
//        area_PcObject.Parent = root_PcObject;
//        area_PcObject._IsDeleted = false;
//    }
//    paramsDictionary = area_PcObject.ParamsDictionary;
//    paramsDictionary[PazCheckCentralServerConstants.ParamName_Title] = parent_PcObject_Title;
//    area_PcObject.ParamsDictionary = paramsDictionary;

//    dbContext.SaveChanges();

//    return (item_BasePcObject, area_PcObject);
//}