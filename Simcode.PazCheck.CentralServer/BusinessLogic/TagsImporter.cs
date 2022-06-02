using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{   
    public class TagsImporter
    {
        #region construction and destruction

        public TagsImporter(ILogger<TagsImporter> logger,
            PazCheckDbContext context,
            AddonsManager addonsManager,
            IHostApplicationLifetime applicationLifetime
            )
        {
            _logger = logger;
            _context = context;
            _addonsManager = addonsManager;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        public async Task ImportTagsAsync(Stream stream, int projectId, IJobProgress jobProgress, CancellationToken cancellationToken)
        {            
            var project = await _context.Projects
                .Include(prj => prj.Tags)
                .ThenInclude(tag => tag.Identities)
                .FirstAsync(prj => prj.Id == projectId, cancellationToken: _applicationStopping_CancellationToken);
        }

        public Task ImportTags(Project project, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region private fields

        private readonly ILogger _logger;

        private readonly PazCheckDbContext _context;

        private readonly AddonsManager _addonsManager;

        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}
