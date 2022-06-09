using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Log : VersionEntity
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @"";
        public int UnitId { get; set; }
        [HasOne(PublicName = "unit")]
        public Unit Unit { get; set; }

        [Attr(PublicName = "start")]
        public DateTime Start { get; set; }
        [Attr(PublicName = "end")]
        public DateTime End { get; set; }

        [HasMany(PublicName = "logevents")]
        public List<UnitEvent> Logevents { get; set; } = new();
    }
}
