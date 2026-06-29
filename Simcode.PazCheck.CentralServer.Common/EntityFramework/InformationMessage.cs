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
    ///     Информационное сообщение пользователю
    /// </summary>
    [Resource]
    public class InformationMessage : Identifiable<int>
    {
        /// <summary>
        ///     Кому адресовано
        /// </summary>
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>
        ///     Тип сообщения
        /// </summary>
        /// <remarks>
        ///     2 - OK; 3 - OK with Warning; 4 - NOT OK.
        /// </remarks>
        [Attr]
        public int LogLevel { get; set; }

        /// <summary>
        ///     Время создания сообщения
        /// </summary>
        [Attr]
        public DateTime TimeUtc { get; set; }

        /// <summary>
        ///     Текст сообщения
        /// </summary>
        [Attr]
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Детали сообщения
        /// </summary>
        [Attr]
        public string Details { get; set; } = @"";

        /// <summary>
        ///     True, если прочитано
        /// </summary>
        [Attr]
        public bool Acknowledged { get; set; }

        /// <summary>
        ///     Связанное сообщение-запрос
        /// </summary>
        [HasOne]
        public RequestMessage? RelatedRequestMessage { get; set; }
    }
}
