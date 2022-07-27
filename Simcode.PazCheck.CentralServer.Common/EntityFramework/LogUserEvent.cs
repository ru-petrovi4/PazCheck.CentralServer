using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class LogUserEvent : Identifiable<int>
    {
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     2 - Information; 3 - Warning; 4 - Error;
        /// </summary>
        [Attr]
        public int LogLevel { get; set; }

        [Attr]
        public string Message { get; set; } = @"";

        [Attr]
        public string Details { get; set; } = @"";
    }
}
