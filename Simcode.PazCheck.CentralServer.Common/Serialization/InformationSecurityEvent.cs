using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class InformationSecurityEvent
    {
        /// <summary>
        ///     Время сообщения
        /// </summary>        
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///    Класс события
        /// </summary>        
        public int EventId { get; set; }
        
        public string EventIdDesc { get; set; } = @"";

        /// <summary>
        ///    Важность события
        /// </summary>
        /// <remarks>
        ///    Unknown: 0, Low: 1-3, Medium: 4-6, High: 7-8, Very-High: 9-10
        /// </remarks>        
        public int Severity { get; set; }
        
        public string SeverityDesc { get; set; } = @"";

        /// <summary>
        ///     Пользователь совершивший действие
        /// </summary>       
        
        public string User { get; set; } = @"";

        /// <summary>
        ///     IP Адрес, с которого был выполнен запрос
        /// </summary>        
        public string SourceIpAddress { get; set; } = @"";

        /// <summary>
        ///     Имя хоста, с которого был выполнен запрос
        /// </summary>        
        public string SourceHost { get; set; } = @"";

        /// <summary>
        ///     Наименование события
        /// </summary>        
        public string EventName { get; set; } = @"";

        /// <summary>
        ///     Субъект события
        /// </summary>        
        public string EventSubject { get; set; } = @"";

        /// <summary>
        ///     Объект события
        /// </summary>        
        public string EventObject { get; set; } = @"";

        /// <summary>
        ///     Поля с данными        
        /// </summary>
        /// <remarks>
        ///     Дополнительные параметры.        
        /// </remarks>                        
        public List<Param>? EventAdditionalFields { get; set; }

        /// <summary>
        ///     Текстовое сообщение события
        /// </summary>
        /// <remarks>
        ///     Генерируется на основе атрибутов и параметров события.
        /// </remarks>        
        public string EventDesc { get; set; } = @"";

        /// <summary>
        ///     Статус
        /// </summary>        
        public bool Succeeded { get; set; }
    }
}
