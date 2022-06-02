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
    public class Identity : Identifiable<int>
    {
        /**
         * Record type idetifier. May be ALARM or PVHI for e.g.
         */
        [Attr(PublicName="identifier")]
        public string Identifier { get; set; } = @"";

        /**
         * Event type that may be for e.g. Alarm or Acknowledge
         */
        [Attr(PublicName="eventtype")]
        public string EventType { get; set; } = @"";

        /**
         * Value that the tag reched in this record
         */
        [Attr(PublicName="value")]
        public string Value { get; set; } = @"";

        [HasOne(PublicName="tag")]
        public Tag Tag { get; set; }

        [HasMany(PublicName = "causes")]
        public List<Cause> Causes { get; set; } = new();
        [NotMapped]
        public int Key { get; set; }

        public bool ProccessEvent(LogRecord logRecord)
        {
            var isEqual = string.Equals(Tag.Name, logRecord.Tag)
                          && string.Equals(Identifier, logRecord.Identifier)
                          && string.Equals(EventType, logRecord.EventType);
            if (Value.Length > 0)
                isEqual = isEqual && string.Equals(Value, logRecord.Value);
            return isEqual;
        }
    }
}
