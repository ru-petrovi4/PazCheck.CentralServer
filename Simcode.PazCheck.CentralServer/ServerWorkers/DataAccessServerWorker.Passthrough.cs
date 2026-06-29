using Grpc.Core;
using Microsoft.Extensions.Logging;
using Ssz.Dcs.CentralServer.Common;
using Ssz.Dcs.CentralServer.Common.Passthrough;
using Ssz.Dcs.CentralServer.Common.EntityFramework;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils.DataAccess;
using Ssz.Utils;
using Ssz.Utils.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Ssz.DataAccessGrpc.ServerBase.Properties;
using System.Collections.ObjectModel;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions
        
        public override async Task<ReadOnlyMemory<byte>> PassthroughAsync(ServerContext serverContext, string recipientPath, string passthroughName, ReadOnlyMemory<byte> dataToSend)
        {
            try
            {                
                byte[] returnData;
                string systemNameToConnect = serverContext.SystemNameToConnect;
                if (systemNameToConnect == @"") // Utility context 
                {                
                    switch (passthroughName)
                    {
                        case PassthroughConstants.GetDirectoryInfo:
                            GetDirectoryInfoPassthrough(dataToSend, out returnData);
                            return returnData;
                        case PassthroughConstants.LoadFiles:
                            LoadFilesPassthrough(Encoding.UTF8.GetString(dataToSend.Span), out returnData);
                            return returnData;                                    
                        default:
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "Unknown passthroughName."));
                    }                
                }
                else
                {
                    switch (passthroughName)
                    {
                        case PassthroughConstants.GetAddonStatuses:
                            return await GetAddonStatusesPassthroughAsync(serverContext);
                        case PassthroughConstants.ReadConfiguration:                        
                            return await ReadConfigurationPassthroughAsync(serverContext, recipientPath, dataToSend);
                        case PassthroughConstants.WriteConfiguration:
                            await WriteConfigurationPassthroughAsync(serverContext, recipientPath, dataToSend);
                            return ReadOnlyMemory<byte>.Empty;
                        case PassthroughConstants.GetOperatorUserName:
                            return await GetOperatorUserNameAsync(serverContext);
                        case PassthroughConstants.GetOperatorRoleName:
                            return await GetOperatorRoleNameAsync(serverContext);                        
                    }

                    ObservableCollection<EngineSession> engineSessions = GetEngineSessions(serverContext);
                    //var tasks = new List<Task<IEnumerable<byte>?>>(dataAccessProviders.Count);
                    if (!String.IsNullOrEmpty(recipientPath))
                    {
                        string beginRecipientId;
                        string remainingRecipientId;
                        int i = recipientPath.IndexOf('/');
                        if (i >= 0)
                        {
                            beginRecipientId = recipientPath.Substring(0, i);
                            remainingRecipientId = recipientPath.Substring(i + 1);
                        }
                        else
                        {
                            beginRecipientId = recipientPath;
                            remainingRecipientId = @"";
                        }
                        EngineSession? engineSession = engineSessions.FirstOrDefault(es =>
                            string.Equals(
                                es.DataAccessProviderGetter_Addon.InstanceId,
                                beginRecipientId,
                                StringComparison.InvariantCultureIgnoreCase));
                        if (engineSession is not null)
                            return await engineSession.DataAccessProvider.PassthroughAsync(remainingRecipientId, passthroughName, dataToSend);
                    }

                    foreach (EngineSession engineSession in engineSessions)
                    {
                        Logger.LogDebug("dataAccessProvider.Passthrough passthroughName=" + passthroughName);
                        try
                        {
                            var t = engineSession.DataAccessProvider.PassthroughAsync(recipientPath, passthroughName, dataToSend);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "dataAccessProvider.Passthrough passthroughName=" + passthroughName);
                        }
                    }                    
                    return ReadOnlyMemory<byte>.Empty;
                }
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Exception during passthrough."), ex.Message);
            }
        }        

        public override string LongrunningPassthrough(ServerContext serverContext, string recipientPath, string passthroughName, ReadOnlyMemory<byte> dataToSend)
        {
            string systemNameToConnect = serverContext.SystemNameToConnect;            
            if (systemNameToConnect == @"") // Utility context 
            {
                try
                {
                    switch (passthroughName)
                    {                                              
                        case LongrunningPassthroughConstants.SaveFiles:
                            return SaveFiles_LongrunningPassthrough(serverContext, dataToSend);                            
                        case LongrunningPassthroughConstants.DeleteFiles:
                            return DeleteFiles_LongrunningPassthrough(serverContext, dataToSend);                            
                        case LongrunningPassthroughConstants.MoveFiles:
                            return MoveFiles_LongrunningPassthrough(serverContext, dataToSend);                            
                        default:
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "Unknown passthroughName."));
                    }
                }
                catch (Exception ex)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Exception during passthrough."), ex.Message);
                }
            }
            else
            {
                string jobId = Guid.NewGuid().ToString();
                LongrunningPassthrough_ProcessContext(serverContext, jobId, recipientPath, passthroughName, dataToSend);
                return jobId;
            }
        }

        public override void LongrunningPassthroughCancel(ServerContext serverContext, string jobId)
        {
        }

        #endregion

        #region private functions    

        private async void LongrunningPassthrough_ProcessContext(ServerContext serverContext, string jobId, string recipientPath, string passthroughName, ReadOnlyMemory<byte> dataToSend)
        {
            ObservableCollection<EngineSession> engineSessions = GetEngineSessions(serverContext);

            var statusCodeTasks = new List<Task<uint>>();

            if (!String.IsNullOrEmpty(recipientPath))
            {
                string beginRecipientPath;
                string remainingRecipientPath;
                int i = recipientPath.IndexOf('/');
                if (i >= 0)
                {
                    beginRecipientPath = recipientPath.Substring(0, i);
                    remainingRecipientPath = recipientPath.Substring(i + 1);
                }
                else
                {
                    beginRecipientPath = recipientPath;
                    remainingRecipientPath = @"";
                }
                EngineSession? engineSession = engineSessions.FirstOrDefault(es =>
                    string.Equals(
                        es.DataAccessProviderGetter_Addon.InstanceId,
                        beginRecipientPath,
                        StringComparison.InvariantCultureIgnoreCase));
                if (engineSession is not null)
                {
                    Logger.LogDebug("dataAccessProvider.LongrunningPassthrough passthroughName=" + passthroughName);
                    statusCodeTasks.Add(await engineSession.DataAccessProvider.LongrunningPassthroughAsync(remainingRecipientPath, passthroughName, dataToSend, null));
                }
            }
            if (statusCodeTasks.Count == 0)
                foreach (EngineSession engineSession in engineSessions)
                {
                    Logger.LogDebug("dataAccessProvider.LongrunningPassthrough passthroughName=" + passthroughName);
                    statusCodeTasks.Add(await engineSession.DataAccessProvider.LongrunningPassthroughAsync(recipientPath, passthroughName, dataToSend, null));
                }

            bool allSucceeded = true;
            foreach (var statusCodeTask in statusCodeTasks)
            {
                try
                {
                    if (!StatusCodes.IsGood(await statusCodeTask))
                        allSucceeded = false;
                }
                catch
                {
                    allSucceeded = false;
                }
            }
            //if (!allSucceeded)
            //{
            //    serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
            //    {
            //        JobId = jobId,
            //        ProgressPercent = 100,
            //        ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationError_ProgressLabel, serverContext.CultureInfo),
            //        StatusCode = StatusCodes.BadInvalidArgument
            //    });
            //}
            //else
            //{
            //    serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
            //    {
            //        JobId = jobId,
            //        ProgressPercent = 100,
            //        ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationCompleted_ProgressLabel, serverContext.CultureInfo),
            //        StatusCode = StatusCodes.Good
            //    });
            //}
        }

        /// <summary>
        ///     Always throws.
        /// </summary>
        /// <param name="ex"></param>        
        private void ThrowRpcException(Exception? ex)
        {            
            if (ex is not null)
            {
                Logger.LogWarning(ex, "Failed Passthrough Result");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));                
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, @""));
            }            
        }
        
        private void GetDirectoryInfoPassthrough(ReadOnlyMemory<byte> dataToSend, out byte[] returnData)
        {
            try
            {
                var request = new GetDirectoryInfoRequest();
                SerializationHelper.SetOwnedData(request, dataToSend);
                DsFilesStoreDirectory dsFilesStoreDirectory = DsFilesStoreHelper.CreateDsFilesStoreDirectoryObject(FilesStoreDirectoryInfo,
                    request.PathRelativeToRootDirectory, request.FilesAndDirectoriesIncludeLevel);                
                returnData = SerializationHelper.GetOwnedData(dsFilesStoreDirectory);
            }
            catch (Exception ex)
            {
                returnData = new byte[0];
                ThrowRpcException(ex);
            }
        }

        /// <summary>
        ///     pathRelativeToRootCollection paths relative to the root of the Files Store.
        /// </summary>
        /// <param name="invariantPathRelativeToRootDirectoryCollection"></param>
        /// <param name="returnData"></param>
        /// <returns></returns>
        private void LoadFilesPassthrough(string invariantPathRelativeToRootDirectoryCollection, out byte[] returnData)
        {
            var reply = new LoadFilesReply();
            foreach (var invariantPathRelativeToRootDirectoryNullable in CsvHelper.ParseCsvLine(",", invariantPathRelativeToRootDirectoryCollection))
            {
                var invariantPathRelativeToRootDirectory = invariantPathRelativeToRootDirectoryNullable ?? @"";
                var fileInfo = new FileInfo(Path.Combine(FilesStoreDirectoryInfo.FullName, invariantPathRelativeToRootDirectory.Replace('/', Path.DirectorySeparatorChar)));

                if (!FileSystemHelper.IsSubPathOf(fileInfo.Directory!.FullName, FilesStoreDirectoryInfo.FullName))
                    throw new Exception("Access to file destination denied.");

                try
                {
                    reply.DsFilesStoreFileDatasCollection.Add(
                        new DsFilesStoreFileData
                        {
                            InvariantPathRelativeToRootDirectory = invariantPathRelativeToRootDirectory,
                            LastModified = fileInfo.LastWriteTimeUtc,
                            FileData = File.ReadAllBytes(fileInfo.FullName)
                        });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, Properties.Resources.FileReadingError + ": " + fileInfo.Name);                    
                }
            }
            returnData = SerializationHelper.GetOwnedData(reply);
        }

        private string SaveFiles_LongrunningPassthrough(ServerContext serverContext, ReadOnlyMemory<byte> dataToSend)
        {
            string jobId = Guid.NewGuid().ToString();

            SaveFiles_LongrunningPassthroughAsync(serverContext, jobId, dataToSend);

            return jobId;
        }

        private async void SaveFiles_LongrunningPassthroughAsync(ServerContext serverContext, string jobId, ReadOnlyMemory<byte> dataToSend)
        {            
            try
            {
                var request = new SaveFilesRequest();
                SerializationHelper.SetOwnedData(request, dataToSend);
                foreach (DsFilesStoreFileData dsFilesStoreFileData in request.DatasCollection)
                {
                    string fileFullName = Path.Combine(FilesStoreDirectoryInfo.FullName, dsFilesStoreFileData.PathRelativeToRootDirectory);
                    // Creates all directories and subdirectories in the specified path unless they already exist.
                    DirectoryInfo destinationDirectoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(fileFullName)!);

                    if (!FileSystemHelper.IsSubPathOf(destinationDirectoryInfo.FullName, FilesStoreDirectoryInfo.FullName))
                        throw new Exception("Access to file destination denied.");

                    // If the file to be deleted does not exist, no exception is thrown.
                    File.Delete(fileFullName); // For 'a' to 'A' changes in files names to work.
                    //     Asynchronously creates a new file, writes the specified byte array to the file,
                    //     and then closes the file. If the target file already exists, it is overwritten.
                    await File.WriteAllBytesAsync(fileFullName, dsFilesStoreFileData.FileData);
                    File.SetLastWriteTimeUtc(fileFullName, dsFilesStoreFileData.LastModified.UtcDateTime);
                }

                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Simcode.PazCheck.CentralServer.Properties.ResourceStrings.OperationCompleted_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.Good
                //});
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DeleteFilesLongrunningPassthrough error.");
                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationError_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.BadResourceUnavailable
                //});
            }
        }

        private string DeleteFiles_LongrunningPassthrough(ServerContext serverContext, ReadOnlyMemory<byte> dataToSend)
        {
            string jobId = Guid.NewGuid().ToString();

            try
            {
                var request = CsvHelper.ParseCsvLine(@",", Encoding.UTF8.GetString(dataToSend.Span));
                foreach (int index in Enumerable.Range(0, request.Length))
                {
                    string fileFullName = Path.Combine(FilesStoreDirectoryInfo.FullName, (request[index] ?? @"").Replace('/', Path.DirectorySeparatorChar));
                    try
                    {
                        // If the file to be deleted does not exist, no exception is thrown.
                        File.Delete(fileFullName);
                    }
                    catch
                    {
                    }                    
                }

                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationCompleted_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.Good
                //});
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DeleteFilesLongrunningPassthrough error.");
                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationError_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.BadResourceUnavailable
                //});
            }

            return jobId;
        }

        private string MoveFiles_LongrunningPassthrough(ServerContext serverContext, ReadOnlyMemory<byte> dataToSend)
        {
            string jobId = Guid.NewGuid().ToString();

            try
            {
                var request = CsvHelper.ParseCsvLine(@",", Encoding.UTF8.GetString(dataToSend.Span));                
                foreach (int index in Enumerable.Range(0, request.Length / 2))
                {
                    string sourceFileFullName = Path.Combine(FilesStoreDirectoryInfo.FullName, (request[2 * index] ?? @"").Replace('/', Path.DirectorySeparatorChar));
                    string destFileFullName = Path.Combine(FilesStoreDirectoryInfo.FullName, (request[2 * index + 1] ?? @"").Replace('/', Path.DirectorySeparatorChar));
                    File.Move(sourceFileFullName, destFileFullName, true);
                }

                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationCompleted_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.Good
                //});
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DeleteFilesLongrunningPassthrough error.");
                //serverContext.AddCallbackMessage(new ServerContext.LongrunningPassthroughCallbackMessage
                //{
                //    JobId = jobId,
                //    ProgressPercent = 100,
                //    ProgressLabel = Resources.ResourceManager.GetString(Properties.ResourceStrings.OperationError_ProgressLabel, serverContext.CultureInfo),
                //    StatusCode = StatusCodes.BadResourceUnavailable
                //});
            }

            return jobId;
        }        

        #endregion
    }
}