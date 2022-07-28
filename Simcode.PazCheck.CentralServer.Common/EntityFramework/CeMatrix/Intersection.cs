using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ����������� � ������� ���
    /// </summary>
    [Resource]
    public class Intersection : VersionEntityBase
    {
        /// <summary>
        ///     TagConditionString_SymbolToDisplay ��� Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     ������� �����������
        /// </summary>
        [HasOne]
        public Cause Cause { get; set; } = null!;

        /// <summary>
        ///     ���������� ����� ���������� ������ ������������ �������, ���� �������� �� ��������� �� ���� �������
        /// </summary>
        [Attr]
        public int? SubCauseNum { get; set; }

        /// <summary>
        ///     ��������� �����������
        /// </summary>
        [HasOne]
        public Effect Effect { get; set; } = null!;

        /// <summary>
        ///     ������������ �������
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}
