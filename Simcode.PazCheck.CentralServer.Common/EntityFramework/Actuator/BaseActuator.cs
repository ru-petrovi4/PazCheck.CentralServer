using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class BaseActuator : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Title { get; set; } = @"";
        
        public string Desc { get; set; } = @"";

        public int ProjectId { get; set; }

        [HasOne(PublicName = "project")]
        public Project Project { get; set; } = null!;

        [HasMany(PublicName = "actuatorparams")]
        public List<BaseActuatorParam> BaseActuatorParams { get; set; } = new List<BaseActuatorParam>();        
    }
}
