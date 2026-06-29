using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class RowOrColumnResultBase : Identifiable<int>
    {
        /// <summary>
        ///     Строка, определяющая порядок строк или столбцов внутри матрицы        
        /// </summary>
        /// <remarks>
        ///     HEX представление числа, с четным количеством знаков. Примеры: '01', 'a2', 'a201'
        /// </remarks>
        [Attr]
        public string Order { get; set; } = @"";

        /// <summary>
        ///     Колонка с отладочной информацией (не для пользователя)
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        /// <summary>
        ///     Заголовок вспомогательной строки или столбца либо имя тэга.
        /// </summary>
        [Attr]
        public string Header { get; set; } = @"";        

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю
        /// </summary>
        [Attr]
        public string? TagCondition_SymbolToDisplay { get; set; } = @"";        

        /// <summary>
        ///    Родительский результат анализа матрицы ПСС
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;
    }

    /// <summary>
    ///     Строка в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class RowResult : RowOrColumnResultBase
    {
        /// <summary>
        ///     Время срабатывания подпричины
        /// </summary>
        /// <remarks>
        ///     Получается из журнала событий.
        /// </remarks>
        [Attr]
        public DateTime? InputTriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Ссылка на событие в журнале
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(InputTriggeredUnitEventId))]
        public UnitEvent? InputTriggeredUnitEvent { get; set; } = null!;

        /// <summary>
        ///     Ссылка на событие в журнале
        /// </summary>
        public int? InputTriggeredUnitEventId { get; set; }

        /// <summary>
        ///     Время срабатывания причины
        /// </summary>
        /// <remarks>
        ///     Получается из расчета логики, без учета логики пересечений.
        /// </remarks>
        [Attr]
        public DateTime? OutputTriggeredTimeUtc { get; set; }

        /// <summary>
        ///     События строки
        ///     <para>Only ResultEventInfo.NewValue is True</para>
        /// </summary>
        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();

        [HasMany]
        public List<CellResult> CellResults { get; set; } = new();
    }

    /// <summary>
    ///     Столбец в результате анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class ColumnResult : RowOrColumnResultBase
    {
        /// <summary>
        ///     Тип страбатывания следствия
        /// </summary>
        [Attr]
        public TriggeredType TriggeredType { get; set; } = 0;

        /// <summary>
        ///     Время, когда исполнительный механизм сработал
        /// </summary>
        /// <remarks>
        ///     Получается из журнала событий.
        /// </remarks>
        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Ссылка на событие в журнале о срабатывании исполнительного механизма
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(TriggeredUnitEventId))]
        public UnitEvent? TriggeredUnitEvent { get; set; } = null!;

        /// <summary>
        ///     Ссылка на событие в журнале о срабатывании исполнительного механизма
        /// </summary>
        public int? TriggeredUnitEventId { get; set; }        

        /// <summary>
        ///     Максимальное допустимое время срабатывания механизма тэга этого следствия
        ///     Obsolete
        /// </summary>
        [Attr]
        public double MaxActuationTimeSeconds { get; set; }

        /// <summary>
        ///     События столбца
        ///     <para>Only ResultEventInfo.NewValue is True</para>
        /// </summary>
        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();

        [HasMany]
        public List<CellResult> CellResults { get; set; } = new();
    }    
}