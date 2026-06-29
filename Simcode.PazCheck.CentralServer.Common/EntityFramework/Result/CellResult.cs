using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///    Ячейка в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class CellResult : Identifiable<int>
    {
        /// <summary>
        ///     Ссылка на строку
        /// </summary>
        [HasOne]
        public RowResult RowResult { get; set; } = null!;

        /// <summary>
        ///     Ссылка на столбец
        /// </summary>
        [HasOne]
        public ColumnResult ColumnResult { get; set; } = null!;

        /// <summary>
        ///     Содержимое ячейки
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Время срабатывания пересечения
        /// </summary>
        /// <remarks>
        ///     Получается из расчета логики ячейки.
        /// </remarks>
        [Attr]
        public DateTime? OutputTriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Тип срабатывания пересечения
        /// </summary>
        [Attr]
        public TriggeredType TriggeredType { get; set; } = 0;

        /// <summary>
        ///    Родительский результат анализа матрицы ПСС
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;

        /// <summary>
        ///     События ячейки
        ///     <para>Only ResultEventInfo.NewValue is True</para>
        /// </summary>
        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();
    }
}
