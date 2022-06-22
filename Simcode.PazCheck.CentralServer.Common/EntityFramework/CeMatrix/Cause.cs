using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Cause : VersionEntityBase
    {
        /// <summary>
        ///     Порядковый номер внутри диаграммы
        /// </summary>
        [Attr]
        public int Num { get; set; }

        [HasMany]
        public List<SubCause> SubCauses { get; set; } = new();

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;
    }
}
