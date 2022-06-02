using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Actuator : Identifiable<int>
    {
        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";

        [Attr(PublicName="type")]
        public string Type { get; set; } = @"";

        public int ProjectId { get; set; }
        [HasOne(PublicName="project")]
        public Project Project { get; set; }

        [HasMany(PublicName = "actuatorparams")]
        public List<Actuatorparams> Actuatorparams { get; set; } = new List<Actuatorparams>();

        [HasMany(PublicName = "tags")] public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
