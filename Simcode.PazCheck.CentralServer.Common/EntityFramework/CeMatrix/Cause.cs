using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Cause : VersionEntity
    {
        /// <summary>
        ///     Порядковый номер внутри диаграммы
        /// </summary>
        [Attr(PublicName = "num")]
        public int Num { get; set; }

        [HasOne(PublicName = "project")]
        public Project Project { get; set; } = null!;       

        [HasMany(PublicName = "subcauses")]
        public List<SubCause> SubCauses { get; set; } = new();
    }
}
