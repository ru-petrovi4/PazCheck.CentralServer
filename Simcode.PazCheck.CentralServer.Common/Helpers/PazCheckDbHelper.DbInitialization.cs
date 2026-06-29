using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions

        public const string DefaultUnitIdentifier = "mlsp";        
        public const string MainProjectTitle = "Основной";

        public const string PazCheckAdmins = @"mpaz-admin-test";
        public const string PazCheckIsAdmins = @"mpaz-adminIB-test";
        public const string PazCheckEngineers = @"mpaz-engineer-test";
        public const string PazCheckSupervisors = @"mpaz-supervisor-test";
        public const string PazCheckObservers = @"mpaz-viewer-test";
        public const string WidgetsLongRunningViewers = @"mpaz-longrunningviewer-test";

        /// <summary>
        ///     Occurs when process started with -u key.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="loggersSet"></param>
        public static void InitializeMainDb(IServiceProvider serviceProvider, ILoggersSet loggersSet)
        {
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PazCheckDbContext>>();

            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.IsLastChangeFieldsUpdatingDisabled = true;
            dbContext.IsInformationSecurityEventsLoggingDisabled = true;            

            try
            {
                // Applies any pending migrations for the context to the database. Will create the database
                // if it does not already exist.
                dbContext.Database.Migrate();

                dbContext.Database.ExecuteSql($"DELETE FROM \"UserEvents\"");
            }
            catch (Exception ex) 
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.InitializeMainDb, dbContext.Database.Migrate();");
                throw;
            }
            
            if (dbContext.MetaParams.FirstOrDefault(mp => mp.ParamName == PazCheckConstants.MetaParamName_RolesDataGuid) is null) // No initial user data
            {
                dbContext.Database.ExecuteSql($"DELETE FROM \"RolePermissions\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleApiFunctionRoleBusinessFunction\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleBusinessFunctions\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleApiFunctions\"");
                InitializeDb_RolePermissions(dbContext);

                var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);
                UpdateMetaParams(dbContext, metaParams); // Needed for UpdateMainDb(...)
                
                try
                {
                    // Units
                    var mlspUnit = new Unit { Identifier = DefaultUnitIdentifier, Title = "МЛСП", Desc = "Морская ледостойкая стационарная платформа" };
                    dbContext.Units.Add(mlspUnit);                    

                    // Projects                
                    var mainProject = new Project { Title = MainProjectTitle, Desc = "" };
                    mlspUnit.Projects.Add(mainProject);                    

                    dbContext.SaveChanges();

                    InitializePostgresCrypto(dbContext);
                }
                catch
                {
                }
            }            

            UpdateMainDb(serviceProvider, loggersSet);
            
            //var unit = dbContext.Units.Single(u => u.Identifier == DefaultUnitIdentifier);
            //foreach (int i in Enumerable.Range(0, 700 * 20))
            //{
            //    dbContext.UnitEventsIntervals.Add(new UnitEventsInterval
            //    {
            //        LoadTimeUtc = DateTime.UtcNow,
            //        Source = @"test",
            //        Comment = "commnet",
            //        BeginTimeUtc = DateTime.UtcNow - TimeSpan.FromMinutes(70),
            //        EndTimeUtc = DateTime.UtcNow - TimeSpan.FromMinutes(10),
            //        Unit = unit
            //    });                
            //}
            //dbContext.SaveChanges();
        }        

        /// <summary>
        ///     Occurs every process start.
        ///     Precondition, DB must be Initialized
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="loggersSet"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void UpdateMainDb(IServiceProvider serviceProvider, ILoggersSet loggersSet)
        {
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PazCheckDbContext>>();

            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.IsLastChangeFieldsUpdatingDisabled = true;
            dbContext.IsInformationSecurityEventsLoggingDisabled = true;

            #region Obsolete - PreMigrate

            //if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 7) 
            //{                
            //}

            #endregion

            try
            {
                // Applies any pending migrations for the context to the database. Will create the database
                // if it does not already exist.
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.UpdateMainDb, dbContext.Database.Migrate();");
                throw;
            }

            var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);

            MetaParam? metaParam_RolesDataGuid = metaParams.GetValueOrDefault(PazCheckConstants.MetaParamName_RolesDataGuid);            
            if (metaParam_RolesDataGuid is null)
            {
                throw new InvalidOperationException(@"Database is not initialized.");
            } 
            if (metaParam_RolesDataGuid.Value != PazCheckConstants.MetaParamValue_RolesDataGuid)
            {
                metaParam_RolesDataGuid.Value = PazCheckConstants.MetaParamValue_RolesDataGuid;
                metaParam_RolesDataGuid._LastChangeTimeUtc = DateTime.UtcNow;

                dbContext.Database.ExecuteSql($"DELETE FROM \"RolePermissions\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleApiFunctionRoleBusinessFunction\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleBusinessFunctions\"");
                dbContext.Database.ExecuteSql($"DELETE FROM \"RoleApiFunctions\"");
                InitializeDb_RolePermissions(dbContext);
            }

            #region Obsolete - PostMigrate

            if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 1)
            {
                if (Directory.Exists(@"CsvDb"))
                {
                    foreach (var d in Directory.GetDirectories(@"CsvDb", @"Logging.*"))
                    {
                        try
                        {
                            Directory.Delete(d, true);
                        }
                        catch
                        {
                        }
                    }
                    foreach (var d in Directory.GetDirectories(@"CsvDb", @"Diagnost.*"))
                    {
                        try
                        {
                            Directory.Delete(d, true);
                        }
                        catch
                        {
                        }
                    }
                    foreach (var d in Directory.GetDirectories(@"CsvDb", @"Monitoring.*"))
                    {
                        try
                        {
                            Directory.Delete(d, true);
                        }
                        catch
                        {
                        }
                    }
                    foreach (var d in Directory.GetDirectories(@"CsvDb", @"PazCheckCentralServer.*"))
                    {
                        try
                        {
                            Directory.Delete(d, true);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 9)
            {
                dbContext.Database.ExecuteSql($"UPDATE \"PcObjects\" SET \"_IsDeleted\"=TRUE");
                dbContext.Database.ExecuteSql($"UPDATE \"BasePcObjects\" SET \"_IsDeleted\"=TRUE");

                dbContext.SaveChanges();
            }

            if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 10)
            {
                foreach (var pcObjectEvent in dbContext.PcObjectEvents)
                {
                    var params_ = pcObjectEvent.Params;
                    params_ = params_.Replace("MaxActuationTime", PazCheckConstants.ParamName_CommandExecutionDurationMax);
                    params_ = params_.Replace("LogicActuationType", PazCheckConstants.ParamName_LogicCommandResultType);
                    params_ = params_.Replace("LogicActuationTime", PazCheckConstants.ParamName_LogicCommandExecutionDuration);
                    params_ = params_.Replace("CommandActuationType", PazCheckConstants.ParamName_JournalCommandResultType);
                    params_ = params_.Replace("CommandActuationTime", PazCheckConstants.ParamName_JournalCommandExecutionDuration);
                    pcObjectEvent.Params = params_;
                }

                foreach (var resultEvent in dbContext.ResultEvents)
                {
                    var params_ = resultEvent.Params;
                    params_ = params_.Replace("MaxActuationTime", PazCheckConstants.ParamName_CommandExecutionDurationMax);
                    params_ = params_.Replace("LogicActuationType", PazCheckConstants.ParamName_LogicCommandResultType);
                    params_ = params_.Replace("LogicActuationTime", PazCheckConstants.ParamName_LogicCommandExecutionDuration);
                    params_ = params_.Replace("CommandActuationType", PazCheckConstants.ParamName_JournalCommandResultType);
                    params_ = params_.Replace("CommandActuationTime", PazCheckConstants.ParamName_JournalCommandExecutionDuration);
                    resultEvent.Params = params_;
                }

                dbContext.SaveChanges();
            }

            if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 6)
            {
                dbContext.Database.ExecuteSql($"DELETE FROM \"UserEvents\"");
            }

            var unit = dbContext.Units.FirstOrDefault(u => u.IdentifierLower == "mlsp");
            if (GetMetaParamValue<int>(metaParams, PazCheckConstants.MetaParamName_CsvDbVersion, 0) < 7)
            {   
                if (unit is not null)
                {
                    unit.Identifier = DefaultUnitIdentifier;
                    dbContext.SaveChanges();
                }
            }

            #endregion

            #region system Unit
            var systemUnit = dbContext.Units.FirstOrDefault(u => u.IdentifierLower == PazCheckConstants.SystemUnitIdentifier_LowerCase);
            if (systemUnit is null)
            {
                systemUnit = new Unit { Identifier = PazCheckConstants.SystemUnitIdentifier, Title = "Система", Desc = "Системные настройки ИТ-решения" };
                dbContext.Units.Add(systemUnit);
                dbContext.SaveChanges();
            }
            #endregion

            try
            {
                AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamName_CsvDbVersion,
                    paramValue: PazCheckConstants.MetaParamValue_CsvDbVersion,
                    paramType: @"",
                    isTemp: false,
                    group: @"",
                    method: @"",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

                UpdateMetaParams(dbContext, metaParams);
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.UpdateMainDb, UpdateMetaParams(...);");                
            }            

            try
            {
                UpdateStdParamDescs(dbContext);
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.UpdateMainDb, UpdateStdParamDescs(...);");
            }

            try
            {
                UpdateStdPcEntityTypes(dbContext);
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.UpdateMainDb, UpdateStdPcEntityTypes(...);");
            }

            try
            {
                if (unit is not null)
                    UpdateStdBasePcObjects(dbContext, unit);
            }
            catch (Exception ex)
            {
                loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.UpdateMainDb, UpdateStdBasePcObjects(...);");
            }            
        }

        /// <summary>
        ///     Notify project changed.
        ///     <para>Method does not save DbContext!</para>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="metaParams"></param>
        /// <param name="project_ChangedMessage"></param>
        public static void AddOrUpdateMetaParam_Project(
            PazCheckDbContext dbContext,
            CaseInsensitiveOrderedDictionary<MetaParam> metaParams,
            Serialization.Project_ChangedMessage project_ChangedMessage)
        {
            string arg = NameValueCollectionHelper.GetNameValueCollectionString(
                [
                    ("ProjectId", project_ChangedMessage.ProjectId.ToString())
                ]);
            AddOrUpdateMetaParam(
                dbContext,
                metaParams,
                paramName: GetMetaParamName([ PazCheckConstants.MetaParamNameBase_Project, arg, project_ChangedMessage.HubConnectionIds ]),
                paramValue: Guid.NewGuid().ToString(),
                paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                isTemp: true, // Cleared periodically
                group: @"",
                method: PazCheckConstants.HubMethod_Project_Changed,
                hasArg: true,
                excludeConnectionIds: project_ChangedMessage.HubConnectionIds,
                arg: arg);
        }

        /// <summary>
        ///     M<para>Method does not save DbContext!</para>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="metaParams"></param>
        /// <param name="projectId"></param>
        /// <param name="isPaused"></param>
        public static void AddOrUpdateMetaParam_Pause_HubMethod_Project_Changed(
            PazCheckDbContext dbContext,
            CaseInsensitiveOrderedDictionary<MetaParam> metaParams,
            int projectId,
            bool isPaused)
        {
            string arg = NameValueCollectionHelper.GetNameValueCollectionString(
                [
                    ("ProjectId", projectId.ToString())
                ]);
            AddOrUpdateMetaParam(
                dbContext,
                metaParams,
                paramName: GetMetaParamName([ PazCheckConstants.MetaParamNameBase_Paused, PazCheckConstants.MetaParamNameBase_Project, arg ]),
                paramValue: new Any(isPaused).ValueAsString(false),
                paramType: PazCheckConstants.MetaParam_Type_MetaParam_Paused,
                isTemp: true, // Cleared periodically
                group: @"",
                method: @"",
                hasArg: false,
                excludeConnectionIds: @"",
                arg: @"");
        }

        /// <summary>
        ///     If paramValue is null - value is not set.   
        ///     <para>Method does not save DbContext!</para>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="metaParams"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <param name="paramType"></param>
        /// <param name="isTemp"></param>
        /// <param name="group"></param>
        /// <param name="method"></param>
        /// <param name="hasArg"></param>
        /// <param name="excludeConnectionIds"></param>
        /// <param name="arg">See <see cref="Ssz.Utils.NameValueCollectionHelper"/></param>
        public static void AddOrUpdateMetaParam(
            PazCheckDbContext dbContext,
            CaseInsensitiveOrderedDictionary<MetaParam> metaParams,
            string paramName,
            string? paramValue,
            string paramType,
            bool isTemp,
            string group,
            string method,
            bool hasArg,
            string excludeConnectionIds,
            string arg
            )
        {
            if (metaParams.TryGetValue(paramName, out MetaParam? metaParam))
            {
                metaParam.ParamName = paramName; // Because of case-sensitive issues                
            }
            else
            {
                metaParam = new MetaParam
                {
                    ParamName = paramName,
                    _LastChangeTimeUtc = DateTime.UtcNow
                };
                dbContext.MetaParams.Add(metaParam);
                metaParams.Add(paramName, metaParam);
            }
            if (paramValue is not null && paramValue != metaParam.Value)
            {
                metaParam.Value = paramValue;
                metaParam._LastChangeTimeUtc = DateTime.UtcNow;
            }
            metaParam.Type = paramType;
            metaParam.IsTemp = isTemp;
            metaParam.Group = group;
            metaParam.Method = method;
            metaParam.HasArg = hasArg;
            if (metaParam.HasArg)
            {
                var paramNameLower = paramName.ToLowerInvariant();
                var metaParamArg = dbContext.MetaParamArgs.FirstOrDefault(a => a.ParamNameLower == paramNameLower);
                if (metaParamArg is null)
                {
                    metaParamArg = new MetaParamArg
                    {
                        ParamName = paramName,
                        ParamNameLower = paramNameLower,
                    };
                    dbContext.MetaParamArgs.Add(metaParamArg);
                }
                metaParamArg.ExcludeConnectionIds = excludeConnectionIds;
                metaParamArg.Arg = arg;
            }
        }

        /// <summary>
        ///     Returns defaultValue if metaParamName is not found or value is String.Empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metaParams"></param>
        /// <param name="metaParamName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetMetaParamValue<T>(IReadOnlyDictionary<string, MetaParam> metaParams, string metaParamName, T defaultValue)
            where T : notnull
        {
            metaParams.TryGetValue(metaParamName, out var metaParam);
            string? valueString = metaParam?.Value;
            if (String.IsNullOrEmpty(valueString))
                return defaultValue;
            var result = new Any(valueString!).ValueAs<T>(false);
            if (result is null)
                return defaultValue;
            return result;
        }

        #endregion

        #region private fields

        private static void InitializePostgresCrypto(PazCheckDbContext dbContext)
        {
            try
            {
                // SQL injection safe
                dbContext.Database.ExecuteSql($"CREATE EXTENSION pgcrypto SCHEMA public");
            }
            catch
            {
            }            
        }

        /// <summary>
        ///     Saves changes.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="metaParams"></param>
        private static void UpdateMetaParams(PazCheckDbContext dbContext, CaseInsensitiveOrderedDictionary<MetaParam> metaParams)
        {
            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamName_RolesDataGuid,
                    paramValue: PazCheckConstants.MetaParamValue_RolesDataGuid,
                    paramType: @"",
                    isTemp: false,
                    group: @"",
                    method: @"",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamName_CsvDbVersion,
                    paramValue: null, // No Update
                    paramType: @"",
                    isTemp: false,
                    group: @"",
                    method: @"",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid,
                    paramValue: null, // No Update
                    paramType: @"",
                    isTemp: false,
                    group: @"",
                    method: @"",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamNameBase_JournalParamValuesCollection_Guid,
                    paramValue: null, // No Update
                    paramType: @"",
                    isTemp: false,
                    group: @"",
                    method: @"",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamNameBase_Monitoring_Config,
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: PazCheckConstants.HubGroup_MonitoringSubscribe,
                    method: PazCheckConstants.HubMethod_Monitoring_ConfigChanged,
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: PazCheckConstants.MetaParamNameBase_Monitoring_Data,
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: PazCheckConstants.HubGroup_MonitoringSubscribe,
                    method: PazCheckConstants.HubMethod_Monitoring_DataChanged,
                    hasArg: true,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(InformationSecurityEvent) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(InformationSecurityEvent) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(AllRolesAccessInformationSecurityEvent) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(AllRolesAccessInformationSecurityEvent) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(UserEvent) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(UserEvent) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(InformationMessage) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(InformationMessage) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(RequestMessage) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(RequestMessage) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(UnitEventsInterval) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(UnitEventsInterval) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            AddOrUpdateMetaParam(
                    dbContext,
                    metaParams,
                    paramName: nameof(UnitEvent) + @"_Changed_Guid",
                    paramValue: null,
                    paramType: PazCheckConstants.MetaParam_Type_HubEvent,
                    isTemp: false,
                    group: @"",
                    method: nameof(UnitEvent) + @"_Changed",
                    hasArg: false,
                    excludeConnectionIds: @"",
                    arg: @""
                );

            dbContext.SaveChanges();
        }

        /// <summary>
        ///     Saves changes.
        /// </summary>
        /// <param name="dbContext"></param>
        private static void UpdateStdParamDescs(PazCheckDbContext dbContext)
        {
            var newParamDescsData = new (string, string, int, string, CaseInsensitiveOrderedDictionary<string?>)[]
            {
                (PazCheckConstants.ParamName_CeMatrixSource, Properties.Resources.ParamDesc_CeMatrixSource, 0, @"", new()),                
                (PazCheckConstants.ParamName_TagSource, Properties.Resources.ParamDesc_TagSource, 0, @"", new()),
                (PazCheckConstants.ParamName_BaseActuatorSource, Properties.Resources.ParamDesc_BaseActuatorSource, 0, @"", new()),
                (PazCheckConstants.ParamName_SafetyControllerSource, Properties.Resources.ParamDesc_SafetyControllerSource, 0, @"", new()),
                (PazCheckConstants.ParamName_LegendSource, Properties.Resources.ParamDesc_LegendSource, 0, @"", new()),
                (PazCheckConstants.ParamName_ProjectVersionSource, Properties.Resources.ParamDesc_ProjectVersionSource, 0, @"", new()),
                (PazCheckConstants.ParamName_Title, Properties.Resources.ParamDesc_Title, 3, @"", new()),
                (PazCheckConstants.ParamName_Desc, Properties.Resources.ParamDesc_Desc, 2, @"", new()),
                (PazCheckConstants.ParamName_IsVariable, Properties.Resources.ParamDesc_TagIsVariable, 3, PazCheckConstants.DataType_Boolean, new()
                {
                    { "false", Properties.Resources.No },
                    { "true", Properties.Resources.Yes }
                }),
                (PazCheckConstants.ParamName_CanBeCause, Properties.Resources.ParamDesc_CanBeCause, 3, PazCheckConstants.DataType_Boolean, new()
                {
                    { "false", Properties.Resources.No },
                    { "true", Properties.Resources.Yes }
                }),
                (PazCheckConstants.ParamName_CanBeEffect, Properties.Resources.ParamDesc_CanBeEffect, 3, PazCheckConstants.DataType_Boolean, new()
                {
                    { "false", Properties.Resources.No },
                    { "true", Properties.Resources.Yes }
                }),
                (PazCheckConstants.ParamName_CeMatrixTemplate, Properties.Resources.ParamDesc_CeMatrixTemplate, 1, @"", new()),
                (PazCheckConstants.ParamName_TagTemplate, Properties.Resources.ParamDesc_TagTemplate, 1, @"", new()),
                (PazCheckConstants.ParamName_TagActuator, Properties.Resources.ParamDesc_TagActuator, 2, @"", new()),                
                (PazCheckConstants.ParamName_ActuatorTemplate, Properties.Resources.ParamDesc_BaseActuatorTemplate, 1, @"", new()),
                (PazCheckConstants.ParamName_PcObjectTemplate, Properties.Resources.ParamDesc_PcObjectTemplate, 1, @"", new()),
                (PazCheckConstants.ParamName_PcObjectParent, Properties.Resources.ParamDesc_PcObjectParent, 1, @"", new()),
                //(PazCheckCentralServerConstants.ParamName_LegendTemplate, Properties.Resources.ParamDesc_LegendTemplate, 1, @"", new()),
                (PazCheckConstants.ParamName_Manufacturer, Properties.Resources.ParamDesc_Manufacturer, 0, @"", new()),
                (PazCheckConstants.ParamName_Comment, Properties.Resources.ParamDesc_Comment, 0, @"", new()),
                (PazCheckConstants.ParamName_Command_ToDisplay, Properties.Resources.ParamDesc_Command_ToDisplay, 0, @"", new()),                
                (PazCheckConstants.ParamName_CommandExecutionDurationMax, Properties.Resources.ParamDesc_MaxActuationTime, 0, PazCheckConstants.DataType_TimeSpan, new() { { PazCheckConstants.MetadataParamName_Format, @"m\ \м\и\н\ s\.f\ \с\е\к" } }),
                (PazCheckConstants.ParamName_CommandExecutionDurationMax + "[" + PazCheckConstants.ParamIndex_ValveClose + "]", Properties.Resources.ParamDesc_MaxActuationTimeClose, 0, PazCheckConstants.DataType_TimeSpan, new() { { PazCheckConstants.MetadataParamName_Format, @"m\ \м\и\н\ s\.f\ \с\е\к" } }),
                (PazCheckConstants.ParamName_CommandExecutionDurationMax + "[" + PazCheckConstants.ParamIndex_ValveOpen + "]", Properties.Resources.ParamDesc_MaxActuationTimeOpen, 0, PazCheckConstants.DataType_TimeSpan, new() { { PazCheckConstants.MetadataParamName_Format, @"m\ \м\и\н\ s\.f\ \с\е\к" } }),
                //(PazCheckConstants.ParamName_CommandResultType, Properties.Resources.ParamDesc_CommandResultType, 0, PazCheckConstants.DataType_Enum, new()
                //{
                //    { new Any(TriggeredType.SuccessFirstTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessFirstTriggered },
                //    { new Any(TriggeredType.SuccessTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessTriggered },
                //    { new Any(TriggeredType.LateTriggered).ValueAsString(false), Properties.Resources.ParamValue_LateTriggered },
                //    { new Any(TriggeredType.NotTriggered).ValueAsString(false), Properties.Resources.ParamValue_NotTriggered },
                //    { new Any(TriggeredType.FaultTriggered).ValueAsString(false), Properties.Resources.ParamValue_FaultTriggered },
                //}),
                (PazCheckConstants.ParamName_LogicCommandResultType, Properties.Resources.ParamDesc_LogicActuationType, 0, PazCheckConstants.DataType_Enum, new()
                {
                    { new Any(TriggeredType.SuccessFirstTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessFirstTriggered },
                    { new Any(TriggeredType.SuccessTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessTriggered },
                    { new Any(TriggeredType.LateTriggered).ValueAsString(false), Properties.Resources.ParamValue_LateTriggered },
                    { new Any(TriggeredType.NotTriggered).ValueAsString(false), Properties.Resources.ParamValue_NotTriggered },
                    { new Any(TriggeredType.FaultTriggered).ValueAsString(false), Properties.Resources.ParamValue_FaultTriggered },
                }),
                (PazCheckConstants.ParamName_LogicCommandExecutionDuration, Properties.Resources.ParamDesc_LogicActuationTime, 0, PazCheckConstants.DataType_TimeSpan, new() { { PazCheckConstants.MetadataParamName_Format, @"m\ \м\и\н\ s\.f\ \с\е\к" } }),
                (PazCheckConstants.ParamName_JournalCommandResultType, Properties.Resources.ParamDesc_CommandActuationType, 0, PazCheckConstants.DataType_Enum, new()
                {
                    { new Any(TriggeredType.SuccessFirstTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessFirstTriggered },
                    { new Any(TriggeredType.SuccessTriggered).ValueAsString(false), Properties.Resources.ParamValue_SuccessTriggered },
                    { new Any(TriggeredType.LateTriggered).ValueAsString(false), Properties.Resources.ParamValue_LateTriggered },
                    { new Any(TriggeredType.NotTriggered).ValueAsString(false), Properties.Resources.ParamValue_NotTriggered },
                    { new Any(TriggeredType.FaultTriggered).ValueAsString(false), Properties.Resources.ParamValue_FaultTriggered },
                }),
                (PazCheckConstants.ParamName_JournalCommandExecutionDuration, Properties.Resources.ParamDesc_CommandActuationTime, 0, PazCheckConstants.DataType_TimeSpan, new() { { PazCheckConstants.MetadataParamName_Format, @"m\ \м\и\н\ s\.f\ \с\е\к" } }),
                (PazCheckConstants.ParamName_MalfunctionCategory, Properties.Resources.ParamDesc_MalfunctionCategory, 0, @"", new()),
                (PazCheckConstants.ParamName_AlarmIntensity, Properties.Resources.ParamDesc_AlarmIntensity, 0, @"", new()),
                (PazCheckConstants.ParamName_EmergencyShutdownLevel, Properties.Resources.ParamDesc_EmergencyShutdownLevel, 0, @"", new()),
                (PazCheckConstants.ParamName_ResultEventTagName, Properties.Resources.ParamDesc_ResultEventTagName, 0, @"", new()),
                (PazCheckConstants.ParamName_ResultEventDesc, Properties.Resources.ParamDesc_ResultEventDesc, 0, @"", new()),
                (PazCheckConstants.ParamName_ResultEventTime, Properties.Resources.ParamDesc_ResultEventTime, 0, PazCheckConstants.DataType_DateTime, new() { { PazCheckConstants.MetadataParamName_Format, "dd.MM.yyyy H:mm:ss.FFF"} }),                
                (PazCheckConstants.ParamName_Strict_PrimeCause_TagName, Properties.Resources.ParamDesc_Strict_PrimeCause_TagName, 0, @"", new()),
                //(PazCheckConstants.ParamName_Strict_PrimeCause_Time, Properties.Resources.ParamDesc_Strict_PrimeCause_Time, 0, @"", new()),
                //(PazCheckConstants.ParamName_Strict_PrimeCause_Condition, Properties.Resources.ParamDesc_Strict_PrimeCause_Condition, 0, @"", new()),
                //(PazCheckConstants.ParamName_PossibleResultSources, Properties.Resources.ParamDesc_PossibleResultSources, 0, @"", new()),
                (PazCheckConstants.ParamName_SafetyIndexK, Properties.Resources.ParamDesc_SafetyIndexK, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_SafetyIndexK2, Properties.Resources.ParamDesc_SafetyIndexK2, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_Reference_ResultId, Properties.Resources.ResultReference, 1, PazCheckConstants.DataType_Int32, new()),                             

                (PazCheckConstants.ParamName_OldState, Properties.Resources.ParamDesc_OldState, 0, @"", new()),
                (PazCheckConstants.ParamName_NewState, Properties.Resources.ParamDesc_NewState, 0, @"", new()),

                (PazCheckConstants.ParamName_SafetyIndex_Green_Percentage, Properties.Resources.ParamDesc_SafetyIndex_Green_Percentage, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_SafetyIndex_Green_Desc, Properties.Resources.ParamDesc_SafetyIndex_Green_Desc, 0, @"", new()),
                (PazCheckConstants.ParamName_SafetyIndex_Yellow_Percentage, Properties.Resources.ParamDesc_SafetyIndex_Yellow_Percentage, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_SafetyIndex_Yellow_Desc, Properties.Resources.ParamDesc_SafetyIndex_Yellow_Desc, 0, @"", new()),
                (PazCheckConstants.ParamName_SafetyIndex_Red_Percentage, Properties.Resources.ParamDesc_SafetyIndex_Red_Percentage, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_SafetyIndex_Red_Desc, Properties.Resources.ParamDesc_SafetyIndex_Red_Desc, 0, @"", new()),

                (PazCheckConstants.ParamName_CommandExecutionType_NotActivated, Properties.Resources.ParamDesc_NotActivated, 0, @"", new()),
                (PazCheckConstants.ParamName_CommandExecutionType_SuccessFirstTriggered, Properties.Resources.ParamDesc_SuccessFirstTriggered, 0, @"", new()),
                (PazCheckConstants.ParamName_CommandExecutionType_SuccessTriggered, Properties.Resources.ParamDesc_SuccessTriggered, 0, @"", new()),
                (PazCheckConstants.ParamName_CommandExecutionType_LateTriggered, Properties.Resources.ParamDesc_LateTriggered, 0, @"", new()),
                (PazCheckConstants.ParamName_CommandExecutionType_NotTriggered, Properties.Resources.ParamDesc_NotTriggered, 0, @"", new()),
                (PazCheckConstants.ParamName_CommandExecutionType_FaultTriggered, Properties.Resources.ParamDesc_FaultTriggered, 0, @"", new()),                                
                
                (PazCheckConstants.ParamName_In, Properties.Resources.ParamDesc_TrendSources, 5, @"", new()),
                (PazCheckConstants.ParamName_Converter, Properties.Resources.ParamDesc_Converter, 4, @"", new()),
                (PazCheckConstants.ParamName_ValueMax, Properties.Resources.ParamDesc_ValueMax, 3, @"", new()),
                (PazCheckConstants.ParamName_ValueMin, Properties.Resources.ParamDesc_ValueMin, 2, @"", new()),
                (PazCheckConstants.ParamName_ValueEU, Properties.Resources.ParamDesc_ValueEU, 1, @"", new()),                
                (PazCheckConstants.ParamName_ValueDeadbandPercentage, Properties.Resources.ParamDesc_ValueDeadbandPercentage, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_ValueDeadband, Properties.Resources.ParamDesc_ValueDeadband, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_ValuePeriod, Properties.Resources.ParamDesc_ValuePeriod, 0, @"", new()),
                (PazCheckConstants.ParamName_TrendDeadbandPercentage, Properties.Resources.ParamDesc_TrendDeadbandPercentage, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_TrendDeadband, Properties.Resources.ParamDesc_TrendDeadband, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_TrendPeriod, Properties.Resources.ParamDesc_TrendPeriod, 0, @"", new()),
                (PazCheckConstants.ParamName_TrendStorePeriod, Properties.Resources.ParamDesc_TrendStorePeriod, 0, @"", new()),
                (PazCheckConstants.ParamName_TrendCalculated, Properties.Resources.ParamDesc_TrendCalculated, 0, @"", new()),
                (PazCheckConstants.ParamName_TrendEnabled, Properties.Resources.ParamDesc_TrendEnabled, 0, PazCheckConstants.DataType_Boolean, new()
                {
                    { "false", Properties.Resources.No },
                    { "true", Properties.Resources.Yes }
                }),
                (PazCheckConstants.ParamName_TrendBeginTimeUtc, Properties.Resources.ParamDesc_TrendBeginTimeUtc, 0, PazCheckConstants.DataType_DateTime, new() { { PazCheckConstants.MetadataParamName_Format, "dd.MM.yyyy H:mm:ss.FFF" } }),
                (PazCheckConstants.ParamName_TrendEndTimeUtc, Properties.Resources.ParamDesc_TrendEndTimeUtc, 0, PazCheckConstants.DataType_DateTime, new() { { PazCheckConstants.MetadataParamName_Format, "dd.MM.yyyy H:mm:ss.FFF" } }),
                //(PazCheckCentralServerConstants.ParamName_EventStatus, Properties.Resources.ParamDesc_EventStatus, 0, PazCheckCentralServerConstants.DataType_Enum, new()
                //{
                //    { PazCheckCentralServerConstants.ParamValue_EventStatus_NotFinished, Properties.Resources.EventStatus_NotFinished },
                //    { PazCheckCentralServerConstants.ParamValue_EventStatus_Finished, Properties.Resources.EventStatus_Finished }
                //}),
                (PazCheckConstants.ParamName_SafetyIndex, Properties.Resources.ParamDesc_SafetyIndex, 0, PazCheckConstants.DataType_Single, new()),
                (PazCheckConstants.ParamName_SafetyIndexDesc, Properties.Resources.ParamDesc_SafetyIndexDesc, 0, @"", new()),                
                (PazCheckConstants.ParamName_EventTitle, Properties.Resources.ParamDesc_EventTitle, 5, @"", new()),
                (PazCheckConstants.ParamName_EventDesc, Properties.Resources.ParamDesc_EventDesc, 4, @"", new()),
                (PazCheckConstants.ParamName_EventTypeTitle, Properties.Resources.ParamDesc_EventTypeTitle, 3, @"", new()),
                (PazCheckConstants.ParamName_EventTypeDesc, Properties.Resources.ParamDesc_EventTypeDesc, 2, @"", new()),
                
                // TODO remove resources
                //(PazCheckCentralServerConstants.CriterionName_BasePcObject_Identifier, Properties.Resources.ParamDesc_BasePcObject_Identifier, 0, @"", new()),   
                //(PazCheckCentralServerConstants.CriterionName_Children, Properties.Resources.ParamDesc_Children, 0, @"", new()),
                //(PazCheckCentralServerConstants.CriterionName_Query, Properties.Resources.ParamDesc_Query, 0, @"", new()),
                //(PazCheckCentralServerConstants.CriterionName_QueryType, Properties.Resources.ParamDesc_QueryType, 0, @"", new()),
                //(PazCheckCentralServerConstants.CriterionName_From, Properties.Resources.ParamDesc_From, 0, PazCheckCentralServerConstants.DataType_DateTime, new() { {@"", "dd.MM.yyyy H:mm:ss.FFF"} }),
                //(PazCheckCentralServerConstants.CriterionName_To, Properties.Resources.ParamDesc_To, 0, PazCheckCentralServerConstants.DataType_DateTime, new() { {@"", "dd.MM.yyyy H:mm:ss.FFF"} }),
                //(PazCheckCentralServerConstants.CriterionName_EventTypeTitle, Properties.Resources.ParamDesc_EventTypeTitle, 0, @"", new()),
            };

            var paramDescs = dbContext.ParamDescs.ToDictionary(pd => pd.Id, StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var newParamDescData in newParamDescsData)
            {
                if (paramDescs.TryGetValue(newParamDescData.Item1, out ParamDesc? paramDesc))
                {
                    paramDesc.Id = newParamDescData.Item1; // Because of case-sensitive issues                    
                    paramDesc.Desc = newParamDescData.Item2;
                    paramDesc.Priority = newParamDescData.Item3;
                    paramDesc.DataType = newParamDescData.Item4;
                    paramDesc.MetadataFieldsDictionary = newParamDescData.Item5;

                    paramDescs.Remove(newParamDescData.Item1);
                }
                else
                {
                    paramDesc = new ParamDesc
                    {
                        Id = newParamDescData.Item1,                        
                        Desc = newParamDescData.Item2,
                        Priority = newParamDescData.Item3,
                        DataType = newParamDescData.Item4,
                        MetadataFieldsDictionary = newParamDescData.Item5,
                    };
                    dbContext.ParamDescs.Add(paramDesc);
                }
            }

            dbContext.ParamDescs.RemoveRange(paramDescs.Values);

            dbContext.SaveChanges();
        }

        /// <summary>
        ///     Saves changes.
        /// </summary>
        /// <param name="dbContext"></param>
        private static void UpdateStdPcEntityTypes(PazCheckDbContext dbContext)
        {
            // ProjectVersionTypes
            var newProjectVersionTypesData = new (string, string, string)[]
            {
                (PazCheckConstants.PcEntityType_Main, Properties.Resources.PcEntityType_Main, @"")                
            };
            var newProjectVersionTypes = UpdateStdPcEntityTypes(dbContext, dbContext.ProjectVersionTypes, newProjectVersionTypesData);            
            UpdateStdParamInfo(dbContext, newProjectVersionTypes[0], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_ProjectVersionSource,
            });                                  

            // PcObjectEventTypes
            var newPcObjectEventTypesData = new (string, string, string)[]
            {
                (PazCheckConstants.PcObjectEventType_ActuatorAction, Properties.Resources.PcObjectEventTypeTitle_ActuatorAction, @""),
                (PazCheckConstants.PcObjectEventType_EmergencyShutdown, Properties.Resources.PcObjectEventTypeTitle_EmergencyShutdown, @""),
                (PazCheckConstants.PcObjectEventType_ManualCheck, Properties.Resources.PcObjectEventTypeTitle_ManualCheck, @""),
                (PazCheckConstants.PcObjectEventType_DBK, Properties.Resources.PcObjectEventTypeTitle_DBK, @""),
                (PazCheckConstants.PcObjectEventType_ForcedVariable, Properties.Resources.PcObjectEventTypeTitle_ForcedVariable, @""),
            };
            var newPcObjectEventTypes = UpdateStdPcEntityTypes(dbContext, dbContext.PcObjectEventTypes, newPcObjectEventTypesData);
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[0], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_LogicCommandResultType,
            });
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[0], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_LogicCommandExecutionDuration,
            });
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[0], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_JournalCommandResultType,
            });            
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[0], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_JournalCommandExecutionDuration,
            });
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[1], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_EmergencyShutdownLevel,
            });
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[3], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_AlarmIntensity,
                DefaultValue = @"1"
            });
            UpdateStdParamInfo(dbContext, newPcObjectEventTypes[4], new ParamInfo
            {
                ParamName = PazCheckConstants.ParamName_AlarmIntensity,
                DefaultValue = @"1"
            });

            dbContext.SaveChanges();
        }        

        private static List<TPcEntityType> UpdateStdPcEntityTypes<TPcEntityType>(PazCheckDbContext dbContext, DbSet<TPcEntityType> dbSet, (string, string, string)[] data) 
            where TPcEntityType : PcEntityType, new()
        {
            var result = new List<TPcEntityType>();
            var pcEntityTypes = dbSet.Include(pet => pet.StandardParamInfos).ToDictionary(pd => pd.Type, StringComparer.InvariantCultureIgnoreCase);

            foreach (var newPcEntityTypeData in data)
            {
                if (pcEntityTypes.TryGetValue(newPcEntityTypeData.Item1, out TPcEntityType? pcEntityType))
                {
                    pcEntityType.Type = newPcEntityTypeData.Item1; // Because of case-sensitive issues
                    pcEntityType.Title = newPcEntityTypeData.Item2;
                    pcEntityType.Desc = newPcEntityTypeData.Item3;
                }
                else
                {
                    pcEntityType = new TPcEntityType
                    {
                        Type = newPcEntityTypeData.Item1,
                        Title = newPcEntityTypeData.Item2,
                        Desc = newPcEntityTypeData.Item3
                    };
                    dbSet.Add(pcEntityType);
                }
                result.Add(pcEntityType);
            }

            return result;
        }

        /// <summary>
        ///     Saves changes.
        /// </summary>        
        private static void UpdateStdBasePcObjects(PazCheckDbContext dbContext, Unit unit)
        {
            // ProjectVersionTypes
            var data = new []
            {
                new BasePcObject()
                {
                    Identifier = PazCheckConstants.BasePcObject_Overview_Template, 
                    Unit = unit,
                    ParamsDictionary = new CaseInsensitiveOrderedDictionary<string?>() { { PazCheckConstants.ParamName_Title, Properties.Resources.BasePcObject_Overview_Title } }
                },
                //new BasePcObject()
                //{
                //    Identifier = PazCheckConstants.BasePcObject_Default_Template,
                //    Unit = unit,
                //    ParamsDictionary = new CaseInsensitiveOrderedDictionary<string?>() { { PazCheckConstants.ParamName_Title, Properties.Resources.BasePcObject_Default_Title } }                    
                //}
            };
            UpdateStdBasePcObjects(dbContext, dbContext.BasePcObjects, data);
            
            dbContext.SaveChanges();
        }

        private static void UpdateStdBasePcObjects(PazCheckDbContext dbContext, DbSet<BasePcObject> dbSet, BasePcObject[] newBasePcObjects)            
        {            
            var basePcObjects = dbSet.Where(o => o._IsDeleted == false).ToDictionary(pd => pd.Unit.Identifier + "." + pd.Identifier, StringComparer.InvariantCultureIgnoreCase);

            foreach (var newBasePcObject in newBasePcObjects)
            {
                if (!basePcObjects.TryGetValue(newBasePcObject.Unit.Identifier + "." + newBasePcObject.Identifier, out BasePcObject? basePcObject))
                    dbSet.Add(newBasePcObject);
            }
        }

        private static void UpdateStdParamInfo<TPcEntityType>(PazCheckDbContext dbContext, TPcEntityType pcEntityType, ParamInfo paramInfo)
            where TPcEntityType : PcEntityType, new()
        {   
            var existingParamInfos = pcEntityType.StandardParamInfos.Where(pi => String.Equals(pi.ParamName, paramInfo.ParamName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (existingParamInfos.Count == 0) 
            {
                pcEntityType.StandardParamInfos.Add(paramInfo);
                dbContext.ParamInfos.Add(paramInfo);
            }            
            else
            {
                //if (existingParamInfos.Count > 1)

                var existingParamInfo = existingParamInfos[0];
                existingParamInfo.ParamName = paramInfo.ParamName; // Because of vase-sensivity issues.
                existingParamInfo.DefaultValue = paramInfo.DefaultValue;
                existingParamInfo.MetadataFields = paramInfo.MetadataFields;
            }
        }
        
        #endregion
    }
}

//public static void InitializeByScriptMainDb_IfNeeded(IServiceProvider serviceProvider)
//{
//    var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PazCheckDbContext>>();

//    using var dbContext = dbContextFactory.CreateDbContext();
//    try
//    {
//        if (dbContext.MetaParams.Count() > 0) // If any system data
//            return;
//    }
//    catch
//    {
//    }

//    var startInfo = new ProcessStartInfo();
//    // All environment variables of the created process are inherited from the
//    // current process
//    startInfo.EnvironmentVariables["PGPASSWORD"] = @"postgres";
//    // Required for EnvironmentVariables to be set
//    startInfo.UseShellExecute = false;
//    // The executable will be search in directories that are specified
//    // in the PATH variable of the current process
//    startInfo.FileName = @"psql.exe";

//    startInfo.Arguments = @"-U postgres -w -c ""DROP DATABASE pazcheck""";
//    var process = Process.Start(startInfo);
//    process!.WaitForExit();

//    startInfo.Arguments = @"-U postgres -w -c ""CREATE DATABASE pazcheck""";
//    process = Process.Start(startInfo);
//    process!.WaitForExit();

//    startInfo.Arguments = @"-U postgres -w -d pazcheck -f PazCheck.sql";
//    process = Process.Start(startInfo);
//    process!.WaitForExit();

//    InitializePostgresCrypto(dbContext);
//}


//Common.Serialization.SerializationRootObject? serializationRootObject = await Common.Serialization.SerializationHelper.GetSerializationRootObjectAsync(new Common.Serialization.ExportSerializationRootObjectInfo
//{
//    ProjectId = 1,
//    TagIdsExportAll = true,
//    BaseActuatorIdsExportAll = true,
//    SafetyControllerIdsExportAll = true,
//});
//if (serializationRootObject is not null)
//{
//    byte[]? jsonUtf8Bytes = null;
//    try
//    {
//        jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(serializationRootObject, Common.Serialization.SourceGenerationContext.Default.SerializationRootObject);
//    }
//    catch //(Exception ex)
//    {
//    }

//    if (jsonUtf8Bytes is not null)
//    {
//        string fileFullName = Path.Combine(ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration), "export.json");
//        using (var stream = File.Create(fileFullName))
//        {
//            stream.Write(new UTF8Encoding(true).GetPreamble());
//            stream.Write(jsonUtf8Bytes);
//        }
//    }
//    return;
//}

// =============
//{
//    using var dbContext = _dbContextFactory.CreateDbContext();
//    Project project;
//    try
//    {
//        project = dbContext.Projects.Single(p => p.Id == 1);
//    }
//    catch
//    {
//        return;
//    }

//    string fileFullName = Path.Combine(ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration), "export.json");
//    await using var stream = System.IO.File.OpenRead(fileFullName);
//    await Common.Serialization.SerializationHelper.ImportSerializationRootObjectFileAsync(stream, Path.GetFileName(fileFullName), dbContext, project, CancellationToken.None, DummyJobProgress.Default, LoggersSet<object>.Empty);
//    return;
//}


//
//try
//{
//    //dbContext.BasePcObjects
//    //        .Where(bpo => bpo._IsDeleted && bpo.Identifier == PazCheckCentralServerConstants.BasePcObject_SystemArea)
//    //        .ExecuteDelete();

//    dbContext.PcObjects
//            .Where(po => po.Parent!.Identifier == PazCheckCentralServerConstants.PcObject_SystemArea)
//            .ExecuteDelete();

//    dbContext.PcObjects
//            .Where(po => po.Identifier == PazCheckCentralServerConstants.PcObject_SystemArea)
//            .ExecuteDelete();

//    dbContext.BasePcObjects
//            .Where(bpo => bpo.Identifier == PazCheckCentralServerConstants.BasePcObject_SystemArea)
//            .ExecuteDelete();
//}
//catch (Exception ex)
//{
//    loggersSet.Logger.LogCritical(ex, @"PazCheckDbHelper.InitializeMainDb, ExecuteDelete();");
//    throw;
//}