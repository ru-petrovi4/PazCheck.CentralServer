using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Logging;
using Ssz.Utils.Addons;
using Simcode.PazCheck.CentralServer.Common;
using System.Collections.Generic;
using System.Text;
using Ssz.Utils.DataAccess;
using System.Runtime.CompilerServices;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using System.Collections.Specialized;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Simcode.PazCheck.CentralServer.Common.MicroServices;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices
{
    public class Logging
    {
        #region construction and destruction

        public Logging(            
            AddonsManager addonsManager,
            JobsManager jobsManager,
            IDbContextFactory<PazCheckDbContext> dbContextFactory,
            IConfiguration configuration,
            IHostApplicationLifetime applicationLifetime,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILogger<Logging> logger)
        {            
            _addonsManager = addonsManager;
            _jobsManager = jobsManager;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;
            _logger = logger;
            _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
        }

        #endregion

        #region public functions

        public void Initialize(CaseInsensitiveOrderedDictionary<string?> addonOptionsSubstituted)
        {
            _addonOptionsSubstituted = addonOptionsSubstituted;            
        }

        public async Task DoWorkAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            if (!ConfigurationHelper.IsMainProcess(_configuration))
                return;

            if (nowUtc < _lastDoWorkDateTimeUtc + TimeSpan.FromSeconds(60)) // 1 minute
                return;
            _lastDoWorkDateTimeUtc = nowUtc;

            using var dbContext = _dbContextFactory.CreateDbContext();
            
            var informationSecurityEvents_MaxRecordsCount = new Any(_addonOptionsSubstituted.TryGetValue(LoggingAddon.InformationSecurityEvents_MaxRecordsCount_OptionName)).ValueAsInt32(false);
            var informationSecurityEvents_MaxStorageTime = new Any(_addonOptionsSubstituted.TryGetValue(LoggingAddon.InformationSecurityEvents_MaxStorageTime_OptionName)).ValueAs<TimeSpan>(false);
            if (informationSecurityEvents_MaxRecordsCount > 0 && informationSecurityEvents_MaxRecordsCount < Int32.MaxValue)
            {
                var count = await dbContext.InformationSecurityEvents.CountAsync();
                if (count > informationSecurityEvents_MaxRecordsCount)
                    await dbContext.InformationSecurityEvents.OrderBy(le => le.EventTimeUtc).Take(count - informationSecurityEvents_MaxRecordsCount).ExecuteDeleteAsync();

                //if (informationSecurityEvents_WarningRecordsCount > 0)
                //{
                //    if (count > informationSecurityEvents_WarningRecordsCount)
                //    {
                //        CaseInsensitiveOrderedDictionary<string?> params_ = new()
                //        {
                //            { @"EventType", @"InformationSecurityEvents_WarningRecordsCount" }
                //        };

                //        _informationSecurityEventsLogger.InformationSecurityEvent("System",
                //                        @"127.0.0.1",
                //                        @"localhost",
                //                        InformationSecurityEventsLogger.WarningRecordsCount_EventId,
                //                        9,
                //                        false,
                //                        Common.Properties.Resources.InformationSecurityEvents_WarningRecordsCount_EventName,
                //                        @"System",
                //                        @"InformationSecurityEvents",
                //                        NameValueCollectionHelper.GetNameValueCollectionString(params_),
                //                        Common.Properties.Resources.InformationSecurityEvents_WarningRecordsCount_EventDesc);

                //        EventMessagesCollection eventMessagesCollection = new();
                //        eventMessagesCollection.EventMessages.Add(new EventMessage(new Ssz.Utils.DataAccess.EventId())
                //        {
                //            EventType = EventType.SimpleAlarm,
                //            Fields = params_
                //        });
                //        _dataAccessManager.LocalDataAccessProvider.NotifyEventMessages(eventMessagesCollection);
                //    }
                //}                
            }
            if (informationSecurityEvents_MaxStorageTime > TimeSpan.Zero)
            {
                DateTime timeUtc = DateTime.UtcNow - informationSecurityEvents_MaxStorageTime;
                await dbContext.InformationSecurityEvents.Where(le => le.EventTimeUtc < timeUtc).ExecuteDeleteAsync();
            }
            
            var userEvents_MaxRecordsCount = new Any(_addonOptionsSubstituted.TryGetValue(LoggingAddon.UserEvents_MaxRecordsCount_OptionName)).ValueAsInt32(false);
            var userEvents_MaxStorageTime = new Any(_addonOptionsSubstituted.TryGetValue(LoggingAddon.UserEvents_MaxStorageTime_OptionName)).ValueAs<TimeSpan>(false);
            if (userEvents_MaxRecordsCount > 0)
            {
                var count = await dbContext.UserEvents.CountAsync();
                if (count > userEvents_MaxRecordsCount)
                    await dbContext.UserEvents.OrderBy(le => le.EventTimeUtc).Take(count - userEvents_MaxRecordsCount).ExecuteDeleteAsync();

                //if (userEvents_WarningRecordsCount > 0)
                //{
                //    if (count > userEvents_WarningRecordsCount)
                //    {
                //        // TODO
                //        //CaseInsensitiveOrderedDictionary<string?> params_ = new()
                //        //{
                //        //    { @"EventType", @"UserEvents_WarningRecordsCount" }
                //        //};

                //        //_informationSecurityEventsLogger.InformationSecurityEvent("System",
                //        //                @"127.0.0.1",
                //        //                @"localhost",
                //        //                InformationSecurityEventsLogger.WarningRecordsCount_EventId,
                //        //                9,
                //        //                false,
                //        //                Properties.Resources.UserEvents_WarningRecordsCount_EventName,
                //        //                @"System",
                //        //                @"UserEvents",
                //        //                NameValueCollectionHelper.GetNameValueCollectionString(params_),
                //        //                Properties.Resources.UserEvents_WarningRecordsCount_EventDesc);

                //        //EventMessagesCollection eventMessagesCollection = new();
                //        //eventMessagesCollection.EventMessages.Add(new EventMessage(new Ssz.Utils.DataAccess.EventId())
                //        //{
                //        //    EventType = EventType.SimpleAlarm,
                //        //    Fields = params_
                //        //});
                //        //_dataAccessManager.LocalDataAccessProvider.NotifyEventMessages(eventMessagesCollection);
                //    }
                //}                
            }
            if (userEvents_MaxStorageTime > TimeSpan.Zero)
            {
                DateTime timeUtc = DateTime.UtcNow - userEvents_MaxStorageTime;
                await dbContext.UserEvents.Where(le => le.EventTimeUtc < timeUtc).ExecuteDeleteAsync();
            }
        }

        public void Close()
        {            
        }

        #endregion        

        #region private fields
        
        private readonly AddonsManager _addonsManager;
        private readonly JobsManager _jobsManager;
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly ILogger _logger;
        private readonly CancellationToken _applicationStopping_CancellationToken;

        private CaseInsensitiveOrderedDictionary<string?> _addonOptionsSubstituted = null!;

        /// <summary>
        ///     [Unit.Identifier, UnitEventsProcessingInfo]
        /// </summary>
        private readonly CaseInsensitiveOrderedDictionary<UnitEventsProcessingInfo> _unitEventsProcessingInfos = new();

        private DateTime _lastDoWorkDateTimeUtc;

        #endregion

        private class UnitEventsProcessingInfo
        {
            public Unit Unit { get; set; } = null!;

            public bool InformationSecurityEventIsSent { get; set; }

            public DateTime? UnitEventsRecievedTimeUtc { get; set; }
        }
    }
}