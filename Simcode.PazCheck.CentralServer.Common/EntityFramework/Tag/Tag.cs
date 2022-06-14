#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    [Index(nameof(IsActive), nameof(TagName))]
    public class Tag : VersionEntity
    {
        [Attr]
        public string TagName { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [HasOne]
        public Project Project { get; set; } = null!;

        [HasMany]
        public List<TagCondition> TagConditions { get; set; } = new();

        [HasMany]
        public List<TagElement> TagElements { get; set; } = new();

        [HasOne]
        public Actuator? Actuator { get; set; }

        [HasMany]
        public List<TagEvent> TagEvents { get; set; } = new();
    }
}
