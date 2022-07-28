using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ������� � ������� ���
    /// </summary>
    [Resource]
    public class Cause : VersionEntityBase
    {
        /// <summary>
        ///     ���������� ����� ������ �������
        /// </summary>
        [Attr]
        public int Num { get; set; }

        [HasMany]
        public List<SubCause> SubCauses { get; set; } = new();

        /// <summary>
        ///     ������������ �������
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
