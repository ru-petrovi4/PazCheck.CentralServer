﻿using Microsoft.Extensions.Configuration;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
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
        public abstract Task LoadFixturesAsync(IConfiguration configuration, IServiceProvider serviceProvider, PazCheckDbContext context, Project project);

        public abstract string? GetCeMatrixString(PazCheckDbContext context, CeMatrix diagram);

        public abstract string? GetCeMatrixRuntimeString(PazCheckDbContext context, CeMatrixResult diagResult);

        public abstract Task CalculateResultsAsync(PazCheckDbContext dbContext, int logId, DateTime startTimeUtc, DateTime endTimeUtc,
            CancellationToken cancellationToken, IJobProgress jobProgress);

        public abstract string? GetTimelineJson(Result result);
    }
}
