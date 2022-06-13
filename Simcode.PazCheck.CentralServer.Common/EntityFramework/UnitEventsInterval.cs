using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class UnitEventsInterval : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = @"";        

        [Attr]
        public DateTime Start { get; set; }

        [Attr]
        public DateTime End { get; set; }

        [HasMany]
        public List<UnitEvent> UnitEvents { get; set; } = new();

        [HasOne]
        public Unit Unit { get; set; } = null!;
    }
}