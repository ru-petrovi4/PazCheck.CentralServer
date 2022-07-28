using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///    Пересечение в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class IntersectionResult : Identifiable<int>
    {
        /// <summary>
        ///     Причина пересечения
        /// </summary>
        [HasOne]
        public CauseResult CauseResult { get; set; } = null!;

        /// <summary>
        ///     Порядковый номер подпричины внутри родительской причины, если значение не относится ко всей причине
        /// </summary>
        [Attr]
        public int? SubCauseNum { get; set; }

        /// <summary>
        ///     Следствие пересечения
        /// </summary>
        [HasOne]
        public EffectResult EffectResult { get; set; } = null!;

        /// <summary>
        ///     TagConditionString_SymbolToDisplay или Custom Field Value
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Тип срабатывания пересечения
        /// </summary>
        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        /// <summary>
        ///     Время срабатывания пересечения
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;
    }
}
