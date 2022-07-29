using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class EventMessagesProcessingAddonBase : AddonBase
    {
        public virtual Task ImportEventsJournalFileAsync(Stream stream, string fileName, PazCheckDbContext dbContext, Unit unit, CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            return Task.CompletedTask;
        }

        public virtual bool CanProcessEventMessageFrom(string sourceSystemName)
        {
            return false;
        }
        
        public virtual Task<UnitEventsInterval?> ProcessEventMessagesAsync(string sourceSystemName, List<EventMessage> eventMessages, string unitEventsIntervalTitle, CancellationToken cancellationToken, IJobProgress? jobProgress)
        {
            return Task.FromResult<UnitEventsInterval?>(null);
        }

        //public virtual bool CanAddToEventSourceModelEventMessageFrom(string sourceSystemId)
        //{
        //    return false;
        //}

        //public virtual Task<IEnumerable<AlarmInfoViewModelBase>?> AddToEventSourceModelAsync(EventSourceModel eventSourceModel,
        //    EventMessage eventMessage)
        //{
        //    return Task.FromResult<IEnumerable<AlarmInfoViewModelBase>?>(null);
        //}
    }
}
