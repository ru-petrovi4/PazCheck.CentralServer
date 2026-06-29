using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions
        
        public static async Task QuickTestAsync(
            AddonsManager addonsManager, 
            IServiceProvider serviceProvider, 
            IConfiguration configuration, 
            ILoggersSet loggersSet)
        {
            await Task.Delay(0);

            //InitializeMainDb(serviceProvider, loggersSet);

            //string programDataDirectoryFullName = ConfigurationHelper.GetProgramDataDirectoryFullName(configuration);            

            //var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PazCheckDbContext>>();

            //using var dbContext = dbContextFactory.CreateDbContext();
            //dbContext.IsLastChangeFieldsUpdatingDisabled = true;
            //dbContext.IsInformationSecurityEventsLoggingDisabled = true;

            //var project = dbContext.Projects
            //    .Include(p => p.Unit)
            //    .First(p => p.Unit.Identifier == DefaultUnitIdentifier && p.Title == MainProjectTitle);

            ////await VersionedEntities_SetIsDeletedAsync(dbContext, project.Id, new EntitiesCollectionInfo { IncludeAll = true }, typeof(CeMatrix), @"");
            ////await VersionedEntities_SetIsDeletedAsync(dbContext, project.Id, new EntitiesCollectionInfo { IncludeAll = true }, typeof(Tag), @"");
            //await SaveUnversionedChangesAsync(dbContext, project.Id, dbContext.ProjectVersionTypes.OrderBy(pvt => pvt.Id).First(), null, @"", @"Сохранение перед импортом");
            //dbContext.UnitEventsIntervals.Where(uei => uei.Unit == project.Unit).ExecuteDelete();

            //string inputDirectoryFullName = Path.Combine(programDataDirectoryFullName, @"input");
            //string logsInputDirectoryFullName = Path.Combine(inputDirectoryFullName, @"logs");
            //// Creates all directories and subdirectories in the specified path with the specified permissions unless they already exist.
            //Directory.CreateDirectory(inputDirectoryFullName);
            //Directory.CreateDirectory(logsInputDirectoryFullName);
            //foreach (var fileFullName in Directory.EnumerateFiles(inputDirectoryFullName))
            //{
            //    var fileName = Path.GetFileName(fileFullName);
            //    if (fileName.StartsWith("~"))
            //        continue;

            //    using MemoryStream stream = new();
            //    using (FileStream fileStream = File.Open(fileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        fileStream.CopyTo(stream);
            //    }
            //    stream.Position = 0;

            //    dbContext.IsInformationSecurityEventsLoggingDisabled = true;
            //    dbContext.User = "pceng2";
            //    //await SerializationHelper.ImportFileAsync(
            //    //    dbContext,
            //    //    stream,
            //    //    fileName,                    
            //    //    PazCheckCentralServerConstants.StdFileTypeIdentifier,
            //    //    null,
            //    //    project.Id,                        
            //    //    CancellationToken.None,
            //    //    NullJobProgress.Instance,                    
            //    //    null,
            //    //    mainServerWorker,
            //    //    loggersSet);
            //}

            //var ceMatrixRuntimeAddon = addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None)!;
            //var eventMessagesProcessingAddon = addonsManager.CreateInitializedAddonThreadSafe<EventMessagesProcessingAddonBase>(@"ExperionEventMessagesProcessing", addonsManager.Dispatcher!, CancellationToken.None)!;
            //foreach (var logFileFullName in Directory.EnumerateFiles(logsInputDirectoryFullName))
            //{
            //    using MemoryStream logStream = new();
            //    using (FileStream fileStream = File.Open(logFileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        fileStream.CopyTo(logStream);
            //    }
            //    logStream.Position = 0;
            //    var unitEventsInterval = (await
            //        eventMessagesProcessingAddon.ImportEventsJournalFileAsync(@"EPKS.OTHER", logStream, Path.GetFileName(logFileFullName), dbContext, project.Unit, CancellationToken.None, LoggersSet.Empty, NullJobProgress.Instance))!;
                
            //    if (unitEventsInterval is null)
            //    {
            //        Console.WriteLine(@"Нет событий для анализа.");
            //    }
            //    else
            //    {
            //        //var result = await ceMatrixRuntimeAddon.CalculateResultsAsync(dbContextFactory,
            //        //                project.Id,
            //        //                null,
            //        //                unitEventsInterval.BeginTimeUtc, //beginTimeUtc,
            //        //                unitEventsInterval.EndTimeUtc,
            //        //                @"",
            //        //                CancellationToken.None,
            //        //                NullJobProgress.Instance,
            //        //                loggersSet);                    

            //        //var (fileName, resultStream) = (await SerializationHelper.ExportResultAsync(dbContext,
            //        //   result!,
            //        //   addonsManager,
            //        //   @"",
            //        //   @"",
            //        //   CancellationToken.None,                       
            //        //   loggersSet))!;

            //        //using (var fs = File.Create(Path.Combine(programDataDirectoryFullName, @"test_output.zip")))
            //        //{
            //        //    resultStream!.CopyTo(fs);
            //        //}
            //    }
            //}

            Console.WriteLine(Properties.Resources.QuickTestSuccess);
        }        

        #endregion
    }
}
