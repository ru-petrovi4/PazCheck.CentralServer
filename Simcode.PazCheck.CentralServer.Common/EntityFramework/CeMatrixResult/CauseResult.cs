using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Причина в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class CauseResult : Identifiable<int>
    {
        /// <summary>
        ///     Порядковый номер внутри матрицы
        /// </summary>
        [Attr]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     Строчка с отладочной информацией (не для пользователя)
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        [HasMany]
        public List<SubCauseResult> SubCauseResults { get; set; } = new();

        /// <summary>
        ///     Время срабатывания причины
        /// </summary>
        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Родительский результат анализа матрицы ПСС
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
