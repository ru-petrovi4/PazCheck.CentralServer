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
using System.Linq;
using System.Text;
using Simcode.PazCheck.CentralServer.ServerListItems;

namespace Simcode.PazCheck.CentralServer
{
    public partial class DataAccessServerWorker : Ssz.DataAccessGrpc.ServerBase.DataAccessServerWorkerBase
    {
        #region public functions
        
        public Guid UtilityDataGuid { get; set; } = Guid.Empty;

        public void AddUtilityElementValueListItem(UtilityElementValueListItem listItem, string clientWorkstationName)
        {
            string id = GetUtilityItemsKey(listItem.ElementId, clientWorkstationName);            
            if (!_utilityItems.TryGetValue(id, out UtilityItem? utilityItem))
            {
                utilityItem = new UtilityItem(listItem.ElementId, clientWorkstationName);
                _utilityItems.Add(id, utilityItem);                

                _utilityItemsDoWorkNeeded = true;
            }
            utilityItem.UtilityElementValueListItemsCollection.Add(listItem);
            listItem.IsReadable = true;
            listItem.IsWritable = false;
            listItem.UpdateValueStatusTimestamp(utilityItem.ValueStatusTimestamp);
        }
        
        public void RemoveUtilityElementValueListItem(UtilityElementValueListItem listItem, string clientWorkstationName)
        {
            string id = GetUtilityItemsKey(listItem.ElementId, clientWorkstationName);
            if (!_utilityItems.TryGetValue(id, out UtilityItem? utilityItem))
            {
                return;
            }
            utilityItem.UtilityElementValueListItemsCollection.Remove(listItem);
            if (utilityItem.UtilityElementValueListItemsCollection.Count == 0)
            {
                _utilityItems.Remove(id);
            }
        }

        /// <summary>
        ///     Reruns Status Code <see cref="Ssz.Utils.StatusCodes"/>
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="value"></param>
        public uint WriteUtilityElementValueListItem(UtilityElementValueListItem listItem, Any value)
        {
            //string elementId = listItem.ElementId;

            //if (!_utilityItems.TryGetValue(elementId, out UtilityItem? utilityItem))
            //{
            //    utilityItem = new UtilityItem(elementId);
            //    _utilityItems.Add(elementId, utilityItem);                
            //}
            //utilityItem.UpdateValue(value.ValueAsString(false), DateTime.UtcNow);

            //_utilityItemsDoWorkNeeded = true;

            return StatusCodes.Good;
        }

        #endregion

        #region private function

        /// <summary>
        ///     On loop in working thread.
        /// </summary>
        /// <param name="nowUtc"></param>
        /// <param name="cancellationToken"></param>
        private void DoWorkUtilityItems(DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (_utilityItemsDoWorkNeeded)
            {
                _utilityItemsDoWorkNeeded = false;                

                UtilityDataGuid = Guid.NewGuid();
            }
        } 

        private string GetUtilityItemsKey(string elementId, string clientWorkstationName)
        {
            if (String.Equals(elementId, DataAccessConstants.UtilityItem_CentralServer, StringComparison.InvariantCultureIgnoreCase))
                return elementId + "@" + clientWorkstationName;
            
            return elementId;
        }

        #endregion

        #region private fields       

        private volatile bool _utilityItemsDoWorkNeeded;

        /// <summary>
        ///     [id, UtilityItem]
        /// </summary>
        private readonly CaseInsensitiveOrderedDictionary<UtilityItem> _utilityItems = new(256);        

        #endregion

        private class UtilityItem
        {
            #region construction and destruction

            public UtilityItem(string elementId, string clientWorkstationName)
            {
                ElementId = elementId;
                ClientWorkstationName = clientWorkstationName;
            }

            #endregion

            #region public functions

            public string ElementId { get; }

            public string ClientWorkstationName { get; }

            public List<UtilityElementValueListItem> UtilityElementValueListItemsCollection { get; } = new();

            public ValueStatusTimestamp ValueStatusTimestamp { get { return _valueStatusTimestamp; } }            

            /// <summary>
            ///     Checks for value change.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="nowUtc"></param>
            public void UpdateValue(string value, DateTime nowUtc)
            {
                bool updated = false;
                if (StatusCodes.IsUncertain(_valueStatusTimestamp.StatusCode))
                {
                    _valueStatusTimestamp = new ValueStatusTimestamp(new Any(value), StatusCodes.Good, nowUtc);
                    updated = true;
                }
                else
                {
                    if (_valueStatusTimestamp.Value.ValueAsString(false) != value)
                    {
                        _valueStatusTimestamp.Value.Set(value);
                        _valueStatusTimestamp.TimestampUtc = nowUtc;
                        updated = true;
                    }
                }
                if (updated)
                {
                    foreach (UtilityElementValueListItem utilityElementValueListItem in UtilityElementValueListItemsCollection)
                    {
                        utilityElementValueListItem.UpdateValueStatusTimestamp(_valueStatusTimestamp);
                    }
                }    
            }

            #endregion            

            #region private fields

            private ValueStatusTimestamp _valueStatusTimestamp = new ValueStatusTimestamp { StatusCode = StatusCodes.Uncertain };

            #endregion
        }
    }
}

// string utilityItemValue = ConfigurationHelper.GetValue<string>(_configuration, @"Kestrel:Endpoints:HttpsDefaultCert:Url", @"");