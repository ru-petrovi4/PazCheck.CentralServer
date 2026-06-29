using System;
using System.Collections.Generic;
using System.Threading;
using Ssz.DataAccessGrpc.ServerBase;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Ssz.Utils.DataAccess;
using Ssz.Utils;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Ssz.Dcs.CentralServer.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using Ssz.Utils.Addons;
using System.Linq;
using Ssz.Dcs.CentralServer.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Ssz.Dcs.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.MicroServices;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region construction and destruction

        public DataAccessServerWorker(                
                ILogger<DataAccessServerWorker> logger,                
                IConfiguration configuration,
                AddonsManager addonsManager) :
            base(logger)
        {  
            _configuration = configuration;
            _addonsManager = addonsManager;

            // Creates all directories and subdirectories in the specified path unless they already exist.
            Directory.CreateDirectory(@"FilesStore");
            FilesStoreDirectoryInfo = new DirectoryInfo(@"FilesStore");

            ServerContextAddedOrRemoved += On_ServerContextAddedOrRemoved;
        }        

        #endregion

        #region public functions  

        public DirectoryInfo FilesStoreDirectoryInfo { get; private set; }        

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await base.InitializeAsync(cancellationToken);
        }

        public override async Task<int> DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) 
                return 0;

            Cleanup(nowUtc, cancellationToken);

            if (cancellationToken.IsCancellationRequested) 
                return 0;

            DoWorkUtilityItems(nowUtc, cancellationToken);

            return await base.DoWorkAsync(nowUtc, cancellationToken);
        }

        public override async Task CloseAsync()
        {
            await base.CloseAsync();
        }

        #endregion

        #region private fields

        private readonly IConfiguration _configuration;

        private readonly AddonsManager _addonsManager;

        /// <summary>
        ///     [ProcessModelingSessionId, ProcessModelingSession]
        /// </summary>
        private readonly CaseInsensitiveOrderedDictionary<ProcessModelingSession> _processModelingSessionsCollection = new();               

        #endregion
    }
}