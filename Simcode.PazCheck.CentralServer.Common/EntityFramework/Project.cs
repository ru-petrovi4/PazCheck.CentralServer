#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;


namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Project : Identifiable<int>
    {        
        /// <summary>        
        ///     <para>Текстовое поле RW: Название</para>
        /// </summary>
        [Attr] 
        public string Title { get; set; } = @"";

        /// <summary>        
        ///     <para>Текстовое поле RW: Описание</para>
        /// </summary>
        [Attr] 
        public string Desc { get; set; } = @"";

        /// <summary>        
        ///     <para>Текстовое поле RW: Комментарий</para>
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        [Attr]
        public bool _IsDeleted { get; set; }

        [HasMany]
        [InverseProperty(nameof(ProjectVersion.Project))] // Because ActiveProjectVersion property exists.
        public List<ProjectVersion> ProjectVersions { get; set; } = new();

        [HasOne]
        public ProjectVersion? ActiveProjectVersion { get; set; } = null!;

        [HasOne]        
        public Unit Unit { get; set; } = null!;

        [HasMany] 
        public List<CeMatrix> CeMatrices { get; set; } = new();

        [HasMany] 
        public List<Tag> Tags { get; set; } = new();                
    }
}
