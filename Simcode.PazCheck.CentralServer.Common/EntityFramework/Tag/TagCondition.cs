using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Состояние тэга
    /// </summary>
    [Resource]
    public class TagCondition : VersionEntityBase
    {
        /// <summary>
        ///     Идентификатор состояния (ALARM, PVHighHigh и т.д.)
        ///     <para>Текстовое поле RW (выбор из списка (таблица TagConditionInfo) либо свое значение): Идентификатор состояния</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Значение состояния
        ///     <para>Текстовое поле RW: Значение состояния</para>
        ///     <example>пусто</example><example>ЗАКР</example>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        ///     <para>Текстовое поле RW: Краткое имя</para>
        ///     <para>Tooltip: Краткое имя состояния для отображения в матрицах ПСС</para>
        ///     <example>Alarm</example>  
        /// </summary>
        [Attr]
        public string SymbolToDisplay { get; set; } = @"";

        /// <summary>
        ///     Может быть причиной в матрицах ПСС
        ///     <para>Чекбокс RW: Может быть причиной</para>
        ///     <para>Tooltip: Может быть причиной в матрицах ПСС</para>
        /// </summary>
        [Attr]
        public bool CanBeCause { get; set; } = true;

        /// <summary>
        ///     Может быть следствием в матрицах ПСС
        ///     <para>Чекбокс RW: Может быть следствием</para>
        ///     <para>Tooltip: Может быть следствием в матрицах ПСС</para>
        /// </summary>
        [Attr]
        public bool CanBeEffect { get; set; } = true;

        /// <summary>
        ///     Ссылка на описание состояния
        /// </summary>
        [HasOne]
        public TagConditionInfo? TagConditionInfo { get; set; }
        
        /// <summary>
        ///     Родительский тэг
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }
}