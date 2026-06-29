using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class CeMatrix : ProjectVersionedEntityBase
    {
        [JsonIgnore]
        public EntityFramework.CeMatrix? SourceEntity { get; set; }
    }
}
