using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Office : VersionEntity
    {
        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";
        [HasMany(PublicName="users")]
        public List<Simuser> Users { get; set; }
    }
}
