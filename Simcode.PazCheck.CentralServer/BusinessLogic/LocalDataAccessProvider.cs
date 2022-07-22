using Microsoft.Extensions.Logging;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class LocalDataAccessProvider : DataAccessProviderBase
    {
        #region construction and destruction

        public LocalDataAccessProvider(ILogger<LocalDataAccessProvider> logger, IUserFriendlyLogger? userFriendlyLogger = null) :
            base(logger, userFriendlyLogger)
        {
        }

        #endregion

        #region public functions

        public override event Action<IDataAccessProvider, EventMessagesCollection> EventMessagesCallback = delegate { };

        /// <summary>
        ///     You can set updateValueItems = false and invoke PollElementValuesChangesAsync(...) manually.
        /// </summary>
        /// <param name="elementIdsMap"></param>
        /// <param name="elementValueListCallbackIsEnabled"></param>
        /// <param name="eventListCallbackIsEnabled"></param>
        /// <param name="serverAddress"></param>
        /// <param name="clientApplicationName"></param>
        /// <param name="clientWorkstationName"></param>
        /// <param name="systemNameToConnect"></param>
        /// <param name="contextParams"></param>
        /// <param name="callbackDispatcher"></param>
        public override void Initialize(ElementIdsMap? elementIdsMap,
            bool elementValueListCallbackIsEnabled,
            bool eventListCallbackIsEnabled,
            string serverAddress,
            string clientApplicationName,
            string clientWorkstationName,
            string systemNameToConnect,
            CaseInsensitiveDictionary<string?> contextParams,
            IDispatcher? callbackDispatcher)
        {
            base.Initialize(elementIdsMap,
                elementValueListCallbackIsEnabled,
                eventListCallbackIsEnabled,
                serverAddress,
                clientApplicationName,
                clientWorkstationName,
                systemNameToConnect,
                contextParams,
                callbackDispatcher);

            if (CallbackDispatcher is null || !EventListCallbackIsEnabled) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var previousWorkingTask = _workingTask;
            _workingTask = Task.Factory.StartNew(() =>
            {
                if (previousWorkingTask is not null)
                    previousWorkingTask.Wait();
                WorkingTaskMainAsync(cancellationToken).Wait();
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        ///     Tou can call Dispose() instead of this method.
        ///     Closes without waiting working thread exit.
        /// </summary>
        public override void Close()
        {
            if (!IsInitialized) return;

            base.Close();

            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        ///     Tou can call DisposeAsync() instead of this method.
        ///     Closes WITH waiting working thread exit.
        /// </summary>
        public override async Task CloseAsync()
        {
            Close();

            if (_workingTask is not null)
                await _workingTask;
        }

        public void NotifyEventMessages(EventMessagesCollection eventMessagesCollection)
        {
            ThreadSafeDispatcher.BeginInvoke(ct =>
            {
                _eventMessagesCollectionsQueue.Add(eventMessagesCollection);
            }
            );
        }

        #endregion

        #region protected functions

        /// <summary>
        ///     Dispacther for working thread.
        /// </summary>
        protected ThreadSafeDispatcher ThreadSafeDispatcher { get; } = new();

        #endregion

        #region private functions

        private async Task WorkingTaskMainAsync(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(1000);
                if (cancellationToken.IsCancellationRequested) break;

                var nowUtc = DateTime.UtcNow;

                //if (WrapperUserFriendlyLogger.IsEnabled(LogLevel.Debug))
                //    stopwatch.Restart();

                await DoWorkAsync(nowUtc, cancellationToken);

                //if (WrapperUserFriendlyLogger.IsEnabled(LogLevel.Debug))
                //{
                //    stopwatch.Stop();
                //    WrapperUserFriendlyLogger.LogDebug("DoWorkAsync, ElapsedMilliseconds: " + stopwatch.ElapsedMilliseconds);
                //}
            }
        }

        /// <summary>
        ///     On loop in working thread.
        /// </summary>
        /// <param name="nowUtc"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            await ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            foreach (var eventMessagesCollection in _eventMessagesCollectionsQueue.ToArray())
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (eventMessagesCollection.EventMessages.Count > 0)
                {
                    var сallbackDispatcher = CallbackDispatcher;
                    if (сallbackDispatcher is not null)
                    {
                        ElementIdsMap?.AddCommonFieldsToEventMessagesCollection(eventMessagesCollection);
                        try
                        {
                            сallbackDispatcher.BeginInvoke(ct =>
                            {
                                EventMessagesCallback(this, eventMessagesCollection);
                            });
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            _eventMessagesCollectionsQueue.Clear();

            //_lastScanTimeUtc = nowUtc;
        }

        #endregion

        #region private fields        

        private Task? _workingTask;

        private CancellationTokenSource? _cancellationTokenSource;

        //private DateTime _lastScanTimeUtc;

        private readonly List<EventMessagesCollection> _eventMessagesCollectionsQueue = new();

        #endregion
    }
}