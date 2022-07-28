using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Подпричина в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class SubCauseResult : Identifiable<int>
    {
        /// <summary>
        ///     Порядковый номер внутри родительской причины
        /// </summary>
        [Attr]
        public int Num { get; set; } = 0;

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
        ///     Заголовок кастомной (пользовательской) строчки
        /// </summary>
        [Attr]
        public string CustomFieldHeader { get; set; } = @"";

        /// <summary>
        ///     Время срабатывания подпричины
        /// </summary>
        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Родительсая причина в результате анализа матрицы ПСС
        /// </summary>
        [Attr]
        public CauseResult CauseResult { get; set; } = null!;        
    }
}
