using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Intersection : Identifiable<int>
    {
        [HasOne(PublicName="diagram")]
        public Diagram Diagram { get; set; }
        [HasOne(PublicName="cause")]
        public Cause Cause { get; set; }
        [HasOne(PublicName="effect")]
        public Effect Effect { get; set; }
        //For import/export
        [NotMapped]
        public int CauseKey { get; set; }
        [NotMapped]
        public int EffectKey { get; set; }
    }
}
