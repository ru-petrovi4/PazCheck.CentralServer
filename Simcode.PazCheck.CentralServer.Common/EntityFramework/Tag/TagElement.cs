using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class TagElement : VersionEntity
    {
        [Attr]
        public string ElementName { get; set; } = @"";

        [Attr]
        public string Value { get; set; } = @"";

        [Attr]
        public string Type { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [HasOne]
        public Tag Tag { get; set; } = null!;
    }
}
