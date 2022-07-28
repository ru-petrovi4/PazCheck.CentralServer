using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ��������
    /// </summary>    
    public abstract class Param : VersionEntityBase
    {
        /// <summary>
        ///     ��� ��������
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     �������� ��������
        ///     <para>��������� ���� RW: �������� ��������</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     ������� ���������
        ///     <para>��������� ���� RW (����� �� ������ (������� EngineeringUnit) ���� ���� ��������): ������� ���������</para>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        /// <summary>
        ///     ������ �� �������� ��������
        /// </summary>
        [HasOne]
        public ParamInfo? ParamInfo { get; set; }
    }

    /// <summary>
    ///     �������� ����
    /// </summary>
    [Resource]
    public class TagParam : Param
    {
        /// <summary>
        ///     ������������ ���
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    /// <summary>
    ///     �������� ��������������� ���������
    /// </summary>
    [Resource]
    public class ActuatorParam : Param
    {
        /// <summary>
        ///     ������������ ���
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    /// <summary>
    ///     �������� ������ ��������������� ���������
    /// </summary>
    [Resource]
    public class BaseActuatorParam : Param
    {
        /// <summary>
        ///     ������������ ������ ��������������� ���������
        /// </summary>
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => BaseActuator;
    }

    /// <summary>
    ///     �������� ������� ���
    /// </summary>
    [Resource]
    public class CeMatrixParam : Param
    {
        /// <summary>
        ///     ������������ �������
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}