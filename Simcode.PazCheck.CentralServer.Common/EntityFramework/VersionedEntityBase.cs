using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class VersionedEntityBase : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>
        ///     Версия проекта, в которой сущность была создана
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении изменений в проекте.
        /// </remarks>
        [Attr]
        public UInt32? _CreateProjectVersionNum { get; set; }

        /// <summary>
        ///     Версия проекта, в которой сущность была удалена
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении изменений в проекте.
        /// </remarks>
        [Attr]
        public UInt32? _DeleteProjectVersionNum { get; set; }

        /// <summary>
        ///     Есть ли несохраненные изменения в сущности или дочерних сущностях
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public bool _HasUnversionedChanges { get; set; }

        /// <summary>
        ///     Время создания сущности
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public DateTime _CreateTimeUtc { get; set; }

        /// <summary>
        ///     Пользователь, создавший сущность
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        [Attr]
        public string _CreateUser { get; set; } = @"";

        /// <summary>
        ///     Время последнего изменения
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public DateTime _LastChangeTimeUtc { get; set; }

        /// <summary>
        ///     Последний пользователь, изменивший сущность
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        [Attr]
        public string _LastChangeUser { get; set; } = @"";

        /// <summary>
        ///     Время последнего изменения. Только для сохранененных в версию изменений.
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public DateTime _LastSavedChangeTimeUtc { get; set; }

        /// <summary>
        ///     Последний пользователь, изменивший сущность. Только для сохранененных в версию изменений.
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        [Attr]
        public string _LastSavedChangeUser { get; set; } = @"";

        ///// <summary>
        /////     Obsolete
        ///// </summary> 
        //[Attr]
        //[NotMapped]
        //public DateTime? _DeleteTimeUtc { get; set; }

        ///// <summary>
        /////     Obsolete
        ///// </summary> 
        //[Attr]
        //[NotMapped]
        //public string _DeleteUser { get; set; } = @"";

        /// <summary>
        ///     Удалена ли сущность
        /// </summary>        
        [Attr]
        public bool _IsDeleted { get; set; }

        /// <summary>
        ///     Родительская сущность для автоматического обновления свойств:
        ///     _HasUnversionedChanges, _LastChangeUser, _LastChangeTimeUtc
        ///     Возвращает значение не гарантированно
        /// </summary>
        /// <returns></returns>
        public abstract ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity();        
        public abstract bool HasParentProjectVersionedEntity();
        public abstract Type GetParentProjectVersionedEntity_PropertyType();
        public abstract int GetParentProjectVersionedEntity_Id();

        public override string ToString()
        {
            return @"";
        }
    }   
    
}
