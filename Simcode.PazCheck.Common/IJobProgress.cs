using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Common
{
    public interface IJobProgress
    {        
        Task ReportAsync(double progressPercent, string? progressLabel, string? progressDetail, bool failed);        
    }
}