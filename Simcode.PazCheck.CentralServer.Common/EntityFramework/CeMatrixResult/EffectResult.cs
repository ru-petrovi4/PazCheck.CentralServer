using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Следствие в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class EffectResult : Identifiable<int>
    {
        /// <summary>
        ///     Порядковый номер внутри матрицы
        /// </summary>
        [Attr]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     Колонка с отладочной информацией (не для пользователя)
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        /// <summary>
        ///     Имя тега
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]
        /// </summary>
        [Attr]
        public string ConditionString { get; set; } = @"";

        /// <summary>
        ///     Краткий символ для отображения пользователю
        /// </summary>
        [Attr]
        public string ConditionString_SymbolToDisplay { get; set; } = @"";

        /// <summary>
        ///     Заголовок кастомного (пользовательского) столбца
        /// </summary>
        [Attr]
        public string CustomFieldHeader { get; set; } = @"";  

        /// <summary>
        ///     Тип страбатывания следствия
        /// </summary>
        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        /// <summary>
        ///     Время страбатывания следствия
        /// </summary>
        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Максимальное допустимое время срабатывания механизма тэга этого следствия
        /// </summary>
        [Attr]
        public UInt64 MaxDelayMs { get; set; } = 0;

        /// <summary>
        ///    Родительский результат анализа матрицы ПСС
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
