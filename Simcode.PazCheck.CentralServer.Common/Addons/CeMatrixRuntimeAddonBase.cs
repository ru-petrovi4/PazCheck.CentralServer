using Microsoft.Extensions.Configuration;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class CeMatrixRuntimeAddonBase : AddonBase
    {
        public abstract void LoadFixtures(IConfiguration configuration, IServiceProvider serviceProvider, PazCheckDbContext context, Project project);

        public abstract string? GetCeMatrixString(PazCheckDbContext context, CeMatrix diagram);

        public abstract string? GetCeMatrixRuntimeString(PazCheckDbContext context, CeMatrixResult diagResult);

        public abstract Task CalculateResultsAsync(PazCheckDbContext dbContext, int logId, DateTime startTimeUtc, DateTime endTimeUtc,
            CancellationToken cancellationToken, IJobProgress jobProgress);
    }
}
