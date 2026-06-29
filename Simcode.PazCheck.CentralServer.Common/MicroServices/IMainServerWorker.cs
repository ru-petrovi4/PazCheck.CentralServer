using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices
{    
    public interface IMainServerWorker
    {
        IServiceProvider ServiceProvider { get; }

        CsvDb CsvDb { get; }

        IDataAccessProvider DataAccessProvider { get; }

        /// <summary>
        ///     Need synchronization: lock (((System.Collections.ICollection)mainServerWorker.SystemParams).SyncRoot)
        /// </summary>
        CaseInsensitiveOrderedDictionary<Any> SystemParams { get; }        

        void NotifyEventMessages(string targetWorkstationName, EventMessagesCollection eventMessagesCollection);
    }

    public class AddonsSource
    {
        public string SourceId { get; set; } = @"";

        public string SourceIdToDisplay { get; set; } = @"";

        public List<Ssz.Utils.Addons.AddonStatus> AddonStatuses { get; } = new();

        public Ssz.Utils.Addons.AddonStatus? ResourceMonitoringAddonStatus { get; set; }
    }
}
