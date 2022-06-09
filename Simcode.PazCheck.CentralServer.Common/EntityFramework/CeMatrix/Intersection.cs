using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Intersection : VersionEntity
    {
        [HasOne(PublicName="diagram")]
        public CeMatrix CeMatrix { get; set; }

        [HasOne(PublicName="cause")]
        public Cause Cause { get; set; }

        [HasOne(PublicName="effect")]
        public Effect Effect { get; set; }

        /// <summary>
        ///     For import/export
        /// </summary>
        [NotMapped]
        public int CauseKey { get; set; }

        /// <summary>
        ///     For import/export
        /// </summary>
        [NotMapped]
        public int EffectKey { get; set; }
    }
}
