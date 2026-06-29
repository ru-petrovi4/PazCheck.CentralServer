using CommunityToolkit.HighPerformance.Helpers;
using IdentityServer4;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Presentation.Grafana.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    /// <summary>
    ///     Grafana plugin simpod-json-datasource, v0.2.6
    ///     grafana-cli plugins install simpod-json-datasource
    /// </summary>
    [Route("Widgets")]
    public partial class WidgetsController : ControllerBase
    {
        #region construction and destruction

        public WidgetsController(
            IMainServerWorker mainServerWorker,
            Cache cache,
            JobsManager jobsManager,
            AddonsManager addonsManager,            
            IDbContextFactory<PazCheckDbContext> dbContextFactory,            
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<WidgetsController> logger)
        {
            _mainServerWorker = mainServerWorker;
            _cache = cache;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;            
            _dbContextFactory = dbContextFactory;            
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        /// <summary>
        ///     Function for autogenerating default permissions.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.WidgetsLongRunningView) }
            )]
        [HttpGet(@"WidgetsViewer")]
        public Task<IActionResult> WidgetsViewerAsync()
        {            
            return Task.FromResult<IActionResult>(Ok());
        }

        /// <summary>
        ///     Function for autogenerating default permissions.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.WidgetsLongRunningView) }
            )]
        [HttpGet(@"WidgetsLongRunningViewer")]
        public Task<IActionResult> WidgetsLongRunningViewerAsync()
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        /// <summary>
        ///     Function for autogenerating default permissions.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) }
            )]
        [HttpGet(@"WidgetsEditor")]
        public Task<IActionResult> WidgetsEditorAsync()
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        /// <summary>
        ///     Function for autogenerating default permissions.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"WidgetsAdmin")]
        public Task<IActionResult> WidgetsAdminAsync()
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        #endregion        

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly Cache _cache;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;       
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;        
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion                
    }    
}