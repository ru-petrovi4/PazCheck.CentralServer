using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Пересечение в матрице ПСС
    /// </summary>
    [Resource]
    public class Intersection : VersionEntityBase
    {
        /// <summary>
        ///     TagConditionString_SymbolToDisplay или Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Причина пересечения
        /// </summary>
        [HasOne]
        public Cause Cause { get; set; } = null!;

        /// <summary>
        ///     Порядковый номер подпричины внутри родительской причины, если значение не относится ко всей причине
        /// </summary>
        [Attr]
        public int? SubCauseNum { get; set; }

        /// <summary>
        ///     Следствие пересечения
        /// </summary>
        [HasOne]
        public Effect Effect { get; set; } = null!;

        /// <summary>
        ///     Родительская матрица
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
