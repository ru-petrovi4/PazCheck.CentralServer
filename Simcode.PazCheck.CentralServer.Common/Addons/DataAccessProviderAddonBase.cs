using Ssz.Utils;
using Ssz.Utils.DataAccess;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class DataAccessProviderAddonBase : AddonBase
    {
        public static readonly string EventMessagesProcessingAddonName_OptionName = @"%(EventMessagesProcessingAddonName)";

        /// <summary>
        ///     Gets initialized IDataAccessProvider or writes to log and returns null.        
        /// </summary>
        /// <returns></returns>
        public abstract IDataAccessProvider? GetDataAccessProvider(IDispatcher dispatcher);
    }
}
