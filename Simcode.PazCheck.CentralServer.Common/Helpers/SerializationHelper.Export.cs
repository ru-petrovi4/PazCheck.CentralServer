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
using Ssz.Utils.Addons;
using ClosedXML.Excel;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using JsonApiDotNetCore.Resources;
using IdentityServer4.Stores;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Ssz.Utils.ClosedXML;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class SerializationHelper
    {
        #region public functions        

        /// <summary>       
        ///     Returns (File Name, FileData)
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="dbFileIds"></param>
        /// <param name="fileRelativePathAndNames"></param>        
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static async Task<(string?, Stream?)> DownloadFilesAsync(
            PazCheckDbContext readOnlyDbContext,
            int[] dbFileIds,
            string[] fileRelativePathAndNames,            
            ILoggersSet loggersSet)
        {
            if (dbFileIds.Length == 0 || dbFileIds.Length != fileRelativePathAndNames.Length)
            {
                loggersSet.Logger.LogError("Invalid dbFileIds: {0}", dbFileIds);
                return (null, null);
            }

            DbFile[] dbFiles;
            try
            {
                dbFiles = await readOnlyDbContext.DbFiles
                    .Where(df => dbFileIds.Contains(df.Id))
                    .Include(df => df.DbFileContent)
                    .ToArrayAsync();                    
            }
            catch
            {
                loggersSet.Logger.LogError("Invalid dbFileIds: {0}", dbFileIds);
                return (null, null);
            }            
            
            var zipFileMemoryStream = new MemoryStream();
            using (ZipArchive zipArchive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var index in Enumerable.Range(0, dbFiles.Length))
                {
                    DbFile dbFile = dbFiles.First(f => f.Id == dbFileIds[index]);
                    ZipArchiveEntry entry = zipArchive.CreateEntry(fileRelativePathAndNames[index]);
                    using (var entryStream = entry.Open())
                    {        
                        if (dbFile.DbFileContent is not null)
                            entryStream.Write(Convert.FromBase64String(dbFile.DbFileContent.FileBytes_Base64));
                    }
                }
            }
            zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
            return (@"Файлы " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip", zipFileMemoryStream);
        }

        public static async Task<Stream?> DownloadFileAsync(
            PazCheckDbContext readOnlyDbContext,
            int dbFileId,            
            ILoggersSet loggersSet)
        {            
            DbFile? dbFile = null;
            try
            {
                dbFile = await readOnlyDbContext.DbFiles
                    .Where(df => dbFileId == df.Id)
                    .Include(df => df.DbFileContent)
                    .FirstOrDefaultAsync();
            }
            catch
            {                
            }

            if (dbFile is null || dbFile.DbFileContent is null)
            {
                loggersSet.Logger.LogError("Invalid dbFileId: {0}", dbFileId);
                return null;
            }
                  
            return new MemoryStream(Convert.FromBase64String(dbFile.DbFileContent.FileBytes_Base64));
        }

        /// <summary>
        ///     Returns (File Name, FileData)
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="projectEntitiesCollectionInfo"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>        
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static async Task<(string?, Stream?)> ExportProjectAsync(
            PazCheckDbContext readOnlyDbContext,            
            ProjectEntitiesCollectionInfo projectEntitiesCollectionInfo,
            IMainServerWorker mainServerWorker,
            string addonIdentifier, 
            string destinationTypeIdentifier,            
            ILoggersSet loggersSet)
        {
            var dbCache = mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache;

            (Serialization.SerializationRootObject serializationRootObject, ProjectEntities_Temp projectEntities_Temp) = 
                await GetSerializationRootObjectAsync(
                    projectEntitiesCollectionInfo, 
                    readOnlyDbContext,
                    dbCache,
                    NullJobProgress.Instance
                    );

            List<(string, byte[])> filesData = new();

            if (addonIdentifier == @"" &&
                String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_File, StringComparison.InvariantCultureIgnoreCase))
            {
                byte[]? serializationRootObjectJsonBytes = null;

                try
                {
                    byte[] a1;
                    if (projectEntitiesCollectionInfo.FullExport_ProjectVersion)
                        a1 = StringHelper.GetUTF8BytesWithBomPreamble(
                            PazCheckConstants.ContentDirective_FileFormat + "," +
                            PazCheckConstants.StdFileFormat_JsonObjects + "," +
                            PazCheckConstants.ContentDirective_FormatVersion + ",1," +
                            PazCheckConstants.ContentDirective_RootCollectionMode + ",Replace," +
                            PazCheckConstants.ContentDirective_ChildCollectionMode + ",Replace," +
                            PazCheckConstants.ContentDirective_DataCollectionMode + ",Update\n"
                            );
                    else
                        a1 = StringHelper.GetUTF8BytesWithBomPreamble(
                            PazCheckConstants.ContentDirective_FileFormat + "," +
                            PazCheckConstants.StdFileFormat_JsonObjects + "," +
                            PazCheckConstants.ContentDirective_FormatVersion + ",1," +
                            PazCheckConstants.ContentDirective_RootCollectionMode + ",Update," +
                            PazCheckConstants.ContentDirective_ChildCollectionMode + ",Update," +
                            PazCheckConstants.ContentDirective_DataCollectionMode + ",Update\n"
                            );
                    var a2 = JsonSerializer.SerializeToUtf8Bytes(serializationRootObject, Serialization.SourceGenerationContext.Default.SerializationRootObject);
                    serializationRootObjectJsonBytes = new byte[a1.Length + a2.Length];
                    a1.CopyTo(serializationRootObjectJsonBytes, 0);
                    a2.CopyTo(serializationRootObjectJsonBytes, a1.Length);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, "JsonSerializer.SerializeToUtf8Bytes Error.");
                }

                if (serializationRootObjectJsonBytes is null)
                    return (null, null);

                filesData.Add(("01. " + String.Join(", ", serializationRootObject.GetLocalizedInfo()) + ".json", serializationRootObjectJsonBytes));                
            }
            else if (addonIdentifier == @"" && 
                String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_CsvFile, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var it in await GetXLWorkbooks_CompactAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    var worksheet = workbook.Worksheets.First();

                    //using var scope3 = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    using MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                    filesData.Add((Path.GetFileNameWithoutExtension(it.Item1)! + @".csv", csvMemoryStream.ToArray()));

                    workbook.Dispose();
                }
            }
            else if (addonIdentifier == @"" && 
                String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile, StringComparison.InvariantCultureIgnoreCase)
                )
            {
                foreach (var it in await GetXLWorkbooks_CompactAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    //using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    filesData.Add((it.Item1, workbook.GetAsByteArray()));
                }
            }
            else if (addonIdentifier == @"" &&
                String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended, StringComparison.InvariantCultureIgnoreCase)
                )
            {
                foreach (var it in await GetXLWorkbooks_ExtendedAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    //using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    filesData.Add((it.Item1, workbook.GetAsByteArray()));
                }
            }

            if (serializationRootObject.CeMatrices?.Count > 0)
            {
                CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                if (ceMatrixRuntimeAddon is null)
                {
                    loggersSet.Logger.LogError(Properties.Resources.InvalidAddonType, @"CeMatrixRuntimeAddonBase");
                    return (null, null);
                }

                if (addonIdentifier == @"")
                {
                    if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_File, StringComparison.InvariantCultureIgnoreCase) ||
                        String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_CsvFile, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var serializationCeMatrix in serializationRootObject.CeMatrices)
                        {
                            (string? fileName, XLWorkbook? workbook) = await ceMatrixRuntimeAddon.GetCeMatrixXLWorkbookAsync(
                                readOnlyDbContext, 
                                serializationCeMatrix.SourceEntity!, 
                                projectEntitiesCollectionInfo.ProjectVersionNum, 
                                mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                                loggersSet, 
                                humanReadable: false);
                            if (workbook is not null)
                            {
                                var worksheet = workbook.Worksheets.First();

                                //using var scope3 = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                                using MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                                filesData.Add((Path.GetFileNameWithoutExtension(fileName)! + @".csv", csvMemoryStream.ToArray()));

                                workbook.Dispose();
                            }
                        }
                    }
                    else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile, StringComparison.InvariantCultureIgnoreCase) ||
                        String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var serializationCeMatrix in serializationRootObject.CeMatrices)
                        {   
                            (string? fileName, XLWorkbook? workbook) = await ceMatrixRuntimeAddon.GetCeMatrixXLWorkbookAsync(
                                readOnlyDbContext, 
                                serializationCeMatrix.SourceEntity!, 
                                projectEntitiesCollectionInfo.ProjectVersionNum, 
                                mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                                loggersSet, 
                                humanReadable: false);
                            if (workbook is not null)
                                filesData.Add((fileName!, workbook.GetAsByteArray()));
                        }
                    }
                }
                else
                {
                    CustomImportExportAddonBase? customImportExportAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>()
                        .CreateInitializedAddonThreadSafe<CustomImportExportAddonBase>(null, CancellationToken.None, addonIdentifier);
                    if (customImportExportAddon is null)
                    {
                        loggersSet.Logger.LogError(Properties.Resources.InvalidAddonIdentifier, addonIdentifier);
                        return (null, null);
                    }

                    filesData.AddRange(await customImportExportAddon.ExportCeMatricesAsync(
                        destinationTypeIdentifier, 
                        readOnlyDbContext,
                        serializationRootObject.CeMatrices.Select(m => m.SourceEntity!).ToList(),                        
                        projectEntitiesCollectionInfo.ProjectVersionNum, 
                        mainServerWorker.ServiceProvider, 
                        loggersSet));                    
                }
            }

            //var fileDownloadName = "ProjectObjects " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip";
            if (projectEntitiesCollectionInfo.FullExport_ProjectVersion && projectEntities_Temp.ProjectAllParamValues.ProjectVersion is not null)
                return (Properties.Resources.ProjectVersionFullExportFile +
                    " " + projectEntities_Temp.ProjectAllParamValues.ProjectVersion.TimeUtc.ToLocalTime().ToString("yyyy'-'MM'-'dd HH'-'mm") + @".zip", ExportZip(filesData));

            return (String.Join(", ", serializationRootObject.GetLocalizedInfo()) + 
                " " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + @".zip", ExportZip(filesData));
        }

        /// <summary>
        ///     Returns (File Name, FileData)
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="entitiesCollectionInfo"></param>
        /// <param name="fullExport"></param>
        /// <param name="entitiesName"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static async Task<(string?, Stream?)> ExportEntitiesAsync(PazCheckDbContext readOnlyDbContext, EntitiesCollectionInfo entitiesCollectionInfo, bool fullExport, string entitiesName, IMainServerWorker mainServerWorker, string addonIdentifier, string destinationTypeIdentifier, ILoggersSet loggersSet)
        {
            Serialization.SerializationRootObject serializationRootObject = await GetSerializationRootObjectAsync(
                entitiesCollectionInfo, 
                entitiesName, 
                readOnlyDbContext, 
                mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache);

            List<(string, byte[])> filesData = new();

            if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_File, StringComparison.InvariantCultureIgnoreCase))
            {
                byte[]? serializationRootObjectJsonBytes = null;

                try
                {
                    byte[] a1;
                    if (fullExport)
                        a1 = StringHelper.GetUTF8BytesWithBomPreamble(
                            PazCheckConstants.ContentDirective_FileFormat + "," +
                            PazCheckConstants.StdFileFormat_JsonObjects + "," +
                            PazCheckConstants.ContentDirective_FormatVersion + ",1," +
                            PazCheckConstants.ContentDirective_RootCollectionMode + ",Replace," +
                            PazCheckConstants.ContentDirective_ChildCollectionMode + ",Replace," +
                            PazCheckConstants.ContentDirective_DataCollectionMode + ",Update\n"
                            );
                    else
                        a1 = StringHelper.GetUTF8BytesWithBomPreamble(
                            PazCheckConstants.ContentDirective_FileFormat + "," +
                            PazCheckConstants.StdFileFormat_JsonObjects + "," +
                            PazCheckConstants.ContentDirective_FormatVersion + ",1," +
                            PazCheckConstants.ContentDirective_RootCollectionMode + ",Update," +
                            PazCheckConstants.ContentDirective_ChildCollectionMode + ",Update," +
                            PazCheckConstants.ContentDirective_DataCollectionMode + ",Update\n"
                            );
                    var a2 = JsonSerializer.SerializeToUtf8Bytes(serializationRootObject, Serialization.SourceGenerationContext.Default.SerializationRootObject);
                    serializationRootObjectJsonBytes = new byte[a1.Length + a2.Length];
                    a1.CopyTo(serializationRootObjectJsonBytes, 0);
                    a2.CopyTo(serializationRootObjectJsonBytes, a1.Length);
                }
                catch (Exception ex)
                {
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, "JsonSerializer.SerializeToUtf8Bytes Error.");
                }

                if (serializationRootObjectJsonBytes is null)
                    return (null, null);

                filesData.Add(("SerializationRootObject.pc.json", serializationRootObjectJsonBytes));                
            }
            else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_CsvFile, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var it in await GetXLWorkbooks_CompactAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    var worksheet = workbook.Worksheets.First();

                    //using var scope3 = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    using MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                    filesData.Add((Path.GetFileNameWithoutExtension(it.Item1) + @".csv", csvMemoryStream.ToArray()));

                    workbook.Dispose();
                }
            }
            else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var it in await GetXLWorkbooks_CompactAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    //using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    filesData.Add((it.Item1, workbook.GetAsByteArray()));
                }
            }
            else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile_Extended, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var it in await GetXLWorkbooks_ExtendedAsync(serializationRootObject, mainServerWorker, loggersSet))
                {
                    XLWorkbook workbook = it.Item2;

                    //using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    filesData.Add((it.Item1, workbook.GetAsByteArray()));
                }
            }

            return (String.Join(", ", serializationRootObject.GetLocalizedInfo()) + " " + 
                DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip", ExportZip(filesData));
        }        

        /// <summary>
        ///     Returns (File Name, FileData)
        /// </summary>
        /// <param name="readOnlyDbContext"></param>
        /// <param name="ceMatrixResults"></param>
        /// <param name="addonIdentifier"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="user"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="informationSecurityEventsLogger"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static async Task<(string?, Stream?)> ExportCeMatrixResultsToFileAsync(PazCheckDbContext readOnlyDbContext,
            List<CeMatrixResult> ceMatrixResults,
            string addonIdentifier,
            string destinationTypeIdentifier,
            string user,
            IMainServerWorker mainServerWorker,
            CancellationToken cancellationToken,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,            
            ILoggersSet loggersSet)
        {
            List<(string, byte[])> filesData = new();            

            CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>().CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
            if (ceMatrixRuntimeAddon is null)
            {
                loggersSet.Logger.LogError(Properties.Resources.InvalidAddonType, @"CeMatrixRuntimeAddonBase");
                return (null, null);
            }

            if (addonIdentifier == @"")
            {
                if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_CsvFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var ceMatrixResult in ceMatrixResults)
                    {
                        (string? fileName, XLWorkbook? workbook) = await ceMatrixRuntimeAddon.GetCeMatrixResultXLWorkbookAsync(
                            readOnlyDbContext, 
                            ceMatrixResult, 
                            mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                            loggersSet, 
                            humanReadable: false);
                        if (workbook is not null)
                        {
                            var worksheet = workbook.Worksheets.First();

                            using var scope3 = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                            using MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                            filesData.Add((Path.GetFileNameWithoutExtension(fileName)! + @".csv", csvMemoryStream.ToArray()));

                            workbook.Dispose();
                        }
                    }
                }
                else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var ceMatrixResult in ceMatrixResults)
                    {
                        (string? fileName, XLWorkbook? workbook) = await ceMatrixRuntimeAddon.GetCeMatrixResultXLWorkbookAsync(
                            readOnlyDbContext, 
                            ceMatrixResult, 
                            mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache, 
                            loggersSet, 
                            humanReadable: false);
                        if (workbook is not null)
                        {
                            filesData.Add((fileName!, workbook.GetAsByteArray()));
                        }
                    }
                }
            }
            else
            {
                CustomImportExportAddonBase? customImportExportAddon = mainServerWorker.ServiceProvider.GetRequiredService<AddonsManager>()
                    .CreateInitializedAddonThreadSafe<CustomImportExportAddonBase>(null, CancellationToken.None, addonIdentifier);
                if (customImportExportAddon is null)
                {
                    loggersSet.Logger.LogError(Properties.Resources.InvalidAddonIdentifier, addonIdentifier);
                    return (null, null);
                }
                
                filesData.AddRange(await customImportExportAddon.ExportCeMatrixResultsAsync(destinationTypeIdentifier, readOnlyDbContext, ceMatrixResults, mainServerWorker.ServiceProvider, loggersSet));
            }

            return ("Результаты анализа " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip", ExportZip(filesData));
        }

        /// <summary>
        /// </summary>
        /// <param name="filesData"></param>
        /// <returns></returns>
        public static Stream ExportZip(IEnumerable<(string, byte[])> filesData)
        {
            var zipFileMemoryStream = new MemoryStream();
            using (ZipArchive zipArchive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var it in filesData)
                {
                    var entry = zipArchive.CreateEntry(it.Item1);
                    using (var entryStream = entry.Open())
                    {                        
                        entryStream.Write(it.Item2);
                    }
                }
            }
            zipFileMemoryStream.Position = 0;
            return zipFileMemoryStream;
        }

        public static byte[] GetXlsxBytes(List<List<string?>> fileContent, string worksheetName)
        {
            using XLWorkbook workbook = new();            
            IXLWorksheet worksheet = workbook.Worksheets.Add(ExcelHelper.MakeValidSheetName(worksheetName));            
            foreach (int row in Enumerable.Range(1, fileContent.Count))
            {
                var line = fileContent[row - 1];
                foreach (int column in Enumerable.Range(1, line.Count))
                {
                    worksheet.Cell(row, column).Value = line[column - 1];
                }                    
            }
            worksheet.AdjustToContents();

            return workbook.GetAsByteArray();
        }

        /// <summary>
        ///     Returns (File Name, FileData)
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="dbCache"></param>
        /// <param name="entityType"></param>
        /// <param name="destinationTypeIdentifier"></param>
        /// <param name="filter"></param>        
        /// <returns></returns>
        public static async Task<(string?, Stream?)> ExportEventsToFileAsync(
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            DbCache dbCache,
            Type entityType,
            string destinationTypeIdentifier,
            Filter filter)
        {
            PazCheckDbContext.EntityName_PropertyInfos.TryGetValue(entityType.Name, out PropertyInfo? propertyInfo);
            if (propertyInfo is null)
                return (null, null);

            List<(string, byte[])> filesData = new();

            await using var readOnlyDbContext = dbContextFactory.CreateDbContext();
            readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            FilterHelper.Prepare(filter, dbCache);

            IEnumerable<object> result = await PazCheckDbHelper.FilterAsync(readOnlyDbContext, dbCache, entityType, filter, null, needOrdering: true);

            string displayName;
            switch (propertyInfo.Name)
            {
                case nameof(PazCheckDbContext.PcObjectEvents):
                    displayName = Properties.Resources.PcObjectEvents;
                    break;
                case nameof(PazCheckDbContext.InformationSecurityEvents):
                case nameof(PazCheckDbContext.AllRolesAccessInformationSecurityEvents):
                    displayName = Properties.Resources.InformationSecurityEvents;
                    result = result.Select(it => GetSerializationInformationSecurityEvent((InformationSecurityEvent)it));
                    entityType = typeof(Serialization.InformationSecurityEvent);
                    break;                
                case nameof(PazCheckDbContext.UnitEvents):
                    displayName = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? propertyInfo.Name;
                    result = result.Select(it => GetSerializationUnitEvent((UnitEvent)it));
                    entityType = typeof(Serialization.UnitEvent);
                    break;
                case nameof(PazCheckDbContext.UserEvents):
                    displayName = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? propertyInfo.Name;
                    result = result.Select(it => GetSerializationUserEvent((UserEvent)it));
                    entityType = typeof(Serialization.UserEvent);
                    break;
                default:
                    displayName = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()?.DisplayName ?? propertyInfo.Name;
                    break;
            }

            if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_CsvFile, StringComparison.InvariantCultureIgnoreCase))
            {
                var it = GetXLWorkbook_Extended(result, entityType, propertyInfo.Name, displayName, dbCache, stdFile: true);

                using XLWorkbook workbook = it.Item2;

                foreach (var worksheet in workbook.Worksheets)
                {
                    //using var scope3 = loggersSet.WrapperUserFriendlyLogger.BeginScope((Properties.Resources.WorksheetScopeName, worksheet.Name));

                    using MemoryStream csvMemoryStream = GetCsvMemoryStream(worksheet);

                    filesData.Add((worksheet.Name + @".csv", csvMemoryStream.ToArray()));
                }
            }
            else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_Std_ExcelFile, StringComparison.InvariantCultureIgnoreCase))
            {
                var it = GetXLWorkbook_Extended(result, entityType, propertyInfo.Name, displayName, dbCache, stdFile: true);

                using XLWorkbook workbook = it.Item2;                

                filesData.Add((it.Item1, workbook.GetAsByteArray()));
            }
            else if (String.Equals(destinationTypeIdentifier, PazCheckConstants.TypeIdentifier_HumanReadable_ExcelFile, StringComparison.InvariantCultureIgnoreCase))
            {
                var it = GetXLWorkbook_Extended(result, entityType, propertyInfo.Name, displayName, dbCache, stdFile: false);

                using XLWorkbook workbook = it.Item2;

                filesData.Add((it.Item1, workbook.GetAsByteArray()));
            }            

            return (displayName + " " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH'-'mm") + ".zip", ExportZip(filesData));
        }

        /// <summary>
        ///     Returns (File Name, XLWorkbook)
        ///     Each XLWorkbook has 1 Worksheet
        ///     Preconditions: objects in Array != null
        /// </summary>        
        /// <param name="objectsList"></param>
        /// <param name="elementType"></param>
        /// <param name="collection"></param>
        /// <param name="collectionDisplayName"></param>
        /// <param name="dbCache"></param>
        /// <param name="stdFile"></param>
        /// <returns></returns>
        public static (string, XLWorkbook) GetXLWorkbook_Extended(
            IEnumerable<object> objectsList,
            Type elementType,
            string collection,
            string collectionDisplayName,
            DbCache dbCache,
            bool stdFile)
        {
            // Do not forget make fixes to GetXLWorkbook_Compact

            XLWorkbook workbook = new();

            IXLWorksheet worksheet = workbook.Worksheets.Add(ExcelHelper.MakeValidSheetName(collectionDisplayName));

            int rowN = 1;            
            if (stdFile)
            {
                worksheet.Cell(rowN, 1).Value = PazCheckConstants.ContentDirective_FileFormat;
                worksheet.Cell(rowN, 2).Value = PazCheckConstants.StdFileFormat_TableObjects;
                worksheet.Cell(rowN, 3).Value = PazCheckConstants.ContentDirective_FormatVersion;
                worksheet.Cell(rowN, 4).Value = @"1";
                worksheet.Cell(rowN, 5).Value = PazCheckConstants.ContentDirective_RootCollectionMode;
                worksheet.Cell(rowN, 6).Value = @"Update";
                worksheet.Cell(rowN, 7).Value = PazCheckConstants.ContentDirective_ChildCollectionMode;
                worksheet.Cell(rowN, 8).Value = @"Update";
                worksheet.Cell(rowN, 9).Value = PazCheckConstants.ContentDirective_DataCollectionMode;
                worksheet.Cell(rowN, 10).Value = @"Update";

                rowN += 1;
                worksheet.Cell(rowN, 1).Value = PazCheckConstants.ContentDirective_Collection;
                worksheet.Cell(rowN, 2).Value = collection;
            }            

            rowN += 1;
            int endColumnN;
            (rowN, endColumnN) = FillXLWorksheet_Extended(
                objectsList,
                elementType,
                dbCache,
                worksheet,
                rowN,
                1);            

            foreach (var columnN_2 in Enumerable.Range(1, endColumnN))
            {
                worksheet.Column(columnN_2).Width = 30;
            }

            worksheet.AdjustToContents();

            return (collectionDisplayName + ".xlsx", workbook);
        }

        /// <summary>
        ///     Preconditions: objects in Array != null
        /// </summary>
        /// <param name="objectsList"></param>
        /// <param name="elementType"></param>        
        /// <param name="dbCache"></param>
        /// <param name="excelWorksheet"></param>
        /// <param name="beginRowN"></param>
        /// <param name="beginColumnN"></param>
        public static (int endRowN, int endColumnN) FillXLWorksheet_Extended(
            IEnumerable<object> objectsList,
            Type elementType,
            DbCache dbCache,            
            IXLWorksheet excelWorksheet,
            int beginRowN,
            int beginColumnN)
        {            
            int rowN = beginRowN - 1;            

            PropertyInfo[] plainPropertyInfos;
            // Length is always 0 or 1
            PropertyInfo[] paramPropertyInfos;
            // Length is always 0 or 1
            PropertyInfo[] tagConditionPropertyInfos;

            if (elementType.IsAssignableTo(typeof(Identifiable<int>)))
            {
                plainPropertyInfos = elementType.GetProperties()
                    .Where(pi => pi.PropertyType.Name != @"List`1" &&
                        !pi.HasAttribute<JsonIgnoreAttribute>() &&
                        pi.Name != @"_IsDeleted" &&
                        pi.HasAttribute<AttrAttribute>())
                    .ToArray();

                paramPropertyInfos = [];

                tagConditionPropertyInfos = [];
            }
            else
            {
                plainPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name != @"List`1")
                    .ToArray();

                paramPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name == @"List`1" && pi.PropertyType.GetGenericArguments()[0] == typeof(Serialization.Param))
                    .ToArray();

                tagConditionPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name == @"List`1" && pi.PropertyType.GetGenericArguments()[0] == typeof(Serialization.TagCondition))
                    .ToArray();
            }

            // [Param.Name][]
            HashSet<string>[] paramNames = new HashSet<string>[paramPropertyInfos.Length];
            foreach (int i in Enumerable.Range(0, paramPropertyInfos.Length))
            {
                paramNames[i] = new HashSet<string>();
            }
            int[] tagConditionCounts = new int[tagConditionPropertyInfos.Length];
            foreach (var o in objectsList)
            {
                foreach (int i in Enumerable.Range(0, paramPropertyInfos.Length))
                {
                    var pn = paramNames[i];
                    var l = (List<Serialization.Param>?)paramPropertyInfos[i].GetValue(o);
                    if (l is not null)
                    {
                        foreach (var param_ in l)
                        {
                            pn.Add(param_.Name);
                        }
                    }
                }

                foreach (int i in Enumerable.Range(0, tagConditionCounts.Length))
                {
                    var l = (List<Serialization.TagCondition>?)tagConditionPropertyInfos[i].GetValue(o);
                    if (l is not null && l.Count > tagConditionCounts[i])
                        tagConditionCounts[i] = l.Count;
                }
            }
            int columnN = beginColumnN + plainPropertyInfos.Length - 1;
            // [Param.Name, columnN][]
            CaseInsensitiveOrderedDictionary<int>[] paramsColumnN = new CaseInsensitiveOrderedDictionary<int>[paramPropertyInfos.Length];
            foreach (int i in Enumerable.Range(0, paramPropertyInfos.Length))
            {
                var d = new CaseInsensitiveOrderedDictionary<int>();
                paramsColumnN[i] = d;
                foreach (string paramName in paramNames[i]
                    .OrderByDescending(p => dbCache.ParamDescs.GetValueOrDefault(p)?.Priority ?? 0)
                    .ThenBy(p => p))
                {
                    columnN += 1;
                    d[paramName] = columnN;
                }
            }            

            rowN += 1;
            columnN = beginColumnN - 1;
            foreach (var pi in plainPropertyInfos)
            {
                columnN += 1;
                excelWorksheet.Cell(rowN, columnN).Value = @"." + pi.Name;
                excelWorksheet.Cell(rowN, columnN).Style.Font.Bold = true;
            }
            foreach (int i in Enumerable.Range(0, paramPropertyInfos.Length))
            {
                var paramPropertyInfo = paramPropertyInfos[i];
                var d = paramsColumnN[i];
                foreach (var kvp in d
                    .OrderBy(it => it.Value))
                {
                    columnN += 1;
                    excelWorksheet.Cell(rowN, columnN).Value = "." + paramPropertyInfo.Name + "[" + kvp.Key + "]";
                    excelWorksheet.Cell(rowN, columnN).Style.Font.Bold = true;
                }
            }
            foreach (int i in Enumerable.Range(0, tagConditionPropertyInfos.Length))
            {
                var tagConditionPropertyInfo = tagConditionPropertyInfos[i];
                foreach (var j in Enumerable.Range(0, tagConditionCounts[i]))
                {
                    columnN += 1;
                    excelWorksheet.Cell(rowN, columnN).Value = "." + tagConditionPropertyInfo.Name + "[]";
                    excelWorksheet.Cell(rowN, columnN).Style.Font.Bold = true;
                }
            }

            int endColumnN = columnN;

            foreach (var o in objectsList)
            {
                rowN += 1;
                columnN = beginColumnN - 1;
                foreach (var pi in plainPropertyInfos)
                {
                    columnN += 1;
                    excelWorksheet.Cell(rowN, columnN).Value = new Any(pi.GetValue(o)).ValueAsString(false);
                }
                foreach (int i in Enumerable.Range(0, paramPropertyInfos.Length))
                {
                    columnN += paramsColumnN[i].Count;
                    var paramPropertyInfo = paramPropertyInfos[i];
                    var d = paramsColumnN[i];
                    var paramsList = (List<Serialization.Param>?)paramPropertyInfo.GetValue(o);
                    if (paramsList is not null)
                        foreach (var param in paramsList)
                        {
                            excelWorksheet.Cell(rowN, d.TryGetValue(param.Name)).Value = param.Value;
                        }
                }
                foreach (var pi in tagConditionPropertyInfos)
                {
                    var tagConditionsList = (List<Serialization.TagCondition>?)pi.GetValue(o);
                    if (tagConditionsList is not null)
                        foreach (var tagCondition in tagConditionsList)
                        {
                            columnN += 1;
                            excelWorksheet.Cell(rowN, columnN).Value = tagCondition.ToString();
                        }
                }
            }            

            return (rowN, endColumnN);
        }

        #endregion

        #region private functions

        /// <summary>
        ///     Returns List of (File Name, XLWorkbook)
        ///     Each XLWorkbook has 1 Worksheet
        /// </summary>
        /// <param name="serializationRootObject"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        private static Task<List<(string, XLWorkbook)>> GetXLWorkbooks_CompactAsync(Serialization.SerializationRootObject serializationRootObject, IMainServerWorker mainServerWorker, ILoggersSet loggersSet)
        {
            var dbCache = mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache;

            List<(string, XLWorkbook)> result = new();

            int i = 0;

            if ((serializationRootObject.Tags?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.Tags!,
                    typeof(Serialization.Tag),
                    nameof(serializationRootObject.Tags),
                    Properties.Resources.Tags,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.Actuators?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.Actuators!,
                    typeof(Serialization.Actuator),
                    nameof(serializationRootObject.Actuators),
                    Properties.Resources.BaseActuators,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.MonitoringObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.MonitoringObjects!,
                    typeof(Serialization.MonitoringObject),
                    nameof(serializationRootObject.MonitoringObjects),
                    Properties.Resources.SafetyControllers,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.Legends?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.Legends!,
                    typeof(Serialization.Legend),
                    nameof(serializationRootObject.Legends),
                    Properties.Resources.Legends,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.BasePcObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.BasePcObjects!,
                    typeof(Serialization.BasePcObject),
                    nameof(serializationRootObject.BasePcObjects),
                    Properties.Resources.BasePcObjects,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.PcObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Compact_Std(
                    serializationRootObject.PcObjects!,
                    typeof(Serialization.PcObject),
                    nameof(serializationRootObject.PcObjects),
                    Properties.Resources.PcObjects,
                    dbCache, 
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Returns List of (File Name, XLWorkbook)
        ///     Each XLWorkbook has 1 Worksheet
        /// </summary>
        /// <param name="serializationRootObject"></param>
        /// <param name="mainServerWorker"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        private static Task<List<(string, XLWorkbook)>> GetXLWorkbooks_ExtendedAsync(Serialization.SerializationRootObject serializationRootObject, IMainServerWorker mainServerWorker, ILoggersSet loggersSet)
        {
            var dbCache = mainServerWorker.ServiceProvider.GetRequiredService<Cache>().DbCache;

            List<(string, XLWorkbook)> result = new();

            int i = 0;

            if ((serializationRootObject.Tags?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.Tags!,
                    typeof(Serialization.Tag),
                    nameof(serializationRootObject.Tags),
                    Properties.Resources.Tags,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.Actuators?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.Actuators!,
                    typeof(Serialization.Actuator),
                    nameof(serializationRootObject.Actuators),
                    Properties.Resources.BaseActuators,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.MonitoringObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.MonitoringObjects!,
                    typeof(Serialization.MonitoringObject),
                    nameof(serializationRootObject.MonitoringObjects),
                    Properties.Resources.SafetyControllers,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.Legends?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.Legends!,
                    typeof(Serialization.Legend),
                    nameof(serializationRootObject.Legends),
                    Properties.Resources.Legends,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.BasePcObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.BasePcObjects!,
                    typeof(Serialization.BasePcObject),
                    nameof(serializationRootObject.BasePcObjects),
                    Properties.Resources.BasePcObjects,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            if ((serializationRootObject.PcObjects?.Count ?? 0) > 0)
            {
                var it = GetXLWorkbook_Extended(
                    serializationRootObject.PcObjects!,
                    typeof(Serialization.PcObject),
                    nameof(serializationRootObject.PcObjects),
                    Properties.Resources.PcObjects,
                    dbCache,
                    stdFile: true);
                i += 1;
                result.Add(($"{i}. {it.Item1}", it.Item2));
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Returns (File Name, XLWorkbook)
        ///     Each XLWorkbook has 1 Worksheet        
        ///     Preconditions: objects in list != null
        /// </summary>        
        /// <param name="objectsList"></param>
        /// <param name="elementType"></param>
        /// <param name="collection"></param>
        /// <param name="collectionDisplayName"></param>
        /// <param name="dbCache"></param>
        /// <param name="stdFile"></param>
        /// <returns></returns>
        private static (string, XLWorkbook) GetXLWorkbook_Compact_Std(
            IEnumerable<object> objectsList, 
            Type elementType,
            string collection, 
            string collectionDisplayName,
            DbCache dbCache,
            bool stdFile)
        {
            // Do not forget make fixes to GetXLWorkbook_Extended

            XLWorkbook workbook = new();

            IXLWorksheet worksheet = workbook.Worksheets.Add(ExcelHelper.MakeValidSheetName(collectionDisplayName));
            
            int rowN = 1;
            int endColumnN = 10;
            if (stdFile)
            {
                worksheet.Cell(rowN, 1).Value = PazCheckConstants.ContentDirective_FileFormat;
                worksheet.Cell(rowN, 2).Value = PazCheckConstants.StdFileFormat_TableObjects;
                worksheet.Cell(rowN, 3).Value = PazCheckConstants.ContentDirective_FormatVersion;
                worksheet.Cell(rowN, 4).Value = @"1";
                worksheet.Cell(rowN, 5).Value = PazCheckConstants.ContentDirective_RootCollectionMode;
                worksheet.Cell(rowN, 6).Value = @"Update";
                worksheet.Cell(rowN, 7).Value = PazCheckConstants.ContentDirective_ChildCollectionMode;
                worksheet.Cell(rowN, 8).Value = @"Update";
                worksheet.Cell(rowN, 9).Value = PazCheckConstants.ContentDirective_DataCollectionMode;
                worksheet.Cell(rowN, 10).Value = @"Update";
            }

            PropertyInfo[] plainPropertyInfos;
            // Length is always 0 or 1
            PropertyInfo[] paramPropertyInfos;
            // Length is always 0 or 1
            PropertyInfo[] tagConditionPropertyInfos;

            if (elementType.IsAssignableTo(typeof(Identifiable<int>)))
            {
                plainPropertyInfos = elementType.GetProperties()
                    .Where(pi => pi.PropertyType.Name != @"List`1" &&
                        !pi.HasAttribute<JsonIgnoreAttribute>() &&
                        pi.Name != @"_IsDeleted" &&
                        pi.HasAttribute<AttrAttribute>())
                    .ToArray();

                paramPropertyInfos = [];

                tagConditionPropertyInfos = [];
            }
            else
            {
                plainPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name != @"List`1")
                    .ToArray();

                paramPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name == @"List`1" && pi.PropertyType.GetGenericArguments()[0] == typeof(Serialization.Param))
                    .ToArray();

                tagConditionPropertyInfos = elementType.GetProperties()
                    .Where(pi => !pi.HasAttribute<JsonIgnoreAttribute>() && pi.PropertyType.Name == @"List`1" && pi.PropertyType.GetGenericArguments()[0] == typeof(Serialization.TagCondition))
                    .ToArray();
            }

            int[] tagConditionCounts = new int[tagConditionPropertyInfos.Length];
            foreach (var o in objectsList)
            {
                foreach (int i in Enumerable.Range(0, tagConditionCounts.Length))
                {
                    var l = (List<Serialization.TagCondition>?)tagConditionPropertyInfos[i].GetValue(o);
                    if (l is not null && l.Count > tagConditionCounts[i])
                        tagConditionCounts[i] = l.Count;
                }
            }

            foreach (var g in objectsList.GroupBy(o => CsvHelper.FormatForCsv(@",",
                paramPropertyInfos
                    .Select(pi => (pi, (List<Serialization.Param>?)pi.GetValue(o)))
                    .Where(pi_it => pi_it.Item2 is not null)
                    .SelectMany(pi_it => pi_it.Item2!
                        .OrderByDescending(p => dbCache.ParamDescs.GetValueOrDefault(p.Name)?.Priority ?? 0)
                        .ThenBy(p => p.Name)
                        .Select(p => "." + pi_it.Item1.Name + "[" + p.Name + "]"))
                    .Concat(
                tagConditionPropertyInfos
                    .SelectMany((pi, i) => Enumerable.Repeat("." + pi.Name + @"[]", tagConditionCounts[i]))
                    ))))
            {
                if (stdFile)
                {
                    rowN += 1;
                    worksheet.Cell(rowN, 1).Value = PazCheckConstants.ContentDirective_Collection;
                    worksheet.Cell(rowN, 2).Value = collection;
                }

                rowN += 1;
                int columnN = 0;
                foreach (var pi in plainPropertyInfos)
                {
                    columnN += 1;
                    worksheet.Cell(rowN, columnN).Value = @"." + pi.Name;
                    worksheet.Cell(rowN, columnN).Style.Font.Bold = true;
                }
                foreach (var columnHeader in CsvHelper.ParseCsvLine(@",", g.Key))
                {
                    columnN += 1;
                    worksheet.Cell(rowN, columnN).Value = columnHeader;
                    worksheet.Cell(rowN, columnN).Style.Font.Bold = true;
                }

                if (columnN > endColumnN)
                    endColumnN = columnN;

                foreach (var o in g)
                {
                    rowN += 1;
                    columnN = 0;
                    foreach (var pi in plainPropertyInfos)
                    {
                        object? value = pi.GetValue(o);
                        string? cellValue;
                        if (pi.PropertyType.IsAssignableTo(typeof(Dictionary<string, string?>)))
                        {
                            if (value is null)
                                cellValue = null;
                            else
                                cellValue = NameValueCollectionHelper.GetNameValueCollectionString((Dictionary<string, string?>)value);
                        }
                        else
                        {
                            cellValue = new Any(value).ValueAsString(false);
                        }
                        columnN += 1;
                        worksheet.Cell(rowN, columnN).Value = cellValue;
                    }
                    foreach (var pi in paramPropertyInfos)
                    {
                        var paramsList = (List<Serialization.Param>?)pi.GetValue(o);
                        if (paramsList is not null)
                            foreach (var param in paramsList
                                .OrderByDescending(p => dbCache.ParamDescs.GetValueOrDefault(p.Name)?.Priority ?? 0)
                                .ThenBy(p => p.Name))
                            {
                                columnN += 1;
                                worksheet.Cell(rowN, columnN).Value = param.Value;
                            }
                    }
                    foreach (var pi in tagConditionPropertyInfos)
                    {
                        var tagConditionsList = (List<Serialization.TagCondition>?)pi.GetValue(o);
                        if (tagConditionsList is not null)
                            foreach (var tagCondition in tagConditionsList)
                            {
                                columnN += 1;
                                worksheet.Cell(rowN, columnN).Value = tagCondition.ToString();
                            }
                    }
                }
            }

            foreach (int columnN_2 in Enumerable.Range(1, endColumnN))
            {
                worksheet.Column(columnN_2).Width = 30;
            }

            worksheet.AdjustToContents();

            return (collectionDisplayName + ".xlsx", workbook);
        }     

        #endregion
    }
}

//switch (entityType.Name)
//{
//    case nameof(InformationSecurityEvent):
//    case nameof(AllRolesAccessInformationSecurityEvent):
//        result.OfType<InformationSecurityEvent>();
//        break;
//    case nameof(UnitEvent):
//        break;
//}

//public static Stream ExportZip(string fileName, Stream stream)
//{
//    var zipFileMemoryStream = new MemoryStream();
//    using (ZipArchive zipArchive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Create, leaveOpen: true))
//    {
//        var entry = zipArchive.CreateEntry(fileName);
//        using (var entryStream = entry.Open())
//        {
//            stream.CopyTo(entryStream);
//        }
//    }
//    zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
//    return zipFileMemoryStream;
//}
