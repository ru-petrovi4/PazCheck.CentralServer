#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Tag : VersionEntity
    {
        [Attr(PublicName = "name")]
        public string TagName { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string Desc { get; set; } = @"";

        [HasOne(PublicName = "project")]
        public Project Project { get; set; } = null!;

        [HasMany(PublicName = "identities")]
        public List<TagCondition> TagConditions { get; set; } = new();

        [HasOne(PublicName="actuator")]
        public Actuator? Actuator { get; set; }

        [HasMany(PublicName = "tagevents")]
        public List<TagEvent> TagEvents { get; set; } = new();
    }
}
