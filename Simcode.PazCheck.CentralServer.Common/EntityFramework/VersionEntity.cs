using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class VersionEntity : Identifiable<int>
    {
        public string _ProposalCreateUser { get; set; } = @"";

        public DateTime? _ProposalCreateTimeUtc { get; set; }

        public string _CreateUser { get; set; } = @"";

        public DateTime? _CreateTimeUtc { get; set; }

        public string _ProposalDeleteUser { get; set; } = @"";

        public DateTime? _ProposalDeleteTimeUtc { get; set; }

        public string _DeleteUser { get; set; } = @"";

        public DateTime? _DeleteDateTimeUtc { get; set; }

        public bool IsActive => _CreateTimeUtc is not null && _DeleteDateTimeUtc is null;
    }
}
