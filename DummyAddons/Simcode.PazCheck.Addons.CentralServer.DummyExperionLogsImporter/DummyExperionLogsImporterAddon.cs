using Microsoft.Extensions.DependencyInjection;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Addons.CentralServer.DummyExperionLogsImporter
{
    [Export(typeof(AddonBase))]
    public class DummyExperionLogsImporterAddon : LogsImporterAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"344EE0F7-311B-49D2-8AA3-4EF4FCE7D1BA");

        public static readonly string AddonName = @"ExperionLogsImporter";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyExperionLogsImporterAddon_Desc;

        public override string Version => "1.0";

        public override string SimcodePazCheckServerVersion
        {
            get { return SimcodePazCheckServerVersionConst; }
        }

        public override (string, string)[] OptionsInfo => new (string, string)[0]; // { ("%(ServerAddress)", Properties.Resources.ServerAddress_Option) };

        public override async Task ImportLogsAsync(Stream stream, string logName, PazCheckDbContext context, int unitId, CancellationToken cancellationToken, IJobProgress jobProgress)
        {
            var logLoader = ActivatorUtilities.CreateInstance<DummyExperionLogLoader>(ServiceProvider);
            await logLoader.ImportFileLogAsync(CsvDb, stream, logName, context, unitId, cancellationToken, jobProgress);
        }
    }
}