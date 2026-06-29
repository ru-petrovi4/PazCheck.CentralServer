using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices;

public class DbCache
{
    #region public functions        

    /// <summary>
    ///     [Param.Name, ParamDesc]
    /// </summary>
    public FrozenDictionary<string, ParamDesc> ParamDescs { get; set; } = new Dictionary<string, ParamDesc>().ToFrozenDictionary();

    /// <summary>
    ///     Filled in Monitoring.
    ///     Unit.ActiveProjectVersion.Project is included.
    ///     [Unit.Identifier, Unit]
    /// </summary>
    public FrozenDictionary<string, Unit> UnitsDictionary { get; set; } = new Dictionary<string, Unit>().ToFrozenDictionary();

    /// <summary>
    ///     [PcObjectEventType.Type, PcObjectEventType]
    /// </summary>
    public FrozenDictionary<string, PcObjectEventType> PcObjectEventTypesDictionary { get; set; } = new Dictionary<string, PcObjectEventType>().ToFrozenDictionary();

    ///// <summary>        
    /////     [Param.Desc, ParamDesc]
    ///// </summary>
    //public FrozenDictionary<string, ParamDesc> ReverseParamDescs { get; private set; } = new Dictionary<string, ParamDesc>().ToFrozenDictionary();        

    /// <summary>
    ///     Filled in Monitoring. BasePcObject.Identifiers are unique only within unit.
    ///     [Unit.Identifier + '.' + BasePcObject.Identifier, BasePcObject]
    /// </summary>
    public FrozenDictionary<string, BasePcObject> BasePcObjectsDictionary1 { get; set; } = new Dictionary<string, BasePcObject>().ToFrozenDictionary();

    /// <summary>
    ///     Filled in Monitoring.
    ///     [BasePcObject.Id, BasePcObject]
    /// </summary>
    public FrozenDictionary<int, BasePcObject> BasePcObjectsDictionary2 { get; set; } = new Dictionary<int, BasePcObject>().ToFrozenDictionary();

    /// <summary>
    ///     Filled in Monitoring. PcObject.Identifiers are unique only within unit.
    ///     [Unit.Identifier + '.' + PcObject.Identifier, PcObject]
    /// </summary>
    public FrozenDictionary<string, PcObject> PcObjectsDictionary1 { get; set; } = new Dictionary<string, PcObject>().ToFrozenDictionary();

    /// <summary>
    ///     Filled in Monitoring.
    ///     [PcObject.Id, PcObject]
    /// </summary>
    public FrozenDictionary<int, PcObject> PcObjectsDictionary2 { get; set; } = new Dictionary<int, PcObject>().ToFrozenDictionary();

    /// <summary>
    ///     All Project Version params
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersionNum"></param>
    /// <param name="readOnlyDbContext"></param>
    /// <param name="loggersSet"></param>        
    /// <returns></returns>
    public async Task<ProjectAllParamValues> GetProjectAllParamValuesAsync(int projectId, uint? projectVersionNum, PazCheckDbContext readOnlyDbContext, ILoggersSet loggersSet)
    {
        await _projectAllParamValuesCache_SyncRoot.WaitAsync();
        try
        {
            ProjectAllParamValues? projectAllParamValues = null;

            _projectAllParamValuesCache.TryGetValue((projectId, projectVersionNum ?? UInt32.MaxValue), out projectAllParamValues);

            string projectDataGuid = @"";
            if (projectVersionNum is null)
            {
                projectDataGuid = readOnlyDbContext.Database.SqlQuery<ProjectOptimized>($"SELECT \"DataGuid\" FROM \"Projects\" WHERE \"Id\" = {projectId}").FirstOrDefault()?.DataGuid ?? @"";

                if (projectAllParamValues is not null && projectDataGuid != projectAllParamValues.ProjectDataGuid)
                    projectAllParamValues = null;
            }

            if (projectAllParamValues is null)
            {
                projectAllParamValues = new ProjectAllParamValues()
                {
                    ProjectDataGuid = projectDataGuid,
                };

                var projectAllParamValuesTemp = new ProjectAllParamValuesTemp();

                if (projectVersionNum is null)
                {
                    foreach (var legend in readOnlyDbContext.Legends
                        .Include(t => t.LegendParams.Where(e => e._IsDeleted == false)) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<Legend>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> legendParams = new();
                        projectAllParamValuesTemp.LegendsParams.Add(legend.Identifier, legendParams);

                        foreach (var param_ in legend.LegendParams
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.LegendsParamValues[legend.Identifier + @"." + paramName] = param_.Value;
                            legendParams.Add(param_);
                        }
                    }

                    foreach (var ceMatrix in readOnlyDbContext.CeMatrices
                        .Include(t => t.CeMatrixParams.Where(e => e._IsDeleted == false)) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<CeMatrix>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> ceMatrixParams = new();
                        projectAllParamValuesTemp.CeMatricesParams.Add(ceMatrix.Identifier, ceMatrixParams);

                        foreach (var param_ in ceMatrix.CeMatrixParams
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.CeMatricesParamValues[ceMatrix.Identifier + @"." + paramName] = param_.Value;
                            ceMatrixParams.Add(param_);
                        }
                    }

                    foreach (var baseActuator in readOnlyDbContext.BaseActuators
                        .Include(t => t.BaseActuatorParams.Where(e => e._IsDeleted == false)) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<BaseActuator>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> baseActuatorParams = new();
                        projectAllParamValuesTemp.BaseActuatorsParams.Add(baseActuator.Identifier, baseActuatorParams);

                        foreach (var param_ in baseActuator.BaseActuatorParams
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.BaseActuatorsParamValues[baseActuator.Identifier + @"." + paramName] = param_.Value;
                            baseActuatorParams.Add(param_);
                        }
                    }

                    foreach (var tag in readOnlyDbContext.Tags
                        .Include(t => t.TagParams.Where(e => e._IsDeleted == false)) // Optimize
                        .Include(t => t.TagConditions.Where(e => e._IsDeleted == false)) // Optimize  
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<Tag>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> tagParams = new();
                        projectAllParamValuesTemp.TagsParams.Add(tag.Identifier, tagParams);

                        //string baseActuatorIdentifier = @"";
                        foreach (var param_ in tag.TagParams
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.TagsParamValues[tag.Identifier + @"." + paramName] = param_.Value;
                            tagParams.Add(param_);
                            //if (String.Equals(paramName, PazCheckCentralServerConstants.ParamName_BaseActuator, StringComparison.InvariantCultureIgnoreCase))
                            //    baseActuatorIdentifier = param_.Value;
                        }

                        List<TagCondition> list = new();
                        projectAllParamValuesTemp.TagConditions.Add(tag.Identifier, list);

                        foreach (var tc in tag.TagConditions
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            CheckTagCondition(projectAllParamValuesTemp.LegendsParams, tag, tc, loggersSet);
                            list.Add(tc);
                        }
                    }

                    foreach (var safetyController in readOnlyDbContext.SafetyControllers
                        .Include(t => t.SafetyControllerParams.Where(e => e._IsDeleted == false)) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<SafetyController>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> safetyControllerParams = new();
                        projectAllParamValuesTemp.SafetyControllersParams.Add(safetyController.Identifier, safetyControllerParams);

                        foreach (var param_ in safetyController.SafetyControllerParams
                            .Where(e => e._IsDeleted == false)) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.SafetyControllersParamValues[safetyController.Identifier + @"." + paramName] = param_.Value;
                            safetyControllerParams.Add(param_);
                        }
                    }
                }
                else if (projectVersionNum != 0)
                {
                    projectAllParamValues.ProjectVersion = readOnlyDbContext.ProjectVersions.Single(pv => pv.ProjectId == projectId && pv.VersionNum == projectVersionNum);

                    foreach (var legend in readOnlyDbContext.Legends
                        .Include(t => t.LegendParams.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<Legend>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> legendParams = new();
                        projectAllParamValuesTemp.LegendsParams.Add(legend.Identifier, legendParams);

                        foreach (var param_ in legend.LegendParams
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; //  PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.LegendsParamValues[legend.Identifier + @"." + paramName] = param_.Value;
                            legendParams.Add(param_);
                        }
                    }

                    foreach (var ceMatrix in readOnlyDbContext.CeMatrices
                        .Include(t => t.CeMatrixParams.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<CeMatrix>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> ceMatrixParams = new();
                        projectAllParamValuesTemp.CeMatricesParams.Add(ceMatrix.Identifier, ceMatrixParams);

                        foreach (var param_ in ceMatrix.CeMatrixParams
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.CeMatricesParamValues[ceMatrix.Identifier + @"." + paramName] = param_.Value;
                            ceMatrixParams.Add(param_);
                        }
                    }

                    foreach (var baseActuator in readOnlyDbContext.BaseActuators
                        .Include(t => t.BaseActuatorParams.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<BaseActuator>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> baseActuatorParams = new();
                        projectAllParamValuesTemp.BaseActuatorsParams.Add(baseActuator.Identifier, baseActuatorParams);

                        foreach (var param_ in baseActuator.BaseActuatorParams
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.BaseActuatorsParamValues[baseActuator.Identifier + @"." + paramName] = param_.Value;
                            baseActuatorParams.Add(param_);
                        }
                    }

                    foreach (var tag in readOnlyDbContext.Tags
                        .Include(t => t.TagParams.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize
                        .Include(t => t.TagConditions.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize 
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<Tag>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> tagParams = new();
                        projectAllParamValuesTemp.TagsParams.Add(tag.Identifier, tagParams);

                        //string baseActuatorIdentifier = @"";
                        foreach (var param_ in tag.TagParams
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; // PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.TagsParamValues[tag.Identifier + @"." + paramName] = param_.Value;
                            tagParams.Add(param_);
                            //if (String.Equals(paramName, PazCheckCentralServerConstants.ParamName_BaseActuator, StringComparison.InvariantCultureIgnoreCase))
                            //    baseActuatorIdentifier = param_.Value;
                        }

                        List<TagCondition> list = new();
                        projectAllParamValuesTemp.TagConditions.Add(tag.Identifier, list);

                        foreach (var tc in tag.TagConditions
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext entities loading issues
                        {
                            CheckTagCondition(projectAllParamValuesTemp.LegendsParams, tag, tc, loggersSet);
                            list.Add(tc);
                        }
                    }

                    foreach (var safetyController in readOnlyDbContext.SafetyControllers
                        .Include(t => t.SafetyControllerParams.Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                            (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // Optimize                                                
                        .Where(PazCheckDbHelper.GetVersionEntityPredicate<SafetyController>(projectVersionNum, projectId)))
                    {
                        List<VersionedParamBase> safetyControllerParams = new();
                        projectAllParamValuesTemp.SafetyControllersParams.Add(safetyController.Identifier, safetyControllerParams);

                        foreach (var param_ in safetyController.SafetyControllerParams
                            .Where(e => e._CreateProjectVersionNum != null && e._CreateProjectVersionNum <= projectVersionNum.Value &&
                                (e._DeleteProjectVersionNum == null || e._DeleteProjectVersionNum > projectVersionNum.Value))) // because of dbContext issues
                        {
                            var paramName = param_.ParamName; //  PazCheckDbHelper.RemoveParamNamePrefixIfAny(param_.ParamName);
                            projectAllParamValuesTemp.SafetyControllersParamValues[safetyController.Identifier + @"." + paramName] = param_.Value;
                            safetyControllerParams.Add(param_);
                        }
                    }
                }

                projectAllParamValues.CeMatricesParamValues = projectAllParamValuesTemp.CeMatricesParamValues.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.CeMatricesParams = projectAllParamValuesTemp.CeMatricesParams.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.TagsParamValues = projectAllParamValuesTemp.TagsParamValues.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.TagsParams = projectAllParamValuesTemp.TagsParams.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.TagConditions = projectAllParamValuesTemp.TagConditions.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.BaseActuatorsParamValues = projectAllParamValuesTemp.BaseActuatorsParamValues.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.BaseActuatorsParams = projectAllParamValuesTemp.BaseActuatorsParams.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.SafetyControllersParamValues = projectAllParamValuesTemp.SafetyControllersParamValues.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.SafetyControllersParams = projectAllParamValuesTemp.SafetyControllersParams.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.LegendsParamValues = projectAllParamValuesTemp.LegendsParamValues.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
                projectAllParamValues.LegendsParams = projectAllParamValuesTemp.LegendsParams.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);

                _projectAllParamValuesCache[(projectId, projectVersionNum ?? UInt32.MaxValue)] = projectAllParamValues;
            }

            return projectAllParamValues;
        }
        finally
        {
            _projectAllParamValuesCache_SyncRoot.Release();
        }
    }

    public static void CheckTagCondition(CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> projectAllLegends, Tag tag, TagCondition tagCondition, ILoggersSet loggersSet)
    {
        foreach (var symbolToDisplay in CsvHelper.ParseCsvLine(@",", tagCondition.SymbolToDisplay))
        {
            if (String.IsNullOrEmpty(symbolToDisplay))
                continue;
            if (!projectAllLegends.TryGetValue(symbolToDisplay, out List<VersionedParamBase>? legend))
            {
                using var tagScope = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.Scope_TagName, tag.Identifier));
                using var tagConditionScope = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.Scope_TagCondition, tagCondition.ToString()));
                using var symbolToDisplayScope = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.Scope_SymbolToDisplay, symbolToDisplay));
                loggersSet.UserFriendlyLogger.LogWarning(Properties.Resources.InvalidSymbolToDisplay_NoLegend);
            }
        }
    }

    #endregion

    #region private fields

    /// <summary>
    ///     [(Project.Id, ProjectVersionNum), ProjectAllParamValues], ProjectVersionNum == 0 for unsaved changes
    /// </summary>
    private readonly Dictionary<(int, uint), ProjectAllParamValues> _projectAllParamValuesCache = new();

    private readonly SemaphoreSlim _projectAllParamValuesCache_SyncRoot = new(1);

    #endregion

    private class ProjectOptimized
    {
        public string DataGuid { get; set; } = @"";
    }
}
