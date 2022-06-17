using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Состояние тэга.
    /// </summary>
    [Resource]
    public class TagCondition : VersionEntity
    {
        /// <summary>
        ///     <para>Текстовое поле RW: Идентификатор</para>
        /// </summary>
        [Attr]
        public string ElementName { get; set; } = @"";

        [Attr]
        public string MathOperator { get; set; } = @"";

        /// <summary>
        ///     Value that the tag reched in this record
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Event type that may be for e.g. Alarm or Acknowledge
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        [Attr]
        public string SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [HasOne]
        public Tag Tag { get; set; } = null!;         
    }
}
