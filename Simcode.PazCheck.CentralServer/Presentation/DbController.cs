using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Db")]
    public class DbController : ControllerBase
    {
        #region construction and destruction

        public DbController(
            IMainServerWorker mainServerWorker,
            Cache cache,
            JobsManager jobsManager,
            AddonsManager addonsManager,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<ImporterController> logger)
        {
            _mainServerWorker = mainServerWorker;
            _cache = cache;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _dbContextFactory = dbContextFactory;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;            
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        /// <summary>
        ///     Метод для получения описания всех фильтров.
        /// </summary>
        /// <param name="entitiesName"></param>
        /// <param name="projectVersionNum"></param>
        /// <param name="parentEntityId"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpGet(@"GetFilterInfo")]
        public async Task<IActionResult> GetFilterInfoAsync(string entitiesName, UInt32? projectVersionNum, int? parentEntityId)
        {
            if (projectVersionNum == 0)
                projectVersionNum = null; // Normalize

            var roles = HttpContextHelper.GetRoles(HttpContext);
            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName ?? @"", out PropertyInfo? pazCheckDbContext_PropertyInfo);
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Read");
            if (!checkAccessSucceeded)
                return Unauthorized();

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            FilterInfo filterInfo = new();
            
            switch (entityType.Name)
            {
                case nameof(CeMatrix):
                    {
                        var projectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(
                            parentEntityId ?? 0, 
                            projectVersionNum, 
                            readOnlyDbContext,
                            LoggersSet.Empty);
                        //var ceMatrixTypes = await readOnlyDbContext.CeMatrixTypes.ToArrayAsync();
                        filterInfo.SearchBy_List =
                        [
                            Common.Properties.Resources.SearchBy_All,
                            Common.Properties.Resources.SearchBy_ExcludeContent,
                            Common.Properties.Resources.SearchBy_IdentifierTitleDesc,
                        ];
                        filterInfo.SearchBy_DescList =
                        [
                            Common.Properties.Resources.SearchBy_All,
                            Common.Properties.Resources.SearchBy_ExcludeContent,
                            Common.Properties.Resources.SearchBy_IdentifierTitleDesc,
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_CeMatrixType}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_CeMatrixType,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = ceMatrixTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                            //    ValueDescsList = ceMatrixTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_TagName,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_TagName,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                                ValuesList = projectAllParamValues.TagsParams.Keys.OrderBy(k => k).ToArray(),
                                ValueDescsList = projectAllParamValues.TagsParams
                                    .OrderBy(kvp => kvp.Key)
                                    .Select(kvp => kvp.Value.FirstOrDefault(v => String.Equals(v.ParamName, PazCheckConstants.ParamName_Desc, StringComparison.InvariantCultureIgnoreCase))?.Value ?? @"")
                                    .ToArray()
                            },                            
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [ PazCheckConstants.ParamName_CeMatrixSource ],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                        ];
                        filterInfo.StandardFilterNamesList = 
                        [
                            Common.Properties.Resources.ParamDesc_CeMatrixTemplate_ForFilter + " - САО",
                            Common.Properties.Resources.ParamDesc_CeMatrixTemplate_ForFilter + " - СПГС",
                        ];
                        filterInfo.StandardFiltersList =
                        [
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_Params + "[" + PazCheckConstants.ParamName_CeMatrixTemplate + "]",
                                            Operator = @"==",
                                            ValuesList = [ "САО_Template" ]
                                        }
                                    ]
                                ]
                            },
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_Params + "[" + PazCheckConstants.ParamName_CeMatrixTemplate + "]",
                                            Operator = @"==",
                                            ValuesList = [ "СПГС_Template" ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }
                    break;
                case nameof(CeMatrixResult):
                    filterInfo.SearchBy_List =
                    [
                        Common.Properties.Resources.SearchBy_All,
                        Common.Properties.Resources.SearchBy_ExcludeContent,
                        Common.Properties.Resources.SearchBy_IdentifierTitleDesc,
                        //CentralServer.Common.Properties.Resources.SearchBy_TagName,
                        //CentralServer.Common.Properties.Resources.SearchBy_TagDesc,
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        Common.Properties.Resources.SearchBy_All,
                        Common.Properties.Resources.SearchBy_ExcludeContent,
                        Common.Properties.Resources.SearchBy_IdentifierTitleDesc,
                    ];
                    Result? result = readOnlyDbContext.Results                         
                        .FirstOrDefault(r => r.Id == (parentEntityId ?? 0));
                    if (result is not null)
                    {
                        var projectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(
                            result.ProjectId ?? 0, 
                            result.ProjectVersionNum, 
                            readOnlyDbContext,
                            LoggersSet.Empty);
                        //var ceMatrixTypes = await readOnlyDbContext.CeMatrixTypes.ToArrayAsync();
                        filterInfo.CriterionInfosList =
                        [
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_CeMatrixType}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_CeMatrixType,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = ceMatrixTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                            //    ValueDescsList = ceMatrixTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_TagName,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_TagName,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                                ValuesList = projectAllParamValues.TagsParams.Keys.OrderBy(k => k).ToArray(),
                                ValueDescsList = projectAllParamValues.TagsParams
                                    .OrderBy(kvp => kvp.Key)
                                    .Select(kvp => kvp.Value.FirstOrDefault(v => String.Equals(v.ParamName, PazCheckConstants.ParamName_Desc, StringComparison.InvariantCultureIgnoreCase))?.Value ?? @"")
                                    .ToArray()
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                            // TODO in Front
                            // Always included CriterionName = PazCheckCentralServerConstants.CriterionName_TriggeredType
                        ];
                        filterInfo.StandardFilterNamesList =
                        [
                            Common.Properties.Resources.ParamDesc_CeMatrixTemplate + " - САО",
                            Common.Properties.Resources.ParamDesc_CeMatrixTemplate + " - СПГС",
                        ];
                        filterInfo.StandardFiltersList =
                        [
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_Params + "[" + PazCheckConstants.ParamName_CeMatrixTemplate + "]",
                                            Operator = @"==",
                                            ValuesList = [ "САО_Template" ]
                                        }
                                    ]
                                ]
                            },
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_Params + "[" + PazCheckConstants.ParamName_CeMatrixTemplate + "]",
                                            Operator = @"==",
                                            ValuesList = [ "СПГС_Template" ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }
                    break;
                case nameof(Tag):
                    {
                        var projectAllParamValues = await _cache.DbCache.GetProjectAllParamValuesAsync(
                            parentEntityId ?? 0, 
                            projectVersionNum, 
                            readOnlyDbContext,
                            LoggersSet.Empty);
                        filterInfo.SearchBy_List =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_All,
                            CentralServer.Common.Properties.Resources.SearchBy_TagName,                                              
                            //CentralServer.Common.Properties.Resources.SearchBy_TagDesc,
                        ];
                        filterInfo.SearchBy_DescList =
                            [
                                CentralServer.Common.Properties.Resources.SearchBy_All,
                            CentralServer.Common.Properties.Resources.SearchBy_TagName,
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_IsTemplate,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValuesList = new string[] {
                                    "true",
                                    "false" },
                                ValueDescsList = new string[] {
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.True,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.False }
                            },
                            new CriterionInfo
                            {
                                CriterionName = $"Params[{PazCheckConstants.ParamName_TagTemplate}]",
                                CriterionDesc = Common.Properties.Resources.ParamDesc_TagTemplate,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                            },
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_BaseActuator}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_BaseActuator,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = projectAllParamValues.BaseActuatorsParams.Keys.OrderBy(k => k).ToArray(),
                            //    ValueDescsList = projectAllParamValues.BaseActuatorsParams
                            //        .OrderBy(kvp => kvp.Key)
                            //        .Select(kvp => kvp.Value.FirstOrDefault(v => String.Equals(v.ParamName, PazCheckCentralServerConstants.ParamName_Title, StringComparison.InvariantCultureIgnoreCase))?.Value ?? @"")
                            //        .ToArray()
                            //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                        ];
                        filterInfo.StandardFilterNamesList =
                        [
                            Properties.Resources.StandardFilterName_Templates,                            
                        ];
                        filterInfo.StandardFiltersList =
                        [
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                            Operator = @"==",
                                            ValuesList = [ "true" ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }                    
                    break;
                case nameof(BaseActuator):
                    {
                        //var baseActuatorTypes = await readOnlyDbContext.BaseActuatorTypes.ToArrayAsync();
                        filterInfo.SearchBy_List =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_All,
                            //CentralServer.Common.Properties.Resources.SearchBy_TagName,
                            //CentralServer.Common.Properties.Resources.SearchBy_TagDesc,
                        ];
                        filterInfo.SearchBy_DescList =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_All,
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_IsTemplate,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValuesList = new string[] {
                                    "true",
                                    "false" },
                                ValueDescsList = new string[] {
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.True,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.False }
                            },
                            //new CriteriaDesc
                            //{
                            //    CriterionName = Common.Properties.Resources.BaseActuatorType,
                            //    ValueType = CentralServer.Common.PazCheckCentralServerConstants.ValueType_String
                            //},
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_BaseActuatorType}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_BaseActuatorType,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = baseActuatorTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                            //    ValueDescsList = baseActuatorTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            //},                            
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                        ];
                        filterInfo.StandardFilterNamesList =
                        [
                            Properties.Resources.StandardFilterName_Templates,
                        ];
                        filterInfo.StandardFiltersList =
                        [
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                            Operator = @"==",
                                            ValuesList = [ "true" ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }                    
                    break;
                case nameof(SafetyController):
                    {
                        //var safetyControllerTypes = await readOnlyDbContext.CeMatrixTypes.ToArrayAsync();
                        filterInfo.SearchBy_List =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_All,
                            //CentralServer.Common.Properties.Resources.SearchBy_TagName,
                            //CentralServer.Common.Properties.Resources.SearchBy_TagDesc,
                        ];
                        filterInfo.SearchBy_DescList =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_All,
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_IsTemplate,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValuesList = new string[] {
                                    "true",
                                    "false" },
                                ValueDescsList = new string[] {
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.True,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.False }
                            },
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_SafetyControllerType}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_SafetyControllerType,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = safetyControllerTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                            //    ValueDescsList = safetyControllerTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                        ];
                        filterInfo.StandardFilterNamesList =
                        [
                            Properties.Resources.StandardFilterName_Templates,
                        ];
                        filterInfo.StandardFiltersList =
                        [
                            new Filter
                            {
                                CriterionCollection =
                                [
                                    [
                                        new Criterion()
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_IsTemplate,
                                            Operator = @"==",
                                            ValuesList = [ "true" ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }                    
                    break;
                case nameof(Legend):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,                        
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                    ];
                    filterInfo.CriterionInfosList =
                    [   
                    ];
                    break;
                case nameof(PcObject):
                    {
                        //var basePcObjectTypes = await readOnlyDbContext.BasePcObjectTypes.ToArrayAsync();
                        var basePcObjects = await readOnlyDbContext.BasePcObjects
                            .Include(bpo => bpo.Unit)
                            .Where(bpo => bpo._IsDeleted == false)
                            .ToArrayAsync();
                        var pcObjectEventTypes = await readOnlyDbContext.PcObjectEventTypes.ToArrayAsync();
                        filterInfo.SearchBy_List =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_PcObjectAll,
                            CentralServer.Common.Properties.Resources.SearchBy_PcObjectEventAll
                        ];
                        filterInfo.SearchBy_DescList =
                        [
                            CentralServer.Common.Properties.Resources.SearchBy_PcObjectAll,
                        CentralServer.Common.Properties.Resources.SearchBy_PcObjectEventAll
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            //new CriterionInfo
                            //{
                            //    CriterionName = $"Params[{PazCheckCentralServerConstants.ParamName_BasePcObjectType}]",
                            //    CriterionDesc = Common.Properties.Resources.ParamDesc_BasePcObjectType,
                            //    OperatorsList = GetStringOperatorsList(),
                            //    OperatorDescsList = GetStringOperatorDescsList(),
                            //    OptionsList = GetStringOptionsList(),
                            //    OptionDescsList = GetStringOptionDescsList(),
                            //    IsMultiValue = true,
                            //    ValuesList = basePcObjectTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                            //    ValueDescsList = basePcObjectTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_PcObject_Template,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_BasePcObject_Identifier,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                                ValuesList = basePcObjects.OrderBy(bo => bo.Identifier)
                                    .Select(bo => bo.Identifier)
                                    .ToArray(),
                                ValueDescsList = basePcObjects.OrderBy(t => t.Identifier)
                                    .Select(bo => bo.Unit.Title + @": " + bo.ParamsDictionary.FirstOrDefault(kvp => String.Equals(kvp.Key, PazCheckConstants.ParamName_Title, StringComparison.InvariantCultureIgnoreCase)).Value ?? @"")
                                    .ToArray()
                            },
                            new CriterionInfo
                            {
                                CriterionName = $"{PazCheckConstants.CriterionName_Values}[{PazCheckConstants.ParamNamePrefix_Data + PazCheckConstants.ParamName_SafetyIndex}]",
                                CriterionDesc = Common.Properties.Resources.ParamDesc_SafetyIndex,
                                OperatorsList = GetNumericOperatorsList(),
                                OperatorDescsList = GetNumericOperatorDescsList(),
                                OptionsList = GetNumericOptionsList(),
                                OptionDescsList = GetNumericOptionDescsList(),
                                IsMultiValue = true,
                                ValuesList = [
                                    @"(66..100]",
                                    @"(33..66]",
                                    @"[0..33]",
                                ],
                                ValueDescsList = [
                                    CentralServer.Common.Properties.Resources.SafetyIndex_Green,
                                    CentralServer.Common.Properties.Resources.SafetyIndex_Yellow,
                                    CentralServer.Common.Properties.Resources.SafetyIndex_Red,
                                ]
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = $"{PazCheckConstants.CriterionName_EventParams}[{PazCheckConstants.ParamName_MalfunctionCategory}]",
                                CriterionDesc = Common.Properties.Resources.ParamDesc_MalfunctionCategory,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },                            
                            new CriterionInfo
                            {                                
                                CriterionName = PazCheckConstants.CriterionName_EventType,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_EventType,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = true,
                                ValuesList = pcObjectEventTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                                ValueDescsList = pcObjectEventTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = $"{PazCheckConstants.CriterionName_EventParams}[{PazCheckConstants.ParamName_EventTypeTitle},{PazCheckConstants.ParamName_EventTypeDesc},{PazCheckConstants.ParamName_EventTitle},{PazCheckConstants.ParamName_EventDesc}]",
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Event,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Params,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Params,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Values,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_JournalParams,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            }
                        ];
                    }                    
                    break;
                case nameof(PcObjectEvent):
                    {
                        //var basePcObjectTypes = await readOnlyDbContext.BasePcObjectTypes.ToArrayAsync();
                        //var basePcObjects = await readOnlyDbContext.BasePcObjects.ToArrayAsync();
                        var pcObjectEventTypes = await readOnlyDbContext.PcObjectEventTypes.ToArrayAsync();
                        filterInfo.SearchBy_List =
                        [
                            Common.Properties.Resources.SearchBy_PcObjectEventAll,
                            Common.Properties.Resources.SearchBy_EventDesc,
                            Common.Properties.Resources.SearchBy_PrimeCause
                        ];
                        filterInfo.SearchBy_DescList =
                        [
                            Common.Properties.Resources.SearchBy_PcObjectEventAll,
                            Common.Properties.Resources.SearchBy_EventDesc,
                            Common.Properties.Resources.SearchBy_PrimeCause
                        ];
                        filterInfo.CriterionInfosList =
                        [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_EventType,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_EventType,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = true,
                                ValuesList = pcObjectEventTypes.OrderBy(t => t.Type).Select(t => t.Type).ToArray(),
                                ValueDescsList = pcObjectEventTypes.OrderBy(t => t.Type).Select(t => t.Title).ToArray()
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = $"{PazCheckConstants.CriterionName_EventParams}[{PazCheckConstants.ParamName_EventTypeTitle},{PazCheckConstants.ParamName_EventTypeDesc},{PazCheckConstants.ParamName_EventTitle},{PazCheckConstants.ParamName_EventDesc}]",
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_Event,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = $"{PazCheckConstants.CriterionName_EventParams}[{PazCheckConstants.ParamName_LogicCommandResultType},{PazCheckConstants.ParamName_JournalCommandResultType}]",
                                CriterionDesc = Common.Properties.Resources.ParamDesc_CommandResultType,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,                                
                                ValuesList = [
                                    new Any(TriggeredType.SuccessTriggered).ValueAsString(false),
                                    new Any(TriggeredType.NotTriggered).ValueAsString(false),
                                    new Any(TriggeredType.LateTriggered).ValueAsString(false)
                                ],
                                ValueDescsList = [
                                    CentralServer.Common.Properties.Resources.ParamValue_SuccessTriggered,
                                    CentralServer.Common.Properties.Resources.ParamValue_NotTriggered,
                                    CentralServer.Common.Properties.Resources.ParamValue_LateTriggered
                                ]
                            },
                            new CriterionInfo
                            {                                
                                CriterionName = $"{PazCheckConstants.CriterionName_EventParams}[{PazCheckConstants.ParamName_EmergencyShutdownLevel}]",
                                CriterionDesc = Common.Properties.Resources.ParamDesc_EmergencyShutdownLevel,
                                OperatorsList = GetStringOperatorsList(),
                                OperatorDescsList = GetStringOperatorDescsList(),
                                OptionsList = GetStringOptionsList(),
                                OptionDescsList = GetStringOptionDescsList(),
                                IsMultiValue = true,                                
                                ValuesList = [ "2A", "2B", "2C", "3", "4" ] 
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_EventParams,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_EventParams,
                                IsRequiredParamName = true,
                                PriorityParamNamesList = [],
                                OperatorsList = GetFullOperatorsList(),
                                OperatorDescsList = GetFullOperatorDescsList(),
                                OptionsList = GetFullOptionsList(),
                                OptionDescsList = GetFullOptionDescsList(),
                                IsMultiValue = true,
                            },
                        ];
                        filterInfo.StandardFilterNamesList = [ Common.Properties.Resources.PcObjectEventTypeTitle_EmergencyShutdown ];
                        filterInfo.StandardFiltersList = 
                        [                        
                            new Filter
                            {
                                CriterionCollection = 
                                [
                                    [ 
                                        new Criterion() 
                                        {
                                            CriterionName = PazCheckConstants.CriterionName_EventType,
                                            Operator = @"==",
                                            ValuesList = [ PazCheckConstants.PcObjectEventType_EmergencyShutdown ]
                                        }
                                    ]
                                ]
                            }
                        ];
                    }                    
                    break;
                case nameof(UnitEventsInterval):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_ExcludeContent_UnitEventsInterval,                        
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_ExcludeContent_UnitEventsInterval,
                    ];
                    filterInfo.CriterionInfosList =
                    [
                        new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                    ];
                    break;
                case nameof(UnitEvent):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_TagName
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_TagName
                    ];
                    filterInfo.CriterionInfosList =
                    [                      
                        //new CriteriaDesc
                        //{
                        //    CriterionName = "Статус сигнализации",
                        //    ValueType = CentralServer.Common.PazCheckCentralServerConstants.ValueType_FixedList,
                        //    ValuesList = new string[] {
                        //        "Активна",
                        //        "Неактивна" }
                        //},
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },   
                        // Always included CriterionName = PazCheckCentralServerConstants.CriterionName_Priority
                    ];
                    break;
                case nameof(UserEvent):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All
                    ];
                    filterInfo.CriterionInfosList =
                    [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },   
                        // Always included CriterionName = PazCheckCentralServerConstants.CriterionName_LogLevel
                    ];
                    break;
                case nameof(InformationSecurityEvent):
                case nameof(AllRolesAccessInformationSecurityEvent):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All
                    ];
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All
                    ];
                    filterInfo.CriterionInfosList =
                    [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_Severity,
                                CriterionDesc = Simcode.PazCheck.CentralServer.Common.Properties.Resources.CriterionDesc_Severity,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = true,
                                ValuesList =
                                [
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Low_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Medium_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.High_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.VeryHigh_Severity
                                ],
                                ValueDescsList = 
                                [
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Low_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Medium_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.High_Severity,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.VeryHigh_Severity
                                ]                                
                            },
                            new CriterionInfo
                            {                            
                                CriterionName = PazCheckConstants.CriterionName_Succeeded,
                                CriterionDesc = Simcode.PazCheck.CentralServer.Common.Properties.Resources.CriterionDesc_Succeeded,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValuesList = new string[] {
                                    "true",
                                    "false" },
                                ValueDescsList = new string[] {
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Succeeded_True,
                                    Simcode.PazCheck.CentralServer.Common.Properties.Resources.Succeeded_False }
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },   
                        // TODO!!! Always included CriterionName = PazCheckCentralServerConstants.CriterionName_Severity
                    ];
                    break;
                case nameof(ResultEvent):
                    filterInfo.SearchBy_List =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_TagName
                    ];
                    filterInfo.SearchBy_DescList =
                    [
                        CentralServer.Common.Properties.Resources.SearchBy_All,
                        CentralServer.Common.Properties.Resources.SearchBy_TagName
                    ];
                    filterInfo.CriterionInfosList =
                    [
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_From,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_From,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },
                            new CriterionInfo
                            {
                                CriterionName = PazCheckConstants.CriterionName_To,
                                CriterionDesc = Common.Properties.Resources.CriterionDesc_To,
                                OperatorsList = [ @"==" ],
                                OperatorDescsList = [ Common.Properties.Resources.Operator_Equals ],
                                IsMultiValue = false,
                                ValueType = CentralServer.Common.PazCheckConstants.ValueType_DateTime
                            },   
                            // Always included CriterionName = PazCheckCentralServerConstants.CriterionName_Priority
                    ];
                    break;
            }
                     
            return Ok(filterInfo);
        }                

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entitiesName"></param>
        /// <param name="projectVersionNum"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpPost("Filter")]
        public async Task<IActionResult> FilterAsync([FromBody] Filter filter, string entitiesName, UInt32? projectVersionNum)
        {            
            filter.User = (filter.User ?? @"").ToLowerInvariant(); // Normalize             
            if (projectVersionNum == 0)
                projectVersionNum = null; // Normalize 
            filter.SearchString = filter.SearchString?.Trim();

            var roles = HttpContextHelper.GetRoles(HttpContext);
            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName ?? @"", out PropertyInfo? pazCheckDbContext_PropertyInfo);
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Read");
            if (!checkAccessSucceeded)
                return Unauthorized();

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            FilterHelper.Prepare(filter, _cache.DbCache);

            try
            {
                var results = await PazCheckDbHelper.FilterAsync(readOnlyDbContext, _cache.DbCache, entityType, filter, projectVersionNum, needOrdering: true);

                if (String.IsNullOrEmpty(filter.Aggregation))
                {
                    return Ok(new { results = results.Select(po => po.Id).ToArray() });
                }
                else
                {
                    var sourceArray = results.ToArray();
                    Array targetArray = Array.CreateInstance(entityType, sourceArray.Length);
                    for (int i = 0; i < sourceArray.Length; i++)
                    {
                        var item = sourceArray[i];                        
                        targetArray.SetValue(item, i);
                    }
                    return Ok(new { results = targetArray });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FilterAsync(...) Exception.");

                return NoContent();
            }
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpGet("GetChildrenForSafetyIndex")]
        public Task<IActionResult> GetChildrenForSafetyIndexAsync(int pcObjectId)
        {   
            try
            {
                _cache.DbCache.PcObjectsDictionary2.TryGetValue(pcObjectId, out PcObject? pcObject);
                if (pcObject is null)
                    return Task.FromResult<IActionResult>(NoContent());
                
                var results = PazCheckDbHelper.GetChildrenForSafetyIndex(pcObject);

                return Task.FromResult<IActionResult>(Ok(results.Select(x => new 
                {
                    Id = x.Id,
                    Identifier = x.Identifier,
                    Title = PazCheckDbHelper.GetParamValue(x.ParamsDictionary, x.BasePcObject.ParamsDictionary, PazCheckConstants.ParamName_Title, @""),
                    Desc = PazCheckDbHelper.GetParamValue(x.ParamsDictionary, x.BasePcObject.ParamsDictionary, PazCheckConstants.ParamName_Desc, @""),
                    SafetyIndex = x.SafetyIndex,
                    SafetyIndexDesc = x.SafetyIndexDesc,
                    K = x.K
                }).ToArray()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetChildrenForSafetyIndexAsync(...) Exception.");

                return Task.FromResult<IActionResult>(NoContent());
            }
        }

        /// <summary>
        ///     Always allowed method.
        /// </summary>
        /// <returns></returns>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="entitiesName"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) }
            )]
        [HttpPost("DeleteEntities")]
        public async Task<IActionResult> DeleteEntitiesAsync([FromBody] EntitiesCollectionInfo entitiesCollectionInfo, string entitiesName, int projectId)
        {            
            var roles = HttpContextHelper.GetRoles(HttpContext);
            InformationSecurityContext informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);                    

            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName, out PropertyInfo? pazCheckDbContext_PropertyInfo);            
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();

            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool succeeded = false;

            try
            {
                if (typeof(VersionedEntityBase).IsAssignableFrom(entityType))
                {
                    bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Update");
                    if (checkAccessSucceeded)
                    {
                        await using var dbContext = _dbContextFactory.CreateDbContext();                        
                        dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                        try
                        {
                            await PazCheckDbHelper.VersionedEntities_SetIsDeletedAsync(
                                dbContext,
                                _cache.DbCache,
                                projectId,
                                entitiesCollectionInfo,
                                pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First(),
                                informationSecurityContext.User);

                            succeeded = true;

                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "DeleteEntitiesAsync(...) Exception.");

                            return NoContent();
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else if (typeof(ICreateDeleteInfoEntity).IsAssignableFrom(entityType))
                {
                    bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Update");
                    if (checkAccessSucceeded)
                    {
                        await using var dbContext = _dbContextFactory.CreateDbContext();                        
                        try
                        {
                            await PazCheckDbHelper.CreateDeleteInfoEntities_SetIsDeletedAsync(dbContext, entitiesCollectionInfo, pazCheckDbContext_PropertyInfo);

                            succeeded = true;

                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "DeleteEntitiesAsync(...) Exception.");

                            return NoContent();
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Delete");
                    if (checkAccessSucceeded)
                    {
                        await using var dbContext = _dbContextFactory.CreateDbContext();

                        try
                        {
                            await PazCheckDbHelper.EntitiesDeleteAsync(dbContext, _cache.DbCache, entitiesCollectionInfo, pazCheckDbContext_PropertyInfo);

                            succeeded = true;

                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "DeleteEntitiesAsync(...) Exception.");

                            return NoContent();
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
            }
            finally
            {
                if (Attribute.IsDefined(pazCheckDbContext_PropertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                {
                    var informationSecurityEventEntityAttribute = pazCheckDbContext_PropertyInfo.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                    var pcDisplayNameAttribute = pazCheckDbContext_PropertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()!;

                    CaseInsensitiveOrderedDictionary<string?> eventAdditionalFields = new();

                    if (typeof(ProjectVersionedEntityBase).IsAssignableFrom(entityType))
                    {
                        await using PazCheckDbContext secondary_ReadOnlyDbContext = _dbContextFactory.CreateDbContext();
                        secondary_ReadOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        ProjectVersionedEntityBase? projectVersionedEntity = null;                        
                        switch (entityType.Name)
                        {
                            case nameof(CeMatrix):
                                projectVersionedEntity = secondary_ReadOnlyDbContext.CeMatrices
                                    .Include(m => m.Project)
                                    .ThenInclude(p => p.Unit)
                                    .FirstOrDefault();
                                break;
                            case nameof(Tag):
                                projectVersionedEntity = secondary_ReadOnlyDbContext.Tags
                                    .Include(m => m.Project)
                                    .ThenInclude(p => p.Unit)
                                    .FirstOrDefault();
                                break;
                            case nameof(BaseActuator):
                                projectVersionedEntity = secondary_ReadOnlyDbContext.BaseActuators
                                    .Include(m => m.Project)
                                    .ThenInclude(p => p.Unit)
                                    .FirstOrDefault();
                                break;
                            case nameof(SafetyController):
                                projectVersionedEntity = secondary_ReadOnlyDbContext.SafetyControllers
                                    .Include(m => m.Project)
                                    .ThenInclude(p => p.Unit)
                                    .FirstOrDefault();
                                break;
                            case nameof(Legend):
                                projectVersionedEntity = secondary_ReadOnlyDbContext.Legends
                                    .Include(m => m.Project)
                                    .ThenInclude(p => p.Unit)
                                    .FirstOrDefault();
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        if (projectVersionedEntity is not null)
                        {
                            eventAdditionalFields[CentralServer.Common.Properties.Resources.Project] = projectVersionedEntity.Project.Title;
                            eventAdditionalFields[CentralServer.Common.Properties.Resources.Unit] = projectVersionedEntity.Project.Unit.Title;
                        }                        
                    }

                    _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                                informationSecurityContext.SourceIpAddress,
                                                informationSecurityContext.SourceHost,
                                                informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                    InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                    InformationSecurityEventsLogger.EntityChanged_EventId,
                                                7,
                                                succeeded,
                                                CentralServer.Common.Properties.Resources.EntitiesDeleted_EventName,
                                                informationSecurityContext.User,
                                                @"",
                                                NameValueCollectionHelper.GetNameValueCollectionString(eventAdditionalFields),
                                                CentralServer.Common.Properties.Resources.EntitiesDeleted_EventDesc, pcDisplayNameAttribute.DisplayName);
                }
            }                       
        }        

        /// <summary>
        ///     Adds PcObjectEvent to Unit
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) }
            )]
        [HttpPost("AddResultToPcObject")]
        public async Task<IActionResult> AddResultToPcObjectAsync(int resultId)
        {
            InformationSecurityContext informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());

            bool succeeded = false;

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            Result? result = null;
            try
            {
                result = await readOnlyDbContext.Results
                    .Include(r => r.Unit)
                    .Include(r => r.CeMatrixResults)
                    .Include(r => r.ResultEvents)
                    .SingleAsync(r => r.Id == resultId);

                await PazCheckDbHelper.AddResultToRootPcObjectAsync(
                    _dbContextFactory,
                    informationSecurityContext.User,
                    _cache.DbCache, 
                    result, 
                    loggersSet);

                succeeded = true;

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteEntitiesAsync(...) Exception.");

                return NoContent();
            }
            finally
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                        informationSecurityContext.SourceIpAddress,
                        informationSecurityContext.SourceHost,                        
                        InformationSecurityEventsLogger.Calculation_AllRolesAccessEventId,
                        3,
                        succeeded,
                        Properties.Resources.AddResultToPcObject_Event,
                        informationSecurityContext.User,
                        Common.Properties.Resources.Result,
                        NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                            {
                                            (@"Action", @"AddResultToPcObject"),
                                            (@"ResultId", new Any(resultId).ValueAsString(false)),
                                            (@"AlalysisTimeUtc", new Any(result?.AlalysisTimeUtc).ValueAsString(false)),
                                            (@"BeginTimeUtc", new Any(result?.BeginTimeUtc).ValueAsString(false)),
                                            (@"EndTimeUtc", new Any(result?.EndTimeUtc).ValueAsString(false)),
                            }),
                        Properties.Resources.AddResultToPcObject_EventDesc);
            }
        }

        #endregion

        #region private functions

        // [ @"==" ] [ Common.Properties.Resources.Operator_Equals ]
        private string[] GetFullOperatorsList()
        {
            return 
                [
                   @"==",
                   @"!=",                   
                   @"<=",
                   @">=",                   
                   @"<",
                   @">",
                   @"Contains|",
                   @"!Contains|",
                   @"StartsWith|",
                   @"!StartsWith|",
                   @"EndsWith|",
                   @"!EndsWith|",
                   @"IsEmpty|",
                   @"!IsEmpty|"
                ];
        }

        private string[] GetFullOperatorDescsList()
        {
            return
                [
                   Common.Properties.Resources.Operator_Equals,
                   Common.Properties.Resources.Operator_NotEquals,
                   Common.Properties.Resources.Operator_LessThanOrEquals,
                   Common.Properties.Resources.Operator_GreaterThanOrEquals,
                   Common.Properties.Resources.Operator_LessThan,
                   Common.Properties.Resources.Operator_GreaterThan,
                   Common.Properties.Resources.Operator_Contains,
                   Common.Properties.Resources.Operator_NotContains,
                   Common.Properties.Resources.Operator_StartsWith,
                   Common.Properties.Resources.Operator_NotStartsWith,
                   Common.Properties.Resources.Operator_EndsWith,
                   Common.Properties.Resources.Operator_NotEndsWith,
                   Common.Properties.Resources.Operator_IsEmpty,
                   Common.Properties.Resources.Operator_NotIsEmpty
                ];
        }

        private string[] GetStringOperatorsList()
        {
            return
                [
                   @"==",
                   @"!=",                   
                   @"Contains|",
                   @"!Contains|",
                   @"StartsWith|",
                   @"!StartsWith|",
                   @"EndsWith|",
                   @"!EndsWith|",
                   @"IsEmpty|",
                   @"!IsEmpty|"
                ];
        }

        private string[] GetStringOperatorDescsList()
        {
            return
                [
                   Common.Properties.Resources.Operator_Equals,
                   Common.Properties.Resources.Operator_NotEquals,                   
                   Common.Properties.Resources.Operator_Contains,
                   Common.Properties.Resources.Operator_NotContains,
                   Common.Properties.Resources.Operator_StartsWith,
                   Common.Properties.Resources.Operator_NotStartsWith,
                   Common.Properties.Resources.Operator_EndsWith,
                   Common.Properties.Resources.Operator_NotEndsWith,
                   Common.Properties.Resources.Operator_IsEmpty,
                   Common.Properties.Resources.Operator_NotIsEmpty
                ];
        }

        private string[] GetNumericOperatorsList()
        {
            return
                [
                   @"==",
                   @"!=",
                   @"<=",
                   @">=",
                   @"<",
                   @">",                   
                ];
        }

        private string[] GetNumericOperatorDescsList()
        {
            return
                [
                   Common.Properties.Resources.Operator_Equals,
                   Common.Properties.Resources.Operator_NotEquals,
                   Common.Properties.Resources.Operator_LessThanOrEquals,
                   Common.Properties.Resources.Operator_GreaterThanOrEquals,
                   Common.Properties.Resources.Operator_LessThan,
                   Common.Properties.Resources.Operator_GreaterThan,                   
                ];
        }

        private string[] GetFullOptionsList()
        {
            return
                [
                   @"CaseSensitive"
                ];
        }

        private string[] GetFullOptionDescsList()
        {
            return
                [
                   Common.Properties.Resources.OperatorOption_CaseSensitive,                   
                ];
        }

        private string[] GetStringOptionsList()
        {
            return
                [
                   @"CaseSensitive"
                ];
        }

        private string[] GetStringOptionDescsList()
        {
            return
                [
                   Common.Properties.Resources.OperatorOption_CaseSensitive,
                ];
        }

        private string[] GetNumericOptionsList()
        {
            return
                [                   
                ];
        }

        private string[] GetNumericOptionDescsList()
        {
            return
                [
                ];
        }

        #endregion        

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly Cache _cache;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}



//new CriteriaDesc
//{
//    CriterionName = Common.Properties.Resources.ParamDesc_EventStatus,
//    ValueType = CentralServer.Common.PazCheckCentralServerConstants.ValueType_FixedList,
//    ValuesList = new string[] {
//                                CentralServer.Common.Properties.Resources.EventStatus_NotFinished,
//                                CentralServer.Common.Properties.Resources.EventStatus_Finished,
//                            }
//},

//Project project;
//try
//{
//    project = dbContext.Projects.Single(p => p.Id == projectId);
//}
//catch
//{
//    _logger.LogError("Invalid projectId: {0}", projectId);
//    taskCompletionSource.SetResult((null, null));
//    return;
//}