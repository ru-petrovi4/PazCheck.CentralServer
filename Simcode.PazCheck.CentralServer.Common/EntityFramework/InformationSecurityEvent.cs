using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Событие информационной безопасности
    /// </summary>
    [Resource]
    public class InformationSecurityEvent : Identifiable<int>
    {
        /// <summary>
        ///     Время события
        /// </summary>
        [Attr]
        public DateTime EventTimeUtc { get; set; }       

        /// <summary>
        ///    Класс события
        /// </summary>
        [Attr]
        public int EventId { get; set; }

        [Attr]
        public string EventIdDesc { get; set; } = @"";

        /// <summary>
        ///    Важность события
        /// </summary>
        /// <remarks>
        ///    Unknown: 0, Low: 1-3, Medium: 4-6, High: 7-8, Very-High: 9-10
        /// </remarks>
        [Attr]
        public int Severity { get; set; }

        [Attr]
        public string SeverityDesc { get; set; } = @"";

        /// <summary>
        ///     Пользователь совершивший действие
        /// </summary>        
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>
        ///     IP Адрес, с которого был выполнен запрос
        /// </summary>
        [Attr]
        public string SourceIpAddress { get; set; } = @"";

        /// <summary>
        ///     Имя хоста, с которого был выполнен запрос
        /// </summary>
        [Attr]
        public string SourceHost { get; set; } = @"";               

        /// <summary>
        ///     Наименование события
        /// </summary>
        [Attr]
        public string EventName { get; set; } = @"";

        /// <summary>
        ///     Субъект события
        /// </summary>
        [Attr]
        public string EventSubject { get; set; } = @"";

        /// <summary>
        ///     Объект события
        /// </summary>
        [Attr]
        public string EventObject { get; set; } = @"";

        /// <summary>
        ///     Поля с данными        
        /// </summary>
        /// <remarks>
        ///     Дополнительные параметры.
        ///     (Url encoded name-values collection)        
        /// </remarks>           
        public string EventAdditionalFields { get; set; } = @"";

        /// <summary>
        ///     Свойства объекта
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> EventAdditionalFieldsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(EventAdditionalFields);
            }
            set
            {
                EventAdditionalFields = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Текстовое сообщение события
        /// </summary>
        /// <remarks>
        ///     Генерируется на основе атрибутов и параметров события.
        /// </remarks>
        [Attr]
        public string EventDesc { get; set; } = @"";

        /// <summary>
        ///     Статус
        /// </summary>
        [Attr]
        public bool Succeeded { get; set; }
    }
}
