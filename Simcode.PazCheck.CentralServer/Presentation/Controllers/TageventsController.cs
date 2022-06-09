// Copyright (c) 2021
// All rights reserved by Simcode

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class TageventsController : JsonApiController<TagEvent, int>
    {
        public TageventsController(IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<TagEvent, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
