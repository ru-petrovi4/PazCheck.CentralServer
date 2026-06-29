using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    /// <summary>
    ///     Помечает сущность. Create, Update, Delete попадает в LogEvent
    /// </summary>
    public class InformationSecurityEventEntityAttribute : Attribute
    {
        public bool AllRolesAccess { get; set; }
    }
}
