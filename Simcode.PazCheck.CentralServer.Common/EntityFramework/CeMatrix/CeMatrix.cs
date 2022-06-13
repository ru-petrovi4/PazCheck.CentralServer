using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class CeMatrix : VersionEntity
    {
        /// <summary>
        ///     Guid.Version - unique identifier of matrix state
        /// </summary>
        [Attr]
        public string Guid { get; set; } = @"";

        /// <summary>
        ///     Guid.Version - unique identifier of matrix state
        /// </summary>
        [Attr]
        public int Version { get; set; }

        [Attr]
        public string Title { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [Attr]
        public string Comment { get; set; } = @"";

        [Attr]
        public string Status { get; set; } = @"";

        [Attr]
        public string Source { get; set; } = @"";

        [HasOne]
        public Project Project { get; set; } = null!;
        
        [HasMany]
        [InverseProperty(nameof(Cause.CeMatrix))] // Bacause Intersections exists.
        public List<Cause> Causes { get; set; } = new();
        
        [HasMany]
        [InverseProperty(nameof(Effect.CeMatrix))] // Bacause Intersections exists.
        public List<Effect> Effects { get; set; } = new();

        [HasMany]
        public List<Intersection> Intersections { get; set; } = new();        

        [Attr]
        public string CauseCustomFieldNames { get; set; } = @"";

        [Attr]
        public string EffectCustomFieldNames { get; set; } = @"";
    }
}
