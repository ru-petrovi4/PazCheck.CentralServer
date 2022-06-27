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
    public class SetActiveProjectVersionRequest : Identifiable<int>
    {
        [HasOne]
        public ProjectVersion RequestedActiveProjectVersion { get; set; } = null!;

        /// <summary>
        ///     Создавший запрос пользователь.
        /// </summary>
        [Attr]
        public string RequestUser { get; set; } = @"";

        [Attr]
        public string RequestMessage { get; set; } = @"";

        /// <summary>
        ///     Ответивший пользователь.
        /// </summary>
        [Attr]
        public string ReplyUser { get; set; } = @"";

        [Attr]
        public string ReplyMessage { get; set; } = @"";

        /// <summary>
        ///     0 - unknown
        ///     1 - approved
        ///    -1 - rejected
        /// </summary>
        [Attr]
        public int Status { get; set; }
    }
}
