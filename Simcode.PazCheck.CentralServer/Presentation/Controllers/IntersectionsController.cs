using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class IntersectionsController : JsonApiController<Intersection, int>
    {
        public IntersectionsController(IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<Intersection, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
