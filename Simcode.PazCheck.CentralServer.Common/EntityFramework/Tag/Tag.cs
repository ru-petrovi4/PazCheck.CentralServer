#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    [Index(nameof(TagName))]
    public class Tag : VersionEntityBase
    {
        /// <summary>        
        ///     <para>Текстовое поле RW: Имя тэга</para>
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>        
        ///     <para>Текстовое поле RW: Тип</para>
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        /// <summary>        
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

        [HasOne]
        public Project Project { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Project;
    }
}
