using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Интервал технологических событий установки
    /// </summary>
    [Resource]
    public class UnitEventsInterval : Identifiable<int>
    {
        /// <summary>
        ///     Время загрузки интервала в систему ПАЗ-Чек
        /// </summary>
        [Attr]
        public DateTime LoadTimeUtc { get; set; }

        /// <summary>
        ///     Источник интервала (кем загружен либо авто-загрузка)
        /// </summary>
        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     Комментарий к интервалу
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        /// <summary>
        ///     Начальное время интервала
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     Конечное время интервала
        /// </summary>
        [Attr]
        public DateTime EndTimeUtc { get; set; }        

        /// <summary>
        ///     Технологические события интервала
        /// </summary>
        [HasMany]
        public List<UnitEvent> UnitEvents { get; set; } = new();

        /// <summary>
        ///     Родительская установка
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; } = null!;

        /// <summary>
        ///     Родительская установка
        /// </summary>
        public int UnitId { get; set; }
    }
}
