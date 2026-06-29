using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.MicroServices;
using Ssz.DataAccessGrpc.ServerBase;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public class DataAccessBackgroundService : BackgroundService
    {
        #region construction and destruction

        public DataAccessBackgroundService(
            ILogger<DataAccessBackgroundService> logger,
            MainServerWorker mainServerWorker)
        {
            Logger = logger;
            _mainServerWorker = mainServerWorker;
        }

        #endregion

        #region public functions

        public ILogger<DataAccessBackgroundService> Logger { get; }

        #endregion

        #region protected functions

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(3, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _mainServerWorker.DataAccessServerWorker.DoWorkAsync(DateTime.UtcNow, cancellationToken);
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, @"DataAccessBackgroundService.DataAccessServerWorker_TaskDataAccessAsync(...) Exception");
                }
            }
        }

        #endregion        

        #region private fields

        private readonly MainServerWorker _mainServerWorker;

        #endregion
    }
}
