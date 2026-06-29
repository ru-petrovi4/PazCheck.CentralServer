 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Событие результатов анализа
    ///     <para><see cref="PazCheckConstants.ResultEventType_Cause" /> - </para>
    /// </summary>
    [Resource]
    public class ResultEvent : Identifiable<int>
    {
        /// <summary>
        ///     TagName из матрицы ПСС
        /// </summary>
        [Attr]        
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     0 - Причина, 1 - Следствие, 
        ///     2 - Дополнительная причина, 3 - Дополнительное следствие,         
        ///     5 - Выход логики для ячейки,
        ///     6 - Выход логики для столбца (итоговый выход логики)
        /// </summary>
        /// <remarks>        
        ///     <see cref="PazCheckConstants"/>
        /// </remarks>   
        [Attr]
        public int Type { get; set; } = 0;

        /// <summary>
        ///     Категория события
        /// </summary>
        /// <remarks>
        ///     Категория события: команда на закрытие, команда на открытие, факт открытия, факт закрытия.   
        ///     Возможно, с '!' в начале.
        ///     <see cref="PazCheckConstants"/>
        /// </remarks>
        [Attr]
        public string ConditionCategory { get; set; } = @"";

        public string EventKind => PazCheckDbHelper.GetEventKind(Type, ConditionCategory, NewValue);

        /// <summary>
        ///     Строка события для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag.ConditionType|TripValue|Desc.        
        /// </remarks>
        [Attr]
        public string AeCondition { get; set; } = @"";

        /// <summary>
        ///     Строка события для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.        
        /// </remarks>
        [Attr]
        public string DaCondition { get; set; } = @"";

        [Attr]
        public TriggeredType TriggeredType { get; set; }

        /// <summary>
        ///     Время события
        /// </summary>   
        [Attr]
        public DateTime TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     Ссылка на событие в журнале
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(TriggeredUnitEventId))]
        public UnitEvent? TriggeredUnitEvent { get; set; }

        /// <summary>
        ///     Ссылка на событие в журнале
        /// </summary>
        public int? TriggeredUnitEventId { get; set; }

        /// <summary>
        ///     Новое значение.
        /// </summary>
        [Attr]
        public bool NewValue { get; set; }

        /// <summary>
        ///     Для причины, следствия, которые вызваны этой причиной        
        ///     <para>From <see cref="PazCheckConstants.ResultEventType_Cause"/> to <see cref="PazCheckConstants.ResultEventType_Effect"/></para>
        /// </summary>
        [HasMany]
        public List<ResultEvent> EffectResultEvents { get; set; } = new();

        /// <summary>
        ///     Для следствия, причины, которые вызвали данное следствие        
        ///     <para>From <see cref="PazCheckConstants.ResultEventType_Cause"/> to <see cref="PazCheckConstants.ResultEventType_Effect"/></para>
        /// </summary>
        [HasMany]
        public List<ResultEvent> CauseResultEvents { get; set; } = new();

        /// <summary>
        ///     Поля события. 
        ///     (Url encoded name-values collection)
        /// </summary>
        public string Params { get; set; } = @"";

        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> ParamsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(Params);
            }
            set
            {
                Params = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        [HasOne]
        [ForeignKey(nameof(ResultId))]
        public Result Result { get; set; } = null!;

        public int ResultId { get; set; }

        /// <summary>
        ///     Строки результатов анализа, связянные с данным событием
        /// </summary>
        [HasMany]
        public List<RowResult> RowResults { get; set; } = new();

        /// <summary>
        ///     Столбцы результатов анализа, связянные с данным событием
        /// </summary>
        [HasMany]
        public List<ColumnResult> ColumnResults { get; set; } = new();

        /// <summary>
        ///     Ячейки результатов анализа, связянные с данным событием
        /// </summary>
        [HasMany]
        public List<CellResult> CellResults { get; set; } = new();
    }
}


//     <para>From <see cref="PazCheckConstants.ResultEventType_ColumnLogicOutput"/> to <see cref="PazCheckConstants.ResultEventType_Effect"/></para>
//     <para>From <see cref="PazCheckConstants.ConditionCategory_Command"/> to <see cref="PazCheckConstants.ResultEventType_Effect"/></para>