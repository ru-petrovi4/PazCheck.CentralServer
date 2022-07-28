#nullable enable
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
    [Index(nameof(TagName))]
    public class Tag : VersionEntityBase
    {
        /// <summary>  
        ///     Имя тэга. Поле (!)версионируется.
        ///     <para>Текстовое поле RW: Имя тэга</para>
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>     
        ///     Описание тэга. Поле не версионируется.
        ///     <para>Текстовое поле RW: Описание</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        [Attr]
        public string _LockedByUser { get; set; } = @"";

        /// <summary>        
        ///     Состояния тэга.
        /// </summary>
        [HasMany]
        public List<TagCondition> TagConditions { get; set; } = new();

        /// <summary>
        ///     Свойства тэга.
        /// </summary>
        [HasMany]
        public List<TagParam> TagParams { get; set; } = new();

        /// <summary>
        ///     Модель исполнительного механимзма.
        ///     Всегда ровно одна (может указывать на 'пустой' механизм)
        /// </summary>        
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        /// <summary>
        ///     Может перекрывать или добавлять свойства модели исполнительного механизма
        /// </summary>
        [HasMany]
        public List<ActuatorParam> ActuatorParams { get; set; } = new();

        [HasMany]
        public List<TagEvent> TagEvents { get; set; } = new();

        /// <summary>
        ///     Родительский проект
        /// </summary>
        [HasOne]
        public Project Project { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Project;
    }
}
