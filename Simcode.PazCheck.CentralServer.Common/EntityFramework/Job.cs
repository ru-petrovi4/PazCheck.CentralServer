using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Job : Identifiable<int>
    {
        [Attr(PublicName="guid")]
        public string Guid { get; set; } = @"";
        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";
        [Attr(PublicName="issuccess")]
        public bool IsSuccess { get; set; }
        [Attr(PublicName="message")]
        public string Message { get; set; } = @"";
        [Attr(PublicName="status")]
        public TaskStatus Status { get; set; }
    }
}
