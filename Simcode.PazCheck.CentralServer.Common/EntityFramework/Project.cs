#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.Properties;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Проект
    /// </summary>
    /// <remarks>
    ///     Совокупность версионируемых объектов технологической установки. 
    ///     Содержит матрицы ПСС, тэги, модели устройств, Объект мониторинга.
    /// </remarks>
    [Resource]
    public class Project : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>    
        ///     Название проекта        
        /// </summary>
        [PcDisplayName(ResourceStrings.Title)]
        [Attr] 
        public string Title { get; set; } = @"";

        /// <summary> 
        ///     Описание, комментарии проекта        
        /// </summary>
        [PcDisplayName(ResourceStrings.Desc)]
        [Attr] 
        public string Desc { get; set; } = @"";        

        /// <summary>
        ///     Версии данного проекта
        /// </summary>
        [HasMany]
        [InverseProperty(nameof(ProjectVersion.Project))] // Because ActiveProjectVersion property exists.
        public List<ProjectVersion> ProjectVersions { get; set; } = new();

        /// <summary>
        ///     Родительская установка
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; } = null!;

        /// <summary>
        ///     Родительская установка
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        ///     Матрицы ПСС данного проекта
        /// </summary>
        [HasMany] 
        public List<CeMatrix> CeMatrices { get; set; } = new();

        /// <summary>
        ///     Тэги данного проекта
        /// </summary>
        [HasMany] 
        public List<Tag> Tags { get; set; } = new();

        /// <summary>
        ///     Модели устройств данного проекта
        /// </summary>
        [HasMany]
        public List<BaseActuator> BaseActuators { get; set; } = new();

        /// <summary>
        ///     Объект мониторинга данного проекта
        /// </summary>
        [HasMany]
        public List<SafetyController> SafetyControllers { get; set; } = new();

        /// <summary> 
        ///     Guid данных проекта. Если поле не изменилось, то данные проекта гарантированно не менялись.        
        ///     Поле пока не используется и не обновляется.
        /// </summary>        
        [Attr]
        public string DataGuid { get; set; } = @"";

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
        ///     Удалена ли сущность
        /// </summary>
        [Attr]
        public bool _IsDeleted { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }    
}
