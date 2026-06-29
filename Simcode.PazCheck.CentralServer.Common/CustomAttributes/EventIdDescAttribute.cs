using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class EventIdDescAttribute : Attribute
    {
        #region construction and destruction

        public EventIdDescAttribute(string desc)            
        {
            Desc = desc;            
        }

        #endregion

        #region public functions

        public string Desc { get; set; }

        #endregion
    }
}
