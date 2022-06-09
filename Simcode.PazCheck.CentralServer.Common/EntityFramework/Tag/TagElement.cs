using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class TagElement : VersionEntity
    {
        [Attr(PublicName="name")]
        public string ElementName { get; set; } = @"";

        [Attr(PublicName="value")]
        public string Value { get; set; } = @"";

        public string Type { get; set; } = @"";

        public string Desc { get; set; } = @"";
       
        public Tag Tag { get; set; } = null!;
    }
}
