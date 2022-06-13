using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class UnitEvent : Identifiable<int>
    {        
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     ElementName[=Value]
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";

        [Attr]
        public bool ConditionIsActive { get; set; }

        [Attr]
        public string EventSource { get; set; } = @"";

        [Attr]
        public string OriginalEvent { get; set; } = @"";
    }
}
