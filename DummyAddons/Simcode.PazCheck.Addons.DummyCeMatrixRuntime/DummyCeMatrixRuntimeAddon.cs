using Microsoft.Extensions.Configuration;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Addons.DummyCeMatrixRuntime
{
    [Export(typeof(AddonBase))]
    public partial class DummyCeMatrixRuntimeAddon : CeMatrixRuntimeAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"746D6449-241E-438F-A294-399655516673");

        public static readonly string AddonName = @"DummyCeMatrixRuntime";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyCeMatrixRuntimeAddon_Desc;

        public override string Version => "1.0";

        public override (string, string)[] OptionsInfo => new (string, string)[0];

        public override bool IsDummy => true;

        public override string? GetCeMatrixString(PazCheckDbContext context, CeMatrix diagram)
        {
            return "Example CeMatrix String";
        }

        public override string? GetCeMatrixRuntimeString(PazCheckDbContext context, CeMatrixResult diagResult)
        {
            return "Example CeMatrixRuntime String";
        }

        public override Task LoadFixturesAsync(IConfiguration configuration, IServiceProvider serviceProvider, PazCheckDbContext dbContext, Project project)
        {
            //foreach (int ceMatrixIndex in Enumerable.Range(0, 2))
            //{
            //    CeMatrixHelper.SaveCeMatrixToDb(dbContext, project, ceMatrixIndex, WrapperUserFriendlyLogger);
            //}
            return Task.CompletedTask;
        }

        public override Task CalculateResultsAsync(PazCheckDbContext dbContext, int logId, DateTime startTimeUtc, DateTime endTimeUtc,
            UInt32? projectVersionNum,
            CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            return Task.CompletedTask;
        }
    }
}