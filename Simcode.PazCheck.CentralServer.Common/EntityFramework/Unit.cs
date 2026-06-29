#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Технологическая установка
    /// </summary>
    /// <remarks>
    ///     Домен предприятия (enterprise domain). Часть предприятия, считающаяся 
    ///     достаточной для определенного набора бизнес-задач [ИСО 19439:2008].
    ///     Используется в версионируемом справочнике элементов ПАЗ и модуле «Диагност». 
    ///     Необходима для разбиения предметной области на независимые части, с которыми можно работать по отдельности.
    ///     Identifier должен соответсвовать корневому объекту в модуле мониторинг PcObject.Identifier.
    /// </remarks>
    [Resource]    
    public class Unit : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>
        ///     Идентификатор установки
        ///     <para>Не может содержать точки!</para>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        public string IdentifierLower { get; set; } = @"";

        /// <summary>
        ///     Название установки        
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>
        ///     Описание, комментарии установки        
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Проекты данной установки
        /// </summary>
        [HasMany]
        [InverseProperty(nameof(Project.Unit))] // Because ActiveProject property exists.
        public List<Project> Projects { get; set; } = new();

        /// <summary>
        ///     Активная версия проекта
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(ActiveProjectVersionId))]
        public ProjectVersion? ActiveProjectVersion { get; set; } = null!;

        /// <summary>
        ///     Активная версия проекта
        /// </summary>
        public int? ActiveProjectVersionId { get; set; }

        /// <summary>
        ///     Интервалы технологических событий установки
        /// </summary>
        [HasMany]
        public List<UnitEventsInterval> UnitEventsIntervals { get; set; } = new();

        /// <summary>
        ///     Результаты анализа технологических событий установк
        /// </summary>
        [HasMany]
        public List<Result> Results { get; set; } = new();

        /// <summary>
        ///     Объекты модуля "Мониторинг" данной установки
        /// </summary>
        [HasMany]        
        public List<PcObject> PcObjects { get; set; } = new();

        /// <summary>
        ///     Шаблоны объектов модуля "Мониторинг" данной установки
        /// </summary>
        [HasMany]        
        public List<BasePcObject> BasePcObjects { get; set; } = new();

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

        public override string ToString()
        {
            return Title;
        }
    }
}
