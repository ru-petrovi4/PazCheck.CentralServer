using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class DefaultMethod_RoleBusinessFunctionsAttribute : Attribute
    {
        #region construction and destruction
        
        public DefaultMethod_RoleBusinessFunctionsAttribute(string[] access)            
        {
            Access = String.Join(@",", access.Concat(new string[] { @"SuperUser" }).ToArray());             
        }

        #endregion

        #region public functions

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string Access { get; set; }

        #endregion
    }
}
