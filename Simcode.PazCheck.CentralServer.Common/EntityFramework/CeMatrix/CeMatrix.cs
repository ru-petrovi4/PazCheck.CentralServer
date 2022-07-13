using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class CeMatrix : VersionEntityBase
    {
        /// <summary>
        ///     Поле не версионируется.
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>
        ///     Поле не версионируется.
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";        

        [Attr]
        public string _LockedByUser { get; set; } = @"";

        [HasOne]
        public Project Project { get; set; } = null!;

        [HasMany]
        public List<CeMatrixParam> CeMatrixParams { get; set; } = new();

        [HasMany]
        [InverseProperty(nameof(Cause.CeMatrix))] // Bacause Intersections exists.
        public List<Cause> Causes { get; set; } = new();
        
        [HasMany]
        [InverseProperty(nameof(Effect.CeMatrix))] // Bacause Intersections exists.
        public List<Effect> Effects { get; set; } = new();

        [HasMany]
        public List<Intersection> Intersections { get; set; } = new();        

        public override ILastChangeEntity? GetParentForLastChange() => Project;
    }
}
