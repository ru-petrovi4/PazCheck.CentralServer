using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Actuator : VersionEntity
    {
        [Attr(PublicName="name")]
        public string Title { get; set; } = @"";

        public string Desc { get; set; } = @"";

        [Attr(PublicName="type")]
        public BaseActuator BaseActuator { get; set; } = null!;

        [HasMany(PublicName = "actuatorparams")]
        public List<ActuatorParam> ActuatorParams { get; set; } = new();

        [HasOne(PublicName = "tag")]
        public Tag Tag { get; set; } = null!;
    }
}
