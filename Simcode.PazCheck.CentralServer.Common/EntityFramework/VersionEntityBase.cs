using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class VersionEntityBase : Identifiable<int>
    {
        [HasOne]
        public VersionMessage? _ProposalCreateMessage { get; set; }

        [Attr]
        public DateTime? _ProposalCreateTimeUtc { get; set; }

        [HasOne]
        public VersionMessage? _CreateMessage { get; set; }

        [Attr]
        public DateTime? _CreateTimeUtc { get; set; }

        [HasOne]
        public VersionMessage? _ProposalDeleteMessage { get; set; }

        [Attr]
        public DateTime? _ProposalDeleteTimeUtc { get; set; }

        [HasOne]
        public VersionMessage? _DeleteMessage { get; set; }

        [Attr]
        public DateTime? _DeleteDateTimeUtc { get; set; }

        [HasOne]
        public VersionMessage? _RejectedMessage { get; set; }

        [Attr]
        public DateTime? _RejectedDateTimeUtc { get; set; }

        /// <summary>
        ///     Configured as calculated in DB property. 
        /// </summary>
        [Attr]
        public bool IsActive { get; set; }
    }
}
