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
        public virtual Task ImportEventsJournalFileAsync(Stream stream, string fileName, PazCheckDbContext dbContext, int unitId, CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            return Task.CompletedTask;
        }

        public virtual bool CanSaveToDbEventMessageFrom(string sourceSystemId)
        {
            return false;
        }

        /// <summary>
        ///     You shouldn't dbContext.SaveChanges()
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <param name="dbContext"></param>
        /// <param name="unit"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="jobProgress"></param>
        /// <returns></returns>
        public virtual Task SaveToDbAsync(EventMessage eventMessage, PazCheckDbContext dbContext, Unit unit, CancellationToken cancellationToken, IJobProgress? jobProgress)
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
