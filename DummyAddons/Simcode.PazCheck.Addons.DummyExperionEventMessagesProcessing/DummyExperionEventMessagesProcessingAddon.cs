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

namespace Simcode.PazCheck.Addons.DummyExperionEventMessagesProcessing
{
    [Export(typeof(AddonBase))]
    public class DummyExperionEventMessagesProcessingAddon : EventMessagesProcessingAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"344EE0F7-311B-49D2-8AA3-4EF4FCE7D1BA");

        public static readonly string AddonName = @"DummyExperionEventMessagesProcessing";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DummyExperionEventMessagesProcessingAddon_Desc;

        public override string Version => "1.0";

        public override (string, string)[] OptionsInfo => new (string, string)[0];
    }
}