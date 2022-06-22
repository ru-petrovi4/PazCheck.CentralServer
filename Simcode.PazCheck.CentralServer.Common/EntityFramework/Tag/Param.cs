using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     �������� ���� ��� ��������������� ���������.
    /// </summary>    
    public abstract class Param : VersionEntityBase
    {
        /// <summary>
        ///     ��� ��������
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW: �������� ��������</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW (����� �� ������ (������� EngineeringUnit) ���� ���� ��������): ������� ���������</para>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        /// <summary>
        ///     Reserved
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW: �������� ��������</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }

    [Resource]
    public class TagParam : Param
    {
        [HasOne]
        public Tag Tag { get; set; } = null!;
    }

    [Resource]
    public class ActuatorParam : Param
    {
        [HasOne]
        public Tag Tag { get; set; } = null!;
    }

    [Resource]
    public class BaseActuatorParam : Param
    {
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;
    }
}
