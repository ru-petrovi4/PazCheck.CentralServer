﻿using Microsoft.Extensions.Configuration;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Addons.CentralServer.DummyCeMatrixRuntime
{
    [Export(typeof(AddonBase))]
    public class DummyCeMatrixRuntimeAddon : CeMatrixRuntimeAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"746D6449-241E-438F-A294-399655516673");

        public static readonly string AddonName = @"DummyCeMatrixRuntime";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyCeMatrixRuntimeAddon_Desc;

        public override string Version => "1.0";

        public override string SimcodePazCheckServerVersion
        {
            get { return SimcodePazCheckServerVersionConst; }
        }

        public override (string, string)[] OptionsInfo => new (string, string)[0];

        public override bool IsDummy => true;

        public override string? GetCeMatrixString(PazCheckDbContext context, CeMatrix diagram)
        {
            return null;
        }

        public override string? GetCeMatrixRuntimeString(PazCheckDbContext context, CeMatrixResult diagResult)
        {
            return null;
        }

        public override void LoadFixtures(IConfiguration configuration, IServiceProvider serviceProvider, PazCheckDbContext context, Project project)
        {
        }

        public override Task CalculateResultsAsync(PazCheckDbContext dbContext, int logId, DateTime startTimeUtc, DateTime endTimeUtc,
            CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            return Task.CompletedTask;
        }
    }
}