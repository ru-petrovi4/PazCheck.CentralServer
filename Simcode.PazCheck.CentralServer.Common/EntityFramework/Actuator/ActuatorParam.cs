using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class ActuatorParam : VersionEntity
    {
        [Attr]
        public string ParamName { get; set; } = @"";

        [Attr]
        public string Value { get; set; } = @"";

        [Attr]
        public string Type { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";        

        [HasOne]
        public Actuator Actuator { get; set; } = null!;
    }
}
