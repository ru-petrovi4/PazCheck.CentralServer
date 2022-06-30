using Microsoft.Extensions.DependencyInjection;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Addons.DummyExperionEventsImporter
{
    [Export(typeof(AddonBase))]
    public class DummyExperionEventsImporterAddon : EventsImporterAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"344EE0F7-311B-49D2-8AA3-4EF4FCE7D1BA");

        public static readonly string AddonName = @"DummyExperionEventsImporter";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyExperionEventsImporterAddon_Desc;

        public override string Version => "1.0";

        public override (string, string)[] OptionsInfo => new (string, string)[0]; // { ("%(ServerAddress)", Properties.Resources.ServerAddress_Option) };

        public override async Task ImportLogsAsync(Stream stream, string logName, PazCheckDbContext context, int unitId, CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            var logLoader = ActivatorUtilities.CreateInstance<DummyExperionLogLoader>(ServiceProvider);
            await logLoader.ImportFileLogAsync(CsvDb, stream, logName, context, unitId, cancellationToken, jobProgress);
        }
    }
}