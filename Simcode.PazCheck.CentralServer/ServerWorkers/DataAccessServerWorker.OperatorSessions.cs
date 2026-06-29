using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ssz.Utils;
using Ssz.Dcs.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Properties;
using Ssz.Utils.DataAccess;
using Ssz.DataAccessGrpc.ServerBase;
using System.Threading;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer
{    
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions 

        public void ConcludeOperatorSession(string operatorSessionId)
        {
            OperatorSession? operatorSession = OperatorSessionsCollection.TryGetValue(operatorSessionId);
            if (operatorSession is null) return;
            OperatorSessionsCollection.Remove(operatorSessionId);

            ServerContextsAbort(operatorSession.OperatorSession_ProcessServerContextsCollection.ToArray());

            _utilityItemsDoWorkNeeded = true;
        }

        public void SetOperatorSessionProps(string operatorSessionId, string operatorUserName)
        {
            OperatorSession? operatorSession = OperatorSessionsCollection.TryGetValue(operatorSessionId);
            if (operatorSession is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid operatorSession: " + operatorSessionId));

            operatorSession.OperatorUserName = operatorUserName;                                   

            _utilityItemsDoWorkNeeded = true;
        }

        #endregion

        #region internal functions

        /// <summary>
        ///     [OperatorSessionId, OperatorSession]
        /// </summary>
        internal CaseInsensitiveOrderedDictionary<OperatorSession> OperatorSessionsCollection { get; } = new();

        #endregion

        #region private functions

        /// <summary>
        ///     processServerContext with ContextParams['OperatorSessionId'] != String.Empty
        /// </summary>
        /// <param name="processServerContext"></param>
        /// <param name="added"></param>
        /// <param name="systemNameToConnect"></param>
        /// <exception cref="RpcException"></exception>
        private void OnProcessServerContext_AddedOrRemoved(ServerContext processServerContext, bool added, string systemNameToConnect)
        {
            if (!String.Equals(systemNameToConnect, DataAccessConstants.Dcs_SystemName, StringComparison.InvariantCultureIgnoreCase))
            {            
                ProcessModelingSession? processModelingSession = GetProcessModelingSessionOrNull(systemNameToConnect);
                if (processModelingSession is not null)
                {
                    if (added)
                    {
                        if (processServerContext.ClientApplicationName == DataAccessConstants.Instructor_ClientApplicationName &&
                                processModelingSession.ProcessServerContextsCollection.Count(sc =>
                                    String.Equals(sc.ClientApplicationName, DataAccessConstants.Instructor_ClientApplicationName, StringComparison.InvariantCultureIgnoreCase)
                                    ) == 0)
                        {
                            processModelingSession.ForTimeout_LastDateTimeUtc = null; // Reset timeout, instruxtor connected.
                            processModelingSession.ProcessModelingSessionStatus = ProcessModelingSessionConstants.InstructorConnected;
                        }
                        processModelingSession.ProcessServerContextsCollection.Add(processServerContext);
                    }
                    else
                    {
                        processModelingSession.ProcessServerContextsCollection.Remove(processServerContext);
                        if (processServerContext.ClientApplicationName == DataAccessConstants.Instructor_ClientApplicationName &&
                            processModelingSession.ProcessServerContextsCollection.Count(sc =>
                                String.Equals(sc.ClientApplicationName, DataAccessConstants.Instructor_ClientApplicationName, StringComparison.InvariantCultureIgnoreCase)
                                ) == 0)
                        {
                            processModelingSession.ForTimeout_LastDateTimeUtc = DateTime.UtcNow; // Set timeout, last instruxtor disconnected.
                            processModelingSession.ProcessModelingSessionStatus = ProcessModelingSessionConstants.InstructorDisconnected;
                        }
                    }
                }                    
            }

            string? operatorSessionId = processServerContext.ContextParams.GetValueOrDefault(@"OperatorSessionId");
            if (!String.IsNullOrEmpty(operatorSessionId)) // Context with operatorSessionId.
            {
                OperatorSession? operatorSession = OperatorSessionsCollection.TryGetValue(operatorSessionId);
                if (operatorSession is not null)
                {
                    if (added)
                    {
                        operatorSession.OperatorSession_ProcessServerContextsCollection.Add(processServerContext);
                        if (!operatorSession.OperatorInterfaceConnected)
                        {
                            operatorSession.OperatorInterfaceConnected = true;
                            operatorSession.ForTimeout_LastDateTimeUtc = null;
                            SetOperatorSessionStatus(operatorSession, OperatorSessionConstants.LaunchedOperator);
                            _utilityItemsDoWorkNeeded = true;
                        }
                    }
                    else
                    {
                        operatorSession.OperatorSession_ProcessServerContextsCollection.Remove(processServerContext);
                        if (operatorSession.OperatorSession_ProcessServerContextsCollection.Count == 0)
                        {
                            if (!processServerContext.IsConcludeCalledByClient)
                            {
                                operatorSession.ForTimeout_LastDateTimeUtc = DateTime.UtcNow;
                            }
                            else
                            {
                                OperatorSessionsCollection.Remove(operatorSession.OperatorSessionId);
                                SetOperatorSessionStatus(operatorSession, OperatorSessionConstants.ShutdownedOperator);
                                _utilityItemsDoWorkNeeded = true;
                            }
                        }
                    }
                }                
            }
        }

        private void SetOperatorSessionStatus(OperatorSession operatorSession, int operatorSessionStatus)
        {
            operatorSession.OperatorSessionStatus = operatorSessionStatus;
            switch (operatorSessionStatus)
            {
                //case OperatorSessionPazCheckCentralServerConstants.LaunchedOperator:
                //    if (operatorSession.ProcessModelingSession is not null &&
                //        operatorSession.ProcessModelingSession.DbEnity_ProcessModelingSessionId is not null)
                //    {
                //        try
                //        {
                //            using (var dbContext = _dbContextFactory.CreateDbContext())
                //            {
                //                if (dbContext.IsConfigured) 
                //                {
                //                    var operatorUser = dbContext.Users.FirstOrDefault(u => u.UserName == operatorSession.OperatorUserName);
                //                    if (operatorUser is not null)
                //                    {
                //                        var dbEnity_OperatorSession = new Common.EntityFramework.OperatorSession
                //                        {
                //                            OperatorUser = operatorUser,
                //                            StartDateTimeUtc = DateTime.UtcNow,
                //                            ProcessModelingSessionId = operatorSession.ProcessModelingSession.DbEnity_ProcessModelingSessionId.Value,
                //                        };
                //                        dbContext.OperatorSessions.Add(dbEnity_OperatorSession);
                //                        dbContext.SaveChanges();
                //                        operatorSession.DbEnity_OperatorSessionId = dbEnity_OperatorSession.Id;
                //                    }
                //                }                                
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Logger.LogError(ex, @"LaunchedOperator dbContext error.");
                //        }
                //    }
                //    break;
                //case OperatorSessionPazCheckCentralServerConstants.ShutdownedOperator:
                //    try
                //    {
                //        if (operatorSession.DbEnity_OperatorSessionId is not null)
                //        {
                //            using (var dbContext = _dbContextFactory.CreateDbContext())
                //            {
                //                if (dbContext.IsConfigured)
                //                {
                //                    var dbEnity_OperatorSession = dbContext.OperatorSessions.FirstOrDefault(pms => pms.Id == operatorSession.DbEnity_OperatorSessionId.Value);
                //                    if (dbEnity_OperatorSession is not null)
                //                    {
                //                        dbEnity_OperatorSession.FinishDateTimeUtc = DateTime.UtcNow;
                //                        dbContext.SaveChanges();
                //                    };
                //                }                                
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Logger.LogError(ex, @"ShutdownedOperator dbContext error.");
                //    }
                //    break;
            }
            
        }

        #endregion

        internal class OperatorSession
        {
            #region construction and destruction

            public OperatorSession(string operatorSessionId, string operatorWorkstationName)
            {
                OperatorSessionId = operatorSessionId;
                OperatorWorkstationName = operatorWorkstationName;                
            }

            #endregion

            #region public functions

            public string OperatorSessionId { get; }

            public string OperatorWorkstationName { get; }

            public string?[] ProcessModelNames { get; set; } = null!;

            public List<ServerContext> UtilityServerContexts { get; } = new();

            public ProcessModelingSession? ProcessModelingSession { get; set; }

            /// <summary>
            ///     'UserDomainName\UserName'
            /// </summary>
            public string WindowsUserName { get; set; } = "";

            /// <summary>
            ///     TODO
            /// </summary>
            public string WindowsUserNameToDisplay { get; set; } = "";

            public string OperatorUserName { get; set; } = "";

            public string OperatorRoleId { get; set; } = "";

            public string OperatorRoleName { get; set; } = "";

            public string DsProject_PathRelativeToDataDirectory { get; set; } = "";

            public string Interface_NameToDisplay { get; set; } = "";

            public string OperatorPlay_AdditionalCommandLine { get; set; } = "";

            public int OperatorSessionStatus { get; set; }

            public bool OperatorInterfaceConnected { get; set; }

            public string? LaunchOperatorJobId { get; set; }

            /// <summary>
            ///     ProcessServerContexts with ContextParams['OperatorSessionId'] != String.Empty
            /// </summary>
            public List<ServerContext> OperatorSession_ProcessServerContextsCollection { get; } = new List<ServerContext>();

            /// <summary>
            ///     Time for inactivity time-out check.
            /// </summary>
            public DateTime? ForTimeout_LastDateTimeUtc { get; set; }

            public long? DbEnity_OperatorSessionId { get; set; }

            #endregion
        }
    }
}