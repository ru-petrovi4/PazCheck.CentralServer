using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    /**
    * Исполнительные механизмы создаются на основе зарезервированных типов.
    * Подробнее смотри CreateActuatorService
    */
    public class ActuatorController : JsonApiController<Actuator, int>
    {
        public ActuatorController(IJsonApiOptions options, IResourceGraph resourceGraph,            
            ILoggerFactory loggerFactory,
            IResourceService<Actuator, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
