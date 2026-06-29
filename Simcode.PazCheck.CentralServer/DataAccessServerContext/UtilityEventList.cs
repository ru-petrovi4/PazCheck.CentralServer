using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public class UtilityEventList : EventListBase
    {
        #region construction and destruction
        
        public UtilityEventList(DataAccessServerWorkerBase dataAccessServerWorker, ServerContext serverContext, uint listClientAlias, CaseInsensitiveOrderedDictionary<string?> listParams)
            : base(dataAccessServerWorker, serverContext, listClientAlias, listParams)
        {
            ((DataAccessServerWorker)ServerContext.ServerWorker).UtilityEventMessageNotification += OnUtilityEventMessageNotification;
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                ((DataAccessServerWorker)ServerContext.ServerWorker).UtilityEventMessageNotification -= OnUtilityEventMessageNotification;
            }

            base.Dispose(disposing);
        }

        #endregion


        #region private functions

        private void OnUtilityEventMessageNotification(string? targetWorkstationName, Ssz.Utils.DataAccess.EventMessagesCollection eventMessagesCollection)
        {
            if (Disposed) return;

            bool send = false;
            if (String.IsNullOrEmpty(targetWorkstationName))
            {
                send = true;
            }
            else if (String.Equals(targetWorkstationName, ServerContext.ClientWorkstationName, StringComparison.InvariantCultureIgnoreCase))
            {
                send = true;
            }
            else if (String.Equals(targetWorkstationName, @"localhost", StringComparison.InvariantCultureIgnoreCase) &&
                        String.Equals(ServerContext.ClientWorkstationName, Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))                
            {
                send = true;
            }

            if (send)
                EventMessagesCollections.Add(eventMessagesCollection);
        }

        #endregion
    }
}
