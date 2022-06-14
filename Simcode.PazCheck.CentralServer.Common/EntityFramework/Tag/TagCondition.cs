using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /**
     * Identity are used to be compared with LogRecord to find out is the tag
     * was triggered. From configuration files we collect possible Identities and
     * when configure matrix we select that identity, that will be used.
     */
    [Resource]
    public class TagCondition : VersionEntity
    {
        /// <summary>
        ///     Record type idetifier. May be ALARM or PVHI for e.g.
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
