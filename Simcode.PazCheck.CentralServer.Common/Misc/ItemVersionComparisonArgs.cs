using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class ItemVersionComparisonArgs
    {
        public int ProjectId { get; init; }

        public UInt32 MinProjectVersionNum { get; init; }

        public UInt32? MaxProjectVersionNum { get; init; }

        public string User { get; init; } = @"";

        public Dictionary<UInt32, ProjectVersion> ProjectVersions { get; init; } = null!;
    }
}
