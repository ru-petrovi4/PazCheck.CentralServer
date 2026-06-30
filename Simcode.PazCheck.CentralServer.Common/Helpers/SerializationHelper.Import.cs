using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Logging;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Linq.Dynamic.Core;
using Ssz.Utils.Addons;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClosedXML.Excel;
using System.Net.Http;
using System.Net.Mime;
using Npgsql.PostgresTypes;
using Microsoft.Extensions.DependencyInjection;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Newtonsoft.Json.Bson;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net;
using System.Globalization;
using Ssz.Utils.ClosedXML;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class SerializationHelper
    {
        #region public functions

        public static List<StreamWithInfo> GetStreamWithInfoList(string path, string fileName, Stream stream)
        {
            List<StreamWithInfo> result = new();

            if (fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase) ||
                        fileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
            {                
                result.Add(new StreamWithInfo
                {
                    Path = path,
                    FileName = fileName,
                    Stream = stream,
                    IsStdFormat = IsStdFormat(stream)
                });
            }
            else if (fileName.EndsWith(".xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                if (path == @"")
                    path = fileName;
                else
                    path += "->" + fileName;

                using (var workbook = new XLWorkbook(stream))
                {
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var csvMemoryStream = GetCsvMemoryStream(worksheet);                        

                        result.Add(new StreamWithInfo
                        {
                            Path = path,
                            FileName = worksheet.Name + @".csv",
                            Stream = csvMemoryStream,
                            IsStdFormat = IsStdFormat(csvMemoryStream)
                        });
                    }
                }
            }
            else if (fileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                if (path == @"")
                    path = fileName;
                else
                    path += "->" + fileName;

                using (var zipArchive = ZipArchiveHelper.GetZipArchiveForRead(stream))
                {
                    var entries = zipArchive.Entries
                        .OrderByDescending(e => e.Name.EndsWith(@".json", StringComparison.InvariantCultureIgnoreCase))
                        .ThenBy(e => e.Name)
                        .ToArray();                    
                    foreach (var entry in entries)
                    {
                        if (String.IsNullOrEmpty(entry.Name))
                            continue;                        

                        MemoryStream entryMemoryStream = new();                        
                        using (var entryStream = entry.Open())
                        {
                            entryStream.CopyTo(entryMemoryStream);
                        }
                        entryMemoryStream.Position = 0;
                        var childStreamWithInfoList = GetStreamWithInfoList(path, entry.Name, entryMemoryStream);
                        if (!childStreamWithInfoList.Any(i => ReferenceEquals(i.Stream, entryMemoryStream)))
                            entryMemoryStream.Dispose();

                        result.AddRange(childStreamWithInfoList);
                    }
                }
            }
            else
            {
                result.Add(new StreamWithInfo
                {
                    Path = path,
                    FileName = fileName,
                    Stream = stream,
                    IsStdFormat = false
                });
            }

            return result;
        }

        public static MemoryStream GetCsvMemoryStream(IXLWorksheet worksheet)
        {
            List<string?[]> values = new();
            IXLRange? usedRange = worksheet.RangeUsed();
            if (usedRange is not null)
            {
                var usedRange_FirstAddress = usedRange.RangeAddress.FirstAddress;
                var usedRange_LastAddress = usedRange.RangeAddress.LastAddress;
                int rowsCount = usedRange.RowCount();                
                int columnsCount = usedRange.ColumnCount();
                foreach (int _ in Enumerable.Range(0, usedRange_FirstAddress.RowNumber - 1))
                {
                    var rowValues = new string?[usedRange_FirstAddress.ColumnNumber + columnsCount - 1];
                    values.Add(rowValues);
                }
                foreach (int row in Enumerable.Range(usedRange_FirstAddress.RowNumber, rowsCount))
                {
                    var rowValues = new string?[usedRange_FirstAddress.ColumnNumber + columnsCount - 1];
                    values.Add(rowValues);

                    foreach (int column in Enumerable.Range(usedRange_FirstAddress.ColumnNumber, columnsCount))
                    {
                        var cell = worksheet.Cell(row, column);
                        rowValues[column - 1] = ExcelHelper.GetCellValueForCsv(cell);                        
                    }                    
                }

                foreach (var mergedRange in worksheet.MergedRanges)
                {
                    var firstAddress = mergedRange.RangeAddress.FirstAddress;
                    var lastAddress = mergedRange.RangeAddress.LastAddress;
                    int row = firstAddress.RowNumber;
                    for (int column = firstAddress.ColumnNumber + 1; column <= Math.Min(lastAddress.ColumnNumber, usedRange_LastAddress.ColumnNumber); column += 1)
                    {                        
                        values[row - 1][column - 1] = @"--";
                    }
                    for (row = firstAddress.RowNumber + 1; row <= Math.Min(lastAddress.RowNumber, usedRange_LastAddress.RowNumber); row += 1)
                        for (int column = firstAddress.ColumnNumber; column <= Math.Min(lastAddress.ColumnNumber, usedRange_LastAddress.ColumnNumber); column += 1)
                        {
                            values[row - 1][column - 1] = @"-";
                        }
                }
            }

            MemoryStream memoryStream = new();
            StreamWriter sw = new StreamWriter(memoryStream, new UTF8Encoding(true)); // Does not close stream
            foreach (var row in values)
            {   
                sw.WriteLine(CsvHelper.FormatForCsv(@",", row));
            }                
            sw.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        ///     Вызывается для каждого выбранного пользователем файла.
        ///     По завершению jobProgress должен стать 100% или перейти в статус ошибки.
        ///     Precondition: Main thread.
        /// </summary>
        /// <param name="dbContextFactory"></param>       
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="sourceTypeIdentifier"></param>
        /// <param name="entityType"></param>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="jobProgress"></param>
        /// <param name="informationSecurityContext"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="informationSecurityEventsLogger"></param>
        /// <param name="loggersSet"></param>
        /// <param name="preview"></param>
        /// <returns></returns>
        public static async Task<Serialization.ImportSerializationRootObjectResult> ImportFileAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,             
            Stream stream,
            string fileName,            
            string addonIdentifier, 
            string sourceTypeIdentifier,
            Type? entityType,
            int? projectId,            
            CancellationToken cancellationToken,
            IJobProgress jobProgress,            
            InformationSecurityContext informationSecurityContext,
            IMainServerWorker mainServerWorker,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILoggersSet loggersSet,
            bool preview)
        {
            Serialization.ImportSerializationRootObjectResult result = new();

            await using PazCheckDbContext metaParams_DbContext = dbContextFactory.CreateDbContext();
            metaParams_DbContext.User = informationSecurityContext.User;
            metaParams_DbContext.IsInformationSecurityEventsLoggingDisabled = true;

            var metaParams = metaParams_DbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);

            if (projectId is not null)
            {                
                PazCheckDbHelper.AddOrUpdateMetaParam_Pause_HubMethod_Project_Changed(
                        metaParams_DbContext,
                        metaParams,
                        projectId.Value,
                        isPaused: true);
                await metaParams_DbContext.SaveChangesAsync();
            }

            try
            {
                await using PazCheckDbContext dbContext = dbContextFactory.CreateDbContext();
                dbContext.User = informationSecurityContext.User;
                dbContext.IsInformationSecurityEventsLoggingDisabled = true;

                if (String.Equals(sourceTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_File, StringComparison.InvariantCultureIgnoreCase))
                {
                    var streamWithInfoList = GetStreamWithInfoList(@"", fileName, stream);

                    if (streamWithInfoList.Count == 0)
                    {
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_FileHasErrors);
                        await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    }
                    else
                    {
                        bool allFailed = true;

                        int i = 0;
                        foreach (var streamWithInfo in streamWithInfoList)
                        {
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(new (string, object?)[]
                            {
                                (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                            });
                            using var scope2 = informationSecurityEventsLogger.BeginScope(new (string, object?)[]
                            {
                                (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                            });

                            var childJobProgress = await jobProgress.GetChildJobProgressAsync((uint)(100.0 * i / streamWithInfoList.Count), (uint)(100.0 * (i + 1) / streamWithInfoList.Count), parentFailedIfFailed: false);

                            await ImportStdFileAsync(
                                    dbContextFactory,
                                    informationSecurityContext.User,
                                    streamWithInfo.Stream,
                                    projectId,
                                    cancellationToken,
                                    childJobProgress,
                                    mainServerWorker,
                                    loggersSet,
                                    result,
                                    preview);

                            if (Ssz.Utils.StatusCodes.IsGood(childJobProgress.StatusCode))
                                allFailed = false;

                            if (!ReferenceEquals(streamWithInfo.Stream, stream))
                                streamWithInfo.Stream.Dispose();

                            i += 1;
                        }

                        if (allFailed)
                        {                            
                            await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                        }
                    }
                }
                else
                {
                    if (entityType is null)
                    {   
                        await jobProgress.SetJobProgressAsync(null, Properties.Resources.Error, Properties.Resources.Error_FileHasErrors, Ssz.Utils.StatusCodes.BadInvalidArgument);                        
                        return result;
                    }

                    AddonBase? addon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().CreateInitializedAddonThreadSafe<AddonBase>(null, CancellationToken.None, addonIdentifier);                    
                    if (addon is null)
                    {
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.InvalidAddonIdentifier, addonIdentifier);
                        await jobProgress.SetJobProgressAsync(null, Properties.Resources.Error, String.Format(Properties.Resources.InvalidAddonIdentifier, addonIdentifier), Ssz.Utils.StatusCodes.BadInvalidState);
                        return result;
                    }

                    var part0JobProgress = await jobProgress.GetChildJobProgressAsync(0, 20, parentFailedIfFailed: true);

                    Serialization.SerializationRootObject serializationRootObject = new();

                    var streamWithInfoList = GetStreamWithInfoList(@"", fileName, stream);

                    if (streamWithInfoList.Count == 0)
                    {
                        await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    }
                    else
                    {

                        bool allFailed = true;

                        int i = 0;
                        foreach (var streamWithInfo in streamWithInfoList)
                        {
                            using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(new (string, object?)[]
                            {
                                (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                            });
                            using var scope2 = informationSecurityEventsLogger.BeginScope(new (string, object?)[]
                            {
                                (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                            });

                            var childJobProgress = await part0JobProgress.GetChildJobProgressAsync((uint)(100.0 * i / streamWithInfoList.Count), (uint)(100.0 * (i + 1) / streamWithInfoList.Count), parentFailedIfFailed: false);

                            await ImportAddonFileAsync(
                                dbContextFactory,
                                informationSecurityContext.User,
                                entityType,
                                addon,
                                sourceTypeIdentifier,
                                streamWithInfo.FileName,
                                streamWithInfo.Stream,
                                projectId,                                
                                cancellationToken,
                                serializationRootObject,
                                childJobProgress,
                                mainServerWorker,
                                loggersSet,
                                result,
                                preview);

                            if (Ssz.Utils.StatusCodes.IsGood(childJobProgress.StatusCode))
                                allFailed = false;

                            if (!ReferenceEquals(streamWithInfo.Stream, stream))
                                streamWithInfo.Stream.Dispose();

                            i += 1;
                        }

                        if (allFailed)
                        {
                            await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                        }
                        else
                        {
                            var part1JobProgress = await jobProgress.GetChildJobProgressAsync(20, 90, parentFailedIfFailed: true);

                            await ImportSerializationRootObjectAsync(
                                serializationRootObject,
                                new Common.Serialization.ImportMetadata
                                {
                                    RootCollectionMode = Common.Serialization.CollectionMode.Update,
                                    ChildCollectionMode = Common.Serialization.CollectionMode.Update,
                                    DataCollectionMode = Common.Serialization.CollectionMode.Update,
                                },
                                dbContextFactory,
                                informationSecurityContext.User,
                                projectId,
                                cancellationToken,
                                part1JobProgress,
                                loggersSet,
                                result,
                                preview);
                        }
                    }
                }
            }            
            finally
            {
                if (projectId is not null)
                {
                    PazCheckDbHelper.AddOrUpdateMetaParam_Pause_HubMethod_Project_Changed(
                        metaParams_DbContext,
                        metaParams,
                        projectId.Value,
                        isPaused: false);
                    await metaParams_DbContext.SaveChangesAsync();
                }

                if (informationSecurityEventsLogger is not null)
                {
                    if (preview)
                    {
                        Common.FileContentType contentType =
                            new() { Id = @"Objects", Desc = Common.Properties.Resources.Objects_ContentType };
                        informationSecurityEventsLogger.InformationSecurityEvent(
                            informationSecurityContext.User,
                            informationSecurityContext.SourceIpAddress,
                            informationSecurityContext.SourceHost,
                            InformationSecurityEventsLogger.DataImportedPreview_AllRolesAccessEventId,
                            6,
                            Ssz.Utils.StatusCodes.IsGood(jobProgress.StatusCode),
                            Properties.Resources.ImportFilePreview_EventName,
                            informationSecurityContext.User,
                            fileName,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                            {
                            (@"ContentType", contentType.Id)
                            }),
                            Properties.Resources.ImportFilePreview_EventDesc, fileName, SerializationHelper.RemoveHtml(jobProgress.ProgressDetails));
                    }
                    else
                    {
                        Common.FileContentType contentType =
                            new() { Id = @"Objects", Desc = Common.Properties.Resources.Objects_ContentType };
                        informationSecurityEventsLogger.InformationSecurityEvent(
                            informationSecurityContext.User,
                            informationSecurityContext.SourceIpAddress,
                            informationSecurityContext.SourceHost,
                            InformationSecurityEventsLogger.DataImported_AllRolesAccessEventId,
                            6,
                            Ssz.Utils.StatusCodes.IsGood(jobProgress.StatusCode),
                            Properties.Resources.ImportFile_EventName,
                            informationSecurityContext.User,
                            fileName,
                            NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                            {
                            (@"ContentType", contentType.Id)
                            }),
                            Properties.Resources.ImportFile_EventDesc, fileName, SerializationHelper.RemoveHtml(jobProgress.ProgressDetails));
                    }
                }
            }

            return result;
        }

        public static async Task<int> ImportEventsJournalFileAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            string user,
            Stream stream,
            string fileName,
            string addonIdentifier,
            string sourceTypeIdentifier,            
            int unitId,
            CancellationToken cancellationToken,
            IJobProgress jobProgress,            
            IMainServerWorker mainServerWorker,            
            ILoggersSet loggersSet,
            bool preview)
        {
            await using PazCheckDbContext dbContext = dbContextFactory.CreateDbContext();
            dbContext.User = user;
            dbContext.IsInformationSecurityEventsLoggingDisabled = true;

            EventMessagesProcessingAddonBase? eventMessagesProcessingAddon = null;
            if (!String.IsNullOrEmpty(addonIdentifier))
            {
                eventMessagesProcessingAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().CreateInitializedAddonThreadSafe<EventMessagesProcessingAddonBase>(null, CancellationToken.None, addonIdentifier);
                if (eventMessagesProcessingAddon is null)
                {
                    loggersSet.Logger.LogError("Invalid addonIdentifier: {0}", addonIdentifier);
                    await jobProgress.SetJobProgressAsync(0, Properties.Resources.Error, String.Format(Properties.Resources.InvalidAddonIdentifier, addonIdentifier), Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return 0;
                }
            }            
            
            Common.EntityFramework.Unit unit;
            try
            {
                unit = dbContext.Units.Single(u => u.Id == unitId);
            }
            catch
            {
                loggersSet.Logger.LogError("Invalid unitId: {0}", unitId);
                await jobProgress.SetJobProgressAsync(0, Properties.Resources.Error, String.Format("Invalid unitId: {0}", unitId), Ssz.Utils.StatusCodes.BadInvalidArgument);
                return 0;
            }

            int addedEventsCount = 0;

            var streamWithInfoList = GetStreamWithInfoList(@"", fileName, stream);

            bool allFailed = true;

            int i = 0;
            foreach (var streamWithInfo in streamWithInfoList)
            {
                using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(new (string, object?)[]
                {
                        (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.Path)
                });                

                var childJobProgress = await jobProgress.GetChildJobProgressAsync((uint)(100.0 * i / streamWithInfoList.Count), (uint)(100.0 * (i + 1) / streamWithInfoList.Count), parentFailedIfFailed: false);

                try
                {   
                    UnitEventsInterval? unitEventsInterval;
                    if (!streamWithInfo.IsStdFormat && eventMessagesProcessingAddon is not null)
                    {
                        unitEventsInterval = await eventMessagesProcessingAddon.ImportEventsJournalFileAsync(sourceTypeIdentifier, streamWithInfo.Stream, streamWithInfo.FileName, dbContext, unit, cancellationToken, loggersSet,
                            childJobProgress, preview);
                    }
                    else
                    {
                        unitEventsInterval = await ImportStdEventsJournalFileAsync(streamWithInfo.Stream, streamWithInfo.FileName, dbContext, unit, cancellationToken, loggersSet,
                            childJobProgress, preview);
                    }

                    if (unitEventsInterval is not null)
                        addedEventsCount += unitEventsInterval.UnitEvents.Count;
                }
                catch (Exception ex)
                {
                    await jobProgress.SetJobProgressAsync(100, Properties.Resources.Error, Common.Properties.Resources.Error_FileInvalidFormat, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    loggersSet.Logger.LogError(ex, "ImportEventsJournalFiles Error");
                }

                if (Ssz.Utils.StatusCodes.IsGood(childJobProgress.StatusCode))
                    allFailed = false;                

                if (!ReferenceEquals(streamWithInfo.Stream, stream))
                    streamWithInfo.Stream.Dispose();

                i += 1;
            }

            if (allFailed)
                await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);

            return addedEventsCount;
        }

        public static async Task ImportLicenseFileAsync(
            PazCheckDbContext dbContext,
            Stream stream,
            string fileName,
            string addonIdentifier,
            string sourceTypeIdentifier,            
            CancellationToken cancellationToken,
            IJobProgress jobProgress,
            InformationSecurityContext? informationSecurityContext,
            IMainServerWorker mainServerWorker,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILoggersSet loggersSet,
            bool preview)
        {
            string user = informationSecurityContext?.User ?? @"";
            
            try
            {
                var streamWithInfoList = GetStreamWithInfoList(@"", fileName, stream);

                int i = 0;
                foreach (var streamWithInfo in streamWithInfoList)
                {
                    using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(new (string, object?)[]
                    {
                        (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                    });
                    using var scope2 = informationSecurityEventsLogger.BeginScope(new (string, object?)[]
                    {
                        (Ssz.Utils.Properties.Resources.FileNameScopeName, streamWithInfo.GetFilePathAndName())
                    });

                    try
                    {
                        await ImportLicenseFileAsync(streamWithInfo.Stream, streamWithInfo.FileName, user, dbContext, loggersSet);
                    }
                    catch (Exception ex)
                    {
                        loggersSet.Logger.LogError(ex, "ImportLicenseFile Error");
                    }                    

                    if (!ReferenceEquals(streamWithInfo.Stream, stream))
                        streamWithInfo.Stream.Dispose();

                    i += 1;
                }                        
            }
            finally
            {
                if (!preview)
                {
                    Common.FileContentType contentType =
                        new() { Id = @"LicenseFile", Desc = Common.Properties.Resources.LicenseFile_ContentType };
                    if (informationSecurityContext is not null)
                        informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
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

        /// <summary>        
        ///     По завершению jobProgress должен стать 100% или перейти в статус ошибки, если нужно прекратить всю комплексную операцию. 
        /// </summary>
        /// <param name="serializationRootObject"></param>
        /// <param name="importMetadata"></param>
        /// <param name="dbContextFactory"></param>
        /// <param name="user"></param>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="jobProgress"></param>        
        /// <param name="loggersSet"></param>
        /// <param name="result"></param>
        /// <param name="preview"></param>
        /// <returns></returns>
        public static async Task ImportSerializationRootObjectAsync(
            Common.Serialization.SerializationRootObject serializationRootObject, 
            Common.Serialization.ImportMetadata importMetadata,             
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            string user,
            int? projectId,             
            CancellationToken cancellationToken, 
            IJobProgress jobProgress,            
            ILoggersSet loggersSet,
            Serialization.ImportSerializationRootObjectResult result,
            bool preview)
        {
            await using PazCheckDbContext dbContext = dbContextFactory.CreateDbContext();
            dbContext.IsInformationSecurityEventsLoggingDisabled = true;
            dbContext.User = user;

            IDbContextTransaction? dbContextTransaction;
            if (preview)
                dbContextTransaction = null;
            else
                dbContextTransaction = dbContext.Database.BeginTransaction();            

            try
            {
                Project? project = null;

                if (projectId is not null)
                {
                    project = dbContext.Projects
                        .Include(p => p.Unit)
                        .FirstOrDefault(p => p.Id == projectId.Value);

                    serializationRootObject.BasePcObjects = null; // skip
                    serializationRootObject.PcObjects = null; // skip
                }
                else
                {                    
                    serializationRootObject.CeMatrices = null; // skip
                    serializationRootObject.Tags = null; // skip
                    serializationRootObject.Actuators = null; // skip
                    serializationRootObject.MonitoringObjects = null; // skip
                    serializationRootObject.Legends = null; // skip
                }

                int projectEntitiesN = 0;
                int projectEntitiesCount = (serializationRootObject.CeMatrices?.Count ?? 0) +
                    (serializationRootObject.Actuators?.Count ?? 0) +
                    (serializationRootObject.Tags?.Count ?? 0) +
                    (serializationRootObject.MonitoringObjects?.Count ?? 0) +
                    (serializationRootObject.Legends?.Count ?? 0);

                ReferenceEntities referenceEntities = new();

                bool changed = false;                

                // DbFile
                referenceEntities.DbFiles = await dbContext.DbFiles.ToDictionaryAsync(dbf => dbf.FileBytesHash_Base64);
                if (serializationRootObject.DbFiles is not null)
                    foreach (var serializationDbFile in serializationRootObject.DbFiles)
                    {                        
                        referenceEntities.DbFiles.TryGetValue(serializationDbFile.FileBytesHash_Base64, out DbFile? dbFile);
                        if (dbFile is null)
                        {
                            dbFile = new DbFile
                            {
                                OriginalFileName = serializationDbFile.OriginalFileName,                                
                                FileBytesCount = serializationDbFile.FileBytesCount,
                                FileBytesHash_Base64 = serializationDbFile.FileBytesHash_Base64,
                                DbFileContent = new DbFileContent { FileBytes_Base64 = serializationDbFile.FileBytes_Base64 }
                            };
                            dbContext.DbFiles.Add(dbFile);
                            referenceEntities.DbFiles.Add(dbFile.FileBytesHash_Base64, dbFile);
                            changed = true;
                        }                        
                    }                

                // ParamDesc            
                referenceEntities.ParamDescs = await dbContext.ParamDescs.ToDictionaryAsync(pd => pd.Id);
                if (serializationRootObject.ParamDescs is not null)
                    foreach (var serializationParamDesc in serializationRootObject.ParamDescs)
                    {
                        referenceEntities.ParamDescs.TryGetValue(serializationParamDesc.Name, out ParamDesc? paramDesc);
                        if (paramDesc is null)
                        {
                            paramDesc = new()
                            {
                                Id = serializationParamDesc.Name,
                                Desc = serializationParamDesc.Desc,
                                Details = serializationParamDesc.Details,
                                Priority = serializationParamDesc.Priority,
                                DataType = serializationParamDesc.DataType,
                                MetadataFields = serializationParamDesc.MetadataFields
                            };
                            dbContext.ParamDescs.Add(paramDesc);
                            referenceEntities.ParamDescs.Add(paramDesc.Id, paramDesc);
                            changed = true;
                        }
                        else
                        {
                            if (paramDesc.Desc != serializationParamDesc.Desc || paramDesc.Priority != serializationParamDesc.Priority)
                            {
                                paramDesc.Desc = serializationParamDesc.Desc;
                                paramDesc.Details = serializationParamDesc.Details;
                                paramDesc.Priority = serializationParamDesc.Priority;
                                paramDesc.DataType = serializationParamDesc.DataType;
                                paramDesc.MetadataFields = serializationParamDesc.MetadataFields;
                                changed = true;
                            }
                        }
                    }                

                // ProjectVersionType
                referenceEntities.ProjectVersionTypes = await dbContext.ProjectVersionTypes
                    .Include(t => t.StandardParamInfos)
                    .ToDictionaryAsync(t => t.Type, StringComparer.InvariantCultureIgnoreCase);
                if (AddTypes(referenceEntities.ProjectVersionTypes, dbContext.ProjectVersionTypes, serializationRootObject.ProjectVersionTypes, referenceEntities, loggersSet))
                    changed = true;

                // PcObjectEventType
                referenceEntities.PcObjectEventTypes = await dbContext.PcObjectEventTypes
                    .Include(t => t.StandardParamInfos)
                    .ToDictionaryAsync(t => t.Type, StringComparer.InvariantCultureIgnoreCase);
                if (AddTypes(referenceEntities.PcObjectEventTypes, dbContext.PcObjectEventTypes, serializationRootObject.PcObjectEventTypes, referenceEntities, loggersSet))
                    changed = true;

                await jobProgress.SetJobProgressAsync((uint)(100.0 * projectEntitiesN / projectEntitiesCount), Properties.Resources.ImportSafetyControllers_ProgressLabel, null, Ssz.Utils.StatusCodes.Good);                

                if (serializationRootObject.Legends is not null)
                {
                    if (result.Legends_ImportSerializationResult is null)
                    {
                        result.Legends_ImportSerializationResult = new()
                        {
                            All = await dbContext.Legends
                                .Include(sc => sc.LegendParams.Where(e => e._IsDeleted == false))
                                .Where(GetAllPredicate<Legend>(project!))
                                .ToDictionaryAsync(ba => ba.Identifier, StringComparer.InvariantCultureIgnoreCase),                            
                        };
                    }

                    ImportProjectVersionedEntities<Serialization.Legend, Legend>(
                        dbContext,
                        project!,
                        dbContext.Legends,                        
                        serializationRootObject.Legends,
                        result.Legends_ImportSerializationResult,
                        importMetadata,
                        (serializationEntity, entity) =>
                        {
                            UpdateParams(dbContext, entity.LegendParams, serializationEntity.Params, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                        },
                        PazCheckDbHelper.SetIsDeleted_Legend,
                        ref projectEntitiesN,
                        Properties.Resources.Legend,
                        loggersSet);                    

                    changed = true;
                }

                if (serializationRootObject.MonitoringObjects is not null)
                {
                    if (result.SafetyControllers_ImportSerializationResult is null)
                    {
                        result.SafetyControllers_ImportSerializationResult = new()
                        {                            
                            All = await dbContext.SafetyControllers
                                    .Include(sc => sc.SafetyControllerParams.Where(e => e._IsDeleted == false))
                                    .Include(sc => sc.SafetyControllerDbFileReferences.Where(e => e._IsDeleted == false))
                                    .Where(GetAllPredicate<SafetyController>(project!))
                                    .ToDictionaryAsync(ba => ba.Identifier, StringComparer.InvariantCultureIgnoreCase)
                        };
                    }                    

                    ImportProjectVersionedEntities<Serialization.MonitoringObject, SafetyController>(
                        dbContext,
                        project!,
                        dbContext.SafetyControllers,
                        serializationRootObject.MonitoringObjects,
                        result.SafetyControllers_ImportSerializationResult,
                        importMetadata,
                        (serializationEntity, entity) =>
                        {
                            UpdateParams(dbContext, entity.SafetyControllerParams, serializationEntity.Params, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);                            
                            UpdateDbFileReferences(dbContext, entity.SafetyControllerDbFileReferences, serializationEntity.DbFileReferences, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                        },
                        PazCheckDbHelper.SetIsDeleted_SafetyController,
                        ref projectEntitiesN,
                        Properties.Resources.SafetyController,
                        loggersSet);

                    await CheckMonitoringEntitiesAsync(
                        dbContextFactory,
                        result.SafetyControllers_ImportSerializationResult,
                        project!.Unit.Identifier,
                        referenceEntities,
                        loggersSet);

                    changed = true;
                }                

                if (serializationRootObject.Actuators is not null)
                {
                    if (result.BaseActuators_ImportSerializationResult is null)
                    {
                        result.BaseActuators_ImportSerializationResult = new()
                        {                            
                            All = await dbContext.BaseActuators
                                .Include(ba => ba.BaseActuatorParams.Where(e => e._IsDeleted == false))
                                .Include(ba => ba.BaseActuatorDbFileReferences.Where(e => e._IsDeleted == false))
                                .Where(GetAllPredicate<BaseActuator>(project!))
                                .ToDictionaryAsync(ba => ba.Identifier, StringComparer.InvariantCultureIgnoreCase)
                        };
                    }                    

                    ImportProjectVersionedEntities<Serialization.Actuator, BaseActuator>(
                        dbContext,
                        project!,
                        dbContext.BaseActuators,
                        serializationRootObject.Actuators,
                        result.BaseActuators_ImportSerializationResult,
                        importMetadata,
                        (serializationEntity, entity) =>
                        {
                            UpdateParams(dbContext, entity.BaseActuatorParams, serializationEntity.Params, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                            UpdateDbFileReferences(dbContext, entity.BaseActuatorDbFileReferences, serializationEntity.DbFileReferences, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                        },
                        PazCheckDbHelper.SetIsDeleted_BaseActuator,
                        ref projectEntitiesN,
                        Properties.Resources.BaseActuator,
                        loggersSet);

                    changed = true;
                }                

                if (serializationRootObject.Tags is not null)
                {
                    if (result.Tags_ImportSerializationResult is null)
                    {
                        result.Tags_ImportSerializationResult = new()
                        {                            
                            All = await dbContext.Tags
                                .Include(t => t.TagParams.Where(e => e._IsDeleted == false))
                                .Include(t => t.TagConditions.Where(e => e._IsDeleted == false))
                                .Include(t => t.TagDbFileReferences.Where(e => e._IsDeleted == false))
                                .Where(GetAllPredicate<Tag>(project!))
                                .ToDictionaryAsync(ba => ba.Identifier, StringComparer.InvariantCultureIgnoreCase)
                        };
                    }                    

                    ImportProjectVersionedEntities<Serialization.Tag, Tag>(
                        dbContext,
                        project!,
                        dbContext.Tags,
                        serializationRootObject.Tags,
                        result.Tags_ImportSerializationResult,
                        importMetadata,
                        (serializationEntity, entity) =>
                        {
                            UpdateParams(dbContext, entity.TagParams, serializationEntity.Params, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                            UpdateTagConditions(dbContext, entity.TagConditions, serializationEntity.TagConditions, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                            UpdateDbFileReferences(dbContext, entity.TagDbFileReferences, serializationEntity.DbFileReferences, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                        },
                        PazCheckDbHelper.SetIsDeleted_Tag,
                        ref projectEntitiesN,
                        Properties.Resources.Tag,
                        loggersSet);

                    changed = true;
                }

                await jobProgress.SetJobProgressAsync((uint)(100.0 * projectEntitiesN / projectEntitiesCount), Properties.Resources.ImportTags_ProgressLabel, null, Ssz.Utils.StatusCodes.Good);

                if (serializationRootObject.CeMatrices is not null)
                {
                    if (result.CeMatrices_ImportSerializationResult is null)
                    {
                        result.CeMatrices_ImportSerializationResult = new()
                        {   
                            All = await dbContext.CeMatrices
                                .Include(m => m.CeMatrixParams.Where(e => e._IsDeleted == false))
                                .Include(t => t.CeMatrixDbFileReferences.Where(e => e._IsDeleted == false))
                                .Where(GetAllPredicate<CeMatrix>(project!))
                                .ToDictionaryAsync(ba => ba.Identifier, StringComparer.InvariantCultureIgnoreCase)
                        };
                    }                    

                    ImportProjectVersionedEntities<Serialization.CeMatrix, CeMatrix>(
                        dbContext,
                        project!,
                        dbContext.CeMatrices,
                        serializationRootObject.CeMatrices,
                        result.CeMatrices_ImportSerializationResult,
                        importMetadata,
                        (serializationEntity, entity) =>
                        {
                            UpdateParams(dbContext, entity.CeMatrixParams, serializationEntity.Params, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                            UpdateDbFileReferences(dbContext, entity.CeMatrixDbFileReferences, serializationEntity.DbFileReferences, referenceEntities, importMetadata.ChildCollectionMode, loggersSet);
                        },
                        PazCheckDbHelper.SetIsDeleted_CeMatrix,
                        ref projectEntitiesN,
                        Properties.Resources.CeMatrix,
                        loggersSet);

                    changed = true;
                }

                await jobProgress.SetJobProgressAsync((uint)(100.0 * projectEntitiesN / projectEntitiesCount), Properties.Resources.ImportCeMatrices_ProgressLabel, null, Ssz.Utils.StatusCodes.Good);

                if (serializationRootObject.BasePcObjects is not null ||
                        serializationRootObject.PcObjects is not null)
                {
                    if (result.BasePcObjects_ImportSerializationResult is null)
                    {
                        result.BasePcObjects_ImportSerializationResult = new()
                        {
                            All = (await dbContext.BasePcObjects
                                .Include(bpo => bpo.Unit)
                                .Include(bpo => bpo.JournalParams)
                                .Include(bpo => bpo.PcObjectEventTypes)
                                .ToListAsync())
                                .GroupBy(bpo => bpo.Unit.IdentifierLower + "." + bpo.IdentifierLower)
                                .ToDictionary(
                                    it => it.Key,
                                    it => it.OrderByDescending(bpo => bpo._IsDeleted).ThenBy(bpo => bpo._LastChangeTimeUtc).ToList(),
                                    StringComparer.InvariantCultureIgnoreCase)
                        };
                    }

                    if (result.PcObjects_ImportSerializationResult is null)
                    {
                        result.PcObjects_ImportSerializationResult = new()
                        {
                            All = (await dbContext.PcObjects
                                .Include(po => po.Unit)
                                .Include(po => po.BasePcObject)
                                .Include(po => po.Parent)
                                .Include(po => po.JournalParams)                                
                                .ToListAsync())
                                .GroupBy(po => po.Unit.IdentifierLower + "." + po.IdentifierLower)
                                .ToDictionary(
                                    it => it.Key,
                                    it => it.OrderByDescending(po => po._IsDeleted).ThenBy(po => po._LastChangeTimeUtc).ToList(),
                                    StringComparer.InvariantCultureIgnoreCase)
                        };                        
                    }

                    await ImportMonitoringEntitiesAsync(
                        dbContext,                                                
                        serializationRootObject,
                        referenceEntities,
                        result,
                        importMetadata,             
                        loggersSet);                    

                    changed = true;
                }                                  

                if (changed && !preview)
                    await dbContext.SaveChangesAsync();
                
                if (loggersSet.UserFriendlyLogger.GetStatistics(LogLevel.Error) > 0)
                    throw new Exception(@"Import operation has errors.");

                if (preview)
                {
                    await jobProgress.SetJobProgressAsync(100, null, result.GetLocalizedInfo_Html(), Ssz.Utils.StatusCodes.Good);
                }
                else
                {
                    await dbContextTransaction!.CommitAsync();

                    if (project is not null)
                        await PazCheckDbHelper.RemoveLockedByUserOnProjectVersionedEntitiesAsync(dbContext, project.Id, dbContext.User);

                    await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.Good);
                }   
            }
            catch (Exception ex)
            {
                if (!preview)
                    await dbContextTransaction!.RollbackAsync();

                loggersSet.Logger.LogError(ex, "Import error.");
                await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidState);
            }
            finally
            {
                dbContextTransaction?.Dispose();                
            }
        }        

        public static System.Linq.Expressions.Expression<Func<TProjectVersionEntityBase, bool>> GetAllPredicate<TProjectVersionEntityBase>(Project project)
            where TProjectVersionEntityBase : ProjectVersionedEntityBase
        {
            return m => m.Project == project && m._IsDeleted == false;
        }

        public static bool IsStdFormat(Stream stream)
        {
            var streamReader = new StreamReader(stream, Encoding.UTF8, true); // do not use 'using'

            bool isStdFormat;
            var firstLine = streamReader.ReadLine();
            var firstLineValues = Ssz.Utils.CsvHelper.ParseCsvLine(@",", firstLine);
            if (firstLineValues.Length < 4 || !String.Equals(firstLineValues[0], PazCheckConstants.ContentDirective_FileFormat, StringComparison.InvariantCultureIgnoreCase) ||
                    !String.Equals(firstLineValues[2], PazCheckConstants.ContentDirective_FormatVersion, StringComparison.InvariantCultureIgnoreCase))
                isStdFormat = false;
            else
                isStdFormat = true;

            stream.Position = 0;
            return isStdFormat;
        }

        #endregion

        #region private functions

        private static void ImportProjectVersionedEntities<TSerializationEntity, TEntity>(
                PazCheckDbContext dbContext,
                Project project,
                DbSet<TEntity> entities,
                List<TSerializationEntity> serializationEntities, 
                Serialization.ImportSerializationResult<TEntity> entities_ImportSerializationResult, 
                Serialization.ImportMetadata importMetadata, 
                Action<TSerializationEntity, TEntity> updateSubEntities_Action, 
                Action<PazCheckDbContext, TEntity> setIsDeleted_Entity_Action, 
                ref int projectEntitiesN, 
                string entityDesc, 
                ILoggersSet loggersSet)
            where TEntity : ProjectVersionedEntityBase, new()
            where TSerializationEntity : Serialization.ProjectVersionedEntityBase
        {
            List<TEntity> originalEntities = entities_ImportSerializationResult.All.Values.ToList();

            foreach (var serializationEntity in serializationEntities)
            {
                if (String.IsNullOrEmpty(serializationEntity.Identifier))
                    continue;

                using var entityIdentifierScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((entityDesc, serializationEntity.Identifier));

                entities_ImportSerializationResult.All.TryGetValue(serializationEntity.Identifier, out TEntity? entity);
                if (entity is null)
                {
                    entity = new TEntity
                    {
                        Identifier = serializationEntity.Identifier,
                        Project = project,
                        ProjectId = project.Id // Optimization
                    };
                    entities.Add(entity);
                    entities_ImportSerializationResult.Added.Add(entity.Identifier);
                    entities_ImportSerializationResult.All.Add(entity.Identifier, entity);
                }
                else
                {
                    if (importMetadata.RootCollectionMode == Serialization.CollectionMode.Add)
                        continue;

                    entity.Identifier = serializationEntity.Identifier; // Case-sensivity issues
                    entities_ImportSerializationResult.Updated.Add(entity.Identifier);
                    originalEntities.Remove(entity);
                }
                entity._LockedByUser = dbContext.User;

                updateSubEntities_Action(serializationEntity, entity);

                projectEntitiesN += 1;
            }

            if (importMetadata.RootCollectionMode == Serialization.CollectionMode.Replace)
            {
                foreach (var entity in originalEntities)
                {
                    setIsDeleted_Entity_Action(dbContext, entity);
                    entities_ImportSerializationResult.Deleted.Add(entity.Identifier);
                    entities_ImportSerializationResult.All.Remove(entity.Identifier);
                }
            }
        }

        private static async Task CheckMonitoringEntitiesAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,            
            Serialization.ImportSerializationResult<SafetyController> safetyControllers_ImportSerializationResult, 
            string unitIdentifier,
            ReferenceEntities referenceEntities,
            ILoggersSet loggersSet)
        {
            await using var dbContext = dbContextFactory.CreateDbContext(); // Not saving this, only for checking
            dbContext.IsInformationSecurityEventsLoggingDisabled = true;

            Common.Serialization.SerializationRootObject serializationRootObject = new();
            serializationRootObject.BasePcObjects = new List<Common.Serialization.BasePcObject>();
            serializationRootObject.PcObjects = new List<Common.Serialization.PcObject>();
            foreach (var safetyController in safetyControllers_ImportSerializationResult.All.Values)
            {
                var safetyControllerParams = safetyController.SafetyControllerParams.Where(e => e._IsDeleted == false);
                if (safetyController.Identifier.EndsWith(PazCheckConstants.IdentifierEnding_Template, StringComparison.InvariantCultureIgnoreCase))
                {
                    var serializationBasePcObject = new Common.Serialization.BasePcObject()
                    {
                        Identifier = safetyController.Identifier,
                        Unit = unitIdentifier,
                        Params = safetyControllerParams.Select(p =>
                        {
                            var param_ = new Serialization.Param();
                            param_.Name = p.ParamName;
                            param_.Value = p.Value;
                            return param_;
                        })
                        .ToList()
                    };
                    serializationRootObject.BasePcObjects.Add(serializationBasePcObject);
                }
                else
                {
                    var serializationPcObject = new Common.Serialization.PcObject()
                    {
                        Identifier = safetyController.Identifier,
                        Unit = unitIdentifier,
                        Params = safetyControllerParams.Select(p =>
                        {
                            var param_ = new Serialization.Param();
                            param_.Name = p.ParamName;
                            param_.Value = p.Value;
                            return param_;
                        })
                        .ToList()
                    };
                    serializationRootObject.PcObjects.Add(serializationPcObject);
                }
            }

            Common.Serialization.ImportSerializationRootObjectResult result = new();

            if (result.BasePcObjects_ImportSerializationResult is null)
            {
                result.BasePcObjects_ImportSerializationResult = new()
                {
                    All = (await dbContext.BasePcObjects
                        .Include(bpo => bpo.Unit)
                        .Include(bpo => bpo.JournalParams)
                        .Include(bpo => bpo.PcObjectEventTypes)
                        .ToListAsync())
                        .GroupBy(bpo => bpo.Unit.IdentifierLower + "." + bpo.IdentifierLower)
                        .ToDictionary(
                            it => it.Key,
                            it => it.OrderByDescending(bpo => bpo._IsDeleted).ThenBy(bpo => bpo._LastChangeTimeUtc).ToList(),
                            StringComparer.InvariantCultureIgnoreCase)
                };
            }

            if (result.PcObjects_ImportSerializationResult is null)
            {
                result.PcObjects_ImportSerializationResult = new()
                {
                    All = (await dbContext.PcObjects
                        .Include(po => po.Unit)
                        .Include(po => po.BasePcObject)
                        .Include(po => po.Parent)
                        .Include(po => po.JournalParams)
                        .ToListAsync())
                        .GroupBy(po => po.Unit.IdentifierLower + "." + po.IdentifierLower)
                        .ToDictionary(
                            it => it.Key,
                            it => it.OrderByDescending(po => po._IsDeleted).ThenBy(po => po._LastChangeTimeUtc).ToList(),
                            StringComparer.InvariantCultureIgnoreCase)
                };
            }

            await ImportMonitoringEntitiesAsync(
                dbContext,                
                serializationRootObject,
                referenceEntities,
                result,
                new CentralServer.Common.Serialization.ImportMetadata()
                {
                    RootCollectionMode = CentralServer.Common.Serialization.CollectionMode.Replace,
                    ChildCollectionMode = CentralServer.Common.Serialization.CollectionMode.Replace,
                    DataCollectionMode = CentralServer.Common.Serialization.CollectionMode.Update,
                },
                loggersSet);
        }

        private static async Task ImportMonitoringEntitiesAsync(
            PazCheckDbContext dbContext, 
            Serialization.SerializationRootObject serializationRootObject,
            ReferenceEntities referenceEntities,
            Serialization.ImportSerializationRootObjectResult result, 
            Serialization.ImportMetadata importMetadata, 
            ILoggersSet loggersSet)
        {
            Dictionary<string, Unit> units = await dbContext.Units.ToDictionaryAsync(u => u.Identifier, StringComparer.InvariantCultureIgnoreCase);

            // BasePcObjects
            if (serializationRootObject.BasePcObjects is not null)
            {
                HashSet<string> serializationRootObject_BasePcObjects_Units = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var serializationBasePcObject in serializationRootObject.BasePcObjects)
                {
                    if (String.IsNullOrEmpty(serializationBasePcObject.Unit))
                    {
                        using var basePcObjectScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.BasePcObject, serializationBasePcObject.Identifier));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierIsNull, "Unit");
                        continue;
                    }

                    serializationRootObject_BasePcObjects_Units.Add(serializationBasePcObject.Unit);
                }

                // [bpo.Unit.IdentifierLower, List of Existing BasePcObjects]
                Dictionary<string, List<BasePcObject>> originalBasePcObjects = result.BasePcObjects_ImportSerializationResult!.All
                    .Select(kvp => kvp.Value.LastOrDefault(o => !o._IsDeleted))
                    .OfType<BasePcObject>()
                    .Where(bpo => serializationRootObject_BasePcObjects_Units.Contains(bpo.Unit.IdentifierLower))
                    .GroupBy(bpo => bpo.Unit.IdentifierLower)
                    .ToDictionary(
                                it => it.Key,
                                it => it.ToList(),
                                StringComparer.InvariantCultureIgnoreCase);

                foreach (var serializationBasePcObject in serializationRootObject.BasePcObjects)
                {
                    if (String.IsNullOrEmpty(serializationBasePcObject.Identifier) ||
                            String.IsNullOrEmpty(serializationBasePcObject.Unit)) // Because of previous loop
                        continue;

                    string key = serializationBasePcObject.Unit + "." + serializationBasePcObject.Identifier;
                    using var basePcObjectScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.BasePcObject, key));

                    //result.BasePcObjects_ImportSerializationResult.All.TryGetValue(serializationBasePcObject.Identifier, out BasePcObject? basePcObject);
                    //if (basePcObject is not null)
                    //{
                    //    loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.Error_IdentifierDuplicated, serializationBasePcObject.Identifier);
                    //    continue;
                    //}

                    result.BasePcObjects_ImportSerializationResult.All.TryGetValue(key, out var basePcObjectsList);
                    BasePcObject? basePcObject = basePcObjectsList?.LastOrDefault();

                    if (basePcObject is null)
                    {
                        basePcObject = new()
                        {
                            Identifier = serializationBasePcObject.Identifier
                        };

                        units.TryGetValue(serializationBasePcObject.Unit, out Unit? unit);
                        if (unit is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid, "Unit");
                            continue;
                        }
                        basePcObject.Unit = unit;
                        basePcObject.Widgets = serializationBasePcObject.Widgets ?? @"";
                        basePcObject.Params = GetParamsString(basePcObject.Params, serializationBasePcObject.Params, Serialization.CollectionMode.AddToEmpty, ValidateParam, loggersSet);
                        SetBasePcObjectJournalParams(basePcObject.JournalParams, serializationBasePcObject.Params, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);
                        SetPcObjectEventTypes(basePcObject.PcObjectEventTypes, serializationBasePcObject.PcObjectEventTypes, referenceEntities, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);
                        SetDbFileReferences(basePcObject.BasePcObjectDbFileReferences, serializationBasePcObject.BasePcObjectDbFileReferences, referenceEntities, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);

                        dbContext.BasePcObjects.Add(basePcObject);
                        result.BasePcObjects_ImportSerializationResult.Added.Add(key);
                        if (basePcObjectsList is null)
                            result.BasePcObjects_ImportSerializationResult.All.Add(key, new List<BasePcObject>() { basePcObject });
                        else
                            basePcObjectsList.Add(basePcObject);
                    }
                    else
                    {
                        basePcObject.Identifier = serializationBasePcObject.Identifier; // Because of case-insensitive issues.

                        if (serializationBasePcObject.Widgets is not null)
                            basePcObject.Widgets = serializationBasePcObject.Widgets;
                        basePcObject.Params = GetParamsString(basePcObject.Params, serializationBasePcObject.Params, importMetadata.ChildCollectionMode, ValidateParam, loggersSet);
                        SetBasePcObjectJournalParams(basePcObject.JournalParams, serializationBasePcObject.Params, dbContext, importMetadata.ChildCollectionMode, loggersSet);
                        SetPcObjectEventTypes(basePcObject.PcObjectEventTypes, serializationBasePcObject.PcObjectEventTypes, referenceEntities, dbContext, importMetadata.ChildCollectionMode, loggersSet);
                        SetDbFileReferences(basePcObject.BasePcObjectDbFileReferences, serializationBasePcObject.BasePcObjectDbFileReferences, referenceEntities, dbContext, importMetadata.ChildCollectionMode, loggersSet);

                        basePcObject._IsDeleted = false;
                        result.BasePcObjects_ImportSerializationResult.Updated.Add(key);
                        if (originalBasePcObjects.TryGetValue(basePcObject.Unit.IdentifierLower, out List<BasePcObject>? forUnit_basePcObjects))
                            forUnit_basePcObjects.Remove(basePcObject);
                        basePcObjectsList!.Remove(basePcObject);
                        basePcObjectsList.Add(basePcObject); // Add to end
                    }
                }

                if (importMetadata.RootCollectionMode == Serialization.CollectionMode.Replace)
                {
                    foreach (var forUnit_basePcObjects in originalBasePcObjects)
                    {
                        foreach (var basePcObject in forUnit_basePcObjects.Value)
                        {
                            if (!basePcObject._IsDeleted &&
                                basePcObject.Identifier != PazCheckConstants.BasePcObject_OtherArea_Template &&
                                basePcObject.Identifier != PazCheckConstants.BasePcObject_OtherItem_Template &&
                                basePcObject.Identifier != PazCheckConstants.BasePcObject_SystemArea_Template &&
                                basePcObject.Identifier != PazCheckConstants.BasePcObject_SystemProcess_Template &&
                                basePcObject.Identifier != PazCheckConstants.BasePcObject_SystemAddon_Template)
                            {
                                string key = basePcObject.Unit.IdentifierLower + "." + basePcObject.IdentifierLower;

                                basePcObject._IsDeleted = true;                                
                                result.BasePcObjects_ImportSerializationResult.Deleted.Add(key);
                                result.BasePcObjects_ImportSerializationResult.All.TryGetValue(key, out var basePcObjectsList);
                                if (basePcObjectsList is not null)
                                {
                                    basePcObjectsList.Remove(basePcObject);
                                    basePcObjectsList.Insert(0, basePcObject); // Move to begin
                                }
                            }
                        }
                    }
                }
            }

            // PcObjects                
            if (serializationRootObject.PcObjects is not null)
            {
                HashSet<string> serializationRootObject_PcObjects_Units = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var serializationPcObject in serializationRootObject.PcObjects)
                {
                    if (String.IsNullOrEmpty(serializationPcObject.Unit))
                    {
                        using var pcObjectScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObject, serializationPcObject.Identifier));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierIsNull, "Unit");
                        continue;
                    }

                    serializationRootObject_PcObjects_Units.Add(serializationPcObject.Unit);
                }

                // [po.Unit.IdentifierLower, List of Existing PcObjects]
                Dictionary<string, List<PcObject>> originalPcObjects = result.PcObjects_ImportSerializationResult!.All
                    .Select(kvp => kvp.Value.LastOrDefault(o => !o._IsDeleted))
                    .OfType<PcObject>()
                    .Where(po => serializationRootObject_PcObjects_Units.Contains(po.Unit.IdentifierLower))
                    .GroupBy(po => po.Unit.IdentifierLower)
                    .ToDictionary(
                                it => it.Key,
                                it => it.ToList(),
                                StringComparer.InvariantCultureIgnoreCase);

                foreach (var serializationPcObject in serializationRootObject.PcObjects)
                {
                    if (String.IsNullOrEmpty(serializationPcObject.Identifier) ||
                            String.IsNullOrEmpty(serializationPcObject.Unit)) // Because of previous loop
                        continue;

                    string key = serializationPcObject.Unit + "." + serializationPcObject.Identifier;
                    using var pcObjectScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObject, key));

                    result.PcObjects_ImportSerializationResult.All.TryGetValue(key, out var pcObjectsList);
                    PcObject? pcObject = pcObjectsList?.LastOrDefault();

                    if (pcObject is null)
                    {
                        pcObject = new()
                        {
                            Identifier = serializationPcObject.Identifier
                        };

                        units.TryGetValue(serializationPcObject.Unit ?? @"", out Unit? unit);
                        if (unit is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid, "Unit");
                            continue;
                        }
                        pcObject.Unit = unit;
                        pcObject.Temp_NewParent_Identifier = serializationPcObject.Params
                            ?.FirstOrDefault(p => String.Equals(p.Name, PazCheckConstants.ParamName_PcObjectParent, StringComparison.InvariantCultureIgnoreCase))
                            ?.Value ?? @"";
                        pcObject.Temp_NewTemplate_Identifier = serializationPcObject.Params
                            ?.FirstOrDefault(p => String.Equals(p.Name, PazCheckConstants.ParamName_PcObjectTemplate, StringComparison.InvariantCultureIgnoreCase))
                            ?.Value ?? @"";

                        pcObject.Widgets = serializationPcObject.Widgets ?? @"";
                        pcObject.Params = GetParamsString(pcObject.Params, serializationPcObject.Params, Serialization.CollectionMode.AddToEmpty, ValidateParam, loggersSet);
                        SetPcObjectJournalParams(pcObject.JournalParams, serializationPcObject.Params, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);
                        await SetJournalParamValuesCollectionsAsync(pcObject.JournalParamValuesCollections, serializationPcObject.JournalParamValuesCollections, dbContext, Serialization.CollectionMode.AddToEmpty, Serialization.CollectionMode.AddToEmpty, loggersSet);
                        await AddPcObjectEventsAsync(pcObject, serializationPcObject.PcObjectEvents, referenceEntities, dbContext, Serialization.CollectionMode.AddToEmpty, Serialization.CollectionMode.AddToEmpty, loggersSet);
                        SetDbFileReferences(pcObject.PcObjectDbFileReferences, serializationPcObject.PcObjectDbFileReferences, referenceEntities, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);

                        dbContext.PcObjects.Add(pcObject);
                        result.PcObjects_ImportSerializationResult.Added.Add(key);
                        if (pcObjectsList is null)
                            result.PcObjects_ImportSerializationResult.All.Add(key, new List<PcObject>() { pcObject });
                        else
                            pcObjectsList.Add(pcObject);
                    }
                    else
                    {
                        pcObject.Identifier = serializationPcObject.Identifier; // Strict comparison, case-insensitive issues.

                        pcObject.Temp_NewParent_Identifier = serializationPcObject.Params
                            ?.FirstOrDefault(p => String.Equals(p.Name, PazCheckConstants.ParamName_PcObjectParent, StringComparison.InvariantCultureIgnoreCase))
                            ?.Value;
                        pcObject.Temp_NewTemplate_Identifier = serializationPcObject.Params
                            ?.FirstOrDefault(p => String.Equals(p.Name, PazCheckConstants.ParamName_PcObjectTemplate, StringComparison.InvariantCultureIgnoreCase))
                            ?.Value;

                        if (serializationPcObject.Widgets is not null)
                            pcObject.Widgets = serializationPcObject.Widgets;
                        pcObject.Params = GetParamsString(pcObject.Params, serializationPcObject.Params, importMetadata.ChildCollectionMode, ValidateParam, loggersSet);
                        SetPcObjectJournalParams(pcObject.JournalParams, serializationPcObject.Params, dbContext, importMetadata.ChildCollectionMode, loggersSet);
                        await SetJournalParamValuesCollectionsAsync(pcObject.JournalParamValuesCollections, serializationPcObject.JournalParamValuesCollections, dbContext, importMetadata.ChildCollectionMode, importMetadata.DataCollectionMode, loggersSet);
                        await AddPcObjectEventsAsync(pcObject, serializationPcObject.PcObjectEvents, referenceEntities, dbContext, importMetadata.ChildCollectionMode, importMetadata.DataCollectionMode, loggersSet);
                        SetDbFileReferences(pcObject.PcObjectDbFileReferences, serializationPcObject.PcObjectDbFileReferences, referenceEntities, dbContext, importMetadata.ChildCollectionMode, loggersSet);

                        pcObject._IsDeleted = false;
                        result.PcObjects_ImportSerializationResult.Updated.Add(key);
                        var rootPcObject = PazCheckDbHelper.GetRootPcObject(pcObject);
                        if (originalPcObjects.TryGetValue(pcObject.Unit.IdentifierLower, out List<PcObject>? forUnit_PcObjects))
                            forUnit_PcObjects.Remove(pcObject);
                        pcObjectsList!.Remove(pcObject);
                        pcObjectsList.Add(pcObject); // Add to end
                    }
                }

                if (importMetadata.RootCollectionMode == Serialization.CollectionMode.Replace)
                {
                    foreach (var forUnit_PcObjects in originalPcObjects)
                    {
                        foreach (var pcObject in forUnit_PcObjects.Value)
                        {
                            if (!pcObject._IsDeleted)
                            {
                                string key = pcObject.Unit.IdentifierLower + "." + pcObject.IdentifierLower;

                                pcObject._IsDeleted = true;                                
                                result.PcObjects_ImportSerializationResult.Deleted.Add(key);
                                result.PcObjects_ImportSerializationResult.All.TryGetValue(key, out var pcObjectsList);
                                if (pcObjectsList is not null)
                                {
                                    pcObjectsList.Remove(pcObject);
                                    pcObjectsList.Insert(0, pcObject); // Move to begin
                                }
                            }
                        }
                    }
                }
            }

            List<PcObject> currentPcObjects = result.PcObjects_ImportSerializationResult!.All
                    .Select(kvp => kvp.Value.LastOrDefault(o => !o._IsDeleted))
                    .OfType<PcObject>()
                    .ToList();

            foreach (var pcObject in currentPcObjects)
            {
                string unit_Identifier = pcObject.Unit.Identifier;

                // pcObject.Unit actualization.
                if (serializationRootObject.PcObjects is not null)
                {
                    string parentIdentifier;
                    if (pcObject.Temp_NewParent_Identifier is not null)
                    {
                        parentIdentifier = pcObject.Temp_NewParent_Identifier;
                        pcObject.Temp_NewParent_Identifier = null;
                    }
                    else
                    {
                        parentIdentifier = pcObject!.Parent?.Identifier ?? @"";
                    }

                    PcObject? parentPcObject = null;
                    if (parentIdentifier != @"")
                    {
                        result.PcObjects_ImportSerializationResult.All.TryGetValue(unit_Identifier + "." + parentIdentifier, out var pcObjectsList2);
                        parentPcObject = pcObjectsList2?.LastOrDefault(bpo => !bpo._IsDeleted);
                        if (parentPcObject is null)
                        {
                            using var s = loggersSet.UserFriendlyLogger.BeginScope<(string, string)[]>([
                                (Properties.Resources.PcObject, pcObject!.Identifier!),
                                        (Properties.Resources.PcObject_Parent, parentIdentifier) ]);
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid);
                            continue;
                        }
                    }

                    pcObject!.Parent = parentPcObject;
                }

                // pcObject.BasePcObject actualization.
                string template_Identifier;
                if (pcObject.Temp_NewTemplate_Identifier is not null)
                {
                    template_Identifier = pcObject.Temp_NewTemplate_Identifier;
                    pcObject.Temp_NewTemplate_Identifier = null;
                }
                else
                {
                    template_Identifier = pcObject!.BasePcObject.Identifier;
                }

                BasePcObject? basePcObject = null;
                if (template_Identifier != @"")
                {
                    result.BasePcObjects_ImportSerializationResult!.All.TryGetValue(unit_Identifier + "." + template_Identifier, out var basePcObjectsList2);
                    basePcObject = basePcObjectsList2?.LastOrDefault(bpo => !bpo._IsDeleted);
                    if (basePcObject is null)
                    {
                        using var s = loggersSet.UserFriendlyLogger.BeginScope<(string, string)[]>([
                            (Properties.Resources.PcObject, pcObject!.Identifier!),
                                    (Properties.Resources.BasePcObject, template_Identifier) ]);
                        loggersSet.UserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid);
                        continue;
                    }
                }
                else
                {
                    using var s = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.PcObject, pcObject!.Identifier!));
                    loggersSet.UserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierIsNull, PazCheckConstants.ParamName_PcObjectTemplate);
                    continue;
                }

                if (basePcObject is not null)
                    pcObject!.BasePcObject = basePcObject;
            }

            // pcObject.Level actualization.
            if (serializationRootObject.PcObjects is not null)
            {
                foreach (var pcObject in currentPcObjects)
                {
                    int level = 0;
                    var parent = pcObject.Parent;
                    while (parent is not null)
                    {
                        level += 1;
                        if (level > 255)
                        {
                            using var s1 = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.PcObject, pcObject.Identifier!));
                            using var s2 = loggersSet.UserFriendlyLogger.BeginScope((Properties.Resources.PcObject_Parent, pcObject.Parent?.Identifier!));
                            loggersSet.UserFriendlyLogger.LogError(Properties.Resources.PcObjectParentLoop);
                            break;
                        }
                        parent = parent.Parent;
                    }
                    pcObject.Level = level;
                }
            }

            var metaParam = dbContext.MetaParams.Single(mp => mp.ParamName == PazCheckConstants.MetaParamNameBase_Monitoring_Config);
            metaParam.Value = Guid.NewGuid().ToString();
            metaParam._LastChangeTimeUtc = DateTime.UtcNow;
        }

        private static void ValidateParam(string paramName, string? paramValue, IUserFriendlyLogger userFriendlyLogger)
        {
            if (String.Equals(paramName, PazCheckConstants.ParamName_Converter, StringComparison.InvariantCultureIgnoreCase) &&
                !String.IsNullOrEmpty(paramValue))
            {
                foreach (var v in CsvHelper.ParseCsvLine(@",", paramValue))
                {
                    if (String.IsNullOrEmpty(v))
                        continue;
                    
                    var parts = v.Split(new[] { "->" }, StringSplitOptions.None);
                    if (parts.Length == 1)
                    {
                        string expressionString = parts[0].Trim();
                        if (!new SszExpression(expressionString).IsValid)
                            userFriendlyLogger.LogError(Ssz.Utils.Properties.Resources.PrepareLambdaExpressionError + ": " + expressionString);
                    }
                    else
                    {
                        string expressionString = parts[0].Trim();
                        if (!new SszExpression(expressionString).IsValid)
                            userFriendlyLogger.LogError(Ssz.Utils.Properties.Resources.PrepareLambdaExpressionError + ": " + expressionString);

                        expressionString = parts[1].Trim();
                        if (!new SszExpression(expressionString).IsValid)
                            userFriendlyLogger.LogError(Ssz.Utils.Properties.Resources.PrepareLambdaExpressionError + ": " + expressionString);
                    }
                }
            }
        }

        /// <summary>
        ///     Вызывается для единичного файла или файла из архива.
        ///     По завершению jobProgress должен стать 100% или перейти в статус ошибки, если нужно прекратить всю комплексную операцию.     
        ///     Precondition: Main thread.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="user"></param>
        /// <param name="csvStream"></param>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="jobProgress"></param>
        /// <param name="mainServerWorker"></param>        
        /// <param name="loggersSet"></param>
        /// <param name="result"></param>
        /// <param name="preview"></param>
        /// <returns></returns>
        private static async Task<FileContentType> ImportStdFileAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            string user,
            Stream csvStream, 
            int? projectId,            
            CancellationToken cancellationToken,
            IJobProgress jobProgress,
            IMainServerWorker mainServerWorker,            
            ILoggersSet loggersSet,
            Serialization.ImportSerializationRootObjectResult result,
            bool preview)
        {
            var csvStreamReader = CharsetDetectorHelper.GetStreamReader(csvStream, Encoding.UTF8, loggersSet); // Do not dispose stream

            var firstLine = csvStreamReader.ReadLine();            
            var firstLineValues = Ssz.Utils.CsvHelper.ParseCsvLine(@",", firstLine);
            if (firstLineValues.Length < 4 || !String.Equals(firstLineValues[0], PazCheckConstants.ContentDirective_FileFormat, StringComparison.InvariantCultureIgnoreCase) ||
                !String.Equals(firstLineValues[2], PazCheckConstants.ContentDirective_FormatVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_FileInvalidFormat);
                await jobProgress.SetJobProgressAsync(null, null, Properties.Resources.Error_FileInvalidFormat, Ssz.Utils.StatusCodes.BadInvalidArgument);
                return FileContentType.Unknown;
            }
            string stdFileFormat = firstLineValues[1] ?? @"";
            string formatVersion = firstLineValues[3] ?? @"";

            Common.Serialization.ImportMetadata importMetadata = new();
            importMetadata.RootCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_RootCollectionMode), Common.Serialization.CollectionMode.Update);
            importMetadata.ChildCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_ChildCollectionMode), Common.Serialization.CollectionMode.Update);
            importMetadata.DataCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_DataCollectionMode), Common.Serialization.CollectionMode.Update);

            string stdFileContentNoFirstLine = await csvStreamReader.ReadToEndAsync();

            FileContentType contentType = FileContentType.Unknown;            

            if (String.Equals(stdFileFormat, PazCheckConstants.StdFileFormat_JsonObjects, StringComparison.InvariantCultureIgnoreCase))
            {
                contentType = new FileContentType() { Id = @"JSON Objects", Desc = @"JSON Objects" };

                Serialization.SerializationRootObject? serializationRootObject = null;
                try
                {
                    serializationRootObject = JsonSerializer.Deserialize(
                            stdFileContentNoFirstLine, Simcode.PazCheck.CentralServer.Common.Serialization.SourceGenerationContext.Default.SerializationRootObject);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.ImportSerializationRootObject_Error + "; " + ex.Message);
                    await jobProgress.SetJobProgressAsync(null, null, Properties.Resources.ImportSerializationRootObject_Error + "; " + ex.Message, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }

                if (serializationRootObject is null)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.ImportSerializationRootObject_Error);
                    await jobProgress.SetJobProgressAsync(null, null, Properties.Resources.ImportSerializationRootObject_Error, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }

                await ImportSerializationRootObjectAsync(
                    serializationRootObject, 
                    importMetadata,
                    dbContextFactory,
                    user,
                    projectId, 
                    cancellationToken, 
                    jobProgress,                    
                    loggersSet, 
                    result,
                    preview);                             
            }
            else if (String.Equals(stdFileFormat, PazCheckConstants.StdFileFormat_TableObjects, StringComparison.InvariantCultureIgnoreCase))
            {                
                contentType = new FileContentType() { Id = @"CSV Objects", Desc = @"CSV Objects" };

                Serialization.SerializationRootObject? serializationRootObject = null;
                try
                {
                    serializationRootObject = new();
                    ImportTableObjects(
                        stdFileContentNoFirstLine, 
                        formatVersion, 
                        1,
                        serializationRootObject,
                        cancellationToken,
                        loggersSet);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.ImportSerializationRootObject_Error);                    
                    await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }                

                await ImportSerializationRootObjectAsync(
                    serializationRootObject, 
                    importMetadata,
                    dbContextFactory,
                    user,
                    projectId, 
                    cancellationToken, 
                    jobProgress,                    
                    loggersSet, 
                    result,
                    preview);                
            }
            else if (String.Equals(stdFileFormat, PazCheckConstants.StdFileFormat_JournalParamValues, StringComparison.InvariantCultureIgnoreCase))
            {
                contentType = new FileContentType() { Id = @"Journal Param Values", Desc = @"Journal Param Values" };

                Serialization.SerializationRootObject? serializationRootObject = null;
                try
                {
                    serializationRootObject = ImportJournalParamValues(stdFileContentNoFirstLine, formatVersion, 1, cancellationToken, loggersSet);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.ImportSerializationRootObject_Error + "; " + ex.Message);
                    await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }

                if (serializationRootObject is null)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.ImportSerializationRootObject_Error);
                    await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }

                await ImportSerializationRootObjectAsync(
                    serializationRootObject, 
                    importMetadata,
                    dbContextFactory,
                    user,
                    projectId, 
                    cancellationToken, 
                    jobProgress,                    
                    loggersSet,
                    result,
                    preview);
            }
            else if (String.Equals(stdFileFormat, PazCheckConstants.StdFileFormat_TableCeMatrix, StringComparison.InvariantCultureIgnoreCase))
            {                
                contentType = new FileContentType() { Id = @"CSV CeMatrix", Desc = @"CSV CeMatrix" };                

                CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().AddonsThreadSafe.OfType<CeMatrixRuntimeAddonBase>().FirstOrDefault();
                if (ceMatrixRuntimeAddon is null)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.InvalidAddonType, @"CeMatrixRuntimeAddonBase");
                    await jobProgress.SetJobProgressAsync(null, null, string.Format(Properties.Resources.InvalidAddonType, @"CeMatrixRuntimeAddonBase"), Ssz.Utils.StatusCodes.BadInvalidArgument);
                    return FileContentType.Unknown;
                }

                await ceMatrixRuntimeAddon.ImportCeMatrixAsync(
                    stdFileContentNoFirstLine, 
                    formatVersion,
                    dbContextFactory,
                    user,
                    projectId!.Value, 
                    mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                    cancellationToken, 
                    jobProgress, 
                    loggersSet, 
                    result,
                    preview);                
            }    
            else
            {
                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_FileInvalidFormat);
                await jobProgress.SetJobProgressAsync(null, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
            }
            
            return contentType;
        }

        private static async Task<UnitEventsInterval?> ImportStdEventsJournalFileAsync(            
            Stream stream,
            string fileName,
            PazCheckDbContext dbContext,
            Unit unit,
            CancellationToken cancellationToken,
            ILoggersSet loggersSet,
            IJobProgress jobProgress,
            bool preview)
        {
            UnitEventsInterval? unitEventsInterval = null;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Serialization.UnitEventsInterval? serializationUnitEventsInterval = null;

                if (fileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                {
                    serializationUnitEventsInterval = new();

                    await ImportStdFile_SimpleAsync(
                        dbContext,
                        stream,
                        cancellationToken,
                        loggersSet,
                        serializationUnitEventsInterval);

                    await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.Good);
                }                
                else
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_FileInvalidFormat);
                    await jobProgress.SetJobProgressAsync(100, null, Properties.Resources.Error_FileInvalidFormat, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    //contentType = FileContentType.Unknown;
                }

                if (serializationUnitEventsInterval is null || serializationUnitEventsInterval.UnitEvents is null || serializationUnitEventsInterval.UnitEvents.Count == 0)
                {
                    unitEventsInterval = null;
                }
                else
                {
                    unitEventsInterval = new();
                    unitEventsInterval.UnitEvents = serializationUnitEventsInterval.UnitEvents
                        .Select(ui => GetUnitEvent(ui))
                        .OrderBy(ue => ue.EventTimeUtc).ToList();

                    unitEventsInterval.LoadTimeUtc = DateTime.UtcNow;
                    unitEventsInterval.BeginTimeUtc = unitEventsInterval.UnitEvents.First().EventTimeUtc;
                    unitEventsInterval.EndTimeUtc = unitEventsInterval.UnitEvents.Last().EventTimeUtc;
                }

                if (unitEventsInterval is not null)
                {
                    unitEventsInterval.Unit = unit; // For performance
                    unit.UnitEventsIntervals.Add(unitEventsInterval);

                    if (!preview)
                        await dbContext.SaveChangesAsync();
                }
            }
            catch //(Exception ex)
            {
                await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
            }
            finally
            {
                stopwatch.Stop();
                loggersSet.LoggerAndUserFriendlyLogger.LogInformation(Properties.Resources.ImportEventsJournalFileFinished, stopwatch.ElapsedMilliseconds);
            }

            return unitEventsInterval;
        }        

        private static void ImportTableObjects(
            string csvStdFileContentNoFirstLine, 
            string formatVersion, 
            int lineNumber, 
            object containerObject,
            CancellationToken cancellationToken, 
            ILoggersSet loggersSet)
        {
            if (new Any(formatVersion).ValueAsInt32(false) != 1)
            {
                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.UnknownFileFormatVersion, formatVersion);
                throw new Exception();
            }            
            
            var fileLines = Ssz.Utils.CsvHelper.ParseCsvMultiline(@",", csvStdFileContentNoFirstLine);
            if (fileLines.Count < 2)
                return;            

            // [List Property Name, List_PropertyDesc]
            Dictionary<string, List_PropertyDesc> root_List_PropertyDescs = GetList_PropertyDescs(containerObject.GetType(), includeChildren: true);
                        
            List_PropertyDesc? parent_List_PropertyDesc = null;
            PropertyInfo? parentIdentifier_PropertyInfo = null;            
            List_PropertyDesc? list_PropertyDesc = null;
            // (PropertyInfo, NameValues in [])
            List<(PropertyInfo?, CaseInsensitiveOrderedDictionary<string?>?)>? columnsInfo = null;
            foreach (var line in fileLines)
            {
                lineNumber += 1;
                if (line.All(v => String.IsNullOrEmpty(v)))
                    continue;

                using var lineScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.LineScope, lineNumber));

                string column0 = (line[0] ?? @"").Trim();

                if (String.Equals(column0, PazCheckConstants.ContentDirective_Collection, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (line.Count < 2)
                    {                        
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, @"<empty>");
                        throw new Exception();
                    }
                    string? collection = line[1];
                    if (String.IsNullOrEmpty(collection))
                    {
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, @"<empty>");
                        throw new Exception();
                    }
                    int i = collection.IndexOf('.');
                    if (i < 0)
                    {
                        parentIdentifier_PropertyInfo = null;
                        parent_List_PropertyDesc = null;                        
                        root_List_PropertyDescs.TryGetValue(collection, out list_PropertyDesc);
                        if (list_PropertyDesc is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, collection);
                            throw new Exception();
                        }

                        Type serializationObjectsListType2 = typeof(List<>).MakeGenericType(list_PropertyDesc.ListElement_Type!);
                        var serializationObjectsList2 = list_PropertyDesc.List_PropertyInfo!.GetValue(containerObject);
                        if (serializationObjectsList2 is null)
                        {
                            serializationObjectsList2 = Activator.CreateInstance(serializationObjectsListType2);
                            list_PropertyDesc.List_PropertyInfo!.SetValue(containerObject, serializationObjectsList2);
                        }
                    }
                    else
                    {
                        string parentCollection = collection.Substring(0, i);
                        string childCollection = collection.Substring(i + 1);

                        root_List_PropertyDescs.TryGetValue(parentCollection, out parent_List_PropertyDesc);
                        if (parent_List_PropertyDesc is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, parentCollection);
                            throw new Exception();
                        }

                        parent_List_PropertyDesc.ChildList_PropertyDescs!.TryGetValue(childCollection, out list_PropertyDesc);
                        if (list_PropertyDesc is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, childCollection);
                            throw new Exception();
                        }
                    }
                    
                    columnsInfo = null;
                    continue;
                }

                if (column0.StartsWith("#")) // Comment
                    continue;

                if (list_PropertyDesc is null)
                {                    
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, @"<empty>");
                    throw new Exception();
                }
                if (column0.Length > 0 && column0[0] == '.') // Props descs rows
                {
                    columnsInfo = new List<(PropertyInfo?, CaseInsensitiveOrderedDictionary<string?>?)>(line.Count);                    
                    
                    foreach (int i in Enumerable.Range(0, line.Count))
                    {
                        string? columnHeader = line[i];
                        if (String.IsNullOrEmpty(columnHeader))
                        {
                            columnsInfo.Add((null, null));
                            continue;
                        }

                        columnHeader = columnHeader.Trim();
                        if (!columnHeader.StartsWith(@"."))
                        {
                            columnHeader = @".Params[" + columnHeader + "]";
                        }

                        if (columnHeader.StartsWith(".Parent."))
                        {
                            if (parent_List_PropertyDesc is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_UnknownCollection, @"Parent:<empty>");
                                throw new Exception();
                            }

                            string parentIdentifierPropertyName = columnHeader.Substring(".Parent.".Length).Trim();
                            parentIdentifier_PropertyInfo = parent_List_PropertyDesc.ListElement_Type!.GetProperty(parentIdentifierPropertyName);
                            if (parentIdentifier_PropertyInfo is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, parentIdentifierPropertyName);
                                throw new Exception();
                            }

                            columnsInfo.Add((parentIdentifier_PropertyInfo, null));
                            continue;
                        }
                        columnHeader = columnHeader.Substring(1);
                        var parIndex = columnHeader.IndexOf('[');
                        if (parIndex == -1)
                        {
                            var propertyInfo = list_PropertyDesc.ListElement_Type!.GetProperty(columnHeader);
                            if (propertyInfo is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, columnHeader);
                                throw new Exception();
                            }
                            columnsInfo.Add((propertyInfo, null));
                        }
                        else
                        {
                            if (columnHeader[columnHeader.Length - 1] != ']')
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, columnHeader);
                                throw new Exception();
                            }
                            var propertyInfo = list_PropertyDesc.ListElement_Type!.GetProperty(columnHeader.Substring(0, parIndex));
                            if (propertyInfo is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, columnHeader.Substring(0, parIndex));
                                throw new Exception();
                            }
                            columnsInfo.Add((propertyInfo,
                                NameValueCollectionHelper.Parse(
                                    columnHeader.Substring(parIndex + 1, columnHeader.Length - parIndex - 2))));
                        }
                    }

                    continue;
                }
                if (columnsInfo is null)
                {                    
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, @"<empty>");
                    throw new Exception();
                }                
                var newSerializationObject = Activator.CreateInstance(list_PropertyDesc!.ListElement_Type!);
                if (newSerializationObject is null)
                {                    
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, list_PropertyDesc.ListElement_Type!.FullName);
                    throw new Exception();
                }
                object? parentIdentifier = null;
                foreach (int columnIndex in Enumerable.Range(0, columnsInfo.Count))
                {
                    string? listItemString;
                    if (columnIndex < line.Count)
                        listItemString = line[columnIndex];
                    else
                        listItemString = null;

                    var (propertyInfo, header_NameValueCollection) = columnsInfo[columnIndex];
                    if (propertyInfo is null)
                        continue;
                    if (propertyInfo == parentIdentifier_PropertyInfo)
                    {
                        parentIdentifier = new Any(listItemString).ValueAs(parentIdentifier_PropertyInfo.PropertyType, false);
                        continue;
                    }
                    if (header_NameValueCollection is null)
                    {
                        object? value;
                        if (propertyInfo.PropertyType.IsAssignableFrom(typeof(CaseInsensitiveOrderedDictionary<string?>)))
                        {
                            value = NameValueCollectionHelper.Parse(listItemString);
                        }
                        else
                        {
                            value = new Any(listItemString ?? @"").ValueAs(propertyInfo.PropertyType, false);
                        }                        
                        propertyInfo.SetValue(newSerializationObject, value);
                    }
                    else if (!String.IsNullOrEmpty(listItemString))
                    {
                        Type propertyType = propertyInfo.PropertyType;
                        if (!propertyType.IsGenericType)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, list_PropertyDesc.ListElement_Type!.FullName);
                            throw new Exception();
                        }
                        var genericArguments = propertyType.GetGenericArguments();
                        if (genericArguments.Length != 1)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, list_PropertyDesc.ListElement_Type!.FullName);
                            throw new Exception();
                        }
                        object? list = propertyInfo.GetValue(newSerializationObject);
                        object? listItem;
                        if (list is null)
                        {
                            list = Activator.CreateInstance(propertyType)!;
                            if (list is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, propertyType!.FullName);
                                throw new Exception();
                            }
                            propertyInfo.SetValue(newSerializationObject, list);
                        }
                        var genericArgument = genericArguments[0];
                        if (genericArgument == typeof(string))
                        {
                            // Optimization
                            listItem = listItemString;
                        }
                        else
                        {
                            listItem = new Any(listItemString).ValueAs(genericArgument, false);
                            if (listItem is null)
                            {
                                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.CsvObjectsFile_InvalidObjectValue, listItemString);
                                listItem = Activator.CreateInstance(genericArgument)!;
                                if (listItem is null)
                                {
                                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, genericArgument!.FullName);
                                    throw new Exception();
                                }
                            }
                            NameValueCollectionHelper.SetNameValueCollection(listItem, header_NameValueCollection);
                        }
                        propertyType.GetMethod("Add")!.Invoke(list, new[] { listItem });
                    }
                }

                object obj;
                if (parent_List_PropertyDesc is null)
                {
                    obj = containerObject;
                }
                else
                {
                    Type parentSerializationObjectsListType = typeof(List<>).MakeGenericType(parent_List_PropertyDesc.ListElement_Type!);
                    var parentSerializationObjectsList = parent_List_PropertyDesc.List_PropertyInfo!.GetValue(containerObject);
                    if (parentSerializationObjectsList is null)
                    {
                        parentSerializationObjectsList = Activator.CreateInstance(parentSerializationObjectsListType);
                        if (parentSerializationObjectsList is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, parentSerializationObjectsListType!.FullName);
                            throw new Exception();
                        }
                        parent_List_PropertyDesc.List_PropertyInfo!.SetValue(containerObject, parentSerializationObjectsList);
                    }

                    object? parentObject = null;
                    foreach (var po in (IEnumerable<object?>)parentSerializationObjectsList!)
                    {
                        var pi = parentIdentifier_PropertyInfo!.GetValue(po);
                        if (Equals(pi, parentIdentifier))
                        {
                            parentObject = po;
                            break;
                        }
                    }
                    if (parentObject is null)
                    {
                        parentObject = Activator.CreateInstance(parent_List_PropertyDesc.ListElement_Type!);
                        if (parentObject is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, parent_List_PropertyDesc.ListElement_Type!.FullName);
                            throw new Exception();
                        }
                        parentIdentifier_PropertyInfo!.SetValue(parentObject, parentIdentifier);
                        parentSerializationObjectsListType.GetMethod("Add")!.Invoke(parentSerializationObjectsList, new[] { parentObject });
                    }

                    obj = parentObject;
                }

                Type serializationObjectsListType = typeof(List<>).MakeGenericType(list_PropertyDesc.ListElement_Type!);
                var serializationObjectsList = list_PropertyDesc.List_PropertyInfo!.GetValue(obj);
                if (serializationObjectsList is null)
                {
                    serializationObjectsList = Activator.CreateInstance(serializationObjectsListType);
                    list_PropertyDesc.List_PropertyInfo!.SetValue(obj, serializationObjectsList);
                }
                serializationObjectsListType.GetMethod("Add")!.Invoke(serializationObjectsList, new[] { newSerializationObject });
            }
        }

        /// <summary>
        ///     Imports simplified CSV format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="csvStream"></param>
        /// <param name="cancellationToken"></param>        
        /// <param name="loggersSet"></param>
        /// <param name="containerObject"></param>
        private static async Task ImportStdFile_SimpleAsync(PazCheckDbContext dbContext, Stream csvStream, CancellationToken cancellationToken, ILoggersSet loggersSet, object containerObject)            
        {
            using var csvStreamReader = CharsetDetectorHelper.GetStreamReader(csvStream, Encoding.UTF8, loggersSet);

            var firstLine = csvStreamReader.ReadLine();
            var firstLineValues = Ssz.Utils.CsvHelper.ParseCsvLine(@",", firstLine);

            string stdFileFormat = firstLineValues[1] ?? @"";
            string formatVersion = firstLineValues[3] ?? @"";

            Common.Serialization.ImportMetadata importMetadata = new();
            importMetadata.RootCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_RootCollectionMode), Common.Serialization.CollectionMode.Update);
            importMetadata.ChildCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_ChildCollectionMode), Common.Serialization.CollectionMode.Update);
            importMetadata.DataCollectionMode = ConfigurationHelper.GetValue_Enum<Common.Serialization.CollectionMode>(
                GetDocumentMetadataValue(firstLineValues, PazCheckConstants.ContentDirective_DataCollectionMode), Common.Serialization.CollectionMode.Update);

            string stdFileContentNoFirstLine = await csvStreamReader.ReadToEndAsync();

            ImportTableObjects(
                stdFileContentNoFirstLine,
                formatVersion,
                1,
                containerObject,
                cancellationToken,
                loggersSet);

            //bool isStdFileFormat = IsStdFileFormat(csvStream);                       

            //if (isStdFileFormat)
            //{
                
            //}
            //else
            //{
            //    var firstLine = csvStreamReader.ReadLine();                

            //    string csvString = firstLine + "\n" + await csvStreamReader.ReadToEndAsync();                

            //    var fileLines = Ssz.Utils.CsvHelper.ParseCsvMultiline(@",", csvString);
            //    if (fileLines.Count < 2)
            //        return;

            //    int lineNumber = 0;

            //    // (PropertyInfo, NameValues in [])
            //    List<(PropertyInfo?, CaseInsensitiveOrderedDictionary<string?>?)>? columnsInfo = null;
            //    foreach (var line in fileLines)
            //    {
            //        lineNumber += 1;
            //        if (line.All(v => String.IsNullOrEmpty(v)))
            //            continue;

            //        using var lineScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.LineScope, lineNumber));

            //        string column0 = (line[0] ?? @"").Trim();

            //        if (column0.StartsWith("#")) // Comment
            //            continue;

            //        if (column0.Length > 0 && columnsInfo is null)
            //        {
            //            columnsInfo = new List<(PropertyInfo?, CaseInsensitiveOrderedDictionary<string?>?)>(line.Count);

            //            foreach (int i in Enumerable.Range(0, line.Count))
            //            {
            //                string? columnHeader = line[i];
            //                if (String.IsNullOrEmpty(columnHeader))
            //                {
            //                    columnsInfo.Add((null, null));
            //                    continue;
            //                }
            //                columnHeader = columnHeader.Trim();
            //                var propertyInfo = typeof(UnitEvent).GetProperty(columnHeader);
            //                if (propertyInfo is null)
            //                {
            //                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, columnHeader);
            //                    throw new Exception();
            //                }
            //                columnsInfo.Add((propertyInfo, null));
            //            }

            //            continue;
            //        }
            //        if (columnsInfo is null)
            //        {
            //            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, @"<empty>");
            //            throw new Exception();
            //        }
            //        var newSerializationObject = new UnitEvent();
            //        if (newSerializationObject is null)
            //        {
            //            loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, typeof(UnitEvent).FullName);
            //            throw new Exception();
            //        }
            //        foreach (int columnIndex in Enumerable.Range(0, columnsInfo.Count))
            //        {
            //            string? listItemString;
            //            if (columnIndex < line.Count)
            //                listItemString = line[columnIndex];
            //            else
            //                listItemString = null;

            //            var (propertyInfo, header_NameValueCollection) = columnsInfo[columnIndex];
            //            if (propertyInfo is null)
            //                continue;
            //            if (header_NameValueCollection is null)
            //            {
            //                object? value;
            //                if (propertyInfo.PropertyType.IsAssignableFrom(typeof(CaseInsensitiveOrderedDictionary<string?>)))
            //                {
            //                    value = NameValueCollectionHelper.Parse(listItemString);
            //                }
            //                else
            //                {
            //                    value = new Any(listItemString ?? @"").ValueAs(propertyInfo.PropertyType, false);
            //                }
            //                propertyInfo.SetValue(newSerializationObject, value);
            //            }
            //            //else if (!String.IsNullOrEmpty(listItemString))
            //            //{
            //            //    Type propertyType = propertyInfo.PropertyType;
            //            //    if (!propertyType.IsGenericType)
            //            //    {
            //            //        loggersSet.WrapperUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, list_PropertyDesc.ListElement_Type!.FullName);
            //            //        return;
            //            //    }
            //            //    var genericArguments = propertyType.GetGenericArguments();
            //            //    if (genericArguments.Length != 1)
            //            //    {
            //            //        loggersSet.WrapperUserFriendlyLogger.LogError(Properties.Resources.CsvObjectsFile_InvalidObjectProperty, list_PropertyDesc.ListElement_Type!.FullName);
            //            //        return;
            //            //    }
            //            //    object? list = propertyInfo.GetValue(newSerializationObject);
            //            //    object? listItem;
            //            //    if (list is null)
            //            //    {
            //            //        list = Activator.CreateInstance(propertyType)!;
            //            //        if (list is null)
            //            //        {
            //            //            loggersSet.WrapperUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, propertyType!.FullName);
            //            //            return null;
            //            //        }
            //            //        propertyInfo.SetValue(newSerializationObject, list);
            //            //    }
            //            //    var genericArgument = genericArguments[0];
            //            //    if (genericArgument == typeof(string))
            //            //    {
            //            //        // Optimization
            //            //        listItem = listItemString;
            //            //    }
            //            //    else
            //            //    {
            //            //        listItem = new Any(listItemString).ValueAs(genericArgument, false);
            //            //        if (listItem is null)
            //            //        {
            //            //            loggersSet.WrapperUserFriendlyLogger.LogWarning(Properties.Resources.CsvObjectsFile_InvalidObjectValue, listItemString);
            //            //            listItem = Activator.CreateInstance(genericArgument)!;
            //            //            if (listItem is null)
            //            //            {
            //            //                loggersSet.WrapperUserFriendlyLogger.LogError(Properties.Resources.Activator_CreateInstance_Error, genericArgument!.FullName);
            //            //                return null;
            //            //            }
            //            //        }
            //            //        NameValueCollectionHelper.SetNameValueCollection(ref listItem, header_NameValueCollection);
            //            //    }
            //            //    propertyType.GetMethod("Add")!.Invoke(list, new[] { listItem });
            //            //}
            //        }

            //        ((UnitEventsInterval)containerObject).UnitEvents.Add(newSerializationObject);
            //    }
            //}                        
        }

        private static Serialization.SerializationRootObject? ImportJournalParamValues(string csvStdFileContentNoFirstLine, string formatVersion, int lineNumber, CancellationToken cancellationToken, ILoggersSet loggersSet)
        {
            if (new Any(formatVersion).ValueAsInt32(false) != 1)
            {
                loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.UnknownFileFormatVersion, formatVersion);
                return null;
            }
            
            var fileLines = Ssz.Utils.CsvHelper.ParseCsvMultiline(@",", csvStdFileContentNoFirstLine);
            if (fileLines.Count == 0)
                return null;

            Serialization.SerializationRootObject serializationRootObject = new();

            CaseInsensitiveOrderedDictionary<Serialization.PcObject> pcObjects = new();
            
            foreach (var line in fileLines)
            {
                lineNumber += 1;
                if (line.All(v => String.IsNullOrEmpty(v)) || 
                        (line[0]?.TrimStart()?.StartsWith("#") ?? false) || 
                        (line[0]?.TrimStart()?.StartsWith("!") ?? false))
                    continue;

                using var lineScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.LineScope, lineNumber));

                if (line.Count < 5)
                {                    
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.JournalParamValuesFile_InvalidLine);
                    return null;
                }

                string? unit = line[0];
                string? tagName = line[1];
                string? paramName = line[2];

                if (String.IsNullOrEmpty(tagName) || String.IsNullOrEmpty(paramName))
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.JournalParamValuesFile_InvalidLine);
                    return null;
                }

                Serialization.PcObject? pcObject = pcObjects.TryGetValue(tagName);
                if (pcObject is null)
                {
                    pcObject = new()
                    {
                        Identifier = tagName,
                        Unit = unit,                        
                    };
                    pcObjects.Add(tagName, pcObject);
                }
                Serialization.JournalParamValuesCollection? journalParamValuesCollection = pcObject.JournalParamValuesCollections!.FirstOrDefault(jp => String.Equals(jp.Name, paramName, StringComparison.InvariantCultureIgnoreCase));
                if (journalParamValuesCollection is null)
                {
                    journalParamValuesCollection = new()
                    {
                        Name = paramName
                    };
                    pcObject.JournalParamValuesCollections!.Add(journalParamValuesCollection);
                }

                DateTime timestampUtc = new Any(line[3]).ValueAs<DateTime>(false);
                if (timestampUtc == default(DateTime))
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.InvalidDateTimeString);
                    return null;
                }
                
                var value = Any.ConvertToBestType(line[4], false);
                if (journalParamValuesCollection.FloatValues is null)
                    journalParamValuesCollection.FloatValues = new();
                journalParamValuesCollection.FloatValues.Add(new Serialization.FloatJournalParamValue
                {
                    TimestampUtc = timestampUtc,
                    Value = value.ValueAsSingle(false)
                });
                //switch (AnyHelper.GetTransportType(value))
                //{
                //    case TransportType.Double:
                //        if (journalParamValuesCollection.FloatValues is null)
                //            journalParamValuesCollection.FloatValues = new();
                //        journalParamValuesCollection.FloatValues.Add(new Serialization.FloatJournalParamValue
                //        {
                //            TimestampUtc = timestampUtc,
                //            Value = value.ValueAsSingle(false)
                //        });
                //        break;
                //    case TransportType.UInt32:
                //        if (journalParamValuesCollection.Int32Values is null)
                //            journalParamValuesCollection.Int32Values = new();
                //        journalParamValuesCollection.Int32Values.Add(new Serialization.Int32JournalParamValue
                //        {
                //            TimestampUtc = timestampUtc,
                //            Value = value.ValueAsInt32(false)
                //        });
                //        break;
                //    case TransportType.Object:
                //        if (journalParamValuesCollection.StringValues is null)
                //            journalParamValuesCollection.StringValues = new();
                //        journalParamValuesCollection.StringValues.Add(new Serialization.StringJournalParamValue
                //        {
                //            TimestampUtc = timestampUtc,
                //            Value = value.ValueAsString(false)
                //        });
                //        break;
                //    default:
                //        throw new InvalidOperationException();
                //}
            }

            serializationRootObject.PcObjects = pcObjects.Values.ToList();

            return serializationRootObject;
        }

        /// <summary>
        ///     По завершению jobProgress должен стать 100% или перейти в статус ошибки, если нужно прекратить всю комплексную операцию. 
        ///     Precondition: Main thread.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="user"></param>
        /// <param name="entityType"></param>
        /// <param name="addon"></param>
        /// <param name="sourceTypeIdentifier"></param>
        /// <param name="fileName"></param>
        /// <param name="importStream"></param>
        /// <param name="projectId"></param>        
        /// <param name="cancellationToken"></param>
        /// <param name="serializationRootObject"></param>
        /// <param name="jobProgress"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="loggersSet"></param>
        /// <param name="result"></param>
        /// <param name="preview"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task<FileContentType> ImportAddonFileAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            string user,
            Type entityType,             
            AddonBase addon, 
            string sourceTypeIdentifier, 
            string fileName, 
            Stream importStream,
            int? projectId,            
            CancellationToken cancellationToken, 
            Serialization.SerializationRootObject serializationRootObject,            
            IJobProgress jobProgress,
            IMainServerWorker mainServerWorker,
            ILoggersSet loggersSet,
            Serialization.ImportSerializationRootObjectResult result,
            bool preview)
        {
            FileContentType contentType;

            if (entityType == typeof(Tag))
            {
                contentType = new FileContentType() { Id = @"TAGs", Desc = Properties.Resources.Tags };

                try
                {
                    ((TagsImportAddonBase)addon).ImportTagsFile(sourceTypeIdentifier, fileName, importStream, cancellationToken, serializationRootObject, loggersSet);
                    await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.Good);
                }
                catch
                {
                    await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                }                

                return contentType;
            }

            if (entityType == typeof(CeMatrix))
            {
                contentType = new FileContentType() { Id = @"CeMatrices", Desc = Properties.Resources.CeMatrices };                

                try
                {
                    bool allFailed = true;

                    List<string> ceMatrixStdFileContents = await ((CustomImportExportAddonBase)addon).ImportCeMatrixAsync(sourceTypeIdentifier, fileName, importStream, cancellationToken, loggersSet);
                    int i = 0;
                    foreach (var ceMatrixStdFileContent in ceMatrixStdFileContents)
                    {
                        var childJobProgress = await jobProgress.GetChildJobProgressAsync((uint)(100.0 * i / ceMatrixStdFileContents.Count), (uint)(100.0 * (i + 1) / ceMatrixStdFileContents.Count), parentFailedIfFailed: false);

                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ceMatrixStdFileContent)))
                        {
                            await ImportStdFileAsync(
                                dbContextFactory,
                                user,
                                stream,                                
                                projectId,                                
                                cancellationToken,
                                childJobProgress,
                                mainServerWorker,
                                loggersSet,
                                result,
                                preview);
                        }

                        if (Ssz.Utils.StatusCodes.IsGood(childJobProgress.StatusCode))
                            allFailed = false;

                        i += 1;
                    }

                    if (allFailed)
                        await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                    else
                        await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.Good);
                }
                catch
                {
                    await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);
                }

                return contentType;
            }

            await jobProgress.SetJobProgressAsync(100, null, null, Ssz.Utils.StatusCodes.BadInvalidArgument);

            throw new InvalidOperationException();
        }

        private static string GetDocumentMetadataValue(string?[] firstLineValues, string name)
        {
            int i = Array.FindIndex(firstLineValues, v => String.Equals(v, name, StringComparison.InvariantCultureIgnoreCase));
            if (i >= 0 && i < firstLineValues.Length - 1)
                return firstLineValues[i + 1] ?? @"";
            else
                return @"";
        }        

        private static void UpdateParams<TParam>(
            PazCheckDbContext dbContext, 
            List<TParam> params_, 
            List<Serialization.Param>? serializationParams,
            ReferenceEntities referenceEntities,
            Serialization.CollectionMode collectionMode,             
            ILoggersSet loggersSet)
                where TParam : VersionedParamBase, new()
        {
            if (serializationParams is null)
                return;

            HashSet<string> serializationParamsHashSet = new(StringComparer.InvariantCultureIgnoreCase);            

            List<TParam> originalParams = params_.Where(e => e._IsDeleted == false).ToList();                
            
            foreach (var serializationParam in serializationParams)
            {
                using var paramNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(("ParamName", serializationParam.Name));

                if (!serializationParamsHashSet.Add(serializationParam.Name))
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                    throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                }                

                TParam? param_ = params_.FirstOrDefault(p => String.Equals(p.ParamName, serializationParam.Name, StringComparison.InvariantCultureIgnoreCase) && 
                    p._IsDeleted == false);
                if (param_ is null)
                {
                    param_ = new()
                    {                            
                        ParamName = serializationParam.Name,
                        Value = serializationParam.Value                                                    
                    };
                    params_.Add(param_);
                }
                else
                {
                    originalParams.Remove(param_);

                    if (param_.Value != serializationParam.Value)
                    {           
                        if (param_._CreateProjectVersionNum is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.Error_ImportSerializationRootObject_ParamWithUnversionedChangesExists);

                            param_.ParamName = serializationParam.Name; // For case-sensivity issues.
                            param_.Value = serializationParam.Value;
                        }
                        else
                        {
                            PazCheckDbHelper.SetIsDeleted(dbContext, param_);
                            param_ = new()
                            {
                                ParamName = serializationParam.Name,
                                Value = serializationParam.Value
                            };
                            params_.Add(param_);
                        }                            
                    }
                    else
                    {
                        param_.ParamName = serializationParam.Name; // For case-sensivity issues.
                    }
                }
            }

            if (collectionMode == Serialization.CollectionMode.Replace)
                foreach (var originalParam in originalParams)
                {
                    PazCheckDbHelper.SetIsDeleted(dbContext, originalParam);
                }
        }

        private static void UpdateTagConditions(
            PazCheckDbContext dbContext, 
            List<TagCondition> tagConditions, 
            List<Serialization.TagCondition>? serializationTagConditions,
            ReferenceEntities referenceEntities,
            Serialization.CollectionMode collectionMode,             
            ILoggersSet loggersSet)
        {
            if (serializationTagConditions is null)
                return;

            HashSet<string> serializationTagConditionsHashSet = new();

            List<TagCondition> originalTagConditions = tagConditions.Where(e => e._IsDeleted == false).ToList();            
            
            foreach (var serializationTagCondition in serializationTagConditions)
            {
                string tagConditionIdentifier = StringHelper.JoinNotNullOrEmpty(",", serializationTagCondition.ConditionCategory, serializationTagCondition.AeCondition, serializationTagCondition.DaCondition, serializationTagCondition.SymbolToDisplay);
                using var tagConditionScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope(("TagCondition", tagConditionIdentifier));

                if (!serializationTagConditionsHashSet.Add(tagConditionIdentifier))
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                    throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                }               

                TagCondition? tagCondition = tagConditions.FirstOrDefault(p => p.ConditionCategory == (serializationTagCondition.ConditionCategory ?? @"") &&
                    p.AeCondition == (serializationTagCondition.AeCondition ?? @"") &&
                    p.DaCondition == (serializationTagCondition.DaCondition ?? @"") &&
                    p.SymbolToDisplay == serializationTagCondition.SymbolToDisplay &&
                    p._IsDeleted == false);
                if (tagCondition is null)
                {
                    tagCondition = new()
                    {                            
                        AeCondition = serializationTagCondition.AeCondition ?? @"",
                        DaCondition = serializationTagCondition.DaCondition ?? @"",
                        ConditionCategory = serializationTagCondition.ConditionCategory ?? @"",
                        SymbolToDisplay = serializationTagCondition.SymbolToDisplay,
                        CanBeCause = serializationTagCondition.CanBeCause,
                        CanBeEffect = serializationTagCondition.CanBeEffect                                                  
                    };
                    tagConditions.Add(tagCondition);
                }
                else
                {
                    originalTagConditions.Remove(tagCondition);

                    if (tagCondition.CanBeCause != serializationTagCondition.CanBeCause || 
                        tagCondition.CanBeEffect != serializationTagCondition.CanBeEffect)
                    {
                        if (tagCondition._CreateProjectVersionNum is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.Error_ImportSerializationRootObject_TagConditionWithUnversionedChangesExists);
                        }

                        PazCheckDbHelper.SetIsDeleted(dbContext, tagCondition);                                        
                        tagCondition = new()
                        {                                
                            AeCondition = serializationTagCondition.AeCondition ?? @"",
                            DaCondition = serializationTagCondition.DaCondition ?? @"",
                            ConditionCategory = serializationTagCondition.ConditionCategory ?? @"",
                            SymbolToDisplay = serializationTagCondition.SymbolToDisplay,
                            CanBeCause = serializationTagCondition.CanBeCause,
                            CanBeEffect = serializationTagCondition.CanBeEffect                                                             
                        };
                        tagConditions.Add(tagCondition);
                    }                        
                }
            }

            if (collectionMode == Serialization.CollectionMode.Replace)
                foreach (var originalTagCondition in originalTagConditions)
                {
                    PazCheckDbHelper.SetIsDeleted(dbContext, originalTagCondition);                    
                }
        }

        private static void UpdateDbFileReferences<TDbFileReference>(
            PazCheckDbContext dbContext,
            List<TDbFileReference> dbFileReferences,
            List<Serialization.DbFileReference>? serializationDbFileReferences,
            ReferenceEntities referenceEntities,
            Serialization.CollectionMode collectionMode,             
            ILoggersSet loggersSet)
            where TDbFileReference : VersionDbFileReference, new()
        {
            if (serializationDbFileReferences is null)
                return;

            HashSet<string> serializationDbFileReferencesHashSet = new();

            List<TDbFileReference> originalDbFileReferences = dbFileReferences.Where(e => e._IsDeleted == false).ToList();
            
            foreach (var serializationDbFileReference in serializationDbFileReferences)
            {
                using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope("File: " + serializationDbFileReference.Path + "/" + serializationDbFileReference.Name);

                if (!serializationDbFileReferencesHashSet.Add(serializationDbFileReference.Path + "/" + serializationDbFileReference.Name))
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                    throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                }

                DbFile? dbFile = GetDbFile(serializationDbFileReference, referenceEntities, loggersSet);                    

                TDbFileReference? dbFileReference = dbFileReferences.FirstOrDefault(fr => fr.Path + "/" + fr.Name == serializationDbFileReference.Path + "/" + serializationDbFileReference.Name && fr._IsDeleted == false);
                if (dbFileReference is null)
                {   
                    dbFileReference = new()
                    {                            
                        Name = serializationDbFileReference.Name,
                        Path = serializationDbFileReference.Path ?? @"",
                        Tags = serializationDbFileReference.Tags ?? @"",
                        LastWriteTimeUtc = serializationDbFileReference.LastWriteTimeUtc,
                        BytesCount = serializationDbFileReference.BytesCount,
                        FileBytesHash_Base64 = serializationDbFileReference.FileBytesHash_Base64,
                        DbFile = dbFile,                            
                    };
                    dbFileReferences.Add(dbFileReference);
                }
                else
                {
                    originalDbFileReferences.Remove(dbFileReference);                    

                    if (dbFileReference.Tags != serializationDbFileReference.Tags ||
                        dbFileReference.FileBytesHash_Base64 != serializationDbFileReference.FileBytesHash_Base64)
                    {
                        if (dbFileReference._CreateProjectVersionNum is null)
                        {
                            loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.Error_ImportSerializationRootObject_DbFileReferenceWithUnversionedChangesExists);
                        }

                        DeleteDbFileReference(dbContext, dbFileReference, loggersSet);
                        dbFileReference = new()
                        {
                            Name = serializationDbFileReference.Name,
                            Path = serializationDbFileReference.Path ?? @"",
                            Tags = serializationDbFileReference.Tags ?? @"",
                            LastWriteTimeUtc = serializationDbFileReference.LastWriteTimeUtc,
                            BytesCount = serializationDbFileReference.BytesCount,
                            FileBytesHash_Base64 = serializationDbFileReference.FileBytesHash_Base64,
                            DbFile = dbFile,
                        };
                        dbFileReferences.Add(dbFileReference);
                    }
                }
            }

            if (collectionMode == Serialization.CollectionMode.Replace)
                foreach (var originalDbFileReference in originalDbFileReferences)
                {
                    DeleteDbFileReference(dbContext, originalDbFileReference, loggersSet);
                }
        }

        private static DbFile? GetDbFile(
            Serialization.DbFileReference? serializationDbFileReference,
            ReferenceEntities referenceEntities, ILoggersSet loggersSet)
        {
            if (serializationDbFileReference is null || serializationDbFileReference.FileBytesHash_Base64 is null)
                return null;

            referenceEntities.DbFiles.TryGetValue(serializationDbFileReference.FileBytesHash_Base64, out DbFile? dbFile);
            if (dbFile is null)
            {
                using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"FileBytesHash_Base64", serializationDbFileReference.FileBytesHash_Base64));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.FileReferenceError);
            }
            return dbFile;
        }

        /// <summary>
        ///     TDbFileReference.DbFile must be included
        /// </summary>
        /// <typeparam name="TDbFileReference"></typeparam>
        /// <param name="dbFileReferences"></param>
        /// <param name="serializationDbFileReferences"></param>
        /// <param name="referenceEntities"></param>
        /// <param name="dbContext"></param>
        /// <param name="collectionMode"></param>
        /// <param name="loggersSet"></param>
        /// <exception cref="Exception"></exception>
        private static void SetDbFileReferences<TDbFileReference>(
            List<TDbFileReference> dbFileReferences, 
            List<Serialization.DbFileReference>? serializationDbFileReferences, 
            ReferenceEntities referenceEntities,
            PazCheckDbContext dbContext,
            Serialization.CollectionMode collectionMode,
            ILoggersSet loggersSet)
            where TDbFileReference: DbFileReference, new()
        {
            if (serializationDbFileReferences is null)
                return;

            List<TDbFileReference>? originalDbFileReferences = new List<TDbFileReference>(dbFileReferences);

            foreach (var serializationDbFileReference in serializationDbFileReferences)
            {
                TDbFileReference? dbFileReference = dbFileReferences
                    .Find(fr => fr.Path + "/" + fr.Name == serializationDbFileReference.Path + "/" + serializationDbFileReference.Name);
                if (dbFileReference is null)
                {
                    DbFile? dbFile = GetDbFile(serializationDbFileReference, referenceEntities, loggersSet);
                    dbFileReference = new TDbFileReference
                    {
                        Name = serializationDbFileReference.Name,
                        Path = serializationDbFileReference.Path ?? @"",
                        Tags = serializationDbFileReference.Tags ?? @"",
                        LastWriteTimeUtc = serializationDbFileReference.LastWriteTimeUtc,
                        BytesCount = serializationDbFileReference.BytesCount,
                        FileBytesHash_Base64 = serializationDbFileReference.FileBytesHash_Base64,
                        DbFile = dbFile,
                    };
                    dbFileReferences.Add(dbFileReference);
                }
                else
                {
                    if (!originalDbFileReferences.Remove(dbFileReference))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"FileFullName", serializationDbFileReference.Path + "/" + serializationDbFileReference.Name));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                        throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                    }                        
                    DbFile? dbFile = GetDbFile(serializationDbFileReference, referenceEntities, loggersSet);
                    dbFileReference.Name = serializationDbFileReference.Name;
                    dbFileReference.Path = serializationDbFileReference.Path ?? @"";
                    dbFileReference.Tags = serializationDbFileReference.Tags ?? @"";
                    dbFileReference.LastWriteTimeUtc = serializationDbFileReference.LastWriteTimeUtc;
                    dbFileReference.BytesCount = serializationDbFileReference.BytesCount;
                    dbFileReference.FileBytesHash_Base64 = serializationDbFileReference.FileBytesHash_Base64;
                    dbFileReference.DbFile = dbFile;
                }
            }

            if (collectionMode == Serialization.CollectionMode.Replace)
                PazCheckDbHelper.SafeRemoveRange(dbContext, originalDbFileReferences);
        }

        private static void DeleteDbFileReference<TDbFileReference>(PazCheckDbContext dbContext, TDbFileReference dbFileReference, ILoggersSet loggersSet)
            where TDbFileReference : VersionDbFileReference, new()
        {
            PazCheckDbHelper.SetIsDeleted(dbContext, dbFileReference);
            //try
            //{
            //    dbContext.DbFiles.Single(f => f.Id == dbFileReference.DbFileId).FileDeletionTimeUtc = DateTime.UtcNow;
            //}
            //catch (Exception ex)
            //{
            //    loggersSet.Logger.LogError(ex, "Invalid DbFileReference.DbFileId. FileName: " + dbFileReference.FileName);
            //}
        }

        private static string GetParamsString(string paramsString, List<Serialization.Param>? serializationParams, Serialization.CollectionMode collectionMode, Action<string, string?, IUserFriendlyLogger>? validateParamFunc, ILoggersSet loggersSet)
        {
            if (serializationParams is null)
                return paramsString;

            CaseInsensitiveOrderedDictionary<string?> serializationParamsDictionary = new();
            foreach (var serializationParam in serializationParams)
            {
                if (serializationParam.Name.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"ParamName", serializationParam.Name));

                if (validateParamFunc is not null)
                    validateParamFunc(serializationParam.Name, serializationParam.Value, loggersSet.UserFriendlyLogger);
                if (!serializationParamsDictionary.TryAdd(serializationParam.Name, serializationParam.Value))
                {                    
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                    throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                }
            }

            switch (collectionMode)
            {
                case Serialization.CollectionMode.AddToEmpty:
                case Serialization.CollectionMode.Replace:                    
                    return NameValueCollectionHelper.GetNameValueCollectionString(serializationParamsDictionary);                    
                case Serialization.CollectionMode.Update:
                    var params_ = NameValueCollectionHelper.Parse(paramsString);
                    foreach (var kvp in serializationParamsDictionary) 
                    {
                        params_[kvp.Key] = kvp.Value;
                    }
                    return NameValueCollectionHelper.GetNameValueCollectionString(params_);
            }
            throw new InvalidOperationException();
        }

        private static void SetPcObjectEventTypes(
            List<PcObjectEventType> pcObjectEventTypes, 
            List<string>? serializationPcObjectEventTypes, 
            ReferenceEntities referenceEntities,
            PazCheckDbContext dbContext,
            Serialization.CollectionMode childCollectionMode,
            ILoggersSet loggersSet)
        {
            if (serializationPcObjectEventTypes is null)
                return;

            List<PcObjectEventType>? originalPcObjectEventTypes = new List<PcObjectEventType>(pcObjectEventTypes);

            foreach (var serializationPcObjectEventType in serializationPcObjectEventTypes)
            {
                PcObjectEventType? pcObjectEventType = pcObjectEventTypes
                    .Find(et => String.Equals(et.Type, serializationPcObjectEventType, StringComparison.InvariantCultureIgnoreCase));
                if (pcObjectEventType is null)
                {
                    if (!referenceEntities.PcObjectEventTypes.TryGetValue(serializationPcObjectEventType, out pcObjectEventType))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObjectEventType, serializationPcObjectEventType));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid);
                        throw new Exception(loggersSet.LoggerAndUserFriendlyLogger.GetScopesString() + " " + Properties.Resources.Error_IdentifierInvalid);
                    }
                    pcObjectEventTypes.Add(pcObjectEventType);
                }
                else
                {
                    if (!originalPcObjectEventTypes.Remove(pcObjectEventType))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObjectEventType, serializationPcObjectEventType));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                        throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                    }                    
                }
            }

            if (childCollectionMode == Serialization.CollectionMode.Replace)
                PazCheckDbHelper.SafeRemoveRange(dbContext, originalPcObjectEventTypes);            
        }

        private static bool AddTypes<TType, TSerializationType>(Dictionary<string, TType> types, DbSet<TType> dbSet, List<TSerializationType>? serializationTypes, ReferenceEntities referenceEntities, ILoggersSet loggersSet)
            where TType : PcEntityType, new()
            where TSerializationType : Serialization.PcEntityType
        {
            if (serializationTypes is null)
                return false;

            bool changed = false;
            foreach (var serializationType in serializationTypes)
            {                
                types.TryGetValue(serializationType.Type, out TType? type_);
                if (type_ is null)
                {
                    type_ = new()
                    {
                        Type = serializationType.Type,
                        Title = serializationType.Title,
                        Desc = serializationType.Desc,
                        IconDbFile = GetDbFile(serializationType.IconDbFileReference, referenceEntities, loggersSet),
                    };
                    dbSet.Add(type_);
                    types.Add(type_.Type, type_);
                }
                else
                {
                    type_.Type = serializationType.Type;
                    type_.Title = serializationType.Title;
                    type_.Desc = serializationType.Desc;
                    // TODO remove old file
                    type_.IconDbFile = GetDbFile(serializationType.IconDbFileReference, referenceEntities, loggersSet);
                    type_.StandardParamInfos.Clear();
                }
                AddStandardParamInfos(type_.StandardParamInfos, serializationType.StandardParamInfos, referenceEntities, loggersSet);
                changed = true;
            }
            return changed;
        }

        private static void AddStandardParamInfos(List<ParamInfo> standardParamInfos, List<Serialization.ParamInfo>? serializationStandardParamInfos,
            ReferenceEntities referenceEntities, ILoggersSet loggersSet)
        {
            if (serializationStandardParamInfos is null)
                return;

            foreach (var serializationStandardParamInfo in serializationStandardParamInfos)
            {
                if (!String.IsNullOrEmpty(serializationStandardParamInfo.Name))
                {
                    standardParamInfos.Add(new ParamInfo
                    {
                        ParamName = serializationStandardParamInfo.Name,
                        DefaultValue = serializationStandardParamInfo.DefaultValue,
                        MetadataFields = serializationStandardParamInfo.MetadataFields
                    });
                }
            }
        }

        private static void SetBasePcObjectJournalParams(
            List<BasePcObjectJournalParam> journalParams, 
            List<Serialization.Param>? serializationParams,
            PazCheckDbContext dbContext,
            Serialization.CollectionMode childCollectionMode,
            ILoggersSet loggersSet)
        {
            if (serializationParams is null)
                return;

            List<BasePcObjectJournalParam>? originalJournalParams = new(journalParams);

            foreach (var serializationParam in serializationParams)
            {
                if (!serializationParam.Name.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                BasePcObjectJournalParam? journalParam = journalParams.Find(jp => String.Equals(jp.ParamName, serializationParam.Name, StringComparison.InvariantCultureIgnoreCase));
                if (journalParam is null)
                {
                    journalParam = new()
                    {
                        ParamName = serializationParam.Name,
                    };
                    journalParam.MetadataFields = serializationParam.Value ?? @"";
                    journalParams.Add(journalParam);
                }
                else
                {
                    if (!originalJournalParams.Remove(journalParam))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"ParamName", serializationParam.Name));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                        throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                    }
                    journalParam.ParamName = serializationParam.Name; // For case sensivity                    
                    journalParam.MetadataFields = serializationParam.Value ?? @"";
                }
            }

            if (childCollectionMode == Serialization.CollectionMode.Replace)
                PazCheckDbHelper.SafeRemoveRange(dbContext, originalJournalParams);
        }

        private static void SetPcObjectJournalParams(
            List<PcObjectJournalParam> journalParams, 
            List<Serialization.Param>? serializationParams,
            PazCheckDbContext dbContext,            
            Serialization.CollectionMode childCollectionMode,            
            ILoggersSet loggersSet)
        {
            if (serializationParams is null)
                return;

            List<PcObjectJournalParam>? originalJournalParams = new(journalParams);

            foreach (var serializationParam in serializationParams)
            {
                if (!serializationParam.Name.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                PcObjectJournalParam? journalParam = journalParams.Find(jp => String.Equals(jp.ParamName, serializationParam.Name, StringComparison.InvariantCultureIgnoreCase));
                if (journalParam is null)
                {
                    journalParam = new()
                    {
                        ParamName = serializationParam.Name,                        
                    };                    
                    journalParam.MetadataFields = serializationParam.Value ?? @"";                    
                    journalParams.Add(journalParam);
                }
                else
                {
                    if (!originalJournalParams.Remove(journalParam))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"ParamName", serializationParam.Name));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                        throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                    }                        
                    journalParam.ParamName = serializationParam.Name; // For case sensivity                    
                    journalParam.MetadataFields = serializationParam.Value ?? @"";                                        
                }
            }

            if (childCollectionMode == Serialization.CollectionMode.Replace)
                PazCheckDbHelper.SafeRemoveRange(dbContext, originalJournalParams);            
        }

        private static async Task SetJournalParamValuesCollectionsAsync(
            List<JournalParamValuesCollection> journalParamValuesCollections, 
            List<Serialization.JournalParamValuesCollection>? serializationJournalParamValuesCollections,
            PazCheckDbContext dbContext,            
            Serialization.CollectionMode childCollectionMode,
            Serialization.CollectionMode dataCollectionMode,
            ILoggersSet loggersSet)
        {
            if (serializationJournalParamValuesCollections is null)
                return;

            List<JournalParamValuesCollection>? originalJournalParamValuesCollections = new(journalParamValuesCollections);

            foreach (var serializationJournalParamValuesCollection in serializationJournalParamValuesCollections)
            {
                JournalParamValuesCollection? journalParamValuesCollection = journalParamValuesCollections.Find(jp => String.Equals(jp.ParamName, serializationJournalParamValuesCollection.Name, StringComparison.InvariantCultureIgnoreCase));
                if (journalParamValuesCollection is null)
                {
                    journalParamValuesCollection = new()
                    {
                        ParamName = serializationJournalParamValuesCollection.Name,                        
                    };                    
                    journalParamValuesCollection.MetadataFields = serializationJournalParamValuesCollection.MetadataFields ?? @"";
                    await AddValuesAsync(journalParamValuesCollection, serializationJournalParamValuesCollection, Serialization.CollectionMode.AddToEmpty, dbContext, loggersSet);
                    journalParamValuesCollections.Add(journalParamValuesCollection);
                }
                else
                {
                    if (!originalJournalParamValuesCollections.Remove(journalParamValuesCollection))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"ParamName", serializationJournalParamValuesCollection.Name));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierDuplicated);
                        throw new Exception(Properties.Resources.Error_IdentifierDuplicated);
                    }                        
                    journalParamValuesCollection.ParamName = serializationJournalParamValuesCollection.Name; // For case sensivity                    
                    if (serializationJournalParamValuesCollection.MetadataFields is not null)
                        journalParamValuesCollection.MetadataFields = serializationJournalParamValuesCollection.MetadataFields;                    
                    await AddValuesAsync(journalParamValuesCollection, serializationJournalParamValuesCollection, dataCollectionMode, dbContext, loggersSet);
                }
            }

            if (childCollectionMode == Serialization.CollectionMode.Replace)
                PazCheckDbHelper.SafeRemoveRange(dbContext, originalJournalParamValuesCollections);
        }

        private static async Task AddValuesAsync(JournalParamValuesCollection journalParamValuesCollection, Serialization.JournalParamValuesCollection serializationJournalParamValuesCollection, Serialization.CollectionMode dataCollectionMode, PazCheckDbContext dbContext, ILoggersSet loggersSet)
        {
            if (dataCollectionMode == Serialization.CollectionMode.Replace)
            {
                await dbContext.FloatJournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection).ExecuteDeleteAsync();
                //await dbContext.Int32JournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection).ExecuteDeleteAsync();
                //await dbContext.StringJournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection).ExecuteDeleteAsync();
            }
            else if (dataCollectionMode == Serialization.CollectionMode.Update)
            {
                DateTime minTimestampUtc = DateTime.UtcNow;
                DateTime maxTimestampUtc = DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime;                
                if (serializationJournalParamValuesCollection.FloatValues is not null && serializationJournalParamValuesCollection.FloatValues.Count > 0)
                {
                    DateTime serializationMinTimestampUtc = serializationJournalParamValuesCollection.FloatValues.Min(v => v.TimestampUtc);
                    if (serializationMinTimestampUtc < minTimestampUtc)
                        minTimestampUtc = serializationMinTimestampUtc;
                    DateTime serializationMaxTimestampUtc = serializationJournalParamValuesCollection.FloatValues.Max(v => v.TimestampUtc);
                    if (serializationMaxTimestampUtc > maxTimestampUtc)
                        maxTimestampUtc = serializationMaxTimestampUtc;
                }
                //if (serializationJournalParamValuesCollection.Int32Values is not null && serializationJournalParamValuesCollection.Int32Values.Count > 0)
                //{
                //    DateTime serializationMinTimestampUtc = serializationJournalParamValuesCollection.Int32Values.Min(v => v.TimestampUtc);
                //    if (serializationMinTimestampUtc < minTimestampUtc)
                //        minTimestampUtc = serializationMinTimestampUtc;
                //    DateTime serializationMaxTimestampUtc = serializationJournalParamValuesCollection.Int32Values.Max(v => v.TimestampUtc);
                //    if (serializationMaxTimestampUtc > maxTimestampUtc)
                //        maxTimestampUtc = serializationMaxTimestampUtc;
                //}
                //if (serializationJournalParamValuesCollection.StringValues is not null && serializationJournalParamValuesCollection.StringValues.Count > 0)
                //{
                //    DateTime serializationMinTimestampUtc = serializationJournalParamValuesCollection.StringValues.Min(v => v.TimestampUtc);
                //    if (serializationMinTimestampUtc < minTimestampUtc)
                //        minTimestampUtc = serializationMinTimestampUtc;
                //    DateTime serializationMaxTimestampUtc = serializationJournalParamValuesCollection.StringValues.Max(v => v.TimestampUtc);
                //    if (serializationMaxTimestampUtc > maxTimestampUtc)
                //        maxTimestampUtc = serializationMaxTimestampUtc;
                //}                
                long beginTimeUtc_Optimized = new DateTimeOffset(minTimestampUtc).ToUnixTimeMilliseconds();
                long endTimeUtc_Optimized = new DateTimeOffset(maxTimestampUtc).ToUnixTimeMilliseconds();
                if (beginTimeUtc_Optimized <= endTimeUtc_Optimized)
                {
                    await dbContext.FloatJournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection &&
                        jp.TimestampUtc >= beginTimeUtc_Optimized &&
                        jp.TimestampUtc <= endTimeUtc_Optimized).ExecuteDeleteAsync();
                    //await dbContext.Int32JournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection &&
                    //    jp.TimestampUtc >= minTimestampUtc &&
                    //    jp.TimestampUtc <= maxTimestampUtc).ExecuteDeleteAsync();
                    //await dbContext.StringJournalParamValues.Where(jp => jp.JournalParamValuesCollection == journalParamValuesCollection &&
                    //    jp.TimestampUtc >= minTimestampUtc &&
                    //    jp.TimestampUtc <= maxTimestampUtc).ExecuteDeleteAsync();
                }                
            }

            if (serializationJournalParamValuesCollection.FloatValues is not null)
                foreach (var serializationFloatValue in serializationJournalParamValuesCollection.FloatValues)
                {
                    journalParamValuesCollection.FloatValues.Add(
                            new FloatJournalParamValue 
                            { 
                                TimestampUtc = new DateTimeOffset(serializationFloatValue.TimestampUtc).ToUnixTimeMilliseconds(), 
                                Value = serializationFloatValue.Value 
                            });
                }

            //if (serializationJournalParamValuesCollection.Int32Values is not null)
            //    foreach (var serializationInt32Value in serializationJournalParamValuesCollection.Int32Values)
            //    {
            //        journalParamValuesCollection.Int32Values.Add(
            //                new Int32JournalParamValue { TimestampUtc = serializationInt32Value.TimestampUtc, Value = serializationInt32Value.Value });
            //    }

            //if (serializationJournalParamValuesCollection.StringValues is not null)
            //    foreach (var serializationStringValue in serializationJournalParamValuesCollection.StringValues)
            //    {
            //        journalParamValuesCollection.StringValues.Add(
            //                new StringJournalParamValue { TimestampUtc = serializationStringValue.TimestampUtc, Value = serializationStringValue.Value });
            //    }
        }

        private static async Task AddPcObjectEventsAsync(
            PcObject pcObject, 
            List<Serialization.PcObjectEvent>? serializationPcObjectEvents,
            ReferenceEntities referenceEntities, 
            PazCheckDbContext dbContext,
            Serialization.CollectionMode childCollectionMode,
            Serialization.CollectionMode dataCollectionMode,
            ILoggersSet loggersSet)
        {
            if (serializationPcObjectEvents is null)
                return;

            DateTime minBeginDateTime = serializationPcObjectEvents.Min(e => new Any(e.BeginTimeUtc).ValueAs<DateTime>(false)) - TimeSpan.FromMilliseconds(1);

            var pcObjectEvents = await dbContext.PcObjectEvents
                .Where(e => e._IsDeleted == false && e.PcObject.Id == pcObject.Id && e.BeginTimeUtc >= minBeginDateTime)                
                .ToListAsync();

            List<PcObjectEvent> originalPcObjectEvents = new(pcObjectEvents);

            foreach (var serializationPcObjectEvent in serializationPcObjectEvents)
            {
                PcObjectEventType? pcObjectEventType;
                if (serializationPcObjectEvent.EventType is null)
                {
                    pcObjectEventType = null;
                }
                else
                {
                    referenceEntities.PcObjectEventTypes.TryGetValue(serializationPcObjectEvent.EventType, out pcObjectEventType);
                    if (pcObjectEventType is null)
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObjectEventType, serializationPcObjectEvent.EventType));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid);
                        throw new Exception(loggersSet.LoggerAndUserFriendlyLogger.GetScopesString() + " " + Properties.Resources.Error_IdentifierInvalid);
                    }                    
                }
                DateTime beginTimeUtc = new Any(serializationPcObjectEvent.BeginTimeUtc).ValueAs<DateTime>(false);
                if (beginTimeUtc == default(DateTime))
                {
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"BeginTimeUtc", serializationPcObjectEvent.BeginTimeUtc));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.InvalidDateTimeString);
                    throw new Exception(Properties.Resources.InvalidDateTimeString);
                }
                DateTime? endTimeUtc;
                if (String.IsNullOrEmpty(serializationPcObjectEvent.EndTimeUtc))
                {
                    endTimeUtc = null;
                }
                else
                {
                    endTimeUtc = new Any(serializationPcObjectEvent.EndTimeUtc).ValueAs<DateTime>(false);
                    if (endTimeUtc == default(DateTime))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((@"EndTimeUtc", serializationPcObjectEvent.EndTimeUtc));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.InvalidDateTimeString);
                        throw new Exception(Properties.Resources.InvalidDateTimeString);
                    }
                }
                PcObjectEvent? pcObjectEvent = pcObjectEvents.FirstOrDefault(e => e.BeginTimeUtc >= beginTimeUtc - TimeSpan.FromMilliseconds(1) &&
                     e.BeginTimeUtc <= beginTimeUtc + TimeSpan.FromMilliseconds(1) &&
                     String.Equals(e.PcObjectEventType, serializationPcObjectEvent.EventType, StringComparison.InvariantCultureIgnoreCase));
                if (pcObjectEvent is null)
                {
                    if (pcObjectEventType is null)
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.PcObjectEventType, serializationPcObjectEvent.EventType));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Properties.Resources.Error_IdentifierInvalid);
                        throw new Exception(loggersSet.LoggerAndUserFriendlyLogger.GetScopesString() + " " + Properties.Resources.Error_IdentifierInvalid);
                    }
                    pcObjectEvent = new()
                    {
                        BeginTimeUtc = beginTimeUtc,
                        EndTimeUtc = endTimeUtc,                        
                        PcObjectEventType = pcObjectEventType.Type
                    };
                    pcObjectEvent.Params = GetParamsString(pcObjectEvent.Params, serializationPcObjectEvent.EventParams, Serialization.CollectionMode.AddToEmpty, null, loggersSet);
                    SetDbFileReferences(pcObjectEvent.PcObjectEventDbFileReferences, serializationPcObjectEvent.PcObjectEventDbFileReferences, referenceEntities, dbContext, Serialization.CollectionMode.AddToEmpty, loggersSet);

                    pcObjectEvent.PcObject = pcObject; // Optimization
                    pcObject.PcObjectEvents.Add(pcObjectEvent);
                }
                else
                {
                    originalPcObjectEvents.Remove(pcObjectEvent);

                    pcObjectEvent.BeginTimeUtc = beginTimeUtc;
                    pcObjectEvent.EndTimeUtc = endTimeUtc;                    
                    pcObjectEvent.Params = GetParamsString(pcObjectEvent.Params, serializationPcObjectEvent.EventParams, childCollectionMode, null, loggersSet);
                    SetDbFileReferences(pcObjectEvent.PcObjectEventDbFileReferences, serializationPcObjectEvent.PcObjectEventDbFileReferences, referenceEntities, dbContext, childCollectionMode, loggersSet);
                }                
            }

            if (dataCollectionMode == Serialization.CollectionMode.Replace)
            {                
                await dbContext.PcObjectEvents
                    .Where(e => e._IsDeleted == false && e.Id == pcObject.Id && e.BeginTimeUtc < minBeginDateTime)
                    .ExecuteUpdateAsync(e => e.SetProperty(e => e._IsDeleted, e => true));
                
                foreach (var originalPcObjectEvent in originalPcObjectEvents)
                {
                    originalPcObjectEvent._IsDeleted = true;
                }
            }
        }

        /// <summary>
        ///     [List Property Name, List_PropertyDesc]
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        private static Dictionary<string, List_PropertyDesc> GetList_PropertyDescs(Type type, bool includeChildren)
        {
            return type.GetProperties().Where(pi => pi.PropertyType.Name == @"List`1")
                .Select(pi => new List_PropertyDesc
                {
                    List_PropertyInfo = pi,
                    ListElement_Type = pi.PropertyType.GetGenericArguments()[0],
                    ChildList_PropertyDescs = includeChildren ? GetList_PropertyDescs(pi.PropertyType.GetGenericArguments()[0], includeChildren: false) : null
                })
                .ToDictionary(i => i.List_PropertyInfo!.Name, StringComparer.InvariantCultureIgnoreCase);
        }        

        #endregion

        public struct StreamWithInfo
        {
            public string Path;
            public string FileName;
            public Stream Stream;
            public bool IsStdFormat;

            public string GetFilePathAndName()
            {
                if (String.IsNullOrEmpty(Path))
                    return FileName;
                else
                    return Path + "->" + FileName;
            }
        }

        private class List_PropertyDesc
        {
            public PropertyInfo? List_PropertyInfo;
            public Type? ListElement_Type;
            /// <summary>
            ///     [ListElement_Type.Name, List_PropertyInfo]
            /// </summary>
            public Dictionary<string, List_PropertyDesc>? ChildList_PropertyDescs;
        }

        private class ReferenceEntities
        {
            /// <summary>
            ///     [ParamName, ParamDesc]
            /// </summary>
            public Dictionary<string, ParamDesc> ParamDescs { get; set; } = null!;

            /// <summary>
            ///     [Identifier, Legend]
            /// </summary>
            public Dictionary<string, Legend> Legends { get; set; } = null!;

            /// <summary>
            ///     [FileBytesHash_Base64, DbFile]
            /// </summary>
            public Dictionary<string, DbFile> DbFiles { get; set; } = null!;

            /// <summary>
            ///     [Type, ProjectVersionType]
            /// </summary>
            public Dictionary<string, ProjectVersionType> ProjectVersionTypes { get; set; } = null!;            

            /// <summary>
            ///     [Type, PcObjectEventType]
            /// </summary>
            public Dictionary<string, PcObjectEventType> PcObjectEventTypes { get; set; } = null!;            
        }        
    }
}

//if (importMetadata.RootsCollectionMode == Serialization.CollectionMode.Replace)
//{
//    foreach (var rootPcObject in originalRootPcObjects)
//    {
//        if (!rootPcObject._IsDeleted &&
//            rootPcObject.BasePcObject.Identifier != PazCheckConstants.BasePcObject_SystemArea)
//        {
//            if (originalPcObjectsDictionary.Remove(rootPcObject.Identifier, out forUnit_PcObjects))
//            {
//                foreach (var pcObject in forUnit_PcObjects)
//                {
//                    if (!pcObject._IsDeleted)
//                    {
//                        pcObject._IsDeleted = true;
//                        changed = true;
//                        result.PcObjects_ImportSerializationResult.Deleted.Add(rootPcObject.Identifier + "." + pcObject.Identifier, pcObject);
//                        result.PcObjects_ImportSerializationResult.All.TryGetValue(rootPcObject.Identifier + "." + pcObject.Identifier, out var pcObjectsList2);
//                        if (pcObjectsList2 is not null)
//                        {
//                            pcObjectsList2.Remove(pcObject);
//                            if (pcObjectsList2.Count == 0)
//                                result.PcObjects_ImportSerializationResult.All.Remove(rootPcObject.Identifier + "." + pcObject.Identifier);
//                        }
//                    }
//                }
//            }
//        }
//    }
//    originalRootPcObjects.Clear();
//}


//using DocumentFormat.OpenXml.Packaging;
//using System.IO.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
//using Row = DocumentFormat.OpenXml.Spreadsheet.Row;
//using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;

//using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
//{
//    WorkbookPart? workbookPart = spreadsheetDocument.WorkbookPart;
//    if (workbookPart is not null)
//    {
//        IEnumerable<Sheet> sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>() ?? new Sheet[0];

//        foreach (WorksheetPart worksheetPart in workbookPart.WorksheetParts)
//        {
//            string relationshipId = workbookPart.GetIdOfPart(worksheetPart);                                    
//            var sheet = sheets.FirstOrDefault(s => (s.Id?.HasValue ?? false) && (s.Id?.Value == relationshipId));

//            using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, sheet?.Name ?? @""));

//            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

//            using (MemoryStream memoryStream = new())
//            {
//                StreamWriter sw = new StreamWriter(memoryStream, Encoding.Unicode); // Does not close stream
//                foreach (Row row in sheetData.Elements<Row>())
//                {
//                    string line = Ssz.Utils.CsvHelper.FormatForCsv(@",", row.Elements<Cell>().Select(c => c.CellValue?.Text));
//                    sw.WriteLine();
//                }

//                memoryStream.Position = 0;

//                await ImportStdFileAsync(dbContext,
//                    memoryStream,
//                    addonsManager,
//                    projectId,
//                    user,
//                    cancellationToken,
//                    jobProgress,
//                    loggersSet);
//            }
//        }
//    }                            
//}


//public static object? PrepareCellValue(IXLCell cell)
//{
//    if (cell.Value.IsText)
//    {
//        string stringValue = cell.Value.GetText();
//        stringValue = stringValue.Trim();
//        stringValue = stringValue.Replace("\n", " ");
//        StringBuilder sbOut = new StringBuilder();
//        if (!string.IsNullOrEmpty(stringValue))
//        {
//            bool isWhiteSpace = false;
//            for (int i = 0; i < stringValue.Length; i++)
//            {
//                if (char.IsWhiteSpace(stringValue[i])) //Comparion with WhiteSpace
//                {
//                    if (!isWhiteSpace) //Comparison with previous Char
//                    {
//                        sbOut.Append(" ");
//                        isWhiteSpace = true;
//                    }
//                }
//                else
//                {
//                    isWhiteSpace = false;
//                    sbOut.Append(stringValue[i]);
//                }
//            }
//        }
//        return sbOut.ToString();
//    }
//    else
//    {
//        return cell.Value.ToString();
//    }
//}