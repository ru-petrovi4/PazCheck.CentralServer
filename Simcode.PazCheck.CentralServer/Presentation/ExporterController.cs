using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("Exporter")]
    public class ExporterController : ControllerBase
    {
        #region construction and destruction

        public ExporterController(
            IMainServerWorker mainServerWorker,
            Cache cache,
            JobsManager jobsManager,
            AddonsManager addonsManager,            
            IDbContextFactory<PazCheckDbContext> dbContextFactory,            
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILogger<ExporterController> logger,            
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _mainServerWorker = mainServerWorker;
            _cache = cache;
            _jobsManager = jobsManager;
            _addonsManager = addonsManager;            
            _dbContextFactory = dbContextFactory;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        #endregion

        #region public functions

        /// <summary>
        ///     «агружает требуемые файлы в виде архива *.zip
        /// </summary>
        /// <param name="exportFilesInfo"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportFiles")]
        public async Task<IActionResult> ExportFilesAsync([FromBody] ExportFilesInfo exportFilesInfo)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            var sourceAddress = HttpContextHelper.GetSourceIpAddress(HttpContext);
            
            if (exportFilesInfo.DbFileIds is null ||
                exportFilesInfo.FileRelativePathAndNames is null)
                return NoContent();

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            (string? fileDownloadName, Stream? stream) = await SerializationHelper.DownloadFilesAsync(
                readOnlyDbContext,
                exportFilesInfo.DbFileIds,
                exportFilesInfo.FileRelativePathAndNames,  
                loggersSet);
            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        /// <summary>
        ///     «агружает требуемый файл 'как есть'
        /// </summary>
        /// <param name="dbFileId"></param>
        /// <param name="fileDownloadName"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportFile")]
        public async Task<IActionResult> ExportFileAsync(int dbFileId, string fileDownloadName)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            Stream? stream = await SerializationHelper.DownloadFileAsync(
                readOnlyDbContext,
                dbFileId,                
                loggersSet);
            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportProjectFilesInfo")]
        public Task<IActionResult> GetExportProjectFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });            
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportCeMatrixFilesInfo")]
        public Task<IActionResult> GetExportCeMatrixFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },                        
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        }                        
                    }
            });
            foreach (CustomImportExportAddonBase ceMatrixImportExportAddon in _addonsManager.AddonsThreadSafe.OfType<CustomImportExportAddonBase>())
            {
                var ceMatrix_ExportTypeFilesInfos = ceMatrixImportExportAddon.GetExportCeMatrixFilesInfos();
                if (ceMatrix_ExportTypeFilesInfos.Count > 0)
                {
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = ceMatrixImportExportAddon.Identifier,
                        AddonDesc = ceMatrixImportExportAddon.Desc,
                        AddonVersion = ceMatrixImportExportAddon.Version,
                        ExportTypeFilesInfos = ceMatrix_ExportTypeFilesInfos
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportTagFilesInfo")]
        public Task<IActionResult> GetExportTagFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },                        
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportBaseActuatorFilesInfo")]
        public Task<IActionResult> GetExportBaseActuatorFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportSafetyControllerFilesInfo")]
        public Task<IActionResult> GetExportSafetyControllerFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportLegendFilesInfo")]
        public Task<IActionResult> GetExportLegendFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        /// <summary>
        ///     Ёкспортирует из версии проекта CeMatrices, Tags, BaseActuators, SafetyControllers в файл .zip. 
        /// </summary>
        /// <param name="projectEntitiesCollectionInfo"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="optionsString"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportProjectFile")]
        public async Task<IActionResult> ExportProjectFileAsync([FromBody] ProjectEntitiesCollectionInfo projectEntitiesCollectionInfo, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            // Normalize
            if (projectEntitiesCollectionInfo.ProjectVersionNum == 0)
                projectEntitiesCollectionInfo.ProjectVersionNum = null;
            if (addonIdentifier is null)
                addonIdentifier = "";

            // TODO
            if (addonIdentifier == @"" &&
                (String.IsNullOrEmpty(destinationTypeIdentifier) ||
                String.Equals(destinationTypeIdentifier, "StdFile", StringComparison.InvariantCultureIgnoreCase))) // StdFile is Obsolete
            {
                projectEntitiesCollectionInfo.FullExport_ProjectVersion = true;
                destinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File;
            }

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            (string? fileDownloadName, Stream? stream) = await SerializationHelper.ExportProjectAsync(
                readOnlyDbContext,
                projectEntitiesCollectionInfo,
                mainServerWorker,
                addonIdentifier,
                destinationTypeIdentifier,
                loggersSet);
            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportEntitiesFilesInfo")]
        public Task<IActionResult> GetExportEntitiesFilesInfoAsync(string entitiesName)
        {
            if (String.Equals(entitiesName, PazCheckConstants.ExportEntitiesName_ReferenceEntities, StringComparison.InvariantCultureIgnoreCase))
            {
                List<AddonExportFilesInfo> result = new();
                result.Add(new AddonExportFilesInfo
                {
                    AddonIdentifier = @"",
                    AddonDesc = @"",
                    AddonVersion = @"",
                    ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                            ExportOptionInfos = new()
                        },
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                            ExportOptionInfos = new()
                        }
                    }
                });
                return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
            }
            else
            {
                PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName, out PropertyInfo? pazCheckDbContext_PropertyInfo);
                if (pazCheckDbContext_PropertyInfo is null)
                    return Task.FromResult<IActionResult>(NotFound());
                Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
                if (entityType == typeof(Common.EntityFramework.PcObject) || 
                    entityType == typeof(Common.EntityFramework.BasePcObject))
                {
                    List<AddonExportFilesInfo> result = new();
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = @"",
                        AddonDesc = @"",
                        AddonVersion = @"",
                        ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                        {
                            new ExportTypeFilesInfo
                            {
                                DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File,
                                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
                                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
                                ExportOptionInfos = new()
                            },
                            new ExportTypeFilesInfo
                            {
                                DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                                ExportOptionInfos = new()
                            },
                            new ExportTypeFilesInfo
                            {
                                DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended,
                                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended,
                                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended_Details,
                                ExportOptionInfos = new()
                            }
                        }
                    });
                    return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
                }
            }            
            return Task.FromResult<IActionResult>(NotFound());
        }

        /// <summary>
        ///     Ёкспортирует объекты в файл .zip. 
        /// </summary>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="entitiesName"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="optionsString"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportEntitiesFile")]
        public async Task<IActionResult> ExportEntitiesFileAsync([FromBody] EntitiesCollectionInfo entitiesCollectionInfo, string entitiesName, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var mainServerWorker = _mainServerWorker.ServiceProvider.GetRequiredService<IMainServerWorker>();

            (string? fileDownloadName, Stream? stream) = await SerializationHelper.ExportEntitiesAsync(
                readOnlyDbContext,
                entitiesCollectionInfo,
                fullExport: true,
                entitiesName,
                mainServerWorker,
                addonIdentifier,
                destinationTypeIdentifier,
                loggersSet);
            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportResultFilesInfo")]
        public Task<IActionResult> GetExportResultFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            //result.Add(new AddonExportFilesInfo
            //{
            //    AddonIdentifier = @"",
            //    AddonDesc = @"",
            //    AddonVersion = @"",
            //    ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
            //        {
            //            new ExportTypeFilesInfo
            //            {
            //                DestinationTypeIdentifier = PazCheckCentralServerConstants.TypeIdentifier_Std_File,
            //                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_File,
            //                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_File_Details,
            //                ExportOptionInfos = new()
            //            }
            //        }
            //});
            foreach (AddonBase reportsExportAddon in _addonsManager.AddonsThreadSafe.OfType<IReportsExportAddon>())
            {
                var exportResultFilesInfoExs = ((IReportsExportAddon)reportsExportAddon).GetExportTypeFilesInfoExs([
                    IReportsExportAddon.ReportCategory_DiagnostResult_UpperCase,
                    IReportsExportAddon.ReportCategory_DiagnostResultDetailed_UpperCase ]);
                if (exportResultFilesInfoExs.Length > 0)
                {
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = reportsExportAddon.Identifier,
                        AddonDesc = reportsExportAddon.Desc,
                        AddonVersion = reportsExportAddon.Version,
                        ExportTypeFilesInfos = exportResultFilesInfoExs.Select(fi => fi.ExportTypeFilesInfo).ToList(),
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportResultToFile/{resultId}")]
        public async Task<IActionResult> ExportResultToFileAsync(int resultId, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            bool succeeded = false;

            if (addonIdentifier is null)
                addonIdentifier = @""; // Normalize
            if (String.IsNullOrEmpty(destinationTypeIdentifier))
                destinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_File; // Normalize
                                                                              
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            try
            {
                IReportsExportAddon? reportsExportAddon = _addonsManager.AddonsThreadSafe
                    .OfType<IReportsExportAddon>()
                    .FirstOrDefault(a => String.Equals(((AddonBase)a).Identifier, addonIdentifier, StringComparison.InvariantCultureIgnoreCase));

                List<(string, byte[])> filesData = new();

                if (reportsExportAddon is not null)
                {
                    Filter filter = FilterHelper.Create(new List<CaseInsensitiveOrderedDictionary<List<string>>>
                    {
                        new CaseInsensitiveOrderedDictionary<List<string>>()
                        {
                            { PazCheckConstants.CriterionName_ResultId, [ resultId.ToString() ]}
                        }
                    });                    

                    filesData.AddRange(await reportsExportAddon.ExportReportAsync(
                        _dbContextFactory,
                        _cache.DbCache,                        
                        destinationTypeIdentifier,
                        filter,
                        new CaseInsensitiveOrderedDictionary<string?>(),
                        _hostApplicationLifetime.ApplicationStopping,
                        loggersSet));
                }

                if (filesData.Count == 0)
                    return NoContent();                

                succeeded = true;
                
                return File(
                    SerializationHelper.ExportZip(filesData),
                    "APPLICATION/octet-stream",
                    Path.GetFileNameWithoutExtension(filesData[0].Item1) + ".zip");
            }
            catch
            {
                _logger.LogError("Invalid ResultId: {0}", resultId);
                return NoContent();
            }
            finally
            {
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,                                    
                                    InformationSecurityEventsLogger.DataExported_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Common.Properties.Resources.ExportResultToFile_Event,
                                    informationSecurityContext.User,
                                    Common.Properties.Resources.Result,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"ExportResultToFile"),                                            
                                            (@"ResultId", new Ssz.Utils.Any(resultId).ValueAsString(false))                                            
                                        }),
                                    Common.Properties.Resources.ExportResultToFile_EventDesc);
            }            
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportCeMatrixResultFilesInfo")]
        public Task<IActionResult> GetExportCeMatrixResultFilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();
            //result.Add(new AddonExportFilesInfo
            //{
            //    AddonIdentifier = @"",
            //    AddonDesc = @"",
            //    AddonVersion = @"",
            //    ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
            //        {   
            //            new ExportTypeFilesInfo
            //            {
            //                DestinationTypeIdentifier = PazCheckCentralServerConstants.TypeIdentifier_Std_ExcelFile,
            //                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
            //                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
            //                ExportOptionInfos = new()
            //            }
            //        }
            //});
            foreach (CustomImportExportAddonBase ceMatrixImportExportAddon in _addonsManager.AddonsThreadSafe.OfType<CustomImportExportAddonBase>())
            {
                var ceMatrix_ExportTypeFilesInfos = ceMatrixImportExportAddon.GetExportCeMatrixResultFilesInfos();
                if (ceMatrix_ExportTypeFilesInfos.Count > 0)
                {
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = ceMatrixImportExportAddon.Identifier,
                        AddonDesc = ceMatrixImportExportAddon.Desc,
                        AddonVersion = ceMatrixImportExportAddon.Version,
                        ExportTypeFilesInfos = ceMatrix_ExportTypeFilesInfos
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        /// <summary>
        ///     Ёкспорт результатов анализа матриц ѕ——.
        /// </summary>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="optionsString"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportCeMatrixResultsToFile")]
        public async Task<IActionResult> ExportCeMatrixResultsToFileAsync([FromBody] EntitiesCollectionInfo entitiesCollectionInfo, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            // Norm
            if (addonIdentifier is null)
                addonIdentifier = @""; // Normalize
            if (String.IsNullOrEmpty(destinationTypeIdentifier))
                destinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile; // Normalize

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
            if (ceMatrixRuntimeAddon is null)
                return NoContent();

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            List<CeMatrixResult> ceMatrixResults;
            try
            {
                ceMatrixResults = await readOnlyDbContext.CeMatrixResults
                        .Where(PazCheckDbHelper.GetEntity_Predicate<Common.EntityFramework.CeMatrixResult>(entitiesCollectionInfo.IncludeAll,
                            entitiesCollectionInfo.IdsToInclude, entitiesCollectionInfo.IdsToExclude))
                        .Include(mr => mr.Result)
                        .Include(mr => mr.RowResults)
                        .ThenInclude(rr => rr.ResultEvents)
                        .ThenInclude(re => re.TriggeredUnitEvent)
                        .Include(mr => mr.ColumnResults)
                        .ThenInclude(cr => cr.ResultEvents)
                        .ThenInclude(re => re.TriggeredUnitEvent)
                        .Include(mr => mr.CellResults)
                        .ThenInclude(cr => cr.RowResult)
                        .Include(mr => mr.CellResults)
                        .ThenInclude(cr => cr.ColumnResult)
                        .Include(mr => mr.CellResults)
                        .ThenInclude(cr => cr.ResultEvents)
                        .ThenInclude(re => re.TriggeredUnitEvent)
                        .ToListAsync();                
            }
            catch
            {
                _logger.LogError("Invalid ceMatrixIds");
                return NoContent();
            }

            (string? fileDownloadName, Stream? stream) = await SerializationHelper.ExportCeMatrixResultsToFileAsync(
                readOnlyDbContext, 
                ceMatrixResults,
                addonIdentifier,
                destinationTypeIdentifier,
                informationSecurityContext.User,
                _mainServerWorker,
                _hostApplicationLifetime.ApplicationStopping,
                _informationSecurityEventsLogger,
                loggersSet);
            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetExportVersionComparisonFilesInfo")]
        public Task<IActionResult> GetExportVersionComparisonFilesInfoAsync(string versionedEntityType)
        {
            List<AddonExportFilesInfo> result = new();
            result.Add(new AddonExportFilesInfo
            {
                AddonIdentifier = @"",
                AddonDesc = @"",
                AddonVersion = @"",
                ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                    {
                        new ExportTypeFilesInfo
                        {
                            DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_HumanReadable_ExcelFile,
                            DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_HumanReadable_ExcelFile,
                            DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_HumanReadable_ExcelFile_Details,
                            ExportOptionInfos = new()
                        }
                    }
            });
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        /// <summary>
        ///     Ёкспорт результатов сравнени€ версий сущности в файл.
        /// </summary>
        /// <param name="versionedEntityType"></param>
        /// <param name="versionedEntityId"></param>
        /// <param name="minProjectVersionNum"></param>
        /// <param name="maxProjectVersionNum"></param>
        /// <param name="user"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="optionsString"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpPost("ExportVersionComparisonToFile")]
        public async Task<IActionResult> ExportVersionComparisonToFileAsync(string versionedEntityType, 
            int versionedEntityId, 
            UInt32 minProjectVersionNum, 
            UInt32? maxProjectVersionNum, 
            string user,
            string addonIdentifier, 
            string destinationTypeIdentifier, 
            string optionsString)
        {
            // Normalize
            if (maxProjectVersionNum == 0)
                maxProjectVersionNum = null;

            user = (user ?? @"").ToLowerInvariant(); // Normalize

            user = _cache.GetEmptyIfAdministrator(user, HttpContext);

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            await using var readOnlyDbContext = _dbContextFactory.CreateDbContext();            
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(versionedEntityType, out PropertyInfo? propertyInfo);
            if (propertyInfo is null)
            {
                _logger.LogError("Invalid versionedEntityType: " + versionedEntityType);
                return NoContent();
            }

            Type versionedEntityType_ = propertyInfo.PropertyType.GetGenericArguments().First();

            Stream? stream;
            string? fileDownloadName;
            bool succeeded = false;            
            try
            {
                (fileDownloadName, stream) = await PazCheckDbHelper.ExportVersionComparisonToFileAsync(readOnlyDbContext, 
                    versionedEntityType_,
                    versionedEntityId,
                    minProjectVersionNum,
                    maxProjectVersionNum,
                    _cache.DbCache,
                    user);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportVersionComparisonToFile Error");
                return NoContent();
            }
            finally
            {
                string enitityTypeDesc = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? versionedEntityType_.Name;                
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,
                                    InformationSecurityEventsLogger.DataExported_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Properties.Resources.ExportVersionComparisonToFile_Event,
                                    informationSecurityContext.User,
                                    enitityTypeDesc,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"ExportVersionComparisonToFile"),
                                            (@"VersionedEntityType", versionedEntityType_.Name),
                                            (@"VersionedEntityId", new Ssz.Utils.Any(versionedEntityId).ValueAsString(false)),
                                            (@"MinProjectVersionNum", new Ssz.Utils.Any(minProjectVersionNum).ValueAsString(false)),
                                            (@"MaxProjectVersionNum", new Ssz.Utils.Any(maxProjectVersionNum).ValueAsString(false))
                                        }),
                                    Properties.Resources.ExportVersionComparisonToFile_EventDesc, enitityTypeDesc, minProjectVersionNum, maxProjectVersionNum);
            }

            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpGet(@"GetExportEvents_FilesInfo")]
        public Task<IActionResult> GetExportEvents_FilesInfoAsync(string entitiesName)
        {
            List<AddonExportFilesInfo> result =
            [                
                new AddonExportFilesInfo
                {
                    AddonIdentifier = @"",
                    AddonDesc = @"",
                    AddonVersion = @"",
                    ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                        {   
                            new ExportTypeFilesInfo
                            {
                                DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                                ExportOptionInfos = new()
                            },
                        }
                },
            ];
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpPost("ExportEventsToFile")]
        public async Task<IActionResult> ExportEventsToFileAsync([FromBody] Filter filter, string entitiesName, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            filter.User = (filter.User ?? @"").ToLowerInvariant(); // Normalize             
            filter.SearchString = filter.SearchString?.Trim();

            var roles = HttpContextHelper.GetRoles(HttpContext);
            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(entitiesName ?? @"", out PropertyInfo? pazCheckDbContext_PropertyInfo);
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Read");
            if (!checkAccessSucceeded)
                return Unauthorized();            

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            Stream? stream;
            string? fileDownloadName;
            bool succeeded = false;            
            try
            {                
                (fileDownloadName, stream) = await SerializationHelper.ExportEventsToFileAsync(
                    _dbContextFactory,
                    _cache.DbCache,
                    entityType,
                    destinationTypeIdentifier,
                    filter);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportEventsToFile Error");
                return NoContent();
            }
            finally
            {
                string enitityTypeDesc = pazCheckDbContext_PropertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? entityType.Name;                
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,
                                    InformationSecurityEventsLogger.DataExported_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Properties.Resources.ExportEventsToFile_Event,
                                    informationSecurityContext.User,
                                    enitityTypeDesc,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"ExportEventsToFile"),
                                            (@"VersionedEntityType", entityType.Name)
                                        }),
                                    Properties.Resources.ExportEventsToFile_EventDesc, enitityTypeDesc);
            }

            if (stream is null)
                return NoContent();

            var contentType = "APPLICATION/octet-stream";            
            return File(stream, contentType, fileDownloadName);
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpGet(@"GetExportMonitoringWidgetsReport_FilesInfo")]
        public Task<IActionResult> GetExportMonitoringWidgetsReport_FilesInfoAsync()
        {
            List<AddonExportFilesInfo> result = new();            
            foreach (AddonBase reportsExportAddon in _addonsManager.AddonsThreadSafe.OfType<IReportsExportAddon>())
            {
                var exportResultFilesInfoExs = ((IReportsExportAddon)reportsExportAddon).GetExportTypeFilesInfoExs([
                    IReportsExportAddon.ReportCategory_MonitoringWidgetsPage_UpperCase ]);
                if (exportResultFilesInfoExs.Length > 0)
                {
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = reportsExportAddon.Identifier,
                        AddonDesc = reportsExportAddon.Desc,
                        AddonVersion = reportsExportAddon.Version,
                        ExportTypeFilesInfos = exportResultFilesInfoExs.Select(fi => fi.ExportTypeFilesInfo).ToList(),
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpPost("ExportMonitoringWidgetsReportToFile")]
        public async Task<IActionResult> ExportMonitoringWidgetsReportToFileAsync([FromBody] Filter filter, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            filter.User = (filter.User ?? @"").ToLowerInvariant(); // Normalize             
            filter.SearchString = filter.SearchString?.Trim();

            // Norm
            if (addonIdentifier is null)
                addonIdentifier = @""; // Normalize
            if (String.IsNullOrEmpty(destinationTypeIdentifier))
                destinationTypeIdentifier = PazCheckConstants.TypeIdentifier_HumanReadable_ExcelFile; // Normalize

            var roles = HttpContextHelper.GetRoles(HttpContext);
            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(nameof(PazCheckDbContext.PcObjectEvents), out PropertyInfo? pazCheckDbContext_PropertyInfo);
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Read");
            if (!checkAccessSucceeded)
                return Unauthorized();

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            Stream? stream = null;
            string? fileDownloadName = null;
            bool succeeded = false;
            try
            {
                IReportsExportAddon? reportsExportAddon = _addonsManager.AddonsThreadSafe
                        .OfType<IReportsExportAddon>()
                        .FirstOrDefault(a => String.Equals(((AddonBase)a).Identifier, addonIdentifier, StringComparison.InvariantCultureIgnoreCase));

                List<(string, byte[])> filesData = new();

                if (reportsExportAddon is not null)
                {
                    filesData.AddRange(await reportsExportAddon.ExportReportAsync(
                        _dbContextFactory,
                        _cache.DbCache,                        
                        destinationTypeIdentifier,
                        filter,
                        new CaseInsensitiveOrderedDictionary<string?>(),
                        _hostApplicationLifetime.ApplicationStopping,
                    loggersSet));

                    fileDownloadName = "ќтчет " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + @".zip";
                    stream = SerializationHelper.ExportZip(filesData);                    
                }                

                if (stream is null)
                    return NoContent();

                succeeded = true;

                var contentType = "APPLICATION/octet-stream";
                return File(stream, contentType, fileDownloadName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportMonitoringWidgetsReportToFile Error");
                return NoContent();
            }
            finally
            {
                string enitityTypeDesc = pazCheckDbContext_PropertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? entityType.Name;
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,
                                    InformationSecurityEventsLogger.DataExported_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Properties.Resources.ExportEventsToFile_Event,
                                    informationSecurityContext.User,
                                    enitityTypeDesc,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"ExportMonitoringWidgetsReportToFile"),
                                            (@"EntityType", entityType.Name),
                                            (@"AddonIdentifier", addonIdentifier),
                                            (@"DestinationTypeIdentifier", destinationTypeIdentifier)
                                        }),
                                    Properties.Resources.ExportEventsToFile_EventDesc, enitityTypeDesc);
            }
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpGet(@"GetExportMonitoringEventsReport_FilesInfo")]
        public Task<IActionResult> GetExportMonitoringEventsReport_FilesInfoAsync()
        {
            List<AddonExportFilesInfo> result =
            [
                new AddonExportFilesInfo
                {
                    AddonIdentifier = @"",
                    AddonDesc = @"",
                    AddonVersion = @"",
                    ExportTypeFilesInfos = new List<ExportTypeFilesInfo>()
                        {
                            new ExportTypeFilesInfo
                            {
                                DestinationTypeIdentifier = PazCheckConstants.TypeIdentifier_Std_ExcelFile,
                                DestinationTypeDesc = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile,
                                DestinationTypeDetails = Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Details,
                                ExportOptionInfos = new()
                            }
                        }
                },
            ];
            foreach (AddonBase reportsExportAddon in _addonsManager.AddonsThreadSafe.OfType<IReportsExportAddon>())
            {
                var exportResultFilesInfoExs = ((IReportsExportAddon)reportsExportAddon).GetExportTypeFilesInfoExs([ IReportsExportAddon.ReportCategory_MonitoringEvents_UpperCase ]);
                if (exportResultFilesInfoExs.Length > 0)
                {
                    result.Add(new AddonExportFilesInfo
                    {
                        AddonIdentifier = reportsExportAddon.Identifier,
                        AddonDesc = reportsExportAddon.Desc,
                        AddonVersion = reportsExportAddon.Version,
                        ExportTypeFilesInfos = exportResultFilesInfoExs.Select(fi => fi.ExportTypeFilesInfo).ToList(),
                    });
                }
            }
            return Task.FromResult<IActionResult>(Ok(new { results = result.ToArray() }));
        }

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
            )]
        [HttpPost("ExportMonitoringEventsReportToFile")]
        public async Task<IActionResult> ExportMonitoringEventsReportToFileAsync([FromBody] Filter filter, string addonIdentifier, string destinationTypeIdentifier, string optionsString)
        {
            filter.User = (filter.User ?? @"").ToLowerInvariant(); // Normalize             
            filter.SearchString = filter.SearchString?.Trim();

            // Norm
            if (addonIdentifier is null)
                addonIdentifier = @""; // Normalize
            if (String.IsNullOrEmpty(destinationTypeIdentifier))
                destinationTypeIdentifier = PazCheckConstants.TypeIdentifier_HumanReadable_ExcelFile; // Normalize

            var roles = HttpContextHelper.GetRoles(HttpContext);
            PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(nameof(PazCheckDbContext.PcObjectEvents), out PropertyInfo? pazCheckDbContext_PropertyInfo);
            if (pazCheckDbContext_PropertyInfo is null)
                return NotFound();
            Type entityType = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First();
            bool checkAccessSucceeded = _cache.CheckAccess(roles, "Entity." + entityType.Name + ".Read");
            if (!checkAccessSucceeded)
                return Unauthorized();

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
            using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
            await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);

            Stream? stream = null;
            string? fileDownloadName = null;
            bool succeeded = false;
            try
            {
                if (addonIdentifier == @"" &&
                    String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_HumanReadable_ExcelFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    (fileDownloadName, stream) = await SerializationHelper.ExportEventsToFileAsync(
                        _dbContextFactory,
                        _cache.DbCache,
                        entityType,
                        destinationTypeIdentifier,
                        filter);
                }
                else
                {
                    IReportsExportAddon? reportsExportAddon = _addonsManager.AddonsThreadSafe
                        .OfType<IReportsExportAddon>()
                        .FirstOrDefault(a => String.Equals(((AddonBase)a).Identifier, addonIdentifier, StringComparison.InvariantCultureIgnoreCase));

                    List<(string, byte[])> filesData = new();

                    if (reportsExportAddon is not null)
                    {
                        filesData.AddRange(await reportsExportAddon.ExportReportAsync(
                            _dbContextFactory,
                            _cache.DbCache,                            
                            destinationTypeIdentifier,
                            filter,
                            new CaseInsensitiveOrderedDictionary<string?>(),
                            _hostApplicationLifetime.ApplicationStopping,
                        loggersSet));                        

                        fileDownloadName = "ќтчет " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + @".zip";
                        stream = SerializationHelper.ExportZip(filesData);
                    }                        
                }

                if (stream is null)
                    return NoContent();

                succeeded = true;

                var contentType = "APPLICATION/octet-stream";
                return File(stream, contentType, fileDownloadName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportMonitoringEventsReportToFile Error");
                return NoContent();
            }
            finally
            {
                string enitityTypeDesc = pazCheckDbContext_PropertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? entityType.Name;
                _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,
                                    InformationSecurityEventsLogger.DataExported_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Properties.Resources.ExportEventsToFile_Event,
                                    informationSecurityContext.User,
                                    enitityTypeDesc,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"ExportMonitoringEventsReportToFile"),
                                            (@"EntityType", entityType.Name),
                                            (@"addonIdentifier", addonIdentifier),
                                            (@"destinationTypeIdentifier", destinationTypeIdentifier)
                                        }),
                                    Properties.Resources.ExportEventsToFile_EventDesc, enitityTypeDesc);
            }            
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
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        #endregion
    }
}


//[HttpPost("ExportCeMatrixToFile/{ceMatrixId}")]
//public async Task<IActionResult> ExportCeMatrixToFileAsync(int ceMatrixId, string addonIdentifier)
//{
//    var user = HttpContextHelper.GetUser(HttpContext)

//    if (String.IsNullOrEmpty(addonIdentifier))
//        addonIdentifier = @"CeMatrixRuntime";

//    CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateAddonThreadSafe<CeMatrixRuntimeAddonBase>();
//    if (ceMatrixRuntimeAddon is null)
//        return NoContent();

//    using var dbContext = _dbContextFactory.CreateDbContext();
//    CeMatrix ceMatrix;
//    try
//    {
//        ceMatrix = dbContext.CeMatrices.Single(m => m.Id == ceMatrixId);
//    }
//    catch
//    {
//        _logger.LogError("Invalid ceMatrixId: {0}", ceMatrixId);
//        return NoContent();
//    }

//    string? csvString = await ceMatrixRuntimeAddon.GetCeMatrixStringAsync(dbContext, ceMatrix);
//    if (csvString is null)
//        return NoContent();

//    var fileStream = new MemoryStream(new UTF8Encoding(true).GetBytes(csvString));
//    var contentType = "APPLICATION/octet-stream";
//    var fileDownloadName = ceMatrix.Title + ".csv";
//    return File(fileStream, contentType, fileDownloadName);
//}