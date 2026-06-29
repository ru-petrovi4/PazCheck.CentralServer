using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    [Index(nameof(SourcePath), nameof(AddonInstanceId), IsUnique = true)]
    public class AddonStatus : Identifiable<int>
    {
        [Attr]
        public DateTime TimestampUtc { get; set; }
        
        /// <summary>        
        ///     
        /// </summary>   
        [Attr]
        public string SourcePath { get; set; } = @"";

        /// <summary>        
        ///     
        /// </summary>   
        [Attr]
        public string SourceId { get; set; } = @"";

        /// <summary>        
        ///     
        /// </summary>        
        [Attr]
        public string SourceIdToDisplay { get; set; } = @"";

        /// <summary>
        /// 
        /// </summary>
        [Attr]
        public Guid AddonGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Attr]
        public string AddonIdentifier { get; set; } = @"";

        /// <summary>
        /// 
        /// </summary>
        [Attr]
        public string AddonDesc { get; set; } = @"";

        /// <summary>
        /// 
        /// </summary>
        [Attr]
        public string AddonInstanceId { get; set; } = @"";

        /// <summary>
        /// 
        /// </summary>
        [Attr]
        public DateTime? LastWorkTimeUtc { get; set; }

        /// <summary>
        ///     See consts in <see cref="Ssz.Utils.Addons.AddonStateCodes"/>
        /// </summary>
        [Attr]
        public uint StateCode { get; set; }

        /// <summary>
        ///     State Info (Invariant culture)
        /// </summary>
        [Attr]
        public string Info { get; set; } = @"";

        /// <summary>
        ///      User-friendly label (Configured UI culture)
        /// </summary>
        [Attr]
        public string Label { get; set; } = @"";

        /// <summary>
        ///      User-friendly details (Configured UI culture)
        /// </summary>
        [Attr]
        public string Details { get; set; } = @"";
    }
}
