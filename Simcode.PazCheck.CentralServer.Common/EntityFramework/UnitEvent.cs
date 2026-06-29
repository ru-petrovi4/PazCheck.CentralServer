using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Технологическое событие установки
    /// </summary>
    [Resource]
    public class UnitEvent : Identifiable<int>
    {
        /// <summary>
        ///     Время события
        /// </summary>
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///     Имя тэга
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     Важные поля события для отображения пользователю
        /// </summary> 
        [Attr]
        public string ConditionString { get; set; } = @"";

        /// <summary>
        ///     Состояне стало активным, неактивным или не изменилось.
        /// </summary>
        [Attr]
        public bool? ConditionIsActive { get; set; }

        /// <summary>
        ///     Приоритет события
        /// </summary>
        /// <remarks>
        ///     0 - Journal (J); 1 - Low (L); 2 - High (H); 3 - Urgent (U).
        /// </remarks>
        [Attr]
        public int Priority { get; set; }
        
        /// <summary>
        ///     Описание события для пользователя
        /// </summary>
        [Attr]        
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Исходное событие со всеми исходными полями
        /// </summary>
        /// <remarks>
        ///     Url encoded name-values collection.
        /// </remarks>
        public string OriginalEvent { get; set; } = @"";

        /// <summary>
        ///     Исходное событие со всеми исходными полями
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> OriginalEventDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(OriginalEvent);
            }
            set
            {
                OriginalEvent = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Родительский интервал
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(UnitEventsIntervalId))]
        public UnitEventsInterval UnitEventsInterval { get; set; } = null!;

        /// <summary>
        ///     Родительский интервал
        /// </summary>
        public int UnitEventsIntervalId { get; set; }

        /// <summary>
        ///     Строки матриц ПСС результатов анализа, которые ссылаются на данное событие в журнале
        /// </summary>
        [HasMany]
        public List<RowResult> RowResults { get; set; } = new();

        /// <summary>
        ///     Столбцы матриц ПСС результатов анализа, которые ссылаются на данное событие в журнале
        /// </summary>
        [HasMany]
        public List<ColumnResult> ColumnResults { get; set; } = new();

        /// <summary>
        ///     События результатов анализа, которые ссылаются на данное событие в журнале
        /// </summary>
        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();
    }
}
