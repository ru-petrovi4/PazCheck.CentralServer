using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Diagnostics;
using Ssz.Utils.Logging;
using System.Text.RegularExpressions;
using Ssz.Dcs.CentralServer.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Simcode.PazCheck.CentralServer.Presentation.Grafana.Serialization;
using System.Collections.Frozen;
using Newtonsoft.Json;

namespace Simcode.PazCheck.CentralServer.MicroServices
{    
    public class PazCheckCentralServer
    {
        #region construction and destruction

        public PazCheckCentralServer(
            IMainServerWorker mainServerWorker,
            AddonsManager addonsManager,
            IHostApplicationLifetime hostApplicationLifetime,
            IConfiguration configuration,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            Cache cache,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<PazCheckCentralServer> logger)
        {
            _mainServerWorker = mainServerWorker;            
            HostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;                      
            _dbContextFactory = dbContextFactory;
            _cache = cache;
            _logger = logger;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;            
        }

        #endregion

        #region public functions

        public IHostApplicationLifetime HostApplicationLifetime { get; }

        /// <summary>
        ///     Immutable. Updated by PazCheckCentralServer service only in MainProcess.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<AddonsSource> AddonsSources { get; set; } = new();

        public void Initialize()
        {            
        }

        public void Close()
        {                     
        }

        public async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (nowUtc < _lastDoWorkDateTimeUtc + TimeSpan.FromSeconds(5))
                return;
            _lastDoWorkDateTimeUtc = nowUtc;            

            bool cleanUp = false;
            if (nowUtc > _lastCleanUpDateTimeUtc + TimeSpan.FromHours(24))
            {
                cleanUp = true;
                _lastCleanUpDateTimeUtc = nowUtc;
            }

            if (ConfigurationHelper.IsMainProcess(_configuration))
            {
                await using var dbContext = _dbContextFactory.CreateDbContext();

                if (_mainServerWorker.DataAccessProvider.IsConnected)
                    try
                    {
                        var reply = await _mainServerWorker.DataAccessProvider.PassthroughAsync(@"", PassthroughConstants.GetAddonStatuses, new byte[0]);
                        var replyAddonStatuses = new AddonStatuses();
                        Ssz.Utils.Serialization.SerializationHelper.SetOwnedData(replyAddonStatuses, reply);                    

                        Dictionary<string, Common.EntityFramework.AddonStatus> addonStatusesDictionary;
                        try
                        {
                            addonStatusesDictionary = dbContext.AddonStatuses.ToDictionary(a => Ssz.Utils.CsvHelper.FormatForCsv(@",", [a.SourcePath, a.AddonInstanceId]));
                        }
                        catch
                        {
                            // SQL injection safe
                            await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"AddonStatuses\"");
                            addonStatusesDictionary = new();
                        }

                        // [SourceId, List [AddonStatus]]
                        CaseInsensitiveOrderedDictionary<AddonsSource> newAddonsSources = new();

                        //bool changed = false;
                        foreach (var newAddonStatus in replyAddonStatuses.AddonStatusesCollection)
                        {
                            newAddonsSources.TryGetValue(newAddonStatus.SourceId, out AddonsSource? addonsSource);
                            if (addonsSource is null)
                            {
                                addonsSource = new AddonsSource
                                {
                                    SourceId = newAddonStatus.SourceId,
                                    SourceIdToDisplay = newAddonStatus.SourceIdToDisplay,
                                };
                                newAddonsSources.Add(newAddonStatus.SourceId, addonsSource);
                            }
                            if (new Any(newAddonStatus.Params.GetValueOrDefault(AddonBase.ParamName_IsResourceMonitoringAddon)).ValueAsBoolean(false))
                            {
                                addonsSource.ResourceMonitoringAddonStatus = newAddonStatus;
                            }
                            addonsSource.AddonStatuses.Add(newAddonStatus);

                            string key = Ssz.Utils.CsvHelper.FormatForCsv(@",", [newAddonStatus.SourcePath, newAddonStatus.AddonInstanceId]);
                            if (!addonStatusesDictionary.TryGetValue(key, out Common.EntityFramework.AddonStatus? addonStatus))
                            {
                                addonStatus = new Common.EntityFramework.AddonStatus
                                {
                                    SourcePath = newAddonStatus.SourcePath,
                                    AddonInstanceId = newAddonStatus.AddonInstanceId,
                                };
                                addonStatusesDictionary.Add(key, addonStatus);
                                dbContext.AddonStatuses.Add(addonStatus);
                                //_logger.LogError(@"Addon statuses error, deplicate key: " + key);

                                //changed = true;
                            }

                            addonStatus.TimestampUtc = nowUtc;
                            addonStatus.SourceId = newAddonStatus.SourceId;
                            addonStatus.SourceIdToDisplay = newAddonStatus.SourceIdToDisplay;
                            addonStatus.AddonGuid = newAddonStatus.AddonGuid;
                            addonStatus.AddonIdentifier = newAddonStatus.AddonIdentifier;
                            addonStatus.AddonDesc = newAddonStatus.AddonDesc;
                            addonStatus.LastWorkTimeUtc = newAddonStatus.LastWorkTimeUtc;
                            addonStatus.StateCode = newAddonStatus.StateCode;
                            addonStatus.Info = newAddonStatus.Info;
                            addonStatus.Label = newAddonStatus.Label;
                            addonStatus.Details = newAddonStatus.Details;

                            //if (changed || 
                            //        addonStatus.SourceId != newAddonStatus.SourceId ||
                            //        addonStatus.SourceIdToDisplay != newAddonStatus.SourceIdToDisplay ||
                            //        addonStatus.AddonGuid != newAddonStatus.AddonGuid ||
                            //        addonStatus.AddonIdentifier != newAddonStatus.AddonIdentifier ||
                            //        addonStatus.LastWorkTimeUtc != newAddonStatus.LastWorkTimeUtc ||
                            //        addonStatus.StateCode != newAddonStatus.StateCode ||
                            //        addonStatus.Info != newAddonStatus.Info ||
                            //        addonStatus.Label != newAddonStatus.Label ||
                            //        addonStatus.Details != newAddonStatus.Details)
                            //{
                            //    addonStatus.TimestampUtc = nowUtc;
                            //    addonStatus.SourceId = newAddonStatus.SourceId;
                            //    addonStatus.SourceIdToDisplay = newAddonStatus.SourceIdToDisplay;
                            //    addonStatus.AddonGuid = newAddonStatus.AddonGuid;
                            //    addonStatus.AddonIdentifier = newAddonStatus.AddonIdentifier;
                            //    addonStatus.LastWorkTimeUtc = newAddonStatus.LastWorkTimeUtc;
                            //    addonStatus.StateCode = newAddonStatus.StateCode;
                            //    addonStatus.Info = newAddonStatus.Info;
                            //    addonStatus.Label = newAddonStatus.Label;
                            //    addonStatus.Details = newAddonStatus.Details;

                            //    changed = true;
                            //}                
                        }

                        AddonsSources = newAddonsSources;

                        SerializationRootObject serializationRootObject = new();
                        serializationRootObject.BasePcObjects = new List<Common.Serialization.BasePcObject>()
                        {
                            new Common.Serialization.BasePcObject()
                            {
                                Identifier = PazCheckConstants.BasePcObject_SystemArea_Template,
                                Unit = PazCheckConstants.SystemUnitIdentifier,
                                Params = new List<Param>() { new Param { Name = PazCheckConstants.ParamName_Title, Value = Common.Properties.Resources.BasePcObject_SystemArea_Title } }
                            },
                            new Common.Serialization.BasePcObject()
                            {
                                Identifier = PazCheckConstants.BasePcObject_SystemProcess_Template,
                                Unit = PazCheckConstants.SystemUnitIdentifier,
                                Params = new List<Param>() { new Param { Name = PazCheckConstants.ParamName_Title, Value = Common.Properties.Resources.BasePcObject_SystemProcess_Title } }
                            },
                            new Common.Serialization.BasePcObject()
                            {
                                Identifier = PazCheckConstants.BasePcObject_SystemAddon_Template,
                                Unit = PazCheckConstants.SystemUnitIdentifier,
                                Params = new List<Param>() { new Param { Name = PazCheckConstants.ParamName_Title, Value = Common.Properties.Resources.BasePcObject_SystemAddon_Title } },
                            }
                        };
                        if (serializationRootObject.BasePcObjects.Any(bo => !PazCheckDbHelper.CheckBasePcObject(bo, _cache.DbCache)))
                            await SerializationHelper.ImportSerializationRootObjectAsync(
                                    serializationRootObject,
                                    new ImportMetadata()
                                    {
                                        RootCollectionMode = CollectionMode.Update,
                                        ChildCollectionMode = CollectionMode.Update,
                                        DataCollectionMode = CollectionMode.Update,
                                    },
                                    _dbContextFactory,
                                    @"",
                                    null,
                                    CancellationToken.None,
                                    NullJobProgress.Instance,
                                    LoggersSet.Empty,
                                    new ImportSerializationRootObjectResult(),
                                    preview: false);

                        lock (((System.Collections.ICollection)_mainServerWorker.SystemParams).SyncRoot)
                        {
                            serializationRootObject = new();
                            serializationRootObject.PcObjects = new List<Common.Serialization.PcObject>();
                            Common.Serialization.PcObject systemArea_PcObject = new Common.Serialization.PcObject()
                            {
                                Identifier = PazCheckConstants.PcObject_SystemArea,
                                Params = new List<Param>()
                            {
                                new Param { Name = PazCheckConstants.ParamName_Title, Value = Common.Properties.Resources.PcObject_SystemArea_Title },
                                new Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_SystemArea_Template },
                                new Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = @"" } // Root object
                            },
                                Unit = PazCheckConstants.SystemUnitIdentifier,
                            };
                            serializationRootObject.PcObjects.Add(systemArea_PcObject);
                            foreach (var addonsSource in newAddonsSources.Values)
                            {
                                // List (JournalParamName, systemValueKey)
                                List<(string, string)> journalParamsInfo = new();

                                string journalParamName_Source_CpuUsedPercentage = ComputerInfoHelper.ParamName_CpuUsedPercentage;
                                string systemValueKey_Source_CpuUsedPercentage = addonsSource.SourceId + "/" + journalParamName_Source_CpuUsedPercentage;
                                journalParamsInfo.Add((journalParamName_Source_CpuUsedPercentage, systemValueKey_Source_CpuUsedPercentage));

                                string journalParamName_Source_MemoryTotalInBytes = ComputerInfoHelper.ParamName_MemoryTotalInBytes;
                                string systemValueKey_Source_MemoryTotalInBytes = addonsSource.SourceId + "/" + journalParamName_Source_MemoryTotalInBytes;
                                journalParamsInfo.Add((journalParamName_Source_MemoryTotalInBytes, systemValueKey_Source_MemoryTotalInBytes));

                                string journalParamName_Source_MemoryUsedInBytes = ComputerInfoHelper.ParamName_MemoryUsedInBytes;
                                string systemValueKey_Source_MemoryUsedInBytes = addonsSource.SourceId + "/" + journalParamName_Source_MemoryUsedInBytes;
                                journalParamsInfo.Add((journalParamName_Source_MemoryUsedInBytes, systemValueKey_Source_MemoryUsedInBytes));

                                if (addonsSource.ResourceMonitoringAddonStatus is not null)
                                {
                                    _mainServerWorker.SystemParams[systemValueKey_Source_CpuUsedPercentage] = addonsSource.ResourceMonitoringAddonStatus.Params.GetValueOrDefault(ComputerInfoHelper.ParamName_CpuUsedPercentage);
                                    _mainServerWorker.SystemParams[systemValueKey_Source_MemoryTotalInBytes] = addonsSource.ResourceMonitoringAddonStatus.Params.GetValueOrDefault(ComputerInfoHelper.ParamName_MemoryTotalInBytes);
                                    _mainServerWorker.SystemParams[systemValueKey_Source_MemoryUsedInBytes] = addonsSource.ResourceMonitoringAddonStatus.Params.GetValueOrDefault(ComputerInfoHelper.ParamName_MemoryUsedInBytes);
                                    var drivesInfo = addonsSource.ResourceMonitoringAddonStatus.Params.GetValueOrDefault(ComputerInfoHelper.ParamName_DrivesInfo);
                                    if (drivesInfo.ValueTypeCode == Any.TypeCode.Dictionary)
                                    {
                                        int driveNum = -1;
                                        foreach (var kvp in drivesInfo.ValueAsDictionary().OrderBy(it => it.Key))
                                        {
                                            if (kvp.Value.ValueTypeCode == Any.TypeCode.Dictionary)
                                            {
                                                driveNum += 1;

                                                string journalParamName_Source_DriveTotalInBytes = $"Drive.{driveNum}/" + ComputerInfoHelper.ParamName_DriveTotalInBytes;
                                                string systemValueKey_Source_DriveTotalInBytes = addonsSource.SourceId + $"/" + journalParamName_Source_DriveTotalInBytes;
                                                journalParamsInfo.Add((journalParamName_Source_DriveTotalInBytes, systemValueKey_Source_DriveTotalInBytes));

                                                string journalParamName_Source_DriveUsedInBytes = $"Drive.{driveNum}/" + ComputerInfoHelper.ParamName_DriveUsedInBytes;
                                                string systemValueKey_Source_DriveUsedInBytes = addonsSource.SourceId + $"/" + journalParamName_Source_DriveUsedInBytes;
                                                journalParamsInfo.Add((journalParamName_Source_DriveUsedInBytes, systemValueKey_Source_DriveUsedInBytes));

                                                var driveInfo = kvp.Value.ValueAsDictionary();
                                                _mainServerWorker.SystemParams[systemValueKey_Source_DriveTotalInBytes] = driveInfo.GetValueOrDefault(ComputerInfoHelper.ParamName_DriveTotalInBytes);
                                                _mainServerWorker.SystemParams[systemValueKey_Source_DriveUsedInBytes] = driveInfo.GetValueOrDefault(ComputerInfoHelper.ParamName_DriveUsedInBytes);
                                            }
                                        }
                                    }
                                }

                                var addonsSource_PcObject = new Common.Serialization.PcObject()
                                {
                                    Identifier = addonsSource.SourceId,
                                    Params = new List<Param>()
                                {
                                    new Param { Name = PazCheckConstants.ParamName_Title, Value = addonsSource.SourceIdToDisplay },
                                    new Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_SystemProcess_Template },
                                    new Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = systemArea_PcObject.Identifier }
                                },
                                    Unit = PazCheckConstants.SystemUnitIdentifier,
                                };
                                foreach (var it in journalParamsInfo)
                                {
                                    addonsSource_PcObject.Params.Add(new Param()
                                    {
                                        Name = PazCheckConstants.ParamNamePrefix_Data + it.Item1,
                                        Value = NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveOrderedDictionary<string?>()
                                                {
                                                    { PazCheckConstants.ParamName_In,
                                                        "{\"" + PazCheckConstants.QueryPartName_QType + "\": \"" + PazCheckConstants.QueryType_System + "\", \"" +
                                                        PazCheckConstants.QueryPartName_QString + "\": " + JsonConvert.ToString(it.Item2) + "}" },
                                                    { PazCheckConstants.ParamName_TrendEnabled, @"true" },
                                                    { PazCheckConstants.ParamName_TrendStorePeriod, @"1M" },
                                                    { PazCheckConstants.ParamName_ValuePeriod, @"30s" },
                                                    { PazCheckConstants.ParamName_TrendPeriod, @"1h" },
                                                })
                                    });
                                }
                                addonsSource_PcObject.Params.Add(new Param()
                                {
                                    Name = PazCheckConstants.ParamNamePrefix_Data + PazCheckConstants.ParamName_SafetyIndex,
                                    Value = NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveOrderedDictionary<string?>()
                                            {
                                                { PazCheckConstants.ParamName_In,
                                                    "{\"" + PazCheckConstants.QueryPartName_QType + "\": \"" + PazCheckConstants.QueryType_System + "\", \"" +
                                                    PazCheckConstants.QueryPartName_QString + "\": " + JsonConvert.ToString(systemValueKey_Source_CpuUsedPercentage) + "}" },
                                                { PazCheckConstants.ParamName_TrendEnabled, @"true" },
                                                { PazCheckConstants.ParamName_TrendStorePeriod, @"1M" },
                                                { PazCheckConstants.ParamName_Converter, "i[0]<80->100---0" }
                                            })
                                });
                                serializationRootObject.PcObjects.Add(addonsSource_PcObject);

                                foreach (var addonsStatus in addonsSource.AddonStatuses)
                                {
                                    string systemValueKey_AddonStateCode = addonsStatus.SourceId + "/" + addonsStatus.AddonInstanceId + "/" + "StateCode";

                                    _mainServerWorker.SystemParams[systemValueKey_AddonStateCode] = new Any(addonsStatus.StateCode);

                                    var addonsStatusPcObject = new Common.Serialization.PcObject()
                                    {
                                        Identifier = addonsStatus.SourceId + "/" + addonsStatus.AddonInstanceId,
                                        Params = new List<Param>()
                                    {
                                        new Param { Name = PazCheckConstants.ParamName_Title, Value = addonsStatus.AddonDesc },
                                        new Param { Name = PazCheckConstants.ParamName_PcObjectTemplate, Value = PazCheckConstants.BasePcObject_SystemAddon_Template },
                                        new Param { Name = PazCheckConstants.ParamName_PcObjectParent, Value = addonsSource_PcObject.Identifier },
                                        new Param()
                                        {
                                            Name = PazCheckConstants.ParamNamePrefix_Data + PazCheckConstants.ParamName_SafetyIndex,
                                            Value = NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveOrderedDictionary<string?>()
                                            {
                                                { PazCheckConstants.ParamName_In,
                                                    "{\"" + PazCheckConstants.QueryPartName_QType + "\": \"" + PazCheckConstants.QueryType_System + "\", \"" +
                                                    PazCheckConstants.QueryPartName_QString + "\": " + JsonConvert.ToString(systemValueKey_AddonStateCode) + "}" },
                                                { PazCheckConstants.ParamName_TrendEnabled, @"true" },
                                                { PazCheckConstants.ParamName_TrendStorePeriod, @"1M" },
                                                { PazCheckConstants.ParamName_Converter, "i[0]==0->100---0" }
                                            })
                                        }
                                    },
                                        Unit = PazCheckConstants.SystemUnitIdentifier
                                    };
                                    serializationRootObject.PcObjects.Add(addonsStatusPcObject);
                                }
                            }
                        }

                        if (serializationRootObject.PcObjects.Any(o => !PazCheckDbHelper.CheckPcObject(o, _cache.DbCache)))
                            await SerializationHelper.ImportSerializationRootObjectAsync(
                                    serializationRootObject,
                                    new ImportMetadata()
                                    {
                                        RootCollectionMode = CollectionMode.Replace,
                                        ChildCollectionMode = CollectionMode.Replace,
                                        DataCollectionMode = CollectionMode.Update,
                                    },
                                    _dbContextFactory,
                                    @"",
                                    null,
                                    CancellationToken.None,
                                    NullJobProgress.Instance,
                                    LoggersSet.Empty,
                                    new ImportSerializationRootObjectResult(),
                                    preview: false);
                    }
                    catch
                    {
                    }

                if (cleanUp)
                {
                    DateTime forTemp_Limit = nowUtc - TimeSpan.FromDays(1) * 2;
                    DateTime forNotTemp_Limit = nowUtc - TimeSpan.FromDays(365) * 2;
                    foreach (var metaParam in dbContext.MetaParams)
                    {
                        bool clean = false;
                        if (metaParam.IsTemp)
                        {
                            if (metaParam._LastChangeTimeUtc < forTemp_Limit)
                                clean = true;
                        }
                        else
                        {
                            if (metaParam._LastChangeTimeUtc < forNotTemp_Limit)
                                clean = true;
                        }
                        if (clean)
                        {
                            dbContext.Remove(metaParam);
                            if (metaParam.HasArg)
                                dbContext.RemoveRange(dbContext.MetaParamArgs.Where(a => a.ParamName == metaParam.ParamName));
                        }
                    }
                }

                await dbContext.SaveChangesAsync(HostApplicationLifetime.ApplicationStopping);
            }                    
        }        

        #endregion

        #region private fields

        private readonly IConfiguration _configuration;        
        private readonly IMainServerWorker _mainServerWorker;        
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly Cache _cache;
        private readonly ILogger _logger;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private DateTime _lastDoWorkDateTimeUtc = DateTime.MinValue;
        private DateTime _lastCleanUpDateTimeUtc = DateTime.MinValue;

        #endregion        
    }
}