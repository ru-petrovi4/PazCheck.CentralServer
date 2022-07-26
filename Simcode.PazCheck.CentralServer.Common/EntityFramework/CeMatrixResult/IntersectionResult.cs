using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class IntersectionResult : Identifiable<int>
    {
        [HasOne]
        public CauseResult CauseResult { get; set; } = null!;

        [Attr]
        public int? SubCauseNum { get; set; }

        [HasOne]
        public EffectResult EffectResult { get; set; } = null!;

        /// <summary>
        ///     TagConditionString_SymbolToDisplay или Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;
    }
}
