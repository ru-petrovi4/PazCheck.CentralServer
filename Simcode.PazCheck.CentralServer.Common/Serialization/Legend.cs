using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class Legend : ProjectVersionedEntityBase
    {
        [JsonIgnore]
        public EntityFramework.Legend? SourceEntity { get; set; }
    }
}
