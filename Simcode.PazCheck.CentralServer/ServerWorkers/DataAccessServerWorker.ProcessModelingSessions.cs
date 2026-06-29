using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ssz.Dcs.CentralServer.Common;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.DataAccessGrpc.Client;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Security.Cryptography;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions        

        public string InitiateProcessModelingSession(
            string clientApplicationName,
            string clientWorkstationName,
            string processModelName, 
            string instructorUserName,
            InstructorAccessFlags instructorAccessFlags,
            string mode)
        {
            DsFilesStoreDirectory? rootDsFilesStoreDirectory = DsFilesStoreHelper.CreateDsFilesStoreDirectoryObject(FilesStoreDirectoryInfo, @"", 1);

            var dsFileStoreDescriptors = DsFilesStoreHelper.GetDsFilesStoreDescriptors(rootDsFilesStoreDirectory);
            DsFilesStoreDescriptor? dsFilesStoreDescriptor = dsFileStoreDescriptors.FirstOrDefault(i => String.Equals(Path.GetFileName(i.RelativeToDescriptorFileOrDirectoryPath), processModelName, StringComparison.InvariantCultureIgnoreCase));
            DsFilesStoreItem? dsFilesStoreItem = null;
            if (dsFilesStoreDescriptor is not null)
            {
                dsFilesStoreItem = DsFilesStoreHelper.FindDsFilesStoreItem(rootDsFilesStoreDirectory, dsFilesStoreDescriptor.RelativeToDescriptorFileOrDirectoryPath);
                if (dsFilesStoreItem is null)
                    dsFilesStoreDescriptor = null;
            }            
            if (dsFilesStoreDescriptor is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid processModelName: " + processModelName));            

            CaseInsensitiveOrderedDictionary<List<string?>> data = CsvHelper.LoadCsvFile(Path.Combine(FilesStoreDirectoryInfo.FullName, dsFilesStoreDescriptor.DescriptorDsFileInfo.Name), true);

            var processModelingSession = new ProcessModelingSession(
                Guid.NewGuid().ToString(),
                clientApplicationName,
                clientWorkstationName,
                processModelName, 
                dsFilesStoreDescriptor.Title, 
                instructorUserName, 
                instructorAccessFlags, 
                mode)
            {
                ProcessModelingSessionStatus = ProcessModelingSessionConstants.Initiated,  
                ForTimeout_LastDateTimeUtc = DateTime.UtcNow,
            };
            _processModelingSessionsCollection.Add(processModelingSession.ProcessModelingSessionId, processModelingSession);

            try
            {
                //using (var dbContext = _dbContextFactory.CreateDbContext())
                //{
                //    if (dbContext.IsConfigured)
                //    {
                //        string enterprise = CsvDb.GetValue(data, "Enterprise", 1) ?? @"";
                //        string plant = CsvDb.GetValue(data, "Plant", 1) ?? @"";
                //        string unit = processModelingSession.ProcessModelNameToDisplay;

                //        var processModel = dbContext.ProcessModels.AsEnumerable().FirstOrDefault(pm => String.Equals(pm.ProcessModelName, processModelName, StringComparison.InvariantCultureIgnoreCase));
                //        if (processModel is null)
                //        {
                //            processModel = new Common.EntityFramework.ProcessModel();
                //            dbContext.ProcessModels.Add(processModel);
                //        }
                //        processModel.ProcessModelName = processModelName; // For case-sensivity issues
                //        processModel.Enterprise = enterprise;
                //        processModel.Plant = plant;
                //        processModel.Unit = unit;

                //        var originalScenarios = dbContext.Scenarios.Where(s => s.ProcessModel == processModel).ToList();
                //        foreach (string fullFileName in Directory.EnumerateFiles(Path.Combine(FilesStoreDirectoryInfo.FullName, dsFilesStoreItem!.DsFilesStoreDirectory.Name, DsFilesStorePazCheckCentralServerConstants.InstructorDataDirectoryName))
                //            .Where(ffn => ffn.EndsWith(@".scenario.csv", StringComparison.InvariantCultureIgnoreCase)))
                //        {
                //            CaseInsensitiveOrderedDictionary<List<string?>> scenarioData = CsvHelper.LoadCsvFile(fullFileName, true);
                //            string fileName = Path.GetFileName(fullFileName);
                //            string scenarioName = fileName.Substring(0, fileName.Length - @".scenario.csv".Length);
                //            var scenario = originalScenarios.FirstOrDefault(s => String.Equals(s.ScenarioName, scenarioName, StringComparison.InvariantCultureIgnoreCase));
                //            if (scenario is null)
                //            {
                //                scenario = new Common.EntityFramework.Scenario()
                //                {
                //                    ProcessModel = processModel
                //                };
                //                dbContext.Scenarios.Add(scenario);
                //            }
                //            else
                //            {
                //                originalScenarios.Remove(scenario);
                //            }
                //            scenario.ScenarioName = scenarioName; // For case-sensivity issues
                //            scenario.InitialConditionName = CsvDb.GetValue(scenarioData, "%(State)", 1) ?? @"";
                //            scenario.MaxPenalty = new Any(CsvDb.GetValue(scenarioData, "%(MaxPenalty)", 1) ?? @"").ValueAsInt32(false);
                //            scenario.ScenarioMaxProcessModelTimeSeconds = new Any(CsvDb.GetValue(scenarioData, "%(ScenarioMaxTimeSeconds)", 1) ?? @"").ValueAsUInt64(false);
                //        }
                //        foreach (var originalScenario in originalScenarios)
                //        {
                //            dbContext.Remove(originalScenario);
                //        }

                //        Common.EntityFramework.ProcessModelingSession? dbEnity_ProcessModelingSession = null;
                //        var instructorUser = dbContext.Users.FirstOrDefault(u => u.UserName == instructorUserName);
                //        if (instructorUser is not null)
                //        {
                //            dbEnity_ProcessModelingSession = new Common.EntityFramework.ProcessModelingSession
                //            {
                //                InstructorUser = instructorUser,
                //                StartDateTimeUtc = DateTime.UtcNow,
                //                ProcessModel = processModel,                                
                //            };
                //            dbContext.ProcessModelingSessions.Add(dbEnity_ProcessModelingSession);
                //        }

                //        dbContext.SaveChanges();

                //        if (dbEnity_ProcessModelingSession is not null)
                //            processModelingSession.DbEnity_ProcessModelingSessionId = dbEnity_ProcessModelingSession.Id;
                //    }                    
                //}
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, @"dbContext error.");
            }

            return processModelingSession.ProcessModelingSessionId;
        }        

        public async void ConcludeProcessModelingSession(string processModelingSessionId)
        {
            _processModelingSessionsCollection.Remove(processModelingSessionId, out ProcessModelingSession? processModelingSession);
            if (processModelingSession is null) 
                return;            

            try
            {
                if (processModelingSession.DbEnity_ProcessModelingSessionId is not null)
                {
                    //using (var dbContext = _dbContextFactory.CreateDbContext())
                    //{
                    //    if (dbContext.IsConfigured)
                    //    {
                    //        var dbEnity_ProcessModelingSession = dbContext.ProcessModelingSessions.FirstOrDefault(pms => pms.Id == processModelingSession.DbEnity_ProcessModelingSessionId.Value);
                    //        if (dbEnity_ProcessModelingSession is not null)
                    //        {
                    //            dbEnity_ProcessModelingSession.FinishDateTimeUtc = DateTime.UtcNow;
                    //            dbContext.SaveChanges();
                    //        };
                    //    }                        
                    //}                    
                }                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, @"dbContext error.");
            }

            var tasks = new List<Task>();
            foreach (EngineSession engineSession in processModelingSession.EngineSessions)
            {
                try
                {
                    tasks.Add(engineSession.DataAccessProvider.PassthroughAsync(@"", PassthroughConstants.Shutdown, new byte[0]));
                }
                catch
                {
                }
            }
            foreach (var t in tasks) 
            {
                try
                {
                    await t;
                }
                catch
                {
                }
            }            

            ThreadSafeDispatcher.BeginInvoke(ct =>
            {
                for (int collectionIndex = processModelingSession.EngineSessions.Count - 1; collectionIndex >= 0; collectionIndex -= 1)
                {
                    var engineSession = processModelingSession.EngineSessions[collectionIndex];
                    processModelingSession.EngineSessions.RemoveAt(collectionIndex);
                    engineSession.DataAccessProviderGetter_Addon.Close();
                }

                bool utilityItemsProcessingNeeded = false;
                foreach (OperatorSession operatorSession in OperatorSessionsCollection.Values
                    .Where(t => String.Equals(t.ProcessModelingSession?.ProcessModelingSessionId, processModelingSessionId, StringComparison.InvariantCultureIgnoreCase)).ToArray())
                {
                    OperatorSessionsCollection.Remove(operatorSession.OperatorSessionId);
                    utilityItemsProcessingNeeded = true;
                }

                ServerContextsAbort(processModelingSession.ProcessServerContextsCollection.ToArray());

                if (utilityItemsProcessingNeeded)
                    _utilityItemsDoWorkNeeded = true;
            });            
        }

        /// <summary>
        ///     Throws RpcException if incorrect processModelingSessionId
        /// </summary>
        /// <param name="processModelingSessionId"></param>
        /// <returns></returns>
        public ProcessModelingSession GetProcessModelingSession(string? processModelingSessionId)
        {
            ProcessModelingSession? processModelingSession = GetProcessModelingSessionOrNull(processModelingSessionId);            
            
            if (processModelingSession is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid processModelingSessionId: " + processModelingSessionId));

            return processModelingSession;
        }

        public ProcessModelingSession? GetProcessModelingSessionOrNull(string? processModelingSessionId)
        {
            ProcessModelingSession? processModelingSession = null;

            if (!String.IsNullOrEmpty(processModelingSessionId))
            {
                if (String.Equals(processModelingSessionId, DataAccessConstants.DefaultProcessModelingSessionId, StringComparison.InvariantCultureIgnoreCase))
                {
                    processModelingSession = _processModelingSessionsCollection.Values.FirstOrDefault();
                }
                else
                {
                    processModelingSession = _processModelingSessionsCollection.TryGetValue(processModelingSessionId);
                }                                
            }

            return processModelingSession;
        }        

        #endregion        

        public class ProcessModelingSession
        {
            #region construction and destruction

            public ProcessModelingSession(string processModelingSessionId,
                string initiatorClientApplicationName,
                string initiatorClientWorkstationName,
                string processModelName, 
                string processModelNameToDisplay, 
                string instructorUserName,
                InstructorAccessFlags instructorAccessFlags,
                string mode)
            {
                ProcessModelingSessionId = processModelingSessionId;                
                InitiatorClientApplicationName = initiatorClientApplicationName;
                InitiatorClientWorkstationName = initiatorClientWorkstationName;
                ProcessModelName = processModelName;
                ProcessModelNameToDisplay = processModelNameToDisplay;
                InstructorUserName = instructorUserName;
                InstructorAccessFlags = instructorAccessFlags;
                Mode = mode;
            }

            #endregion

            #region public functions

            public string ProcessModelingSessionId { get; }

            public string InitiatorClientApplicationName { get; }

            public string InitiatorClientWorkstationName { get; }

            public string ProcessModelName { get; }

            public string ProcessModelNameToDisplay { get; }

            public string InstructorUserName { get; }

            public InstructorAccessFlags InstructorAccessFlags { get; }

            public string Mode { get; }

            public long? DbEnity_ProcessModelingSessionId { get; set; }

            public ObservableCollection<EngineSession> EngineSessions { get; } = new();            

            /// <summary>
            ///     See ProcessModelingSessionConstants
            /// </summary>
            public int ProcessModelingSessionStatus { get; set; }

            public string? LaunchEnginesJobId { get; set; }

            public List<ServerContext> ProcessServerContextsCollection { get; } = new();

            /// <summary>
            ///     Time for inactivity time-out check.
            /// </summary>
            public DateTime? ForTimeout_LastDateTimeUtc { get; set; }

            public UInt64 ProcessTimeSeconds { get; private set; }            

            /// <summary>
            ///    Must be called once.
            /// </summary>
            /// <param name="dataAccessProvider"></param>
            public void SubscribeToMainControlEngine(GrpcDataAccessProvider dataAccessProvider)
            {
                _processTimeSecondsSubscription = new ValueSubscription(dataAccessProvider, "SYSTEM.MODEL_TIME",
                        (sender, args) =>
                        {
                            if (StatusCodes.IsGood(args.NewValueStatusTimestamp.StatusCode))
                            {
                                ProcessTimeSeconds = args.NewValueStatusTimestamp.Value.ValueAsUInt64(false);
                            }
                            else
                            {
                                ProcessTimeSeconds = 0;
                            }
                        });
            }

            #endregion         

            #region private fields

            public ValueSubscription? _processTimeSecondsSubscription;

            #endregion
        }
    }
}