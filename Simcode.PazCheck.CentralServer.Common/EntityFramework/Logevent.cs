using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Logevent : Identifiable<int>
    {        
        [Attr(PublicName="eventtime")]
        public DateTime EventTimeUtc { get; set; }

        [Attr(PublicName="tagandalarmaondition")]
        public string TagAndAlarmCondition { get; set; } = @"";

        [Attr(PublicName="alarmconditionisactive")]
        public bool AlarmConditionIsActive { get; set; }

        [HasOne(PublicName = "log")]
        public Log Log { get; set; }

    }
}
