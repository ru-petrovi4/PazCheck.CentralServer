using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class UserEvent
    {
        /// <summary>
        ///     Время сообщения
        /// </summary>        
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///     Тип сообщения
        /// </summary>
        /// <remarks>
        ///     2 - Information; 3 - Warning; 4 - Error.
        /// </remarks>        
        public int LogLevel { get; set; }

        /// <summary>
        ///     Текст сообщения
        /// </summary>        
        public string Message { get; set; } = @"";        

        /// <summary>
        ///     Поля сообщения
        /// </summary>
        public List<Param>? DetailsParams { get; set; }
    }
}
