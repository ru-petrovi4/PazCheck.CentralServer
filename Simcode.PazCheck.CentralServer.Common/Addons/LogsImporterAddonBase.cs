using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class LogsImporterAddonBase : AddonBase
    {
        public abstract Task ImportLogsAsync(Stream stream, string logName, PazCheckDbContext dbContext, int unitId, CancellationToken cancellationToken, IJobProgress jobProgress);
    }
}
