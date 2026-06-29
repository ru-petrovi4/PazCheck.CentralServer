using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using Ssz.Utils.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using IdentityServer4;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using System.Text;
using Ssz.Utils;
using Ssz.Dcs.CentralServer.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("Configuration")]
    public partial class ConfigurationController : ControllerBase
    {
        #region construction and destruction

        public ConfigurationController(
            IMainServerWorker mainServerWorker,
            IConfiguration configuration,
            JobsManager jobsManager,
            AddonsManager addonsManager,            
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<ConfigurationController> logger)
        {
            _mainServerWorker = mainServerWorker;
            _configuration = configuration;
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
        [HttpGet(@"GetAddonTestInfo")]
        public async Task<IActionResult> GetAddonTestInfoAsync(string addonInstanceId)
        {
            try
            {
                string result = @"";

                if (!_applicationStopping_CancellationToken.IsCancellationRequested)
                {
                    var taskCompletionSource = new TaskCompletionSource<string>();

                    _jobsManager.ServerWorker_ThreadSafeDispatcher.BeginInvoke(ct =>
                    {
                        try
                        {
                            var addon = _addonsManager.Addons.First(a => a.InstanceId == addonInstanceId);
                            var result = addon.GetAddonTestInfo();
                            if (String.IsNullOrEmpty(result))
                                result = Properties.Resources.Default_AddonTestInfo;
                            taskCompletionSource.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            taskCompletionSource.SetException(ex);
                        }
                    });

                    result = await taskCompletionSource.Task;
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"AddonTest error.");
                return NoContent();
            }
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"AddonTest")]
        public async Task<IActionResult> AddonTestAsync(string addonInstanceId, string options)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);                        

            try
            {
                if (!_applicationStopping_CancellationToken.IsCancellationRequested)
                {
                    var taskCompletionSource = new TaskCompletionSource();

                    _jobsManager.ServerWorker_ThreadSafeDispatcher.BeginInvoke(async ct =>
                    {
                        using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(new (string, object?)[]
                        {
                            (UserEventsLogger.ScopeName_JobId, Guid.NewGuid().ToString()),
                        });

                        try
                        {
                            var addon = _addonsManager.Addons.First(a => a.InstanceId == addonInstanceId);
                            await addon.AddonTestAsync(options, loggersSet);
                            taskCompletionSource.SetResult();
                        }
                        catch (Exception ex)
                        {
                            taskCompletionSource.SetException(ex);
                        }
                    });

                    await taskCompletionSource.Task;
                }
            }
            catch (OperationCanceledException)
            {   
            }
            catch (Exception ex)
            {
                loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.AddonTestUnhandledException);                
            }            

            _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                        informationSecurityContext.SourceIpAddress,
                        informationSecurityContext.SourceHost,
                        InformationSecurityEventsLogger.AddonTest_AllRolesAccessEventId,
                        3,
                        loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Error) == 0 &&
                        loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Critical) == 0,
                        Common.Properties.Resources.AddonTest_EventId,
                        informationSecurityContext.User,
                        addonInstanceId,
                        NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                            {
                                (@"AddonIdentifier", addonInstanceId),
                                (@"Options", options),
                            }),
                        Common.Properties.Resources.AddonTest_EventDesc, addonInstanceId);

            string addonTestFinishedMessage = String.Format(Properties.Resources.AddonTestFinished,
                    loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Error),
                    loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Warning));

            loggersSet.LoggerAndUserFriendlyLogger.LogInformation(Properties.Resources.AddonTestFinished_Short);

            return Ok(addonTestFinishedMessage);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"ReadConfiguration")]
        public async Task<IActionResult> ReadConfigurationAsync(string? sourcePath, string? pathRelativeToRootDirectory)
        {
            try
            {
                var reply = await _mainServerWorker.DataAccessProvider.PassthroughAsync(
                                            sourcePath ?? @"",
                                            PassthroughConstants.ReadConfiguration, 
                                            String.IsNullOrEmpty(pathRelativeToRootDirectory) ? new byte[0] : Encoding.UTF8.GetBytes(pathRelativeToRootDirectory));
                var replyConfigurationFiles = new ConfigurationFiles();
                Ssz.Utils.Serialization.SerializationHelper.SetOwnedData(replyConfigurationFiles, reply);
                return Ok(replyConfigurationFiles.ConfigurationFilesCollection.Select(Common.Serialization.ConfigurationFile.CreateFrom).ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"ReadConfiguration error.");
                return NoContent();
            }            
        }

        /// <summary>
        ///     Returns true, if succeeded; false if any errors
        /// </summary>
        /// <param name="configurationFilesCollection"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpPost(@"WriteConfiguration")]
        public async Task<IActionResult> WriteConfigurationAsync([FromBody] Common.Serialization.ConfigurationFile[] configurationFilesCollection)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            try
            {
                bool succeeded = true;                

                if (!_applicationStopping_CancellationToken.IsCancellationRequested && configurationFilesCollection is not null)
                {
                    var configurationFiles = new ConfigurationFiles();
                    configurationFiles.ConfigurationFilesCollection.AddRange(
                        configurationFilesCollection
                            .Where(cf => !String.IsNullOrEmpty(cf.PathRelativeToRootDirectory))
                            .Select(cf => cf.ToConfigurationFile()));

                    await _mainServerWorker.DataAccessProvider.PassthroughAsync(
                        @"",
                        PassthroughConstants.WriteConfiguration,
                        Ssz.Utils.Serialization.SerializationHelper.GetOwnedData(configurationFiles));

                    foreach (var configurationFile in configurationFiles.ConfigurationFilesCollection)
                    {
                        _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                informationSecurityContext.SourceIpAddress,
                                informationSecurityContext.SourceHost,
                                InformationSecurityEventsLogger.ConfigurationChange_EventId,
                                7,
                                succeeded,
                                Properties.Resources.WriteConfigurationFile_Event,
                                informationSecurityContext.User,
                                configurationFile.SourceIdToDisplay,
                                @"",
                                Properties.Resources.WriteConfigurationFile_EventDesc, configurationFile.SourceIdToDisplay, configurationFile.GetPathRelativeToRootDirectory_PlatformSpecific());
                    }                
                }

                return Ok(succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"WriteConfiguration error.");
                return NoContent();
            }
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpPost("AddCryptoEntity")]        
        public async Task<IActionResult> AddCryptoEntityAsync(string identifier, string comment, [FromBody] string valueToEncrypt)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());

            try
            {
                string encryptionPassword = PazCheckDbHelper.GetPostgreCryptoPassword(_configuration, loggersSet);

                await PazCheckDbHelper.SetPostgreCryptoEntityEntityValueAsync(_dbContextFactory, identifier, valueToEncrypt, comment, encryptionPassword);

                _addonsManager.Dispatcher!.BeginInvoke(ct => _addonsManager.RefreshAddons());

                return Ok();
            }
            catch
            {
                return NoContent();
            }
        }

        #endregion

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly IConfiguration _configuration;
        private readonly JobsManager _jobsManager;
        /// <summary>
        ///     Only GetExportedValues_T_() is thread-safe.
        ///     Lock SyncRoot in every call.
        /// </summary>
        private readonly AddonsManager _addonsManager;
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;            
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        #endregion        
    }
}

//new
//{
//    ccf.PathRelativeToRootDirectory,
//    ccf.SourceId,
//    ccf.SourceIdToDisplay,
//    ccf.LastWriteTimeUtc,
//    ccf.FileData
//}