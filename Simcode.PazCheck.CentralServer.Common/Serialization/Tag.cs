using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class Tag : ProjectVersionedEntityBase
    {
        public List<TagCondition>? TagConditions { get; set; }

        [JsonIgnore]
        public EntityFramework.Tag? SourceEntity { get; set; }
    }
}
