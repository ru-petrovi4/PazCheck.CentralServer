using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class PcEntityType
    {
        public string Type
        {
            get => _type;
            set => _type = value.Trim();
        }

        public string Title { get; set; } = @"";

        public string Desc { get; set; } = @"";

        public DbFileReference? IconDbFileReference { get; set; }

        public List<ParamInfo>? StandardParamInfos { get; set; }

        #region private fields

        private string _type = @"";

        #endregion
    }

    public class ProjectVersionType : PcEntityType
    {
    }
    
    public class PcObjectEventType : PcEntityType
    {        
    }
}
