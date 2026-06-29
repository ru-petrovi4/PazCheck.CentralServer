using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [NoResource]
    [PrimaryKey(nameof(UserGroup), nameof(ParamName))]
    public class UserParam
    {
        public string UserGroup { get; set; } = @"";

        public string ParamName { get; set; } = @"";

        public string Value { get; set; } = @"";
    }
}
