using Ssz.Dcs.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Properties;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions      

        public event Action<string?, Ssz.Utils.DataAccess.EventMessagesCollection>? UtilityEventMessageNotification;

        public event Action<ServerContext, Ssz.Utils.DataAccess.EventMessage>? ProcessEventMessageNotification;

        public void NotifyEventMessages(string targetWorkstationName, EventMessagesCollection eventMessagesCollection)
        {
            ThreadSafeDispatcher.BeginInvoke(ct =>
            {
                Action<string?, Ssz.Utils.DataAccess.EventMessagesCollection>? utilityEventMessageNotification = UtilityEventMessageNotification;
                if (utilityEventMessageNotification is null) 
                    return;                

                utilityEventMessageNotification(targetWorkstationName, eventMessagesCollection);
            }
            );
        }

        #endregion

        #region private functions

        //private void Generate_PrepareAndRunInstructorExe_SystemEvent(            
        //    string targetWorkstationName,
        //    ProcessModelingSession processModelingSession)
        //{
        //    Action<string?, Ssz.Utils.DataAccess.EventMessage>? utilityEventMessageNotification = UtilityEventMessageNotification;
        //    if (utilityEventMessageNotification is null) return;            

        //    var eventMessage = new Ssz.Utils.DataAccess.EventMessage(new Ssz.Utils.DataAccess.EventId
        //    {
        //        Conditions = new List<Ssz.Utils.DataAccess.TypeId> { EventMessagePazCheckCentralServerConstants.PrepareAndRunInstructorExe_TypeId }
        //    });

        //    eventMessage.EventType = EventType.SystemEvent;
        //    eventMessage.OccurrenceTimeUtc = DateTime.UtcNow;
        //    eventMessage.TextMessage = CsvHelper.FormatForCsv(",", new object?[] {
        //        processModelingSession.LaunchEnginesJobId,
        //        processModelingSession.ProcessModelingSessionId,
        //        processModelingSession.ProcessModelName,
        //        processModelingSession.InstructorUserName });            

        //    utilityEventMessageNotification(targetWorkstationName, eventMessage);
        //}

        #endregion
    }
}
