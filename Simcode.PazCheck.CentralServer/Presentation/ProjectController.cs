using IdentityServer4;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Project")]
    public partial class ProjectController : ControllerBase
    {
        #region construction and destruction

        public ProjectController(
            IMainServerWorker mainServerWorker,
            JobsManager jobsManager,
            AddonsManager addonsManager,
            Cache cache,            
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IRefreshTokenStore refreshTokenStore,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<ProjectController> logger)
        {
            _mainServerWorker = mainServerWorker;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _cache = cache;            
            _dbContextFactory = dbContextFactory;
            _refreshTokenStore = refreshTokenStore;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost(@"InitializeProject/{projectId}")]
        public Task<IActionResult> InitializeProjectAsync(int projectId)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            try
            {
                return Task.FromResult<IActionResult>(Ok());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"InitializeProject error. projectId: {0}", projectId);
                return Task.FromResult<IActionResult>(NoContent());
            }            
        }        

        /// <summary>
        ///     Always allowed method.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) }
            )]
        [HttpGet(@"GetCurrentUser")]
        public Task<IActionResult> GetCurrentUserAsync()
        {
            var user = HttpContextHelper.GetUserLowerInvariant(HttpContext);

            //var sourceAddress = HttpContextHelper.GetSourceIpAddress(HttpContext);
            List<string> groups = new() { user };

            return Task.FromResult<IActionResult>(Ok(new { user = user, groups = groups.ToArray() }));
        }

        /// <summary>
        ///     Always allowed method.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) }
            )]
        [HttpPost(@"SetUserParamValue")]
        public async Task<IActionResult> SetUserParamValueAsync(string paramName, string value)
        {
            var user = HttpContextHelper.GetUserLowerInvariant(HttpContext);

            // Normalize
            if (paramName is null)
                paramName = @"";
            if (value is null)
                value = @"";

            try
            {
                await using var dbContext = _dbContextFactory.CreateDbContext();

                //var sourceAddress = HttpContextHelper.GetSourceIpAddress(HttpContext);
                List<string> userGroups = new() { user };

                foreach (var userGroup in userGroups)
                {
                    var userParam = await dbContext.UserParams.FirstOrDefaultAsync(up => up.UserGroup == userGroup && up.ParamName == paramName);
                    if (userParam is null)
                    {
                        userParam = new UserParam()
                        {
                            UserGroup = userGroup,
                            ParamName = paramName,
                        };
                        dbContext.UserParams.Add(userParam);
                    }
                    userParam.Value = value ?? @"";
                }

                await dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"SetUserParamValue error. paramName: {0}; value: {1}", paramName, value);                
            }

            return NoContent();
        }

        /// <summary>
        ///     Always allowed method.
        /// </summary>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) }
            )]
        [HttpGet(@"GetUserParamValue")]
        public async Task<IActionResult> GetUserParamValueAsync(string paramName)
        {
            var user = HttpContextHelper.GetUserLowerInvariant(HttpContext);

            // Normalize
            if (paramName is null)
                paramName = @"";

            try
            {
                await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                //var sourceAddress = HttpContextHelper.GetSourceIpAddress(HttpContext);
                List<string> userGroups = new() { user };

                foreach (var userGroup in userGroups)
                {
                    var userParam = await readOnlyDbContext.UserParams.FirstOrDefaultAsync(up => up.UserGroup == userGroup && up.ParamName == paramName);
                    if (userParam is not null)
                        return Ok(new { value = userParam.Value });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetUserParamValue error. paramName: {0}", paramName);
            }

            return NoContent();
        }

        /// <summary>
        ///     Always allowed method.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) }
            )]
        [HttpPost(@"LogOutCurrentUser")]
        public async Task<IActionResult> LogOutCurrentUserAsync(string type)
        {
            string user = HttpContextHelper.GetUserLowerInvariant(HttpContext);

            if (!String.IsNullOrEmpty(user))
            {
                if (String.Equals(type, @"Timeout", StringComparison.InvariantCultureIgnoreCase))
                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                            HttpContextHelper.GetSourceIpAddress(HttpContext),
                            HttpContextHelper.GetSourceHost(HttpContext),
                            InformationSecurityEventsLogger.UserLogOut_EventId,
                            3,
                            true,
                            Properties.Resources.UserLogOut_Event,
                            user,
                            Common.Properties.Resources.ObjectSystem,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                {
                                    (@"Type", "Timeout"),
                                }),
                            Properties.Resources.UserLogOut_Event_Desc_Timeout);
                if (String.Equals(type, @"RefreshTokenTimeout", StringComparison.InvariantCultureIgnoreCase))
                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                            HttpContextHelper.GetSourceIpAddress(HttpContext),
                            HttpContextHelper.GetSourceHost(HttpContext),
                            InformationSecurityEventsLogger.UserLogOut_EventId,
                            3,
                            true,
                            Properties.Resources.UserLogOut_Event,
                            user,
                            Common.Properties.Resources.ObjectSystem,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                {
                                    (@"Type", "RefreshTokenTimeout"),
                                }),
                            Properties.Resources.UserLogOut_Event_Desc_RefreshTokenTimeout);
                else
                    _informationSecurityEventsLogger.InformationSecurityEvent(user,
                            HttpContextHelper.GetSourceIpAddress(HttpContext),
                            HttpContextHelper.GetSourceHost(HttpContext),
                            InformationSecurityEventsLogger.UserLogOut_EventId,
                            3,
                            true,
                            Properties.Resources.UserLogOut_Event,
                            user,
                            Common.Properties.Resources.ObjectSystem,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                {
                                    (@"Type", "UserAction"),
                                }),
                            Properties.Resources.UserLogOut_Event_Desc_UserAction);

                //await HttpContext.SignOutAsync(IdentityServerPazCheckCentralServerConstants.DefaultCookieAuthenticationScheme);
                await _refreshTokenStore.RemoveRefreshTokensAsync(user, "userfront");
            }            

            return Ok();
        }

        #endregion

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly Cache _cache;        
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion        
    }
}