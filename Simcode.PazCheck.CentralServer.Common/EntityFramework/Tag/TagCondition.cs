using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Состояние тэга.
    /// </summary>
    [Resource]
    public class TagCondition : VersionEntity
    {
        /// <summary>
        ///     <para>Текстовое поле RW (выбор из списка (таблица TagConditionIdentifier) либо свое значение): Идентификатор состояния</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Reserved
        /// </summary>
        [Attr]
        public string MathOperator { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Значение состояния</para>
        ///     <example>пусто</example><example>ЗАКР</example>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";        

        /// <summary>
        ///     <para>Текстовое поле RW: Краткое имя</para>
        ///     <para>Tooltip: Краткое имя состояния для отображения в матрицах ПСС</para>
        ///     <example>Alarm</example>  
        /// </summary>
        [Attr]
        public string SymbolToDisplay { get; set; } = @"";        

        [HasOne]
        public Tag Tag { get; set; } = null!;
    }
}
