using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class ResultEvent
    {
        /// <summary>
        ///     TagName из матрицы ПСС
        /// </summary>        
        public string CeMatrix_TagName { get; set; } = @"";

        /// <summary>
        ///     Время события
        /// </summary>        
        public DateTime TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     0 - Причина, 1 - Следствие, 
        ///     2 - Дополнительная причина, 3 - Дополнительное следствие, 
        ///     4 - Выход логики для строки (перед логикой ячеек), 
        ///     5 - Выход логики для ячейки,
        ///     6 - Выход логики для столбца (итоговый выход логики)
        /// </summary>
        /// <remarks>        
        ///     <see cref="PazCheckConstants"/>
        /// </remarks>           
        public string Type { get; set; } = @"";

        /// <summary>
        ///     Категория события
        /// </summary>
        /// <remarks>
        ///     Категория события: команда на закрытие, команда на открытие, факт открытия, факт закрытия.   
        ///     Без '!' в начале.
        ///     <see cref="PazCheckConstants"/>
        /// </remarks>        
        public string ConditionCategory { get; set; } = @"";

        /// <summary>
        ///     Строка события для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag.ConditionType|TripValue|Desc.        
        /// </remarks>        
        public string AeCondition { get; set; } = @"";

        /// <summary>
        ///     Строка события для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.        
        /// </remarks>        
        public string DaCondition { get; set; } = @"";

        public string TriggeredType { get; set; } = @"";

        public bool NewValue { get; set; }

        /// <summary>
        ///     Поля события.         
        /// </summary>
        public List<Param>? Params { get; set; }        

        /// <summary>
        ///     Исходное событие со всеми исходными полями
        /// </summary>
        public List<Param>? Log { get; set; }
    }
}
