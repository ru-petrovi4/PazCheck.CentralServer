using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Intersection : VersionEntity
    {
        [HasOne]
        public Cause Cause { get; set; } = null!;

        [HasOne]
        public Effect Effect { get; set; } = null!;

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;
    }
}
