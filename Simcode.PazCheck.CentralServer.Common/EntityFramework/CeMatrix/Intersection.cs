using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Intersection : VersionEntityBase
    {
        /// <summary>
        ///     TagConditionString_SymbolToDisplay или Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        [HasOne]
        public Cause Cause { get; set; } = null!;

        [Attr]
        public int? SubCauseNum { get; set; }

        [HasOne]
        public Effect Effect { get; set; } = null!;

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
