using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class DefaultRoleBusinessFunction_RolesAttribute : Attribute
    {
        #region construction and destruction

        public DefaultRoleBusinessFunction_RolesAttribute(string roles)            
        {
            Roles = roles + ",SuperUser";            
        }

        #endregion

        #region public functions

        public string Roles { get; set; }

        #endregion
    }
}
