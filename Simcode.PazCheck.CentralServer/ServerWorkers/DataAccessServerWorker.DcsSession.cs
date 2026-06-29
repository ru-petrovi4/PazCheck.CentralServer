using Ssz.Dcs.CentralServer.Common;
using Microsoft.Extensions.Logging;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions

        public ObservableCollection<EngineSession> Dcs_EngineSessions { get; } = new();

        public void OnDataAccessProviderGetter_Addon_Added(DataAccessProviderGetter_AddonBase addedDataAccessProviderGetter_Addon)
        {
            var egineSession = new EngineSession(Guid.NewGuid().ToString(), addedDataAccessProviderGetter_Addon);
            Dcs_EngineSessions.Add(egineSession);
        }

        public void OnDataAccessProviderGetter_Addons_Removed(DataAccessProviderGetter_AddonBase removedDataAccessProviderGetter_Addon)
        {
            for (int collectionIndex = Dcs_EngineSessions.Count - 1; collectionIndex >= 0; collectionIndex -= 1)
            {
                var egineSession = Dcs_EngineSessions[collectionIndex];
                if (ReferenceEquals(egineSession.DataAccessProviderGetter_Addon, removedDataAccessProviderGetter_Addon))
                {
                    Dcs_EngineSessions.RemoveAt(collectionIndex);
                    break;
                }
            }
        }

        #endregion        
    }
}
