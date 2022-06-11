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
        ///     ���������� ����� ������ ���������
        /// </summary>
        [Attr(PublicName = "num")]
        public int Num { get; set; }

        [HasMany(PublicName = "subcauses")]
        public List<SubCause> SubCauses { get; set; } = new();

        public CeMatrix CeMatrix { get; set; } = null!;
    }
}
