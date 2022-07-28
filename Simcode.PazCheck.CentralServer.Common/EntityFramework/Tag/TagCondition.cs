using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ��������� ����
    /// </summary>
    [Resource]
    public class TagCondition : VersionEntityBase
    {
        /// <summary>
        ///     ������������� ��������� (ALARM, PVHighHigh � �.�.)
        ///     <para>��������� ���� RW (����� �� ������ (������� TagConditionInfo) ���� ���� ��������): ������������� ���������</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     �������� ���������
        ///     <para>��������� ���� RW: �������� ���������</para>
        ///     <example>�����</example><example>����</example>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     ������� ��� ��������� ��� ����������� ������������ � �������� ���
        ///     <para>��������� ���� RW: ������� ���</para>
        ///     <para>Tooltip: ������� ��� ��������� ��� ����������� � �������� ���</para>
        ///     <example>Alarm</example>  
        /// </summary>
        [Attr]
        public string SymbolToDisplay { get; set; } = @"";

        /// <summary>
        ///     ����� ���� �������� � �������� ���
        ///     <para>������� RW: ����� ���� ��������</para>
        ///     <para>Tooltip: ����� ���� �������� � �������� ���</para>
        /// </summary>
        [Attr]
        public bool CanBeCause { get; set; } = true;

        /// <summary>
        ///     ����� ���� ���������� � �������� ���
        ///     <para>������� RW: ����� ���� ����������</para>
        ///     <para>Tooltip: ����� ���� ���������� � �������� ���</para>
        /// </summary>
        [Attr]
        public bool CanBeEffect { get; set; } = true;

        /// <summary>
        ///     ������ �� �������� ���������
        /// </summary>
        [HasOne]
        public TagConditionInfo? TagConditionInfo { get; set; }
        
        /// <summary>
        ///     ������������ ���
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }
}