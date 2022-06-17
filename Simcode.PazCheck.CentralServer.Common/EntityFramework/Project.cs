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
        ///     Уникальный ID проекта
        /// </summary>
        [Attr]
        public string Guid { get; set; } = @"";

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

        /// <summary>        
        ///     <para>Текстовое поле R: Дата последнего изменения</para>
        /// </summary>
        [Attr]
        public DateTime LastChanged { get; set; } = DateTime.UtcNow;

        [HasOne]        
        public Unit Unit { get; set; } = null!;

        [HasMany] 
        public List<CeMatrix> CeMatrices { get; set; } = new();

        [HasMany] 
        public List<Tag> Tags { get; set; } = new();                
    }
}
