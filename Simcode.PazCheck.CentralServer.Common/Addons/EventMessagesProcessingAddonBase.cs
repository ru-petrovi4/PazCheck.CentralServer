using Microsoft.Extensions.Logging;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class EventMessagesProcessingAddonBase : AddonBase
    {
        public abstract Task<IEnumerable<AlarmInfoViewModelBase>?> ProcessEventMessage(EventSourceModel eventSourceModel,
            EventMessage eventMessage, ILogger? logger);
    }
}
