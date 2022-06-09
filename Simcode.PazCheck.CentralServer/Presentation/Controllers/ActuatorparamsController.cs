using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class ActuatorParamController : JsonApiController<ActuatorParam, int>
    {
        public ActuatorParamController(IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<ActuatorParam, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
