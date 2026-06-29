using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;

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
        ///     TODO: fix typo
        /// </summary>
        [Attr]
        public DateTime AlalysisTimeUtc { get; set; }

        /// <summary>
        ///     Источник результата
        /// </summary>
        /// <remarks>
        ///     (пользовтаель или автоматический анализ).
        /// </remarks>
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

        /// <summary>
        ///     Ссылки на результаты анализа матриц ПСС
        /// </summary>
        [HasMany]
        public List<CeMatrixResult> CeMatrixResults { get; set; } = new();

        /// <summary>
        ///     Проект, для которого выполнен анализ
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; } = null!;

        /// <summary>
        ///     Проект, для которого выполнен анализ
        /// </summary>        
        public int? ProjectId { get; set; }

        /// <summary>
        ///     Версия проекта, для которой выполнен анализ
        /// </summary>
        [Attr]
        public UInt32? ProjectVersionNum { get; set; }

        /// <summary>
        ///     Родетельская установка
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; } = null!;

        /// <summary>
        ///     Родетельская установка
        /// </summary>        
        public int UnitId { get; set; }

        /// <summary>
        ///     События результата
        /// </summary>
        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();

        /// <summary>
        ///     Статистика по всем матрицам 
        /// </summary>
        /// <remarks>
        ///     Url encoded name-values collection
        /// </remarks>
        public string Statistics { get; set; } = @"";

        /// <summary>
        ///     Статистика по всем матрицам 
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> StatisticsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(Statistics);
            }
            set
            {
                Statistics = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }
    }
}
