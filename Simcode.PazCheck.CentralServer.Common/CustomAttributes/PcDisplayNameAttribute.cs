using Simcode.PazCheck.CentralServer.Common.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    [AttributeUsage(AttributeTargets.All)]
    public class PcDisplayNameAttribute : DisplayNameAttribute
    {   
        #region construction and destruction

        public PcDisplayNameAttribute(string resourceName)
        {            
            _resourceName = resourceName;
        }

        #endregion

        #region public functions

        public override string DisplayName => Resources.ResourceManager.GetString(_resourceName) ?? "";

        #endregion

        #region private fields

        private readonly string _resourceName;

        #endregion
    }
}
