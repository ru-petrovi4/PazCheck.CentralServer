using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Результат анализа
    /// </summary>
    [Resource]
    public class Result : Identifiable<int>
    {
        /// <summary>
        ///     Время получения результата анализа
        /// </summary>
        [Attr]
        public DateTime AlalyzeTimeUtc { get; set; }

        /// <summary>
        ///     Источник результата (пользовтаель или автоматический анализ)
        /// </summary>
        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     Комментарий к результату
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        /// <summary>
        ///     Начало проанализированного временного интервала
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     Конец проанализированного временного интервала
        /// </summary>
        [Attr]
        public DateTime EndTimeUtc { get; set; }

        [HasMany]
        public List<CeMatrixResult> CeMatrixResults { get; set; } = new();

        [HasMany]
        public List<DbFile> DbFiles { get; set; } = new();

        /// <summary>
        ///     Родетельская установка
        /// </summary>
        [HasOne]
        public Unit Unit { get; set; } = null!;

        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();
    }
}
