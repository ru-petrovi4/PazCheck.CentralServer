using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class TagsController : JsonApiController<Tag, int>
    {
        public TagsController(IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<Tag, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
        [HttpGet]
        public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken) =>
            await base.GetAsync(cancellationToken);
    }
}
