using Microsoft.Extensions.DependencyInjection;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Simcode.PazCheck.Addons.DummyExperionTagsImporter
{
    [Export(typeof(AddonBase))]
    public class DummyExperionTagsImporterAddon : TagsImporterAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"3DC1FC70-3941-4A30-BD36-71D6EA355743");

        public static readonly string AddonName = @"DummyExperionTagsImporter";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyExperionTagsImporterAddon_Desc;

        public override string Version => "1.0";        

        public override (string, string)[] OptionsInfo => new (string, string)[0]; // { ("%(ServerAddress)", Properties.Resources.ServerAddress_Option) };

        public override void ImportTags(Stream stream, PazCheckDbContext context, Project project, string user)
        {
            var ti = ActivatorUtilities.CreateInstance<QdbTagsLoader>(ServiceProvider, context);// new (context, NullLogger<QdbTagsLoader>.Instance);
            using (var streamReader = new StreamReader(stream))
            {
                ti.LoadTags(streamReader, project, user);
            }
        }
    }
}