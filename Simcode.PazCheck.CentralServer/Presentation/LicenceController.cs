using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("License")]
    public class LicenseController : ControllerBase
    {
        #region construction and destruction

        public LicenseController(
            IMainServerWorker mainServerWorker,
            JobsManager jobsManager,
            AddonsManager addonsManager,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<LicenseController> logger)
        {
            _mainServerWorker = mainServerWorker;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _dbContextFactory = dbContextFactory;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"GetLicenseFileInfos")]
        public async Task<IActionResult> GetLicenseFileInfosAsync()
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            await using var dbContext = _dbContextFactory.CreateDbContext();            

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            List<LicenseFileInfo> result = SerializationHelper.GetLicenseFileInfos(dbContext, _addonsManager, loggersSet);
            
            return Ok(new { results = result.ToArray() });
        }

        #endregion

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion
    }
}