using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class EntitiesCollectionInfo
    {
        public bool IncludeAll { get; set; }

        public int[]? IdsToInclude { get; set; }

        public int[]? IdsToExclude { get; set; }

        public Filter? Filter { get; set; }
    }
}
