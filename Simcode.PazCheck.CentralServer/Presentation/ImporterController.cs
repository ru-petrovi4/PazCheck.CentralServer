using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4;
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
    [Route("Importer")]
    public class ImporterController : ControllerBase
    {
        #region construction and destruction

        public ImporterController(
            IMainServerWorker mainServerWorker,
            JobsManager jobsManager,
            AddonsManager addonsManager,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,            
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILogger<ImporterController> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _mainServerWorker = mainServerWorker;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;
            _dbContextFactory = dbContextFactory;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;            
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        #endregion

        #region public functions

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpGet(@"GetImportProjectFilesInfo")]
        public Task<IActionResult> GetImportProjectFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        /// <summary>
        ///     Импортирует из текстовых или архивных файлов в проект CeMatrices, Tags, BaseActuators, SafetyControllers.
        ///     Новые объекты создаются как несохраненные изменения, существующие сохраненные объекты обновляются, 
        ///     существующие несохраненные объекты пропускаются и записываются в лог. 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="formFiles"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="sourceTypeIdentifier"></param>
        /// <param name="optionsString"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost("ImportProjectFiles/{projectId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportProjectFilesAsync(int projectId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {            
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();

            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();            

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            Common.Serialization.ImportSerializationRootObjectResult result;
                            
                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName,                                     
                                    addonIdentifier, 
                                    sourceTypeIdentifier, 
                                    null, 
                                    projectId, 
                                    cancellationToken,
                                    previewJobProgress, 
                                    informationSecurityContext,
                                    _mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet, 
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var mainJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            null,
                                            projectId,
                                            cancellationToken,
                                            mainJobProgress,
                                            informationSecurityContext,
                                            _mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(mainJobProgress.StatusCode))
                                        await mainJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"GetImportEntitiesFilesInfo")]
        public Task<IActionResult> GetImportEntitiesFilesInfoAsync(string entitiesName)
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        /// <summary>
        ///     Импортирует из текстовых или архивных файлов PcObjects, BasePcObjects, PcObjectEvents и т.д.        
        /// </summary>        
        /// <param name="formFiles"></param>
        /// <param name="entitiesName"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="sourceTypeIdentifier"></param>
        /// <param name="optionsString"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpPost("ImportEntitiesFiles")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportEntitiesFilesAsync(List<IFormFile> formFiles, string entitiesName, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();

            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            Common.Serialization.ImportSerializationRootObjectResult result;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName,                                    
                                    addonIdentifier,
                                    sourceTypeIdentifier,
                                    null, 
                                    null, 
                                    cancellationToken,
                                    previewJobProgress, 
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet,
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            null,
                                            null,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        await notPreviewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }                                    
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpGet(@"GetImportCeMatrixFilesInfo")]
        public Task<IActionResult> GetImportCeMatrixFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();            
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            foreach (CustomImportExportAddonBase ceMatrixImportExportAddon in _addonsManager.AddonsThreadSafe.OfType<CustomImportExportAddonBase>())
            {
                var ceMatrix_ImportTypeFilesInfos = ceMatrixImportExportAddon.GetImportCeMatrixFilesInfos();
                if (ceMatrix_ImportTypeFilesInfos.Count > 0) 
                {
                    result.Add(new AddonImportFilesInfo
                    {
                        AddonIdentifier = ceMatrixImportExportAddon.Identifier,
                        AddonDesc = ceMatrixImportExportAddon.Desc,
                        AddonVersion = ceMatrixImportExportAddon.Version,
                        ImportTypeFilesInfos = ceMatrix_ImportTypeFilesInfos
                    });
                }                
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost("ImportCeMatrixFiles/{projectId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportCeMatrixFilesAsync(int projectId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();                        
            
            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetRandomFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            Common.Serialization.ImportSerializationRootObjectResult result;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName,                                     
                                    addonIdentifier, 
                                    sourceTypeIdentifier, 
                                    typeof(Common.EntityFramework.CeMatrix), 
                                    projectId, 
                                    cancellationToken,
                                    previewJobProgress,                                     
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet, 
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            typeof(Common.EntityFramework.CeMatrix),
                                            projectId,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        await notPreviewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }                                
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpGet(@"GetImportTagFilesInfo")]
        public Task<IActionResult> GetImportTagFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            foreach (TagsImportAddonBase tagsImportAddon in _addonsManager.AddonsThreadSafe.OfType<TagsImportAddonBase>())
            {
                var tags_ImportTypeFilesInfos = tagsImportAddon.GetTags_ImportTypeFilesInfos();
                if (tags_ImportTypeFilesInfos.Count > 0)
                {
                    result.Add(new AddonImportFilesInfo
                    {
                        AddonIdentifier = tagsImportAddon.Identifier,
                        AddonDesc = tagsImportAddon.Desc,
                        AddonVersion = tagsImportAddon.Version,
                        ImportTypeFilesInfos = tags_ImportTypeFilesInfos
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost("ImportTagFiles/{projectId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportTagFilesAsync(int projectId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();                        
            
            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }                
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            Common.Serialization.ImportSerializationRootObjectResult result;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName,                                     
                                    addonIdentifier, 
                                    sourceTypeIdentifier, 
                                    typeof(Common.EntityFramework.Tag), 
                                    projectId, 
                                    cancellationToken,
                                    previewJobProgress, 
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet, 
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            typeof(Common.EntityFramework.Tag),
                                            projectId,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        await notPreviewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }                            
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpGet(@"GetImportBaseActuatorFilesInfo")]
        public Task<IActionResult> GetImportBaseActuatorFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });            
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost("ImportBaseActuatorFiles/{projectId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportBaseActuatorFilesAsync(int projectId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();            

            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            Common.Serialization.ImportSerializationRootObjectResult result;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName, 
                                    addonIdentifier, 
                                    sourceTypeIdentifier, 
                                    typeof(Common.EntityFramework.BaseActuator), 
                                    projectId, 
                                    cancellationToken,
                                    previewJobProgress,                                     
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet, 
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            typeof(Common.EntityFramework.BaseActuator),
                                            projectId,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        await notPreviewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpGet(@"GetImportSafetyControllerFilesInfo")]
        public Task<IActionResult> GetImportSafetyControllerFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                            FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
            )]
        [HttpPost("ImportSafetyControllerFiles/{projectId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportSafetyControllerFilesAsync(int projectId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();                        
            
            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportProjectFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            Common.Serialization.ImportSerializationRootObjectResult result;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                result = await SerializationHelper.ImportFileAsync(
                                    _dbContextFactory, 
                                    stream, 
                                    fileName, 
                                    addonIdentifier,
                                    sourceTypeIdentifier, 
                                    typeof(Common.EntityFramework.SafetyController), 
                                    projectId, 
                                    cancellationToken,
                                    previewJobProgress, 
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet, 
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        result = await SerializationHelper.ImportFileAsync(
                                            _dbContextFactory,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            typeof(Common.EntityFramework.SafetyController),
                                            projectId,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        await notPreviewJobProgress.SetJobProgressAsync(100, Common.Properties.Resources.Done, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                                }
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }        

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) }
            )]
        [HttpGet(@"GetImportEventsJournalFilesInfo")]
        public Task<IActionResult> GetImportEventsJournalFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();            
            foreach (EventMessagesProcessingAddonBase eventMessagesProcessingAddon in _addonsManager.AddonsThreadSafe.OfType<EventMessagesProcessingAddonBase>())
            {
                var eventsJournal_ImportTypeFilesInfos = eventMessagesProcessingAddon.GetEventsJournal_ImportTypeFilesInfos();
                if (eventsJournal_ImportTypeFilesInfos.Count > 0)
                {
                    result.Add(new AddonImportFilesInfo
                    {
                        AddonIdentifier = eventMessagesProcessingAddon.Identifier,
                        AddonDesc = eventMessagesProcessingAddon.Desc,
                        AddonVersion = eventMessagesProcessingAddon.Version,
                        ImportTypeFilesInfos = eventsJournal_ImportTypeFilesInfos
                    });
                }
            }
            if (result.Count == 0)             
                result.Add(new AddonImportFilesInfo
                {
                    AddonIdentifier = @"",
                    AddonDesc = @"",
                    AddonVersion = @"",
                    ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                        {
                            new ImportTypeFilesInfo
                            {
                                SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                                SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_Std_File,
                                SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_Std_File_Details,
                                FileExtensions = new string[] { @".json", @".csv", @".xlsx", @".zip" },
                                ImportOptionInfos = new()
                            }
                        }
                });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) }
            )]
        [HttpPost("ImportEventsJournalFiles/{unitId}")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportEventsJournalFilesAsync(int unitId, List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);            

            if (formFiles is null)
                return NoContent();                        
            
            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();                
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportEventsJournalFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            int addedEventsCount;

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                addedEventsCount = await SerializationHelper.ImportEventsJournalFileAsync(
                                    _dbContextFactory,
                                    informationSecurityContext.User,
                                    stream,
                                    fileName,
                                    addonIdentifier,
                                    sourceTypeIdentifier,                                    
                                    unitId,
                                    cancellationToken,
                                    previewJobProgress,                                    
                                    mainServerWorker,                                    
                                    loggersSet,
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                await previewJobProgress.SetJobProgressAsync(100,
                                    Common.Properties.Resources.Done,
                                    Common.Properties.Resources.Added + @" - " + Common.Properties.Resources.UnitEvents + ": " + addedEventsCount,
                                    Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    try
                                    {
                                        using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                        {
                                            addedEventsCount = await SerializationHelper.ImportEventsJournalFileAsync(
                                                _dbContextFactory,
                                                informationSecurityContext.User,
                                                stream,
                                                fileName,
                                                addonIdentifier,
                                                sourceTypeIdentifier,
                                                unitId,
                                                cancellationToken,
                                                notPreviewJobProgress,                                                
                                                mainServerWorker,                                                
                                                loggersSet,
                                                preview: false);
                                        }

                                        if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                            await notPreviewJobProgress.SetJobProgressAsync(100,
                                                Common.Properties.Resources.Done,
                                                Common.Properties.Resources.Added + @" - " + Common.Properties.Resources.UnitEvents + ": " + addedEventsCount,
                                                Ssz.Utils.StatusCodes.Good);
                                    }
                                    finally
                                    {
                                        Common.FileContentType contentType =
                                            new() { Id = @"EventsJournal", Desc = Common.Properties.Resources.EventsJournal_ContentType };
                                        if (informationSecurityContext is not null)
                                            _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                                informationSecurityContext.SourceIpAddress,
                                                informationSecurityContext.SourceHost,
                                                InformationSecurityEventsLogger.DataImported_AllRolesAccessEventId,
                                                6,
                                                Ssz.Utils.StatusCodes.IsGood(jobProgress.StatusCode),
                                                Common.Properties.Resources.ImportFile_EventName,
                                                informationSecurityContext.User,
                                                fileName,
                                                NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                                {
                                                    (@"ContentType", contentType.Id)
                                                }),
                                                Common.Properties.Resources.ImportFile_EventDesc, fileName, SerializationHelper.RemoveHtml(jobProgress.ProgressDetails));
                                    }                                    
                                }
                            }

                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpGet(@"GetImportLicenseFilesInfo")]        
        public Task<IActionResult> GetImportLicenseFilesInfoAsync()
        {
            List<AddonImportFilesInfo> result = new();
            result.Add(new AddonImportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ImportTypeFilesInfos = new List<ImportTypeFilesInfo>()
                    {
                        new ImportTypeFilesInfo
                        {
                            SourceTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            SourceTypeDesc = Common.Properties.Resources.SourceTypeDesc_PcLicFileType,
                            SourceTypeDetails = Common.Properties.Resources.SourceTypeDesc_PcLicFileType_Details,
                            FileExtensions = new string[] { @".pclic", @".zip" },
                            ImportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
            )]
        [HttpPost("ImportLicenseFiles")]
        [PcRequestSizeLimit]
        public async Task<IActionResult> ImportLicenseFilesAsync(List<IFormFile> formFiles, string addonIdentifier, string sourceTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            if (formFiles is null)
                return NoContent();

            var size = formFiles.Sum(f => f.Length);
            var jobIds = new List<string>();

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            foreach (var formFile in formFiles)
            {
                var tempFileFullName = Path.GetTempFileName();               
                await using (var tempFileStream = System.IO.File.Create(tempFileFullName))
                {
                    await formFile.CopyToAsync(tempFileStream, _hostApplicationLifetime.ApplicationStopping);
                }
                var jobId = Guid.NewGuid().ToString();
                jobIds.Add(jobId);
                if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    string fileName = formFile.FileName;
                    _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.ImportLicenseFile_JobTitle, informationSecurityContext.User,
                        async (cancellationToken, jobProgress) =>
                        {
                            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                            var previewJobProgress = await jobProgress.GetChildJobProgressAsync(0, 33, parentFailedIfFailed: true);

                            using (var stream = System.IO.File.OpenRead(tempFileFullName))
                            {
                                await using PazCheckDbContext dbContext = _dbContextFactory.CreateDbContext();
                                dbContext.User = informationSecurityContext.User;
                                dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                                await SerializationHelper.ImportLicenseFileAsync(
                                    dbContext,
                                    stream,
                                    fileName,
                                    addonIdentifier,
                                    sourceTypeIdentifier,                                    
                                    cancellationToken,
                                    previewJobProgress,
                                    informationSecurityContext,
                                    mainServerWorker,
                                    _informationSecurityEventsLogger,
                                    loggersSet,
                                    preview: true);
                            }

                            if (Ssz.Utils.StatusCodes.IsGood(previewJobProgress.StatusCode))
                            {
                                // TODO add info
                                await previewJobProgress.SetJobProgressAsync(100,
                                        Common.Properties.Resources.Done,
                                        Common.Properties.Resources.Added + @" - " + Common.Properties.Resources.LicenseFile,
                                        Ssz.Utils.StatusCodes.GoodMoreData);

                                await jobProgress.Job_ContinuationSemaphoreSlim.WaitAsync();

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

                                    var notPreviewJobProgress = await jobProgress.GetChildJobProgressAsync(33, 100, parentFailedIfFailed: true);

                                    using (var stream = System.IO.File.OpenRead(tempFileFullName))
                                    {
                                        await using PazCheckDbContext dbContext = _dbContextFactory.CreateDbContext();
                                        dbContext.User = informationSecurityContext.User;
                                        dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                                        await SerializationHelper.ImportLicenseFileAsync(
                                            dbContext,
                                            stream,
                                            fileName,
                                            addonIdentifier,
                                            sourceTypeIdentifier,
                                            cancellationToken,
                                            notPreviewJobProgress,
                                            informationSecurityContext,
                                            mainServerWorker,
                                            _informationSecurityEventsLogger,
                                            loggersSet,
                                            preview: false);
                                    }

                                    if (Ssz.Utils.StatusCodes.IsGood(notPreviewJobProgress.StatusCode))
                                        // TODO add info
                                        await notPreviewJobProgress.SetJobProgressAsync(100,
                                                Common.Properties.Resources.Done,
                                                Common.Properties.Resources.Added + @" - " + Common.Properties.Resources.LicenseFile,
                                                Ssz.Utils.StatusCodes.GoodMoreData);
                                }
                            }                           


                            try
                            {
                                System.IO.File.Delete(tempFileFullName);
                            }
                            catch
                            {
                            }
                        });
                }
            }

            return Ok(new { count = formFiles.Count, size, jobIds = jobIds.ToArray() });
        }

        #endregion

        #region private fields

        private readonly IMainServerWorker _mainServerWorker;
        private readonly JobsManager _jobsManager;
        private readonly AddonsManager _addonsManager;
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;        
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        #endregion
    }
}