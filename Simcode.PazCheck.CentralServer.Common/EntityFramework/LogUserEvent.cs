using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Сообщение пользователю о статусе операции
    /// </summary>
    [Resource]
    public class LogUserEvent : Identifiable<int>
    {
        /// <summary>
        ///     Время сообщения
        /// </summary>
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///      Источник сообщения 
        /// </summary>
        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     Тип сообщения. 2 - Information; 3 - Warning; 4 - Error;
        /// </summary>
        [Attr]
        public int LogLevel { get; set; }

        /// <summary>
        ///     Сообщение пользователю
        /// </summary>
        [Attr]
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Детали сообщения
        /// </summary>
        [Attr]
        public string Details { get; set; } = @"";
    }
}
