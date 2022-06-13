using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Actuator : VersionEntity
    {
        [Attr]
        public string Title { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        [HasMany]
        public List<ActuatorParam> ActuatorParams { get; set; } = new();

        [HasOne]
        public Tag Tag { get; set; } = null!;
    }
}
