using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Описание сущности 
    /// </summary>
    /// <remarks>
    ///     Например, описание состояния тэга.
    /// </remarks>
    [Resource]    
    public class Legend : ProjectVersionedEntityBase
    {
        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        /// </summary>        
        [Attr]
        public override string Identifier { get; set; } = @"";

        /// <summary>
        ///     Свойства дегенды
        /// </summary>
        [HasMany]
        public List<LegendParam> LegendParams { get; set; } = new();

        /// <summary>
        ///     Ссылки на на приложенные файлы
        /// </summary>
        [HasMany]
        public List<LegendDbFileReference> LegendDbFileReferences { get; set; } = new();

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => null;
        public override bool HasParentProjectVersionedEntity() => false;
        public override Type GetParentProjectVersionedEntity_PropertyType() => throw new InvalidOperationException();
        public override int GetParentProjectVersionedEntity_Id() => throw new InvalidOperationException();
    }
}
