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
    ///     Сообщение-запрос пользователю
    /// </summary>
    [Resource]
    public class RequestMessage : Identifiable<int>
    {
        /// <summary>
        ///     Время создания сообщения
        /// </summary>
        [Attr]
        public DateTime TimeUtc { get; set; }

        /// <summary>
        ///     Тип запроса        
        /// </summary>
        /// <remarks>
        ///     1 - изменение активной версии проекта, RequestData: Id запрашиваемой версии проекта.
        /// </remarks>
        [Attr]
        public int RequestType { get; set; }

        /// <summary>
        ///     Данные запроса
        /// </summary>
        /// <remarks>
        ///     Если RequestType = 1 - Id версии проекта, котроая запрошена, что бы стать активной.
        /// </remarks>
        [Attr]
        public string RequestData { get; set; } = null!;

        /// <summary>
        ///     Пользователь, создавший запрос
        /// </summary>
        [Attr]
        public string RequestUser { get; set; } = @"";

        /// <summary>
        ///     Текст сообщения
        /// </summary>
        [Attr]
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Пользователь, подтвердивший запрос
        /// </summary>
        [Attr]
        public string ReplyUser { get; set; } = @"";

        /// <summary>
        ///     Сообщение от подтвердившего запрос
        /// </summary>
        [Attr]
        public string ReplyMessage { get; set; } = @"";

        /// <summary>
        ///     Статус ответа на запрос        
        /// </summary>
        /// <remarks>
        ///     0 - unknown;
        ///     1 - approved;
        ///     2 - rejected;        
        /// </remarks>
        [Attr]
        public int Status { get; set; }
    }
}
