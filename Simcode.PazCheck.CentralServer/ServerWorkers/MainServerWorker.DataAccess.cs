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
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Presentation;
using Ssz.Utils;
using Ssz.Utils.Logging;
using Ssz.Utils.Addons;
using Simcode.PazCheck.CentralServer.Common;
using System.Collections.Generic;
using System.Text;
using Ssz.Utils.DataAccess;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Simcode.PazCheck.CentralServer.Common.MicroServices;

namespace Simcode.PazCheck.CentralServer
{
    public partial class MainServerWorker
    {
        #region public functions
        
        public void NotifyEventMessages(string targetWorkstationName, EventMessagesCollection eventMessagesCollection)
        {
            DataAccessServerWorker.NotifyEventMessages(targetWorkstationName, eventMessagesCollection);
        }

        #endregion

        #region private functions

        private void OnDataAccessProviderGetter_Addon_Added(DataAccessProviderGetter_AddonBase addedDataAccessProviderGetter_Addon)
        {
            addedDataAccessProviderGetter_Addon.DataAccessProvider!.ContextStatusChanged += (s, e) => DataAccessProvider_OnContextStatusChanged(addedDataAccessProviderGetter_Addon, e);
            addedDataAccessProviderGetter_Addon.DataAccessProvider!.PropertyChanged += (s, e) => DataAccessProvider_OnPropertyChanged(addedDataAccessProviderGetter_Addon, e);
            
            DataAccessServerWorker.OnDataAccessProviderGetter_Addon_Added(addedDataAccessProviderGetter_Addon);            
        }        

        private void OnDataAccessProviderGetter_Addon_Removed(DataAccessProviderGetter_AddonBase removedDataAccessProviderGetter_Addon)
        {
            DataAccessServerWorker.OnDataAccessProviderGetter_Addons_Removed(removedDataAccessProviderGetter_Addon);
        }        

        private void DataAccessProvider_OnContextStatusChanged(DataAccessProviderGetter_AddonBase dataAccessProviderGetter_Addon, ContextStatusChangedEventArgs e)
        {
            if (dataAccessProviderGetter_Addon.DataAccessProvider is null)
                return;
            
            switch (e.ContextStateCode)
            {
                case ContextStateCodes.STATE_ABORTING:
                    _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                InformationSecurityEventsLogger.ComponentStartStop_EventId,
                                4,
                                true,
                                Properties.Resources.ComponentStopped_Event,
                                @"System",
                                dataAccessProviderGetter_Addon.Desc,
                                @"",
                                Properties.Resources.ComponentStopped_EventDesc, dataAccessProviderGetter_Addon.Desc);
                    break;                
            }
        }

        private void DataAccessProvider_OnPropertyChanged(DataAccessProviderGetter_AddonBase dataAccessProviderGetter_Addon, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDataAccessProvider.IsConnected))
            {
                if (dataAccessProviderGetter_Addon.DataAccessProvider is null)
                    return;

                if (dataAccessProviderGetter_Addon.DataAccessProvider!.IsConnected)
                {
                    _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                InformationSecurityEventsLogger.Connection_EventId,
                                4,
                                true,
                                Simcode.PazCheck.CentralServer.Common.Properties.Resources.Connected_Event,
                                @"System",
                                dataAccessProviderGetter_Addon.Desc,
                                @"",
                                Simcode.PazCheck.CentralServer.Common.Properties.Resources.Connected_EventDesc, dataAccessProviderGetter_Addon.Desc);
                }
                else
                {
                    _informationSecurityEventsLogger.InformationSecurityEvent(@"System",
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                new Uri(dataAccessProviderGetter_Addon.DataAccessProvider!.ServerAddress).Host,
                                InformationSecurityEventsLogger.Connection_EventId,
                                6,
                                true,
                                Simcode.PazCheck.CentralServer.Common.Properties.Resources.Disconnected_Event,
                                @"System",
                                dataAccessProviderGetter_Addon.Desc,
                                @"",
                                Simcode.PazCheck.CentralServer.Common.Properties.Resources.Disconnected_EventDesc, dataAccessProviderGetter_Addon.Desc);
                }
            }
        }

        #endregion        
    }
}
