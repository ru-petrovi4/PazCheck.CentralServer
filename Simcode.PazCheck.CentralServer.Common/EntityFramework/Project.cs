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
    public class Project : Identifiable<int>, ILastChangeEntity
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

        [HasMany]
        [InverseProperty(nameof(ProjectVersion.Project))] // Because ActiveProjectVersion property exists.
        public List<ProjectVersion> ProjectVersions { get; set; } = new();

        [HasOne]
        public ProjectVersion? ActiveProjectVersion { get; set; } = null!;

        [HasOne]
        public ProjectVersion? LastProjectVersion { get; set; } = null!;

        [HasOne]        
        public Unit Unit { get; set; } = null!;

        [HasMany] 
        public List<CeMatrix> CeMatrices { get; set; } = new();

        [HasMany] 
        public List<Tag> Tags { get; set; } = new();

        [Attr]
        public bool _IsDeleted { get; set; }

        [Attr]
        public string _LastChangeUser { get; set; } = @"";

        [Attr]
        public DateTime _LastChangeTimeUtc { get; set; }

        /// <summary>
        ///     Parent for _LastChangeTimeUtc updating.
        /// </summary>
        /// <returns></returns>
        public ILastChangeEntity? GetParentForLastChange() => null;
    }
}
