using System;
using System.Threading;
using System.Threading.Tasks;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public interface IUnitHelper
    {
        public string ExportUnit(Project project, CancellationToken cancellationToken);
        public Task ImportUnitAsync(string filePath, CancellationToken cancellationToken, IJobProgress jobProgress);
    }
}
