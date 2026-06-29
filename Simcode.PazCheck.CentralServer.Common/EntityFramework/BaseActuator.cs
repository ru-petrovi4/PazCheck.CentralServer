using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Модель (модификация) устройства    
    /// </summary>    
    [Resource]    
    public class BaseActuator : ProjectVersionedEntityBase
    {
        /// <summary>
        ///     Код модели устройства в каталоге производителя
        /// </summary>
        [Attr]
        public override string Identifier { get; set; } = @"";        

        /// <summary>
        ///     Свойства модели механизма
        /// </summary>
        [HasMany]
        public List<BaseActuatorParam> BaseActuatorParams { get; set; } = new();

        /// <summary>
        ///     Ссылки на на приложенные файлы
        /// </summary>
        [HasMany]
        public List<BaseActuatorDbFileReference> BaseActuatorDbFileReferences { get; set; } = new();

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => null;
        public override bool HasParentProjectVersionedEntity() => false;
        public override Type GetParentProjectVersionedEntity_PropertyType() => throw new InvalidOperationException();
        public override int GetParentProjectVersionedEntity_Id() => throw new InvalidOperationException();
    }
}
