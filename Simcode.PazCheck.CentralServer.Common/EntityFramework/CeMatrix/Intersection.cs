using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Intersection : VersionEntityBase
    {
        [Attr]
        public string TagConditionString_SymbolToDisplay { get; set; } = @"";

        [HasOne]
        public Cause Cause { get; set; } = null!;

        [HasOne]
        public Effect Effect { get; set; } = null!;

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
