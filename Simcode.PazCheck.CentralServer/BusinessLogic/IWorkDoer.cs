using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public interface IWorkDoer
    {
        Task InitializeAsync(CancellationToken cancellationToken);

        Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken);

        Task CloseAsync();
    }
}
