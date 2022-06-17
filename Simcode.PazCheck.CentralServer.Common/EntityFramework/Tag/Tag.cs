#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    [Index(nameof(IsActive), nameof(TagName))]
    public class Tag : VersionEntity
    {
        /// <summary>        
        ///     <para>��������� ���� RW: ��� ����</para>
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>        
        ///     <para>��������� ���� RW: ��������</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";        

        /// <summary>        
        ///     ��������� ����.
        /// </summary>
        [HasMany]
        public List<TagCondition> TagConditions { get; set; } = new();

        /// <summary>
        ///     �������� ����.
        /// </summary>
        [HasMany]
        public List<Param> Params { get; set; } = new();

        /// <summary>
        ///     �������������� ���������.
        /// </summary>
        [HasOne]
        public Actuator? Actuator { get; set; }

        [HasMany]
        public List<TagEvent> TagEvents { get; set; } = new();

        [HasOne]
        public Project Project { get; set; } = null!;
    }
}
