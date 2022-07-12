using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class VersionEntityBase : Identifiable<int>
    {
        [Attr]
        public UInt32? _CreateProjectVersionNum { get; set; }

        [Attr]
        public UInt32? _DeleteProjectVersionNum { get; set; }

        [Attr]
        public bool _IsDeleted { get; set; }

        [Attr]
        public string _LastChangeUser { get; set; } = @"";

        [Attr]
        public DateTime _LastChangeTimeUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive(UInt32? projectVersionNum)
        {
            if (projectVersionNum is null)
            {
                return !_IsDeleted;
            }
            else
            {
                return (_CreateProjectVersionNum is not null && _CreateProjectVersionNum <= projectVersionNum) && 
                    (_DeleteProjectVersionNum is null || _DeleteProjectVersionNum > projectVersionNum);
            }
        }
    }
}
