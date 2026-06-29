using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Тэг
    /// </summary>    
    [Resource]    
    public class Tag : ProjectVersionedEntityBase
    {   
        /// <summary>
        ///     Имя тэга (TagName)
        /// </summary>        
        [Attr]
        public override string Identifier { get; set; } = @"";                

        /// <summary>
        ///     Свойства тэга
        /// </summary>
        [HasMany]
        public List<TagParam> TagParams { get; set; } = new();        

        /// <summary>        
        ///     Состояния тэга
        /// </summary>
        [HasMany]
        public List<TagCondition> TagConditions { get; set; } = new();

        /// <summary>
        ///     Ссылки на приложенные файлы
        /// </summary>
        [HasMany]
        public List<TagDbFileReference> TagDbFileReferences { get; set; } = new();

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => null;
        public override bool HasParentProjectVersionedEntity() => false;
        public override Type GetParentProjectVersionedEntity_PropertyType() => throw new InvalidOperationException();
        public override int GetParentProjectVersionedEntity_Id() => throw new InvalidOperationException();
    }
}
