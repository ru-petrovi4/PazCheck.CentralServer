using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class Project_ChangedMessage
    {
        public int ProjectId { get; set; }

        public string HubConnectionIds { get; set; } = @"";
    }

    public class Project_ChangedMessage_EqualityComparer : IEqualityComparer<Project_ChangedMessage>
    {
        public static Project_ChangedMessage_EqualityComparer Instance { get; } = new();

        public bool Equals(Project_ChangedMessage? x, Project_ChangedMessage? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.ProjectId == y.ProjectId && x.HubConnectionIds == y.HubConnectionIds;
        }

        public int GetHashCode([DisallowNull] Project_ChangedMessage obj)
        {
            return 0;
        }
    }
}
