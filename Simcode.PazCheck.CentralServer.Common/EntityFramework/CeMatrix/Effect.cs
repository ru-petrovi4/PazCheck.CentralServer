using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Effect : VersionEntityBase
    {
        /// <summary>
        ///     Порядковый номер внутри диаграммы
        /// </summary>
        [Attr]
        public int Num { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition.Identifier[=TagCondition.Value]
        ///     <example>PVHighHigh</example>
        ///     <example>ALARM=ЗАКР</example>
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";

        [Attr]
        public string TagConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string CustomFieldValues { get; set; } = @"";

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
