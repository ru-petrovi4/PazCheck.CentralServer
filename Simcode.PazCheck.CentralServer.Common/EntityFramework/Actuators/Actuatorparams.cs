using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Actuatorparams : Identifiable<int>
    {
        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";
        [Attr(PublicName="value")]
        public string Value { get; set; } = @"";
        public int ActuatorId { get; set; }
        [HasOne(PublicName="actuator")]
        public Actuator Actuator { get; set; }
    }
}
