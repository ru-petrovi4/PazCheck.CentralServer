using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class VersionMessage : Identifiable<int>
    {
        [Attr]
        public string User { get; set; } = @"";

        [Attr]
        public string Message { get; set; } = @"";

        [HasMany]
        public List<DbFile> DbFiles { get; set; } = new();
    }
}
