using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ��������� ����.
    /// </summary>
    [Resource]
    public class TagCondition : VersionEntity
    {
        /// <summary>
        ///     <para>��������� ���� RW (����� �� ������ (������� TagConditionIdentifier) ���� ���� ��������): ������������� ���������</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Reserved
        /// </summary>
        [Attr]
        public string MathOperator { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW: �������� ���������</para>
        ///     <example>�����</example><example>����</example>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";        

        /// <summary>
        ///     <para>��������� ���� RW: ������� ���</para>
        ///     <para>Tooltip: ������� ��� ��������� ��� ����������� � �������� ���</para>
        ///     <example>Alarm</example>  
        /// </summary>
        [Attr]
        public string SymbolToDisplay { get; set; } = @"";        

        [HasOne]
        public Tag Tag { get; set; } = null!;
    }
}
