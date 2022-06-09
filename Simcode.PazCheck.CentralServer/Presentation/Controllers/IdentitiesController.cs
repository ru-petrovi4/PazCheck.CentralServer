using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class IdentitiesController : JsonApiController<TagCondition, int>
    {
        public IdentitiesController(IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<TagCondition, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
