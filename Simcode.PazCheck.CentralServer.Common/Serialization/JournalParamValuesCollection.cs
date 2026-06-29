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
    public class JournalParamValuesCollection
    {
        public string Name { get; set; } = @"";

        public string? MetadataFields { get; set; }

        public List<FloatJournalParamValue>? FloatValues { get; set; }        
    }
}


//public List<Int32JournalParamValue>? Int32Values { get; set; }

//public List<StringJournalParamValue>? StringValues { get; set; }