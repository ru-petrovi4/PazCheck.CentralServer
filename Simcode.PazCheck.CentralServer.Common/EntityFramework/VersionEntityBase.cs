using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class VersionEntityBase : Identifiable<int>
    {
        [Attr]
        public DateTime _CreateTimeUtc { get; set; }

        [Attr]
        public DateTime? _DeleteTimeUtc { get; set; }
        
        public bool IsActive(DateTime? timeUtc)
        {
            if (timeUtc is null)
            {
                return _DeleteTimeUtc is null;
            }
            else
            {
                return _CreateTimeUtc <= timeUtc && (_DeleteTimeUtc is null || _DeleteTimeUtc > timeUtc);
            }
        }
    }
}
