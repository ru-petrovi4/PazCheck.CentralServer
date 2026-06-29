using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class ExportFilesInfo
    {
        public int[] DbFileIds { get; set; } = null!;

        public string[] FileRelativePathAndNames { get; set; } = null!;
    }
}
