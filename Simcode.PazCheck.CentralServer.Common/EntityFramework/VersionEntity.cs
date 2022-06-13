﻿using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class VersionEntity : Identifiable<int>
    {
        [Attr]
        public string _ProposalCreateUser { get; set; } = @"";

        [Attr]
        public DateTime? _ProposalCreateTimeUtc { get; set; }

        [Attr]
        public string _CreateUser { get; set; } = @"";

        [Attr]
        public DateTime? _CreateTimeUtc { get; set; }

        [Attr]
        public string _ProposalDeleteUser { get; set; } = @"";

        [Attr]
        public DateTime? _ProposalDeleteTimeUtc { get; set; }

        [Attr]
        public string _DeleteUser { get; set; } = @"";

        [Attr]
        public DateTime? _DeleteDateTimeUtc { get; set; }

        [Attr]
        public bool IsActive => _CreateTimeUtc is not null && _DeleteDateTimeUtc is null;
    }
}