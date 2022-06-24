using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public interface IJobProgress
    {        
        Task ReportAsync(double progressPercent, string? progressLabel, string? progressDetail, bool failed);        
    }

    public class DummyJobProgress : IJobProgress
    {
        public static readonly DummyJobProgress Dafault = new();

        public Task ReportAsync(double progressPercent, string? progressLabel, string? progressDetail, bool failed)
        {
            return Task.CompletedTask;
        }
    }
}