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
    public class MetaParam
    {
        public string ParamName { get; set; } = @"";

        public string ParamNameLower { get; set; } = @"";

        public string Value { get; set; } = @"";        

        /// <summary>
        ///     Время последнего изменения
        /// </summary>        
        public DateTime _LastChangeTimeUtc { get; set; }        

        public string Type { get; set; } = @"";

        public bool IsTemp { get; set; }

        /// <summary>
        ///     Hub Group
        /// </summary>
        public string Group { get; set; } = @"";

        /// <summary>
        ///     Hub Method, Если InternalEvent, то внутреннее событие
        /// </summary>
        public string Method { get; set; } = @"";
        
        public bool HasArg { get; set; }
    }

    [NoResource]    
    public class MetaParamArg
    {
        public string ParamName { get; set; } = @"";

        public string ParamNameLower { get; set; } = @"";

        /// <summary>
        ///     ConnectionIds to exclude from sending messages.
        /// </summary>
        public string ExcludeConnectionIds { get; set; } = @"";

        /// <summary>
        ///     See <see cref="Ssz.Utils.NameValueCollectionHelper"/>
        /// </summary>
        public string Arg { get; set; } = @"";
    }
}
