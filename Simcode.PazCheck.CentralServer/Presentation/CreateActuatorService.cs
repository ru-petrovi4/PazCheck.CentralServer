using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class CreateActuatorService : JsonApiResourceService<Actuator, int>
    {
        private readonly PazCheckDbContext _context;
        public CreateActuatorService(IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options, IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<Actuator> resourceChangeTracker,
            IResourceDefinitionAccessor resourceDefinitionAccessor, PazCheckDbContext context) : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, resourceDefinitionAccessor)
        {
            _context = context;
        }

        public override async Task<Actuator> CreateAsync(Actuator resource, CancellationToken cancellationToken)
        {
            var act = await base.CreateAsync(resource, cancellationToken);
            var param = new Param {ParamName = "MaxSafeSpeed", Value = "5"};
            act.Params.Add(param);
            param = new Param { ParamName = "SafeSpeed", Value = "2" };
            act.Params.Add(param);
            await _context.SaveChangesAsync(cancellationToken); //ToDo: Сделать по человечески. Тут явно должно как-то работать через классы json api

            return act;
        }
    }
}
