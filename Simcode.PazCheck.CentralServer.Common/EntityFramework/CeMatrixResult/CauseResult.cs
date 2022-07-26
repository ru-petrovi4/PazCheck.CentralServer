using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class CauseResult : Identifiable<int>
    {
        [Attr]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     Is row with debug info
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        [HasMany]
        public List<SubCauseResult> SubCauseResults { get; set; } = new();

        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
