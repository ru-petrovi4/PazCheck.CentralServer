using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public abstract class ProjectVersionedEntityBase
    {        
        public string Identifier 
        {
            get => _identifier;
            set => _identifier = value.Trim();
        }

        public List<Param>? Params { get; set; }

        public List<DbFileReference>? DbFileReferences { get; set; }

        #region private fields

        private string _identifier = @"";

        #endregion
    }
}
