using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///    ����������� � ���������� ������� ������� ���
    /// </summary>
    [Resource]
    public class IntersectionResult : Identifiable<int>
    {
        /// <summary>
        ///     ������� �����������
        /// </summary>
        [HasOne]
        public CauseResult CauseResult { get; set; } = null!;

        /// <summary>
        ///     ���������� ����� ���������� ������ ������������ �������, ���� �������� �� ��������� �� ���� �������
        /// </summary>
        [Attr]
        public int? SubCauseNum { get; set; }

        /// <summary>
        ///     ��������� �����������
        /// </summary>
        [HasOne]
        public EffectResult EffectResult { get; set; } = null!;

        /// <summary>
        ///     TagConditionString_SymbolToDisplay ��� Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     ��� ������������ �����������
        /// </summary>
        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        /// <summary>
        ///     ����� ������������ �����������
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;
    }
}
