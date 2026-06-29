using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    /// <summary>
    ///     Index - BeginTimeUtc, PcObjectEventType
    /// </summary>
    public class PcObjectEvent
    {
        public string EventType { get; set; } = @"";

        public string BeginTimeUtc { get; set; } = @"";        

        public string? EndTimeUtc { get; set; }

        public List<Param>? EventParams { get; set; }

        public List<DbFileReference>? PcObjectEventDbFileReferences { get; set; }       
        
    }
}
