using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Office : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = @"";

        [HasMany]
        public List<Simuser> Users { get; set; } = new();
    }
}
