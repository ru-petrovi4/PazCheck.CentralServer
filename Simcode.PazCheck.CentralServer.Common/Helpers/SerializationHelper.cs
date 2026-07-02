using Microsoft.EntityFrameworkCore;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static Simcode.PazCheck.CentralServer.Common.Helpers.SerializationHelper;
using System.Reflection;
using System.Threading;
using Ssz.Utils.Logging;
using IdentityServer4;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class SerializationHelper
    {
        #region public functions        

        public static string? RemoveHtml(string? value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return value;

            value = value.Replace("<strong>", "");
            value = value.Replace("</strong>", "");
            value = value.Replace("<br>", "");
            value = value.Replace("  ", " ");

            return value;
        }

        public static string? GetNullIfEmptyString(string? value)
        {
            if (value == @"") return null;
            return value;
        }

        public static DateTime GetDateTimeUtc(string stdTimeUtcString)
        {
            if (stdTimeUtcString.EndsWith(@"Z", StringComparison.InvariantCultureIgnoreCase))
                stdTimeUtcString = stdTimeUtcString.Substring(0, stdTimeUtcString.Length - 1);
            DateTime.TryParseExact(stdTimeUtcString, @"s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime time);            
            return time.ToUniversalTime();
        }

        public static async Task<(SerializationRootObject, ProjectEntities_Temp)> GetSerializationRootObjectAsync(
            ProjectEntitiesCollectionInfo projectEntitiesCollectionInfo, 
            PazCheckDbContext readOnlyDbContext,
            DbCache dbCache,
            IJobProgress jobProgress)
        {
            var projectVersionNum = projectEntitiesCollectionInfo.ProjectVersionNum;

            ProjectEntities_Temp projectEntities_Temp = new();
            
            projectEntities_Temp.ProjectAllParamValues = await dbCache.GetProjectAllParamValuesAsync(
                projectEntitiesCollectionInfo.ProjectId,
                projectVersionNum,
                readOnlyDbContext,
                LoggersSet.Empty);

            await jobProgress.SetJobProgressAsync(0, null, null, Ssz.Utils.StatusCodes.Good);

            if (projectEntitiesCollectionInfo.CeMatrices is not null)
            {
                if (projectVersionNum is null)
                {
                    projectEntities_Temp.RequestedCeMatrices = await readOnlyDbContext.CeMatrices
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.CeMatrix>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.CeMatrices,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.CeMatrixDbFileReferences.Where(p => p._IsDeleted == false))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.CeMatrix?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }
                else
                {
                    projectEntities_Temp.RequestedCeMatrices = await readOnlyDbContext.CeMatrices
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.CeMatrix>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.CeMatrices,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.CeMatrixDbFileReferences.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.CeMatrix?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }                              
            }

            await jobProgress.SetJobProgressAsync(20, null, null, Ssz.Utils.StatusCodes.Good);

            if (projectEntitiesCollectionInfo.Tags is not null)
            {
                if (projectVersionNum is null)
                {
                    projectEntities_Temp.RequestedTags = await readOnlyDbContext.Tags
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.Tag>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.Tags,
                            readOnlyDbContext,
                            dbCache))
                        .Include(t => t.TagConditions.Where(p => p._IsDeleted == false))
                        .Include(t => t.TagDbFileReferences.Where(p => p._IsDeleted == false))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.Tag?)i, StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    projectEntities_Temp.RequestedTags = await readOnlyDbContext.Tags
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.Tag>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.Tags,
                            readOnlyDbContext,
                            dbCache))
                        .Include(t => t.TagConditions.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .Include(t => t.TagDbFileReferences.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.Tag?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }                
            }

            await jobProgress.SetJobProgressAsync(40, null, null, Ssz.Utils.StatusCodes.Good);

            if (projectEntitiesCollectionInfo.BaseActuators is not null)
            {
                if (projectVersionNum is null)
                {
                    projectEntities_Temp.RequestedBaseActuators = await readOnlyDbContext.BaseActuators
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.BaseActuator>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.BaseActuators,
                            readOnlyDbContext,
                            dbCache))
                        .Include(ba => ba.BaseActuatorDbFileReferences.Where(p => p._IsDeleted == false))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.BaseActuator?)i, StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    projectEntities_Temp.RequestedBaseActuators = await readOnlyDbContext.BaseActuators
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.BaseActuator>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.BaseActuators,
                            readOnlyDbContext,
                            dbCache))
                        .Include(ba => ba.BaseActuatorDbFileReferences.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.BaseActuator?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }                
            }

            await jobProgress.SetJobProgressAsync(60, null, null, Ssz.Utils.StatusCodes.Good);

            if (projectEntitiesCollectionInfo.SafetyControllers is not null)
            {
                if (projectVersionNum is null)
                {
                    projectEntities_Temp.RequestedSafetyControllers = await readOnlyDbContext.SafetyControllers
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.SafetyController>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.SafetyControllers,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.SafetyControllerDbFileReferences.Where(p => p._IsDeleted == false))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.SafetyController?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }
                else
                {
                    projectEntities_Temp.RequestedSafetyControllers = await readOnlyDbContext.SafetyControllers
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.SafetyController>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.SafetyControllers,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.SafetyControllerDbFileReferences.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.SafetyController?)i, StringComparer.InvariantCultureIgnoreCase);
                }                
            }

            await jobProgress.SetJobProgressAsync(80, null, null, Ssz.Utils.StatusCodes.Good);

            if (projectEntitiesCollectionInfo.Legends is not null)
            {
                if (projectVersionNum is null)
                {
                    projectEntities_Temp.RequestedLegends = await readOnlyDbContext.Legends
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.Legend>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.Legends,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.LegendDbFileReferences.Where(p => p._IsDeleted == false))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.Legend?)i, StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    projectEntities_Temp.RequestedLegends = await readOnlyDbContext.Legends
                        .Where(await PazCheckDbHelper.GetProjectVersionedEntity_PredicateAsync<Common.EntityFramework.Legend>(
                            projectEntitiesCollectionInfo.ProjectId,
                            projectVersionNum,
                            projectEntitiesCollectionInfo.Legends,
                            readOnlyDbContext,
                            dbCache))
                        .Include(sc => sc.LegendDbFileReferences.Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                                (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum)))
                        .ThenInclude(fr => fr.DbFile)
                        .ThenInclude(f => f!.DbFileContent)
                        .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.Legend?)i, StringComparer.InvariantCultureIgnoreCase);                    
                }                
            }

            await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.Good);

            foreach (var ceMatrix in projectEntities_Temp.RequestedCeMatrices.Values)
            {
                Serialization.CeMatrix serializationCeMatrix = GetSerializationCeMatrix(ceMatrix!, projectVersionNum, projectEntities_Temp, dbCache);
                projectEntities_Temp.SerializationCeMatrices.Add(serializationCeMatrix);
            }

            foreach (var tag in projectEntities_Temp.RequestedTags.Values)
            {
                Serialization.Tag serializationTag = GetSerializationTag(tag!, projectVersionNum, projectEntities_Temp, dbCache);
                projectEntities_Temp.SerializationTags.Add( serializationTag);
            }

            foreach (var baseActuator in projectEntities_Temp.RequestedBaseActuators.Values)
            {
                Serialization.Actuator serializationBaseActuator = GetSerializationBaseActuator(baseActuator!, projectVersionNum, projectEntities_Temp, dbCache);
                projectEntities_Temp.SerializationBaseActuators.Add(serializationBaseActuator);
            }

            foreach (var safetyController in projectEntities_Temp.RequestedSafetyControllers.Values)
            {
                Serialization.MonitoringObject serializationSafetyController = GetSerializationSafetyController(safetyController!, projectVersionNum, projectEntities_Temp, dbCache);
                projectEntities_Temp.SerializationSafetyControllers.Add(serializationSafetyController);
            }

            foreach (var legend in projectEntities_Temp.RequestedLegends.Values)
            {
                Serialization.Legend serializationLegend = GetSerializationLegend(legend!, projectVersionNum, projectEntities_Temp, dbCache);
                projectEntities_Temp.SerializationLegends.Add(serializationLegend);
            }

            Serialization.SerializationRootObject serializationRootObject = new();

            if (projectEntities_Temp.DbFilesDictionary.Count > 0)
                serializationRootObject.DbFiles = projectEntities_Temp.DbFilesDictionary.Values.OrderBy(i => i.OriginalFileName).ToList();

            if (projectEntities_Temp.ParamDescsDictionary.Count > 0)
                serializationRootObject.ParamDescs = projectEntities_Temp.ParamDescsDictionary.Values.OrderBy(i => i.Name).ToList();            

            if (projectEntities_Temp.SerializationCeMatrices.Count > 0)
                serializationRootObject.CeMatrices = projectEntities_Temp.SerializationCeMatrices.OrderBy(i => i.Identifier).ToList();

            if (projectEntities_Temp.SerializationTags.Count > 0)
                serializationRootObject.Tags = projectEntities_Temp.SerializationTags.OrderBy(i => i.Identifier).ToList();

            if (projectEntities_Temp.SerializationBaseActuators.Count > 0)
                serializationRootObject.Actuators = projectEntities_Temp.SerializationBaseActuators.OrderBy(i => i.Identifier).ToList();

            if (projectEntities_Temp.SerializationSafetyControllers.Count > 0)
                serializationRootObject.MonitoringObjects = projectEntities_Temp.SerializationSafetyControllers.OrderBy(i => i.Identifier).ToList();

            if (projectEntities_Temp.SerializationLegends.Count > 0)
                serializationRootObject.Legends = projectEntities_Temp.SerializationLegends.OrderBy(i => i.Identifier).ToList();            

            return (serializationRootObject, projectEntities_Temp);
        }

        public static async Task<SerializationRootObject> GetSerializationRootObjectAsync(
            EntitiesCollectionInfo entitiesCollectionInfo,
            string entitiesName, 
            PazCheckDbContext readOnlyDbContext,
            DbCache dbCache)
        {
            Serialization.SerializationRootObject serializationRootObject = new();

            if (String.Equals(entitiesName, PazCheckConstants.ExportEntitiesName_ReferenceEntities, StringComparison.InvariantCultureIgnoreCase))
            {
                serializationRootObject.Units = new();
                foreach (var unit in await readOnlyDbContext.Units
                    .Where(po => po._IsDeleted == false)
                    .ToListAsync())
                {
                    serializationRootObject.Units.Add(new Serialization.Unit()
                    {
                        Identifier = unit.Identifier,
                        Title = unit.Title,
                        Desc = unit.Desc,
                    });
                }

                serializationRootObject.ProjectVersionTypes = new();
                foreach (var projectVersionType in await readOnlyDbContext.ProjectVersionTypes
                    .ToListAsync())
                {
                    serializationRootObject.ProjectVersionTypes.Add(GetSerializationType<Common.EntityFramework.ProjectVersionType, Serialization.ProjectVersionType>(projectVersionType));
                }

                serializationRootObject.PcObjectEventTypes = new();
                foreach (var projectVersionType in await readOnlyDbContext.PcObjectEventTypes
                    .ToListAsync())
                {
                    serializationRootObject.PcObjectEventTypes.Add(GetSerializationType<Common.EntityFramework.PcObjectEventType, Serialization.PcObjectEventType>(projectVersionType));
                }

                serializationRootObject.ParamDescs = new();
                foreach (var paramDesc in await readOnlyDbContext.ParamDescs                    
                    .ToListAsync())
                {
                    serializationRootObject.ParamDescs.Add(new Serialization.ParamDesc()
                    {
                        Name = paramDesc.Id,
                        Desc = paramDesc.Desc,
                        Details = paramDesc.Details,
                        Priority = paramDesc.Priority,
                        DataType = paramDesc.DataType,
                        MetadataFields = paramDesc.MetadataFields
                    });
                }
            }
            else
            {
                PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName, out PropertyInfo? pazCheckDbContext_PropertyInfo);
                if (pazCheckDbContext_PropertyInfo is null)
                    return serializationRootObject;
                Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();

                MonitoringEntities_Temp monitoringEntities_Temp = new();

                if (entityType == typeof(Common.EntityFramework.PcObject))
                {
                    monitoringEntities_Temp.RequestedPcObjects = await readOnlyDbContext.PcObjects
                            .Where(PazCheckDbHelper.GetEntity_Predicate<Common.EntityFramework.PcObject>(
                                entitiesCollectionInfo.IncludeAll,
                                entitiesCollectionInfo.IdsToInclude,
                                entitiesCollectionInfo.IdsToExclude))
                            .Where(po => po._IsDeleted == false)
                            .Include(sc => sc.Unit)
                            .Include(sc => sc.BasePcObject)
                            .Include(sc => sc.Parent)
                            .Include(sc => sc.PcObjectDbFileReferences)
                            .ThenInclude(fr => fr.DbFile)
                            .ThenInclude(f => f!.DbFileContent)
                            .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.PcObject?)i, StringComparer.InvariantCultureIgnoreCase);

                    foreach (var pcObject in monitoringEntities_Temp.RequestedPcObjects.Values)
                    {
                        Serialization.PcObject serializationPcObject = GetSerializationPcObject(pcObject!, monitoringEntities_Temp, dbCache);
                        monitoringEntities_Temp.SerializationPcObjects.Add(serializationPcObject);
                    }
                }

                if (entityType == typeof(Common.EntityFramework.BasePcObject))
                {
                    monitoringEntities_Temp.RequestedBasePcObjects = await readOnlyDbContext.BasePcObjects
                            .Where(PazCheckDbHelper.GetEntity_Predicate<Common.EntityFramework.BasePcObject>(
                                entitiesCollectionInfo.IncludeAll,
                                entitiesCollectionInfo.IdsToInclude,
                                entitiesCollectionInfo.IdsToExclude))
                            .Where(bpo => bpo._IsDeleted == false)
                            .Include(sc => sc.Unit)
                            .Include(sc => sc.BasePcObjectDbFileReferences)
                            .ThenInclude(fr => fr.DbFile)
                            .ThenInclude(f => f!.DbFileContent)
                            .ToDictionaryAsync(i => i.Identifier, i => (EntityFramework.BasePcObject?)i, StringComparer.InvariantCultureIgnoreCase);

                    foreach (var basePcObject in monitoringEntities_Temp.RequestedBasePcObjects.Values)
                    {
                        Serialization.BasePcObject serializationBasePcObject = GetSerializationBasePcObject(basePcObject!, monitoringEntities_Temp, dbCache);
                        monitoringEntities_Temp.SerializationBasePcObjects.Add(serializationBasePcObject);
                    }
                }

                if (monitoringEntities_Temp.SerializationPcObjects.Count > 0)
                    serializationRootObject.PcObjects = monitoringEntities_Temp.SerializationPcObjects.OrderBy(i => i.Identifier).ToList();

                if (monitoringEntities_Temp.SerializationBasePcObjects.Count > 0)
                    serializationRootObject.BasePcObjects = monitoringEntities_Temp.SerializationBasePcObjects.OrderBy(i => i.Identifier).ToList();

                if (monitoringEntities_Temp.DbFilesDictionary.Count > 0)
                    serializationRootObject.DbFiles = monitoringEntities_Temp.DbFilesDictionary.Values.OrderBy(i => i.OriginalFileName).ToList();

                if (monitoringEntities_Temp.ParamDescsDictionary.Count > 0)
                    serializationRootObject.ParamDescs = monitoringEntities_Temp.ParamDescsDictionary.Values.OrderBy(i => i.Name).ToList();

                if (monitoringEntities_Temp.PcObjectEventTypesDictionary.Count > 0)
                    serializationRootObject.PcObjectEventTypes = monitoringEntities_Temp.PcObjectEventTypesDictionary.Values.OrderBy(i => i.Type).ToList();
            }                                     

            return serializationRootObject;
        }

        public static async Task SetCurrentProjectVersionAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,                        
            int projectId,
            UInt32 projectVersionNum,
            int newProjectId,
            CancellationToken cancellationToken,
            IJobProgress jobProgress,
            InformationSecurityContext informationSecurityContext,
            IMainServerWorker mainServerWorker,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILoggersSet loggersSet)
        {            
            string projectTitle = @"";
            string newProjectTitle = @"";
            string projectVersionComment = @"";

            await using PazCheckDbContext metaParams_DbContext = dbContextFactory.CreateDbContext();
            metaParams_DbContext.User = informationSecurityContext.User;
            metaParams_DbContext.IsInformationSecurityEventsLoggingDisabled = true;

            var metaParams = metaParams_DbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
            PazCheckDbHelper.AddOrUpdateMetaParam_Pause_HubMethod_Project_Changed(
                    metaParams_DbContext,
                    metaParams,
                    projectId,
                    isPaused: true);
            await metaParams_DbContext.SaveChangesAsync();

            List<MemoryStream> ceMatrices_CsvMemoryStreams = new();
            try
            {
                var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                Common.Serialization.SerializationRootObject serializationRootObject;                
                Common.Serialization.ImportSerializationRootObjectResult result;

                {
                    await using PazCheckDbContext dbContext = dbContextFactory.CreateDbContext();
                    dbContext.User = informationSecurityContext.User;
                    dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                    Project? project = dbContext.Projects
                        .Include(p => p.Unit)
                        .SingleOrDefault(pv => pv.Id == projectId);
                    if (project is null)
                    {
                        await previewJobProgress.SetJobProgressAsync(0, Common.Properties.Resources.Error, String.Format(@"Invalid projectId: {0}", projectId), Ssz.Utils.StatusCodes.BadInvalidArgument);
                        return;
                    }
                    projectTitle = project.Title;

                    Project? newProject = dbContext.Projects
                        .Include(p => p.Unit)
                        .SingleOrDefault(pv => pv.Id == newProjectId);
                    if (newProject is null)
                    {
                        await previewJobProgress.SetJobProgressAsync(0, Common.Properties.Resources.Error, String.Format(@"Invalid projectId: {0}", newProjectId), Ssz.Utils.StatusCodes.BadInvalidArgument);
                        return;
                    }
                    newProjectTitle = newProject.Title;

                    ProjectVersion? projectVersion = dbContext.ProjectVersions.SingleOrDefault(pv => pv.ProjectId == projectId && pv.VersionNum == projectVersionNum);
                    if (projectVersion is null)
                    {
                        loggersSet.Logger.LogError(@"Invalid projectVersionNum: {0}", projectVersionNum);
                        await previewJobProgress.SetJobProgressAsync(0, Common.Properties.Resources.Error, String.Format(@"Invalid projectVersionNum: {0}", projectVersionNum), Ssz.Utils.StatusCodes.BadInvalidArgument);
                        return;
                    }
                    projectVersionComment = projectVersion.Comment;

                    CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                    if (ceMatrixRuntimeAddon is null)
                    {
                        loggersSet.Logger.LogError(Properties.Resources.InvalidAddonType, nameof(CeMatrixRuntimeAddonBase));
                        await previewJobProgress.SetJobProgressAsync(0, Properties.Resources.Error, String.Format(Properties.Resources.InvalidAddonType, nameof(CeMatrixRuntimeAddonBase)), Ssz.Utils.StatusCodes.BadInvalidState);
                        return;
                    }

                    ProjectEntitiesCollectionInfo projectEntitiesCollectionInfo = new()
                    {
                        ProjectId = projectId,
                        ProjectVersionNum = projectVersionNum,
                        CeMatrices = new EntitiesCollectionInfo { IncludeAll = true },
                        Tags = new EntitiesCollectionInfo { IncludeAll = true },
                        BaseActuators = new EntitiesCollectionInfo { IncludeAll = true },
                        SafetyControllers = new EntitiesCollectionInfo { IncludeAll = true },
                        Legends = new EntitiesCollectionInfo { IncludeAll = true },
                        FullExport_ProjectVersion = true
                    };

                    var dbCache = mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache;

                    var childJobProgress = await previewJobProgress.GetChildJobProgressAsync(0, 50, parentFailedIfFailed: true);

                    (serializationRootObject, ProjectEntities_Temp projectEntities_Temp) =
                        await GetSerializationRootObjectAsync(
                            projectEntitiesCollectionInfo,
                            dbContext,
                            dbCache,
                            childJobProgress
                            );                    

                    if (serializationRootObject.CeMatrices?.Count > 0)
                    {
                        foreach (var serializationCeMatrix in serializationRootObject.CeMatrices)
                        {
                            EntityFramework.CeMatrix? ceMatrix = await dbContext.CeMatrices
                                .FirstOrDefaultAsync(PazCheckDbHelper.GetVersionEntityPredicate<EntityFramework.CeMatrix>(
                                    projectEntitiesCollectionInfo.ProjectVersionNum,
                                    projectEntitiesCollectionInfo.ProjectId,
                                    serializationCeMatrix.Identifier));

                            (string? fileName, XLWorkbook? workbook) = await ceMatrixRuntimeAddon.GetCeMatrixXLWorkbookAsync(
                                dbContext, 
                                ceMatrix!, 
                                projectEntitiesCollectionInfo.ProjectVersionNum, 
                                mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                                loggersSet, 
                                humanReadable: false);
                            if (workbook is not null)
                            {
                                var worksheet = workbook.Worksheets.First();
                                //using var scope3 = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                                MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                                ceMatrices_CsvMemoryStreams.Add(csvMemoryStream);

                                workbook.Dispose();
                            }
                        }
                    }                    

                    result = new();

                    childJobProgress = await previewJobProgress.GetChildJobProgressAsync(50, 75, parentFailedIfFailed: true);

                    await ImportSerializationRootObjectAsync(
                        serializationRootObject,
                        new Common.Serialization.ImportMetadata
                        {
                            RootCollectionMode = Common.Serialization.CollectionMode.Replace,
                            ChildCollectionMode = Common.Serialization.CollectionMode.Replace,
                            DataCollectionMode = Common.Serialization.CollectionMode.Update,
                        },
                        dbContextFactory,
                        informationSecurityContext.User,
                        newProjectId,
                        cancellationToken,
                        childJobProgress,                        
                        loggersSet,
                        result,
                        preview: true);

                    childJobProgress = await previewJobProgress.GetChildJobProgressAsync(75, 100, parentFailedIfFailed: true);

                    int i = 0;
                    foreach (var csvStream in ceMatrices_CsvMemoryStreams)
                    {
                        csvStream.Position = 0;

                        await ImportStdFileAsync(
                            dbContextFactory,
                            informationSecurityContext.User,
                            csvStream,
                            newProjectId,
                            cancellationToken,
                            await childJobProgress.GetChildJobProgressAsync((uint)(100.0 * i / ceMatrices_CsvMemoryStreams.Count), (uint)(100.0 * (i + 1) / ceMatrices_CsvMemoryStreams.Count), parentFailedIfFailed: true),
                            mainServerWorker,
                            loggersSet,
                            result,
                            preview: true);

                        i += 1;
                    }
                }

                if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                {
                    await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                    await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await using PazCheckDbContext dbContext = dbContextFactory.CreateDbContext();
                        dbContext.User = informationSecurityContext.User;
                        dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                        await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                        var mainJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                        result = new();

                        await ImportSerializationRootObjectAsync(
                            serializationRootObject,
                            new Common.Serialization.ImportMetadata
                            {
                                RootCollectionMode = Common.Serialization.CollectionMode.Replace,
                                ChildCollectionMode = Common.Serialization.CollectionMode.Replace,
                                DataCollectionMode = Common.Serialization.CollectionMode.Update,
                            },
                            dbContextFactory,
                            informationSecurityContext.User,
                            newProjectId,
                            cancellationToken,
                            await mainJobProgress.GetChildJobProgressAsync(0, 50, parentFailedIfFailed: true),                            
                            loggersSet,
                            result,
                            preview: false);

                        var childJobProgress = await mainJobProgress.GetChildJobProgressAsync(50, 100, parentFailedIfFailed: true);

                        int i = 0;
                        foreach (var csvStream in ceMatrices_CsvMemoryStreams)
                        {
                            csvStream.Position = 0;

                            await ImportStdFileAsync(
                                dbContextFactory,
                                informationSecurityContext.User,
                                csvStream,
                                newProjectId,
                                cancellationToken,
                                await childJobProgress.GetChildJobProgressAsync((uint)(100.0 * i / ceMatrices_CsvMemoryStreams.Count), (uint)(100.0 * (i + 1) / ceMatrices_CsvMemoryStreams.Count), parentFailedIfFailed: true),
                                mainServerWorker,
                                loggersSet,
                                result,
                                preview: false);

                            i += 1;
                        }

                        if (Ssz.Utils.StatusCodes.IsGood(mainJobProgress.StatusCode))
                            await mainJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                    }                        
                }
            }            
            finally
            {   
                foreach (var ceMatrix_CsvMemoryStream in ceMatrices_CsvMemoryStreams)
                {
                    ceMatrix_CsvMemoryStream.Dispose();
                }

                string newDataGuid = Guid.NewGuid().ToString();
                metaParams_DbContext.Database.ExecuteSql($"UPDATE \"Projects\" SET \"DataGuid\" = {newDataGuid} WHERE \"Id\" = {projectId}");

                PazCheckDbHelper.AddOrUpdateMetaParam_Pause_HubMethod_Project_Changed(
                        metaParams_DbContext,
                        metaParams,
                        projectId,
                        isPaused: false);
                await metaParams_DbContext.SaveChangesAsync();

                informationSecurityEventsLogger.InformationSecurityEvent(
                            informationSecurityContext.User,
                            informationSecurityContext.SourceIpAddress,
                            informationSecurityContext.SourceHost,
                            InformationSecurityEventsLogger.DataImported_AllRolesAccessEventId,
                            6,
                            StatusCodes.IsGood(jobProgress.StatusCode),
                            Properties.Resources.SetCurrentProjectVersion_EventName,
                            informationSecurityContext.User,
                            Common.Properties.Resources.Project + @": " + newProjectTitle,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                {
                                            (@"ProjectId", new Any(projectId).ValueAsString(false)),
                                            (@"ProjectVersion", new Any(projectVersionNum).ValueAsString(false)),
                                            (@"NewProjectId", new Any(newProjectId).ValueAsString(false)),
                                }),
                            Properties.Resources.SetCurrentProjectVersion_EventDesc, projectTitle, projectVersionNum, newProjectTitle, SerializationHelper.RemoveHtml(jobProgress.ProgressDetails));

            }
        }

        public static Serialization.ResultEvent GetSerializationResultEvent(EntityFramework.ResultEvent resultEvent, bool childResultEvent = false)
        {
            Serialization.ResultEvent serializationResultEvent = new()
            {
                TriggeredTimeUtc = resultEvent.TriggeredTimeUtc,
                CeMatrix_TagName = childResultEvent ? "<-" + resultEvent.TagName : resultEvent.TagName,
                Type = PazCheckDbHelper.GetEventTypeString(resultEvent.Type),
                ConditionCategory = resultEvent.ConditionCategory,
                AeCondition = resultEvent.AeCondition,
                DaCondition = resultEvent.DaCondition,
                TriggeredType = GetResultEventTriggeredTypeString(resultEvent.TriggeredType),
                NewValue = resultEvent.NewValue,
            };

            serializationResultEvent.Params = new();
            foreach (var f in NameValueCollectionHelper.ParseExact(resultEvent.Params))
            {
                serializationResultEvent.Params.Add(new Param()
                {
                    Name = f.Item1,
                    Value = f.Item2 ?? @""
                });
            }

            if (resultEvent.TriggeredUnitEvent is not null)
            {
                serializationResultEvent.Log = new();
                foreach (var f in NameValueCollectionHelper.ParseExact(resultEvent.TriggeredUnitEvent.OriginalEvent))
                {
                    serializationResultEvent.Log.Add(new Param()
                    {
                        Name = f.Item1,
                        Value = f.Item2 ?? @""
                    });
                }
            }

            return serializationResultEvent;
        }        

        #endregion

        #region private functions                

        private static Serialization.CeMatrix GetSerializationCeMatrix(
            Common.EntityFramework.CeMatrix ceMatrix,
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp,
            DbCache dbCache)
        {
            Serialization.CeMatrix serializationCeMatrix = new()
            {
                Identifier = ceMatrix.Identifier,
                //Params = GetSerializationParams(projectEntities_Temp.ProjectAllParamValues.CeMatricesParams[ceMatrix.Identifier], projectEntities_Temp, dbCache), // Params export in .csv file
                DbFileReferences = GetSerializationDbFileReferences(ceMatrix.CeMatrixDbFileReferences, projectVersionNum, projectEntities_Temp),
                SourceEntity = ceMatrix
            };            

            return serializationCeMatrix;
        }        

        private static Serialization.Tag GetSerializationTag(
            Common.EntityFramework.Tag tag, 
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp,
            DbCache dbCache)
        {
            //string baseActuatorIdentifier = projectEntities_Temp.ProjectAllParamValues.TagsParamValues.GetValueOrDefault(tag.Identifier + @"." + PazCheckCentralServerConstants.ParamName_BaseActuator) ?? @"";
            //if (baseActuatorIdentifier != @"" && !projectEntities_Temp.RequestedBaseActuators.TryGetValue(baseActuatorIdentifier, out BaseActuator? baseActuator))
            //    projectEntities_Temp.RequestedBaseActuators.Add(baseActuatorIdentifier, null);

            Serialization.Tag serializationTag = new()
            {
                Identifier = tag.Identifier,                             
                Params = GetSerializationParams(projectEntities_Temp.ProjectAllParamValues.TagsParams[tag.Identifier], projectEntities_Temp, dbCache),
                TagConditions = GetSerializationTagConditions(tag.TagConditions, projectVersionNum, projectEntities_Temp),
                DbFileReferences = GetSerializationDbFileReferences(tag.TagDbFileReferences, projectVersionNum, projectEntities_Temp),
                SourceEntity = tag
            };            

            return serializationTag;
        }

        private static Serialization.Actuator GetSerializationBaseActuator(
            Common.EntityFramework.BaseActuator baseActuator, 
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp,
            DbCache dbCache)
        {            
            Serialization.Actuator serializationBaseActuator = new()
            {
                Identifier = baseActuator.Identifier,                                
                Params = GetSerializationParams(projectEntities_Temp.ProjectAllParamValues.BaseActuatorsParams[baseActuator.Identifier], projectEntities_Temp, dbCache),                
                DbFileReferences = GetSerializationDbFileReferences(baseActuator.BaseActuatorDbFileReferences, projectVersionNum, projectEntities_Temp),
                SourceEntity = baseActuator
            };            

            return serializationBaseActuator;
        }

        private static Serialization.MonitoringObject GetSerializationSafetyController(
            Common.EntityFramework.SafetyController safetyController,
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp,
            DbCache dbCache)
        {
            Serialization.MonitoringObject serializationSafetyController = new()
            {
                Identifier = safetyController.Identifier,                
                Params = GetSerializationParams(projectEntities_Temp.ProjectAllParamValues.SafetyControllersParams[safetyController.Identifier], projectEntities_Temp, dbCache),
                DbFileReferences = GetSerializationDbFileReferences(safetyController.SafetyControllerDbFileReferences, projectVersionNum, projectEntities_Temp),
                SourceEntity = safetyController
            };            

            return serializationSafetyController;
        }

        private static Serialization.Legend GetSerializationLegend(
            Common.EntityFramework.Legend legend,
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp,
            DbCache dbCache)
        {
            Serialization.Legend serializationLegend = new()
            {
                Identifier = legend.Identifier,
                Params = GetSerializationParams(projectEntities_Temp.ProjectAllParamValues.LegendsParams[legend.Identifier], projectEntities_Temp, dbCache),
                DbFileReferences = GetSerializationDbFileReferences(legend.LegendDbFileReferences, projectVersionNum, projectEntities_Temp),
                SourceEntity = legend
            };
            
            return serializationLegend;
        }        

        private static Serialization.PcObject GetSerializationPcObject(
            Common.EntityFramework.PcObject pcObject,            
            MonitoringEntities_Temp entities_Temp,
            DbCache dbCache)
        {
            //string basePcObjectIdentifier = pcObject.BasePcObject.Identifier;
            //if (basePcObjectIdentifier != @"" && !entities_Temp.RequestedBasePcObjects.TryGetValue(basePcObjectIdentifier, out BasePcObject? basePcObject))
            //{
            //    entities_Temp.AllBasePcObjects.TryGetValue(basePcObjectIdentifier, out EntityFramework.BasePcObject? basePcObject);
            //    if (basePcObject is not null)
            //    {
            //        serializationBasePcObject = GetSerializationBasePcObject(basePcObject, entities_Temp);
            //        entities_Temp.SerializationBasePcObjectsDictionary.Add(basePcObjectIdentifier, serializationBasePcObject);
            //    }
            //}

            Serialization.PcObject serializationPcObject = new()
            {
                Identifier = pcObject.Identifier,                
                Widgets = pcObject.Widgets,
                Unit = pcObject.Unit.Identifier,                
                Params = GetSerializationParams(pcObject.ParamsDictionary, pcObject.JournalParams, entities_Temp, dbCache),                
                PcObjectDbFileReferences = GetSerializationDbFileReferences(pcObject.PcObjectDbFileReferences, entities_Temp),
            };
            return serializationPcObject;
        }        

        private static Serialization.BasePcObject GetSerializationBasePcObject(
            Common.EntityFramework.BasePcObject basePcObject,            
            MonitoringEntities_Temp entities_Temp,
            DbCache dbCache)
        {
            Serialization.BasePcObject serializationBasePcObject = new()
            {
                Identifier = basePcObject.Identifier,                
                Widgets = basePcObject.Widgets,
                Unit = basePcObject.Unit.Identifier,
                Params = GetSerializationParams(basePcObject.ParamsDictionary, basePcObject.JournalParams, entities_Temp, dbCache),                
                BasePcObjectDbFileReferences = GetSerializationDbFileReferences(basePcObject.BasePcObjectDbFileReferences, entities_Temp),
            };            

            return serializationBasePcObject;
        }

        private static ST GetSerializationType<ET, ST>(ET entityType)
            where ET : CentralServer.Common.EntityFramework.PcEntityType
            where ST : CentralServer.Common.Serialization.PcEntityType, new()
        {
            ST st = new()
            {
                Type = entityType.Type,
                Title = entityType.Title,
                Desc = entityType.Desc,
                // TODO
                //IconDbFileReference = GetSerializationDbFiles(new List<DbFile>() { entityType.IconDbFile }, projectVersionNum, projectEntities_Temp),
                //StandardParamInfos = GetSerializationParamInfos()
            };
            return st;
        }

        private static List<Serialization.Param>? GetSerializationParams<TParam>(
                IEnumerable<TParam> params_,                
                ProjectEntities_Temp projectEntities_Temp,
                DbCache dbCache)
            where TParam : CentralServer.Common.EntityFramework.VersionedParamBase            
        {
            List<Serialization.Param> serializationParams = new();
            foreach (var param_ in params_)
            {
                Serialization.Param serializationParam = new()
                {
                    Name = param_.ParamName,
                    Value = param_.Value,
                };
                serializationParams.Add(serializationParam);

                dbCache.ParamDescs.TryGetValue(param_.ParamName, out EntityFramework.ParamDesc? paramDesc);
                serializationParam.ParamDesc = paramDesc;

                if (paramDesc is not null && !projectEntities_Temp.ParamDescsDictionary.ContainsKey(param_.ParamName))
                {                    
                    projectEntities_Temp.ParamDescsDictionary.Add(param_.ParamName, new Serialization.ParamDesc
                    {
                        Name = param_.ParamName,
                        Desc = paramDesc.Desc,
                        Details = paramDesc.Details,
                        Priority = paramDesc.Priority,
                        DataType = paramDesc.DataType,
                        MetadataFields = paramDesc.MetadataFields
                    });
                }
            }            
            if (serializationParams.Count == 0)
                return null;
            return serializationParams.OrderByDescending(sp => sp.ParamDesc?.Priority ?? 0).ToList();
        }        

        private static List<Param>? GetSerializationParams<TJournalParam>(
                IDictionary<string, string?> paramsDictionary,
                IEnumerable<TJournalParam> journalParams,
                MonitoringEntities_Temp entities_Temp,
                DbCache dbCache
            )
            where TJournalParam : CentralServer.Common.EntityFramework.JournalParam
        {
            List<Serialization.Param> serializationParams = new();
            foreach (var kvp in paramsDictionary)
            {
                string paramName = kvp.Key;
                string paramValue = kvp.Value ?? @"";
                Serialization.Param serializationParam = new()
                {
                    Name = paramName,
                    Value = paramValue,
                };
                serializationParams.Add(serializationParam);

                if (!entities_Temp.ParamDescsDictionary.ContainsKey(paramName) &&
                    dbCache.ParamDescs.TryGetValue(paramName, out EntityFramework.ParamDesc? paramDesc))
                {
                    entities_Temp.ParamDescsDictionary.Add(paramName, new Serialization.ParamDesc
                    {
                        Name = paramName,
                        Desc = paramDesc.Desc,
                        Details = paramDesc.Details,
                        Priority = paramDesc.Priority,
                        DataType = paramDesc.DataType,
                        MetadataFields = paramDesc.MetadataFields
                    });
                }
            }
            foreach (var journalParam in journalParams)
            {
                Serialization.Param serializationParam = new()
                {
                    Name = journalParam.ParamName,
                    Value = journalParam.MetadataFields,
                };
                serializationParams.Add(serializationParam);

                if (!entities_Temp.ParamDescsDictionary.ContainsKey(journalParam.ParamName) &&
                        dbCache.ParamDescs.TryGetValue(journalParam.ParamName, out EntityFramework.ParamDesc? paramDesc))
                {
                    entities_Temp.ParamDescsDictionary.Add(journalParam.ParamName, new Serialization.ParamDesc
                    {
                        Name = journalParam.ParamName,
                        Desc = paramDesc.Desc,
                        Details = paramDesc.Details,
                        Priority = paramDesc.Priority,
                        DataType = paramDesc.DataType,
                        MetadataFields = paramDesc.MetadataFields
                    });
                }
            }
            if (serializationParams.Count == 0)
                return null;
            return serializationParams;
        }

        private static List<Serialization.TagCondition>? GetSerializationTagConditions(
            List<CentralServer.Common.EntityFramework.TagCondition> tagConditions, 
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp)
        {
            List<Serialization.TagCondition> serializationTagConditions = new();
            List<CentralServer.Common.EntityFramework.TagCondition> tagConditionsInProjectVersion;
            if (projectVersionNum is null)
            {
                tagConditionsInProjectVersion = tagConditions
                        .Where(p => p._IsDeleted == false)
                        .ToList();
            }
            else
            {
                tagConditionsInProjectVersion = tagConditions
                        .Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                            (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum))
                        .ToList();
            }
            foreach (var tagCondition in tagConditionsInProjectVersion)
            {
                //if (!String.IsNullOrEmpty(tagCondition.SymbolToDisplay) &&
                //    !projectEntities_Temp.RequestedLegends.TryGetValue(tagCondition.SymbolToDisplay, out Legend? legend))
                //{
                //    if (projectEntities_Temp.AllLegends.TryGetValue(tagCondition.SymbolToDisplay, out EntityFramework.Legend? legend))
                //    {
                //        serializationLegend = GetSerializationLegend(legend, projectVersionNum, projectEntities_Temp);
                //        projectEntities_Temp.SerializationLegendsDictionary.Add(serializationLegend.Identifier, serializationLegend);
                //    }
                //}

                Serialization.TagCondition serializationTagCondition = new()
                {
                    AeCondition = tagCondition.AeCondition,                    
                    DaCondition = tagCondition.DaCondition,
                    ConditionCategory = tagCondition.ConditionCategory,
                    SymbolToDisplay = tagCondition.SymbolToDisplay,
                    CanBeCause = tagCondition.CanBeCause,
                    CanBeEffect = tagCondition.CanBeEffect
                };
                serializationTagConditions.Add(serializationTagCondition);                
            }
            if (serializationTagConditions.Count == 0)
                return null;
            return serializationTagConditions;
        }

        private static List<Serialization.DbFileReference>? GetSerializationDbFileReferences<TDbFileReference>(
            List<TDbFileReference> dbFileReferences, 
            UInt32? projectVersionNum,
            ProjectEntities_Temp projectEntities_Temp)
                where TDbFileReference : CentralServer.Common.EntityFramework.VersionDbFileReference
        {
            List<Serialization.DbFileReference> serializationDbFileReferences = new();
            List<TDbFileReference> dbFileReferenceInProjectVersion;
            if (projectVersionNum is null)
            {
                dbFileReferenceInProjectVersion = dbFileReferences
                        .Where(p => p._IsDeleted == false)
                        .ToList();
            }
            else
            {
                dbFileReferenceInProjectVersion = dbFileReferences
                        .Where(p => (p._CreateProjectVersionNum != null && p._CreateProjectVersionNum <= projectVersionNum) &&
                            (p._DeleteProjectVersionNum == null || p._DeleteProjectVersionNum > projectVersionNum))
                        .ToList();
            }
            foreach (var dbFileReference in dbFileReferenceInProjectVersion)
            {
                if (dbFileReference.DbFile is null || dbFileReference.FileBytesHash_Base64 is null)
                    continue;

                Serialization.DbFile? serializationDbFile;                
                if (!projectEntities_Temp.DbFilesDictionary.TryGetValue(dbFileReference.FileBytesHash_Base64, out serializationDbFile))
                {
                    serializationDbFile = new()
                    {
                        OriginalFileName = dbFileReference.Name,                        
                        FileBytesCount = dbFileReference.BytesCount,
                        FileBytesHash_Base64 = dbFileReference.FileBytesHash_Base64,
                        FileBytes_Base64 = dbFileReference.DbFile?.DbFileContent?.FileBytes_Base64 ?? @""
                    };
                    projectEntities_Temp.DbFilesDictionary.Add(dbFileReference.FileBytesHash_Base64, serializationDbFile);
                }
                Serialization.DbFileReference serializationDbFileReference = new()
                {
                    Name = dbFileReference.Name,
                    Path = dbFileReference.Path,
                    Tags = dbFileReference.Tags,
                    LastWriteTimeUtc = dbFileReference.LastWriteTimeUtc,
                    BytesCount = dbFileReference.BytesCount,
                    FileBytesHash_Base64 = dbFileReference.FileBytesHash_Base64,
                };
                serializationDbFileReferences.Add(serializationDbFileReference);
            }
            if (serializationDbFileReferences.Count == 0)
                return null;
            return serializationDbFileReferences;
        }

        private static List<Serialization.DbFileReference>? GetSerializationDbFileReferences<TDbFileReference>(
            List<TDbFileReference> dbFileReferences,
            MonitoringEntities_Temp entities_Temp)
                where TDbFileReference : CentralServer.Common.EntityFramework.DbFileReference
        {
            List<Serialization.DbFileReference> serializationDbFileReferences = new();
            
            foreach (var dbFileReference in dbFileReferences)
            {
                if (dbFileReference.DbFile is null || dbFileReference.FileBytesHash_Base64 is null)
                    continue;

                Serialization.DbFile? serializationDbFile;
                if (!entities_Temp.DbFilesDictionary.TryGetValue(dbFileReference.FileBytesHash_Base64, out serializationDbFile))
                {
                    serializationDbFile = new()
                    {
                        OriginalFileName = dbFileReference.Name,
                        FileBytesCount = dbFileReference.BytesCount,
                        FileBytesHash_Base64 = dbFileReference.FileBytesHash_Base64,
                        FileBytes_Base64 = dbFileReference.DbFile?.DbFileContent?.FileBytes_Base64 ?? @""
                    };
                    entities_Temp.DbFilesDictionary.Add(dbFileReference.FileBytesHash_Base64, serializationDbFile);
                }
                Serialization.DbFileReference serializationDbFileReference = new()
                {
                    Name = dbFileReference.Name,
                    Path = dbFileReference.Path,
                    Tags = dbFileReference.Tags,
                    LastWriteTimeUtc = dbFileReference.LastWriteTimeUtc,
                    BytesCount = dbFileReference.BytesCount,
                    FileBytesHash_Base64 = dbFileReference.FileBytesHash_Base64,
                };
                serializationDbFileReferences.Add(serializationDbFileReference);
            }
            if (serializationDbFileReferences.Count == 0)
                return null;
            return serializationDbFileReferences;
        }

        private static Serialization.UnitEvent GetSerializationUnitEvent(EntityFramework.UnitEvent unitEvent)
        {
            Serialization.UnitEvent serializationUnitEvent = new()
            {
                EventTimeUtc = unitEvent.EventTimeUtc,
                TagName = unitEvent.TagName,
                ConditionString = unitEvent.ConditionString,
                ConditionIsActive = unitEvent.ConditionIsActive,
                Priority = unitEvent.Priority,
                Message = unitEvent.Message,
                Params = new()
            };

            foreach (var f in NameValueCollectionHelper.ParseExact(unitEvent.OriginalEvent))
            {
                serializationUnitEvent.Params.Add(new Param()
                {
                    Name = f.Item1,
                    Value = f.Item2 ?? @""
                });
            }

            return serializationUnitEvent;
        }

        private static EntityFramework.UnitEvent GetUnitEvent(Serialization.UnitEvent serializationUnitEvent)
        {
            EntityFramework.UnitEvent unitEvent = new()
            {
                EventTimeUtc = serializationUnitEvent.EventTimeUtc,
                TagName = serializationUnitEvent.TagName,
                ConditionString = serializationUnitEvent.ConditionString,
                ConditionIsActive = serializationUnitEvent.ConditionIsActive,
                Priority = serializationUnitEvent.Priority,
                Message = serializationUnitEvent.Message,                
            };

            if (!String.IsNullOrEmpty(serializationUnitEvent.OriginalEventDictionary)) // Obsolete, for compatibility only
            {
                unitEvent.OriginalEvent = serializationUnitEvent.OriginalEventDictionary;
            }
            else
            {
                if (serializationUnitEvent.Params is not null)
                {
                    List<(string, string?)> originalEvents = new();
                    foreach (var p in serializationUnitEvent.Params)
                    {
                        originalEvents.Add((p.Name, p.Value));
                    }
                    unitEvent.OriginalEvent = NameValueCollectionHelper.GetNameValueCollectionString(originalEvents);
                }
            }

            return unitEvent;
        }

        private static Serialization.UserEvent GetSerializationUserEvent(EntityFramework.UserEvent userEvent)
        {
            Serialization.UserEvent serializationUserEvent = new()
            {
                EventTimeUtc = userEvent.EventTimeUtc,
                LogLevel = userEvent.LogLevel,                
                Message = userEvent.Message,                
                DetailsParams = new()
            };

            foreach (var f in NameValueCollectionHelper.ParseExact(userEvent.Details))
            {
                serializationUserEvent.DetailsParams.Add(new Param()
                {
                    Name = f.Item1,
                    Value = f.Item2 ?? @""
                });
            }

            return serializationUserEvent;
        }

        private static Serialization.InformationSecurityEvent GetSerializationInformationSecurityEvent(EntityFramework.InformationSecurityEvent informationSecurityEvent)
        {
            Serialization.InformationSecurityEvent serializationInformationSecurityEvent = new()
            {
                EventTimeUtc = informationSecurityEvent.EventTimeUtc,
                EventId = informationSecurityEvent.EventId,
                EventIdDesc = informationSecurityEvent.EventIdDesc,
                Severity = informationSecurityEvent.Severity,
                SeverityDesc = informationSecurityEvent.SeverityDesc,
                User = informationSecurityEvent.User,
                SourceIpAddress = informationSecurityEvent.SourceIpAddress,
                SourceHost = informationSecurityEvent.SourceHost,
                EventName = informationSecurityEvent.EventName,
                EventSubject = informationSecurityEvent.EventSubject,
                EventObject = informationSecurityEvent.EventObject,
                EventDesc = informationSecurityEvent.EventDesc,
                Succeeded = informationSecurityEvent.Succeeded,
                EventAdditionalFields = new()
            };

            foreach (var f in NameValueCollectionHelper.ParseExact(informationSecurityEvent.EventAdditionalFields))
            {
                serializationInformationSecurityEvent.EventAdditionalFields.Add(new Param()
                {
                    Name = f.Item1,
                    Value = f.Item2 ?? @""
                });
            }

            return serializationInformationSecurityEvent;
        }        

        private static string GetResultEventTriggeredTypeString(TriggeredType triggeredType)
        {
            switch (triggeredType)
            {
                case TriggeredType.NotActivated:
                    return @"";
                case TriggeredType.FaultTriggered:
                    return Properties.Resources.ParamValue_FaultTriggered;
                case TriggeredType.SuccessFirstTriggered:
                    return Properties.Resources.ParamValue_SuccessFirstTriggered;
                case TriggeredType.SuccessTriggered:
                    return Properties.Resources.ParamValue_SuccessTriggered;
                case TriggeredType.LateTriggered:
                    return Properties.Resources.ParamValue_LateTriggered;
                case TriggeredType.NotTriggered:
                    return Properties.Resources.ParamValue_NotTriggered;
                default:
                    return @"";
            }
        }

        #endregion

        public class ProjectEntities_Temp
        {            
            public Dictionary<string, Common.EntityFramework.CeMatrix?> RequestedCeMatrices = new();
            public Dictionary<string, Common.EntityFramework.Tag?> RequestedTags = new();
            public Dictionary<string, Common.EntityFramework.BaseActuator?> RequestedBaseActuators = new();
            public Dictionary<string, Common.EntityFramework.SafetyController?> RequestedSafetyControllers = new();
            public Dictionary<string, Common.EntityFramework.Legend?> RequestedLegends = new();

            public ProjectAllParamValues ProjectAllParamValues = null!;            

            public readonly List<Serialization.CeMatrix> SerializationCeMatrices = new();
            public readonly List<Serialization.Tag> SerializationTags = new();
            public readonly List<Serialization.Actuator> SerializationBaseActuators = new();
            public readonly List<Serialization.MonitoringObject> SerializationSafetyControllers = new();
            public readonly List<Serialization.Legend> SerializationLegends = new();

            public readonly Dictionary<string, Serialization.DbFile> DbFilesDictionary = new();
            public readonly Dictionary<string, Serialization.ParamDesc> ParamDescsDictionary = new();                    
        }

        public class MonitoringEntities_Temp
        {
            public Dictionary<string, Common.EntityFramework.PcObject?> RequestedPcObjects = new();
            public Dictionary<string, Common.EntityFramework.BasePcObject?> RequestedBasePcObjects = new();

            public readonly List<Serialization.PcObject> SerializationPcObjects = new();
            public readonly List<Serialization.BasePcObject> SerializationBasePcObjects = new();                    

            public readonly Dictionary<string, Serialization.DbFile> DbFilesDictionary = new();
            public readonly Dictionary<string, Serialization.ParamDesc> ParamDescsDictionary = new();
            public readonly Dictionary<string, Serialization.PcObjectEventType> PcObjectEventTypesDictionary = new();            
        }        
    }
}


//public readonly Dictionary<string, Serialization.BaseActuatorType> BaseActuatorTypesDictionary = new();
//public readonly Dictionary<string, Serialization.SafetyControllerType> SafetyControllerTypesDictionary = new();

//private static Serialization.DbFileReference? GetSerializationDbFileReference(Common.EntityFramework.DbFile? dbFile,
//    Dictionary<string, Serialization.DbFile> serializationDbFilesDictionary)
//{
//    if (dbFile is null || dbFile.DbFileContent is null)
//        return null;

//    Serialization.DbFile? serializationDbFile;            
//    if (!serializationDbFilesDictionary.TryGetValue(dbFile.FileBytesHash_Base64, out serializationDbFile))
//    {
//        serializationDbFile = new()
//        {
//            OriginalFileName = dbFile.OriginalFileName,
//            FileBytesCount = dbFile.FileBytesCount,
//            FileBytesHash_Base64 = dbFile.FileBytesHash_Base64,
//            FileBytes_Base64 = dbFile.DbFileContent?.FileBytes_Base64 ?? @""
//        };
//        serializationDbFilesDictionary.Add(dbFile.FileBytesHash_Base64, serializationDbFile);
//    }

//    Serialization.DbFileReference serializationDbFileReference = new()
//    {
//        FileName = dbFile.OriginalFileName,
//        FileBytesCount = dbFile.FileBytesCount,
//        FileBytesHash_Base64 = dbFile.FileBytesHash_Base64,
//    };
//    return serializationDbFileReference;
//}
