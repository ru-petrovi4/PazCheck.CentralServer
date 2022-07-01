using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class EventMessagesProcessingAddonBase : AddonBase
    {
        public virtual bool CanSaveToDbEventMessageFrom(string sourceSystemId)
        {
            return false;
        }

        public virtual Task SaveToDbAsync(PazCheckDbContext dbContext, EventMessage eventMessage, CancellationToken cancellationToken, IJobProgress? jobProgress)
        {
            return Task.CompletedTask;
        }

        public virtual bool CanAddToEventSourceModelEventMessageFrom(string sourceSystemId)
        {
            return false;
        }

        public virtual Task<IEnumerable<AlarmInfoViewModelBase>?> AddToEventSourceModelAsync(EventSourceModel eventSourceModel,
            EventMessage eventMessage)
        {
            return Task.FromResult<IEnumerable<AlarmInfoViewModelBase>?>(null);
        }
    }
}
