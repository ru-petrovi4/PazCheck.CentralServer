using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Job : Identifiable<int>
    {
        [Attr]
        public string Guid { get; set; } = @"";

        [Attr]
        public string Name { get; set; } = @"";

        [Attr]
        public bool IsSuccess { get; set; }

        [Attr]
        public string Message { get; set; } = @"";

        [Attr]
        public TaskStatus Status { get; set; }
    }
}
