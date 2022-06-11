using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class CeMatrix : VersionEntity
    {
        /// <summary>
        ///     Guid.Version - unique identifier of matrix state
        /// </summary>
        public string Guid { get; set; } = @"";

        /// <summary>
        ///     Guid.Version - unique identifier of matrix state
        /// </summary>
        public int Version { get; set; }

        [Attr(PublicName="name")]
        public string Title { get; set; } = @"";

        public string Desc { get; set; } = @"";

        public string Comment { get; set; } = @"";        

        public string Status { get; set; } = @"";

        public string Source { get; set; } = @"";

        [HasOne(PublicName = "project")]
        public Project Project { get; set; } = null!;
        
        [HasMany(PublicName = "causes")]
        public List<Cause> Causes { get; set; } = new();
        
        [HasMany(PublicName = "effects")]
        public List<Effect> Effects { get; set; } = new();

        [HasMany(PublicName="intersections")]
        public List<Intersection> Intersections { get; set; } = new();        

        [Attr(PublicName = "causecustomfieldnames")]
        public string CauseCustomFieldNames { get; set; } = @"";

        [Attr(PublicName = "effectcustomfieldnames")]
        public string EffectCustomFieldNames { get; set; } = @"";
    }
}
