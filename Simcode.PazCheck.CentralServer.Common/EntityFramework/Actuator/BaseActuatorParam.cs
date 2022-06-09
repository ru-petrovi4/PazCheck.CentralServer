using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class BaseActuatorParam : VersionEntity
    {
        [Attr(PublicName="name")]
        public string ParamName { get; set; } = @"";

        [Attr(PublicName="value")]
        public string Value { get; set; } = @"";

        public string Type { get; set; } = @"";

        public string Desc { get; set; } = @"";

        public int BaseActuatorId { get; set; }

        [HasOne(PublicName = "actuator")]
        public BaseActuator BaseActuator { get; set; } = null!;
    }
}
