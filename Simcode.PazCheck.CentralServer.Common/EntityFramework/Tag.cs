#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Tag : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string Descr { get; set; } = @"";
        [Attr(PublicName = "ext")]
        public string Ext { get; set; } = @"";
        public int ProjectId { get; set; }
        [HasOne(PublicName="project")]
        public Project Project { get; set; }

        [HasMany(PublicName = "identities")]
        public List<Identity> Identities { get; set; } = new List<Identity>();
        [HasOne(PublicName="actuator")]
        public Actuator? Actuator { get; set; }

        [HasMany(PublicName = "tagevents")]
        public List<Tagevents> TagEvents { get; set; } = new List<Tagevents>();
        [NotMapped] public int Key { get; set; } = 0;
    }
}
