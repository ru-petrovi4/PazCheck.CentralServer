using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class DbFileReference : VersionEntityBase
    {
        [HasOne]
        public DbFile DbFile { get; set; } = null!;        
    }

    public class CeMatrixDbFileReference : DbFileReference
    {
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }

    public class BaseActuatorDbFileReference : DbFileReference
    {
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => BaseActuator;
    }
}
