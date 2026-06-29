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
    public class PcObject
    {
        public string Identifier
        {
            get => _identifier;
            set => _identifier = value.Trim();
        }

        public string? Unit { get; set; }

        public string? Widgets { get; set; }        

        public List<Param>? Params { get; set; } = new(); // Clears collection when imported

        public List<JournalParamValuesCollection>? JournalParamValuesCollections { get; set; }

        public List<PcObjectEvent>? PcObjectEvents { get; set; }

        public List<DbFileReference>? PcObjectDbFileReferences { get; set; }

        #region private fields

        private string _identifier = @"";

        #endregion
    }
}
