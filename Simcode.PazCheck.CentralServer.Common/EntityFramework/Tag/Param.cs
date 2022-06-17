using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     �������� ���� ��� ��������������� ���������.
    /// </summary>
    public class Param : VersionEntity
    {
        /// <summary>
        ///     <para>��������� ���� RW: ��� ��������</para>
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW: �������� ��������</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     <para>��������� ���� RW (����� �� ������ ���� ���� ��������): ������� ���������</para>
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
}
