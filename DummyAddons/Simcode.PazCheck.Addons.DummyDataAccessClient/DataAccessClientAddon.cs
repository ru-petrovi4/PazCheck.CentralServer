using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common;
using Ssz.DataAccessGrpc.Client;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;
using Ssz.Utils.Logging;
using System;
using System.ComponentModel.Composition;

namespace Simcode.PazCheck.Addons.DummyDataAccessClient
{
    [Export(typeof(AddonBase))]
    public class DataAccessClientAddon : DataAccessProviderAddonBase
    {
        public static readonly Guid AddonGuid = new Guid(@"1D50751F-5BC0-4D5A-A9E1-A92FA21D1046");

        public static readonly string AddonName = @"DataAccessClient";

        public override Guid Guid => AddonGuid;

        public override string Name => AddonName;

        public override string Desc => Properties.Resources.DataAccessClientAddon_Desc;

        public override string Version => "1.0";        

        public static readonly string ServerAddress_OptionName = @"%(DataAccess_ServerAddress)";        

        public override (string, string)[] OptionsInfo => new (string, string)[] 
        {
            (ServerAddress_OptionName, Properties.Resources.ServerAddress_Option)            
        };

        public override IDataAccessProvider? GetDataAccessProvider(IDispatcher dispatcher)
        {
            string? serverAddress = CsvDb.GetValue(OptionsCsvFileName, ServerAddress_OptionName, 1);
            if (String.IsNullOrEmpty(serverAddress))
            {
                Logger.LogError("ServerAddress is null or empty.");
                return null;
            }

            IDataAccessProvider dataAccessProvider = ActivatorUtilities.CreateInstance<GrpcDataAccessProvider>(ServiceProvider, dispatcher);

            dataAccessProvider.Initialize(
                null,
                true,
                true,
                serverAddress,
                @"Simcode.PazCheck.Addons.DataAccessClient",
                Environment.MachineName,
                @"DCS",
                new CaseInsensitiveDictionary<string?>()
                );

            return dataAccessProvider;
        }        
    }
}