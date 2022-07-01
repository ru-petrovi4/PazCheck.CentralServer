using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.DataAccess;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class DataAccessProviderAddonBase : AddonBase
    {        
        /// <summary>
        ///     Gets initialized IDataAccessProvider or writes to log and returns null.        
        /// </summary>
        /// <returns></returns>
        public abstract IDataAccessProvider? GetDataAccessProvider(IDispatcher dispatcher);
    }
}
