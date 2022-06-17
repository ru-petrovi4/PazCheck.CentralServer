using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     �������������� ��������.
    /// </summary>
    [Resource]
    public class Actuator : VersionEntity
    {
        /// <summary>        
        ///     <para>��������� ���� RW: �������� ��������������� ���������</para>
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>        
        ///     <para>��������� ���� RW: ��������</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        [HasMany]
        public List<Param> Params { get; set; } = new();
    }
}
