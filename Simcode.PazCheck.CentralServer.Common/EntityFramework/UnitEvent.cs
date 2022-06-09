using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class UnitEvent : Identifiable<int>
    {        
        [Attr(PublicName="eventtime")]
        public DateTime EventTimeUtc { get; set; }

        public string TagName { get; set; } = @"";

        /// <summary>
        ///     ElementName[=Value]
        /// </summary>
        [Attr(PublicName="tagandalarmaondition")]
        public string TagConditionString { get; set; } = @"";

        [Attr(PublicName="alarmconditionisactive")]
        public bool ConditionIsActive { get; set; }

        public string EventSource { get; set; } = @"";

        public string OriginalEvent { get; set; } = @"";
    }
}
